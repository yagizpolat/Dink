using System;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// "Kapı Aralığı" duraklatma menüsü.
/// Ekran kararır, ortada aralık bir kapıdan sızan dikey ışık şeridi açılır; seçenekler ışığın yanında,
/// karanlıkta durur. Seçili satır ışığa yaklaşır ve aydınlanır, diğerleri karanlıkta kalır.
/// Arayüz çalışma anında kurulur; Escmenu tarafından açılıp kapatılır.
/// </summary>
public class DoorLightPauseMenu : MonoBehaviour
{
    private static readonly Color lightColor = new Color(1f, 0.86f, 0.64f);
    private static readonly Color textLit = new Color(0.95f, 0.9f, 0.82f, 1f);
    private static readonly Color textDark = new Color(0.55f, 0.5f, 0.45f, 0.35f);

    private const float StripWidth = 10f;
    private const float StripHeight = 760f;
    private const float StripX = -170f;          // ekran merkezine göre ışık şeridinin konumu
    private const float ItemX = 70f;             // şeridin sağına göre ilk harf
    private const float ItemSpacing = 74f;
    private const float OpenDuration = 0.45f;

    private Canvas canvas;
    private CanvasGroup group;
    private RectTransform strip;
    private RectTransform glow;
    private RectTransform spill;
    private readonly System.Collections.Generic.List<TextMeshProUGUI> items = new System.Collections.Generic.List<TextMeshProUGUI>();
    private Action[] actions;
    private int selected;
    private float flickerSeed;

    public GameObject Root => canvas != null ? canvas.gameObject : null;
    public bool IsOpen => canvas != null && canvas.gameObject.activeSelf;

    /// <summary>
    /// Menüyü kurar. Etiketler ve eylemler aynı sırada verilir.
    /// </summary>
    public void Build(string[] labels, Action[] itemActions)
    {
        actions = itemActions;
        flickerSeed = UnityEngine.Random.value * 100f;

        GameObject canvasObj = new GameObject("DoorLightPauseMenu");
        canvasObj.transform.SetParent(transform, false);
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 150;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 1f;
        canvasObj.AddComponent<GraphicRaycaster>();
        group = canvasObj.AddComponent<CanvasGroup>();

        // Karanlık: oyun dünyası arkada çok hafif seçilir
        Image dark = CreateImage(canvasObj.transform, "Dark", null, new Color(0f, 0f, 0f, 0.985f));
        Stretch(dark.rectTransform);

        Sprite soft = CreateSoftSprite();

        // Işığın duvara/havaya yayılan geniş, soluk parlaması
        glow = CreateImage(canvasObj.transform, "Glow", soft, new Color(lightColor.r, lightColor.g, lightColor.b, 0.16f)).rectTransform;
        glow.anchoredPosition = new Vector2(StripX, 0f);
        glow.sizeDelta = new Vector2(420f, StripHeight + 120f);

        // Kapı altından yere vuran ışık
        spill = CreateImage(canvasObj.transform, "FloorSpill", soft, new Color(lightColor.r, lightColor.g, lightColor.b, 0.22f)).rectTransform;
        spill.anchoredPosition = new Vector2(StripX + 40f, -StripHeight / 2f - 10f);
        spill.sizeDelta = new Vector2(520f, 70f);
        spill.localRotation = Quaternion.Euler(0f, 0f, -4f);

        // Kapı aralığı: keskin, parlak dikey şerit
        Image stripImg = CreateImage(canvasObj.transform, "DoorGap", null, lightColor);
        strip = stripImg.rectTransform;
        strip.anchoredPosition = new Vector2(StripX, 0f);
        strip.sizeDelta = new Vector2(StripWidth, StripHeight);

        // Seçenekler
        float startY = (labels.Length - 1) * ItemSpacing / 2f;
        for (int i = 0; i < labels.Length; i++)
        {
            int index = i;
            GameObject itemObj = new GameObject($"Item_{i}");
            itemObj.transform.SetParent(canvasObj.transform, false);
            RectTransform rt = itemObj.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0f, 0.5f);
            rt.anchoredPosition = new Vector2(StripX + ItemX, startY - i * ItemSpacing);
            rt.sizeDelta = new Vector2(620f, 64f);

            TextMeshProUGUI tmp = itemObj.AddComponent<TextMeshProUGUI>();
            tmp.text = labels[i];
            tmp.fontSize = 44;
            tmp.characterSpacing = 4f;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            tmp.color = textDark;
            tmp.raycastTarget = true;
            items.Add(tmp);

            EventTrigger trigger = itemObj.AddComponent<EventTrigger>();
            AddTrigger(trigger, EventTriggerType.PointerEnter, () => Select(index));
            AddTrigger(trigger, EventTriggerType.PointerClick, () => Activate(index));
        }

        canvasObj.SetActive(false);
    }

    public void Open()
    {
        if (canvas == null) return;
        canvas.gameObject.SetActive(true);
        Select(0, instant: true);

        // Kapı aralanır: şerit sıfırdan açılır, yazılar karanlıktan belirir
        group.alpha = 0f;
        strip.sizeDelta = new Vector2(0f, StripHeight);
        Tween.Alpha(group, 1f, 0.2f, useUnscaledTime: true);
        Tween.UISizeDelta(strip, new Vector2(StripWidth, StripHeight), OpenDuration, Ease.OutCubic, useUnscaledTime: true);
    }

    public void Close()
    {
        if (canvas != null) canvas.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!IsOpen) return;

        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) Select((selected + 1) % items.Count);
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) Select((selected - 1 + items.Count) % items.Count);
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space)) Activate(selected);

        // Eski ampul gibi hafif titreme (oyun duraklatıldığı için gerçek zaman kullanılır)
        float t = Time.unscaledTime;
        float flicker = Mathf.Lerp(0.85f, 1f, Mathf.PerlinNoise(flickerSeed, t * 4f));
        if (Mathf.PerlinNoise(flickerSeed + 30f, t * 0.8f) > 0.86f) flicker *= 0.55f;
        SetAlpha(glow, 0.16f * flicker);
        SetAlpha(spill, 0.22f * flicker);
        SetAlpha(strip, flicker);
    }

    private void Select(int index, bool instant = false)
    {
        selected = index;
        for (int i = 0; i < items.Count; i++)
        {
            bool isSel = i == index;
            // Seçili satır ışığa yaklaşır, diğerleri karanlıkta biraz uzaklaşır
            float targetX = StripX + (isSel ? ItemX - 18f : ItemX);
            RectTransform rt = items[i].rectTransform;
            Vector2 target = new Vector2(targetX, rt.anchoredPosition.y);
            Color targetColor = isSel ? textLit : textDark;

            if (instant)
            {
                rt.anchoredPosition = target;
                items[i].color = targetColor;
            }
            else
            {
                Tween.UIAnchoredPosition(rt, target, 0.18f, Ease.OutQuad, useUnscaledTime: true);
                Tween.Color(items[i], targetColor, 0.18f, useUnscaledTime: true);
            }
        }
    }

    private void Activate(int index)
    {
        if (actions != null && index >= 0 && index < actions.Length)
        {
            actions[index]?.Invoke();
        }
    }

    private static void SetAlpha(RectTransform rt, float a)
    {
        Image img = rt.GetComponent<Image>();
        Color c = img.color;
        c.a = a;
        img.color = c;
    }

    private static Image CreateImage(Transform parent, string name, Sprite sprite, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        Image img = go.AddComponent<Image>();
        img.sprite = sprite;
        img.color = color;
        img.raycastTarget = false;
        return img;
    }

    private static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static void AddTrigger(EventTrigger trigger, EventTriggerType type, Action callback)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry { eventID = type };
        entry.callback.AddListener(_ => callback());
        trigger.triggers.Add(entry);
    }

    /// <summary>
    /// Kenarlara doğru yumuşakça sönen (radyal) beyaz doku; parlama ve yere vuran ışık için.
    /// </summary>
    private static Sprite CreateSoftSprite()
    {
        const int size = 64;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = (x - size / 2f + 0.5f) / (size / 2f);
                float dy = (y - size / 2f + 0.5f) / (size / 2f);
                float d = Mathf.Clamp01(Mathf.Sqrt(dx * dx + dy * dy));
                float a = Mathf.Pow(1f - d, 2.2f);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }
}
