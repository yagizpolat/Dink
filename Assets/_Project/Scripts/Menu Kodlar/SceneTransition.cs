using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Ana menüden prolog sahnesine akıcı, hızlı ve 1 defaya mahsus hikaye yazısıyla geçişi sağlar.
/// Oyuncu adı burada sorulmaz; giriş katındaki kapıcı defterinde (LobbyLogbook) sorulur.
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

    [Header("Eski Oyuncu Adı Paneli (kullanılmıyor, sahne referansı için duruyor)")]
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

        if (LeaderboardManager.instance != null)
        {
            LeaderboardManager.instance.ResetAllLeaderboardData();
        }

        Debug.Log("<color=green>[DINK] Intro & Hikaye & İsim hafızası sıfırlandı!</color>");
    }

    /// <param name="sahneNo">Kullanılmıyor (sahnedeki buton bağlantıları için imza korunuyor); her zaman prolog açılır.</param>
    public void ButonlaSahneyeGit(int sahneNo)
    {
        StartCoroutine(GecisRoutine(sahneNo));
    }

    /// <summary>
    /// Devam Et: kayıttaki kat sahnesini aynı karartmayla doğrudan açar; hikaye yazısı gösterilmez.
    /// Ad imza olarak geri yüklenir, kronometre kayıttaki süreden sürer.
    /// </summary>
    public void KayittanDevamEt(string sahne)
    {
        StartCoroutine(DevamRoutine(sahne));
    }

    private IEnumerator DevamRoutine(string sahne)
    {
        Time.timeScale = 0f;

        MainMenu3DController controller = FindFirstObjectByType<MainMenu3DController>();
        if (controller != null)
        {
            controller.HideMenu();
            controller.enabled = false;
        }

        if (AudioManager.instance != null)
        {
            AudioManager.instance.StopMusicWithFade(1.2f);
        }

        yield return StartCoroutine(FadeCanvasGroup(globalFadeGroup, 1f, fadeDuration));

        if (mainMenuUI != null)
        {
            mainMenuUI.SetActive(false);
        }

        string ad = DinkSave.PlayerName;
        if (string.IsNullOrWhiteSpace(ad) && LeaderboardManager.instance != null) ad = LeaderboardManager.instance.GetPlayerName();
        if (string.IsNullOrWhiteSpace(ad)) ad = "Anonymous Player";
        LobbyLogbook.RestoreSignature(ad);
        if (LeaderboardManager.instance != null)
        {
            LeaderboardManager.instance.StartTimer(DinkSave.ElapsedSeconds);
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(sahne);
    }

    private IEnumerator GecisRoutine(int sahneNo)
    {
        // 1. ZAMANI ANINDA DONDUR
        Time.timeScale = 0f;

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
            PlayerPrefs.SetInt(INTRO_SEEN_KEY, 1);
            PlayerPrefs.Save();
        }

        // 5. Kronometreyi başlat, zamanı normale al ve sahneyi yükle
        if (LeaderboardManager.instance != null)
        {
            LeaderboardManager.instance.StartTimer();
        }

        Time.timeScale = 1f;
        // Yeni oyun Karanfil Apartmanı prologuyla açılır
        DinkScenes.LoadPrologue();
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
                "[ Cliquez pour continuer ]", "[ Haz clic para continuar ]", "[ Clique para continuar ]", "[ Нажмите, чтобы продолжить ]", "[ 点击继续 ]", "[ クリックして続ける ]", "[ 클릭하여 계속 ]");
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