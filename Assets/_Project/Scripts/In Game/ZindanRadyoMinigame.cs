using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Zindan Minigame: Radyo Frekans & Osiloskop Dengeleme
/// Oyuncu 2 farklı ayar topuzu (Frekans & Genlik) ile kırmızı dalgayı yeşil hedef dalga ile tam örtüştürür.
/// </summary>
public class ZindanRadyoMinigame : MonoBehaviour
{
    [Header("Geri Sayım Ayarları")]
    [SerializeField] private float toplamSure = 25f;

    [Header("UI Elemanları")]
    [SerializeField] private TextMeshProUGUI sayaçMetni;
    [SerializeField] private TextMeshProUGUI durumMetni;
    [SerializeField] private TextMeshProUGUI frekansMetni;
    [SerializeField] private Slider frekansSlider;
    [SerializeField] private Slider genlikSlider;

    [Header("Ses Efektleri")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip parazitSesi;
    [SerializeField] private AudioClip basariSesi;

    private float kalanSure;
    private float hedefFrekans;
    private float hedefGenlik;
    private bool oyunBitti = false;

    private void OnEnable()
    {
        kalanSure = toplamSure;
        oyunBitti = false;

        // Hedef rastgele değerler (0.1 - 0.9 aralığında)
        hedefFrekans = Random.Range(0.2f, 0.8f);
        hedefGenlik = Random.Range(0.2f, 0.8f);

        if (frekansSlider != null)
        {
            frekansSlider.value = Random.Range(0.05f, 0.95f);
            frekansSlider.onValueChanged.RemoveAllListeners();
            frekansSlider.onValueChanged.AddListener((v) => KontrolEt());
        }

        if (genlikSlider != null)
        {
            genlikSlider.value = Random.Range(0.05f, 0.95f);
            genlikSlider.onValueChanged.RemoveAllListeners();
            genlikSlider.onValueChanged.AddListener((v) => KontrolEt());
        }

        KontrolEt();
    }

    private void Update()
    {
        if (oyunBitti) return;

        kalanSure -= Time.deltaTime;
        if (sayaçMetni != null)
        {
            sayaçMetni.text = $"KALAN SÜRE: {Mathf.Max(0f, kalanSure):F1}s";
            if (kalanSure <= 5f) sayaçMetni.color = Color.red;
        }

        if (kalanSure <= 0f)
        {
            Basarisiz();
        }
    }

    private void KontrolEt()
    {
        if (oyunBitti || frekansSlider == null || genlikSlider == null) return;

        float fFark = Mathf.Abs(frekansSlider.value - hedefFrekans);
        float gFark = Mathf.Abs(genlikSlider.value - hedefGenlik);

        float uyumYuzdesi = Mathf.Clamp01(1f - ((fFark + gFark) / 2f)) * 100f;

        if (frekansMetni != null)
        {
            frekansMetni.text = $"FREKANS UYUMU: %{uyumYuzdesi:F0}";
            frekansMetni.color = uyumYuzdesi >= 92f ? Color.green : Color.yellow;
        }

        if (durumMetni != null)
        {
            durumMetni.text = uyumYuzdesi >= 92f ? "FREKANS KİLİTLENDİ!" : "TOPUZLARI ÇEVİREREK DALGAYI OTURT!";
            durumMetni.color = uyumYuzdesi >= 92f ? Color.green : Color.yellow;
        }

        // %92 ve üzeri uyum sağlandığında kilit açılır
        if (uyumYuzdesi >= 92f)
        {
            Basarili();
        }
    }

    private void Basarili()
    {
        oyunBitti = true;
        SesCal(basariSesi);
        if (durumMetni != null)
        {
            durumMetni.text = "TELSİZ FREKANSI YAKALANDI! KAPIK AÇILDI!";
            durumMetni.color = Color.cyan;
        }
        StartCoroutine(GecikmeliKurtul());
    }

    private IEnumerator GecikmeliKurtul()
    {
        yield return new WaitForSecondsRealtime(1.2f);
        if (ZindanKurtulmaManager.instance != null) ZindanKurtulmaManager.instance.ZindandanKurtul();
        else DinkScenes.LoadRoom(DinkScenes.FirstRoom);
    }

    private void Basarisiz()
    {
        oyunBitti = true;
        if (durumMetni != null)
        {
            durumMetni.text = "PARAZİT KESTİ! SÜRE BİTTİ!";
            durumMetni.color = Color.red;
        }
        StartCoroutine(GecikmeliBasarisiz());
    }

    private IEnumerator GecikmeliBasarisiz()
    {
        yield return new WaitForSecondsRealtime(2f);
        if (ZindanKurtulmaManager.instance != null) ZindanKurtulmaManager.instance.HaklariSifirla();
        DinkScenes.LoadMainMenu();
    }

    private void SesCal(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            float sfxVol = AudioManager.instance != null ? AudioManager.instance.GetSFXVolume() : PlayerPrefs.GetFloat("Dink_SFXVolume", 0.8f);
            audioSource.PlayOneShot(clip, sfxVol);
        }
    }
}
