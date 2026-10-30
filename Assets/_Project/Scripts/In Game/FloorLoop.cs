using System.Collections;
using UnityEngine;

/// <summary>
/// Canlı kat döngüsü: aynı yerde duran iki kat kopyasından biri açıktır. Görev bitince açık katın 6 numaralı kapısı
/// açılır; oyuncu kapının ardındaki karanlık girişe adım atınca açık kat kapanır, öteki kat kapısı açık halde
/// belirir (kararma yok). Oyuncu dönüp kapıdan çıkınca kapı arkasından kapanır; kat numarası artar, görev baştan başlar.
/// </summary>
public class FloorLoop : MonoBehaviour
{
    [System.Serializable]
    private class Floor
    {
        public GameObject root;
        public Transform hinge;          // 6 numaralı kapının menteşesi; +Y içeri açar
        public GameObject[] targets;     // FocusAnomaly'nin bu katta kaybettiği nesneler (sırayla)
        public Renderer koridor;         // kat levhasını taşıyan birleşik model
    }

    [SerializeField] private FocusAnomaly anomaly;
    [SerializeField] private Floor[] floors;
    [SerializeField] private Bounds passZone;     // kapının ardındaki giriş (dünya)
    [SerializeField] private Bounds exitZone;     // kapının önü, koridor tarafı (dünya)
    [SerializeField] private Texture2D[] signs;   // FLOOR 1, FLOOR 2, ...
    [SerializeField] private string signMaterialPrefix = "M_K_KatLevha";
    [SerializeField] private float openAngle = 80f;
    [SerializeField] private float openSeconds = 2.2f;

    private enum Phase { Closed, Open, Swapped }

    private Phase phase;
    private int current;
    private int floorNumber = 1;
    private Transform player;
    private AudioSource source;
    private static readonly int BaseMap = Shader.PropertyToID("_BaseMap");

    private void Awake()
    {
        source = gameObject.AddComponent<AudioSource>();
        source.spatialBlend = 1f;
        source.maxDistance = 10f;
        var walker = FindFirstObjectByType<PlayerWalker>();
        if (walker != null) player = walker.transform;
        for (int i = 0; i < floors.Length; i++) floors[i].root.SetActive(i == current);
        SetSign(floors[current], floorNumber);
    }

    /// <summary>Görev bitti (FocusAnomaly.onFinished): açık katın kapısı aralanır.</summary>
    public void Open()
    {
        if (phase != Phase.Closed) return;
        phase = Phase.Open;
        transform.position = floors[current].hinge.position;
        source.PlayOneShot(Latch());
        StartCoroutine(Swing(floors[current].hinge, 0f, openAngle, openSeconds, 0.4f));
    }

    private void Update()
    {
        if (player == null) return;
        if (phase == Phase.Open && passZone.Contains(player.position)) Swap();
        else if (phase == Phase.Swapped && exitZone.Contains(player.position))
        {
            phase = Phase.Closed;
            StartCoroutine(Swing(floors[current].hinge, openAngle, 0f, 1.4f, 0.8f));
        }
    }

    private void Swap()
    {
        Floor old = floors[current];
        current = (current + 1) % floors.Length;
        Floor next = floors[current];

        // Yeni kat, eskisinin bıraktığı halde: kapı açık; kaybolan nesneler yerinde
        next.hinge.localRotation = Quaternion.Euler(0f, openAngle, 0f);
        foreach (var t in next.targets) t.SetActive(true);
        next.root.SetActive(true);
        old.root.SetActive(false);
        old.hinge.localRotation = Quaternion.identity;

        floorNumber++;
        SetSign(next, floorNumber);
        anomaly.Restart(next.targets);
        phase = Phase.Swapped;
    }

    private void SetSign(Floor f, int n)
    {
        if (signs == null || signs.Length == 0 || f.koridor == null) return;
        Texture2D tex = signs[Mathf.Clamp(n - 1, 0, signs.Length - 1)];
        foreach (var m in f.koridor.materials)
            if (m.name.StartsWith(signMaterialPrefix)) m.SetTexture(BaseMap, tex);
    }

    private IEnumerator Swing(Transform hinge, float from, float to, float seconds, float delay)
    {
        yield return new WaitForSeconds(delay);
        for (float t = 0f; t < seconds; t += Time.deltaTime)
        {
            hinge.localRotation = Quaternion.Euler(0f, Mathf.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t / seconds)), 0f);
            yield return null;
        }
        hinge.localRotation = Quaternion.Euler(0f, to, 0f);
        if (to == 0f) source.PlayOneShot(Latch(), 0.6f);
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
}
