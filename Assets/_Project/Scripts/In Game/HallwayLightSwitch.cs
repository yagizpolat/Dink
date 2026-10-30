using UnityEngine;

/// <summary>Koridor duvarındaki turuncu ışık otomatiği düğmesi. Bakıp E / sol tık: HallwayLightTimer'ı yakar.</summary>
[RequireComponent(typeof(Collider))]
public class HallwayLightSwitch : MonoBehaviour
{
    [SerializeField] private HallwayLightTimer timer;
    [SerializeField] private float reach = 1.8f;

    private Collider col;
    private Camera cam;
    private AudioSource source;

    private void Awake()
    {
        col = GetComponent<Collider>();
        source = gameObject.AddComponent<AudioSource>();
        source.spatialBlend = 1f;
        source.minDistance = 0.5f;
        source.maxDistance = 6f;
        var target = GetComponent<InteractionTarget>() ?? gameObject.AddComponent<InteractionTarget>();
        target.Configure(InteractionTarget.Verb.Press, reach);
    }

    private void Update()
    {
        if (timer == null || PauseService.IsPaused || Time.timeScale == 0f) return;
        if (!Input.GetMouseButtonDown(0) && !Input.GetKeyDown(KeyCode.E)) return;

        if (cam == null) cam = Camera.main;
        if (cam == null) return;

        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (col.Raycast(ray, out _, reach)) timer.Press(source);
    }
}
