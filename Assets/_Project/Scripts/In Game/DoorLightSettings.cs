using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Kapı aralığı tarzı ayarlar ekranı. Simsiyah zemin, solda titreyen ışık şeridi,
/// GRAFİK / SES / OYUN sekmeleri ve tek satırlık açıklama. Ayar mantığı SettingsManager'da kalır,
/// bu ekran yalnızca onun metotlarını çağırır. W/S satır, A/D değer, Q/E sekme, Enter değiştir, ESC geri.
/// </summary>
public class DoorLightSettings : MonoBehaviour
{
    private enum RowKind { Cycle, Toggle, Slider, Action }

    private class Row
    {
        public string label;
        public string desc;
        public RowKind kind;
        public Func<string> value;
        public Func<float> sliderValue;
        public Action<int> change;
        public RectTransform rect;
        public TextMeshProUGUI labelText;
        public TextMeshProUGUI valueText;
        public float offset;
    }

    private static readonly Color lightColor = new Color(1f, 0.886f, 0.722f, 1f);
    private static readonly Color accentColor = new Color(1f, 0.788f, 0.541f, 1f);
    private static readonly Color normalColor = new Color(1f, 1f, 1f, 0.55f);
    private const float RowWidth = 760f;
    private const float RowHeight = 54f;
    private const float ContentX = -560f;

    private Canvas canvas;
    private RectTransform strip;
    private RectTransform rowsRoot;
    private RectTransform marker;
    private TextMeshProUGUI descText;
    private readonly List<TextMeshProUGUI> tabTexts = new List<TextMeshProUGUI>();
    private readonly List<RectTransform> tabLines = new List<RectTransform>();
    private readonly List<List<Row>> tabs = new List<List<Row>>();
    private int tab;
    private int selected;
    private Action onBack;
    private float flickerSeed;
    private int openedFrame;

    /// <summary>Ekranın en son kapandığı kare; arkadaki menü bu karede tuşları yok sayar.</summary>
    public static int LastClosedFrame { get; private set; } = -1;

    public bool IsOpen => canvas != null && canvas.gameObject.activeSelf;

    private static bool IsTR => LanguageManager.instance == null || LanguageManager.instance.CurrentLanguage == LanguageManager.Language.TR;
    private static string L(string tr, string en) => IsTR ? tr : en;
    private static SettingsManager SM => SettingsManager.instance != null ? SettingsManager.instance
        : FindAnyObjectByType<SettingsManager>(FindObjectsInactive.Include);

    /// <summary>Ekranı açar; geri dönülünce onBack çağrılır.</summary>
    public void Open(int sortingOrder, Action backAction)
    {
        onBack = backAction;
        if (SM != null) SM.PrepareForExternalUI();
        Build(sortingOrder);
        canvas.sortingOrder = sortingOrder;
        canvas.gameObject.SetActive(true);
        openedFrame = Time.frameCount;
        SetTab(0);
    }

    public void Close()
    {
        if (canvas != null) canvas.gameObject.SetActive(false);
        LastClosedFrame = Time.frameCount;
        Action back = onBack;
        onBack = null;
        back?.Invoke();
    }

    private void DefineRows()
    {
        tabs.Clear();
        tabs.Add(new List<Row>
        {
            new Row { label = L("KALİTE", "QUALITY"), desc = L("Gölge ve ışık detayını belirler.", "Shadow and lighting detail."), kind = RowKind.Cycle,
                value = () => SM != null ? SM.QualityName : "-", change = d => SM?.ChangeQuality(d) },
            new Row { label = L("EKRAN MODU", "DISPLAY MODE"), desc = L("Oyunun ekranı nasıl kaplayacağı.", "How the game fills the screen."), kind = RowKind.Toggle,
                value = () => SM != null && SM.IsFullscreen ? L("TAM EKRAN", "FULLSCREEN") : L("PENCERE", "WINDOWED"), change = d => SM?.ToggleFullscreen() },
            new Row { label = L("ÇÖZÜNÜRLÜK", "RESOLUTION"), desc = L("Ekran çözünürlüğü.", "Screen resolution."), kind = RowKind.Cycle,
                value = () => SM != null ? SM.ResolutionName : "-", change = d => SM?.ChangeResolution(d) },
            new Row { label = L("FPS SINIRI", "FPS LIMIT"), desc = L("Saniyedeki kare üst sınırı.", "Maximum frames per second."), kind = RowKind.Cycle,
                value = () => SM != null ? SM.FrameRateName : "-", change = d => SM?.ChangeFrameRate(d) },
            new Row { label = "V-SYNC", desc = L("Ekran yırtılmasını önler.", "Prevents screen tearing."), kind = RowKind.Toggle,
                value = () => SM != null && SM.IsVSync ? L("AÇIK", "ON") : L("KAPALI", "OFF"), change = d => SM?.ToggleVSync() },
        });
        tabs.Add(new List<Row>
        {
            VolumeRow(L("GENEL SES", "MASTER"), L("Tüm seslerin seviyesi.", "Overall volume."),
                () => AudioManager.instance != null ? AudioManager.instance.GetMasterVolume() : AudioListener.volume, v => SM?.SetMasterVolume(v)),
            VolumeRow(L("MÜZİK", "MUSIC"), L("Arka plan müziği.", "Background music."),
                () => AudioManager.instance != null ? AudioManager.instance.GetMusicVolume() : PlayerPrefs.GetFloat("Dink_MusicVolume", 0.5f), v => SM?.SetMusicVolume(v)),
            VolumeRow(L("EFEKTLER", "EFFECTS"), L("Kapı, adım ve fısıltılar.", "Doors, footsteps and whispers."),
                () => AudioManager.instance != null ? AudioManager.instance.GetSFXVolume() : PlayerPrefs.GetFloat("Dink_SFXVolume", 0.8f), v => SM?.SetSFXVolume(v)),
        });
        tabs.Add(new List<Row>
        {
            new Row { label = L("DİL", "LANGUAGE"), desc = L("Arayüz ve mektup dili.", "Interface and letter language."), kind = RowKind.Toggle,
                value = () => LanguageManager.instance != null ? LanguageManager.instance.GetCurrentLanguageName().ToUpperInvariant() : "-",
                change = d => { if (LanguageManager.instance != null) LanguageManager.instance.NextLanguage(); RebuildRows(2); } },
            new Row { label = L("VARSAYILANA DÖN", "RESTORE DEFAULTS"), desc = L("Tüm ayarları sıfırlar.", "Resets every setting."), kind = RowKind.Action,
                value = () => "", change = d => SM?.ResetToDefaults() },
            new Row { label = L("GERİ", "BACK"), desc = L("Önceki menüye döner.", "Returns to the previous menu."), kind = RowKind.Action,
                value = () => "", change = d => Close() },
        });
    }

    private static Row VolumeRow(string label, string desc, Func<float> get, Action<float> set)
    {
        return new Row
        {
            label = label, desc = desc, kind = RowKind.Slider, sliderValue = get,
            change = d => set(Mathf.Clamp01(Mathf.Round(get() * 10f + d) / 10f))
        };
    }

    // ─── Kurulum ───

    private void Build(int sortingOrder)
    {
        if (canvas != null) return;
        flickerSeed = UnityEngine.Random.value * 100f;

        GameObject canvasObj = new GameObject("DoorLightSettings");
        canvasObj.transform.SetParent(transform, false);
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 1f;
        canvasObj.AddComponent<GraphicRaycaster>();

        Image dark = NewImage(canvasObj.transform, "Dark", new Color(0f, 0f, 0f, 1f));
        dark.rectTransform.anchorMin = Vector2.zero;
        dark.rectTransform.anchorMax = Vector2.one;
        dark.rectTransform.sizeDelta = Vector2.zero;

        strip = NewImage(canvasObj.transform, "DoorGap", lightColor).rectTransform;
        strip.anchoredPosition = new Vector2(ContentX - 160f, 0f);
        strip.sizeDelta = new Vector2(10f, 760f);

        NewText(canvasObj.transform, "Title", L("AYARLAR", "SETTINGS"), 26, new Color(1f, 1f, 1f, 0.4f), 20f,
            new Vector2(ContentX, 330f), new Vector2(600f, 40f));

        for (int i = 0; i < 3; i++)
        {
            int index = i;
            string[] names = { L("GRAFİK", "GRAPHICS"), L("SES", "AUDIO"), L("OYUN", "GAME") };
            TextMeshProUGUI t = NewText(canvasObj.transform, $"Tab_{i}", names[i], 24, normalColor, 8f,
                new Vector2(ContentX + i * 190f, 265f), new Vector2(180f, 40f));
            t.raycastTarget = true;
            AddTrigger(t.gameObject, EventTriggerType.PointerClick, e => SetTab(index));
            tabTexts.Add(t);

            RectTransform line = NewImage(t.transform, "Line", accentColor).rectTransform;
            line.anchorMin = line.anchorMax = new Vector2(0f, 0f);
            line.pivot = new Vector2(0f, 0.5f);
            line.anchoredPosition = new Vector2(0f, -2f);
            line.sizeDelta = new Vector2(t.preferredWidth, 2f);
            tabLines.Add(line);
        }

        rowsRoot = new GameObject("Rows", typeof(RectTransform)).GetComponent<RectTransform>();
        rowsRoot.SetParent(canvasObj.transform, false);
        rowsRoot.anchoredPosition = Vector2.zero;

        marker = NewImage(canvasObj.transform, "Marker", accentColor).rectTransform;
        marker.sizeDelta = new Vector2(3f, 30f);

        descText = NewText(canvasObj.transform, "Desc", "", 20, new Color(1f, 1f, 1f, 0.45f), 2f,
            new Vector2(ContentX, -330f), new Vector2(RowWidth, 40f));
    }

    private void SetTab(int index)
    {
        tab = index;
        selected = 0;
        RebuildRows(index);
        for (int i = 0; i < tabTexts.Count; i++)
        {
            tabTexts[i].color = i == tab ? Color.white : new Color(1f, 1f, 1f, 0.35f);
            tabLines[i].gameObject.SetActive(i == tab);
        }
    }

    private void RebuildRows(int index)
    {
        DefineRows();
        foreach (Transform child in rowsRoot) Destroy(child.gameObject);

        List<Row> rows = tabs[index];
        for (int i = 0; i < rows.Count; i++)
        {
            int rowIndex = i;
            Row row = rows[i];
            GameObject rowObj = new GameObject($"Row_{i}", typeof(RectTransform));
            row.rect = rowObj.GetComponent<RectTransform>();
            row.rect.SetParent(rowsRoot, false);
            row.rect.anchoredPosition = new Vector2(ContentX, 180f - i * RowHeight);
            row.rect.pivot = new Vector2(0f, 0.5f);
            row.rect.sizeDelta = new Vector2(RowWidth, RowHeight);

            Image hit = rowObj.AddComponent<Image>();
            hit.color = Color.clear;
            AddTrigger(rowObj, EventTriggerType.PointerEnter, e => Select(rowIndex));
            AddTrigger(rowObj, EventTriggerType.PointerClick, e =>
                Change(((PointerEventData)e).button == PointerEventData.InputButton.Right ? -1 : 1));

            Image divider = NewImage(rowObj.transform, "Divider", new Color(1f, 1f, 1f, 0.07f));
            divider.rectTransform.anchorMin = new Vector2(0f, 0f);
            divider.rectTransform.anchorMax = new Vector2(1f, 0f);
            divider.rectTransform.pivot = new Vector2(0.5f, 0f);
            divider.rectTransform.anchoredPosition = Vector2.zero;
            divider.rectTransform.sizeDelta = new Vector2(0f, 1f);

            row.labelText = NewText(rowObj.transform, "Label", row.label, 26, normalColor, 6f, Vector2.zero, new Vector2(RowWidth, RowHeight));
            Fill(row.labelText.rectTransform);
            row.valueText = NewText(rowObj.transform, "Value", "", 22, normalColor, 3f, Vector2.zero, new Vector2(RowWidth, RowHeight));
            Fill(row.valueText.rectTransform);
            row.valueText.alignment = TextAlignmentOptions.MidlineRight;
        }
        if (index == tab) tabs[tab] = rows;
        selected = Mathf.Clamp(selected, 0, rows.Count - 1);
        Refresh();
    }

    // ─── Etkileşim ───

    private void Update()
    {
        if (!IsOpen) return;

        float t = Time.unscaledTime * 6f + flickerSeed;
        float flicker = 0.85f + Mathf.PerlinNoise(t, 0.3f) * 0.15f;
        if (Mathf.PerlinNoise(t * 0.4f, 7f) > 0.82f) flicker *= 0.6f;
        strip.GetComponent<Image>().color = new Color(lightColor.r, lightColor.g, lightColor.b, flicker);

        List<Row> rows = tabs[tab];
        for (int i = 0; i < rows.Count; i++)
        {
            Row row = rows[i];
            row.offset = Mathf.Lerp(row.offset, i == selected ? 22f : 0f, Time.unscaledDeltaTime * 14f);
            row.rect.anchoredPosition = new Vector2(ContentX + row.offset, row.rect.anchoredPosition.y);
        }
        Row sel = rows[selected];
        marker.anchoredPosition = new Vector2(ContentX + sel.offset - 16f, sel.rect.anchoredPosition.y);

        // Açıldığı karede (Enter/ESC ile açıldıysa) tuşlar ikinci kez işlenmesin
        if (Time.frameCount == openedFrame) return;

        if (DinkInput.EscapeDown) { DinkInput.ConsumeEscape(); Close(); return; }
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) Select((selected + 1) % rows.Count);
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) Select((selected - 1 + rows.Count) % rows.Count);
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) Change(1);
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) Change(-1);
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space)) Change(1);
        if (Input.GetKeyDown(KeyCode.E)) SetTab((tab + 1) % tabs.Count);
        if (Input.GetKeyDown(KeyCode.Q)) SetTab((tab + tabs.Count - 1) % tabs.Count);
    }

    private void Select(int index)
    {
        selected = index;
        Refresh();
    }

    private void Change(int delta)
    {
        Row row = tabs[tab][selected];
        // Aksiyon satırları (Geri, Varsayılan) yalnızca Enter/tık ile çalışsın, A/D ile değil
        if (row.kind == RowKind.Action && delta < 0) return;
        row.change?.Invoke(delta);
        if (IsOpen) Refresh();
    }

    private void Refresh()
    {
        List<Row> rows = tabs[tab];
        for (int i = 0; i < rows.Count; i++)
        {
            Row row = rows[i];
            bool on = i == selected;
            row.labelText.color = on ? Color.white : normalColor;
            row.valueText.color = on ? Color.white : normalColor;
            row.valueText.text = FormatValue(row, on);
        }
        descText.text = rows[selected].desc;
    }

    private static string FormatValue(Row row, bool selected)
    {
        if (row.kind == RowKind.Slider)
        {
            int filled = Mathf.RoundToInt(row.sliderValue() * 10f);
            return $"<color=#FFFFFF>{new string('■', filled)}</color><color=#FFFFFF2E>{new string('■', 10 - filled)}</color>  {filled * 10,3}";
        }
        string v = row.value();
        if (row.kind == RowKind.Action) return selected ? "→" : "";
        return selected ? $"‹   {v}   ›" : v;
    }

    // ─── Yardımcılar ───

    private static Image NewImage(Transform parent, string name, Color color)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        Image img = obj.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
        return img;
    }

    private static TextMeshProUGUI NewText(Transform parent, string name, string text, float size, Color color, float spacing, Vector2 pos, Vector2 sizeDelta)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        RectTransform rt = (RectTransform)obj.transform;
        rt.pivot = new Vector2(0f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = sizeDelta;
        TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.color = color;
        tmp.characterSpacing = spacing;
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
        tmp.enableWordWrapping = false;
        tmp.raycastTarget = false;
        return tmp;
    }

    private static void Fill(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = Vector2.zero;
    }

    private static void AddTrigger(GameObject target, EventTriggerType type, Action<BaseEventData> callback)
    {
        EventTrigger trigger = target.GetComponent<EventTrigger>() ?? target.AddComponent<EventTrigger>();
        EventTrigger.Entry entry = new EventTrigger.Entry { eventID = type };
        entry.callback.AddListener(e => callback(e));
        trigger.triggers.Add(entry);
    }
}
