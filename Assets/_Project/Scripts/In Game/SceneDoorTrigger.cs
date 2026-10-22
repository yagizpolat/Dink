using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Kapıdan sahne geçişi (prolog → giriş katı). Hedef sahne Start'ta arka planda yüklenmeye başlar.
/// requireInteract açıkken oyuncu kapıya bakıp E / sol tık ile açar; kapalıyken tetikleyiciye girmek yeter.
/// Geçişte kapı (varsa) döner, ekran kısa sürede kararır ve önceden yüklenmiş sahne etkinleşir.
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class SceneDoorTrigger : MonoBehaviour
{
    [SerializeField] private string targetScene = "GirisKati";
    [SerializeField] private float fadeSeconds = 0.35f;

    [Header("Etkileşim")]
    [Tooltip("Açıkken kapı bakılıp E / sol tık ile açılır; kapalıyken tetikleyiciye girmek yeter")]
    [SerializeField] private bool requireInteract = true;
    [SerializeField] private float reach = 2.2f;

    [Header("Kapı kanadı (isteğe bağlı)")]
    [SerializeField] private Transform doorHinge;
    [SerializeField] private float doorAngle = -80f;
    [SerializeField] private float doorSeconds = 0.35f;

    private bool used;
    private BoxCollider col;
    private Camera cam;
    private Transform player;
    private readonly RaycastHit[] hits = new RaycastHit[16];

    private void Reset()
    {
        GetComponent<BoxCollider>().isTrigger = true;
    }

    private void Awake()
    {
        col = GetComponent<BoxCollider>();
        if (requireInteract)
        {
            var target = GetComponent<InteractionTarget>();
            if (target == null) target = gameObject.AddComponent<InteractionTarget>();
            target.Configure(InteractionTarget.Verb.Open, reach);
        }
    }

    private void Start()
    {
        Preload(targetScene);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (requireInteract || used || other.GetComponent<PlayerWalker>() == null) return;
        Use();
    }

    private void Update()
    {
        if (!requireInteract || used || Time.timeScale == 0f) return;
        if (!Input.GetMouseButtonDown(0) && !Input.GetKeyDown(KeyCode.E)) return;
        if (LobbyLogbook.Busy) return;

        if (cam == null) cam = Camera.main;
        if (cam == null) return;
        if (player == null) player = FindAnyObjectByType<PlayerWalker>()?.transform;

        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (LooksAtDoor(ray)) Use();
    }

    // Nişangâhla aynı kural: kapı kutusu, aradaki ilk katı yüzeyden önce (ya da hemen onunla) vurulmalı
    private bool LooksAtDoor(Ray ray)
    {
        int n = Physics.RaycastNonAlloc(ray, hits, reach, ~0, QueryTriggerInteraction.Collide);
        float doorDist = float.MaxValue, wallDist = float.MaxValue;
        for (int i = 0; i < n; i++)
        {
            Collider c = hits[i].collider;
            if (player != null && c.transform.IsChildOf(player)) continue;
            if (c == col) doorDist = hits[i].distance;
            else if (doorHinge != null && c.transform.IsChildOf(doorHinge)) continue;
            else if (!c.isTrigger) wallDist = Mathf.Min(wallDist, hits[i].distance);
        }
        return doorDist <= wallDist + 0.02f;
    }

    private void Use()
    {
        used = true;
        var target = GetComponent<InteractionTarget>();
        if (target != null) target.enabled = false;
        if (doorHinge != null) StartCoroutine(SwingDoor());
        StartCoroutine(FadeAndLoad(targetScene, fadeSeconds));
    }

    private IEnumerator SwingDoor()
    {
        Quaternion from = doorHinge.localRotation, to = from * Quaternion.Euler(0f, doorAngle, 0f);
        for (float t = 0f; t < doorSeconds; t += Time.unscaledDeltaTime)
        {
            doorHinge.localRotation = Quaternion.Slerp(from, to, Mathf.SmoothStep(0f, 1f, t / doorSeconds));
            yield return null;
        }
        doorHinge.localRotation = to;
    }

    private static readonly bool PreloadEnabled = false;
    private static AsyncOperation preloadOp;
    private static string preloadScene;

    /// <summary>
    /// Sahneyi arka planda yükler ama etkinleştirmez; FadeAndLoad aynı sahne için bunu kullanır.
    /// Bekleyen bir yükleme varken başka async yükleme ilerlemez; bu yüzden aynı anda tek ön yükleme tutulur.
    /// </summary>
    public static void Preload(string scene)
    {
        // Kapalı: bekleyen ön yükleme, menüye dönüşte hedef sahneyi bir anlığına açıyordu.
        // Sahneler küçük; kısa karartma + normal yükleme yeterince hızlı.
        if (!PreloadEnabled) return;
        if (string.IsNullOrEmpty(scene) || preloadOp != null) return;
        if (!Application.CanStreamedLevelBeLoaded(scene)) return;
        preloadOp = SceneManager.LoadSceneAsync(scene);
        if (preloadOp == null) return;
        preloadOp.allowSceneActivation = false;
        preloadScene = scene;
    }

    /// <summary>Karartma sürerken true; Esc menüsü bu sırada açılmaz (yoksa yeni sahne duraklatılmış gelir).</summary>
    public static bool Transitioning { get; private set; }

    /// <summary>
    /// Bekleyen ön yüklemeyi serbest bırakır. Etkinleşmesi engellenmiş bir yükleme varken
    /// SceneManager.LoadScene sıraya takılabilir; başka sahneye (ör. ana menü) geçmeden önce çağrılır.
    /// </summary>
    public static void ReleasePreload()
    {
        if (preloadOp == null) return;
        preloadOp.allowSceneActivation = true;
        preloadOp = null;
        preloadScene = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ResetOnSceneLoad()
    {
        Transitioning = false;
        preloadOp = null;
        preloadScene = null;
        SceneManager.sceneLoaded += (scene, mode) =>
        {
            Transitioning = false;
            preloadOp = null;
            preloadScene = null;
        };
    }

    /// <summary>Kontrolleri kapatır, ekranı karartır ve sahneyi yükler. Kat geçişleri de bunu kullanır.</summary>
    public static IEnumerator FadeAndLoad(string targetScene, float fadeSeconds)
    {
        Transitioning = true;
        PlayerControls.SetEnabled(false);

        var canvasGo = new GameObject("KapiGecisKarartma");
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;
        var image = new GameObject("Siyah").AddComponent<Image>();
        image.transform.SetParent(canvasGo.transform, false);
        image.rectTransform.anchorMin = Vector2.zero;
        image.rectTransform.anchorMax = Vector2.one;
        image.color = new Color(0f, 0f, 0f, 0f);

        for (float t = 0f; t < fadeSeconds; t += Time.unscaledDeltaTime)
        {
            image.color = new Color(0f, 0f, 0f, t / fadeSeconds);
            yield return null;
        }
        image.color = Color.black;

        if (preloadOp != null && preloadScene == targetScene)
        {
            AsyncOperation op = preloadOp;
            op.allowSceneActivation = true;
            while (!op.isDone) yield return null;
        }
        else if (preloadOp != null)
        {
            // Başka sahne ön yüklenmişti: etkinleşmesine izin verilmeden yeni yükleme ilerlemez
            Debug.LogWarning($"[SceneDoorTrigger] Ön yüklenen '{preloadScene}' yerine '{targetScene}' yükleniyor.");
            ReleasePreload();
            SceneManager.LoadScene(targetScene);
        }
        else
        {
            SceneManager.LoadScene(targetScene);
        }
    }
}
