using System.Collections;
using UnityEngine;

/// <summary>
/// Kat 1, "Mesai bitmiyor": Daire 3'ün içindeki market reyonu.
/// Kasadaki fiş yazıcısı durmadan oyuncunun mesai fişini basar; tezgâhta Daire 4'ten Murat'ın notu durur.
/// İkisi de okununca kabindeki 2. kat butonu yanar. Sesler (yazıcı, floresan, kapı zili) burada üretilir.
/// </summary>
public class Kat1Market : MonoBehaviour
{
    [SerializeField] private Collider receipt;
    [SerializeField] private Collider letter;
    [SerializeField] private Collider marketDoorway;
    [SerializeField] private float reach = 1.8f;

    [Header("Ses")]
    [SerializeField] private AudioSource printerSource;
    [SerializeField] private AudioSource humSource;
    [SerializeField] private AudioSource chimeSource;

    [Header("Asansör")]
    [SerializeField] private GameObject nextFloorLight;
    [SerializeField] private AudioSource nextFloorDing;

    public static bool NextFloorUnlocked { get; private set; }

    private bool readReceipt, readLetter, chimed;
    private Camera cam;

    private void Awake()
    {
        NextFloorUnlocked = false;
        if (nextFloorLight != null) nextFloorLight.SetActive(false);

        if (printerSource != null) printerSource.clip = Synth(1.1f, PrinterSample);
        if (humSource != null)
        {
            humSource.clip = Synth(2f, t => Mathf.Sin(2f * Mathf.PI * 100f * t) * 0.12f + Mathf.Sin(2f * Mathf.PI * 200f * t) * 0.05f);
            humSource.loop = true;
            humSource.Play();
        }
        if (chimeSource != null)
            chimeSource.clip = Synth(1.6f, t => Bell(t, 659f) + (t > 0.45f ? Bell(t - 0.45f, 523f) : 0f));
        if (nextFloorDing != null && nextFloorDing.clip == null)
            nextFloorDing.clip = Synth(1.4f, t => Bell(t, 880f));
    }

    private IEnumerator Start()
    {
        // Yazıcı hiç susmaz: birkaç saniyede bir yeni bir fiş daha
        var wait = new WaitForSeconds(4.5f);
        while (true)
        {
            yield return wait;
            if (printerSource != null) printerSource.Play();
        }
    }

    private void Update()
    {
        if (!chimed && marketDoorway != null && PlayerInside(marketDoorway))
        {
            chimed = true;
            if (chimeSource != null) chimeSource.Play();
        }

        if (Time.timeScale == 0f) return;
        if (!Input.GetMouseButtonDown(0) && !Input.GetKeyDown(KeyCode.E)) return;
        if (MektupNotHintUI.instance != null && MektupNotHintUI.instance.IsPanelOpen) return;

        if (cam == null) cam = Camera.main;
        if (cam == null) return;
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (receipt != null && receipt.Raycast(ray, out _, reach))
        {
            MektupNotHintUI.Instance.OpenText(ReceiptText(), true, false);
            readReceipt = true;
        }
        else if (letter != null && letter.Raycast(ray, out _, reach))
        {
            MektupNotHintUI.Instance.OpenText(LetterText(), false, true);
            readLetter = true;
        }
        else return;

        if (readReceipt && readLetter && !NextFloorUnlocked) StartCoroutine(Unlock());
    }

    private IEnumerator Unlock()
    {
        NextFloorUnlocked = true;
        // Panel kapanana kadar bekle; oyuncu döndüğünde asansör çoktan haber vermiş olsun
        while (MektupNotHintUI.instance != null && MektupNotHintUI.instance.IsPanelOpen) yield return null;
        yield return new WaitForSeconds(1.5f);
        if (nextFloorLight != null) nextFloorLight.SetActive(true);
        if (nextFloorDing != null) nextFloorDing.Play();
    }

    private static bool PlayerInside(Collider area)
    {
        PlayerWalker walker = FindAnyObjectByType<PlayerWalker>();
        return walker != null && area.bounds.Contains(walker.transform.position + Vector3.up * 0.5f);
    }

    private static bool IsTR => LanguageManager.instance != null && LanguageManager.instance.CurrentLanguage == LanguageManager.Language.TR;

    private static string ReceiptText()
    {
        string name = LobbyLogbook.SignedName;
        if (string.IsNullOrEmpty(name) && LeaderboardManager.instance != null) name = LeaderboardManager.instance.GetPlayerName();
        if (string.IsNullOrEmpty(name)) name = "Hakan Kaya";
        name = name.ToUpperInvariant();

        return IsTR
            ? "KARANFİL MARKET\nKasa 1\n--------------------------\nPERSONEL: " + name +
              "\n\nMESAİ BAŞLANGICI\n14.11.23        23:42\n\nMESAİ BİTİŞİ\n--.--.--        --:--\n--------------------------\nSÜRE                  —\nÖDENEN                —\n--------------------------\nYİNE BEKLERİZ"
            : "KARANFIL MARKET\nTill 1\n--------------------------\nSTAFF: " + name +
              "\n\nSHIFT START\n14.11.23        23:42\n\nSHIFT END\n--.--.--        --:--\n--------------------------\nHOURS                 —\nPAID                  —\n--------------------------\nSEE YOU AGAIN";
    }

    private static string LetterText()
    {
        return IsTR
            ? "Fişleri sayma. Ben saydım.\n\nHer birinde kendi adım vardı, saat hep 23:42. Kalemle üstünü çizdim, yazıcı yenisini bastı.\n\nBurası benim marketim değildi ama rafların yerini ezbere biliyordum. Senin de biliyorsun, değil mi?\n\nAsansör seni yukarı götürecek. Her kat biraz daha tanıdık gelecek. Bu iyi bir şey değil.\n\n— M.K., Daire 4"
            : "Don't count the receipts. I did.\n\nEvery one had my own name on it, always 23:42. I scratched it out with a pen and the printer printed another.\n\nThis wasn't my market, but I knew where every shelf was. You do too, don't you?\n\nThe elevator will take you up. Every floor will feel a little more familiar. That is not a good thing.\n\n— M.K., Flat 4";
    }

    // Nokta vuruşlu yazıcı: satır satır kısa cızırtılar
    private static float PrinterSample(float t)
    {
        float line = Mathf.Repeat(t, 0.11f);
        if (line > 0.075f || t > 0.95f) return 0f;
        float buzz = Mathf.Sign(Mathf.Sin(2f * Mathf.PI * 780f * t)) * 0.08f;
        float noise = (Mathf.PerlinNoise(t * 4000f, 0.3f) - 0.5f) * 0.25f;
        return (buzz + noise) * Mathf.Clamp01(line * 60f);
    }

    private static float Bell(float t, float f) =>
        (Mathf.Sin(2f * Mathf.PI * f * t) * 0.35f + Mathf.Sin(2f * Mathf.PI * f * 2.01f * t) * 0.1f) * Mathf.Exp(-t * 2.5f);

    private static AudioClip Synth(float seconds, System.Func<float, float> f)
    {
        const int rate = 44100;
        int n = Mathf.RoundToInt(seconds * rate);
        float[] data = new float[n];
        for (int i = 0; i < n; i++) data[i] = f(i / (float)rate);
        AudioClip clip = AudioClip.Create("Kat1Ses", n, 1, rate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
