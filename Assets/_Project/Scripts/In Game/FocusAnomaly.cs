using UnityEngine;

/// <summary>
/// Koridor ışığı kendiliğinden söner ve yanar; her yanışta sıradaki nesne (petek, yangın dolabı…) yok olmuştur.
/// F (ya da sağ tık) basılı tutulunca kamera odaklanır (FOV daralır); boş yere yeterince odaklanınca nesne geri gelir.
/// Hepsi geri gelince görev biter.
/// </summary>
public class FocusAnomaly : MonoBehaviour
{
    [SerializeField] private HallwayLightTimer timer;
    [SerializeField] private GameObject[] targets;   // sırayla kaybolan nesneler
    [SerializeField] private Collider[] spots;       // her nesnenin yeri; kapalı nesnenin collider'ı ışın yakalamaz
    [SerializeField] private Light finishLight;      // görev bitince yanar
    [SerializeField] private UnityEngine.Events.UnityEvent onFinished;  // ör. sıradaki kapı açılır
    [SerializeField] private float darkSeconds = 3f;
    [SerializeField] private float focusFov = 35f;
    [SerializeField] private float focusSpeed = 18f; // derece / sn
    [SerializeField] private float holdSeconds = 3f;

    private Camera cam;
    private float normalFov;
    private float darkTime;
    private float held;
    private int index;       // sıradaki nesne
    private bool vanished;   // sıradaki nesne şu an yok mu
    private AudioSource source;

    private void Start()
    {
        cam = Camera.main;
        normalFov = cam.fieldOfView;
        source = gameObject.AddComponent<AudioSource>();
        if (finishLight != null) finishLight.enabled = false;
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
                if (!vanished && index < targets.Length) Vanish();
                timer.Press(null);
            }
        }

        // Odaklanma
        bool focusing = Input.GetKey(KeyCode.F) || Input.GetMouseButton(1);
        cam.fieldOfView = Mathf.MoveTowards(cam.fieldOfView, focusing ? focusFov : normalFov, focusSpeed * Time.deltaTime);

        if (!vanished) return;
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        bool looking = focusing && spots[index].Raycast(ray, out _, 20f);
        held = looking ? held + Time.deltaTime : 0f;
        if (held >= holdSeconds) Restore();
    }

    /// <summary>Yeni kat: görev yeni nesnelerle baştan başlar (FloorLoop).</summary>
    public void Restart(GameObject[] newTargets)
    {
        targets = newTargets;
        index = 0;
        vanished = false;
        held = 0f;
        darkTime = 0f;
        if (finishLight != null) finishLight.enabled = false;
    }

    private void Vanish()
    {
        vanished = true;
        targets[index].SetActive(false);
        ScreenHint.Show(DinkLang.T("Something is missing. Hold F to focus.", "Bir şey eksik. Odaklanmak için F'yi basılı tut.",
            "Etwas fehlt. Halte F, um dich zu konzentrieren.", "Il manque quelque chose. Maintiens F pour te concentrer.",
            "Falta algo. Mantén F para concentrarte.", "Falta algo. Segure F para se concentrar.",
            "Чего-то не хватает. Удерживай F, чтобы сосредоточиться.", "少了什么。按住 F 集中注意力。",
            "何かが足りない。F を長押しして集中しろ。", "무언가 사라졌다. F를 길게 눌러 집중하라."), 999f);
    }

    private void Restore()
    {
        vanished = false;
        held = 0f;
        targets[index].SetActive(true);
        source.PlayOneShot(Whoomp());
        index++;
        if (index < targets.Length) return;

        if (finishLight != null) finishLight.enabled = true;
        onFinished.Invoke();
        ScreenHint.Show(DinkLang.T("Everything is back. A light is on at the end of the hall.", "Her şey yerinde. Koridorun sonunda bir ışık yanıyor.",
            "Alles ist zurück. Am Ende des Flurs brennt Licht.", "Tout est revenu. Une lumière brille au bout du couloir.",
            "Todo ha vuelto. Hay una luz al final del pasillo.", "Tudo voltou. Há uma luz no fim do corredor.",
            "Всё на месте. В конце коридора горит свет.", "一切都回来了。走廊尽头亮着一盏灯。", "全部戻った。廊下の奥に明かりが点いている。",
            "모두 돌아왔다. 복도 끝에 불빛이 켜져 있다."), 5f);
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
