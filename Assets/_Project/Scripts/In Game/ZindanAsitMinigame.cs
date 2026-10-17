using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Zindan Minigame: Asit Tüpü Nötrleme (Acid Chemical Neutralizer)
/// Oyuncu 3 damlatıcı tüpten doğru miktarlarda ekleyerek tüpteki çözeltiyi tam 7.0 pH nötr seviyesine getirir.
/// </summary>
public class ZindanAsitMinigame : MonoBehaviour
{
    [Header("Geri Sayım Ayarları")]
    [SerializeField] private float toplamSure = 22f;

    [Header("UI Elemanları")]
    [SerializeField] private TextMeshProUGUI sayaçMetni;
    [SerializeField] private TextMeshProUGUI durumMetni;
    [SerializeField] private TextMeshProUGUI phMetni;
    [SerializeField] private Slider phSlider;
    [SerializeField] private Button damlatici1Btn;
    [SerializeField] private Button damlatici2Btn;
    [SerializeField] private Button damlatici3Btn;

    [Header("Ses Efektleri")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip damlaSesi;
    [SerializeField] private AudioClip basariSesi;

    private float kalanSure;
    private float mevcutPh = 2.4f;
    private float hedefPh = 7.0f;
    private bool oyunBitti = false;

    private void OnEnable()
    {
        kalanSure = toplamSure;
        mevcutPh = Random.Range(2.0f, 3.5f);
        oyunBitti = false;

        if (damlatici1Btn != null)
        {
            damlatici1Btn.onClick.RemoveAllListeners();
            damlatici1Btn.onClick.AddListener(() => Damlat(1.5f)); // +1.5 pH
        }

        if (damlatici2Btn != null)
        {
            damlatici2Btn.onClick.RemoveAllListeners();
            damlatici2Btn.onClick.AddListener(() => Damlat(-0.9f)); // -0.9 pH (Asit artırıcı)
        }

        if (damlatici3Btn != null)
        {
            damlatici3Btn.onClick.RemoveAllListeners();
            damlatici3Btn.onClick.AddListener(() => Damlat(2.2f)); // +2.2 pH
        }

        GuncelleUI();
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

    private void Damlat(float miktar)
    {
        if (oyunBitti) return;

        mevcutPh += miktar;
        SesCal(damlaSesi);

        // 9.5 pH üstüne çıkarsa asit patlar (aşırı bazik ceza)
        if (mevcutPh > 9.5f)
        {
            Basarisiz();
            return;
        }

        if (Mathf.Abs(mevcutPh - hedefPh) <= 0.3f)
        {
            Basarili();
        }

        GuncelleUI();
    }

    private void GuncelleUI()
    {
        if (phMetni != null)
        {
            phMetni.text = $"ÇÖZELTİ: pH {mevcutPh:F1} / 7.0";
            phMetni.color = Mathf.Abs(mevcutPh - hedefPh) <= 0.3f ? Color.green : Color.yellow;
        }

        if (phSlider != null)
        {
            phSlider.value = Mathf.Clamp01(mevcutPh / 10.0f);
        }

        if (durumMetni != null)
        {
            durumMetni.text = "DAMLATICILARLA ÇÖZELTİYİ NÖTR (7.0 pH) YAP!";
            durumMetni.color = Color.yellow;
        }
    }

    private void Basarili()
    {
        oyunBitti = true;
        SesCal(basariSesi);
        if (durumMetni != null)
        {
            durumMetni.text = "ASİT NÖTRLENDİ! KİLİT ERİDİ!";
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
            durumMetni.text = "ASİT TAŞTI VE PATLADI!";
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
