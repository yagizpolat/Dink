using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Çift Son & Kötü Döngü Yöneticisi (Ending Manager).
/// 15. odadan sonra SpeedrunnerDetector sonuçlarına göre oyuncuyu:
/// - Hikaye %100 ise İyi Son (Kaçış Finali)
/// - Hikaye atlanmışsa Build Settings'teki Özel Kötü Döngü Sahnesine aktarır.
/// </summary>
public class EndingManager : MonoBehaviour
{
    public static EndingManager instance;

    [Header("Sahne İsimleri / Indexleri")]
    [SerializeField] private string badLoopSceneName = "BadLoopScene";
    [SerializeField] private string goodEndingSceneName = "GoodEndingScene";

    [Tooltip("Kötü Döngü sahnesi yoksa, oyuncu ilk gerçek odaya atılmadan önce 'Yine buradasın...' yazısının ekranda kalma süresi")]
    [SerializeField] private float badLoopMessageSeconds = 2.5f;

    /// <summary>
    /// Sahnede elle eklemeye gerek kalmadan oyun açılışında otomatik oluşur.
    /// Aksi halde 15. odadan sonra final kararı hiç verilmiyordu.
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void OtomatikOlustur()
    {
        if (instance != null) return;

        GameObject go = new GameObject("[EndingManager]");
        go.AddComponent<EndingManager>();
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 15. odadan sonra final kararı verir.
    /// </summary>
    public void CheckEndingAndTransition()
    {
        float elapsedTime = LeaderboardManager.instance != null ? LeaderboardManager.instance.StopTimerAndSaveScore() : 0f;

        bool isSpeedrunner = SpeedrunnerDetector.instance != null && SpeedrunnerDetector.instance.IsSpeedrunner();

        if (isSpeedrunner)
        {
            TriggerBadLoopScene();
        }
        else
        {
            if (LeaderboardManager.instance != null)
            {
                LeaderboardManager.instance.ShowLeaderboardUIPanel(elapsedTime);
            }
            TriggerGoodEnding();
        }
    }

    private void TriggerBadLoopScene()
    {
        Debug.Log("⚠️ KÖTÜ DÖNGÜ TETİKLENDİ! Speedrunner oyuncusu Özel Kötü Döngü Sahnesine aktarılıyor...");
        // Oyuncu zindana bile düşemeyecek, monotonluk tavan yapacak
        if (Application.CanStreamedLevelBeLoaded(badLoopSceneName))
        {
            SceneManager.LoadScene(badLoopSceneName);
        }
        else
        {
            // Özel sahne henüz yok: oyuncuyu tutorial olmayan ilk odaya geri at, döngü hissi korunur.
            // İstatistikler sıfırlanmaz; bu turda daha çok ipucu okursa kaçabilir.
            int loopRoom = TutorialProgress.FirstPlayableRoom;
            Debug.LogWarning($"[EndingManager] '{badLoopSceneName}' sahnesi Build Settings'te bulunamadı! Oda {loopRoom}'e döngüye sokuluyor.");
            StartCoroutine(BadLoopFallbackRoutine(loopRoom));
        }
    }

    private IEnumerator BadLoopFallbackRoutine(int loopRoom)
    {
        GameObject overlay = CreateLoopMessageOverlay();

        yield return new WaitForSecondsRealtime(badLoopMessageSeconds);

        Time.timeScale = 1f;
        DinkScenes.LoadRoom(loopRoom);

        // Yeni oda yüklenip kararmadan çıkana kadar mesajı kısa bir süre daha tut
        yield return new WaitForSecondsRealtime(0.4f);
        Destroy(overlay);
    }

    private GameObject CreateLoopMessageOverlay()
    {
        GameObject canvasObj = new GameObject("BadLoopMessageCanvas");
        canvasObj.transform.SetParent(transform, false);
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        GameObject bg = new GameObject("Black");
        bg.transform.SetParent(canvasObj.transform, false);
        RectTransform bgRect = bg.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        bg.AddComponent<Image>().color = Color.black;

        GameObject msg = new GameObject("LoopMessage");
        msg.transform.SetParent(canvasObj.transform, false);
        RectTransform msgRect = msg.AddComponent<RectTransform>();
        msgRect.anchorMin = new Vector2(0.1f, 0.4f);
        msgRect.anchorMax = new Vector2(0.9f, 0.6f);
        msgRect.offsetMin = Vector2.zero;
        msgRect.offsetMax = Vector2.zero;
        TextMeshProUGUI tmp = msg.AddComponent<TextMeshProUGUI>();
        bool isTR = LanguageManager.instance != null && LanguageManager.instance.CurrentLanguage == LanguageManager.Language.TR;
        tmp.text = isTR ? "Yine buradasın..." : "You're here again...";
        tmp.fontSize = 56;
        tmp.fontStyle = FontStyles.Italic;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(0.8f, 0.75f, 0.68f);
        tmp.raycastTarget = false;

        return canvasObj;
    }

    private void TriggerGoodEnding()
    {
        Debug.Log("🎉 İYİ SON TETİKLENDİ! Oyuncu %100 hikaye tamamlama ile kapı cehenneminden kaçmayı başardı!");
        if (Application.CanStreamedLevelBeLoaded(goodEndingSceneName))
        {
            SceneManager.LoadScene(goodEndingSceneName);
        }
        else
        {
            // Özel sahne henüz yok: skor panelindeki Ana Menü / Yeniden Oyna butonları kullanılabilsin
            Debug.LogWarning($"[EndingManager] '{goodEndingSceneName}' sahnesi Build Settings'te bulunamadı! Skor paneli gösteriliyor.");
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            if (LeaderboardManager.instance == null)
            {
                DinkScenes.LoadMainMenu();
            }
        }
    }
}
