using System.Collections;
using UnityEngine;

/// <summary>
/// Apartman asansörü. Kalkış katında: çağırma butonu (imzadan sonra) kapıyı açar, oyuncu kabine girip
/// kat butonuna basınca kapı kapanır, motor uğuldar, kabin sarsılır, kapı camından kuyu ışıkları aşağı akar
/// ve hedef kat yüklenir. Varış katında: oyuncu kabinde başlar, zil çalar, kapı açılır; katın işi bitince
/// (<see cref="DepartureUnlocked"/>) kabindeki bir sonraki kat butonu aynı yolculukla yukarı götürür.
/// </summary>
public class ElevatorController : MonoBehaviour
{
    private enum State { Closed, Moving, Open, Riding }

    [SerializeField] private bool arrival;
    [SerializeField] private string targetScene = "Kat1";

    [Header("Kapı")]
    [SerializeField] private Transform door;
    [SerializeField] private float openAngle = 95f;
    [SerializeField] private float doorSeconds = 1.3f;

    [Header("Butonlar ve Kabin")]
    [SerializeField] private Collider callButton;
    [SerializeField] private Collider floorButton;
    [SerializeField] private BoxCollider cabArea;
    [SerializeField] private float reach = 1.8f;

    [Header("Yolculuk")]
    [SerializeField] private float rideSeconds = 5.5f;
    [SerializeField] private Transform cabRig;
    [SerializeField] private float rideHeight = 3.2f;
    [SerializeField] private float arrivalRise = 0.9f;
    [SerializeField] private float arrivalSeconds = 1.8f;
    [SerializeField] private GameObject shaftCover;
    [SerializeField] private Transform shaftLight;
    [SerializeField] private Light cabLight;

    [Header("Gösterge")]
    [SerializeField] private TMPro.TMP_Text indicator;
    [SerializeField] private string fromFloor = "0";
    [SerializeField] private string toFloor = "1";

    [Header("Varış katından kalkış")]
    [SerializeField] private Collider departButton;
    [SerializeField] private string departScene;
    [SerializeField] private string departFloor;
    [Tooltip("Varış katında oyuncu kabinden çıkınca kapı kapanır ve asansör aşağı gider. Kapalıyken kabin katta, kapısı açık ve ışığı yanık bekler")]
    [UnityEngine.Serialization.FormerlySerializedAs("leaveAfterExit")]
    [SerializeField] private bool departAfterArrival;

    [Header("Ses")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioSource humSource;
    [SerializeField] private AudioClip openClip, closeClip, dingClip, deniedClip;

    /// <summary>Varış katındaki görev bitince kat betiği açar; her sahnede kapalı başlar.</summary>
    public static bool DepartureUnlocked { get; set; }

    private State state = State.Closed;
    private Quaternion closedRot, openRot;
    private Camera cam;
    private float nextDenied;

    private void Awake()
    {
        DepartureUnlocked = false;
        closedRot = door.rotation;
        openRot = OutwardRotation();
        if (shaftCover != null) shaftCover.SetActive(false);
        // Kat5'te asansör kovalamacanın parçası: sahne ayarı ne olursa olsun gider
        if (arrival && FindAnyObjectByType<Kat5Kovalamaca>() != null) departAfterArrival = true;

        // Paketlerde asansör sesi yok: motor uğultusu ve zil burada üretilir
        if (dingClip == null) dingClip = Synth("AsansorZil", 1.6f, (t, i) => Mathf.Sin(2f * Mathf.PI * 880f * t) * Mathf.Exp(-t * 3f) * 0.5f
                                                                      + Mathf.Sin(2f * Mathf.PI * 1318f * t) * Mathf.Exp(-t * 4f) * 0.25f);
        if (humSource != null && humSource.clip == null)
        {
            var rng = new System.Random(7);
            humSource.clip = Synth("AsansorMotor", 2f, (t, i) => Mathf.Sin(2f * Mathf.PI * 50f * t) * 0.35f
                                                                + Mathf.Sin(2f * Mathf.PI * 100f * t) * 0.18f
                                                                + ((float)rng.NextDouble() - 0.5f) * 0.06f);
            humSource.loop = true;
        }
    }

    private static AudioClip Synth(string name, float seconds, System.Func<float, int, float> f)
    {
        const int rate = 44100;
        int n = Mathf.RoundToInt(seconds * rate);
        float[] data = new float[n];
        for (int i = 0; i < n; i++) data[i] = f(i / (float)rate, i);
        AudioClip clip = AudioClip.Create(name, n, 1, rate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private IEnumerator Start()
    {
        SetIndicator(arrival ? toFloor : fromFloor);
        if (!arrival) yield break;

        // Varış: kabin katın biraz altından başlar, son metreleri yavaşlayarak çıkar ve durur
        PlayerControls.SetEnabled(false);
        if (shaftCover != null) shaftCover.SetActive(true);
        if (humSource != null) { humSource.volume = 0.5f; humSource.Play(); }

        Transform player = FindAnyObjectByType<PlayerWalker>()?.transform;
        CharacterController cc = player != null ? player.GetComponent<CharacterController>() : null;
        if (cc != null) cc.enabled = false;
        if (cabRig != null)
        {
            Vector3 down = Vector3.down * arrivalRise;
            cabRig.position += down;
            if (player != null) player.position += down;
            float travelled = 0f;
            for (float t = 0f; t < arrivalSeconds; t += Time.deltaTime)
            {
                // Hızlı gelir, sona doğru yavaşlar
                float s = 1f - Mathf.Pow(1f - Mathf.Clamp01(t / arrivalSeconds), 3f);
                float step = s * arrivalRise - travelled;
                travelled += step;
                MoveCab(step, player);
                if (humSource != null) humSource.volume = Mathf.Lerp(0.5f, 0.05f, s);
                yield return null;
            }
            MoveCab(arrivalRise - travelled, player);
        }
        else
        {
            yield return new WaitForSeconds(1.2f);
        }
        // Durunca kısa bir oturma sarsıntısı
        yield return Settle();
        if (humSource != null) humSource.Stop();
        if (cc != null) cc.enabled = true;
        Play(dingClip);
        if (shaftCover != null) shaftCover.SetActive(false);
        yield return new WaitForSeconds(0.6f);
        PlayerControls.SetEnabled(true);
        yield return MoveDoor(true);
    }

    // Kapı holün tarafına (bu nesnenin arkasına) doğru açılır
    private Quaternion OutwardRotation()
    {
        Vector3 center = door.GetComponentInChildren<Renderer>().bounds.center;
        float best = openAngle, bestScore = float.MaxValue;
        foreach (float a in new[] { openAngle, -openAngle })
        {
            Vector3 moved = door.position + Quaternion.AngleAxis(a, Vector3.up) * (center - door.position);
            float score = Vector3.Dot(moved - transform.position, transform.forward);
            if (score < bestScore) { bestScore = score; best = a; }
        }
        return Quaternion.AngleAxis(best, Vector3.up) * closedRot;
    }

    private void Update()
    {
        if (Time.timeScale == 0f) return;
        if (arrival && departAfterArrival && state == State.Open && !PlayerInCab()) { StartCoroutine(Leave()); return; }
        if (!Input.GetMouseButtonDown(0) && !Input.GetKeyDown(KeyCode.E)) return;

        if (cam == null) cam = Camera.main;
        if (cam == null) return;
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (arrival)
        {
            if (state != State.Open || departButton == null || !departButton.Raycast(ray, out _, reach) || !PlayerInCab()) return;
            // Sıradaki kat henüz yoksa (Kat4 yapılmadan) asansör kalkmaz; yoksa oyuncu siyah ekranda kalırdı
            if (!DepartureUnlocked || !Application.CanStreamedLevelBeLoaded(departScene))
            {
                if (Time.time >= nextDenied) { Play(deniedClip); nextDenied = Time.time + 0.6f; }
                return;
            }
            fromFloor = toFloor;
            toFloor = departFloor;
            targetScene = departScene;
            StartCoroutine(Ride());
            return;
        }

        if (state == State.Closed && callButton != null && callButton.Raycast(ray, out _, reach))
        {
            if (!LobbyLogbook.Signed)
            {
                // İmza yok: buton tık eder, asansör gelmez
                if (Time.time >= nextDenied) { Play(deniedClip); nextDenied = Time.time + 0.6f; }
                return;
            }
            StartCoroutine(MoveDoor(true));
        }
        else if (state == State.Open && floorButton != null && floorButton.Raycast(ray, out _, reach) && PlayerInCab())
        {
            StartCoroutine(Ride());
        }
    }

    private bool PlayerInCab()
    {
        PlayerWalker walker = FindAnyObjectByType<PlayerWalker>();
        return walker != null && cabArea != null && cabArea.bounds.Contains(walker.transform.position + Vector3.up * 0.5f);
    }

    private IEnumerator MoveDoor(bool open)
    {
        state = State.Moving;
        Play(open ? openClip : closeClip);
        Quaternion from = door.rotation, to = open ? openRot : closedRot;
        for (float t = 0f; t < doorSeconds; t += Time.deltaTime)
        {
            door.rotation = Quaternion.Slerp(from, to, Mathf.SmoothStep(0f, 1f, t / doorSeconds));
            yield return null;
        }
        door.rotation = to;
        state = open ? State.Open : State.Closed;
    }

    private IEnumerator Ride()
    {
        PlayerControls.SetEnabled(false);
        if (cabArea != null) cabArea.enabled = false;
        StartCoroutine(TurnToDoor());
        yield return MoveDoor(false);
        state = State.Riding;
        yield return new WaitForSeconds(0.5f);

        if (shaftCover != null) shaftCover.SetActive(true);
        if (humSource != null) { humSource.volume = 0.5f; humSource.Play(); }

        Transform camT = Camera.main.transform;
        Vector3 rest = camT.localPosition;
        float baseIntensity = cabLight != null ? cabLight.intensity : 0f;
        Transform player = FindAnyObjectByType<PlayerWalker>()?.transform;
        CharacterController cc = player != null ? player.GetComponent<CharacterController>() : null;
        if (cc != null) cc.enabled = false;

        float travelled = 0f;
        for (float t = 0f; t < rideSeconds; t += Time.deltaTime)
        {
            // Kabin gerçekten yükselir: yavaş kalkar, kat arasında hızlanır, yukarı yaklaşırken yavaşlar.
            // Açık kapı aralığından giriş katının kapısı aşağı kayar, yerini kuyu duvarı alır.
            float s = Mathf.SmoothStep(0f, 1f, Mathf.SmoothStep(0f, 1f, t / rideSeconds));
            float step = s * rideHeight - travelled;
            travelled += step;
            MoveCab(step, player);

            // Kalkışta kısa bir çöküş (yukarı ivme), hızlanınca kablo titreşimi
            float speed = Mathf.Sin(Mathf.Clamp01(t / rideSeconds) * Mathf.PI);
            float lift = t < 0.6f ? -Mathf.Sin(t / 0.6f * Mathf.PI) * 0.035f : 0f;
            float shake = 0.002f + 0.004f * speed;
            camT.localPosition = rest + new Vector3((Mathf.PerlinNoise(t * 9f, 0f) - 0.5f) * shake,
                                                    lift + (Mathf.PerlinNoise(0f, t * 11f) - 0.5f) * shake, 0f);
            if (humSource != null) { humSource.volume = 0.35f + 0.3f * speed; humSource.pitch = 0.9f + 0.2f * speed; }

            // Lamba bir an söner; o anda gösterge olmaması gereken bir katı gösterir
            bool glitch = t > 2.4f && t < 2.55f;
            if (cabLight != null) cabLight.intensity = glitch ? baseIntensity * 0.35f : baseIntensity;
            SetIndicator(glitch ? "11" : s < 0.6f ? fromFloor : toFloor);

            yield return null;
        }
        MoveCab(rideHeight - travelled, player);
        camT.localPosition = rest;

        yield return SceneDoorTrigger.FadeAndLoad(targetScene, 1.2f);
    }

    // Oyuncu indikten kısa süre sonra kapı kapanır, motor uğuldar, gösterge aşağı sayar: asansör gitti
    private IEnumerator Leave()
    {
        state = State.Riding;
        yield return new WaitForSeconds(1.2f);
        yield return MoveDoor(false);
        state = State.Riding;
        if (humSource != null) { humSource.volume = 0.4f; humSource.pitch = 0.9f; humSource.Play(); }
        int floor = int.TryParse(toFloor, out int f) ? f : 0;
        for (int i = floor - 1; i >= 0; i--)
        {
            yield return new WaitForSeconds(1.1f);
            SetIndicator(i.ToString());
            if (humSource != null) humSource.volume *= 0.7f;
        }
        if (humSource != null) humSource.Stop();
    }

    private void MoveCab(float dy, Transform player)
    {
        if (cabRig == null || dy == 0f) return;
        Vector3 d = Vector3.up * dy;
        cabRig.position += d;
        if (player != null) player.position += d;
    }

    // Kabin durunca halatlar bir an esner: kamera hafifçe aşağı çöküp toparlanır
    private IEnumerator Settle()
    {
        Transform camT = Camera.main.transform;
        Vector3 rest = camT.localPosition;
        for (float t = 0f; t < 0.5f; t += Time.deltaTime)
        {
            camT.localPosition = rest + Vector3.down * Mathf.Sin(t / 0.5f * Mathf.PI) * 0.025f * (1f - t / 0.5f);
            yield return null;
        }
        camT.localPosition = rest;
    }

    // Butona basan oyuncu nereye bakıyorsa baksın, yolculuk boyunca kapıya döner
    private IEnumerator TurnToDoor()
    {
        PlayerWalker walker = FindAnyObjectByType<PlayerWalker>();
        if (walker == null) yield break;
        Transform body = walker.transform, head = Camera.main.transform;

        Quaternion bodyFrom = body.rotation, bodyTo = Quaternion.LookRotation(-transform.forward, Vector3.up);
        Quaternion headFrom = head.localRotation;
        for (float t = 0f; t < 0.9f; t += Time.deltaTime)
        {
            float k = Mathf.SmoothStep(0f, 1f, t / 0.9f);
            body.rotation = Quaternion.Slerp(bodyFrom, bodyTo, k);
            head.localRotation = Quaternion.Slerp(headFrom, Quaternion.identity, k);
            yield return null;
        }
        body.rotation = bodyTo;
        head.localRotation = Quaternion.identity;
        walker.ResetPitch();
    }

    private void SetIndicator(string floor)
    {
        if (indicator != null && indicator.text != floor) indicator.text = floor;
    }

    private void Play(AudioClip clip)
    {
        if (audioSource != null && clip != null) audioSource.PlayOneShot(clip);
    }
}
