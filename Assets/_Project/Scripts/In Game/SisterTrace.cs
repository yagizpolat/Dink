using System.Collections;
using UnityEngine;

/// <summary>
/// Katlara yerleştirilen toplanabilir nesne. Kimliği (0-4) <see cref="DinkSave"/> bit maskesinde tutulur;
/// alınmışsa sahne açılınca kapanır. Oyuncu bakıp E / sol tıkla alınca kaydedilir, nesne küçülerek kaybolur
/// ve tek bir yumuşak müzik kutusu notası çalar (ses burada üretilir). Arayüz yazısı yoktur.
/// <see cref="hiddenUntilRevealed"/> açıkken nesne sönük durur ve alınamaz; <see cref="Reveal"/> çağrılınca
/// ışığı / parıltısı yanar ve alınabilir olur.
/// </summary>
public class SisterTrace : MonoBehaviour
{
    [SerializeField, Range(0, DinkSave.TraceSlots - 1)] private int traceId;
    [SerializeField] private float reach = 1.8f;

    [Header("Gizli başlangıç (Reveal ile açılır)")]
    [SerializeField] private bool hiddenUntilRevealed;
    [Tooltip("Reveal'da etkinleştirilecek isteğe bağlı nesne (ör. parıltı)")]
    [SerializeField] private GameObject revealRoot;
    [Tooltip("Reveal'da sıfırdan yanacak ışık")]
    [SerializeField] private Light revealLight;
    [Tooltip("Reveal'da emisyonu yanacak renderer")]
    [SerializeField] private Renderer emissiveRenderer;
    [SerializeField, ColorUsage(false, true)] private Color emissionColor = new Color(1f, 0.85f, 0.6f) * 1.5f;
    [SerializeField] private float revealSeconds = 1f;

    [Header("Alma")]
    [SerializeField] private float vanishSeconds = 1.2f;
    [SerializeField, Range(0f, 1f)] private float noteVolume = 0.45f;

    private static readonly int EmissionId = Shader.PropertyToID("_EmissionColor");
    private static AudioClip noteClip;

    private Collider col;
    private InteractionTarget target;
    private Camera cam;
    private Transform player;
    private float lightIntensity;
    private bool interactable;
    private bool taken;
    private readonly RaycastHit[] hits = new RaycastHit[16];

    public bool HiddenUntilRevealed => hiddenUntilRevealed;

    private void Awake()
    {
        col = GetComponent<Collider>();
        if (col == null) col = gameObject.AddComponent<BoxCollider>();
        target = GetComponent<InteractionTarget>();
        if (target == null) target = gameObject.AddComponent<InteractionTarget>();
        target.Configure(InteractionTarget.Verb.Take, reach);

        if (revealLight != null) lightIntensity = revealLight.intensity;
        if (noteClip == null) noteClip = BuildNote();
    }

    private void Start()
    {
        if (DinkSave.IsTraceFound(traceId)) { gameObject.SetActive(false); return; }

        if (hiddenUntilRevealed)
        {
            // Sönük ama görünür: ışık ve parıltı kapalı, alınamaz
            SetInteractable(false);
            if (revealRoot != null) revealRoot.SetActive(false);
            if (revealLight != null) { revealLight.intensity = 0f; revealLight.enabled = false; }
            SetEmission(0f);
        }
        else
        {
            SetInteractable(true);
            if (revealRoot != null) revealRoot.SetActive(true);
        }
    }

    /// <summary>Gizli başlayan nesneyi ~1 sn içinde yakar ve alınabilir yapar.</summary>
    public void Reveal()
    {
        if (taken || interactable || !isActiveAndEnabled) return;
        StartCoroutine(RevealRoutine());
    }

    private IEnumerator RevealRoutine()
    {
        if (revealRoot != null) revealRoot.SetActive(true);
        if (revealLight != null) revealLight.enabled = true;
        for (float t = 0f; t < revealSeconds; t += Time.deltaTime)
        {
            float k = Mathf.SmoothStep(0f, 1f, t / revealSeconds);
            if (revealLight != null) revealLight.intensity = lightIntensity * k;
            SetEmission(k);
            yield return null;
        }
        if (revealLight != null) revealLight.intensity = lightIntensity;
        SetEmission(1f);
        SetInteractable(true);
    }

    private void SetInteractable(bool on)
    {
        interactable = on;
        if (target != null) target.enabled = on;
    }

    private void SetEmission(float k)
    {
        if (emissiveRenderer == null) return;
        Material m = emissiveRenderer.material;
        m.EnableKeyword("_EMISSION");
        m.SetColor(EmissionId, emissionColor * k);
    }

    private void Update()
    {
        if (taken || !interactable || Time.timeScale == 0f) return;
        if (!Input.GetMouseButtonDown(0) && !Input.GetKeyDown(KeyCode.E)) return;
        if (MektupNotHintUI.instance != null && MektupNotHintUI.instance.IsPanelOpen) return;

        if (cam == null) cam = Camera.main;
        if (cam == null) return;
        if (player == null) player = FindAnyObjectByType<PlayerWalker>()?.transform;

        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (FirstHit(ray) == col) StartCoroutine(Take());
    }

    // Menzil içindeki en yakın engel; oyuncunun kendisi ve başka tetikleyiciler sayılmaz
    private Collider FirstHit(Ray ray)
    {
        int n = Physics.RaycastNonAlloc(ray, hits, reach, ~0, QueryTriggerInteraction.Collide);
        Collider best = null;
        float bestDist = float.MaxValue;
        for (int i = 0; i < n; i++)
        {
            Collider c = hits[i].collider;
            if (player != null && c.transform.IsChildOf(player)) continue;
            if (c.isTrigger && c != col) continue;
            if (hits[i].distance < bestDist) { bestDist = hits[i].distance; best = c; }
        }
        return best;
    }

    private IEnumerator Take()
    {
        taken = true;
        SetInteractable(false);
        col.enabled = false;
        DinkSave.MarkTraceFound(traceId);
        DinkSave.Save();
        PlayNote();

        Vector3 startScale = transform.localScale;
        float startLight = revealLight != null ? revealLight.intensity : 0f;
        for (float t = 0f; t < vanishSeconds; t += Time.deltaTime)
        {
            float k = t / vanishSeconds;
            float e = k * k * (3f - 2f * k);
            transform.localScale = Vector3.LerpUnclamped(startScale, Vector3.zero, e);
            if (revealLight != null) revealLight.intensity = startLight * (1f - e);
            SetEmission(1f - e);
            yield return null;
        }
        gameObject.SetActive(false);
    }

    // Ses ayrı bir nesneden çalar; bu nesne kapanınca kesilmez
    private void PlayNote()
    {
        var go = new GameObject("IzNotasi");
        go.transform.position = transform.position;
        var src = go.AddComponent<AudioSource>();
        src.clip = noteClip;
        src.spatialBlend = 0.5f;
        float sfx = DinkPrefs.SfxSourceScale;
        src.volume = noteVolume * sfx;
        src.Play();
        Destroy(go, noteClip.length + 0.1f);
    }

    // Müzik kutusu notası: E6 sinüs, hızlı atak, ~1.6 sn üstel sönüm, hafif üst harmonik
    private static AudioClip BuildNote()
    {
        const int rate = 44100;
        const float seconds = 1.8f, f = 1318.5f;
        int n = Mathf.RoundToInt(seconds * rate);
        float[] data = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)rate;
            float attack = Mathf.Clamp01(t / 0.004f);
            float env = attack * Mathf.Exp(-t * 3.2f);
            float s = Mathf.Sin(2f * Mathf.PI * f * t)
                    + Mathf.Sin(2f * Mathf.PI * f * 2f * t) * 0.18f * Mathf.Exp(-t * 6f)
                    + Mathf.Sin(2f * Mathf.PI * f * 3.01f * t) * 0.06f * Mathf.Exp(-t * 9f);
            data[i] = s * env * 0.5f;
        }
        AudioClip clip = AudioClip.Create("IzNotasi", n, 1, rate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
