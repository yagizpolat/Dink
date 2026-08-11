using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Zindan Minigame 3: Paslı Vana Basınç Dengeleme
/// 3 adet vanayı basılı tutarak ibrelerini aynı anda yeşil güvenli bölgede 1.5 sn tutma minigame'i.
/// </summary>
public class ZindanVanaMinigame : MonoBehaviour
{
    [Header("Geri Sayım Ayarları")]
    [SerializeField] private float toplamSure = 22f;
    [SerializeField] private float hedefSure = 1.5f; // Yeşil bölgede kalma süresi

    [Header("UI Elemanları")]
    [SerializeField] private TextMeshProUGUI sayaçMetni;
    [SerializeField] private TextMeshProUGUI durumMetni;
    [SerializeField] private Slider[] vanaSliderlari = new Slider[3];
    [SerializeField] private Button[] vanaButonlari = new Button[3];
    [SerializeField] private Image[] hedefAlanGorselleri = new Image[3];

    [Header("Ses Efektleri")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip vanaSesi;
    [SerializeField] private AudioClip basariSesi;

    private float kalanSure;
    private bool[] vanaBasiliTutuluyor = new bool[3];
    private float hedefKalmaSayaci = 0f;
    private bool oyunBitti = false;

    private float hedefMin = 0.45f;
    private float hedefMax = 0.70f;

    private void OnEnable()
    {
        kalanSure = toplamSure;
        hedefKalmaSayaci = 0f;
        oyunBitti = false;

        for (int i = 0; i < 3; i++)
        {
            int index = i;
            vanaBasiliTutuluyor[i] = false;
            if (vanaSliderlari[i] != null) vanaSliderlari[i].value = Random.Range(0.05f, 0.25f);
        }

        if (durumMetni != null)
        {
            durumMetni.text = "VANALARA BASILI TUTARAK İBRELERİ SARI/YEŞİL BÖLGEDE TUT!";
            durumMetni.color = Color.yellow;
        }
    }

    private void Update()
    {
        if (oyunBitti) return;

        // 1. Geri Sayım
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

        // 2. Vana Basınç Değişimleri
        bool hepsiHedefte = true;

        for (int i = 0; i < 3; i++)
        {
            if (vanaSliderlari[i] == null) continue;

            // Fare ile buton üzerine basılı tutuluyor mu?
            bool basili = Input.GetMouseButton(0) && IsMouseOverButton(vanaButonlari[i]);

            if (basili)
            {
                vanaSliderlari[i].value += 0.8f * Time.deltaTime;
            }
            else
            {
                // Basılmadığında basınç yavaşça düşer
                vanaSliderlari[i].value -= 0.35f * Time.deltaTime;
            }

            vanaSliderlari[i].value = Mathf.Clamp01(vanaSliderlari[i].value);

            // Hedef bölgede mi?
            bool hedefte = vanaSliderlari[i].value >= hedefMin && vanaSliderlari[i].value <= hedefMax;
            if (!hedefte) hepsiHedefte = false;

            if (hedefAlanGorselleri[i] != null)
            {
                hedefAlanGorselleri[i].color = hedefte ? new Color(0.2f, 0.9f, 0.2f, 0.8f) : new Color(1f, 0.8f, 0f, 0.6f);
            }
        }

        // 3. Hepsi Hedefte Tutuluyor mu?
        if (hepsiHedefte)
        {
            hedefKalmaSayaci += Time.deltaTime;
            if (durumMetni != null)
            {
                durumMetni.text = $"BASINÇ DENGELENDİ! TUTMA SÜRESİ: {(hedefSure - hedefKalmaSayaci):F1}s";
                durumMetni.color = Color.green;
            }

            if (hedefKalmaSayaci >= hedefSure)
            {
                Basarili();
            }
        }
        else
        {
            hedefKalmaSayaci = Mathf.Max(0f, hedefKalmaSayaci - Time.deltaTime * 2f);
        }
    }

    private bool IsMouseOverButton(Button button)
    {
        if (button == null) return false;
        return RectTransformUtility.RectangleContainsScreenPoint(button.GetComponent<RectTransform>(), Input.mousePosition, null);
    }

    private void Basarili()
    {
        oyunBitti = true;
        SesCal(basariSesi);
        if (durumMetni != null)
        {
            durumMetni.text = "BASINÇ SABİTLENDİ! KAPAK AÇILDI!";
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
            durumMetni.text = "BASINÇ PATLADI! ZİNDANDAN ÇIKAMADIN!";
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
