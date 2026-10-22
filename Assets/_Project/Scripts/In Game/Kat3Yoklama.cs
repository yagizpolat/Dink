using System.Collections;
using UnityEngine;

/// <summary>
/// Kat 3, "Yoklama": Daire 3'ün içi bir lise sınıfının köşesi. Sıraya oyuncunun adı kazınmış, yanındakine "11".
/// Öğretmen masasındaki 2019-2020 sınıf defterinde oyuncunun adının karşısında 02.03.2020'den itibaren her gün "Y" var;
/// dönem sonunda kaydı silinmiş. Defter kapanınca floresan susar ve "11" sırasından
/// bir kalem tıklamaya başlar: oyuncu bakarken susar, arkasını dönünce sürer, yaklaştıkça hızlanır, her seferinde
/// 11'de durur. Oyuncu sınıftan çıkarken sıra gıcırdar (biri kalkmış), teneffüs zili kapalı kapıların arkasından
/// çalar, uğultu yükselip kesilir, kabinde 4 yanar. Sesler burada üretilir.
/// </summary>
public class Kat3Yoklama : MonoBehaviour
{
    [SerializeField] private Collider logbook;
    [SerializeField] private float reach = 1.8f;
    [SerializeField] private TMPro.TMP_Text carvedName;

    [Header("Yanındaki sıra")]
    [SerializeField] private AudioSource tapSource;
    [SerializeField] private AudioSource chairSource;
    [SerializeField] private float classroomExitX = 2.75f;
    [SerializeField] private float lookAngle = 55f;

    [Header("Ses")]
    [SerializeField] private AudioSource[] bellSources;
    [SerializeField] private AudioSource crowdSource;
    [SerializeField] private AudioSource humSource;
    [SerializeField] private Light[] classroomLights;
    [SerializeField, Range(0f, 0.5f)] private float darkResidual = 0.12f;
    [SerializeField] private Color darkTint = new Color(0.55f, 0.65f, 0.85f);

    [Header("Asansör")]
    [SerializeField] private GameObject nextFloorLight;
    [SerializeField] private AudioSource nextFloorDing;

    private bool rang;
    private Camera cam;

    private void Awake()
    {
        if (nextFloorLight != null) nextFloorLight.SetActive(false);
        if (carvedName != null) carvedName.text = PlayerName();
        // Kalemin sıraya vuruşu: kuru, kısa, tahta tınısı
        if (tapSource != null)
            tapSource.clip = Synth(0.12f, t => (Mathf.Sin(2f * Mathf.PI * 950f * t) * 0.5f
                                               + (Mathf.PerlinNoise(t * 9000f, 0.1f) - 0.5f) * 0.6f) * Mathf.Exp(-t * 70f));
        // Sıranın demir ayağı döşemede sürtünür: biri kalkıyor
        if (chairSource != null)
            chairSource.clip = Synth(0.9f, t => (Mathf.PerlinNoise(t * 2500f, 0.7f) - 0.5f) * 0.7f
                                              * Mathf.Sin(2f * Mathf.PI * (180f + 220f * t) * t) * Mathf.Sin(Mathf.PI * t / 0.9f));

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

        // Floresanlar iki kez tökezleyip söner, uğultu kesilir; sınıfa yalnız koridordan ışık sızar
        float[] intensity = new float[classroomLights.Length];
        for (int i = 0; i < classroomLights.Length; i++) intensity[i] = classroomLights[i].intensity;
        foreach (float wait in new[] { 0.08f, 0.15f, 0.06f, 0.3f })
        {
            SetLights(intensity, 0f);
            yield return new WaitForSeconds(wait);
            SetLights(intensity, 1f);
            yield return new WaitForSeconds(0.1f);
        }
        SetLights(intensity, 0f);
        if (humSource != null) humSource.Stop();

        // Göz karanlığa alışır: tam siyah kalmaz, soğuk ve çok loş bir artık ışık yavaşça belirir
        yield return new WaitForSeconds(0.6f);
        foreach (Light l in classroomLights) if (l != null) l.color = darkTint;
        for (float t = 0f; t < 1.8f; t += Time.deltaTime)
        {
            SetLights(intensity, darkResidual * Mathf.SmoothStep(0f, 1f, t / 1.8f));
            yield return null;
        }
        SetLights(intensity, darkResidual);
        yield return new WaitForSeconds(0.1f);
        yield return Tapping();

        if (chairSource != null) chairSource.Play();
        yield return new WaitForSeconds(2f);

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

        yield return new WaitForSeconds(2f);
        ElevatorController.DepartureUnlocked = true;
        if (nextFloorLight != null) nextFloorLight.SetActive(true);
        if (nextFloorDing != null) nextFloorDing.Play();
    }

    // Biri yanındaki sırada kalemle vuruyor. Oyuncu ona bakarken susar; on bir vuruşta bir durup bekler.
    // Oyuncu en az bir tam on biri duyup sınıftan çıkınca biter.
    private IEnumerator Tapping()
    {
        Transform player = FindAnyObjectByType<PlayerWalker>()?.transform;
        int count = 0;
        bool heardEleven = false;
        while (player != null && tapSource != null && !(heardEleven && player.position.x < classroomExitX))
        {
            if (Time.timeScale == 0f || Watched()) { yield return null; continue; }

            tapSource.Play();
            if (++count == 11)
            {
                count = 0;
                heardEleven = true;
                yield return new WaitForSeconds(2.8f);
                continue;
            }
            // Yaklaştıkça sabırsızlanır
            float d = Vector3.Distance(player.position, tapSource.transform.position);
            yield return new WaitForSeconds(Mathf.Lerp(0.38f, 0.95f, Mathf.InverseLerp(1f, 4.5f, d)));
        }
    }

    private void SetLights(float[] intensity, float k)
    {
        for (int i = 0; i < classroomLights.Length; i++) classroomLights[i].intensity = intensity[i] * k;
    }

    private bool Watched()
    {
        if (cam == null) cam = Camera.main;
        if (cam == null) return false;
        Vector3 to = tapSource.transform.position - cam.transform.position;
        return Vector3.Angle(cam.transform.forward, to) < lookAngle;
    }

    private static string PlayerName()
    {
        string name = LobbyLogbook.SignedName;
        if (string.IsNullOrEmpty(name) && LeaderboardManager.instance != null) name = LeaderboardManager.instance.GetPlayerName();
        return string.IsNullOrEmpty(name) ? "Hakan Kaya" : name;
    }

    private static string LogbookText()
    {
        string name = DinkLang.Upper(PlayerName());
        // 2 Mart'tan 5 Haziran'a kadar her okul günü "yok" işareti; hafta sonları boş
        string mark = DinkLang.T("A", "Y", "F", "A", "F", "F", "Н");
        var days = new System.Text.StringBuilder();
        var d = new System.DateTime(2020, 3, 2);
        int col = 0;
        for (; d <= new System.DateTime(2020, 6, 5); d = d.AddDays(1))
        {
            if (d.DayOfWeek == System.DayOfWeek.Saturday || d.DayOfWeek == System.DayOfWeek.Sunday) continue;
            days.Append(mark).Append(' ');
            if (++col % 10 == 0) days.Append('\n');
        }

        const string rule = "--------------------------";
        return DinkLang.T("CLASS 11-C  ATTENDANCE\n2019-2020 / TERM 2", "11-C SINIFI  YOKLAMA\n2019-2020 / 2. DÖNEM", "KLASSE 11c  ANWESENHEIT\n2019/20 / 2. HALBJAHR",
                          "CLASSE DE 1re C  APPEL\n2019-2020 / 3e TRIMESTRE", "CLASE 11-C  ASISTENCIA\n2019-2020 / 3.er TRIMESTRE", "TURMA 11-C  CHAMADA\n2019-2020 / 3.º PERÍODO",
                          "11 «В» КЛАСС  ПОСЕЩАЕМОСТЬ\n2019-2020 / 4-Я ЧЕТВЕРТЬ") + "\n" + rule + "\n" +
               "112  " + name + "\n\n" + days.ToString().TrimEnd() + "\n\n" +
               DinkLang.T("ABSENT: {0} DAYS", "DEVAMSIZLIK: {0} GÜN", "FEHLTAGE: {0}", "ABSENCES : {0} JOURS", "FALTAS: {0} DÍAS", "FALTAS: {0} DIAS", "ПРОПУСКИ: {0} ДН.").Replace("{0}", col.ToString()) + "\n" + rule + "\n05.06.20\n" +
               DinkLang.T("Removed from the roll for absence.",
                          "Devamsızlıktan kaydı silinmiştir.",
                          "Wegen Fehlzeiten von der Schule abgemeldet.",
                          "Radié pour absences.",
                          "Dado de baja por faltas.",
                          "Matrícula anulada por faltas.",
                          "Отчислен за пропуски.");
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
