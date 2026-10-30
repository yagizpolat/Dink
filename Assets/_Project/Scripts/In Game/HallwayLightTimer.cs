using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Apartman koridorunun merdiven otomatiği: kat karanlık başlar, duvardaki turuncu düğmelerden birine basınca
/// lambalar belli bir süre yanar, sonra röle tıkıyla söner. Süre bitmeden önce lambalar kısaca titrer (eski
/// otomatlar gibi uyarı vermez ama oyuncu düğmeye koşacak zamanı bulsun). Düğmeye yeniden basmak süreyi baştan başlatır.
/// </summary>
public class HallwayLightTimer : MonoBehaviour
{
    [SerializeField] private float duration = 30f;
    [SerializeField] private float warnSeconds = 4f;
    [SerializeField] private Light[] lights;
    [SerializeField] private Renderer[] glowRenderers;         // lamba kürelerini taşıyan birleşik koridor modelleri (her kat)
    [SerializeField] private string glowMaterialPrefix = "M_K_Opal";
    [SerializeField] private Color darkAmbient = new Color(0.012f, 0.012f, 0.016f);
    [SerializeField] private Color litAmbient = new Color(0.12f, 0.1f, 0.08f);

    public bool IsLit => remaining > 0f;
    public float Remaining => remaining;

    private float remaining;
    private bool locked;
    private float[] baseIntensity;
    private readonly List<Material> glowMaterials = new List<Material>();
    private readonly List<Color> glowBase = new List<Color>();
    private AudioSource audioSource;
    private AudioClip clickOn, clickOff;
    private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

    private void Awake()
    {
        baseIntensity = new float[lights.Length];
        for (int i = 0; i < lights.Length; i++) baseIntensity[i] = lights[i].intensity;

        // renderer.materials örnek kopyalar döndürür: proje malzemeleri oyun sırasında değişmez
        foreach (var r in glowRenderers)
            if (r != null)
                foreach (var m in r.materials)
                    if (m.name.StartsWith(glowMaterialPrefix))
                    {
                        glowMaterials.Add(m);
                        glowBase.Add(m.GetColor(EmissionColor));
                    }

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0f;
        audioSource.volume = 0.55f;
        audioSource.ignoreListenerPause = true;
        clickOn = MakeClick(0.035f, 2400f, 0.9f);
        clickOff = MakeClick(0.06f, 900f, 1f);

        Apply(0f);
    }

    /// <summary>Görev bitti: lambalar söner ve düğmeler artık bir şey yakmaz.</summary>
    public void Shutdown()
    {
        locked = true;
        if (remaining > 0f) audioSource.PlayOneShot(clickOff);
        remaining = 0f;
        Apply(0f);
    }

    /// <summary>Düğmeye basıldı: lambalar yanar, süre baştan başlar.</summary>
    public void Press(AudioSource buttonSource)
    {
        if (buttonSource != null) buttonSource.PlayOneShot(clickOn);
        if (locked) return;  // tık sesi gelir ama hiçbir şey yanmaz
        if (remaining <= 0f) audioSource.PlayOneShot(clickOn, 0.5f);
        remaining = duration;
        Apply(1f);
    }

    private void Update()
    {
        if (remaining <= 0f) return;
        remaining -= Time.deltaTime;
        if (remaining <= 0f)
        {
            remaining = 0f;
            audioSource.PlayOneShot(clickOff);
            Apply(0f);
            return;
        }
        if (remaining < warnSeconds)
        {
            // Son saniyelerde ampul kararsızlaşır: kısa, düzensiz düşüşler
            float n = Mathf.PerlinNoise(Time.time * 9f, 0.37f);
            Apply(n < 0.32f ? 0.25f : 1f);
        }
    }

    private void Apply(float k)
    {
        for (int i = 0; i < lights.Length; i++)
        {
            lights[i].intensity = baseIntensity[i] * k;
            lights[i].enabled = k > 0f;
        }
        for (int i = 0; i < glowMaterials.Count; i++)
            glowMaterials[i].SetColor(EmissionColor, glowBase[i] * k);
        RenderSettings.ambientLight = Color.Lerp(darkAmbient, litAmbient, k);
        RenderSettings.reflectionIntensity = k;  // ortam yansıması açık kalırsa karanlıkta duvarlar mavimsi parlar
        // Karanlıkta bütün sesler durur, ışık gelince kaldığı yerden sürer (röle tıkı bundan etkilenmez)
        if (!PauseService.IsPaused) AudioListener.pause = k <= 0f;
    }

    private void OnDestroy()
    {
        AudioListener.pause = false;
        RenderSettings.reflectionIntensity = 1f;
    }

    /// <summary>Röle/düğme tıkı: kısa sönümlü gürültü + düşük tını.</summary>
    private static AudioClip MakeClick(float length, float tone, float gain)
    {
        const int rate = 44100;
        int n = Mathf.CeilToInt(length * rate);
        var data = new float[n];
        var rng = new System.Random(7);
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)rate;
            float env = Mathf.Exp(-t * 90f);
            float noise = (float)(rng.NextDouble() * 2 - 1);
            data[i] = gain * env * (0.6f * noise + 0.4f * Mathf.Sin(2 * Mathf.PI * tone * t));
        }
        var clip = AudioClip.Create("Click", n, 1, rate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
