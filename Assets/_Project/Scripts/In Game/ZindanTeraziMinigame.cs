using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Zindan Minigame: Antik Terazi Ağırlık Dengeleme
/// Oyuncu 5 farklı ağırlık butonuna tıklayıp sağ kefeyi sol kefedeki nesnenin gramına tam eşitler.
/// </summary>
public class ZindanTeraziMinigame : MonoBehaviour
{
    [Header("Geri Sayım Ayarları")]
    [SerializeField] private float toplamSure = 20f;

    [Header("UI Elemanları")]
    [SerializeField] private TextMeshProUGUI sayaçMetni;
    [SerializeField] private TextMeshProUGUI durumMetni;
    [SerializeField] private TextMeshProUGUI teraziMetni;
    [SerializeField] private Button[] agirlikButonlari = new Button[5];
    [SerializeField] private Image[] agirlikGorselleri = new Image[5];

    [Header("Ses Efektleri")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip tasKoymaSesi;
    [SerializeField] private AudioClip basariSesi;

    private float kalanSure;
    private int solKefeHedefAgirlik = 45; // Hedef gram
    private int sagKefeMevcutAgirlik = 0;
    private int[] agirlikDegerleri = new int[] { 5, 10, 15, 20, 30 };
    private bool[] tasEkliMi = new bool[5];
    private bool oyunBitti = false;

    private void OnEnable()
    {
        kalanSure = toplamSure;
        sagKefeMevcutAgirlik = 0;
        oyunBitti = false;

        // Hedef ağırlık (35 - 65 gram arası)
        solKefeHedefAgirlik = Random.Range(3, 7) * 10 + 5;

        for (int i = 0; i < 5; i++)
        {
            int index = i;
            tasEkliMi[i] = false;

            if (agirlikGorselleri[i] != null) agirlikGorselleri[i].color = new Color(0.4f, 0.4f, 0.4f, 1f);

            if (agirlikButonlari[i] != null)
            {
                agirlikButonlari[i].onClick.RemoveAllListeners();
                agirlikButonlari[i].onClick.AddListener(() => TasTikla(index));
            }
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

    private void TasTikla(int index)
    {
        if (oyunBitti || index < 0 || index >= 5) return;

        tasEkliMi[index] = !tasEkliMi[index];
        SesCal(tasKoymaSesi);

        GuncelleUI();
    }

    private void GuncelleUI()
    {
        sagKefeMevcutAgirlik = 0;
        for (int i = 0; i < 5; i++)
        {
            if (tasEkliMi[i])
            {
                sagKefeMevcutAgirlik += agirlikDegerleri[i];
            }

            if (agirlikGorselleri[i] != null)
            {
                agirlikGorselleri[i].color = tasEkliMi[i] ? new Color(0.9f, 0.7f, 0.2f, 1f) : new Color(0.4f, 0.4f, 0.4f, 1f);
            }
        }

        if (teraziMetni != null)
        {
            teraziMetni.text = $"SAĞ KEFE: {sagKefeMevcutAgirlik}g / HEDEF SOL: {solKefeHedefAgirlik}g";
            teraziMetni.color = sagKefeMevcutAgirlik == solKefeHedefAgirlik ? Color.green : Color.yellow;
        }

        if (durumMetni != null)
        {
            durumMetni.text = sagKefeMevcutAgirlik == solKefeHedefAgirlik ? "TERAZİ DENGELENDİ!" : "AĞIRLIK TAŞLARINA TIKLAYARAK TERAZİYİ EŞİTLE!";
            durumMetni.color = sagKefeMevcutAgirlik == solKefeHedefAgirlik ? Color.green : Color.yellow;
        }

        if (sagKefeMevcutAgirlik == solKefeHedefAgirlik)
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
            durumMetni.text = "TAŞ DENGELENDİ! KAPAK KALKTI!";
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
            durumMetni.text = "DENGESİZ AĞIRLIK! TERAZİ KIRILDI!";
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
