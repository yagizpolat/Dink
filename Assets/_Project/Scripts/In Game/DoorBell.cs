using UnityEngine;

/// <summary>Daire kapısının yanındaki zil düğmesi. Bakıp E / sol tık: içeride "ding-dong" çalar; kimse açmaz.</summary>
[RequireComponent(typeof(Collider))]
public class DoorBell : MonoBehaviour
{
    [SerializeField] private float reach = 1.8f;

    private static AudioClip clip;  // bütün ziller aynı sesi paylaşır
    private Collider col;
    private Camera cam;
    private AudioSource source;

    private void Awake()
    {
        col = GetComponent<Collider>();
        source = gameObject.AddComponent<AudioSource>();
        source.spatialBlend = 1f;
        source.minDistance = 0.6f;
        source.maxDistance = 10f;
        if (clip == null) clip = DingDong();
        var target = GetComponent<InteractionTarget>();
        if (target == null) target = gameObject.AddComponent<InteractionTarget>();
        target.Configure(InteractionTarget.Verb.Press, reach);
    }

    private void Update()
    {
        if (PauseService.IsPaused || Time.timeScale == 0f) return;
        if (!Input.GetMouseButtonDown(0) && !Input.GetKeyDown(KeyCode.E)) return;
        if (source.isPlaying) return;  // çalarken yeniden basılmaz

        if (cam == null) cam = Camera.main;
        if (cam == null) return;
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (col.Raycast(ray, out _, reach)) source.PlayOneShot(clip);
    }

    /// <summary>İki notalı sönümlü zil: kapının ardından duyulduğu için hafif boğuk.</summary>
    private static AudioClip DingDong()
    {
        const int rate = 44100;
        float[] notes = { 880f, 698f };
        int per = Mathf.CeilToInt(0.45f * rate);
        var data = new float[per * notes.Length];
        float y = 0f;
        for (int k = 0; k < notes.Length; k++)
            for (int i = 0; i < per; i++)
            {
                float t = i / (float)rate;
                float env = Mathf.Min(1f, t * 200f) * Mathf.Exp(-t * 5f);
                float s = Mathf.Sin(2 * Mathf.PI * notes[k] * t) + 0.3f * Mathf.Sin(4 * Mathf.PI * notes[k] * t);
                y += 0.35f * (s - y);  // alçak geçiren: duvarın ardından
                data[k * per + i] = 0.45f * env * y;
            }
        var c = AudioClip.Create("DingDong", data.Length, 1, rate, false);
        c.SetData(data, 0);
        return c;
    }
}
