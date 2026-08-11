using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

/// <summary>
/// Dink projesi sinematik giriş sekansı: Karakter yerde yatarken gözünü açar,
/// etrafına bulanık bakar, başını kaldırır, ayağa kalkar ve görüş netleşir.
/// 
/// Hızlı zamanlama, 1 kere gösterim (PlayerPrefs) ve Tuşla Atlama (Skip) koruması içerir.
/// Geliştiricinin Unity Editor içinde rahatça test edebilmesi için Editor Test moduna sahiptir.
/// </summary>
public class IntroCinematic : MonoBehaviour
{
    [Header("Bağlantılar")]
    [Tooltip("Kamera.cs scriptinin bağlı olduğu obje")]
    public Kamera kameraScript;

    [Tooltip("Ana kamera objesi (Kamera.cs içindeki cam referansı)")]
    public Transform cameraTransform;

    [Tooltip("Fener kontrolü (giriş sırasında kapatılacak)")]
    public FenerKontrol fenerScript;

    [Tooltip("Altyazı yöneticisi (göz açıldıktan sonra ses tetiklenecek)")]
    public SubtitleManager subtitleManager;

    [Header("UI Elemanları")]
    [Tooltip("Ekranı kaplayan siyah tam ekran panel")]
    public Image blackScreen;

    [Tooltip("Göz açılışı üst siyah bant")]
    public RectTransform eyeTopBar;

    [Tooltip("Göz açılışı alt siyah bant")]
    public RectTransform eyeBottomBar;

    [Header("URP Bulanıklık (Depth of Field)")]
    [Tooltip("Sahnedeki URP Volume bileşeni (DepthOfField override içermeli)")]
    public Volume blurVolume;

    [Tooltip("Bulanıklığın başlangıç şiddeti (Gaussian Max Radius, 0-1 arası)")]
    public float bulanıklıkBaslangic = 1.0f;

    [Header("Zamanlama Ayarları (Hızlı & Akıcı)")]
    public float siyahEkranSuresi = 0.4f;
    public float gozAcilmaSuresi = 1.0f;
    public float basKaldirmaSuresi = 1.2f;
    public float bulanikliktanNetligeSuresi = 0.8f;
    public float kontrolIadesiGecikmesi = 0.1f;

    [Header("Atlama & Tekrar Ayarları")]
    [Tooltip("Sinematik oyuncunun ilk girişinde 1 defa oynasın, sonraki restartlarda atlasın.")]
    public bool sadeceIlkGiris = true;

    [Tooltip("Oyuncu SPACE, E veya ESC tuşuna bastığında sinematik anında atlasın mı?")]
    public bool tuslaAtlamayaIzinVer = true;

    [Header("Geliştirici / Test Ayarları")]
    [Tooltip("Unity Editor'de test ederken göz açılma sinematiğinin HER SEFERİNDE gösterilmesi için bunu işaretleyin.")]
    public bool editorTestHerZamanGoster = false;

    [Header("Kamera Açıları")]
    [Tooltip("Başlangıçta kameranın yere bakış açısı (X rotasyonu, pozitif = aşağı)")]
    public float baslangicAcisi = 60f;

    [Tooltip("Kameranın son durma açısı (X rotasyonu, 0 = düz ileri)")]
    public float bitisAcisi = 0f;

    // Dahili DepthOfField referansı
    private DepthOfField depthOfField;

    private const string INTRO_SEEN_KEY = "Dink_IntroSeen";

    /// <summary>
    /// Inspector üzerinde script bileşenine sağ tıklayıp Intro kayıtlı verisini sıfırlayabilirsiniz.
    /// </summary>
    [ContextMenu("Intro Hafızasını Sıfırla (Reset PlayerPrefs)")]
    public void ResetIntroPlayerPrefs()
    {
        PlayerPrefs.DeleteKey(INTRO_SEEN_KEY);
        PlayerPrefs.Save();
        Debug.Log("<color=green>[DINK] Tüm Intro & Hikaye hafızası sıfırlandı!</color>");
    }

    private void Start()
    {
        // Volume'dan DepthOfField override'ını al, yoksa otomatik ekle!
        if (blurVolume != null && blurVolume.profile != null)
        {
            if (!blurVolume.profile.TryGet(out depthOfField))
            {
                depthOfField = blurVolume.profile.Add<DepthOfField>(true);
            }
        }

        bool hasSeenIntro = PlayerPrefs.GetInt(INTRO_SEEN_KEY, 0) == 1;

        // Editor test modu kapalıysa, sadeceIlkGiris aktifse ve önceden izlendiyse anında atla!
        if (!editorTestHerZamanGoster && sadeceIlkGiris && hasSeenIntro)
        {
            InstantSkip();
            return;
        }

        StartCoroutine(SinematikGirisSekansı());
    }

    /// <summary>
    /// Sinematiği beklemeden anında atlatıp kontrolü oyuncuya bağlar.
    /// </summary>
    public void InstantSkip()
    {
        StopAllCoroutines();

        // Kamerayı düz açıya getir
        if (cameraTransform != null)
        {
            cameraTransform.rotation = Quaternion.Euler(bitisAcisi, 0f, 0f);
        }

        // UI elemanlarını kapat
        if (blackScreen != null) blackScreen.gameObject.SetActive(false);
        if (eyeTopBar != null) eyeTopBar.gameObject.SetActive(false);
        if (eyeBottomBar != null) eyeBottomBar.gameObject.SetActive(false);

        // Bulanıklığı kapat
        if (depthOfField != null)
        {
            depthOfField.gaussianMaxRadius.Override(0f);
            depthOfField.active = false;
        }

        // Kontrolleri aç
        if (kameraScript != null) kameraScript.enabled = true;
        if (fenerScript != null) fenerScript.enabled = true;

        this.enabled = false;
    }

    private bool CheckSkipInput()
    {
        if (tuslaAtlamayaIzinVer && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Escape)))
        {
            MarkAsSeenAndSkip();
            return true;
        }
        return false;
    }

    private void MarkAsSeenAndSkip()
    {
        PlayerPrefs.SetInt(INTRO_SEEN_KEY, 1);
        PlayerPrefs.Save();
        InstantSkip();
    }

    private IEnumerator SinematikGirisSekansı()
    {
        // ===== FAZ 0: HAZIRLIK =====
        if (kameraScript != null) kameraScript.enabled = false;
        if (fenerScript != null) fenerScript.enabled = false;

        if (cameraTransform != null)
        {
            cameraTransform.rotation = Quaternion.Euler(baslangicAcisi, 0f, 0f);
        }

        if (blackScreen != null)
        {
            blackScreen.gameObject.SetActive(true);
            blackScreen.color = new Color(0f, 0f, 0f, 1f);
        }

        if (eyeTopBar != null && eyeBottomBar != null)
        {
            eyeTopBar.gameObject.SetActive(true);
            eyeBottomBar.gameObject.SetActive(true);
        }

        if (depthOfField != null)
        {
            depthOfField.active = true;
            depthOfField.mode.Override(DepthOfFieldMode.Gaussian);
            depthOfField.gaussianMaxRadius.Override(bulanıklıkBaslangic);
            depthOfField.gaussianStart.Override(0.1f);
            depthOfField.gaussianEnd.Override(5f);
        }

        // Siyah ekranda bekleme
        float tSiyah = 0f;
        while (tSiyah < siyahEkranSuresi)
        {
            if (CheckSkipInput()) yield break;
            tSiyah += Time.deltaTime;
            yield return null;
        }

        if (blackScreen != null)
        {
            blackScreen.gameObject.SetActive(false);
        }

        // ===== FAZ 1: GÖZ AÇILIŞI =====
        if (subtitleManager != null)
        {
            subtitleManager.ShowIntroSubtitle();
        }

        float elapsed = 0f;
        float screenHeight = Screen.height;
        float barStartHeight = screenHeight;
        float barEndHeight = 0f;

        while (elapsed < gozAcilmaSuresi)
        {
            if (CheckSkipInput()) yield break;

            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / gozAcilmaSuresi);
            float currentHeight = Mathf.Lerp(barStartHeight, barEndHeight, t);

            if (eyeTopBar != null) eyeTopBar.sizeDelta = new Vector2(eyeTopBar.sizeDelta.x, currentHeight);
            if (eyeBottomBar != null) eyeBottomBar.sizeDelta = new Vector2(eyeBottomBar.sizeDelta.x, currentHeight);

            yield return null;
        }

        if (eyeTopBar != null) eyeTopBar.gameObject.SetActive(false);
        if (eyeBottomBar != null) eyeBottomBar.gameObject.SetActive(false);

        // ===== FAZ 2: BAŞINI KALDIRMA =====
        elapsed = 0f;
        Quaternion startRot = Quaternion.Euler(baslangicAcisi, 0f, 0f);
        Quaternion endRot = Quaternion.Euler(bitisAcisi, 0f, 0f);

        while (elapsed < basKaldirmaSuresi)
        {
            if (CheckSkipInput()) yield break;

            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / basKaldirmaSuresi);

            if (cameraTransform != null)
            {
                cameraTransform.rotation = Quaternion.Slerp(startRot, endRot, t);
            }

            yield return null;
        }

        if (cameraTransform != null)
        {
            cameraTransform.rotation = endRot;
        }

        // ===== FAZ 3: BULANIKLIKTAN NETLİĞE =====
        elapsed = 0f;

        while (elapsed < bulanikliktanNetligeSuresi)
        {
            if (CheckSkipInput()) yield break;

            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / bulanikliktanNetligeSuresi);

            if (depthOfField != null)
            {
                float currentRadius = Mathf.Lerp(bulanıklıkBaslangic, 0f, t);
                depthOfField.gaussianMaxRadius.Override(currentRadius);
            }

            yield return null;
        }

        if (depthOfField != null)
        {
            depthOfField.gaussianMaxRadius.Override(0f);
            depthOfField.active = false;
        }

        // ===== FAZ 4: KONTROL İADESİ =====
        float tKontrol = 0f;
        while (tKontrol < kontrolIadesiGecikmesi)
        {
            if (CheckSkipInput()) yield break;
            tKontrol += Time.deltaTime;
            yield return null;
        }

        if (kameraScript != null) kameraScript.enabled = true;
        if (fenerScript != null) fenerScript.enabled = true;

        // Sinematik tamamlandı, 1 kere gösterildi olarak işaretle
        PlayerPrefs.SetInt(INTRO_SEEN_KEY, 1);
        PlayerPrefs.Save();

        this.enabled = false;
    }
}
