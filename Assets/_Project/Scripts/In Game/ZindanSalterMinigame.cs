using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Zindan Minigame: Eski Şalter / Sigorta Kutusu Bulmacası
/// Oyuncu 5 paslı şaltere tıklayarak voltaj değerlerini tam 100V seviyesine getirir.
/// </summary>
public class ZindanSalterMinigame : MonoBehaviour
{
    [Header("Geri Sayım Ayarları")]
    [SerializeField] private float toplamSure = 20f;

    [Header("UI Elemanları")]
    [SerializeField] private TextMeshProUGUI sayaçMetni;
    [SerializeField] private TextMeshProUGUI durumMetni;
    [SerializeField] private TextMeshProUGUI voltajMetni;
    [SerializeField] private Slider voltajSlider;
    [SerializeField] private Button[] salterButonlari = new Button[5];
    [SerializeField] private Image[] salterGorselleri = new Image[5];

    [Header("Ses Efektleri")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip salterSesi;
    [SerializeField] private AudioClip basariSesi;

    private float kalanSure;
    private bool[] salterAcikMi = new bool[5];
    private int[] salterVoltajlari = new int[] { 25, 40, 15, 35, 20 }; // Doğru kombinasyon: 40 + 35 + 25 = 100V veya 40 + 20 + 25 + 15 = 100V
    private int mevcutVoltaj = 0;
    private int hedefVoltaj = 100;
    private bool oyunBitti = false;

    private void OnEnable()
    {
        kalanSure = toplamSure;
        mevcutVoltaj = 0;
        oyunBitti = false;

        for (int i = 0; i < 5; i++)
        {
            int index = i;
            salterAcikMi[i] = false;

            if (salterButonlari[i] != null)
            {
                salterButonlari[i].onClick.RemoveAllListeners();
                salterButonlari[i].onClick.AddListener(() => SalterTikla(index));
            }
        }

        VoltajGuncelle();
    }

    private void Update()
    {
        if (oyunBitti) return;

        // Geri Sayım
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

    private void SalterTikla(int index)
    {
        if (oyunBitti || index < 0 || index >= 5) return;

        salterAcikMi[index] = !salterAcikMi[index];
        SesCal(salterSesi);

        VoltajGuncelle();
    }

    private void VoltajGuncelle()
    {
        mevcutVoltaj = 0;

        for (int i = 0; i < 5; i++)
        {
            if (salterAcikMi[i])
            {
                mevcutVoltaj += salterVoltajlari[i];
            }

            if (salterGorselleri[i] != null)
            {
                // Açık: Yeşil/Parlak, Kapalı: Koyu Kırmızı
                salterGorselleri[i].color = salterAcikMi[i] ? new Color(0.2f, 0.9f, 0.3f, 1f) : new Color(0.6f, 0.2f, 0.2f, 1f);
            }
        }

        if (voltajSlider != null)
        {
            voltajSlider.value = Mathf.Clamp01((float)mevcutVoltaj / hedefVoltaj);
        }

        if (voltajMetni != null)
        {
            voltajMetni.text = $"VOLTAJ: {mevcutVoltaj}V / {hedefVoltaj}V";
            voltajMetni.color = mevcutVoltaj == hedefVoltaj ? Color.green : (mevcutVoltaj > hedefVoltaj ? Color.red : Color.yellow);
        }

        if (durumMetni != null)
        {
            if (mevcutVoltaj == hedefVoltaj)
            {
                durumMetni.text = "TAM VOLTAJ SAĞLANDI! KAPAK AÇILIYOR!";
                durumMetni.color = Color.green;
            }
            else if (mevcutVoltaj > hedefVoltaj)
            {
                durumMetni.text = "YÜKSEK VOLTAJ! BAZI ŞALTERLERİ KAPAT!";
                durumMetni.color = Color.red;
            }
            else
            {
                durumMetni.text = "ŞALTERLERE TIKLAYARAK VOLTAJI 100V YAP!";
                durumMetni.color = Color.yellow;
            }
        }

        if (mevcutVoltaj == hedefVoltaj)
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
            durumMetni.text = "GÜÇ SAĞLANDI! ZİNDAN KAPISI AÇILDI!";
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
            durumMetni.text = "VOLTAJ ÇÖKTÜ! SÜRE BİTTİ!";
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
