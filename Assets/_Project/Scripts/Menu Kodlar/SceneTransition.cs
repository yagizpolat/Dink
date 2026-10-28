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

    [ContextMenu("Intro Hafızasını Sıfırla (Reset PlayerPrefs)")]
    public void ResetIntroPlayerPrefs()
    {
        PlayerPrefs.DeleteKey(DinkPrefs.IntroSeen);
        PlayerPrefs.Save();

        if (LeaderboardManager.instance != null)
        {
            LeaderboardManager.instance.ResetAllLeaderboardData();
        }

        Debug.Log("<color=green>[DINK] Intro & Hikaye & İsim hafızası sıfırlandı!</color>");
    }

    /// <summary>Yeni Oyun: karartma, ilk seferde hikaye yazısı, sonra prolog.</summary>
    public void YeniOyunaGec()
    {
        StartCoroutine(GecisRoutine());
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
        yield return MenudenCik();
        LobbyLogbook.RestoreSignature(DinkSave.ResumeName());
        LeaderboardManager.instance.StartTimer(DinkSave.ElapsedSeconds);
        PauseService.ResetAll();
        SceneManager.LoadScene(sahne);
    }

    private IEnumerator GecisRoutine()
    {
        yield return MenudenCik();

        // Hikaye yazısı yalnız ilk yeni oyunda
        if (editorTestHerZamanGoster || PlayerPrefs.GetInt(DinkPrefs.IntroSeen, 0) != 1)
        {
            yield return StartCoroutine(OynatHikayeYazisi());
            PlayerPrefs.SetInt(DinkPrefs.IntroSeen, 1);
            PlayerPrefs.Save();
        }

        LeaderboardManager.instance.StartTimer();
        PauseService.ResetAll();
        // Yeni oyun Karanfil Apartmanı prologuyla açılır
        DinkScenes.LoadPrologue();
    }

    /// <summary>Zamanı durdurur, menüyü gizler, müziği susturur ve ekranı karartır.</summary>
    private IEnumerator MenudenCik()
    {
        PauseService.Pause(this);

        MainMenu3DController controller = FindFirstObjectByType<MainMenu3DController>();
        if (controller != null)
        {
            controller.HideMenu();
            controller.enabled = false;
        }

        AudioManager.instance.StopMusicWithFade(1.2f);
        yield return StartCoroutine(FadeCanvasGroup(globalFadeGroup, 1f, fadeDuration));
        if (mainMenuUI != null) mainMenuUI.SetActive(false);
    }

    /// <summary>
    /// Hikaye yazısını ekrana getirir ve OYUNCU KENDİ TIKLAYANA / BİR TUŞA BASANA kadar bekler.
    /// </summary>
    private IEnumerator OynatHikayeYazisi()
    {
        // Metinleri sahnedeki LocalizedText bileşenleri seçili dilde yazar
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