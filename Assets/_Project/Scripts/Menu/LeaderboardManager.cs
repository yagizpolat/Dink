using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Dink Projesi Oyuncu Adı, Kronometre ve LootLocker Bulut Destekli Skor Tablosu Yöneticisi.
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

    private const string LEADERBOARD_KEY = DinkPrefs.LeaderboardData;
    private const string GAME_FINISHED_KEY = DinkPrefs.GameFinished;

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
        public string metadata; // gönderimde yazılan tarih (dd.MM.yyyy)
        public LootLockerPlayer player;
    }

    // İstek gövdeleri JsonUtility ile kurulur; adda " ya da \ olsa da JSON bozulmaz
    [System.Serializable]
    private class LootLockerSessionRequest
    {
        public string game_key;
        public string game_version = "1.0.0";
        public bool development_mode = true;
    }

    [System.Serializable]
    private class LootLockerNameRequest
    {
        public string name;
    }

    [System.Serializable]
    private class LootLockerScoreRequest
    {
        public int score;
        public string metadata;
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
        string jsonBody = JsonUtility.ToJson(new LootLockerSessionRequest { game_key = lootLockerGameKey.Trim() });

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

        pendingSubmit = StartCoroutine(LootLockerSubmitScoreSequence(playerName, timeInSeconds));
    }

    // Skor gönderimi sürerken tablo çekilirse oyuncunun kendi satırı listede çıkmıyordu; tablo bunu bekler
    private Coroutine pendingSubmit;

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
        string nameJson = JsonUtility.ToJson(new LootLockerNameRequest { name = playerName.Trim() });

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
        string scoreJson = JsonUtility.ToJson(new LootLockerScoreRequest { score = scoreInMS, metadata = DateTime.Now.ToString("dd.MM.yyyy", System.Globalization.CultureInfo.InvariantCulture) });

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
                            // Eski kayıtlarda tarih yok; bugünün tarihini uydurmak yerine boş bırakılır
                            dateString = string.IsNullOrEmpty(item.metadata) ? "—" : item.metadata
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
    // 👤 OYUNCU ADI YÖNETİMİ
    // ════════════════════════════════════════════

    // Ad tek yerde tutulur: kayıttaki oyuncu adı (DinkSave.PlayerName)
    public string GetPlayerName() => DinkSave.PlayerName;

    public void SetPlayerName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return;
        DinkSave.PlayerName = name.Trim();
        PlayerPrefs.Save();
        Debug.Log($"<color=green>[DINK] Oyuncu adı kaydedildi: {name}</color>");
    }

    // ════════════════════════════════════════════
    // ⏱️ KRONOMETRE VE SÜRE ÖLÇÜMÜ
    // ════════════════════════════════════════════

    public bool IsTimerRunning => _isTimerRunning;

    /// <summary>Çalışan kronometrenin o anki süresi (saniye); duruyorsa 0.</summary>
    public float ElapsedTime => _isTimerRunning ? Time.time - _gameStartTime : 0f;

    /// <param name="startOffset">Devam Et'te kayıttaki süreden sürdürmek için başlangıç değeri.</param>
    public void StartTimer(float startOffset = 0f)
    {
        _gameStartTime = Time.time - Mathf.Max(0f, startOffset);
        _isTimerRunning = true;
    }

    /// <summary>Skor yazmadan durdurur (ana menüye dönüş).</summary>
    public void StopTimer()
    {
        _isTimerRunning = false;
    }

    // Oyun kapanırken süre kayda yazılır; kapalıyken geçen zaman sayılmaz
    private void OnApplicationQuit()
    {
        if (_instance == this) DinkSave.SaveElapsedIfRunning();
    }

    public float StopTimerAndSaveScore()
    {
        if (!_isTimerRunning)
        {
            // Kronometre hiç başlamadıysa (ör. eski "Yeniden Oyna" akışı) 00:00:00 skor yazma
            Debug.LogWarning("[DINK] Kronometre çalışmıyordu; skor kaydedilmedi.");
            return 0f;
        }

        float elapsedTime = Time.time - _gameStartTime;
        _isTimerRunning = false;

        string playerName = GetPlayerName();
        if (string.IsNullOrEmpty(playerName)) playerName = "Anonymous Player";

        SubmitScoreToCloud(playerName, elapsedTime);
        PlayerPrefs.SetInt(GAME_FINISHED_KEY, 1);
        PlayerPrefs.Save();
        DinkSave.ClearProgress();

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
            dateString = DateTime.Now.ToString("dd.MM.yyyy", System.Globalization.CultureInfo.InvariantCulture)
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
    // 📊 OYUN SONU SKOR TABLOSU
    // ════════════════════════════════════════════

    public void ShowLeaderboardUIPanel(float currentScoreTime = -1f)
    {
        PauseService.Pause(this);

        // Oyun sonu skor tablosu: giriş defterinin bir sonraki sayfası (LeaderboardLogbookUI)
        LeaderboardLogbookUI.Show(currentScoreTime, GetPlayerName());
        StartCoroutine(FetchAfterSubmitRoutine());
    }

    private IEnumerator FetchAfterSubmitRoutine()
    {
        if (pendingSubmit != null) yield return pendingSubmit;
        pendingSubmit = null;

        yield return StartCoroutine(FetchCloudScoresRoutine(entries =>
        {
            LeaderboardLogbookUI ui = FindAnyObjectByType<LeaderboardLogbookUI>();
            if (ui != null) ui.SetEntries(entries);
        }));
    }

    // ════════════════════════════════════════════
    // 🔄 UNITY EDITOR İLE TEK TIKLA SIFIRLAMA
    // ════════════════════════════════════════════

    [ContextMenu("Skor Tablosunu ve İsimleri Sıfırla (Reset All Scores & Name)")]
    public void ResetAllLeaderboardData()
    {
        PlayerPrefs.DeleteKey(DinkPrefs.SavePlayerName);
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
