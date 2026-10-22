using UnityEngine;

/// <summary>
/// Sabit kameraya elde tutulmuş gibi çok hafif nefes salınımı ve yavaş bir yaklaşma verir.
/// Ölçeksiz zamanla çalışır; başlangıç konumu ve yönü referans alınır.
/// </summary>
public class CameraBreath : MonoBehaviour
{
    [SerializeField] private float swayDegrees = 0.45f;
    [SerializeField] private float swaySpeed = 0.18f;
    [SerializeField] private float bobMeters = 0.006f;
    [SerializeField] private float pushMeters = 0.15f;
    [SerializeField] private float pushSeconds = 90f;

    private Vector3 basePos;
    private Quaternion baseRot;
    private float seed;

    private void Start()
    {
        basePos = transform.position;
        baseRot = transform.rotation;
        seed = Random.value * 100f;
    }

    private void LateUpdate()
    {
        float t = Time.unscaledTime * swaySpeed + seed;
        float yaw = (Mathf.PerlinNoise(t, 0.3f) - 0.5f) * 2f * swayDegrees;
        float pitch = (Mathf.PerlinNoise(0.7f, t) - 0.5f) * 2f * swayDegrees * 0.6f;
        float roll = (Mathf.PerlinNoise(t * 0.5f, 5.1f) - 0.5f) * 2f * swayDegrees * 0.4f;
        float bob = Mathf.Sin(Time.unscaledTime * 0.9f) * bobMeters;

        float push = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(Time.timeSinceLevelLoad / pushSeconds)) * pushMeters;
        transform.position = basePos + baseRot * new Vector3(0f, bob, push);
        transform.rotation = baseRot * Quaternion.Euler(pitch, yaw, roll);
    }
}
