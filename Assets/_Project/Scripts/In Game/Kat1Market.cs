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
        // Buton ışıkla birlikte açılır, ışık yanmadan basılan 2 bir yere götürmez
        ElevatorController.DepartureUnlocked = true;
        if (nextFloorLight != null) nextFloorLight.SetActive(true);
        if (nextFloorDing != null) nextFloorDing.Play();
    }

    private static bool PlayerInside(Collider area)
    {
        PlayerWalker walker = FindAnyObjectByType<PlayerWalker>();
        return walker != null && area.bounds.Contains(walker.transform.position + Vector3.up * 0.5f);
    }

    private static string ReceiptText()
    {
        string name = LobbyLogbook.SignedName;
        if (string.IsNullOrEmpty(name) && LeaderboardManager.instance != null) name = LeaderboardManager.instance.GetPlayerName();
        if (string.IsNullOrEmpty(name)) name = "Hakan Kaya";
        name = name.ToUpperInvariant();

        const string rule = "--------------------------";
        return DinkLang.T("KARANFIL MARKET", "KARANFİL MARKET", "KARANFIL MARKT", "KARANFIL MARCHÉ", "SUPERMERCADO KARANFIL", "MERCADO KARANFIL", "МАГАЗИН «КАРАНФИЛ»") + "\n" +
               DinkLang.T("Till 1", "Kasa 1", "Kasse 1", "Caisse 1", "Caja 1", "Caixa 1", "Касса 1") + "\n" + rule + "\n" +
               DinkLang.T("STAFF: ", "PERSONEL: ", "PERSONAL: ", "EMPLOYÉ : ", "EMPLEADO: ", "FUNCIONÁRIO: ", "СОТРУДНИК: ") + name + "\n\n" +
               DinkLang.T("SHIFT START", "MESAİ BAŞLANGICI", "SCHICHTBEGINN", "DÉBUT DE SERVICE", "INICIO DE TURNO", "INÍCIO DO TURNO", "НАЧАЛО СМЕНЫ") + "\n14.11.23        23:42\n\n" +
               DinkLang.T("SHIFT END", "MESAİ BİTİŞİ", "SCHICHTENDE", "FIN DE SERVICE", "FIN DE TURNO", "FIM DO TURNO", "КОНЕЦ СМЕНЫ") + "\n--.--.--        --:--\n" + rule + "\n" +
               DinkLang.T("HOURS", "SÜRE", "DAUER", "DURÉE", "HORAS", "HORAS", "ЧАСЫ").PadRight(22) + "—\n" +
               DinkLang.T("PAID", "ÖDENEN", "BEZAHLT", "PAYÉ", "PAGADO", "PAGO", "ОПЛАЧЕНО").PadRight(22) + "—\n" + rule + "\n" +
               DinkLang.T("SEE YOU AGAIN", "YİNE BEKLERİZ", "BIS ZUM NÄCHSTEN MAL", "À BIENTÔT", "VUELVA PRONTO", "VOLTE SEMPRE", "ЖДЁМ ВАС СНОВА");
    }

    private static string LetterText()
    {
        return DinkLang.T(
            "Don't count the receipts. I did.\n\nEvery one had my own name on it, always 23:42. I scratched it out with a pen and the printer printed another.\n\nThis wasn't my market, but I knew where every shelf was. You do too, don't you?\n\nThe elevator will take you up. Every floor will feel a little more familiar. That is not a good thing.\n\n— M.K., Flat 4",
            "Fişleri sayma. Ben saydım.\n\nHer birinde kendi adım vardı, saat hep 23:42. Kalemle üstünü çizdim, yazıcı yenisini bastı.\n\nBurası benim marketim değildi ama rafların yerini ezbere biliyordum. Senin de biliyorsun, değil mi?\n\nAsansör seni yukarı götürecek. Her kat biraz daha tanıdık gelecek. Bu iyi bir şey değil.\n\n— M.K., Daire 4",
            "Zähl die Kassenzettel nicht. Ich habe es getan.\n\nAuf jedem stand mein eigener Name, immer 23:42. Ich habe ihn mit dem Stift durchgestrichen, und der Drucker hat einen neuen gedruckt.\n\nDas war nicht mein Laden, aber ich wusste, wo jedes Regal steht. Du auch, nicht wahr?\n\nDer Aufzug bringt dich nach oben. Jedes Stockwerk wird dir ein bisschen vertrauter vorkommen. Das ist nichts Gutes.\n\n— M.K., Wohnung 4",
            "Ne compte pas les tickets. Moi, je l'ai fait.\n\nChacun portait mon propre nom, toujours 23:42. Je l'ai barré au stylo, et l'imprimante en a sorti un autre.\n\nCe n'était pas ma supérette, mais je savais où était chaque rayon. Toi aussi, n'est-ce pas ?\n\nL'ascenseur va te faire monter. Chaque étage te semblera un peu plus familier. Ce n'est pas une bonne chose.\n\n— M.K., appartement 4",
            "No cuentes los tiques. Yo lo hice.\n\nCada uno llevaba mi propio nombre, siempre a las 23:42. Lo taché con un bolígrafo y la impresora sacó otro.\n\nNo era mi tienda, pero sabía dónde estaba cada estante. Tú también lo sabes, ¿verdad?\n\nEl ascensor te llevará arriba. Cada piso te resultará un poco más familiar. Eso no es bueno.\n\n— M.K., piso 4",
            "Não contes os talões. Eu contei.\n\nCada um tinha o meu próprio nome, sempre às 23:42. Risquei-o com uma caneta e a impressora imprimiu outro.\n\nAquele não era o meu mercado, mas eu sabia onde ficava cada prateleira. Tu também sabes, não sabes?\n\nO elevador vai levar-te para cima. Cada andar vai parecer-te um pouco mais familiar. Isso não é bom sinal.\n\n— M.K., apartamento 4",
            "Не считай чеки. Я считал.\n\nНа каждом было моё имя, всегда 23:42. Я зачёркивал его ручкой, а принтер печатал новый.\n\nЭто был не мой магазин, но я знал наизусть, где какая полка. Ты ведь тоже знаешь, правда?\n\nЛифт повезёт тебя наверх. Каждый этаж будет казаться чуть более знакомым. Это не к добру.\n\n— М.К., квартира 4");
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
