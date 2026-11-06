using System.Collections;
using UnityEngine;

/// <summary>Hafıza marketi G1: raf dolunca sağ duvardaki depo kapısının ardında soğuk ışık yanar
/// (önce iki kısa tökezleme), alçak bir vızıltı başlar ve sağ kanat kendiliğinden bir parmak aralanır.
/// Kanat menteşe kökünde döner; açı işareti FBX ekseninden gelir (openAngle eksiyse depoya doğru).</summary>
public class DepoKapiSizinti : MonoBehaviour
{
    [SerializeField] private MarketShelfStocking stocking;
    [SerializeField] private Transform leaf;                 // DepoKapi_KanatSag (köken menteşede)
    [SerializeField] private Transform leafB;                // karşı kanat: itilince ters yöne açılır (tek kanat oyuncuya dar)
    [SerializeField] private Light backLight;              // kapı ardındaki floresan
    [SerializeField] private AudioSource hum;                // ardındaki vızıltı (döngü, 3D)
    [SerializeField] private float delay = 2.5f;
    [SerializeField] private float lightIntensity = 6f;
    [SerializeField] private float humVolume = 0.05f;
    [SerializeField] private float openAngle = -7f;
    [SerializeField] private float openSeconds = 3.5f;
    [SerializeField] private Light[] corridorLights;         // G2: koridor floresanları (backLight ile yanar)
    [SerializeField] private Transform player;
    [SerializeField] private Vector3 pushPoint = new Vector3(5.7f, 0f, 1.85f);  // kapının market yüzü
    [SerializeField] private float pushRadius = 0.9f;
    [SerializeField] private float pushAngle = -82f;
    [SerializeField] private AudioClip pushSound;            // itilince (ItmeKapi kapılarıyla aynı kayıt)

    private bool ajar, pushed;

    private Quaternion closed;

    private void Awake()
    {
        if (leaf != null) closed = leaf.localRotation;
        if (backLight != null) { backLight.intensity = 0f; backLight.enabled = false; }
        if (corridorLights != null) foreach (var l in corridorLights) if (l != null) l.enabled = false;
        if (hum != null) { hum.volume = 0f; hum.Stop(); }
    }

    private void OnEnable() { if (stocking != null) stocking.OnCompleted += Begin; }
    private void OnDisable() { if (stocking != null) stocking.OnCompleted -= Begin; }

    private void Begin() => StartCoroutine(Sequence());

    private IEnumerator Sequence()
    {
        yield return new WaitForSeconds(delay);

        if (backLight != null)
        {
            backLight.enabled = true;
            float[] flick = { 0.45f, 0f, 0.8f, 0f };              // floresan tutuşması
            float[] wait = { 0.06f, 0.18f, 0.05f, 0.35f };
            for (int i = 0; i < flick.Length; i++) { backLight.intensity = flick[i] * lightIntensity; yield return new WaitForSeconds(wait[i]); }
            backLight.intensity = lightIntensity;
        }
        if (corridorLights != null) foreach (var l in corridorLights) if (l != null) l.enabled = true;
        if (hum != null) { hum.Play(); StartCoroutine(Fade(hum, humVolume, 2f)); }

        yield return new WaitForSeconds(1.2f);
        if (leaf == null) yield break;
        var open = closed * Quaternion.Euler(0f, openAngle, 0f);
        for (float t = 0f; t < 1f; t += Time.deltaTime / openSeconds)
        {
            float e = 1f - Mathf.Pow(1f - t, 3f);                // yavaşlayarak durur, hava itmiş gibi
            leaf.localRotation = Quaternion.Slerp(closed, open, e);
            yield return null;
        }
        leaf.localRotation = open;
        ajar = true;
    }

    // G2: aralık kanada yürüyen oyuncu kapıyı iter; kanat koridora açılır ve öyle kalır
    private void Update()
    {
        if (!ajar || pushed || player == null || leaf == null) return;
        var d = player.position - pushPoint; d.y = 0f;
        if (d.magnitude > pushRadius) return;
        pushed = true;
        if (pushSound != null) AudioSource.PlayClipAtPoint(pushSound, leaf.position, 0.5f);
        foreach (var c in leaf.GetComponentsInChildren<Collider>()) c.enabled = false;
        StartCoroutine(Push(leaf, pushAngle));
        if (leafB != null)
        {
            foreach (var c in leafB.GetComponentsInChildren<Collider>()) c.enabled = false;
            StartCoroutine(Push(leafB, -pushAngle));
        }
    }

    private static IEnumerator Push(Transform kanat, float aci)
    {
        var from = kanat.localRotation; var to = from * Quaternion.Euler(0f, aci, 0f);
        for (float t = 0f; t < 1f; t += Time.deltaTime / 0.7f)
        {
            kanat.localRotation = Quaternion.Slerp(from, to, 1f - Mathf.Pow(1f - t, 2f));
            yield return null;
        }
        kanat.localRotation = to;
    }

    private static IEnumerator Fade(AudioSource s, float to, float sec)
    {
        float from = s.volume;
        for (float t = 0f; t < 1f; t += Time.deltaTime / sec) { s.volume = Mathf.Lerp(from, to, t); yield return null; }
        s.volume = to;
    }
}
