using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

/// <summary>
/// Dink projesi sinematik giriş sekansı: Karakter yerde yatarken gözünü açar,
/// etrafına bulanık bakar, başını kaldırır, ayağa kalkar ve görüş netleşir.
/// 
/// Bulanıklık efekti URP Depth of Field (Gaussian) ile sağlanır.
/// Renk yok, sadece saf bulanıklık.
/// 
/// 5 Fazlı Coroutine Dizilimi:
/// Faz 0: Hazırlık (Kamera kontrolünü al, ekranı siyahla)
/// Faz 1: Göz Açılışı (Üst-Alt siyah bantlar açılır)
/// Faz 2: Başını Kaldırma (Kamera yerden yukarı döner)
/// Faz 3: Bulanıklıktan Netliğe (URP Depth of Field sıfırlanır)
/// Faz 4: Kontrol İadesi (Oyuncuya geri ver)
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

    [Header("Zamanlama Ayarları")]
    public float siyahEkranSuresi = 1.2f;
    public float gozAcilmaSuresi = 1.8f;
    public float basKaldirmaSuresi = 2.5f;
    public float bulanikliktanNetligeSuresi = 2.0f;
    public float kontrolIadesiGecikmesi = 0.3f;

    [Header("Kamera Açıları")]
    [Tooltip("Başlangıçta kameranın yere bakış açısı (X rotasyonu, pozitif = aşağı)")]
    public float baslangicAcisi = 60f;

    [Tooltip("Kameranın son durma açısı (X rotasyonu, 0 = düz ileri)")]
    public float bitisAcisi = 0f;

    // Dahili DepthOfField referansı
    private DepthOfField depthOfField;

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

        StartCoroutine(SinematikGirisSekansı());
    }

    private IEnumerator SinematikGirisSekansı()
    {
        // ===== FAZ 0: HAZIRLIK =====
        // Oyuncudan kamera ve fener kontrolünü al
        if (kameraScript != null) kameraScript.enabled = false;
        if (fenerScript != null) fenerScript.enabled = false;

        // Kamerayı yere bakacak şekilde döndür
        if (cameraTransform != null)
        {
            cameraTransform.rotation = Quaternion.Euler(baslangicAcisi, 0f, 0f);
        }

        // Ekranı tamamen siyah yap
        if (blackScreen != null)
        {
            blackScreen.gameObject.SetActive(true);
            blackScreen.color = new Color(0f, 0f, 0f, 1f);
        }

        // Göz bantlarını tam kapalı konuma getir
        if (eyeTopBar != null && eyeBottomBar != null)
        {
            eyeTopBar.gameObject.SetActive(true);
            eyeBottomBar.gameObject.SetActive(true);
        }

        // URP Depth of Field bulanıklığını tam şiddetle başlat
        if (depthOfField != null)
        {
            depthOfField.active = true;
            depthOfField.mode.Override(DepthOfFieldMode.Gaussian);
            depthOfField.gaussianMaxRadius.Override(bulanıklıkBaslangic);
            depthOfField.gaussianStart.Override(0.1f);
            depthOfField.gaussianEnd.Override(5f);
        }

        // Siyah ekranda kısa bekleme (karanlıkta bilinç kazanma hissi)
        yield return new WaitForSeconds(siyahEkranSuresi);

        // Siyah tam ekranı kapat, göz bantları devralacak
        if (blackScreen != null)
        {
            blackScreen.gameObject.SetActive(false);
        }

        // ===== FAZ 1: GÖZ AÇILIŞI (Eye Opening) =====
        // Göz açıldığı an ses ve altyazıyı tetikle!
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
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / gozAcilmaSuresi);

            float currentHeight = Mathf.Lerp(barStartHeight, barEndHeight, t);

            if (eyeTopBar != null)
            {
                eyeTopBar.sizeDelta = new Vector2(eyeTopBar.sizeDelta.x, currentHeight);
            }
            if (eyeBottomBar != null)
            {
                eyeBottomBar.sizeDelta = new Vector2(eyeBottomBar.sizeDelta.x, currentHeight);
            }

            yield return null;
        }



        // Bantları tamamen kapat
        if (eyeTopBar != null) eyeTopBar.gameObject.SetActive(false);
        if (eyeBottomBar != null) eyeBottomBar.gameObject.SetActive(false);

        // ===== FAZ 2: BAŞINI KALDIRMA (Head Rise) =====
        elapsed = 0f;
        Quaternion startRot = Quaternion.Euler(baslangicAcisi, 0f, 0f);
        Quaternion endRot = Quaternion.Euler(bitisAcisi, 0f, 0f);

        while (elapsed < basKaldirmaSuresi)
        {
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

        // ===== FAZ 3: BULANIKLIKTAN NETLİĞE (Blur → Clear) =====
        // URP Depth of Field gaussianMaxRadius değerini 1.0'dan 0.0'a düşür
        // Hiçbir renk yok, saf bulanıklık yavaşça temizlenir
        elapsed = 0f;

        while (elapsed < bulanikliktanNetligeSuresi)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / bulanikliktanNetligeSuresi);

            if (depthOfField != null)
            {
                float currentRadius = Mathf.Lerp(bulanıklıkBaslangic, 0f, t);
                depthOfField.gaussianMaxRadius.Override(currentRadius);
            }

            yield return null;
        }

        // Bulanıklığı tamamen kapat
        if (depthOfField != null)
        {
            depthOfField.gaussianMaxRadius.Override(0f);
            depthOfField.active = false;
        }

        // ===== FAZ 4: KONTROL İADESİ =====
        yield return new WaitForSeconds(kontrolIadesiGecikmesi);

        // Oyuncuya kamera ve fener kontrolünü geri ver
        if (kameraScript != null) kameraScript.enabled = true;
        if (fenerScript != null) fenerScript.enabled = true;

        // Sinematik bitti, kendini devre dışı bırak
        this.enabled = false;
    }
}
