using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Zindan Minigame: Milisaniyelik İğne Yakalama (Precision Needle Gauge)
/// Oyuncu dairesel kadranda hızla dönen iğne dar kırmızı bölgeden geçerken tam anında tıklar.
/// 3 başarılı vuruş gerekir.
/// </summary>
public class ZindanIgneMinigame : MonoBehaviour
{
    [Header("Geri Sayım Ayarları")]
    [SerializeField] private float toplamSure = 20f;
    [SerializeField] private float cezaSuresi = 3f;

    [Header("UI Elemanları")]
    [SerializeField] private TextMeshProUGUI sayaçMetni;
    [SerializeField] private TextMeshProUGUI durumMetni;
    [SerializeField] private Button tiklaButonu;
    [SerializeField] private RectTransform igneTransform;

    [Header("Ses Efektleri")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip vurusSesi;
    [SerializeField] private AudioClip hataSesi;
    [SerializeField] private AudioClip basariSesi;

    private float kalanSure;
    private float igneAcisi = 0f;
    private float donmeHizi = 360f; // Saniyede 360 derece (hızlı)
    private int basariliVurusSayisi = 0;
    private bool oyunBitti = false;

    // Hedef Kırmızı Bölge Açısı (Örn: 40 ile 80 derece arası)
    private float hedefMinAci = 40f;
    private float hedefMaxAci = 80f;

    private void OnEnable()
    {
        kalanSure = toplamSure;
        basariliVurusSayisi = 0;
        igneAcisi = 0f;
        oyunBitti = false;

        if (tiklaButonu != null)
        {
            tiklaButonu.onClick.RemoveAllListeners();
            tiklaButonu.onClick.AddListener(VurusDene);
        }

        if (durumMetni != null)
        {
            durumMetni.text = "İĞNE KIRMIZI BÖLGEDEYKEN BUTONA TIKLA! (0/3)";
            durumMetni.color = Color.yellow;
        }
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
            return;
        }

        // İğneyi döndür
        igneAcisi = (igneAcisi + donmeHizi * Time.deltaTime) % 360f;
        if (igneTransform != null)
        {
            igneTransform.localRotation = Quaternion.Euler(0, 0, -igneAcisi);
        }
    }

    private void VurusDene()
    {
        if (oyunBitti) return;

        // İğne açısı hedef aralıkta mı?
        if (igneAcisi >= hedefMinAci && igneAcisi <= hedefMaxAci)
        {
            basariliVurusSayisi++;
            SesCal(vurusSesi);

            if (durumMetni != null)
            {
                durumMetni.text = $"TAM İSABET! ({basariliVurusSayisi}/3)";
                durumMetni.color = Color.green;
            }

            if (basariliVurusSayisi >= 3)
            {
                Basarili();
            }
        }
        else
        {
            // Iskalama
            kalanSure -= cezaSuresi;
            SesCal(hataSesi);

            if (durumMetni != null)
            {
                durumMetni.text = $"ISKALADIN! (-{cezaSuresi}s)";
                durumMetni.color = Color.red;
            }
        }
    }

    private void Basarili()
    {
        oyunBitti = true;
        SesCal(basariSesi);
        if (durumMetni != null)
        {
            durumMetni.text = "3 İSABET SAĞLANDI! KAPTAN KURTULUNDU!";
            durumMetni.color = Color.cyan;
        }
        StartCoroutine(GecikmeliKurtul());
    }

    private IEnumerator GecikmeliKurtul()
    {
        yield return new WaitForSecondsRealtime(1.2f);
        if (ZindanKurtulmaManager.instance != null) ZindanKurtulmaManager.instance.ZindandanKurtul();
        else DinkScenes.LoadNewGame();
    }

    private void Basarisiz()
    {
        oyunBitti = true;
        if (durumMetni != null)
        {
            durumMetni.text = "İĞNE SIKIŞTI! SÜRE BİTTİ!";
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
