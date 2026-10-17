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
            Debug.LogWarning($"[EndingManager] '{badLoopSceneName}' sahnesi Build Settings'te bulunamadı!");
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
            Debug.LogWarning($"[EndingManager] '{goodEndingSceneName}' sahnesi Build Settings'te bulunamadı!");
        }
    }
}
