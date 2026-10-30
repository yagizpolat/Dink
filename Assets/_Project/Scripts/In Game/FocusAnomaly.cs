using UnityEngine;

/// <summary>
/// Koridor ışığı bir süre sonra kendiliğinden söner ve yanar; yanınca sıradaki nesne (yangın dolabı, petek…) yok olmuştur.
/// Nesne yokken ışık bir daha sönmez. F (ya da sağ tık) basılı tutulunca kamera odaklanır (FOV daralır); boş yere
/// odaklandıkça yer sarsılır, yeterince odaklanınca nesne geri gelir ve ışık döngüsü sürer. Hepsi gelince görev biter.
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
    [SerializeField] private float shakeDegrees = 0.9f;   // odaklanırken en büyük sarsıntı (derece)

    private Camera cam;
    private float normalFov;
    private float darkTime;
    private float held;
    private int index;       // sıradaki nesne
    private bool vanished;   // sıradaki nesne şu an yok mu
    private AudioSource source;
    private AudioSource rumbleSource;
    private float shake;       // anlık sarsıntı gücü 0..1
    private float kick;        // nesne gelince sönen sarsıntı darbesi
    private static AudioClip whoomp, rumble;

    private void Start()
    {
        cam = Camera.main;
        normalFov = cam.fieldOfView;
        source = gameObject.AddComponent<AudioSource>();
        source.ignoreListenerPause = true;  // karanlıkta geri getirilse de duyulsun
        if (whoomp == null) { whoomp = Whoomp(); rumble = Rumble(); }
        rumbleSource = gameObject.AddComponent<AudioSource>();
        rumbleSource.clip = rumble;
        rumbleSource.loop = true;
        rumbleSource.volume = 0f;
        rumbleSource.Play();
        if (finishLight != null) finishLight.enabled = false;
        timer.Press(null);
    }

    private void Update()
    {
        if (PauseService.IsPaused) return;

        // Görev bitti ya da bir nesne kayıp: ışık sürekli yanık (titreme başlamadan tazelenir)
        if (index >= targets.Length || vanished)
        {
            if (timer.Remaining < 5f) timer.Press(null);
        }
        // Otomatik ışık: sönük kaldıkça say, süre dolunca yeniden yak
        else if (!timer.IsLit)
        {
            darkTime += Time.deltaTime;
            if (darkTime >= darkSeconds)
            {
                darkTime = 0f;
                if (!vanished && index < targets.Length) Vanish();
                timer.Press(null);
            }
        }
        else darkTime = 0f;  // her karanlık baştan sayılır

        // Odaklanma
        bool focusing = Input.GetKey(KeyCode.F) || Input.GetMouseButton(1);
        cam.fieldOfView = Mathf.MoveTowards(cam.fieldOfView, focusing ? focusFov : normalFov, focusSpeed * Time.deltaTime);

        shake = 0f;
        if (!vanished) { rumbleSource.volume = 0f; return; }
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        bool looking = focusing && spots[index].Raycast(ray, out RaycastHit hit, 20f)
            && !Physics.Raycast(ray, hit.distance - 0.05f, ~0, QueryTriggerInteraction.Ignore);  // duvar arkasından sayılmaz
        held = looking ? held + Time.deltaTime : Mathf.Max(0f, held - 2f * Time.deltaTime);
        shake = held / holdSeconds;
        rumbleSource.volume = 0.7f * shake * shake;
        if (held >= holdSeconds) Restore();
    }

    // Sarsıntı: PlayerWalker başın dönüşünü her karede yeniden kurar; üstüne küçük bir sapma eklenir
    private void LateUpdate()
    {
        kick = Mathf.MoveTowards(kick, 0f, 1.6f * Time.deltaTime);
        float a = Mathf.Max(shake * shake, kick) * shakeDegrees;
        if (a <= 0f || cam == null) return;
        float t = Time.time * 23f;
        cam.transform.localRotation *= Quaternion.Euler((Mathf.PerlinNoise(t, 0.1f) - 0.5f) * 2f * a,
            (Mathf.PerlinNoise(0.7f, t) - 0.5f) * 2f * a, (Mathf.PerlinNoise(t, 0.9f) - 0.5f) * a);
    }

    /// <summary>Dışarıdan sarsıntı (ör. geçidi açan duvar gömülürken): 0..1.5.</summary>
    public void Shake(float amount) => kick = Mathf.Max(kick, amount);

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
            "何かが足りない。F を長押しして集中しろ。", "무언가 사라졌다. F를 길게 눌러 집중하라."), 1f);  // her kaybolmada yeniden
    }

    private void Restore()
    {
        vanished = false;
        held = 0f;
        targets[index].SetActive(true);
        source.PlayOneShot(whoomp);
        rumbleSource.volume = 0f;
        kick = 1.4f;
        index++;
        if (index < targets.Length)
        {
            timer.CutShort(6f);  // kısa bir sakin an, sonra sıradaki karanlık
            return;
        }

        if (finishLight != null) finishLight.enabled = true;
        onFinished.Invoke();
        ScreenHint.Show(DinkLang.T("Everything is back. A light is on at the end of the hall.", "Her şey yerinde. Koridorun sonunda bir ışık yanıyor.",
            "Alles ist zurück. Am Ende des Flurs brennt Licht.", "Tout est revenu. Une lumière brille au bout du couloir.",
            "Todo ha vuelto. Hay una luz al final del pasillo.", "Tudo voltou. Há uma luz no fim do corredor.",
            "Всё на месте. В конце коридора горит свет.", "一切都回来了。走廊尽头亮着一盏灯。", "全部戻った。廊下の奥に明かりが点いている。",
            "모두 돌아왔다. 복도 끝에 불빛이 켜져 있다."), 5f);
    }

    /// <summary>Yer sarsıntısı: çok alçak, dalgalanan gürültü (döngü).</summary>
    private static AudioClip Rumble()
    {
        const int rate = 44100;
        int n = rate * 2;
        var data = new float[n];
        var rng = new System.Random(4);
        float y = 0f, z = 0f;
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)rate;
            y += 0.01f * ((float)(rng.NextDouble() * 2 - 1) - y);
            z += 0.05f * (y - z);
            float swell = 0.7f + 0.3f * Mathf.Sin(2 * Mathf.PI * 1.5f * t);   // 2 sn içinde tam devir: döngü dikişsiz
            data[i] = 6f * z * swell + 0.15f * Mathf.Sin(2 * Mathf.PI * 38f * t);
        }
        var clip = AudioClip.Create("Rumble", n, 1, rate, false);
        clip.SetData(data, 0);
        return clip;
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
