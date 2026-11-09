using System.Collections;
using UnityEngine;

/// <summary>Hafıza sokağı M5: apartman kapısının yanındaki zil paneli. E/tık ile zile basılır; kısa bir
/// elektrik vızıltısı (kapı otomatı) duyulur, ardından giriş kapısının kilidi açılır ve hol ışığı yanar.</summary>
public class ZilPaneli : MonoBehaviour
{
    [SerializeField] private Collider target;
    [SerializeField] private ItmeKapi door;
    [SerializeField] private AudioClip buzz;                  // gerçek kayıt, tiz çalınır: kapı otomatı vızıltısı
    [SerializeField] private float pitch = 1.8f;
    [SerializeField] private float seconds = 1.1f;
    [SerializeField] private float volume = 0.45f;
    [SerializeField] private float reach = 1.6f;

    private bool used;
    private Camera cam;

    private void Update()
    {
        if (used || Time.timeScale == 0f || PauseService.IsPaused) return;
        if (!Input.GetMouseButtonDown(0) && !Input.GetKeyDown(KeyCode.E)) return;
        if (cam == null) cam = Camera.main;
        if (cam == null) return;
        if (target.Raycast(cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f)), out _, reach)) StartCoroutine(Ring());
    }

    private IEnumerator Ring()
    {
        used = true;
        var it = target.GetComponent<InteractionTarget>();
        if (it != null) it.enabled = false;
        if (buzz != null)
        {
            var g = new GameObject("ZilVizilti");
            g.transform.position = target.bounds.center;
            var s = g.AddComponent<AudioSource>();
            s.clip = buzz; s.loop = true; s.pitch = pitch; s.volume = volume;
            s.spatialBlend = 1f; s.minDistance = 1f; s.maxDistance = 12f;
            s.Play();
            yield return new WaitForSeconds(seconds);
            Destroy(g);
        }
        if (door != null) door.Unlock();
    }
}
