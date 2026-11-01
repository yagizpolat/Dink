using UnityEngine;

/// <summary>Karanlık boşluğun gece sesi: tek kayıt, dinleyicinin çevresine dağılmış birkaç 3D kaynaktan çalar.
/// Her kaynak kaydın başka bir yerinden başlar ve kendi hızında kabarıp söner; böylece ses bir yerden gelir,
/// dönünce yönü değişir. Halka dinleyicinin konumunu izler ama dönüşünü izlemez (boşluğun ucuna varılmaz).
/// İçerideyken (insideArea) dış ses camın ardındaymış gibi boğuklaşır.</summary>
public class VoidAmbience : MonoBehaviour
{
    [SerializeField] private AudioClip nightLoop;              // GeceUgultusu
    [SerializeField] private int sourceCount = 3;
    [SerializeField] private float ringRadius = 14f;
    [SerializeField] private float volume = 0.16f;
    [SerializeField] private Vector2 swellSeconds = new Vector2(18f, 40f);
    [SerializeField] private float outsideCutoff = 1100f;
    [SerializeField] private float insideCutoff = 400f;
    [SerializeField] private Collider insideArea;              // market içi

    private AudioSource[] sources;
    private AudioLowPassFilter[] filters;
    private float[] phase, speed;
    private Transform listener;
    private float cutoff;

    private void Start()
    {
        var l = FindFirstObjectByType<AudioListener>();
        listener = l != null ? l.transform : transform;
        cutoff = outsideCutoff;

        sources = new AudioSource[sourceCount];
        filters = new AudioLowPassFilter[sourceCount];
        phase = new float[sourceCount]; speed = new float[sourceCount];
        for (int i = 0; i < sourceCount; i++)
        {
            float a = (i + Random.Range(-0.3f, 0.3f)) / sourceCount * Mathf.PI * 2f;
            var go = new GameObject("GeceKaynagi" + i);
            go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(Mathf.Cos(a), Random.Range(0.5f, 3f) / ringRadius, Mathf.Sin(a)) * ringRadius;
            var s = go.AddComponent<AudioSource>();
            s.clip = nightLoop; s.loop = true; s.spatialBlend = 1f; s.dopplerLevel = 0f;
            s.rolloffMode = AudioRolloffMode.Linear; s.minDistance = ringRadius; s.maxDistance = ringRadius * 4f;
            s.spread = 60f; s.volume = 0f;
            s.time = Random.Range(0f, nightLoop.length);
            s.Play();
            filters[i] = go.AddComponent<AudioLowPassFilter>();
            sources[i] = s;
            phase[i] = Random.Range(0f, Mathf.PI * 2f);
            speed[i] = Mathf.PI * 2f / Random.Range(swellSeconds.x, swellSeconds.y);
        }
    }

    private void Update()
    {
        if (sources == null) return;
        transform.position = listener.position;

        bool inside = insideArea != null && insideArea.bounds.Contains(listener.position);
        cutoff = Mathf.Lerp(cutoff, inside ? insideCutoff : outsideCutoff, Time.deltaTime * 2f);

        for (int i = 0; i < sources.Length; i++)
        {
            phase[i] += speed[i] * Time.deltaTime;
            // Yavaş kabarma, dipte neredeyse sessizlik: uğultu hep var ama çoğu an zar zor duyulur
            float w = 0.5f + 0.5f * Mathf.Sin(phase[i]);
            sources[i].volume = volume * (0.05f + 0.95f * w * w);
            filters[i].cutoffFrequency = cutoff;
        }
    }
}
