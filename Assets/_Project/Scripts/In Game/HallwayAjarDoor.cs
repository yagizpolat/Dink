using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Koridor görevi: Daire 2'nin kapısı her karanlıkta biraz daha aralanır (4 cm → 15 cm → yarım).
/// Işık yanıkken kapıyı kapatmak (bak + E) onu başa döndürür. Üç kez kapatınca Daire 4'ün kapı altından
/// ışık sızar; kapı yarım açıkken bir karanlık daha geçerse içeriden bir şey çıkar (şimdilik: yakalandın, sahne baştan).
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class HallwayAjarDoor : MonoBehaviour
{
    [SerializeField] private HallwayLightTimer timer;
    [SerializeField] private Transform pivot;           // menteşe ekseni; +Y dönüş kapıyı içeri açar
    [SerializeField] private Collider leafCollider;
    [SerializeField] private Light tvLight;             // içerideki soğuk televizyon ışığı
    [SerializeField] private Light finishLight;         // görev bitince yanan Daire 4 kapı altı ışığı
    [SerializeField] private Light[] switchGlows;       // görev bitince söner
    [SerializeField] private float[] stageAngles = { 0f, 2.5f, 9f, 45f };
    [SerializeField] private float[] stageTv = { 0f, 1.5f, 6f, 0f };
    [SerializeField] private int closesToFinish = 3;
    [SerializeField] private float darkAdvanceSeconds = 25f;  // ışığı hiç yakmayana da kapı ilerler
    [SerializeField] private float reach = 2f;

    private int stage;
    private int closes;
    private bool wasLit;
    private bool done;
    private bool busy;
    private float darkTime;
    private float angle;
    private Camera cam;
    private AudioSource source;
    private AudioSource tvSource;
    private TMP_Text label;
    private AudioClip thud;

    private void Awake()
    {
        source = GetComponent<AudioSource>();
        source.spatialBlend = 1f;
        source.minDistance = 0.8f;
        source.maxDistance = 12f;
        tvSource = gameObject.AddComponent<AudioSource>();
        tvSource.spatialBlend = 1f;
        tvSource.minDistance = 0.6f;
        tvSource.maxDistance = 7f;
        tvSource.loop = true;
        tvSource.clip = Murmur(4f);
        thud = Thud();
        var target = leafCollider.GetComponent<InteractionTarget>();
        if (target == null) target = leafCollider.gameObject.AddComponent<InteractionTarget>();
        target.Configure(InteractionTarget.Verb.Press, reach);
        label = CreateLabel();
        label.text = DinkLang.T("Keep the hallway in order.", "Koridoru düzende tut.", "Halte den Flur in Ordnung.",
            "Garde le couloir en ordre.", "Mantén el pasillo en orden.", "Mantenha o corredor em ordem.", "Следи за порядком в коридоре.",
            "让走廊保持原样。", "廊下をいつも通りに保て。", "복도를 그대로 유지하라.");
        if (finishLight != null) finishLight.enabled = false;
        ApplyStage(0, instant: true);
    }

    private void Update()
    {
        if (done || PauseService.IsPaused || Time.timeScale == 0f) return;

        bool lit = timer.IsLit;
        if (wasLit && !lit) Advance();
        darkTime = lit ? 0f : darkTime + Time.deltaTime;
        if (darkTime > darkAdvanceSeconds) { darkTime = 0f; Advance(); }
        wasLit = lit;

        if (busy || stage == 0) return;
        if (!Input.GetMouseButtonDown(0) && !Input.GetKeyDown(KeyCode.E)) return;
        if (cam == null) cam = Camera.main;
        if (cam == null) return;
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (!leafCollider.Raycast(ray, out _, reach)) return;

        if (!lit)
        {
            ScreenHint.Show(DinkLang.T("I can't see the handle.", "Kolu göremiyorum.", "Ich sehe die Klinke nicht.",
                "Je ne vois pas la poignée.", "No veo el picaporte.", "Não vejo a maçaneta.", "Не вижу ручку.",
                "看不见门把手。", "取っ手が見えない。", "손잡이가 보이지 않는다."), 2f);
            return;
        }
        StartCoroutine(Close());
    }

    private void Advance()
    {
        if (busy) return;
        if (stage >= stageAngles.Length - 1) { StartCoroutine(Caught()); return; }
        ApplyStage(stage + 1, instant: false);
    }

    private void ApplyStage(int next, bool instant)
    {
        stage = next;
        if (tvLight != null) tvLight.intensity = stageTv[stage];
        if (stage == 2) tvSource.Play(); else tvSource.Stop();
        if (instant) { SetAngle(stageAngles[stage]); return; }
        StartCoroutine(Swing(stageAngles[stage], Random.Range(1.4f, 2.6f), creak: true));
    }

    private IEnumerator Swing(float to, float seconds, bool creak)
    {
        busy = true;
        if (creak)
        {
            source.pitch = Random.Range(0.85f, 1.1f);
            source.PlayOneShot(Creak(seconds, Random.Range(0, 1000)), 0.7f);
        }
        float from = angle;
        for (float t = 0f; t < seconds; t += Time.deltaTime)
        {
            // eski menteşe: takılarak ilerler
            float k = Mathf.SmoothStep(0f, 1f, t / seconds);
            k = Mathf.Clamp01(k + 0.03f * Mathf.Sin(t * 23f) * (1f - k));
            SetAngle(Mathf.Lerp(from, to, k));
            yield return null;
        }
        SetAngle(to);
        busy = false;
    }

    private IEnumerator Close()
    {
        busy = true;
        tvSource.Stop();
        if (tvLight != null) tvLight.intensity = 0f;
        float from = angle;
        const float seconds = 0.35f;
        for (float t = 0f; t < seconds; t += Time.deltaTime)
        {
            SetAngle(Mathf.Lerp(from, 0f, (t / seconds) * (t / seconds)));
            yield return null;
        }
        SetAngle(0f);
        source.pitch = 1f;
        source.PlayOneShot(thud);
        stage = 0;
        closes++;
        busy = false;
        if (closes >= closesToFinish) Finish();
    }

    private void Finish()
    {
        done = true;
        timer.Shutdown();
        foreach (var g in switchGlows) if (g != null) g.enabled = false;
        if (finishLight != null) finishLight.enabled = true;
        label.text = DinkLang.T("It stopped. A light is on at the end of the hall.", "Durdu. Koridorun sonunda bir ışık yanıyor.",
            "Es hat aufgehört. Am Ende des Flurs brennt Licht.", "C'est fini. Une lumière brille au bout du couloir.",
            "Se detuvo. Hay una luz al final del pasillo.", "Parou. Há uma luz no fim do corredor.",
            "Прекратилось. В конце коридора горит свет.", "停下了。走廊尽头亮着一盏灯。", "止まった。廊下の奥に明かりが点いている。",
            "멈췄다. 복도 끝에 불빛이 켜져 있다.");
    }

    private IEnumerator Caught()
    {
        done = true;
        tvSource.Stop();
        source.pitch = 0.7f;
        source.PlayOneShot(Creak(0.6f, 7), 1f);
        yield return Swing(100f, 0.6f, creak: false);
        label.text = DinkLang.T("Something came out.", "İçeriden bir şey çıktı.", "Etwas kam heraus.", "Quelque chose est sorti.",
            "Algo salió.", "Algo saiu.", "Что-то вышло.", "有什么东西出来了。", "何かが出てきた。", "무언가 나왔다.");
        yield return new WaitForSeconds(2.5f);
        SceneManager.LoadScene(gameObject.scene.name);
    }

    private void SetAngle(float a)
    {
        angle = a;
        pivot.localRotation = Quaternion.Euler(0f, a, 0f);
    }

    private TMP_Text CreateLabel()
    {
        var go = new GameObject("[KoridorGorev]");
        go.transform.SetParent(transform, false);
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 800;
        var scaler = go.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        var textGo = new GameObject("Yazi");
        textGo.transform.SetParent(go.transform, false);
        var tmp = textGo.AddComponent<TextMeshProUGUI>();
        tmp.font = NicknameLogbookUI.LoadFont("Fonts/PTMono-Regular");
        tmp.fontSize = 26;
        tmp.alignment = TextAlignmentOptions.TopLeft;
        tmp.raycastTarget = false;
        tmp.color = new Color(0.88f, 0.86f, 0.8f, 0.85f);
        RectTransform r = tmp.rectTransform;
        r.anchorMin = r.anchorMax = r.pivot = new Vector2(0f, 1f);
        r.anchoredPosition = new Vector2(48f, -40f);
        r.sizeDelta = new Vector2(1100f, 120f);
        return tmp;
    }

    /// <summary>Menteşe gıcırtısı: sürtünme darbeleri dizisi, perdesi kayar. Her tohum farklı bir gıcırtı verir.</summary>
    private static AudioClip Creak(float length, int seed)
    {
        const int rate = 44100;
        int n = Mathf.CeilToInt(length * rate);
        var data = new float[n];
        var rng = new System.Random(seed);
        float f0 = 180f + (float)rng.NextDouble() * 120f, f1 = f0 * (0.7f + (float)rng.NextDouble() * 0.6f);
        float phase = 0f, y = 0f;
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)rate, u = t / length;
            float f = Mathf.Lerp(f0, f1, u) * (1f + 0.04f * Mathf.Sin(t * 37f));
            phase += f / rate;
            // stick-slip: her döngüde keskin bir darbe, ardından sönüm
            float pulse = Mathf.Exp(-(phase % 1f) * 9f);
            y += 0.3f * ((float)(rng.NextDouble() * 2 - 1) - y);
            float env = Mathf.Sin(Mathf.PI * u) * (0.6f + 0.4f * Mathf.PerlinNoise(t * 6f, seed * 0.1f));
            data[i] = 0.5f * env * (0.8f * pulse + 0.2f * y);
        }
        var clip = AudioClip.Create("Creak", n, 1, rate, false);
        clip.SetData(data, 0);
        return clip;
    }

    /// <summary>Ağır kapı kapanışı: alçak gövde + kısa kilit tıkı.</summary>
    private static AudioClip Thud()
    {
        const int rate = 44100;
        int n = Mathf.CeilToInt(0.5f * rate);
        var data = new float[n];
        var rng = new System.Random(11);
        float y = 0f;
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)rate;
            y += 0.08f * ((float)(rng.NextDouble() * 2 - 1) - y);
            float body = Mathf.Sin(2 * Mathf.PI * 62f * t) * Mathf.Exp(-t * 14f);
            float latch = t > 0.06f ? Mathf.Exp(-(t - 0.06f) * 120f) * (float)(rng.NextDouble() * 2 - 1) * 0.4f : 0f;
            data[i] = 0.9f * body + 1.5f * y * Mathf.Exp(-t * 20f) + latch;
        }
        var clip = AudioClip.Create("Thud", n, 1, rate, false);
        clip.SetData(data, 0);
        return clip;
    }

    /// <summary>Kapı ardından boğuk televizyon mırıltısı (döngü).</summary>
    private static AudioClip Murmur(float length)
    {
        const int rate = 44100;
        int n = Mathf.CeilToInt(length * rate);
        var data = new float[n];
        var rng = new System.Random(3);
        float y = 0f;
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)rate;
            y += 0.04f * ((float)(rng.NextDouble() * 2 - 1) - y);
            float speech = 0.5f + 0.5f * Mathf.Sin(2 * Mathf.PI * 3.3f * t) * Mathf.Sin(2 * Mathf.PI * 0.75f * t);
            data[i] = 2.2f * y * speech;
        }
        var clip = AudioClip.Create("Murmur", n, 1, rate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
