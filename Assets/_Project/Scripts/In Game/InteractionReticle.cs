using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Yürünen bölümlerin nişangâhı. Boşta ekranın ortasında minik bir nokta; etkileşimli bir şeye (InteractionTarget)
/// bakınca nokta küçülür, çevresinde ince bir halka açılıp yavaşça nefes alır ve altında tek kelimelik fiil belirir.
/// Tıklayınca halka kısa bir vuruş yapar. Oyuncu kontrolü kapalıyken (defter, mektup, menü, asansör) görünmez.
/// </summary>
public class InteractionReticle : MonoBehaviour
{
    private const float DotIdle = 5f, DotFocus = 3f, RingIdle = 8f, RingFocus = 26f;

    private static InteractionReticle instance;

    private PlayerWalker walker;
    private Camera cam;
    private CanvasGroup group;
    private RectTransform dot, ring;
    private RectTransform shadow;
    private Image dotImage, ringImage, shadowImage;
    private TextMeshProUGUI label;
    private float focus, punch, visible;
    private readonly RaycastHit[] hits = new RaycastHit[16];

    /// <summary>PlayerWalker açılınca çağrılır; sahnede tek nişangâh olur.</summary>
    public static void Ensure(PlayerWalker owner)
    {
        if (instance == null) instance = new GameObject("[Nisangah]").AddComponent<InteractionReticle>();
        instance.walker = owner;
    }

    private void Awake()
    {
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 300;
        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1600, 900);
        scaler.matchWidthOrHeight = 1f;
        group = gameObject.AddComponent<CanvasGroup>();
        group.blocksRaycasts = false;
        group.interactable = false;

        Sprite disc = Sprite.Create(CircleTexture(64, 0f), new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f));
        Sprite hoop = Sprite.Create(CircleTexture(128, 0.86f), new Rect(0, 0, 128, 128), new Vector2(0.5f, 0.5f));

        // Açık yüzeylerde (kâğıt, fiş) de seçilsin diye halkanın arkasında yumuşak koyu bir gölge
        Sprite soft = Sprite.Create(CircleTexture(128, 0.6f, 6f), new Rect(0, 0, 128, 128), new Vector2(0.5f, 0.5f));
        shadowImage = NewImage("Golge", soft, new Color(0f, 0f, 0f, 0f));
        shadow = shadowImage.rectTransform;
        ringImage = NewImage("Halka", hoop, new Color(1f, 0.96f, 0.88f, 0f));
        ring = ringImage.rectTransform;
        dotImage = NewImage("Nokta", disc, new Color(1f, 0.97f, 0.9f, 0.55f));
        dot = dotImage.rectTransform;

        label = new GameObject("Fiil", typeof(RectTransform)).AddComponent<TextMeshProUGUI>();
        label.transform.SetParent(transform, false);
        label.font = NicknameLogbookUI.LoadFont("Fonts/PTMono-Regular");
        label.fontSize = 13f;
        label.characterSpacing = 30f;
        label.alignment = TextAlignmentOptions.Center;
        label.color = new Color(1f, 0.96f, 0.88f, 0f);
        label.raycastTarget = false;
        label.fontMaterial.EnableKeyword("UNDERLAY_ON");
        label.fontMaterial.SetColor("_UnderlayColor", new Color(0f, 0f, 0f, 0.85f));
        label.fontMaterial.SetFloat("_UnderlaySoftness", 0.6f);
        label.fontMaterial.SetFloat("_UnderlayDilate", 0.4f);
        label.rectTransform.sizeDelta = new Vector2(300f, 24f);
        label.rectTransform.anchoredPosition = new Vector2(0f, -34f);
    }

    private Image NewImage(string name, Sprite sprite, Color color)
    {
        var img = new GameObject(name, typeof(RectTransform)).AddComponent<Image>();
        img.transform.SetParent(transform, false);
        img.sprite = sprite;
        img.color = color;
        img.raycastTarget = false;
        return img;
    }

    private void Update()
    {
        bool active = walker != null && walker.isActiveAndEnabled && Time.timeScale > 0f;
        visible = Mathf.MoveTowards(visible, active ? 1f : 0f, Time.unscaledDeltaTime * 8f);
        group.alpha = visible;
        if (!active) { focus = 0f; return; }

        if (cam == null) cam = Camera.main;
        InteractionTarget target = cam != null ? FindTarget() : null;

        if (target != null) label.text = target.Label;
        focus = Mathf.MoveTowards(focus, target != null ? 1f : 0f, Time.deltaTime * 7f);
        float k = Mathf.SmoothStep(0f, 1f, focus);

        if (target != null && (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.E))) punch = 1f;
        punch = Mathf.MoveTowards(punch, 0f, Time.deltaTime * 5f);

        // Odaktayken halka yavaşça nefes alır; tıklayınca kısa bir içe vuruş
        float breathe = 1f + Mathf.Sin(Time.time * 3.2f) * 0.06f * k;
        float ringSize = Mathf.Lerp(RingIdle, RingFocus, k) * breathe * (1f - punch * 0.25f);
        ring.sizeDelta = new Vector2(ringSize, ringSize);
        ringImage.color = new Color(1f, 0.96f, 0.88f, 0.85f * k);
        shadow.sizeDelta = new Vector2(ringSize + 10f, ringSize + 10f);
        shadowImage.color = new Color(0f, 0f, 0f, 0.45f * k);

        float dotSize = Mathf.Lerp(DotIdle, DotFocus, k);
        dot.sizeDelta = new Vector2(dotSize, dotSize);
        dotImage.color = new Color(1f, 0.97f, 0.9f, Mathf.Lerp(0.55f, 0.95f, k));

        label.color = new Color(1f, 0.96f, 0.88f, 0.75f * k * k);
        label.rectTransform.anchoredPosition = new Vector2(0f, Mathf.Lerp(-28f, -34f, k));
    }

    // Tetikleyici kutular (kabin alanı vb.) görüşü kesmez; ilk katı yüzeyden öncesine bakılır
    private InteractionTarget FindTarget()
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        int n = Physics.RaycastNonAlloc(ray, hits, 3f, ~0, QueryTriggerInteraction.Collide);
        InteractionTarget best = null;
        float bestDist = float.MaxValue, wallDist = float.MaxValue;

        for (int i = 0; i < n; i++)
        {
            if (hits[i].collider.transform.IsChildOf(walker.transform)) continue;
            var t = hits[i].collider.GetComponent<InteractionTarget>();
            if (t != null && t.enabled)
            {
                if (hits[i].distance <= t.Reach && hits[i].distance < bestDist) { best = t; bestDist = hits[i].distance; }
            }
            else if (!hits[i].collider.isTrigger)
            {
                wallDist = Mathf.Min(wallDist, hits[i].distance);
            }
        }
        return bestDist <= wallDist + 0.02f ? best : null;
    }

    private static Texture2D CircleTexture(int size, float inner, float feather = 1f)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
        float r = size / 2f, rin = r * inner;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(r, r));
                float a = Mathf.Clamp01((r - 1f - d) / feather);
                if (inner > 0f) a *= Mathf.Clamp01((d - rin) / feather);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        tex.Apply();
        return tex;
    }
}
