using System.Collections;
using UnityEngine;

/// <summary>Hafıza marketi G1: raf dolunca sağ duvardaki depo kapısının ardında soğuk ışık yanar
/// (önce iki kısa tökezleme), alçak bir vızıltı başlar ve sağ kanat kendiliğinden bir parmak aralanır.
/// Kanat menteşe kökünde döner; açı işareti FBX ekseninden gelir (openAngle eksiyse depoya doğru).</summary>
public class DepoKapiSizinti : MonoBehaviour
{
    [SerializeField] private MarketShelfStocking stocking;
    [SerializeField] private Transform leaf;                 // DepoKapi_KanatSag (köken menteşede)
    [SerializeField] private Light backLight;                // kapı ardındaki floresan
    [SerializeField] private AudioSource hum;                // ardındaki vızıltı (döngü, 3D)
    [SerializeField] private float delay = 2.5f;
    [SerializeField] private float lightIntensity = 6f;
    [SerializeField] private float humVolume = 0.05f;
    [SerializeField] private float openAngle = -7f;
    [SerializeField] private float openSeconds = 3.5f;

    private Quaternion closed;

    private void Awake()
    {
        if (leaf != null) closed = leaf.localRotation;
        if (backLight != null) { backLight.intensity = 0f; backLight.enabled = false; }
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
    }

    private static IEnumerator Fade(AudioSource s, float to, float sec)
    {
        float from = s.volume;
        for (float t = 0f; t < 1f; t += Time.deltaTime / sec) { s.volume = Mathf.Lerp(from, to, t); yield return null; }
        s.volume = to;
    }
}
