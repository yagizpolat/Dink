using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Ana menü ayarları: lobi giriş kapısının sağ camına yapıştırılmış vinil harfler (LobbyMenu'nün aynası).
/// Satırlar DoorLightSettings.BuildRows'tan gelir; mantık tek yerde kalır, burada yalnızca çizim ve girdi var.
/// Fare ile üzerine gelme/tıklama, tekerlek veya A/D/ok tuşları ile değer, W/S/ok ile satır, Enter etkinleştir, ESC geri.
/// </summary>
public class LobbySettings : MonoBehaviour
{
    // ─── Yerleşim (sağ cam yüzeyi, kamera tarafı) ───
    private const float CamZ = -0.03f;
    private const float LeafCenterX = 0.48f;
    private const float LeafWidth = 0.8f;
    private const float TitleY = 1.98f;
    private const float ItemsTopY = 1.86f;
    private const float ItemStep = 0.068f;
    private const float TitleSize = 0.5f;
    private const float ItemSize = 0.38f;
    private const float DescSize = 0.3f;
    private const float UnselectedAlpha = 0.4f;
    private const float FadeTime = 0.25f;
    private const int LineChars = 30;

    private static readonly Color Ink = new Color(0.82f, 0.8f, 0.75f, 1f);

    private class Item
    {
        public DoorLightSettings.SettingRow d;
        public TextMeshPro text;
        public BoxCollider col;
    }

    /// <summary>Menü yazılarını soldurmak ve ışık tökezlemesi için (opsiyonel).</summary>
    public LobbyMenu menu;

    private GameObject root;
    private TextMeshPro title, desc;
    private readonly List<Item> items = new List<Item>();
    private TMP_FontAsset font;
    private Camera cam;
    private int selected;
    private Action onBack;
    private bool open;
    private float vis;
    private int openedFrame;
    private Vector3 lastMouse;

    /// <summary>Ayarlar açık (ya da kapanma solması sürüyor) mu.</summary>
    public bool IsOpen => open;

    /// <summary>Ekranın en son kapandığı kare; arkadaki menü bu karede tuşları yok sayar.</summary>
    public int LastClosedFrame { get; private set; } = -1;

    private void OnEnable() { LanguageManager.OnLanguageChanged += Relabel; }
    private void OnDisable() { LanguageManager.OnLanguageChanged -= Relabel; }

    public void Open(Action backAction)
    {
        onBack = backAction;
        DoorLightSettings.PrepareSettings();
        if (root == null) Build();
        root.SetActive(true);
        Relabel();
        open = true;
        openedFrame = Time.frameCount;
        lastMouse = Input.mousePosition;
        Select(0, false);
        if (menu != null) menu.FadeTo(0.15f, FadeTime);
    }

    public void Close()
    {
        if (!open) return;
        open = false;
        LastClosedFrame = Time.frameCount;
        if (menu != null) menu.FadeTo(1f, FadeTime);
        Action back = onBack;
        onBack = null;
        back?.Invoke();
    }

    // ═══ Kurulum ═══

    private void Build()
    {
        cam = Camera.main;
        font = NicknameLogbookUI.LoadFont("Fonts/PTMono-Regular");
        root = new GameObject("LobbySettings_Yazilar");
        Transform parent = menu != null && menu.Content != null ? menu.Content.transform.parent : null;
        if (parent != null) root.transform.SetParent(parent, false);
        root.transform.position = Vector3.zero;
        root.transform.rotation = Quaternion.identity;

        title = MakeText("Baslik", TitleSize, new Vector3(LeafCenterX, TitleY, CamZ));
        title.alignment = TextAlignmentOptions.Center;
        title.characterSpacing = 4f;
        title.transform.rotation = Quaternion.Euler(0f, 0f, UnityEngine.Random.Range(-1.2f, 1.2f));

        desc = MakeText("Aciklama", DescSize, Vector3.zero);
        desc.enableWordWrapping = true;
        desc.alignment = TextAlignmentOptions.TopLeft;
        desc.rectTransform.sizeDelta = new Vector2(LeafWidth, 0.2f);
        desc.rectTransform.pivot = new Vector2(0.5f, 1f);
        vis = 0f;
    }

    private TextMeshPro MakeText(string name, float size, Vector3 pos)
    {
        var go = new GameObject(name);
        go.transform.SetParent(root.transform, false);
        go.transform.position = pos;
        var t = go.AddComponent<TextMeshPro>();
        if (font != null) t.font = font;
        t.fontSize = size;
        t.alignment = TextAlignmentOptions.Left;
        t.enableWordWrapping = false;
        t.richText = true;
        t.rectTransform.sizeDelta = new Vector2(LeafWidth, 0.1f);
        t.color = new Color(Ink.r, Ink.g, Ink.b, 0f);
        return t;
    }

    // Dil değişince (ya da açılışta) satır verilerini yeniden kurar; satır sayısı sabittir
    private void Relabel()
    {
        if (root == null) return;
        var rows = new List<DoorLightSettings.SettingRow>();
        foreach (var tab in DoorLightSettings.BuildRows(Close)) rows.AddRange(tab);

        title.text = DinkLang.T("SETTINGS", "AYARLAR", "EINSTELLUNGEN", "PARAMÈTRES", "AJUSTES", "CONFIGURAÇÕES", "НАСТРОЙКИ");
        for (int i = 0; i < rows.Count; i++)
        {
            Item it;
            if (i < items.Count) it = items[i];
            else
            {
                it = new Item { text = MakeText("Ayar_" + i, ItemSize, new Vector3(LeafCenterX, ItemsTopY - i * ItemStep, CamZ)) };
                it.text.transform.rotation = Quaternion.Euler(0f, 0f, UnityEngine.Random.Range(-0.8f, 0.8f));
                it.col = it.text.gameObject.AddComponent<BoxCollider>();
                it.col.center = Vector3.zero;
                it.col.size = new Vector3(LeafWidth, ItemStep * 0.9f, 0.02f);
                items.Add(it);
            }
            it.d = rows[i];
        }
        desc.transform.position = new Vector3(LeafCenterX, ItemsTopY - rows.Count * ItemStep + ItemStep * 0.3f, CamZ);
        selected = Mathf.Clamp(selected, 0, items.Count - 1);
        Refresh();
    }

    // ═══ Girdi ═══

    private void Update()
    {
        if (root == null || !root.activeSelf) return;

        // Solma: açıkken 1'e, kapanınca 0'a; 0'a varınca kök gizlenir
        float target = open ? 1f : 0f;
        if (!Mathf.Approximately(vis, target))
        {
            vis = Mathf.MoveTowards(vis, target, Time.unscaledDeltaTime / FadeTime);
            ApplyColors();
            if (!open && vis <= 0f) { root.SetActive(false); return; }
        }

        if (!open || Time.frameCount == openedFrame || items.Count == 0) return;

        bool moved = (Input.mousePosition - lastMouse).sqrMagnitude > 4f;
        lastMouse = Input.mousePosition;
        float wheel = Input.mouseScrollDelta.y;

        if (DinkInput.EscapeDown) { DinkInput.ConsumeEscape(); Close(); return; }

        if (cam == null) cam = Camera.main;
        if (cam != null && Cursor.visible)
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            int hover = -1;
            foreach (RaycastHit h in Physics.RaycastAll(ray, 20f, ~0, QueryTriggerInteraction.Collide))
                for (int k = 0; k < items.Count; k++)
                    if (h.collider == items[k].col) hover = k;
            if (hover >= 0)
            {
                if ((moved || Input.GetMouseButtonDown(0)) && hover != selected) Select(hover, true);
                if (Input.GetMouseButtonDown(0)) { Change(1); return; }
                if (Input.GetMouseButtonDown(1)) { Change(-1); return; }
            }
        }

        if (Mathf.Abs(wheel) > 0.01f && items[selected].d.kind != DoorLightSettings.SettingKind.Action) Change(wheel > 0f ? 1 : -1);
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) Select((selected + 1) % items.Count, true);
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) Select((selected - 1 + items.Count) % items.Count, true);
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) Change(1);
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) Change(-1);
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space)) Change(1);
    }

    private void Select(int i, bool fromUser)
    {
        if (items.Count == 0) return;
        bool changed = i != selected;
        selected = Mathf.Clamp(i, 0, items.Count - 1);
        Refresh();
        if (fromUser && changed && menu != null) menu.Stumble();
    }

    private void Change(int delta)
    {
        if (selected < 0 || selected >= items.Count) return;
        var row = items[selected].d;
        // Aksiyon satırları (Geri, Varsayılan) yalnızca Enter/tık ile çalışsın
        if (row.kind == DoorLightSettings.SettingKind.Action && delta < 0) return;
        row.change?.Invoke(delta);
        Refresh();
    }

    // ═══ Çizim ═══

    private void Refresh()
    {
        for (int i = 0; i < items.Count; i++)
            items[i].text.text = Line(items[i].d, i == selected);
        if (selected >= 0 && selected < items.Count) desc.text = items[selected].d.desc;
        ApplyColors();
    }

    private void ApplyColors()
    {
        if (title != null) title.color = new Color(0.78f, 0.77f, 0.72f, 0.55f * vis);
        if (desc != null) desc.color = new Color(Ink.r, Ink.g, Ink.b, 0.3f * vis);
        for (int i = 0; i < items.Count; i++)
            items[i].text.color = new Color(Ink.r, Ink.g, Ink.b, (i == selected ? 0.85f : UnselectedAlpha) * vis);
    }

    // "KALİTE ........ YÜKSEK": eş aralıklı yazıda değer sağ kenara hizalanır
    private static string Line(DoorLightSettings.SettingRow row, bool on)
    {
        string label = row.label;
        string value;
        if (row.kind == DoorLightSettings.SettingKind.Slider)
        {
            int filled = Mathf.Clamp(Mathf.RoundToInt((row.sliderValue != null ? row.sliderValue() : 0f) * 10f), 0, 10);
            value = "[" + new string('|', filled) + new string('.', 10 - filled) + "]";
        }
        else if (row.kind == DoorLightSettings.SettingKind.Action)
            value = "";
        else
        {
            value = row.value != null ? row.value() : "";
            if (on) value = "< " + value + " >";
        }

        if (value.Length == 0) return label;
        int dots = LineChars - label.Length - value.Length - 2;
        if (dots < 1) return label + " " + value;
        return label + " " + new string('.', dots) + " " + value;
    }
}
