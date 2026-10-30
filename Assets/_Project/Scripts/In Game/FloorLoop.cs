using System.Collections;
using UnityEngine;

/// <summary>
/// Kat döngüsü (Exit 8 tarzı): L kolunun ucundaki köşe geçidi iki kez sağa döner; lobinin batısında birebir aynı
/// bir geçit daha var. Görev bittiyse oyuncu giriş geçidinin iki dönemeci arasındaki çizgiyi geçtiği an, iki ağız da
/// görünmezken çıkış geçidinde aynı noktaya taşınır ve kat yenilenir (levha FLOOR n, nesneler yerinde, görev baştan).
/// Görev bitmeden geçidin ağzı düz duvardır (tapa); görev bitince tapa sarsıntıyla yere gömülür.
/// </summary>
public class FloorLoop : MonoBehaviour
{
    [SerializeField] private FocusAnomaly anomaly;
    [SerializeField] private Transform entryGate;         // giriş geçidi ağzı: ileri (z) = içeri, sağ (x) = dönüş yönü
    [SerializeField] private Transform exitGate;          // çıkış geçidinin aynı noktası
    [SerializeField] private float crossRight = 1.4f;     // iki dönemecin ortası (yerel x)
    [SerializeField] private float minForward = 2.15f;    // dönemeçler arası bölüm başlangıcı (yerel z)
    [SerializeField] private GameObject[] targets;        // kat yenilenince yerine dönen nesneler
    [SerializeField] private Light gateLight;             // görev bitince geçidin ağzında yanan ışık
    [SerializeField] private Transform gateWall;          // geçidin ağzını kapatan duvar tapası
    [SerializeField] private float sinkSeconds = 3.5f;
    [SerializeField] private Renderer koridor;            // kat levhasını taşıyan model
    [SerializeField] private Texture2D[] signs;           // FLOOR 1, FLOOR 2, ...
    [SerializeField] private string signMaterialPrefix = "M_K_KatLevha";

    private bool open;
    private int floorNumber = 1;
    private float lastRight = float.NegativeInfinity;
    private Transform player;
    private Vector3 wallRest;
    private AudioSource wallSource;
    private static AudioClip grind;
    private static readonly int BaseMap = Shader.PropertyToID("_BaseMap");

    private void Awake()
    {
        var walker = FindFirstObjectByType<PlayerWalker>();
        if (walker != null) player = walker.transform;
        if (gateLight != null) gateLight.enabled = false;
        if (gateWall != null)
        {
            wallRest = gateWall.position;
            wallSource = gateWall.gameObject.AddComponent<AudioSource>();
            wallSource.spatialBlend = 1f;
            wallSource.minDistance = 1.5f;
            wallSource.maxDistance = 25f;
            if (grind == null) grind = Grind();
        }
        SetSign(floorNumber);
    }

    /// <summary>Görev bitti (FocusAnomaly.onFinished): geçit bir sonraki kata açılır.</summary>
    public void Open()
    {
        open = true;
        if (gateWall != null) StartCoroutine(Sink());
        else if (gateLight != null) gateLight.enabled = true;
    }

    /// <summary>Tapa titreyerek yere gömülür; ardından ağızdaki ışık yanar.</summary>
    private IEnumerator Sink()
    {
        yield return new WaitForSeconds(0.8f);
        wallSource.PlayOneShot(grind);
        for (float t = 0f; t < sinkSeconds; t += Time.deltaTime)
        {
            float k = t / sinkSeconds;
            float down = 2.35f * k * k * (3f - 2f * k);
            float jitter = 0.006f * Mathf.Sin(t * 60f) * (1f - k);
            gateWall.position = wallRest + new Vector3(jitter, -down, 0f);
            if (anomaly != null && player != null)
                anomaly.Shake(0.6f * (1f - k) / Mathf.Max(1f, Vector3.Distance(player.position, wallRest) * 0.35f));
            yield return null;
        }
        gateWall.gameObject.SetActive(false);
        if (gateLight != null) gateLight.enabled = true;
    }

    private void LateUpdate()
    {
        if (player == null) return;
        Vector3 local = entryGate.InverseTransformPoint(player.position);
        bool between = local.z > minForward && local.z < minForward + 1.3f && local.x > -0.6f && local.x < 3.4f;
        float right = between ? local.x : float.NegativeInfinity;

        // Yalnız ileri yönde (sağa doğru) çizgiyi geçerken
        if (open && between && lastRight < crossRight && right >= crossRight) Pass(local);
        else lastRight = right;
    }

    private void Pass(Vector3 local)
    {
        var cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        Quaternion turn = exitGate.rotation * Quaternion.Inverse(entryGate.rotation);
        player.SetPositionAndRotation(exitGate.TransformPoint(local), turn * player.rotation);
        if (cc != null) cc.enabled = true;
        lastRight = float.NegativeInfinity;

        // Yeni kat: aynı koridor yerinde yenilenir (birikme yok)
        open = false;
        if (gateLight != null) gateLight.enabled = false;
        if (gateWall != null) { gateWall.position = wallRest; gateWall.gameObject.SetActive(true); }
        foreach (var t in targets) t.SetActive(true);
        floorNumber++;
        SetSign(floorNumber);
        anomaly.Restart(targets);
    }

    /// <summary>Taş sürtünmesi: alçak, pürüzlü gürültü; sonda tok bir oturma.</summary>
    private static AudioClip Grind()
    {
        const int rate = 44100;
        int n = Mathf.CeilToInt(4.6f * rate);
        var data = new float[n];
        var rng = new System.Random(12);
        float y = 0f;
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)rate;
            y += 0.06f * ((float)(rng.NextDouble() * 2 - 1) - y);
            float grit = (rng.NextDouble() < 0.002) ? (float)(rng.NextDouble() - 0.5) : 0f;
            float env = t < 4.2f ? Mathf.Min(1f, t * 3f) * (0.75f + 0.25f * Mathf.Sin(t * 7f)) : 0f;
            float thud = t > 4.2f ? Mathf.Sin(2 * Mathf.PI * 55f * t) * Mathf.Exp(-(t - 4.2f) * 12f) : 0f;
            data[i] = 2.5f * y * env + 0.6f * grit * env + 0.9f * thud;
        }
        var clip = AudioClip.Create("Grind", n, 1, rate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private void SetSign(int n)
    {
        if (signs == null || signs.Length == 0 || koridor == null) return;
        Texture2D tex = signs[Mathf.Clamp(n - 1, 0, signs.Length - 1)];
        foreach (var m in koridor.materials)
            if (m.name.StartsWith(signMaterialPrefix)) m.SetTexture(BaseMap, tex);
    }
}
