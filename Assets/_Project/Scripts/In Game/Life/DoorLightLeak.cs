using UnityEngine;

/// <summary>Daire kapısının altından sızan ışık. TV: mavi-beyaz titreme ve boğuk TV sesi.
/// Lamba: sabit sıcak ışık; arada içeride biri geçer, ışık kayarak kararır, hafif adım duyulur.</summary>
public class DoorLightLeak : MonoBehaviour
{
    public enum Mode { TV, Lamp }

    [SerializeField] private Mode mode = Mode.TV;
    [SerializeField] private Light leak;                      // kapı dibindeki Point Light
    [SerializeField] private float intensity = 0.15f;
    [SerializeField] private Color tvColor = new Color(0.62f, 0.75f, 1f);
    [SerializeField] private Color lampColor = new Color(1f, 0.72f, 0.45f);  // ~2700K
    [SerializeField] private float tvVolume = 0.12f;
    [SerializeField] private AudioClip footstep;              // KaroAdim
    [SerializeField] private Vector2 walkInterval = new Vector2(15f, 40f);

    private static AudioClip tvClip;
    private AudioSource source;
    private float seed, cutLevel = 1f, cutTimer, walkTimer, walkT = -1f;
    private int stepsPlayed;
    private Vector3 basePos;

    private void Awake()
    {
        if (leak == null) leak = GetComponent<Light>();
        basePos = leak.transform.localPosition;
        leak.color = mode == Mode.TV ? tvColor : lampColor;
        seed = UnityEngine.Random.value * 100f;
        walkTimer = UnityEngine.Random.Range(walkInterval.x, walkInterval.y);

        source = gameObject.AddComponent<AudioSource>();
        source.spatialBlend = 1f; source.minDistance = 0.5f; source.maxDistance = 8f;
        gameObject.AddComponent<AudioLowPassFilter>().cutoffFrequency = 800f;
        if (mode == Mode.TV)
        {
            if (tvClip == null) tvClip = TvMurmur();
            source.clip = tvClip; source.loop = true; source.volume = tvVolume;
            source.time = UnityEngine.Random.Range(0f, tvClip.length * 0.9f);
            source.Play();
        }
    }

    private void Update()
    {
        leak.enabled = !AudioListener.pause;  // karanlık tam olmalı
        if (PauseService.IsPaused || AudioListener.pause) return;
        leak.intensity = intensity * (mode == Mode.TV ? TvLevel() : LampLevel());
    }

    /// <summary>Perlin titreme + ara sıra "sahne geçişi" basamağı.</summary>
    private float TvLevel()
    {
        cutTimer -= Time.deltaTime;
        if (cutTimer <= 0f)
        {
            cutTimer = UnityEngine.Random.Range(0.6f, 4f);
            cutLevel = UnityEngine.Random.Range(0.35f, 1.2f);
            leak.color = Color.Lerp(tvColor, Color.white, UnityEngine.Random.value * 0.5f);
        }
        float n = Mathf.PerlinNoise(seed, Time.time * 6f);
        return cutLevel * (0.7f + 0.5f * n);
    }

    /// <summary>Biri geçerken ışık önce bir uçtan kararır, sonra öbür uca kayar (~1.2 sn).</summary>
    private float LampLevel()
    {
        if (walkT < 0f)
        {
            walkTimer -= Time.deltaTime;
            if (walkTimer <= 0f) { walkT = 0f; stepsPlayed = 0; }
            leak.transform.localPosition = basePos;
            return 1f;
        }
        walkT += Time.deltaTime / 1.2f;
        if (footstep != null && stepsPlayed < 3 && walkT >= stepsPlayed * 0.35f)
        {
            source.pitch = UnityEngine.Random.Range(0.9f, 1.05f);
            source.PlayOneShot(footstep, 0.12f);
            stepsPlayed++;
        }
        if (walkT >= 1f)
        {
            walkT = -1f;
            walkTimer = UnityEngine.Random.Range(walkInterval.x, walkInterval.y);
            return 1f;
        }
        // gölge kapı boyunca kayar: ışık kaynağı ters yöne itilir
        leak.transform.localPosition = basePos + Vector3.right * Mathf.Lerp(0.3f, -0.3f, walkT);
        return 1f - 0.65f * Mathf.Sin(Mathf.PI * walkT);
    }

    /// <summary>Duvar ardında TV: konuşma bantlı gürültü, yavaş genlik dalgası. 6 sn döngü.</summary>
    private static AudioClip TvMurmur()
    {
        const int rate = 44100; int n = rate * 6;
        var data = new float[n]; var rng = new System.Random(11); float y = 0f, z = 0f;
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)rate;
            float f0 = 150f + 40f * Mathf.PerlinNoise(t * 1.5f, 0.3f);
            float src = Mathf.Sin(2 * Mathf.PI * f0 * t) + 0.6f * ((float)rng.NextDouble() - 0.5f);
            y += 0.12f * (src - y); z += 0.5f * (y - z);
            float env = 0.3f + 0.7f * Mathf.PerlinNoise(t * 5f, 1.7f);
            float edge = Mathf.Min(1f, Mathf.Min(t, 6f - t) * 10f);  // dikişte tık olmasın
            data[i] = 0.7f * (y - 0.5f * z) * env * edge;
        }
        var c = AudioClip.Create("TvMurmur", n, 1, rate, false);
        c.SetData(data, 0);
        return c;
    }
}
