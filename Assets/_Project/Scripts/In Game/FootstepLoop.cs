using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Adım sesi: kayıttaki tek tek adımları başlangıçta ayıklar, oyuncu her adım boyu yol aldığında
/// birini çalar. Böylece ses temposu yürüme hızına uyar. Oyuncu PlayerWalker ile bulunur.
/// (Adı eski; artık döngü çalmaz.)
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class FootstepLoop : MonoBehaviour
{
    [SerializeField] private float volume = 1f;
    [SerializeField] private float strideMeters = 1.0f;
    [SerializeField] private float pitchJitter = 0.06f;
    [SerializeField] private float onsetThreshold = 0.25f;
    [SerializeField] private float stepSeconds = 0.32f;

    private AudioSource source;
    private readonly List<AudioClip> steps = new List<AudioClip>();
    private Transform player;
    private Vector3 lastPos;
    private float travelled;
    private int lastIndex = -1;

    private void Awake()
    {
        source = GetComponent<AudioSource>();
        source.loop = false;
        source.playOnAwake = false;
        source.spatialBlend = 0f;
        if (source.clip != null) SliceSteps(source.clip);
    }

    // Zarfı eşiği aşan noktaları adım başlangıcı sayar; her adımı ayrı, normalize edilmiş klip yapar
    private void SliceSteps(AudioClip clip)
    {
        if (!clip.LoadAudioData()) return;
        int ch = clip.channels, rate = clip.frequency;
        var data = new float[clip.samples * ch];
        if (!clip.GetData(data, 0)) return;

        float peak = 0f;
        for (int i = 0; i < data.Length; i++) peak = Mathf.Max(peak, Mathf.Abs(data[i]));
        if (peak <= 0f) return;

        float threshold = peak * onsetThreshold;
        int len = Mathf.RoundToInt(stepSeconds * rate);
        int frames = clip.samples;
        int f = 0;
        while (f < frames - len && steps.Count < 24)
        {
            float v = 0f;
            for (int c = 0; c < ch; c++) v = Mathf.Max(v, Mathf.Abs(data[f * ch + c]));
            if (v < threshold) { f++; continue; }

            int start = Mathf.Max(0, f - Mathf.RoundToInt(0.01f * rate));
            var seg = new float[len * ch];
            float segPeak = 0f;
            for (int i = 0; i < seg.Length; i++) { seg[i] = data[start * ch + i]; segPeak = Mathf.Max(segPeak, Mathf.Abs(seg[i])); }
            float gain = segPeak > 0f ? 0.95f / segPeak : 1f;
            int fade = Mathf.RoundToInt(0.06f * rate);
            for (int i = 0; i < len; i++)
            {
                float k = i > len - fade ? (len - i) / (float)fade : 1f;
                for (int c = 0; c < ch; c++) seg[i * ch + c] *= gain * k;
            }
            var s = AudioClip.Create(clip.name + "_adim" + steps.Count, len, ch, rate, false);
            s.SetData(seg, 0);
            steps.Add(s);
            f = start + len;
        }
    }

    private void Update()
    {
        if (player == null)
        {
            var walker = FindFirstObjectByType<PlayerWalker>();
            if (walker == null) return;
            player = walker.transform;
            lastPos = player.position;
            return;
        }

        Vector3 delta = player.position - lastPos;
        delta.y = 0f;
        lastPos = player.position;
        if (Time.timeScale == 0f || steps.Count == 0) return;

        float d = delta.magnitude;
        if (d > 1f) { travelled = 0f; return; } // ışınlanma / sahne başı
        if (d < 0.0005f) { travelled = Mathf.Min(travelled, strideMeters * 0.5f); return; }

        travelled += d;
        if (travelled < strideMeters) return;
        travelled -= strideMeters;

        int i = Random.Range(0, steps.Count);
        if (steps.Count > 1 && i == lastIndex) i = (i + 1) % steps.Count;
        lastIndex = i;
        source.pitch = 1f + Random.Range(-pitchJitter, pitchJitter);
        source.PlayOneShot(steps[i], volume);
    }
}
