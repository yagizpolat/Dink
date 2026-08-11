using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Zindan Minigame 4: Antik Rün Şifresi (Memory Sequence)
/// Rünlerin yanıp sönme sırasını aklında tutup sırayla tıklama minigame'i.
/// </summary>
public class ZindanRunMinigame : MonoBehaviour
{
    [Header("Geri Sayım Ayarları")]
    [SerializeField] private float toplamSure = 20f;
    [SerializeField] private float cezaSuresi = 3f;

    [Header("UI Elemanları")]
    [SerializeField] private TextMeshProUGUI sayaçMetni;
    [SerializeField] private TextMeshProUGUI durumMetni;
    [SerializeField] private Button[] runButonlari = new Button[4];
    [SerializeField] private Image[] runGorselleri = new Image[4];

    [Header("Ses Efektleri")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip runSesi;
    [SerializeField] private AudioClip hataSesi;
    [SerializeField] private AudioClip basariSesi;

    private List<int> runSekansi = new List<int>();
    private int oyuncuAdimIndex = 0;
    private float kalanSure;
    private bool sekansGosteriliyor = false;
    private bool oyunBitti = false;

    private Color normalRenk = new Color(0.3f, 0.2f, 0.4f, 1f);
    private Color parlayanRenk = new Color(0.9f, 0.4f, 1.0f, 1f); // Mor parlama

    private void OnEnable()
    {
        kalanSure = toplamSure;
        oyuncuAdimIndex = 0;
        oyunBitti = false;

        for (int i = 0; i < 4; i++)
        {
            int index = i;
            if (runGorselleri[i] != null) runGorselleri[i].color = normalRenk;
            if (runButonlari[i] != null)
            {
                runButonlari[i].onClick.RemoveAllListeners();
                runButonlari[i].onClick.AddListener(() => RunTiklandi(index));
            }
        }

        // Rastgele 4 adımlı sekans oluştur
        runSekansi.Clear();
        for (int i = 0; i < 4; i++)
        {
            runSekansi.Add(Random.Range(0, 4));
        }

        StartCoroutine(SekansiGoster());
    }

    private void Update()
    {
        if (oyunBitti || sekansGosteriliyor) return;

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

    private IEnumerator SekansiGoster()
    {
        sekansGosteriliyor = true;
        if (durumMetni != null)
        {
            durumMetni.text = "RÜN SIRASINI İZLE!";
            durumMetni.color = Color.magenta;
        }

        yield return new WaitForSecondsRealtime(0.8f);

        foreach (int runIndex in runSekansi)
        {
            if (runGorselleri[runIndex] != null) runGorselleri[runIndex].color = parlayanRenk;
            SesCal(runSesi);
            yield return new WaitForSecondsRealtime(0.5f);
            if (runGorselleri[runIndex] != null) runGorselleri[runIndex].color = normalRenk;
            yield return new WaitForSecondsRealtime(0.2f);
        }

        sekansGosteriliyor = false;
        if (durumMetni != null)
        {
            durumMetni.text = "ŞİMDİ SIRAYLA RÜNLERE TIKLA!";
            durumMetni.color = Color.yellow;
        }
    }

    private void RunTiklandi(int index)
    {
        if (oyunBitti || sekansGosteriliyor) return;

        // Doğru rün mü?
        if (runSekansi[oyuncuAdimIndex] == index)
        {
            // Doğru tıklandı
            SesCal(runSesi);
            StartCoroutine(RunParlat(index));
            oyuncuAdimIndex++;

            if (durumMetni != null)
            {
                durumMetni.text = $"ADIM {oyuncuAdimIndex} / 4 DOĞRU!";
                durumMetni.color = Color.green;
            }

            if (oyuncuAdimIndex >= runSekansi.Count)
            {
                Basarili();
            }
        }
        else
        {
            // Yanlış tıklandı
            kalanSure -= cezaSuresi;
            SesCal(hataSesi);
            oyuncuAdimIndex = 0;

            if (durumMetni != null)
            {
                durumMetni.text = $"YANLIŞ RÜN! -{cezaSuresi}s CEZA!";
                durumMetni.color = Color.red;
            }

            StartCoroutine(SekansiGoster());
        }
    }

    private IEnumerator RunParlat(int index)
    {
        if (runGorselleri[index] != null) runGorselleri[index].color = Color.green;
        yield return new WaitForSecondsRealtime(0.25f);
        if (runGorselleri[index] != null) runGorselleri[index].color = normalRenk;
    }

    private void Basarili()
    {
        oyunBitti = true;
        SesCal(basariSesi);
        if (durumMetni != null)
        {
            durumMetni.text = "RÜN MÜHÜRÜ KIRILDI! ZİNDANDAN KAÇILIYOR...";
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
            durumMetni.text = "MÜHÜR PATLADI! ZİNDANDAN ÇIKAMADIN!";
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
