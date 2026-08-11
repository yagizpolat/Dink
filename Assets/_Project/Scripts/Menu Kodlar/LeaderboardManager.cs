using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Dink Projesi Oyuncu Adı, Kronometre ve LootLocker Bulut Destekli 7 Dilli Skor Tablosu Yöneticisi.
/// Oturumu otomatik başlatır, skorları milisaniye bazında bulut sunucusuna gönderir ve canlı skorları çeker.
/// </summary>
public class LeaderboardManager : MonoBehaviour
{
    private static LeaderboardManager _instance;
    public static LeaderboardManager instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<LeaderboardManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("[LeaderboardManager]");
                    _instance = go.AddComponent<LeaderboardManager>();
                }
            }
            return _instance;
        }
    }

    [Header("🌐 LootLocker Bulut Ayarları")]
    [Tooltip("LootLocker Dashboard'dan alacağınız Game API Key (dev_...)")]
    public string lootLockerGameKey = "";

    [Tooltip("LootLocker Dashboard'dan alacağınız Leaderboard Key (global_leaderboard)")]
    public string lootLockerLeaderboardKey = "global_leaderboard";

    [Tooltip("Bulut sunucu aktif edilsin mi?")]
    public bool useCloudLeaderboard = true;

    private const string NICKNAME_KEY = "Dink_PlayerNickname";
    private const string LEADERBOARD_KEY = "Dink_LeaderboardData";
    private const string GAME_FINISHED_KEY = "Dink_GameFinished";

    [System.Serializable]
    public class LeaderboardEntry
    {
        public string playerName;
        public float timeInSeconds;
        public string dateString;
    }

    [System.Serializable]
    public class LeaderboardData
    {
        public List<LeaderboardEntry> entries = new List<LeaderboardEntry>();
    }

    // ─── LootLocker JSON Döküm Modelleri ───
    [System.Serializable]
    private class LootLockerSessionResponse
    {
        public string session_token;
        public int player_id;
    }

    [System.Serializable]
    private class LootLockerMember
    {
        public int rank;
        public int score;
        public LootLockerPlayer player;
    }

    [System.Serializable]
    private class LootLockerPlayer
    {
        public string name;
    }

    [System.Serializable]
    private class LootLockerLeaderboardResponse
    {
        public LootLockerMember[] items;
    }

    private float _gameStartTime;
    private bool _isTimerRunning;
    private LeaderboardData _currentData;
    private string _sessionToken = "";

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            LoadLeaderboardData();
            if (useCloudLeaderboard)
            {
                StartCoroutine(LootLockerStartSessionRoutine());
            }
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    // ════════════════════════════════════════════
    // 🌐 LOOTLOCKER BULUT REST API ENTEGRASYONU
    // ════════════════════════════════════════════

    public IEnumerator LootLockerStartSessionRoutine()
    {
        if (string.IsNullOrWhiteSpace(lootLockerGameKey) || lootLockerGameKey.Contains("placeholder"))
        {
            Debug.LogWarning("<color=yellow>[DINK LootLocker] Game Key henüz girilmemiş! Lütfen [LeaderboardManager] objesindeki Loot Locker Game Key alanına yapıştırın.</color>");
            yield break;
        }

        string url = "https://api.lootlocker.io/game/v2/session/guest";
        string jsonBody = $"{{\"game_key\":\"{lootLockerGameKey.Trim()}\",\"game_version\":\"1.0.0\",\"development_mode\":true}}";

        using (UnityWebRequest req = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            req.uploadHandler = new UploadHandlerRaw(bodyRaw);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");

            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                LootLockerSessionResponse res = JsonUtility.FromJson<LootLockerSessionResponse>(req.downloadHandler.text);
                if (res != null && !string.IsNullOrEmpty(res.session_token))
                {
                    _sessionToken = res.session_token;
                    Debug.Log($"<color=green>[DINK LootLocker] Bulut Oturumu Açıldı! Token: {_sessionToken.Substring(0, Mathf.Min(12, _sessionToken.Length))}...</color>");
                }
                else
                {
                    Debug.LogWarning($"[DINK LootLocker] Yanıt alındı fakat token bulunamadı: {req.downloadHandler.text}");
                }
            }
            else
            {
                Debug.LogError($"[DINK LootLocker] Oturum Açma Hatası ({req.responseCode}): {req.error} | Yanıt: {req.downloadHandler.text}");
            }
        }
    }

    public void SubmitScoreToCloud(string playerName, float timeInSeconds)
    {
        AddScore(playerName, timeInSeconds); // Yerel yedekle

        if (!useCloudLeaderboard) return;

        StartCoroutine(LootLockerSubmitScoreSequence(playerName, timeInSeconds));
    }

    private IEnumerator LootLockerSubmitScoreSequence(string playerName, float timeInSeconds)
    {
        if (string.IsNullOrEmpty(_sessionToken))
        {
            Debug.Log("<color=cyan>[DINK LootLocker] Oturum henüz açılmamış, önce oturum açılıyor...</color>");
            yield return StartCoroutine(LootLockerStartSessionRoutine());
        }

        if (string.IsNullOrEmpty(_sessionToken))
        {
            Debug.LogError("[DINK LootLocker] Oturum açılamadığı için skor bulut sunucusuna gönderilemedi.");
            yield break;
        }

        yield return StartCoroutine(LootLockerSubmitScoreRoutine(playerName, timeInSeconds));
    }

    private IEnumerator LootLockerSubmitScoreRoutine(string playerName, float timeInSeconds)
    {
        // 1. İsim Güncelleme
        string nameUrl = "https://api.lootlocker.io/game/player/name";
        string nameJson = $"{{\"name\":\"{playerName.Trim()}\"}}";

        using (UnityWebRequest nameReq = new UnityWebRequest(nameUrl, "PATCH"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(nameJson);
            nameReq.uploadHandler = new UploadHandlerRaw(bodyRaw);
            nameReq.downloadHandler = new DownloadHandlerBuffer();
            nameReq.SetRequestHeader("Content-Type", "application/json");
            nameReq.SetRequestHeader("x-session-token", _sessionToken);
            yield return nameReq.SendWebRequest();
        }

        // 2. Skor Gönderimi (Milisaniye Cinsinden Integer)
        int scoreInMS = (int)(timeInSeconds * 1000f);
        string scoreUrl = $"https://api.lootlocker.io/game/leaderboards/{lootLockerLeaderboardKey.Trim()}/submit";
        string scoreJson = $"{{\"score\":{scoreInMS}}}";

        using (UnityWebRequest scoreReq = new UnityWebRequest(scoreUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(scoreJson);
            scoreReq.uploadHandler = new UploadHandlerRaw(bodyRaw);
            scoreReq.downloadHandler = new DownloadHandlerBuffer();
            scoreReq.SetRequestHeader("Content-Type", "application/json");
            scoreReq.SetRequestHeader("x-session-token", _sessionToken);

            yield return scoreReq.SendWebRequest();

            if (scoreReq.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"<color=green>[DINK LootLocker] BÜYÜK BAŞARI! Skor Bulut Sunucusuna İletildi: {playerName} - {FormatTime(timeInSeconds)} ({scoreInMS}ms)</color>");
            }
            else
            {
                Debug.LogError($"[DINK LootLocker] Skor Gönderme Hatası ({scoreReq.responseCode}): {scoreReq.error} | Yanıt: {scoreReq.downloadHandler.text}");
            }
        }
    }

    public IEnumerator FetchCloudScoresRoutine(Action<List<LeaderboardEntry>> onComplete)
    {
        if (string.IsNullOrEmpty(_sessionToken))
        {
            yield return StartCoroutine(LootLockerStartSessionRoutine());
        }

        if (string.IsNullOrEmpty(_sessionToken))
        {
            onComplete?.Invoke(GetTopScores());
            yield break;
        }

        string url = $"https://api.lootlocker.io/game/leaderboards/{lootLockerLeaderboardKey.Trim()}/list?count=10";

        using (UnityWebRequest req = UnityWebRequest.Get(url))
        {
            req.SetRequestHeader("x-session-token", _sessionToken);
            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                LootLockerLeaderboardResponse res = JsonUtility.FromJson<LootLockerLeaderboardResponse>(req.downloadHandler.text);
                if (res != null && res.items != null && res.items.Length > 0)
                {
                    List<LeaderboardEntry> cloudList = new List<LeaderboardEntry>();
                    foreach (var item in res.items)
                    {
                        string pName = (item.player != null && !string.IsNullOrEmpty(item.player.name)) ? item.player.name : "Player " + item.rank;
                        float tSec = item.score / 1000f;
                        cloudList.Add(new LeaderboardEntry
                        {
                            playerName = pName,
                            timeInSeconds = tSec,
                            dateString = DateTime.Now.ToString("dd.MM.yyyy")
                        });
                    }
                    Debug.Log($"<color=cyan>[DINK LootLocker] Buluttan {cloudList.Count} adet canlı skor başarıyla çekildi!</color>");
                    onComplete?.Invoke(cloudList);
                    yield break;
                }
            }
        }

        onComplete?.Invoke(GetTopScores());
    }

    // ════════════════════════════════════════════
    // 🌐 7 DİLLİ METİN SÖZLÜĞÜ (LOCALIZATION)
    // ════════════════════════════════════════════

    public string GetLoc(string key)
    {
        LanguageManager.Language lang = LanguageManager.Language.EN;
        if (LanguageManager.instance != null) lang = LanguageManager.instance.CurrentLanguage;
        else Enum.TryParse(PlayerPrefs.GetString("Dink_Language", "EN"), out lang);

        switch (key)
        {
            case "TITLE":
                switch (lang)
                {
                    case LanguageManager.Language.TR: return "🌐 KÜRESEL SKOR TABLOSU & EN İYİ SÜRELER";
                    case LanguageManager.Language.DE: return "🌐 GLOBALE BESTENLISTE & BESTZEITEN";
                    case LanguageManager.Language.FR: return "🌐 CLASSEMENT MONDIAL & MEILLEURS TEMPS";
                    case LanguageManager.Language.ES: return "🌐 TABLA GLOBAL Y MEJORES TIEMPOS";
                    case LanguageManager.Language.PT: return "🌐 CLASSIFICAÇÃO GLOBAL E MELHORES TEMPOS";
                    case LanguageManager.Language.RU: return "🌐 ГЛОБАЛЬНАЯ ТАБЛИЦА ЛИДЕРОВ";
                    default: return "🌐 GLOBAL LEADERBOARD & BEST TIMES";
                }
            case "YOUR_TIME":
                switch (lang)
                {
                    case LanguageManager.Language.TR: return "Sizin Süreniz:";
                    case LanguageManager.Language.DE: return "Ihre Zeit:";
                    case LanguageManager.Language.FR: return "Votre Temps :";
                    case LanguageManager.Language.ES: return "Tu Tiempo:";
                    case LanguageManager.Language.PT: return "Seu Tempo:";
                    case LanguageManager.Language.RU: return "Ваше Время:";
                    default: return "Your Time:";
                }
            case "HEADER":
                switch (lang)
                {
                    case LanguageManager.Language.TR: return "SIRA   OYUNCU ADI                  SÜRE         TARİH";
                    case LanguageManager.Language.DE: return "RANG   SPIELERNAME                 ZEIT         DATUM";
                    case LanguageManager.Language.FR: return "RANG   NOM DU JOUEUR               TEMPS        DATE";
                    case LanguageManager.Language.ES: return "POS    NOMBRE DE JUGADOR           TIEMPO       FECHA";
                    case LanguageManager.Language.PT: return "POS    NOME DO JOGADOR             TEMPO        DATA";
                    case LanguageManager.Language.RU: return "РАНГ   ИМЯ ИГРОКА                  ВРЕМЯ        ДАТА";
                    default: return "RANK   PLAYER NAME                 TIME         DATE";
                }
            case "EMPTY":
                switch (lang)
                {
                    case LanguageManager.Language.TR: return "Henüz kaydedilmiş skor bulunmuyor.";
                    case LanguageManager.Language.DE: return "Noch keine Einträge vorhanden.";
                    case LanguageManager.Language.FR: return "Aucun score enregistré pour le moment.";
                    case LanguageManager.Language.ES: return "Aún no hay puntuaciones registradas.";
                    case LanguageManager.Language.PT: return "Nenhuma pontuação registrada ainda.";
                    case LanguageManager.Language.RU: return "Пока нет записанных результатов.";
                    default: return "No recorded scores yet.";
                }
            case "MAIN_MENU":
                switch (lang)
                {
                    case LanguageManager.Language.TR: return "ANA MENÜ";
                    case LanguageManager.Language.DE: return "HAUPTMENÜ";
                    case LanguageManager.Language.FR: return "MENU PRINCIPAL";
                    case LanguageManager.Language.ES: return "MENÚ PRINCIPAL";
                    case LanguageManager.Language.PT: return "MENU PRINCIPAL";
                    case LanguageManager.Language.RU: return "ГЛАВНОЕ МЕНЮ";
                    default: return "MAIN MENU";
                }
            case "REPLAY":
                switch (lang)
                {
                    case LanguageManager.Language.TR: return "YENİDEN OYNA";
                    case LanguageManager.Language.DE: return "ERNEUT SPIELEN";
                    case LanguageManager.Language.FR: return "REJOUER";
                    case LanguageManager.Language.ES: return "REPETIR";
                    case LanguageManager.Language.PT: return "JOGAR NOVAMENTE";
                    case LanguageManager.Language.RU: return "ИГРАТЬ СНОВА";
                    default: return "REPLAY";
                }
            default: return "";
        }
    }

    // ════════════════════════════════════════════
    // 👤 OYUNCU ADI YÖNETİMİ
    // ════════════════════════════════════════════

    public bool HasPlayerName()
    {
        return PlayerPrefs.HasKey(NICKNAME_KEY) && !string.IsNullOrWhiteSpace(PlayerPrefs.GetString(NICKNAME_KEY));
    }

    public string GetPlayerName()
    {
        return PlayerPrefs.GetString(NICKNAME_KEY, "");
    }

    public void SetPlayerName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return;
        PlayerPrefs.SetString(NICKNAME_KEY, name.Trim());
        PlayerPrefs.Save();
        Debug.Log($"<color=green>[DINK] Oyuncu adı kaydedildi: {name}</color>");
    }

    // ════════════════════════════════════════════
    // ⏱️ KRONOMETRE VE SÜRE ÖLÇÜMÜ
    // ════════════════════════════════════════════

    public void StartTimer()
    {
        _gameStartTime = Time.time;
        _isTimerRunning = true;
    }

    public float StopTimerAndSaveScore()
    {
        float elapsedTime = 0f;
        if (_isTimerRunning)
        {
            elapsedTime = Time.time - _gameStartTime;
            _isTimerRunning = false;
        }

        string playerName = GetPlayerName();
        if (string.IsNullOrEmpty(playerName)) playerName = "Anonymous Player";

        SubmitScoreToCloud(playerName, elapsedTime);
        PlayerPrefs.SetInt(GAME_FINISHED_KEY, 1);
        PlayerPrefs.Save();

        return elapsedTime;
    }

    // ════════════════════════════════════════════
    // 🏆 SKOR TABLOSU VERİ İŞLEMLERİ
    // ════════════════════════════════════════════

    public void AddScore(string playerName, float timeInSeconds)
    {
        LoadLeaderboardData();

        LeaderboardEntry newEntry = new LeaderboardEntry
        {
            playerName = playerName,
            timeInSeconds = timeInSeconds,
            dateString = DateTime.Now.ToString("dd.MM.yyyy")
        };

        _currentData.entries.Add(newEntry);
        _currentData.entries.Sort((a, b) => a.timeInSeconds.CompareTo(b.timeInSeconds));

        if (_currentData.entries.Count > 10)
        {
            _currentData.entries.RemoveRange(10, _currentData.entries.Count - 10);
        }

        SaveLeaderboardData();
    }

    public List<LeaderboardEntry> GetTopScores()
    {
        LoadLeaderboardData();
        return _currentData.entries;
    }

    private void LoadLeaderboardData()
    {
        if (_currentData == null)
        {
            string json = PlayerPrefs.GetString(LEADERBOARD_KEY, "");
            if (!string.IsNullOrEmpty(json))
            {
                _currentData = JsonUtility.FromJson<LeaderboardData>(json);
            }
            if (_currentData == null)
            {
                _currentData = new LeaderboardData();
            }
        }
    }

    private void SaveLeaderboardData()
    {
        if (_currentData != null)
        {
            string json = JsonUtility.ToJson(_currentData);
            PlayerPrefs.SetString(LEADERBOARD_KEY, json);
            PlayerPrefs.Save();
        }
    }

    public static string FormatTime(float timeInSeconds)
    {
        TimeSpan t = TimeSpan.FromSeconds(timeInSeconds);
        return string.Format("{0:D2}:{1:D2}:{2:D2}", t.Hours, t.Minutes, t.Seconds);
    }

    // ════════════════════════════════════════════
    // 📊 7 DİLLİ KÜRESEL SKOR TABLOSU UI PANELİ
    // ════════════════════════════════════════════

    public void ShowLeaderboardUIPanel(float currentScoreTime = -1f)
    {
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        GameObject existingCanvas = GameObject.Find("[SKOR TABLOSU (CANVAS)]");
        if (existingCanvas != null) Destroy(existingCanvas);

        GameObject canvasObj = new GameObject("[SKOR TABLOSU (CANVAS)]");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 30;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        // Arka plan koyu panel
        GameObject bgPanel = new GameObject("Background");
        bgPanel.transform.SetParent(canvasObj.transform, false);
        RectTransform bgRect = bgPanel.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        Image bgImg = bgPanel.AddComponent<Image>();
        bgImg.color = new Color(0.05f, 0.06f, 0.08f, 0.96f);

        // Kutu
        GameObject boxObj = new GameObject("LeaderboardBox");
        boxObj.transform.SetParent(bgPanel.transform, false);
        RectTransform boxRect = boxObj.AddComponent<RectTransform>();
        boxRect.anchorMin = new Vector2(0.5f, 0.5f);
        boxRect.anchorMax = new Vector2(0.5f, 0.5f);
        boxRect.sizeDelta = new Vector2(780, 680);
        Image boxImg = boxObj.AddComponent<Image>();
        boxImg.color = new Color(0.1f, 0.12f, 0.15f, 0.98f);

        // Başlık (7 Dilli)
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(boxObj.transform, false);
        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.anchoredPosition = new Vector2(0, -30);
        titleRect.sizeDelta = new Vector2(0, 50);
        TextMeshProUGUI titleTMP = titleObj.AddComponent<TextMeshProUGUI>();
        titleTMP.text = GetLoc("TITLE");
        titleTMP.fontSize = 24;
        titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.alignment = TextAlignmentOptions.Center;
        titleTMP.color = new Color(0.95f, 0.8f, 0.2f);

        // Mevcut Skor Bilgisi
        if (currentScoreTime >= 0)
        {
            GameObject curScoreObj = new GameObject("CurrentScoreInfo");
            curScoreObj.transform.SetParent(boxObj.transform, false);
            RectTransform csRect = curScoreObj.AddComponent<RectTransform>();
            csRect.anchorMin = new Vector2(0f, 1f);
            csRect.anchorMax = new Vector2(1f, 1f);
            csRect.anchoredPosition = new Vector2(0, -75);
            csRect.sizeDelta = new Vector2(0, 40);
            TextMeshProUGUI csTMP = curScoreObj.AddComponent<TextMeshProUGUI>();
            csTMP.text = $"{GetLoc("YOUR_TIME")} <color=#00FF99>{FormatTime(currentScoreTime)}</color> ({GetPlayerName()})";
            csTMP.fontSize = 20;
            csTMP.alignment = TextAlignmentOptions.Center;
            csTMP.color = Color.white;
        }

        // Liste Alanı
        GameObject listObj = new GameObject("ScoreList");
        listObj.transform.SetParent(boxObj.transform, false);
        RectTransform listRect = listObj.AddComponent<RectTransform>();
        listRect.anchorMin = new Vector2(0.5f, 0.5f);
        listRect.anchorMax = new Vector2(0.5f, 0.5f);
        listRect.anchoredPosition = new Vector2(0, -20);
        listRect.sizeDelta = new Vector2(680, 420);
        TextMeshProUGUI listTMP = listObj.AddComponent<TextMeshProUGUI>();
        listTMP.fontSize = 19;
        listTMP.alignment = TextAlignmentOptions.TopLeft;

        // Canlı Bulut Skorlarını Çek ve Göster
        StartCoroutine(FetchCloudScoresRoutine((entries) =>
        {
            string tableText = $"<color=#888888>{GetLoc("HEADER")}</color>\n";
            tableText += "-----------------------------------------------------\n";

            if (entries == null || entries.Count == 0)
            {
                tableText += $"\n           {GetLoc("EMPTY")}";
            }
            else
            {
                for (int i = 0; i < entries.Count; i++)
                {
                    var entry = entries[i];
                    string rankStr = (i + 1).ToString().PadRight(6);
                    string nameStr = entry.playerName.PadRight(25);
                    if (nameStr.Length > 25) nameStr = nameStr.Substring(0, 22) + "...";
                    string timeStr = FormatTime(entry.timeInSeconds).PadRight(12);

                    string colorTag = (i == 0) ? "<color=#FFD700>" : ((i == 1) ? "<color=#C0C0C0>" : ((i == 2) ? "<color=#CD7F32>" : "<color=#FFFFFF>"));
                    tableText += $"{colorTag}{rankStr}{nameStr}{timeStr}{entry.dateString}</color>\n";
                }
            }
            if (listTMP != null) listTMP.text = tableText;
        }));

        // Butonlar
        GameObject btnBox = new GameObject("Buttons");
        btnBox.transform.SetParent(boxObj.transform, false);
        RectTransform btnBoxRect = btnBox.AddComponent<RectTransform>();
        btnBoxRect.anchorMin = new Vector2(0f, 0f);
        btnBoxRect.anchorMax = new Vector2(1f, 0f);
        btnBoxRect.anchoredPosition = new Vector2(0, 30);
        btnBoxRect.sizeDelta = new Vector2(0, 50);

        // Ana Menü Butonu
        GameObject mainBtnObj = new GameObject("MainMenuButton");
        mainBtnObj.transform.SetParent(btnBox.transform, false);
        RectTransform mbRect = mainBtnObj.AddComponent<RectTransform>();
        mbRect.anchorMin = new Vector2(0.2f, 0.5f);
        mbRect.anchorMax = new Vector2(0.45f, 0.5f);
        mbRect.sizeDelta = new Vector2(0, 45);
        Image mbImg = mainBtnObj.AddComponent<Image>();
        mbImg.color = new Color(0.25f, 0.28f, 0.35f);
        Button mbBtn = mainBtnObj.AddComponent<Button>();
        mbBtn.onClick.AddListener(() =>
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(0);
        });

        GameObject mbTextObj = new GameObject("Text");
        mbTextObj.transform.SetParent(mainBtnObj.transform, false);
        RectTransform mbtRect = mbTextObj.AddComponent<RectTransform>();
        mbtRect.anchorMin = Vector2.zero;
        mbtRect.anchorMax = Vector2.one;
        mbtRect.sizeDelta = Vector2.zero;
        TextMeshProUGUI mbTMP = mbTextObj.AddComponent<TextMeshProUGUI>();
        mbTMP.text = GetLoc("MAIN_MENU");
        mbTMP.fontSize = 18;
        mbTMP.fontStyle = FontStyles.Bold;
        mbTMP.alignment = TextAlignmentOptions.Center;
        mbTMP.color = Color.white;

        // Yeniden Oyna Butonu
        GameObject replayBtnObj = new GameObject("ReplayButton");
        replayBtnObj.transform.SetParent(btnBox.transform, false);
        RectTransform rbRect = replayBtnObj.AddComponent<RectTransform>();
        rbRect.anchorMin = new Vector2(0.55f, 0.5f);
        rbRect.anchorMax = new Vector2(0.8f, 0.5f);
        rbRect.sizeDelta = new Vector2(0, 45);
        Image rbImg = replayBtnObj.AddComponent<Image>();
        rbImg.color = new Color(0.85f, 0.2f, 0.2f);
        Button rbBtn = replayBtnObj.AddComponent<Button>();
        rbBtn.onClick.AddListener(() =>
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(1);
        });

        GameObject rbTextObj = new GameObject("Text");
        rbTextObj.transform.SetParent(replayBtnObj.transform, false);
        RectTransform rbtRect = rbTextObj.AddComponent<RectTransform>();
        rbtRect.anchorMin = Vector2.zero;
        rbtRect.anchorMax = Vector2.one;
        rbtRect.sizeDelta = Vector2.zero;
        TextMeshProUGUI rbTMP = rbTextObj.AddComponent<TextMeshProUGUI>();
        rbTMP.text = GetLoc("REPLAY");
        rbTMP.fontSize = 18;
        rbTMP.fontStyle = FontStyles.Bold;
        rbTMP.alignment = TextAlignmentOptions.Center;
        rbTMP.color = Color.white;
    }

    // ════════════════════════════════════════════
    // 🔄 UNITY EDITOR İLE TEK TIKLA SIFIRLAMA
    // ════════════════════════════════════════════

    [ContextMenu("Skor Tablosunu ve İsimleri Sıfırla (Reset All Scores & Name)")]
    public void ResetAllLeaderboardData()
    {
        PlayerPrefs.DeleteKey(NICKNAME_KEY);
        PlayerPrefs.DeleteKey(LEADERBOARD_KEY);
        PlayerPrefs.DeleteKey(GAME_FINISHED_KEY);
        PlayerPrefs.Save();

        if (_currentData != null)
        {
            _currentData.entries.Clear();
        }

        Debug.Log("<color=yellow>[DINK] Tüm skor tablosu verileri ve oyuncu ismi sıfırlandı!</color>");
    }
}
