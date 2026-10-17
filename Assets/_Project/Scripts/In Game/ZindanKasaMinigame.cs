using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Zindan Minigame: Kasa Kadranı (Çevirmeli Kasa Kilidi)
/// Oyuncu Sol/Sağ butonlar ile kilit kadranını çevirerek 3 haneli şifreyi sırayla doğru sayılara getirir.
/// </summary>
public class ZindanKasaMinigame : MonoBehaviour
{
    [Header("Geri Sayım Ayarları")]
    [SerializeField] private float toplamSure = 25f;

    [Header("UI Elemanları")]
    [SerializeField] private TextMeshProUGUI sayaçMetni;
    [SerializeField] private TextMeshProUGUI durumMetni;
    [SerializeField] private TextMeshProUGUI sifreMetni;
    [SerializeField] private TextMeshProUGUI mevcutSayiMetni;
    [SerializeField] private Button solDondurButonu;
    [SerializeField] private Button sagDondurButonu;
    [SerializeField] private RectTransform kadranIbreTransform;

    [Header("Ses Efektleri")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip klikSesi;
    [SerializeField] private AudioClip basariSesi;

    private float kalanSure;
    private int[] hedefSifre = new int[3];
    private int aktifHaneIndex = 0;
    private int mevcutKadranSayisi = 0; // 0 - 99 arası
    private bool oyunBitti = false;

    private void OnEnable()
    {
        kalanSure = toplamSure;
        aktifHaneIndex = 0;
        mevcutKadranSayisi = 0;
        oyunBitti = false;

        // Rastgele 3 haneli kasa şifresi oluştur (örneğin: 25 - 60 - 15)
        hedefSifre[0] = Random.Range(15, 35);
        hedefSifre[1] = Random.Range(50, 75);
        hedefSifre[2] = Random.Range(10, 30);

        if (solDondurButonu != null)
        {
            solDondurButonu.onClick.RemoveAllListeners();
            solDondurButonu.onClick.AddListener(() => KadranDondur(-2));
        }

        if (sagDondurButonu != null)
        {
            sagDondurButonu.onClick.RemoveAllListeners();
            sagDondurButonu.onClick.AddListener(() => KadranDondur(2));
        }

        GörunumuGuncelle();
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

    public void KadranDondur(int miktar)
    {
        if (oyunBitti) return;

        mevcutKadranSayisi = (mevcutKadranSayisi + miktar + 100) % 100;
        SesCal(klikSesi);

        // Kadran ibresini dairesel döndür
        if (kadranIbreTransform != null)
        {
            float aci = -(mevcutKadranSayisi / 100f) * 360f;
            kadranIbreTransform.localRotation = Quaternion.Euler(0, 0, aci);
        }

        // Doğru sayıya ulaşıldı mı? (Tölerans: +/- 1)
        int hedef = hedefSifre[aktifHaneIndex];
        if (Mathf.Abs(mevcutKadranSayisi - hedef) <= 1)
        {
            aktifHaneIndex++;
            SesCal(klikSesi);

            if (aktifHaneIndex >= 3)
            {
                Basarili();
            }
            else
            {
                if (durumMetni != null)
                {
                    durumMetni.text = $"HANE {aktifHaneIndex} KİLİTLENDİ! SONRAKİ SAYIYI ÇEVİR!";
                    durumMetni.color = Color.green;
                }
            }
        }

        GörunumuGuncelle();
    }

    private void GörunumuGuncelle()
    {
        if (mevcutSayiMetni != null)
        {
            mevcutSayiMetni.text = $"KADRAN: {mevcutKadranSayisi:D2}";
        }

        if (sifreMetni != null)
        {
            string h1 = aktifHaneIndex > 0 ? $"{hedefSifre[0]:D2}" : (aktifHaneIndex == 0 ? $"[{hedefSifre[0]:D2}]" : "??");
            string h2 = aktifHaneIndex > 1 ? $"{hedefSifre[1]:D2}" : (aktifHaneIndex == 1 ? $"[{hedefSifre[1]:D2}]" : "??");
            string h3 = aktifHaneIndex > 2 ? $"{hedefSifre[2]:D2}" : (aktifHaneIndex == 2 ? $"[{hedefSifre[2]:D2}]" : "??");

            sifreMetni.text = $"ŞİFRE HEDEFLERİ: {h1} - {h2} - {h3}";
        }

        if (durumMetni != null && aktifHaneIndex < 3)
        {
            durumMetni.text = $"KADRANI ÇEVİREREK [{hedefSifre[aktifHaneIndex]:D2}] SAYISINA GEL!";
            durumMetni.color = Color.yellow;
        }
    }

    private void Basarili()
    {
        oyunBitti = true;
        SesCal(basariSesi);
        if (durumMetni != null)
        {
            durumMetni.text = "KASA KİLİDİ AÇILDI! ZİNDANDAN KURTULDUN!";
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
            durumMetni.text = "ŞİFRE YANLIŞ! KISA DEVRE YAPTI!";
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
