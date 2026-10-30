using System.Collections;
using UnityEngine;

/// <summary>
/// Kat döngüsü, hep ileri: görev bitince 6 numaralı kapı açılır; oyuncu ardındaki karanlık girişe girince kapı
/// arkasından kapanır. Zifiri karanlıkta oyuncu fark ettirmeden asansör kabinine taşınır, kat yeni kat olarak
/// kurulur (levha FLOOR n, kaybolan nesneler yerinde, görev baştan). Sonra önündeki asansör kapısı açılır.
/// </summary>
public class FloorLoop : MonoBehaviour
{
    [SerializeField] private FocusAnomaly anomaly;
    [SerializeField] private Transform doorHinge;         // 6 numara; +Y içeri açar
    [SerializeField] private Transform elevatorHinge;     // asansör kapısı; lobiye açılır
    [SerializeField] private GameObject[] targets;        // kat yenilenince yerine dönen nesneler
    [SerializeField] private Light doorLight;             // 6 numaranın altından sızan ışık
    [SerializeField] private Renderer koridor;            // kat levhasını taşıyan model
    [SerializeField] private Texture2D[] signs;           // FLOOR 1, FLOOR 2, ...
    [SerializeField] private string signMaterialPrefix = "M_K_KatLevha";
    [SerializeField] private Bounds passZone;             // kapının ardındaki girişin derini (dünya)
    [SerializeField] private Vector3 vestibuleCenter = new Vector3(-5.6f, 0f, 9.1f);  // giriş: içeri bakış -z
    [SerializeField] private Vector3 cabinCenter = new Vector3(0.95f, 0f, -0.8f);     // kabin: dışarı bakış +z
    [SerializeField] private float doorOpenAngle = 80f;
    [SerializeField] private float elevatorOpenAngle = -100f;

    private enum Phase { Closed, Open, Passing }

    private Phase phase;
    private int floorNumber = 1;
    private Transform player;
    private AudioSource source;
    private static readonly int BaseMap = Shader.PropertyToID("_BaseMap");

    private void Awake()
    {
        source = gameObject.AddComponent<AudioSource>();
        source.spatialBlend = 0f;
        var walker = FindFirstObjectByType<PlayerWalker>();
        if (walker != null) player = walker.transform;
        SetSign(floorNumber);
    }

    /// <summary>Görev bitti (FocusAnomaly.onFinished): kapı aralanır.</summary>
    public void Open()
    {
        if (phase != Phase.Closed) return;
        phase = Phase.Open;
        source.PlayOneShot(Latch(), 0.5f);
        StartCoroutine(Swing(doorHinge, 0f, doorOpenAngle, 2.2f, 0.4f));
    }

    private void Update()
    {
        if (phase == Phase.Open && player != null && passZone.Contains(player.position)) StartCoroutine(Pass());
    }

    private IEnumerator Pass()
    {
        phase = Phase.Passing;
        // Kapı arkadan kapanır, sızan ışık söner: zifiri karanlık
        yield return Swing(doorHinge, doorOpenAngle, 0f, 0.7f, 0f);
        source.PlayOneShot(Latch(), 0.8f);
        if (doorLight != null) doorLight.enabled = false;

        // Kabine taşı: girişe göre konum ve bakış 180° döner
        Vector3 rel = player.position - vestibuleCenter;
        var cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        player.SetPositionAndRotation(cabinCenter + new Vector3(-rel.x, rel.y, -rel.z), Quaternion.Euler(0f, player.eulerAngles.y + 180f, 0f));
        if (cc != null) cc.enabled = true;

        // Yeni kat
        foreach (var t in targets) t.SetActive(true);
        floorNumber++;
        SetSign(floorNumber);
        anomaly.Restart(targets);

        yield return new WaitForSeconds(0.9f);
        source.PlayOneShot(Chime(), 0.6f);
        yield return Swing(elevatorHinge, 0f, elevatorOpenAngle, 1.6f, 0.3f);
        phase = Phase.Closed;
        yield return new WaitForSeconds(6f);
        yield return Swing(elevatorHinge, elevatorOpenAngle, 0f, 1.6f, 0f);
    }

    private void SetSign(int n)
    {
        if (signs == null || signs.Length == 0 || koridor == null) return;
        Texture2D tex = signs[Mathf.Clamp(n - 1, 0, signs.Length - 1)];
        foreach (var m in koridor.materials)
            if (m.name.StartsWith(signMaterialPrefix)) m.SetTexture(BaseMap, tex);
    }

    private static IEnumerator Swing(Transform hinge, float from, float to, float seconds, float delay)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);
        for (float t = 0f; t < seconds; t += Time.deltaTime)
        {
            hinge.localRotation = Quaternion.Euler(0f, Mathf.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t / seconds)), 0f);
            yield return null;
        }
        hinge.localRotation = Quaternion.Euler(0f, to, 0f);
    }

    /// <summary>Kilit dili: iki kısa metal tık.</summary>
    private static AudioClip Latch()
    {
        const int rate = 44100;
        int n = Mathf.CeilToInt(0.25f * rate);
        var data = new float[n];
        var rng = new System.Random(9);
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)rate;
            float a = Mathf.Exp(-t * 90f), b = t > 0.12f ? Mathf.Exp(-(t - 0.12f) * 70f) : 0f;
            float noise = (float)(rng.NextDouble() * 2 - 1);
            data[i] = 0.7f * (a + 0.8f * b) * (0.6f * noise + 0.4f * Mathf.Sin(2 * Mathf.PI * 1400f * t));
        }
        var clip = AudioClip.Create("Latch", n, 1, rate, false);
        clip.SetData(data, 0);
        return clip;
    }

    /// <summary>Asansör varış sesi: tek, yumuşak çan.</summary>
    private static AudioClip Chime()
    {
        const int rate = 44100;
        int n = Mathf.CeilToInt(1.2f * rate);
        var data = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)rate;
            float env = Mathf.Min(1f, t * 300f) * Mathf.Exp(-t * 3.5f);
            data[i] = 0.5f * env * (Mathf.Sin(2 * Mathf.PI * 1046f * t) + 0.35f * Mathf.Sin(2 * Mathf.PI * 2093f * t) + 0.15f * Mathf.Sin(2 * Mathf.PI * 3140f * t));
        }
        var clip = AudioClip.Create("Chime", n, 1, rate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
