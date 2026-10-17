using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    [Tooltip("Kötü Döngü sahnesi yoksa oyuncunun hapsolacağı oda (sonsuz döngü)")]
    [SerializeField] private string badLoopFallbackSceneName = "Game 1";

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
            // Özel sahne henüz yok: oyuncuyu baştaki odaya geri at, döngü hissi korunur
            Debug.LogWarning($"[EndingManager] '{badLoopSceneName}' sahnesi Build Settings'te bulunamadı! '{badLoopFallbackSceneName}' odasına döngüye sokuluyor.");
            SceneManager.LoadScene(badLoopFallbackSceneName);
        }
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
                SceneManager.LoadScene(0);
            }
        }
    }
}
