using System.Collections;
using UnityEngine;

/// <summary>
/// Kat 3, "Yoklama": Daire 3'ün içi bir lise sınıfının köşesi. Sıraya oyuncunun adı kazınmış, yanındakine "11".
/// Öğretmen masasındaki sınıf defterinde oyuncunun adının karşısında Eylül'den beri her gün "Y" var;
/// 14.11.23'te kaydı silinmiş. Defter okununca teneffüs zili kapalı daire kapılarının arkasından çalar,
/// boş sınıf uğultusu yükselip kesilir, kabinde 4 yanar. Sesler (zil, uğultu, floresan) burada üretilir.
/// </summary>
public class Kat3Yoklama : MonoBehaviour
{
    [SerializeField] private Collider logbook;
    [SerializeField] private float reach = 1.8f;
    [SerializeField] private TMPro.TMP_Text carvedName;

    [Header("Ses")]
    [SerializeField] private AudioSource[] bellSources;
    [SerializeField] private AudioSource crowdSource;
    [SerializeField] private AudioSource humSource;

    [Header("Asansör")]
    [SerializeField] private GameObject nextFloorLight;
    [SerializeField] private AudioSource nextFloorDing;

    private bool rang;
    private Camera cam;

    private void Awake()
    {
        if (nextFloorLight != null) nextFloorLight.SetActive(false);
        if (carvedName != null) carvedName.text = PlayerName();

        // Eski okul zili: elektrikli çan, tok ve kesintisiz
        AudioClip bell = Synth(3.2f, t => (Mathf.Sign(Mathf.Sin(2f * Mathf.PI * 25f * t)) * 0.5f + 0.5f)
                                          * Mathf.Sin(2f * Mathf.PI * 1250f * t) * 0.3f * Mathf.Clamp01((3.2f - t) * 8f));
        foreach (AudioSource s in bellSources) if (s != null) s.clip = bell;
        if (crowdSource != null)
        {
            // Teneffüs uğultusu: kalabalık gibi kıpırdayan gürültü, kelime yok
            var rng = new System.Random(5);
            crowdSource.clip = Synth(4f, t => ((float)rng.NextDouble() - 0.5f) * (0.25f + 0.2f * Mathf.PerlinNoise(t * 3f, 0.5f))
                                              * Mathf.Sin(2f * Mathf.PI * 0.5f * t) * Mathf.Sin(2f * Mathf.PI * 0.5f * t));
            crowdSource.loop = true;
        }
        if (humSource != null)
        {
            humSource.clip = Synth(2f, t => Mathf.Sin(2f * Mathf.PI * 100f * t) * 0.1f + Mathf.Sin(2f * Mathf.PI * 200f * t) * 0.04f);
            humSource.loop = true;
            humSource.Play();
        }
        if (nextFloorDing != null && nextFloorDing.clip == null)
            nextFloorDing.clip = Synth(1.4f, t => Mathf.Sin(2f * Mathf.PI * 880f * t) * 0.35f * Mathf.Exp(-t * 2.5f));
    }

    private void Update()
    {
        if (Time.timeScale == 0f) return;
        if (!Input.GetMouseButtonDown(0) && !Input.GetKeyDown(KeyCode.E)) return;
        if (MektupNotHintUI.instance != null && MektupNotHintUI.instance.IsPanelOpen) return;

        if (cam == null) cam = Camera.main;
        if (cam == null) return;
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (logbook == null || !logbook.Raycast(ray, out _, reach)) return;

        MektupNotHintUI.Instance.OpenText(LogbookText(), false, false);
        if (!rang) StartCoroutine(Recess());
    }

    private IEnumerator Recess()
    {
        rang = true;
        while (MektupNotHintUI.instance != null && MektupNotHintUI.instance.IsPanelOpen) yield return null;
        yield return new WaitForSeconds(1f);

        foreach (AudioSource s in bellSources) if (s != null) s.Play();
        yield return new WaitForSeconds(2.6f);
        if (crowdSource != null)
        {
            crowdSource.volume = 0f;
            crowdSource.Play();
            for (float t = 0f; t < 3f; t += Time.deltaTime) { crowdSource.volume = t / 3f * 0.8f; yield return null; }
            yield return new WaitForSeconds(1.5f);
            // Kesilir: kapı kapanmış gibi, bir anda
            crowdSource.Stop();
        }
        if (humSource != null) humSource.Stop();

        yield return new WaitForSeconds(2f);
        ElevatorController.DepartureUnlocked = true;
        if (nextFloorLight != null) nextFloorLight.SetActive(true);
        if (nextFloorDing != null) nextFloorDing.Play();
    }

    private static bool IsTR => LanguageManager.instance != null && LanguageManager.instance.CurrentLanguage == LanguageManager.Language.TR;

    private static string PlayerName()
    {
        string name = LobbyLogbook.SignedName;
        if (string.IsNullOrEmpty(name) && LeaderboardManager.instance != null) name = LeaderboardManager.instance.GetPlayerName();
        return string.IsNullOrEmpty(name) ? "Hakan Kaya" : name;
    }

    private static string LogbookText()
    {
        string name = PlayerName().ToUpperInvariant();
        // Eylül'den 14 Kasım'a kadar her okul günü "Y"; hafta sonları boş
        var days = new System.Text.StringBuilder();
        var d = new System.DateTime(2023, 9, 11);
        int col = 0;
        for (; d <= new System.DateTime(2023, 11, 14); d = d.AddDays(1))
        {
            if (d.DayOfWeek == System.DayOfWeek.Saturday || d.DayOfWeek == System.DayOfWeek.Sunday) continue;
            days.Append(IsTR ? "Y " : "A ");
            if (++col % 10 == 0) days.Append('\n');
        }

        return IsTR
            ? "11-C SINIFI  YOKLAMA\n2023-2024 / 1. DÖNEM\n--------------------------\n" +
              "112  " + name + "\n\n" + days.ToString().TrimEnd() +
              "\n\nDEVAMSIZLIK: " + col + " GÜN\n--------------------------\n14.11.23\nDevamsızlıktan kaydı silinmiştir.\n\nYerine kimse oturmasın."
            : "CLASS 11-C  ATTENDANCE\n2023-2024 / TERM 1\n--------------------------\n" +
              "112  " + name + "\n\n" + days.ToString().TrimEnd() +
              "\n\nABSENT: " + col + " DAYS\n--------------------------\n14.11.23\nRemoved from the roll for absence.\n\nLet no one take this seat.";
    }

    private static AudioClip Synth(float seconds, System.Func<float, float> f)
    {
        const int rate = 44100;
        int n = Mathf.RoundToInt(seconds * rate);
        float[] data = new float[n];
        for (int i = 0; i < n; i++) data[i] = f(i / (float)rate);
        AudioClip clip = AudioClip.Create("Kat3Ses", n, 1, rate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
