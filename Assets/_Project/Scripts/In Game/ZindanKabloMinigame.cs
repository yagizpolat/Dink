using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Zindan Minigame 2: Elektrik Kablosu Bağlama
/// Soldaki 4 farklı renkteki kabloyu sağdaki eşleşen renkteki soketlere bağlama minigame'i.
/// </summary>
public class ZindanKabloMinigame : MonoBehaviour
{
    [Header("Geri Sayım Ayarları")]
    [SerializeField] private float toplamSure = 20f;
    [SerializeField] private float cezaSuresi = 4f;

    [Header("UI Elemanları")]
    [SerializeField] private TextMeshProUGUI sayaçMetni;
    [SerializeField] private TextMeshProUGUI durumMetni;
    [SerializeField] private Button[] solKabloButonlari = new Button[4];
    [SerializeField] private Button[] sagSoketButonlari = new Button[4];
    [SerializeField] private Image[] solKabloGorselleri = new Image[4];
    [SerializeField] private Image[] sagSoketGorselleri = new Image[4];

    [Header("Ses Efektleri")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip baglantiSesi;
    [SerializeField] private AudioClip hataSesi;
    [SerializeField] private AudioClip basariSesi;

    private Color[] kabloRenkleri = new Color[]
    {
        new Color(0.9f, 0.2f, 0.2f, 1f), // Kırmızı
        new Color(0.2f, 0.5f, 1.0f, 1f), // Mavi
        new Color(1.0f, 0.8f, 0.1f, 1f), // Sarı
        new Color(0.2f, 0.9f, 0.3f, 1f)  // Yeşil
    };

    private int seciliSolIndex = -1;
    private bool[] baglandiMi = new bool[4];
    private float kalanSure;
    private bool oyunBitti = false;

    private void OnEnable()
    {
        kalanSure = toplamSure;
        seciliSolIndex = -1;
        oyunBitti = false;

        for (int i = 0; i < 4; i++)
        {
            baglandiMi[i] = false;
            int index = i;

            if (solKabloGorselleri[i] != null) solKabloGorselleri[i].color = kabloRenkleri[i];
            if (sagSoketGorselleri[i] != null) sagSoketGorselleri[i].color = kabloRenkleri[i];

            if (solKabloButonlari[i] != null)
            {
                solKabloButonlari[i].onClick.RemoveAllListeners();
                solKabloButonlari[i].onClick.AddListener(() => SolKabloSec(index));
            }

            if (sagSoketButonlari[i] != null)
            {
                sagSoketButonlari[i].onClick.RemoveAllListeners();
                sagSoketButonlari[i].onClick.AddListener(() => SagSoketeBagla(index));
            }
        }

        if (durumMetni != null)
        {
            durumMetni.text = "SOLDAN BİR KABLO SEÇİP SAĞDAKİ EŞLEŞEN SOKETE BAĞLA!";
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
        }
    }

    private void SolKabloSec(int index)
    {
        if (oyunBitti || baglandiMi[index]) return;

        seciliSolIndex = index;
        if (durumMetni != null)
        {
            durumMetni.text = $"KABLO {index + 1} SEÇİLDİ! SAĞDAKİ UYGUN SOKETE TIKLA!";
            durumMetni.color = kabloRenkleri[index];
        }
    }

    private void SagSoketeBagla(int sagIndex)
    {
        if (oyunBitti || seciliSolIndex < 0) return;

        // Doğru renk eşleşmesi mi?
        if (seciliSolIndex == sagIndex)
        {
            // BAŞARILI BAĞLANTI
            baglandiMi[seciliSolIndex] = true;
            SesCal(baglantiSesi);

            if (solKabloButonlari[seciliSolIndex] != null) solKabloButonlari[seciliSolIndex].interactable = false;
            if (sagSoketButonlari[sagIndex] != null) sagSoketButonlari[sagIndex].interactable = false;

            seciliSolIndex = -1;

            if (durumMetni != null)
            {
                durumMetni.text = "KABLO DOĞRU BAĞLANDI!";
                durumMetni.color = Color.green;
            }

            // Hepsi bağlandı mı?
            bool hepsiTamam = true;
            for (int i = 0; i < 4; i++)
            {
                if (!baglandiMi[i]) hepsiTamam = false;
            }

            if (hepsiTamam)
            {
                Basarili();
            }
        }
        else
        {
            // YANLIŞ BAĞLANTI (HATA)
            kalanSure -= cezaSuresi;
            SesCal(hataSesi);

            if (durumMetni != null)
            {
                durumMetni.text = $"YANLIŞ SOKET! KIVILCIM ATTI (-{cezaSuresi}s)!";
                durumMetni.color = Color.red;
            }

            seciliSolIndex = -1;
        }
    }

    private void Basarili()
    {
        oyunBitti = true;
        SesCal(basariSesi);
        if (durumMetni != null)
        {
            durumMetni.text = "JENERATÖR ÇALIŞTI! ZİNDANDAN KAÇILIYOR...";
            durumMetni.color = Color.cyan;
        }
        StartCoroutine(GecikmeliKurtul());
    }

    private IEnumerator GecikmeliKurtul()
    {
        yield return new WaitForSecondsRealtime(1.2f);
        if (ZindanKurtulmaManager.instance != null) ZindanKurtulmaManager.instance.ZindandanKurtul();
        else SceneManager.LoadScene(1);
    }

    private void Basarisiz()
    {
        oyunBitti = true;
        if (durumMetni != null)
        {
            durumMetni.text = "ELEKTRİK KESTİ! ZİNDANDAN ÇIKAMADIN!";
            durumMetni.color = Color.red;
        }
        StartCoroutine(GecikmeliBasarisiz());
    }

    private IEnumerator GecikmeliBasarisiz()
    {
        yield return new WaitForSecondsRealtime(2f);
        if (ZindanKurtulmaManager.instance != null) ZindanKurtulmaManager.instance.HaklariSifirla();
        SceneManager.LoadScene(0);
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
