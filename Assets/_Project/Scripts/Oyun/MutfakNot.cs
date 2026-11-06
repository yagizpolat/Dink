using System;
using System.Collections;
using UnityEngine;

/// <summary>Hafıza mutfağı M2: masadaki not okununca kısa bir sessizlik, sonra evin içinden tek bir fısıltı.
/// Lamba bir kez tökezler. Bittiğinde OnDone (sonraki parça buna bağlanır).</summary>
public class MutfakNot : MonoBehaviour
{
    [SerializeField] private SinifKagit note;
    [SerializeField] private AudioClip whisper;
    [SerializeField] private Light lamp;
    [SerializeField] private float delay = 2.2f;
    [SerializeField] private float volume = 0.7f;

    public event Action OnDone;

    private void OnEnable() { if (note != null) note.OnRead += Begin; }
    private void OnDisable() { if (note != null) note.OnRead -= Begin; }
    private void Begin() => StartCoroutine(Run());

    private IEnumerator Run()
    {
        yield return new WaitForSeconds(delay);
        if (whisper != null)
        {
            var g = new GameObject("MutfakFisilti");
            var s = g.AddComponent<AudioSource>();
            s.clip = whisper; s.volume = volume; s.spatialBlend = 0f; s.Play();
            Destroy(g, whisper.length + 0.2f);
            yield return new WaitForSeconds(whisper.length * 0.6f);
        }
        if (lamp != null)
        {
            float b = lamp.intensity;
            foreach (var k in new[] { 0.25f, 0.9f, 0.1f, 1f }) { lamp.intensity = b * k; yield return new WaitForSeconds(0.07f); }
            lamp.intensity = b;
        }
        OnDone?.Invoke();
    }
}
