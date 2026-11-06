using System.Collections;
using UnityEngine;

/// <summary>Hafıza mutfağı M3: not okunduktan sonra pencere etkileşime açılır; E/tık ile oyuncu denizlikten
/// tırmanıp sokağa iner. Tek yönlüdür: içeriden tırmanılır, geri dönüş yoktur.</summary>
public class PencereGecis : MonoBehaviour
{
    [SerializeField] private MutfakNot note;
    [SerializeField] private Collider target;
    [SerializeField] private Transform player;
    [SerializeField] private Vector3 landing = new Vector3(26.05f, 0.14f, 10.5f);
    [SerializeField] private float sillHeight = 0.95f;
    [SerializeField] private float seconds = 1.5f;
    [SerializeField] private float reach = 1.7f;
    [SerializeField] private AudioClip climbSound;
    [SerializeField] private Light[] keepLights;              // inişte yalnız bunlar kalır: URP nesne başına 8 ışık sınırı sokağı karartıyordu

    private bool armed, used;
    private Camera cam;
    private InteractionTarget it;

    public bool Used => used;

    private void Awake()
    {
        if (target != null) it = target.GetComponent<InteractionTarget>();
        if (it != null) it.enabled = false;
    }

    private void OnEnable() { if (note != null) note.OnDone += Arm; }
    private void OnDisable() { if (note != null) note.OnDone -= Arm; }

    private void Arm()
    {
        armed = true;
        if (it != null) it.enabled = true;
    }

    private void Update()
    {
        if (!armed || used || Time.timeScale == 0f || PauseService.IsPaused) return;
        if (!Input.GetMouseButtonDown(0) && !Input.GetKeyDown(KeyCode.E)) return;
        if (cam == null) cam = Camera.main;
        if (cam == null) return;
        if (target.Raycast(cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f)), out _, reach)) StartCoroutine(Climb());
    }

    private IEnumerator Climb()
    {
        used = true;
        if (it != null) it.enabled = false;
        PlayerControls.SetEnabled(false);
        var cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        if (climbSound != null) AudioSource.PlayClipAtPoint(climbSound, target.bounds.center, 0.5f);

        Vector3 from = player.position;
        Vector3 sill = new Vector3(landing.x, sillHeight + 0.1f, target.bounds.center.z);
        Quaternion rot = player.rotation, face = Quaternion.LookRotation(Vector3.forward);
        for (float t = 0f; t < 1f; t += Time.deltaTime / seconds)
        {
            float e = t * t * (3f - 2f * t);
            Vector3 p = e < 0.5f ? Vector3.Lerp(from, sill, e * 2f) : Vector3.Lerp(sill, landing, (e - 0.5f) * 2f);
            player.SetPositionAndRotation(p, Quaternion.Slerp(rot, face, e));
            yield return null;
        }
        player.SetPositionAndRotation(landing, face);
        foreach (var l in FindObjectsByType<Light>(FindObjectsSortMode.None))
            if (l.type != LightType.Directional && System.Array.IndexOf(keepLights, l) < 0) l.enabled = false;
        if (cc != null) cc.enabled = true;
        Physics.SyncTransforms();
        PlayerControls.SetEnabled(true);
    }
}
