using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Zindan sahnesinde (Zindan.unity) oyuncunun 20 saniyelik geri sayım içinde
/// 3 paslı kilit pimi doğru anda kilitleyerek odaya geri dönmesini sağlayan minigame.
/// </summary>
public class ZindanKilitMinigame : MonoBehaviour
{
    [Header("Geri Sayım (Countdown) Ayarları")]
    [Tooltip("Zindandan kurtulmak için verilen toplam süre (saniye).")]
    [SerializeField] private float toplamSure = 20f;
    [Tooltip("Yanlış tıklamada düşülecek ceza süresi (saniye).")]
    [SerializeField] private float cezaSuresi = 3f;

    [Header("Pim Ayarları")]
    [Tooltip("Pimlerin hareket hız katsayısı.")]
    [SerializeField] private float pimHizi = 2.5f;

    [Header("UI Elemanları")]
    [SerializeField] private TextMeshProUGUI sayaçMetni;
    [SerializeField] private TextMeshProUGUI durumMetni;
    [SerializeField] private TextMeshProUGUI kalanHakMetni;
    [SerializeField] private Slider[] pimSliderlari = new Slider[3];
    [SerializeField] private Image[] hedefAlanGorselleri = new Image[3];
    [SerializeField] private Image[] pimDolguGorselleri = new Image[3];

    [Header("Ses Efektleri (Opsiyonel)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip kilitlemeSesi;
    [SerializeField] private AudioClip hataSesi;
    [SerializeField] private AudioClip basariSesi;
    [SerializeField] private AudioClip basarisizlikSesi;

    // Hedef Alan Sınırları (0.0 - 1.0 Slider aralığında)
    private float hedefMin = 0.4f;
    private float hedefMax = 0.65f;

    private float kalanSure;
    private int aktifPimIndex = 0;
    private bool[] pimKilitlimi = new bool[3];
    private bool oyunBitti = false;
    private float[] pimYonleri = new float[] { 1f, -1f, 1f };

    private void OnEnable()
    {
        kalanSure = toplamSure;
        aktifPimIndex = 0;
        oyunBitti = false;

        for (int i = 0; i < 3; i++)
        {
            pimKilitlimi[i] = false;
            if (pimSliderlari[i] != null)
            {
                pimSliderlari[i].value = Random.Range(0f, 1f);
            }
        }

        // Kalan hak bilgisini güncelle
        if (kalanHakMetni != null && ZindanKurtulmaManager.instance != null)
        {
            kalanHakMetni.text = $"ZİNDAN KURTULMA HAKKI: {ZindanKurtulmaManager.instance.KalanHak + 1} / 2";
        }

        GörünümüGüncelle();
    }

    private void Update()
    {
        if (oyunBitti) return;

        // 1. Geri Sayım Mantığı
        kalanSure -= Time.deltaTime;
        if (sayaçMetni != null)
        {
            sayaçMetni.text = $"KALAN SÜRE: {Mathf.Max(0f, kalanSure):F1}s";
            // Son 5 saniyede sayacı kırmızı yapıp büyütme
            if (kalanSure <= 5f)
            {
                sayaçMetni.color = Color.red;
            }
        }

        if (kalanSure <= 0f)
        {
            ZindanBasarisiz();
            return;
        }

        // 2. Aktif Pimin Hareket Etmesi
        if (aktifPimIndex < 3 && !pimKilitlimi[aktifPimIndex] && pimSliderlari[aktifPimIndex] != null)
        {
            Slider aktifSlider = pimSliderlari[aktifPimIndex];
            aktifSlider.value += pimYonleri[aktifPimIndex] * pimHizi * Time.deltaTime;

            if (aktifSlider.value >= 1f)
            {
                aktifSlider.value = 1f;
                pimYonleri[aktifPimIndex] = -1f;
            }
            else if (aktifSlider.value <= 0f)
            {
                aktifSlider.value = 0f;
                pimYonleri[aktifPimIndex] = 1f;
            }
        }

        // 3. Tıklama / Girdi Kontrolü (Fare Sol Tık veya Boşluk Tuşu)
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            PimKilitlemeyiDene();
        }
    }

    private void PimKilitlemeyiDene()
    {
        if (aktifPimIndex >= 3 || oyunBitti) return;

        Slider aktifSlider = pimSliderlari[aktifPimIndex];
        if (aktifSlider == null) return;

        float degeri = aktifSlider.value;

        // Pim Hedef alanda mı?
        if (degeri >= hedefMin && degeri <= hedefMax)
        {
            // BAŞARILI KİLİTLEME!
            pimKilitlimi[aktifPimIndex] = true;
            if (pimDolguGorselleri[aktifPimIndex] != null)
            {
                pimDolguGorselleri[aktifPimIndex].color = new Color(0.2f, 0.9f, 0.2f, 1f); // Yeşil
            }

            SesCal(kilitlemeSesi);
            aktifPimIndex++;

            if (durumMetni != null)
            {
                durumMetni.text = $"PİM {aktifPimIndex} KİLİTLENDİ!";
                durumMetni.color = Color.green;
            }

            // Tüm pimler kilitlendi mi?
            if (aktifPimIndex >= 3)
            {
                ZindanKurtulmaBasarili();
            }
        }
        else
        {
            // YANLIŞ ZAMANLAMA / HATA!
            kalanSure -= cezaSuresi;
            SesCal(hataSesi);

            if (durumMetni != null)
            {
                durumMetni.text = $"ISKALADIN! -{cezaSuresi}s CEZA!";
                durumMetni.color = Color.red;
            }

            // Hafif sarsıntı etkisi
            StartCoroutine(MetinSarsinti());
        }

        GörünümüGüncelle();
    }

    private void ZindanKurtulmaBasarili()
    {
        oyunBitti = true;
        SesCal(basariSesi);

        if (durumMetni != null)
        {
            durumMetni.text = "KİLİT KIRILDI! ZİNDANDAN KAÇILIYOR...";
            durumMetni.color = Color.cyan;
        }

        StartCoroutine(KurtulmaGecikmesi());
    }

    private IEnumerator KurtulmaGecikmesi()
    {
        yield return new WaitForSecondsRealtime(1.2f);

        if (ZindanKurtulmaManager.instance != null)
        {
            ZindanKurtulmaManager.instance.ZindandanKurtul();
        }
        else
        {
            Debug.LogWarning("[ZindanMinigame] ZindanKurtulmaManager bulunamadı, varsayılan Sahne 1'e dönülüyor.");
            SceneManager.LoadScene(1);
        }
    }

    private void ZindanBasarisiz()
    {
        oyunBitti = true;
        SesCal(basarisizlikSesi);

        if (durumMetni != null)
        {
            durumMetni.text = "SÜRE BİTTİ! ZİNDANDAN ÇIKAMADIN!";
            durumMetni.color = Color.red;
        }

        StartCoroutine(BasarisizGecikmesi());
    }

    private IEnumerator BasarisizGecikmesi()
    {
        yield return new WaitForSecondsRealtime(2f);

        if (ZindanKurtulmaManager.instance != null)
        {
            ZindanKurtulmaManager.instance.HaklariSifirla();
        }

        // Haklar bittiği için Ana Menüye yönlendir
        SceneManager.LoadScene(0);
    }

    private void GörünümüGüncelle()
    {
        for (int i = 0; i < 3; i++)
        {
            if (hedefAlanGorselleri[i] != null)
            {
                if (i == aktifPimIndex)
                {
                    hedefAlanGorselleri[i].color = new Color(1f, 0.8f, 0f, 0.8f); // Aktif pim sarı hedef
                }
                else if (pimKilitlimi[i])
                {
                    hedefAlanGorselleri[i].color = new Color(0.2f, 0.9f, 0.2f, 0.8f); // Kilitlenmiş yeşil
                }
                else
                {
                    hedefAlanGorselleri[i].color = new Color(0.4f, 0.4f, 0.4f, 0.4f); // Bekleyen gri
                }
            }
        }
    }

    private void SesCal(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            float sfxVol = AudioManager.instance != null ? AudioManager.instance.GetSFXVolume() : PlayerPrefs.GetFloat("Dink_SFXVolume", 0.8f);
            audioSource.PlayOneShot(clip, sfxVol);
        }
    }

    private IEnumerator MetinSarsinti()
    {
        if (durumMetni == null) yield break;
        Vector3 baslangicPos = durumMetni.transform.localPosition;
        float elapsed = 0f;
        while (elapsed < 0.25f)
        {
            durumMetni.transform.localPosition = baslangicPos + (Vector3)Random.insideUnitCircle * 8f;
            elapsed += Time.deltaTime;
            yield return null;
        }
        durumMetni.transform.localPosition = baslangicPos;
    }
}
