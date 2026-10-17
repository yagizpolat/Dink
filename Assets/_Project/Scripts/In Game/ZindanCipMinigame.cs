using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Zindan Minigame: Çip Yolu Kesintisiz Çizgi (Grid Circuit Path Finder)
/// Oyuncu 9 karelik 3x3 çip devresinde doğru sıradaki karelere basarak devreyi tamamlar.
/// </summary>
public class ZindanCipMinigame : MonoBehaviour
{
    [Header("Geri Sayım Ayarları")]
    [SerializeField] private float toplamSure = 20f;
    [SerializeField] private float cezaSuresi = 3f;

    [Header("UI Elemanları")]
    [SerializeField] private TextMeshProUGUI sayaçMetni;
    [SerializeField] private TextMeshProUGUI durumMetni;
    [SerializeField] private Button[] cipButonlari = new Button[9];
    [SerializeField] private Image[] cipGorselleri = new Image[9];

    [Header("Ses Efektleri")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip cipSesi;
    [SerializeField] private AudioClip hataSesi;
    [SerializeField] private AudioClip basariSesi;

    private float kalanSure;
    private int[] hedefYol = new int[] { 0, 1, 4, 7, 8 }; // 5 adımlı doğru yol
    private int mevcutAdim = 0;
    private bool oyunBitti = false;

    private Color normalRenk = new Color(0.2f, 0.2f, 0.2f, 1f);
    private Color aktifRenk = new Color(0.2f, 0.9f, 0.3f, 1f);

    private void OnEnable()
    {
        kalanSure = toplamSure;
        mevcutAdim = 0;
        oyunBitti = false;

        for (int i = 0; i < 9; i++)
        {
            int index = i;
            if (cipGorselleri[i] != null) cipGorselleri[i].color = normalRenk;

            if (cipButonlari[i] != null)
            {
                cipButonlari[i].onClick.RemoveAllListeners();
                cipButonlari[i].onClick.AddListener(() => CipKaresiTikla(index));
            }
        }

        if (durumMetni != null)
        {
            durumMetni.text = "ÇİP DEVRESİNDE DOĞRU KARELERE SIRAYLA BAS!";
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

    private void CipKaresiTikla(int index)
    {
        if (oyunBitti) return;

        // Doğru sıradaki kare mi?
        if (hedefYol[mevcutAdim] == index)
        {
            if (cipGorselleri[index] != null) cipGorselleri[index].color = aktifRenk;
            SesCal(cipSesi);
            mevcutAdim++;

            if (durumMetni != null)
            {
                durumMetni.text = $"DEVRE ADIMI {mevcutAdim}/5 İLERLEDİ!";
                durumMetni.color = Color.green;
            }

            if (mevcutAdim >= hedefYol.Length)
            {
                Basarili();
            }
        }
        else
        {
            // Yanlış kare
            kalanSure -= cezaSuresi;
            SesCal(hataSesi);
            mevcutAdim = 0;

            for (int i = 0; i < 9; i++)
            {
                if (cipGorselleri[i] != null) cipGorselleri[i].color = normalRenk;
            }

            if (durumMetni != null)
            {
                durumMetni.text = $"KISA DEVRE! DEVRE SIFIRLANDI (-{cezaSuresi}s)!";
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
            durumMetni.text = "ÇİP DEVRESİ TAMAMLANTI! KAPI AÇILDI!";
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
            durumMetni.text = "DEVRE YANDI! SÜRE BİTTİ!";
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
