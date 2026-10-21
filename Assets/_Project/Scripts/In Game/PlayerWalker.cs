using UnityEngine;

/// <summary>
/// Apartman bölümleri için yürüyen birinci şahıs karakter. Koşma yalnız kovalanılan katta var (Kat5Kovalamaca
/// SpeedMultiplier'ı açar); başka yerde gerilim hızdan değil, yaklaşmaktan gelir.
/// Sabit kameralı odalardaki Kamera.cs'nin yerini alır; PlayerControls.SetEnabled bunu da kapatıp açar.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerWalker : MonoBehaviour
{
    [SerializeField] private Transform head;

    [Header("Yürüme")]
    [SerializeField] private float walkSpeed = 1.7f;
    [SerializeField] private float acceleration = 8f;
    [SerializeField] private float gravity = -15f;

    [Header("Bakış")]
    [SerializeField] private float mouseSensitivity = 2.2f;
    [SerializeField] private float pitchLimit = 80f;

    [Header("Baş Sallanması")]
    [SerializeField] private float bobFrequency = 1.8f;
    [SerializeField] private float bobHeight = 0.035f;
    [SerializeField] private float bobSide = 0.02f;

    private CharacterController controller;
    private Vector3 velocity;
    private float pitch;
    private float bobTime;
    private Vector3 headRest;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (head == null && Camera.main != null) head = Camera.main.transform;
        if (head != null) headRest = head.localPosition;
    }

    private void OnEnable()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        InteractionReticle.Ensure(this);
    }

    private void Update()
    {
        // Mektup okurken veya ESC menüsünde zaman durur; karakter de durur
        if (Time.timeScale == 0f) return;

        Look();
        Move();
        Bob();
    }

    /// <summary>Yürüme hızı çarpanı; koşmaya izin veren kat betiği değiştirir.</summary>
    public float SpeedMultiplier { get; set; } = 1f;

    /// <summary>Kamera dışarıdan düz bakışa çevrildiğinde (ör. asansör) iç bakış açısını eşitler.</summary>
    public void ResetPitch() => pitch = 0f;

    private void Look()
    {
        float yaw = Input.GetAxis("Mouse X") * mouseSensitivity;
        pitch = Mathf.Clamp(pitch - Input.GetAxis("Mouse Y") * mouseSensitivity, -pitchLimit, pitchLimit);

        transform.Rotate(0f, yaw, 0f);
        if (head != null) head.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    private void Move()
    {
        Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
        Vector3 target = transform.TransformDirection(Vector3.ClampMagnitude(input, 1f)) * walkSpeed * SpeedMultiplier;

        Vector3 flat = Vector3.MoveTowards(new Vector3(velocity.x, 0f, velocity.z), target, acceleration * Time.deltaTime);
        velocity.x = flat.x;
        velocity.z = flat.z;

        velocity.y = controller.isGrounded ? -1f : velocity.y + gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void Bob()
    {
        if (head == null) return;

        float speed = new Vector2(velocity.x, velocity.z).magnitude;
        float amount = Mathf.Clamp01(speed / walkSpeed);
        // Koşarken adım sıklaşır
        float pace = Mathf.Max(1f, speed / walkSpeed);
        if (amount > 0.05f) bobTime += Time.deltaTime * bobFrequency * pace * Mathf.PI * 2f;

        Vector3 offset = new Vector3(Mathf.Cos(bobTime * 0.5f) * bobSide, Mathf.Sin(bobTime) * bobHeight, 0f) * amount;
        head.localPosition = Vector3.Lerp(head.localPosition, headRest + offset, 10f * Time.deltaTime);
    }
}
