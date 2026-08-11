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

        if (LeaderboardManager.instance != null)
        {
            LeaderboardManager.instance.ResetAllLeaderboardData();
        }

        Debug.Log("<color=green>[DINK] Intro & Hikaye & İsim hafızası sıfırlandı!</color>");
    }

    public void ButonlaSahneyeGit(int sahneNo)
    {
        StartCoroutine(GecisRoutine(sahneNo));
    }

    private IEnumerator GecisRoutine(int sahneNo)
    {
        // 1. ZAMANI ANINDA DONDUR (Oyuncu isim girerken hiçbir şey hareket etmesin)
        Time.timeScale = 0f;

        // 2. Oyuncu ismi daha önce girilmemişse (veya Editor test modu aktifse) İSİM SORMA PANELİNİ aç!
        bool hasName = LeaderboardManager.instance != null && LeaderboardManager.instance.HasPlayerName();
        if (editorTestHerZamanGoster || !hasName)
        {
            yield return StartCoroutine(OynatIsimSormaPaneli());
        }

        // 2. Arkadaki 3D kapı etkileşimlerini ve seslerini anında durdur
        MainMenu3DController controller = FindFirstObjectByType<MainMenu3DController>();
        if (controller != null)
        {
            controller.enabled = false;
        }

        Menu3DDoor[] doors = FindObjectsByType<Menu3DDoor>(FindObjectsSortMode.None);
        foreach (var door in doors)
        {
            door.HoverExit();
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
        SceneManager.LoadScene(sahneNo);
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
        nicknamePanel = FindNicknamePanelInScene();

        if (nicknamePanel != null)
        {
            nicknamePanel.SetActive(true);

            if (nicknameInputField == null)
                nicknameInputField = nicknamePanel.GetComponentInChildren<TMP_InputField>();

            if (nicknameSubmitButton == null)
                nicknameSubmitButton = nicknamePanel.GetComponentInChildren<UnityEngine.UI.Button>();

            bool submitted = false;

            UnityEngine.Events.UnityAction submitAction = () =>
            {
                if (nicknameInputField != null && !string.IsNullOrWhiteSpace(nicknameInputField.text))
                {
                    LeaderboardManager.instance.SetPlayerName(nicknameInputField.text);
                    submitted = true;
                }
            };

            if (nicknameSubmitButton != null)
                nicknameSubmitButton.onClick.AddListener(submitAction);

            // Fare ile butona tıklanmasını veya Klavyeden ENTER tuşuna basılmasını bekle
            while (!submitted)
            {
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    if (nicknameInputField != null && !string.IsNullOrWhiteSpace(nicknameInputField.text))
                    {
                        LeaderboardManager.instance.SetPlayerName(nicknameInputField.text);
                        submitted = true;
                    }
                }
                yield return null;
            }

            if (nicknameSubmitButton != null)
                nicknameSubmitButton.onClick.RemoveListener(submitAction);

            nicknamePanel.SetActive(false);
        }
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