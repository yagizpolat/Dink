using UnityEngine;

/// <summary>Havada süzülen toz zerrecikleri. Bounds içinde çalışma anında ParticleSystem kurar.</summary>
public class DustMotes : MonoBehaviour
{
    [SerializeField] private Bounds area = new Bounds(Vector3.zero, new Vector3(2f, 2.4f, 10f));  // yerel
    [SerializeField] private int count = 300;
    [SerializeField] private Vector2 size = new Vector2(0.004f, 0.01f);
    [SerializeField] private float alpha = 0.12f;
    [SerializeField] private float drift = 0.03f;   // m/sn

    private static Texture2D dot;
    private ParticleSystemRenderer rend;

    private void Awake()
    {
        var go = new GameObject("DustMotes");
        go.transform.SetParent(transform, false);
        go.transform.localPosition = area.center;
        var ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.loop = true;
        main.prewarm = true;
        main.duration = 10f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(8f, 16f);
        main.startSpeed = 0f;
        main.startSize = new ParticleSystem.MinMaxCurve(size.x, size.y);
        main.startColor = new Color(1f, 0.92f, 0.8f, alpha);
        main.maxParticles = count;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.rateOverTime = count / 12f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = area.size;

        // yavaş, kararsız süzülme
        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = drift;
        noise.frequency = 0.25f;
        noise.scrollSpeed = 0.05f;

        var vel = ps.velocityOverLifetime;
        vel.enabled = true;
        vel.space = ParticleSystemSimulationSpace.World;
        vel.x = new ParticleSystem.MinMaxCurve(-drift * 0.3f, drift * 0.3f);
        vel.y = new ParticleSystem.MinMaxCurve(-drift * 0.4f, drift * 0.15f);
        vel.z = new ParticleSystem.MinMaxCurve(-drift * 0.3f, drift * 0.3f);

        // doğarken/ölürken yumuşak belirme
        var col = ps.colorOverLifetime;
        col.enabled = true;
        var g = new Gradient();
        g.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                  new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.2f),
                          new GradientAlphaKey(1f, 0.8f), new GradientAlphaKey(0f, 1f) });
        col.color = g;

        rend = go.GetComponent<ParticleSystemRenderer>();
        rend.renderMode = ParticleSystemRenderMode.Billboard;
        rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        rend.receiveShadows = false;
        rend.material = MakeMaterial();
        ps.Play();
    }

    private void Update()
    {
        if (PauseService.IsPaused) return;
        rend.enabled = !AudioListener.pause;
    }

    /// <summary>URP Particles/Unlit, alfa harmanlı, yumuşak yuvarlak doku.</summary>
    private static Material MakeMaterial()
    {
        var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        var m = new Material(shader);
        if (dot == null) dot = SoftDot();
        m.SetTexture("_BaseMap", dot);
        m.SetFloat("_Surface", 1f);   // Transparent
        m.SetFloat("_Blend", 0f);     // Alpha
        m.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
        m.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        m.SetFloat("_ZWrite", 0f);
        m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        m.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        return m;
    }

    private static Texture2D SoftDot()
    {
        const int s = 32;
        var t = new Texture2D(s, s, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
        for (int y = 0; y < s; y++)
            for (int x = 0; x < s; x++)
            {
                float d = new Vector2(x - s / 2f + 0.5f, y - s / 2f + 0.5f).magnitude / (s / 2f);
                float a = Mathf.Clamp01(1f - d); a *= a;
                t.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        t.Apply();
        return t;
    }
}
