using UnityEngine;

/// <summary>Koridor ampulünde ara sıra kısa titreme. Parlaklığı HallwayLightTimer yönetir;
/// her titremenin başında o anki değer okunur ve tam o değere geri dönülür.</summary>
[RequireComponent(typeof(Light))]
public class BulbFlicker : MonoBehaviour
{
    [SerializeField] private Vector2 interval = new Vector2(6f, 20f);
    [SerializeField] private float burstLength = 0.3f;
    [SerializeField] private bool buzz = true;
    [SerializeField] private float buzzVolume = 0.03f;

    private static AudioClip buzzClip;
    private Light lamp;
    private float timer, burstT = -1f, restore;
    private int dips;

    private void Awake()
    {
        lamp = GetComponent<Light>();
        timer = UnityEngine.Random.Range(interval.x, interval.y);
        if (!buzz) return;
        if (buzzClip == null) buzzClip = Buzz();
        var s = gameObject.AddComponent<AudioSource>();
        s.clip = buzzClip; s.loop = true; s.volume = buzzVolume;
        s.spatialBlend = 1f; s.minDistance = 0.3f; s.maxDistance = 4f;
        s.Play();
    }

    private void Update()
    {
        if (PauseService.IsPaused) return;
        if (!lamp.enabled || AudioListener.pause) { Abort(); return; }

        if (burstT < 0f)
        {
            timer -= Time.deltaTime;
            if (timer > 0f) return;
            restore = lamp.intensity;  // her seferinde taze okunur
            dips = UnityEngine.Random.Range(2, 6);
            burstT = 0f;
            return;
        }
        burstT += Time.deltaTime;
        if (burstT >= burstLength) { Abort(); return; }
        // burst içinde 'dips' adet kısa çukur
        float phase = burstT / burstLength * dips;
        float dip = Mathf.Repeat(phase, 1f) < 0.45f ? UnityEngine.Random.Range(0.25f, 0.6f) : 1f;
        lamp.intensity = restore * dip;
    }

    /// <summary>Titreme yarıda kalırsa ya da biterse eski parlaklığa döner.</summary>
    private void Abort()
    {
        if (burstT < 0f) return;
        lamp.intensity = restore;
        burstT = -1f;
        timer = UnityEngine.Random.Range(interval.x, interval.y);
    }

    private void OnDisable() => Abort();

    /// <summary>100 Hz elektrik vızıltısı, harmoniklerle. 1 sn, dikişsiz döner.</summary>
    private static AudioClip Buzz()
    {
        const int rate = 44100;
        var data = new float[rate];
        for (int i = 0; i < rate; i++)
        {
            float t = i / (float)rate;
            data[i] = 0.4f * Mathf.Sin(2 * Mathf.PI * 100f * t) + 0.15f * Mathf.Sin(2 * Mathf.PI * 200f * t)
                    + 0.08f * Mathf.Sign(Mathf.Sin(2 * Mathf.PI * 300f * t));
        }
        var c = AudioClip.Create("BulbBuzz", rate, 1, rate, false);
        c.SetData(data, 0);
        return c;
    }
}
