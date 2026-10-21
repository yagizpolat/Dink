using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Kat 5, "Kavga gecesi": uzun koridor, inilen asansör geri gider. Oyuncu Daire 3'ü geçince kapı çarparak açılır,
/// marketten iş arkadaşı çıkar ve kovalar. Koşma yalnız bu katta var (Shift, nefes biter). Yakalanırsa ekran kararır
/// ve kat baştan başlar; koridorun ucundaki yangın merdivenine yetişirse bir üst kata geçer.
/// Sesler (kapı çarpması, ayak sesleri, nefes, darbe) burada üretilir.
/// </summary>
public class Kat5Kovalamaca : MonoBehaviour
{
    [Header("Koşma")]
    [SerializeField] private float runMultiplier = 2.3f;
    [SerializeField] private float staminaSeconds = 6f;
    [SerializeField] private float regenPerSecond = 0.35f;

    [Header("Kovalayan")]
    [SerializeField] private Transform chaser;
    [SerializeField] private Transform doorHinge;
    [SerializeField] private float doorOpenAngle = -100f;
    [SerializeField] private float wakeZ = 2.5f;
    [SerializeField] private Vector3 corridorEntry = new Vector3(1.6f, 0f, 4.5f);
    [SerializeField] private float chaseSpeed = 2.9f;
    [SerializeField] private float catchDistance = 0.85f;

    [Header("Çıkış")]
    [SerializeField] private float exitZ = -21.3f;
    [SerializeField] private string nextScene = "Kat6";

    [Header("Ses")]
    [SerializeField] private AudioSource doorSource;
    [SerializeField] private AudioSource stepSource;
    [SerializeField] private AudioSource breathSource;
    [SerializeField] private AudioSource hitSource;

    private enum Phase { Waiting, Emerging, Chasing, Over }
    private Phase phase = Phase.Waiting;
    private PlayerWalker walker;
    private float stamina, stepTimer;
    private bool winded;

    private void Awake()
    {
        stamina = staminaSeconds;
        if (chaser != null) chaser.gameObject.SetActive(false);

        if (doorSource != null)
            doorSource.clip = Synth(0.8f, t => ((Mathf.PerlinNoise(t * 3000f, 0.1f) - 0.5f) * 1.4f + Mathf.Sin(2f * Mathf.PI * 60f * t)) * Mathf.Exp(-t * 9f));
        if (stepSource != null)
            stepSource.clip = Synth(0.18f, t => ((Mathf.PerlinNoise(t * 1800f, 0.6f) - 0.5f) * 0.9f + Mathf.Sin(2f * Mathf.PI * 90f * t) * 0.6f) * Mathf.Exp(-t * 28f));
        if (breathSource != null)
        {
            // Hızlı soluk alıp verme: yükselip alçalan hışırtı
            breathSource.clip = Synth(1.2f, t => (Mathf.PerlinNoise(t * 2600f, 0.9f) - 0.5f) * Mathf.Pow(Mathf.Abs(Mathf.Sin(Mathf.PI * t / 0.6f)), 2f) * 0.6f);
            breathSource.loop = true;
            breathSource.volume = 0f;
            breathSource.Play();
        }
        if (hitSource != null)
            hitSource.clip = Synth(1.2f, t => ((Mathf.PerlinNoise(t * 900f, 0.2f) - 0.5f) * 1.6f + Mathf.Sin(2f * Mathf.PI * 45f * t)) * Mathf.Exp(-t * 4f));
    }

    private void Update()
    {
        if (Time.timeScale == 0f || phase == Phase.Over) return;
        if (walker == null) walker = FindAnyObjectByType<PlayerWalker>();
        if (walker == null) return;

        Run();

        Vector3 p = walker.transform.position;
        if (phase == Phase.Waiting && p.z < wakeZ) StartCoroutine(Emerge());
        if (phase == Phase.Chasing) Chase(p);
        if (p.z < exitZ) StartCoroutine(Escape());
    }

    // Shift basılıyken koşulur; nefes biterse dolana kadar (%40) yürümek zorunda
    private void Run()
    {
        bool moving = Mathf.Abs(Input.GetAxisRaw("Vertical")) + Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.1f;
        bool running = Input.GetKey(KeyCode.LeftShift) && moving && !winded && walker.enabled;
        if (running)
        {
            stamina -= Time.deltaTime;
            if (stamina <= 0f) { stamina = 0f; winded = true; }
        }
        else
        {
            stamina = Mathf.Min(staminaSeconds, stamina + regenPerSecond * Time.deltaTime);
            if (winded && stamina > staminaSeconds * 0.4f) winded = false;
        }
        walker.SpeedMultiplier = running ? runMultiplier : 1f;

        if (breathSource != null)
        {
            float tired = 1f - stamina / staminaSeconds;
            breathSource.volume = Mathf.Lerp(breathSource.volume, tired * 0.7f, 3f * Time.deltaTime);
            breathSource.pitch = 1f + tired * 0.35f;
        }
    }

    private IEnumerator Emerge()
    {
        phase = Phase.Emerging;
        // Daire 3'ün kapısı çarparak açılır
        if (doorSource != null) doorSource.Play();
        if (doorHinge != null)
        {
            Quaternion from = doorHinge.localRotation, to = from * Quaternion.Euler(0f, doorOpenAngle, 0f);
            for (float t = 0f; t < 0.25f; t += Time.deltaTime)
            {
                doorHinge.localRotation = Quaternion.Slerp(from, to, t / 0.25f);
                yield return null;
            }
            doorHinge.localRotation = to;
        }
        yield return new WaitForSeconds(0.7f);

        // Karanlık antreden koridora adım atar
        chaser.gameObject.SetActive(true);
        Vector3 start = chaser.position;
        for (float t = 0f; t < 0.9f; t += Time.deltaTime)
        {
            chaser.position = Vector3.Lerp(start, corridorEntry, t / 0.9f);
            Face(walker.transform.position);
            Step(0.45f);
            yield return null;
        }
        phase = Phase.Chasing;
        StartCoroutine(RunHint());
    }

    // Koşmanın yalnız burada olduğunu oyuncu bilmez: kovalama başlarken bir kez, kısa süre gösterilir
    private IEnumerator RunHint()
    {
        var go = new GameObject("KosIpucu");
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 500;
        var text = new GameObject("Yazi").AddComponent<TMPro.TextMeshProUGUI>();
        text.transform.SetParent(go.transform, false);
        text.rectTransform.anchorMin = new Vector2(0.5f, 0.12f);
        text.rectTransform.anchorMax = new Vector2(0.5f, 0.12f);
        text.rectTransform.sizeDelta = new Vector2(800f, 60f);
        text.alignment = TMPro.TextAlignmentOptions.Center;
        text.fontSize = 30f;
        text.characterSpacing = 8f;
        text.text = DinkLang.T("SHIFT — RUN", "SHIFT — KOŞ", "SHIFT — RENNEN", "SHIFT — COURIR", "SHIFT — CORRER", "SHIFT — CORRER", "SHIFT — БЕЖАТЬ");
        for (float t = 0f; t < 3.5f; t += Time.unscaledDeltaTime)
        {
            float a = t < 0.3f ? t / 0.3f : t > 2.5f ? 1f - (t - 2.5f) : 1f;
            text.color = new Color(0.85f, 0.85f, 0.8f, a * 0.85f);
            yield return null;
        }
        Destroy(go);
    }

    private void Chase(Vector3 player)
    {
        Vector3 to = player - chaser.position;
        to.y = 0f;
        float d = to.magnitude;
        if (d < catchDistance) { StartCoroutine(Caught()); return; }

        Vector3 next = chaser.position + to / d * chaseSpeed * Time.deltaTime;
        next.x = Mathf.Clamp(next.x, 0.5f, 2.3f);
        chaser.position = next;
        Face(player);
        Step(0.33f);

        // Koşar adım sallanması
        Transform model = chaser.childCount > 0 ? chaser.GetChild(0) : null;
        if (model != null) model.localPosition = new Vector3(0f, Mathf.Abs(Mathf.Sin(Time.time * 9.5f)) * 0.05f, 0f);
    }

    private void Face(Vector3 player)
    {
        Vector3 dir = player - chaser.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.001f) chaser.rotation = Quaternion.Slerp(chaser.rotation, Quaternion.LookRotation(dir), 10f * Time.deltaTime);
    }

    private void Step(float interval)
    {
        stepTimer -= Time.deltaTime;
        if (stepTimer > 0f || stepSource == null) return;
        stepTimer = interval;
        stepSource.transform.position = chaser.position;
        stepSource.pitch = Random.Range(0.85f, 1.1f);
        stepSource.Play();
    }

    private IEnumerator Caught()
    {
        phase = Phase.Over;
        walker.SpeedMultiplier = 1f;
        PlayerControls.SetEnabled(false);

        // Oyuncu son anda ona döner
        Transform head = Camera.main.transform;
        Quaternion from = walker.transform.rotation;
        Vector3 dir = chaser.position - walker.transform.position; dir.y = 0f;
        Quaternion to = Quaternion.LookRotation(dir);
        for (float t = 0f; t < 0.25f; t += Time.deltaTime)
        {
            walker.transform.rotation = Quaternion.Slerp(from, to, t / 0.25f);
            head.localRotation = Quaternion.Slerp(head.localRotation, Quaternion.identity, t / 0.25f);
            yield return null;
        }
        if (hitSource != null) hitSource.Play();
        if (breathSource != null) breathSource.Stop();
        GameObject black = Blackout();
        yield return new WaitForSeconds(2.2f);
        Destroy(black);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private IEnumerator Escape()
    {
        phase = Phase.Over;
        walker.SpeedMultiplier = 1f;
        if (doorSource != null) { doorSource.transform.position = walker.transform.position; doorSource.Play(); }
        // Üst kat henüz yoksa ana menüye döner (Kat6 yapılınca kendiliğinden ona geçer)
        string target = Application.CanStreamedLevelBeLoaded(nextScene) ? nextScene : DinkScenes.MainMenu;
        yield return SceneDoorTrigger.FadeAndLoad(target, 1.5f);
    }

    private static GameObject Blackout()
    {
        var go = new GameObject("YakalanmaKarartma");
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;
        var image = new GameObject("Siyah").AddComponent<UnityEngine.UI.Image>();
        image.transform.SetParent(go.transform, false);
        image.rectTransform.anchorMin = Vector2.zero;
        image.rectTransform.anchorMax = Vector2.one;
        image.color = Color.black;
        return go;
    }

    private static AudioClip Synth(float seconds, System.Func<float, float> f)
    {
        const int rate = 44100;
        int n = Mathf.RoundToInt(seconds * rate);
        float[] data = new float[n];
        for (int i = 0; i < n; i++) data[i] = f(i / (float)rate);
        AudioClip clip = AudioClip.Create("Kat5Ses", n, 1, rate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
