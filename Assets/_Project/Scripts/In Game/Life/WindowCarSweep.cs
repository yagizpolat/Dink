using UnityEngine;

/// <summary>Dışarıdan geçen araba: pencerenin hemen dışındaki spot ışığı tavanı ve duvarları
/// süpürür, uzaktan boğuk bir geçiş sesi duyulur.</summary>
public class WindowCarSweep : MonoBehaviour
{
    [SerializeField] private Light spot;                     // pencere dışında, içeri bakan Spot Light
    [SerializeField] private float intensity = 3f;
    [SerializeField] private Vector3 fromEuler = new Vector3(-20f, -50f, 0f);  // yerel açı (baş)
    [SerializeField] private Vector3 toEuler = new Vector3(-20f, 50f, 0f);     // yerel açı (son)
    [SerializeField] private float duration = 2.5f;
    [SerializeField] private Vector2 interval = new Vector2(20f, 60f);
    [SerializeField] private float volume = 0.25f;

    private static AudioClip passClip;
    private AudioSource source;
    private float timer, t = -1f;

    private void Awake()
    {
        if (spot == null) spot = GetComponentInChildren<Light>();
        spot.enabled = false;
        if (passClip == null) passClip = CarPass();
        source = gameObject.AddComponent<AudioSource>();
        source.spatialBlend = 1f; source.minDistance = 2f; source.maxDistance = 25f;
        gameObject.AddComponent<AudioLowPassFilter>().cutoffFrequency = 1200f;
        timer = UnityEngine.Random.Range(interval.x, interval.y);
    }

    private void Update()
    {
        if (PauseService.IsPaused) return;
        if (AudioListener.pause) { spot.enabled = false; return; }

        if (t < 0f)
        {
            timer -= Time.deltaTime;
            if (timer > 0f) return;
            t = 0f;
            bool reverse = UnityEngine.Random.value < 0.5f;  // araba iki yönden de gelebilir
            if (reverse) { var tmp = fromEuler; fromEuler = toEuler; toEuler = tmp; }
            source.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
            source.PlayOneShot(passClip, volume);
        }

        t += Time.deltaTime / duration;
        if (t >= 1f)
        {
            t = -1f; spot.enabled = false;
            timer = UnityEngine.Random.Range(interval.x, interval.y);
            return;
        }
        float s = Mathf.SmoothStep(0f, 1f, t);
        spot.transform.localRotation = Quaternion.Slerp(Quaternion.Euler(fromEuler), Quaternion.Euler(toEuler), s);
        spot.intensity = intensity * Mathf.Sin(Mathf.PI * t);  // girer, çıkar
        spot.enabled = true;
    }

    /// <summary>Uzaktan araba geçişi: alçak geçirgen gürültü, yaklaşırken artan, uzaklaşırken
    /// düşen "doppler" kayması. 3.5 sn.</summary>
    private static AudioClip CarPass()
    {
        const int rate = 44100; int n = (int)(rate * 3.5f);
        var data = new float[n]; var rng = new System.Random(5); float y = 0f, ph = 0f;
        for (int i = 0; i < n; i++)
        {
            float u = i / (float)n;
            float env = Mathf.Exp(-Mathf.Pow((u - 0.5f) * 3.2f, 2f));     // yaklaş - geç - uzaklaş
            float k = Mathf.Lerp(0.06f, 0.025f, u);                         // gürültü rengi kayar
            y += k * ((float)rng.NextDouble() * 2f - 1f - y);
            ph += 2 * Mathf.PI * Mathf.Lerp(95f, 70f, Mathf.SmoothStep(0f, 1f, u)) / rate;  // motor tonu düşer
            data[i] = env * (3f * y + 0.15f * Mathf.Sin(ph));
        }
        var c = AudioClip.Create("CarPass", n, 1, rate, false);
        c.SetData(data, 0);
        return c;
    }
}
