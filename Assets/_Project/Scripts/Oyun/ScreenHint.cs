using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Ekranın altında kısa süreli yönlendirme yazısı. Aynı yazı bekleme süresi dolmadan tekrar gösterilmez.
/// Kullanım: ScreenHint.Show(DinkLang.T(...));
/// </summary>
public class ScreenHint : MonoBehaviour
{
    private const float ShowSeconds = 3f;
    private const float FadeSeconds = 0.25f;

    private static ScreenHint instance;
    private static readonly Dictionary<string, float> nextAllowed = new Dictionary<string, float>();

    private TMP_Text label;
    private Coroutine running;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        instance = null;
        nextAllowed.Clear();
    }

    /// <summary>Yazıyı ~3 sn gösterir; aynı yazı <paramref name="cooldown"/> saniye içinde yinelenmez.</summary>
    public static void Show(string text, float cooldown = 6f)
    {
        if (string.IsNullOrEmpty(text)) return;
        if (nextAllowed.TryGetValue(text, out float t) && Time.unscaledTime < t) return;
        nextAllowed[text] = Time.unscaledTime + cooldown;

        if (instance == null) instance = Create();
        if (instance.running != null) instance.StopCoroutine(instance.running);
        instance.running = instance.StartCoroutine(instance.Run(text));
    }

    private static ScreenHint Create()
    {
        var go = new GameObject("[EkranIpucu]");
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 900;
        var scaler = go.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        var hint = go.AddComponent<ScreenHint>();
        var textGo = new GameObject("Yazi");
        textGo.transform.SetParent(go.transform, false);
        var tmp = textGo.AddComponent<TextMeshProUGUI>();
        // TR/RU karakterleri için hazır TMP fontu (fallback varsayılan font)
        tmp.font = NicknameLogbookUI.LoadFont("Fonts/PTMono-Regular");
        tmp.fontSize = 30;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        tmp.raycastTarget = false;
        tmp.color = new Color(0.88f, 0.86f, 0.8f, 0f);
        RectTransform r = tmp.rectTransform;
        r.anchorMin = new Vector2(0.5f, 0f);
        r.anchorMax = new Vector2(0.5f, 0f);
        r.pivot = new Vector2(0.5f, 0f);
        r.anchoredPosition = new Vector2(0f, 90f);
        r.sizeDelta = new Vector2(1400f, 120f);
        hint.label = tmp;
        return hint;
    }

    private IEnumerator Run(string text)
    {
        label.text = text;
        Color c = label.color;
        for (float t = 0f; t < ShowSeconds; t += Time.unscaledDeltaTime)
        {
            float a = t < FadeSeconds ? t / FadeSeconds : t > ShowSeconds - FadeSeconds ? (ShowSeconds - t) / FadeSeconds : 1f;
            c.a = a * 0.9f;
            label.color = c;
            yield return null;
        }
        c.a = 0f;
        label.color = c;
        running = null;
    }

    private void OnDestroy()
    {
        if (instance == this) instance = null;
    }
}
