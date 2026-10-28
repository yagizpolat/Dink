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

    private void OnTriggerEnter(Collider other)
    {
        if (requireInteract || used || other.GetComponent<PlayerWalker>() == null) return;
        Use();
    }

    private void Update()
    {
        if (!requireInteract || used || Time.timeScale == 0f) return;
        if (!Input.GetMouseButtonDown(0) && !Input.GetKeyDown(KeyCode.E)) return;

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
        // Kapı hiç vurulmadıysa (iki mesafe de MaxValue) açılmamalı
        return doorDist < float.MaxValue && doorDist <= wallDist + 0.02f;
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

    /// <summary>Karartma sürerken true; Esc menüsü bu sırada açılmaz (yoksa yeni sahne duraklatılmış gelir).</summary>
    public static bool Transitioning { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ResetOnSceneLoad()
    {
        Transitioning = false;
        // Domain reload kapalıyken her Play'de abonelik birikmesin
        SceneManager.sceneLoaded -= ClearTransition;
        SceneManager.sceneLoaded += ClearTransition;
    }

    private static void ClearTransition(Scene scene, LoadSceneMode mode) => Transitioning = false;

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

        SceneManager.LoadScene(targetScene);
    }
}
