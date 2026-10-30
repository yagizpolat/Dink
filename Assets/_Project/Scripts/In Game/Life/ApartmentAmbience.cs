using UnityEngine;

/// <summary>Apartmanın "yaşayan" sesi: alçak bina uğultusu, camın ardından şehir ve
/// arada bir kapı ardından gelen hayat sesleri (kapı, üst kat adımları, boru tıkırtısı, öksürük).</summary>
public class ApartmentAmbience : MonoBehaviour
{
    [SerializeField] private float roomToneVolume = 0.035f;
    [SerializeField] private Transform cityWindow;
    [SerializeField] private AudioClip cityLoop;               // GeceSehir
    [SerializeField] private float cityVolume = 0.25f;
    [SerializeField] private float cityCutoff = 900f;          // cam ardından boğuk
    [SerializeField] private Transform[] lifePoints;           // kapı arkaları, tavan
    [SerializeField] private AudioClip[] doorCloses;           // Door_Close_1/2
    [SerializeField] private AudioClip footstep;               // KaroAdim
    [SerializeField] private Vector2 interval = new Vector2(8f, 25f);
    [SerializeField] private float lifeVolume = 0.3f;

    private static AudioClip roomTone, knock, murmur;
    private AudioSource[] pool;
    private int next;
    private float timer, stepTimer;
    private int stepsLeft;
    private AudioSource stepSource;
    private Vector3 stepDir;

    private void Awake()
    {
        if (roomTone == null) roomTone = RoomTone();
        if (knock == null) knock = PipeKnock();
        if (murmur == null) murmur = Murmur();

        var tone = gameObject.AddComponent<AudioSource>();
        tone.clip = roomTone; tone.loop = true; tone.spatialBlend = 0f; tone.volume = roomToneVolume;
        tone.Play();

        if (cityWindow != null && cityLoop != null)
        {
            var city = cityWindow.gameObject.AddComponent<AudioSource>();
            city.clip = cityLoop; city.loop = true; city.spatialBlend = 1f; city.volume = cityVolume;
            city.minDistance = 1.5f; city.maxDistance = 20f;
            cityWindow.gameObject.AddComponent<AudioLowPassFilter>().cutoffFrequency = cityCutoff;
            city.Play();
        }

        pool = new AudioSource[4];
        for (int i = 0; i < pool.Length; i++)
        {
            var go = new GameObject("LifeSource" + i);
            go.transform.SetParent(transform, false);
            var s = go.AddComponent<AudioSource>();
            s.spatialBlend = 1f; s.minDistance = 1f; s.maxDistance = 18f; s.playOnAwake = false;
            go.AddComponent<AudioLowPassFilter>().cutoffFrequency = 700f;
            pool[i] = s;
        }
        timer = UnityEngine.Random.Range(interval.x, interval.y);
    }

    private void Update()
    {
        if (PauseService.IsPaused || AudioListener.pause) return;
        UpdateSteps();
        timer -= Time.deltaTime;
        if (timer > 0f || lifePoints == null || lifePoints.Length == 0) return;
        timer = UnityEngine.Random.Range(interval.x, interval.y);
        PlayLife(lifePoints[UnityEngine.Random.Range(0, lifePoints.Length)]);
    }

    /// <summary>Rastgele bir hayat sesi seçip noktada çalar.</summary>
    private void PlayLife(Transform at)
    {
        if (at == null) return;
        AudioSource s = Take(at.position);
        int kind = UnityEngine.Random.Range(0, 4);
        if (kind == 0 && doorCloses != null && doorCloses.Length > 0)
            s.PlayOneShot(doorCloses[UnityEngine.Random.Range(0, doorCloses.Length)], lifeVolume * 0.6f);
        else if (kind == 1 && footstep != null && stepsLeft <= 0)
        {
            stepSource = s; stepsLeft = UnityEngine.Random.Range(3, 7); stepTimer = 0f;
            Vector2 d = UnityEngine.Random.insideUnitCircle.normalized * 0.6f;
            stepDir = new Vector3(d.x, 0f, d.y);
        }
        else if (kind == 3) s.PlayOneShot(murmur, lifeVolume * 0.35f);
        else s.PlayOneShot(knock, lifeVolume * 0.7f);
    }

    /// <summary>Üst kat adımları: ~0.55 sn arayla, her adımda biraz ilerler.</summary>
    private void UpdateSteps()
    {
        if (stepsLeft <= 0 || stepSource == null) return;
        stepTimer -= Time.deltaTime;
        if (stepTimer > 0f) return;
        stepTimer = 0.55f + UnityEngine.Random.Range(-0.05f, 0.05f);
        stepSource.transform.position += stepDir;
        stepSource.pitch = UnityEngine.Random.Range(0.9f, 1.05f);
        stepSource.PlayOneShot(footstep, lifeVolume * 0.5f);
        stepsLeft--;
    }

    private AudioSource Take(Vector3 pos)
    {
        AudioSource s = pool[next];
        next = (next + 1) % pool.Length;
        if (s == stepSource) { s = pool[next]; next = (next + 1) % pool.Length; }
        s.transform.position = pos;
        s.pitch = UnityEngine.Random.Range(0.92f, 1.06f);
        return s;
    }

    /// <summary>Döngüsel oda tonu: 50/100 Hz şebeke uğultusu + çok hafif hava gürültüsü.</summary>
    private static AudioClip RoomTone()
    {
        const int rate = 44100; int n = rate * 4;  // 4 sn: 50 Hz'in tam katı, dikişsiz döner
        var data = new float[n]; var rng = new System.Random(7); float y = 0f;
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)rate;
            float hum = 0.5f * Mathf.Sin(2 * Mathf.PI * 50f * t) + 0.25f * Mathf.Sin(2 * Mathf.PI * 100f * t);
            y += 0.02f * ((float)rng.NextDouble() * 2f - 1f - y);  // çok yumuşatılmış hava
            data[i] = 0.5f * hum + 2.5f * y;
        }
        return Make("RoomTone", data, rate);
    }

    /// <summary>Boru vuruşu / kalorifer tıkırtısı: 2-3 kısa, metalik, sönümlü darbe.</summary>
    private static AudioClip PipeKnock()
    {
        const int rate = 44100; int n = (int)(rate * 1.2f);
        var data = new float[n]; float[] hits = { 0f, 0.22f, 0.5f };
        foreach (float h in hits)
            for (int i = (int)(h * rate); i < n; i++)
            {
                float t = i / (float)rate - h;
                data[i] += 0.5f * Mathf.Exp(-t * 28f) * (Mathf.Sin(2 * Mathf.PI * 410f * t) + 0.5f * Mathf.Sin(2 * Mathf.PI * 1130f * t));
            }
        return Make("PipeKnock", data, rate);
    }

    /// <summary>Duvar ardında boğuk konuşma mırıltısı: hece hızında genliklenen sesli gürültü.</summary>
    private static AudioClip Murmur()
    {
        const int rate = 44100; int n = rate * 2;
        var data = new float[n]; var rng = new System.Random(3); float y = 0f;
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)rate;
            float f0 = 120f + 25f * Mathf.Sin(t * 3.1f);
            float voice = Mathf.Sin(2 * Mathf.PI * f0 * t) + 0.4f * ((float)rng.NextDouble() - 0.5f);
            y += 0.08f * (voice - y);
            float syll = Mathf.Max(0f, Mathf.Sin(t * 2 * Mathf.PI * 4.2f)) * Mathf.Sin(Mathf.PI * t / 2f);
            data[i] = 0.8f * y * syll;
        }
        return Make("Murmur", data, rate);
    }

    private static AudioClip Make(string name, float[] data, int rate)
    {
        var c = AudioClip.Create(name, data.Length, 1, rate, false);
        c.SetData(data, 0);
        return c;
    }
}
