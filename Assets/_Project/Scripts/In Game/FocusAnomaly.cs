using UnityEngine;

/// <summary>
/// Koridor ışığı kendiliğinden söner ve yanar; ilk yanışta petek yok olmuştur.
/// F (ya da sağ tık) basılı tutulunca kamera odaklanır (FOV daralır); boş yere yeterince odaklanınca petek geri gelir.
/// </summary>
public class FocusAnomaly : MonoBehaviour
{
    [SerializeField] private HallwayLightTimer timer;
    [SerializeField] private GameObject target;      // kaybolan nesne (Petek)
    [SerializeField] private Collider spot;          // nesnenin yeri (PetekYeri); kapalı nesnenin collider'ı ışın yakalamaz
    [SerializeField] private float darkSeconds = 3f;
    [SerializeField] private float focusFov = 35f;
    [SerializeField] private float focusSpeed = 18f; // derece / sn
    [SerializeField] private float holdSeconds = 3f;

    private Camera cam;
    private float normalFov;
    private float darkTime;
    private float held;
    private bool vanished;
    private bool restored;
    private AudioSource source;

    private void Start()
    {
        cam = Camera.main;
        normalFov = cam.fieldOfView;
        source = gameObject.AddComponent<AudioSource>();
        timer.Press(null);
    }

    private void Update()
    {
        if (PauseService.IsPaused) return;

        // Otomatik ışık: sönük kaldıkça say, süre dolunca yeniden yak
        if (!timer.IsLit)
        {
            darkTime += Time.deltaTime;
            if (darkTime >= darkSeconds)
            {
                darkTime = 0f;
                if (!vanished) Vanish();
                timer.Press(null);
            }
        }

        // Odaklanma
        bool focusing = Input.GetKey(KeyCode.F) || Input.GetMouseButton(1);
        cam.fieldOfView = Mathf.MoveTowards(cam.fieldOfView, focusing ? focusFov : normalFov, focusSpeed * Time.deltaTime);

        if (!vanished || restored) return;
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        bool looking = focusing && spot.Raycast(ray, out _, 20f);
        held = looking ? held + Time.deltaTime : 0f;
        if (held >= holdSeconds) Restore();
    }

    private void Vanish()
    {
        vanished = true;
        target.SetActive(false);
        ScreenHint.Show(DinkLang.T("Something is missing. Hold F to focus.", "Bir şey eksik. Odaklanmak için F'yi basılı tut.",
            "Etwas fehlt. Halte F, um dich zu konzentrieren.", "Il manque quelque chose. Maintiens F pour te concentrer.",
            "Falta algo. Mantén F para concentrarte.", "Falta algo. Segure F para se concentrar.",
            "Чего-то не хватает. Удерживай F, чтобы сосредоточиться.", "少了什么。按住 F 集中注意力。",
            "何かが足りない。F を長押しして集中しろ。", "무언가 사라졌다. F를 길게 눌러 집중하라."), 999f);
    }

    private void Restore()
    {
        restored = true;
        target.SetActive(true);
        source.PlayOneShot(Whoomp());
    }

    /// <summary>Geri gelme sesi: alçalan derin bir vuruş.</summary>
    private static AudioClip Whoomp()
    {
        const int rate = 44100;
        int n = rate;
        var data = new float[n];
        float phase = 0f;
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)rate;
            phase += Mathf.Lerp(90f, 35f, t) / rate;
            data[i] = 0.8f * Mathf.Sin(2 * Mathf.PI * phase) * Mathf.Min(1f, t * 40f) * Mathf.Exp(-t * 4f);
        }
        var clip = AudioClip.Create("Whoomp", n, 1, rate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
