using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Ana menüden oyun sahnesine akıcı, hızlı ve 1 defaya mahsus hikaye yazısıyla geçişi sağlar.
/// Oyuncu ilk defa girdiğinde önce kullanıcı adını sorar, ardından hikaye ekranını getirip oyunu başlatır.
/// </summary>
public class SceneTransition : MonoBehaviour
{
    [Header("Canvas Grupları")]
    [Tooltip("Tüm ekranı karartan siyah fade paneli")]
    public CanvasGroup globalFadeGroup;

    [Tooltip("Hikaye alıntı metin paneli")]
    public CanvasGroup quoteTextGroup;

    [Tooltip("Tarih ve devam etmek için tuşa bas uyarısı paneli")]
    public CanvasGroup dateandpressTextGroup;

    [Header("Oyuncu Adı Giriş Paneli (İsteğe Bağlı)")]
    public GameObject nicknamePanel;
    public TMP_InputField nicknameInputField;
    public UnityEngine.UI.Button nicknameSubmitButton;

    [Header("Metin Bileşenleri (İsteğe Bağlı)")]
    public TMP_Text quoteText;
    public TMP_Text dateText;

    [Header("Referanslar")]
    [Tooltip("Geçiş başladığında kapatılacak ana menü UI nesnesi")]
    public GameObject mainMenuUI;

    [Header("Zamanlama Ayarları")]
    [Tooltip("Kararma ve belirme efektlerinin süresi (saniye)")]
    public float fadeDuration = 0.5f;

    [Header("Geliştirici / Test Ayarları")]
    [Tooltip("Unity Editor'de test ederken hikaye yazısının HER SEFERİNDE gösterilmesi için bunu işaretleyin.")]
    public bool editorTestHerZamanGoster = false;

    private const string INTRO_SEEN_KEY = "Dink_IntroSeen";

    [ContextMenu("Intro Hafızasını Sıfırla (Reset PlayerPrefs)")]
    public void ResetIntroPlayerPrefs()
    {
        PlayerPrefs.DeleteKey(INTRO_SEEN_KEY);
        PlayerPrefs.Save();
        TutorialProgress.Reset();

        if (LeaderboardManager.instance != null)
        {
            LeaderboardManager.instance.ResetAllLeaderboardData();
        }

        Debug.Log("<color=green>[DINK] Intro & Hikaye & İsim hafızası sıfırlandı!</color>");
    }

    /// <param name="sahneNo">Açılacak oda numarası ("Game N" sahnesi).</param>
    public void ButonlaSahneyeGit(int sahneNo)
    {
        StartCoroutine(GecisRoutine(sahneNo));
    }

    private IEnumerator GecisRoutine(int sahneNo)
    {
        // 1. ZAMANI ANINDA DONDUR (Oyuncu isim girerken hiçbir şey hareket etmesin)
        Time.timeScale = 0f;

        // 2. İsim artık apartmanın giriş katında, kapıcı masasındaki defterde soruluyor (LobbyLogbook)

        // 2. Arkadaki 3D kapı etkileşimlerini ve seslerini anında durdur
        MainMenu3DController controller = FindFirstObjectByType<MainMenu3DController>();
        if (controller != null)
        {
            controller.HideMenu();
            controller.enabled = false;
        }

        // Arka plan menü müziğini yumuşakça sustur
        if (AudioManager.instance != null)
        {
            AudioManager.instance.StopMusicWithFade(1.2f);
        }

        // 3. Ekranı karart
        yield return StartCoroutine(FadeCanvasGroup(globalFadeGroup, 1f, fadeDuration));

        if (mainMenuUI != null)
        {
            mainMenuUI.SetActive(false);
        }

        // Zamanı dondur (Arka planda hiçbir şey hareket etmesin/çalmasın)
        Time.timeScale = 0f;

        // 4. Hikaye yazısı gösterimi
        bool hasSeenIntro = PlayerPrefs.GetInt(INTRO_SEEN_KEY, 0) == 1;

        if (editorTestHerZamanGoster || !hasSeenIntro)
        {
            yield return StartCoroutine(OynatHikayeYazisi());
        }

        // 5. Kronometreyi başlat, zamanı normale al ve sahneyi yükle
        if (LeaderboardManager.instance != null)
        {
            LeaderboardManager.instance.StartTimer();
        }

        Time.timeScale = 1f;
        // v0.5: yeni oyun Karanfil Apartmanı prologuyla açılır (sahneNo eski oda akışından kaldı)
        DinkScenes.LoadPrologue();
    }

    private GameObject FindNicknamePanelInScene()
    {
        if (nicknamePanel != null) return nicknamePanel;

        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var c in canvases)
        {
            if (c.gameObject.name.Contains("İSİM GİRİŞİ") || c.gameObject.name.Contains("OYUNCU") || c.gameObject.name.Contains("Nickname"))
            {
                return c.gameObject;
            }
        }
        return GameObject.Find("[OYUNCU İSİM GİRİŞİ (CANVAS)]");
    }

    /// <summary>
    /// Oyuncudan kullanıcı adını isteyen paneli ekrana getirir ve girilene kadar bekler.
    /// İsim 1 kez girildikten sonra oyun bitene kadar tekrar sorulmaz.
    /// </summary>
    private IEnumerator OynatIsimSormaPaneli()
    {
        // Sahnedeki eski panel artık kullanılmıyor; ad, kapıcı masasındaki giriş defterine yazılır
        nicknamePanel = FindNicknamePanelInScene();
        if (nicknamePanel != null) nicknamePanel.SetActive(false);

        yield return StartCoroutine(NicknameLogbookUI.Run(name =>
        {
            if (LeaderboardManager.instance != null) LeaderboardManager.instance.SetPlayerName(name);
        }));

        // Defter siyaha kararıp bitti; aynı siyahı global fade devralsın, sonra defteri kaldır
        if (globalFadeGroup != null) globalFadeGroup.alpha = 1f;
        NicknameLogbookUI.CloseAll();
    }

    /// <summary>
    /// Hikaye yazısını ekrana getirir ve OYUNCU KENDİ TIKLAYANA / BİR TUŞA BASANA kadar bekler.
    /// </summary>
    private IEnumerator OynatHikayeYazisi()
    {
        if (quoteText != null && string.IsNullOrEmpty(quoteText.text))
        {
            quoteText.text = "\"Yüzümdeki kanı silmeden kaçtım... Evde beni bekleyen küçük kardeşimi ve arkamdaki adımları düşünürken, o dar sokakta gördüğüm şeyle olduğum yere çakıldım. Akla mantığa sığmayan o manzara karşısında... ben nereye düştüm?\"";
        }
        if (dateText != null && string.IsNullOrEmpty(dateText.text))
        {
            dateText.text = "14 Kasım 2023 | 23:42\n[ Devam Etmek İçin Tıklayın ]";
        }

        PrepareGroupForFade(quoteTextGroup);
        PrepareGroupForFade(dateandpressTextGroup);

        // Panel açılınca LocalizedText yazıyı sadece tarihle yeniler; oyuncu ne beklendiğini bilsin diye
        // devam ipucu ondan sonra eklenir. Kenar boşluğu tarihin ekranın köşesine yapışmasını önler.
        if (dateText != null)
        {
            string devam = DinkLang.T("[ Click to continue ]", "[ Devam Etmek İçin Tıklayın ]", "[ Zum Fortfahren klicken ]",
                "[ Cliquez pour continuer ]", "[ Haz clic para continuar ]", "[ Clique para continuar ]", "[ Нажмите, чтобы продолжить ]");
            if (!dateText.text.Contains(devam)) dateText.text += "\n" + devam;
            dateText.margin = new Vector4(dateText.margin.x, dateText.margin.y, 60f, 40f);
        }

        StartCoroutine(FadeCanvasGroup(quoteTextGroup, 1f, fadeDuration));
        yield return StartCoroutine(FadeCanvasGroup(dateandpressTextGroup, 1f, fadeDuration));

        yield return new WaitForSecondsRealtime(0.3f);

        while (!Input.anyKeyDown && !Input.GetMouseButtonDown(0) && !Input.GetMouseButtonDown(1))
        {
            yield return null;
        }

        StartCoroutine(FadeCanvasGroup(quoteTextGroup, 0f, fadeDuration));
        yield return StartCoroutine(FadeCanvasGroup(dateandpressTextGroup, 0f, fadeDuration));

        HideGroup(quoteTextGroup);
        HideGroup(dateandpressTextGroup);
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float targetAlpha, float duration)
    {
        if (cg == null) yield break;

        float startAlpha = cg.alpha;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
            yield return null;
        }

        cg.alpha = targetAlpha;
    }

    private void PrepareGroupForFade(CanvasGroup cg)
    {
        if (cg == null) return;
        cg.gameObject.SetActive(true);
        cg.alpha = 0f;
    }

    private void HideGroup(CanvasGroup cg)
    {
        if (cg == null) return;
        cg.alpha = 0f;
        cg.gameObject.SetActive(false);
    }
}