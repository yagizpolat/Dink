using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Oyuncu adını kapıcı masasındaki giriş-çıkış defterine yazdırarak sorar.
/// Tüm UI kodla kurulur; 1600x900 referans tuvalde tek parça ölçeklenir, böylece hiçbir dilde yazılar çakışmaz.
/// Time.timeScale = 0 iken çalışır (unscaled time).
/// </summary>
public class NicknameLogbookUI : MonoBehaviour
{
    internal const float PageW = 1140f, PageH = 740f;
    internal const float PadL = 92f, PadR = 52f, PadT = 44f, PadB = 36f;
    private const float HeaderRowH = 44f, RowH = 70f;
    private const int MaxNameLength = 20;
    private const float NameMaxSize = 34f, NameMinSize = 17f;

    private static readonly float[] ColumnShares = { 0.18f, 0.13f, 0.53f, 0.16f };

    internal static readonly Color PaperColor = Hex("E3D9C2");
    internal static readonly Color PrintColor = Hex("4E5A58");
    internal static readonly Color RuleColor = Hex("B9BCB1");
    internal static readonly Color BlueInk = Hex("34406A");
    internal static readonly Color BlackInk = Hex("2B2621");
    internal static readonly Color PencilInk = new Color(0.38f, 0.36f, 0.33f, 0.8f);
    internal static readonly Color PlaceholderInk = new Color(0.52f, 0.5f, 0.45f, 0.5f);
    internal static readonly Color StampRed = new Color(0.61f, 0.18f, 0.15f, 0.72f);

    private Canvas canvas;
    private TextMeshProUGUI nameText;
    private TextMeshProUGUI cueText;
    private GameObject stamp;
    private Image blackout;
    private TMP_FontAsset handFont, handFontBold, monoFont;

    private string typed = "";
    private string placeholder;
    private bool submitRequested;

    /// <summary>Defteri açar, oyuncu adını yazıp Enter'a basana kadar bekler, sonra ekranı karartır.</summary>
    public static IEnumerator Run(Action<string> onSubmitted)
    {
        NicknameLogbookUI ui = new GameObject("[NicknameLogbook]").AddComponent<NicknameLogbookUI>();
        yield return ui.Play(onSubmitted);
    }

    /// <summary>Karartma tamamlandıktan sonra çağrılır; arkadaki geçiş kendi siyahını devraldığında defteri siler.</summary>
    public static void CloseAll()
    {
        foreach (NicknameLogbookUI ui in FindObjectsByType<NicknameLogbookUI>(FindObjectsSortMode.None))
            Destroy(ui.gameObject);
    }

    private IEnumerator Play(Action<string> onSubmitted)
    {
        Build();
        RefreshName(true);

        float blink = 0f;
        bool done = false;
        while (!done)
        {
            foreach (char c in Input.inputString)
            {
                if (c == '\b')
                {
                    if (typed.Length > 0) typed = typed.Substring(0, typed.Length - 1);
                }
                else if (c == '\n' || c == '\r')
                {
                    if (typed.Trim().Length > 0) { done = true; break; }
                }
                else if (!char.IsControl(c) && c != '<' && c != '>' && typed.Length < MaxNameLength)
                {
                    typed += c;
                }
            }

            // Editör oynatma testleri klavye simüle edemediği için bu bayrağı reflection ile açar
            if (submitRequested && typed.Trim().Length > 0) done = true;
            if (done) break;
            blink += Time.unscaledDeltaTime;
            RefreshName(blink % 1.06f < 0.53f);
            yield return null;
        }

        string finalName = typed.Trim();
        RefreshName(false);
        cueText.gameObject.SetActive(false);
        onSubmitted?.Invoke(finalName);

        yield return WaitUnscaled(0.35f);
        stamp.SetActive(true);
        yield return WaitUnscaled(1.0f);

        float t = 0f;
        while (t < 1.6f)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / 1.6f);
            blackout.color = new Color(0, 0, 0, k * k);
            yield return null;
        }
        blackout.color = Color.black;
    }

    private void RefreshName(bool caret)
    {
        if (typed.Length == 0)
        {
            nameText.color = PlaceholderInk;
            nameText.text = (caret ? "|" : "<alpha=#00>|<alpha=#FF>") + placeholder;
        }
        else
        {
            nameText.color = BlueInk;
            nameText.text = typed + (caret ? "|" : "<alpha=#00>|<alpha=#FF>");
        }
    }

    private static IEnumerator WaitUnscaled(float seconds)
    {
        float end = Time.unscaledTime + seconds;
        while (Time.unscaledTime < end) yield return null;
    }

    // ════════════════════════════════════════════
    // UI KURULUMU
    // ════════════════════════════════════════════

    private void Build()
    {
        handFont = LoadFont("Fonts/Caveat-Regular");
        handFontBold = LoadFont("Fonts/Caveat-SemiBold");
        monoFont = LoadFont("Fonts/PTMono-Regular");
        placeholder = DinkLang.T("write your name", "adınızı yazın", "Namen eintragen", "écrivez votre nom", "escriba su nombre", "escreva seu nome", "впишите имя");

        canvas = gameObject.AddComponent<Canvas>();
        RectTransform pageRect = BuildDeskAndPage(gameObject);
        BuildHeader(pageRect);
        float tableBottom = BuildTable(pageRect);
        BuildFooter(pageRect, tableBottom);
        AddLampFalloff(pageRect);

        cueText = NewText("Cue", transform, monoFont, 14, new Color(0.42f, 0.38f, 0.34f), "ENTER");
        cueText.characterSpacing = 25;
        cueText.alignment = TextAlignmentOptions.Center;
        RectTransform cue = cueText.rectTransform;
        cue.anchorMin = cue.anchorMax = new Vector2(0.5f, 0f);
        cue.anchoredPosition = new Vector2(0, 42);
        cue.sizeDelta = new Vector2(400, 30);

        blackout = NewImage("Blackout", transform, new Color(0, 0, 0, 0));
        Stretch(blackout.rectTransform);
    }

    /// <summary>Tuvali, masayı, yaprak kalınlığını, sayfayı ve cilt deliklerini kurar; sayfanın RectTransform'unu döner. Skor defteri de bunu kullanır.</summary>
    internal static RectTransform BuildDeskAndPage(GameObject root)
    {
        Canvas canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 400;
        CanvasScaler scaler = root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1600, 900);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 1f;
        Transform t = root.transform;

        // Masa ve lamba ışığı
        RawImage desk = NewRaw("Desk", t, DeskTexture());
        Stretch(desk.rectTransform);

        // Sayfanın altındaki yaprak kalınlığı + gölge
        RectTransform shadow = NewImage("PageShadow", t, new Color(0, 0, 0, 0.55f)).rectTransform;
        Place(shadow, new Vector2(0, -24), new Vector2(PageW + 40, PageH + 40), -0.8f);
        for (int i = 3; i >= 1; i--)
        {
            float shade = 0.81f - i * 0.05f;
            RectTransform leaf = NewImage("Leaf" + i, t, new Color(shade, shade * 0.97f, shade * 0.88f)).rectTransform;
            Place(leaf, new Vector2(0, -2f * i), new Vector2(PageW, PageH), -0.8f);
        }

        RawImage page = NewRaw("Page", t, GrainTexture());
        page.color = PaperColor;
        RectTransform pageRect = page.rectTransform;
        Place(pageRect, Vector2.zero, new Vector2(PageW, PageH), -0.8f);
        BuildBindingHoles(pageRect);
        return pageRect;
    }

    /// <summary>Lamba düşüşü (sayfa kenarları kararır); sayfadaki her şeyden sonra eklenmeli.</summary>
    internal static void AddLampFalloff(RectTransform pageRect)
    {
        RawImage falloff = NewRaw("LampFalloff", pageRect, FalloffTexture());
        Stretch(falloff.rectTransform);
    }

    private void BuildHeader(RectTransform page)
    {
        float contentW = PageW - PadL - PadR;

        TextMeshProUGUI title = NewText("Title", page, null, 30, PrintColor,
            DinkLang.T("VISITOR LOG", "GİRİŞ – ÇIKIŞ DEFTERİ", "BESUCHERBUCH", "REGISTRE DES VISITEURS", "REGISTRO DE VISITAS", "LIVRO DE VISITAS", "ЖУРНАЛ ПОСЕТИТЕЛЕЙ"));
        title.fontStyle = FontStyles.Bold;
        title.characterSpacing = 8;
        title.enableAutoSizing = true;
        title.fontSizeMin = 20;
        title.fontSizeMax = 30;
        title.textWrappingMode = TextWrappingModes.NoWrap;
        TopLeft(title.rectTransform, PadL, PadT, contentW * 0.72f, 36);

        TextMeshProUGUI sub = NewText("Subtitle", page, null, 17, PrintColor,
            DinkLang.T("Karanfil Apts. · Front Desk", "Karanfil Apt. · Kapıcı Masası", "Haus Karanfil · Pförtnerloge", "Résidence Karanfil · Loge",
                       "Edificio Karanfil · Portería", "Edifício Karanfil · Portaria", "Дом «Каранфил» · Вахта"));
        sub.textWrappingMode = TextWrappingModes.NoWrap;
        TopLeft(sub.rectTransform, PadL, PadT + 40, contentW * 0.72f, 24);

        TextMeshProUGUI meta = NewText("Meta", page, monoFont, 13, PrintColor,
            DinkLang.T("Form G-2 · Vol. 4\nPage 23", "Form G-2 · Cilt 4\nSayfa 23", "Formular G-2 · Bd. 4\nSeite 23", "Formulaire G-2 · Vol. 4\nPage 23",
                       "Formulario G-2 · Vol. 4\nPágina 23", "Formulário G-2 · Vol. 4\nPágina 23", "Форма Г-2 · Том 4\nСтр. 23"));
        meta.alignment = TextAlignmentOptions.BottomRight;
        meta.lineSpacing = 10;
        TopLeft(meta.rectTransform, PadL + contentW * 0.72f, PadT, contentW * 0.28f, 64);

        RectTransform rule = NewImage("HeaderRule", page, PrintColor).rectTransform;
        TopLeft(rule, PadL, PadT + 72, contentW, 3);
    }

    private float BuildTable(RectTransform page)
    {
        float contentW = PageW - PadL - PadR;
        float top = PadT + 72 + 3 + 18;

        string[] headers =
        {
            DinkLang.T("DATE", "TARİH", "DATUM", "DATE", "FECHA", "DATA", "ДАТА"),
            DinkLang.T("TIME", "SAAT", "UHRZEIT", "HEURE", "HORA", "HORA", "ВРЕМЯ"),
            DinkLang.T("FULL NAME", "AD SOYAD", "VOR- UND NACHNAME", "NOM ET PRÉNOM", "NOMBRE Y APELLIDO", "NOME COMPLETO", "ФАМИЛИЯ, ИМЯ"),
            DinkLang.T("FLAT", "DAİRE", "WOHNUNG", "APPT.", "PISO", "APTO.", "КВ.")
        };

        // (tarih, saat, ad, daire, mürekkep stili) — 0: mavi tükenmez, 1: siyah, 2: kurşun kalem (oyuncu satırı)
        var rows = new (string d, string t, string n, string f, int ink)[]
        {
            ("13.11.23", "21:58", "Murat Kaya", "4", 0),
            ("14.11.23", "22:10", "Zeynep Aydın", "11", 1),
            ("14.11.23", "23:05", "", "11", 0),
            ("14.11.23", "23:42", null, "11", 2),
            ("", "", "", "", -1), ("", "", "", "", -1), ("", "", "", "", -1)
        };

        float[] x = new float[ColumnShares.Length + 1];
        x[0] = PadL;
        for (int i = 0; i < ColumnShares.Length; i++) x[i + 1] = x[i] + contentW * ColumnShares[i];

        float tableH = HeaderRowH + RowH * rows.Length;

        // Kendi satırımızı hafifçe aydınlat
        RectTransform highlight = NewImage("PlayerRow", page, new Color(0.96f, 0.94f, 0.82f, 0.2f)).rectTransform;
        TopLeft(highlight, PadL, top + HeaderRowH + RowH * 3, contentW, RowH);

        // Dikey çizgiler (üst kenarda çizgi yok, başlık doğrudan yatay çizgiye oturur)
        for (int i = 0; i < x.Length; i++)
            TopLeft(NewImage("VRule", page, RuleColor).rectTransform, x[i], top, 1, tableH);
        for (int r = 0; r <= rows.Length; r++)
            TopLeft(NewImage("HRule", page, RuleColor).rectTransform, PadL, top + HeaderRowH + RowH * r, contentW, 1);

        for (int c = 0; c < headers.Length; c++)
        {
            TextMeshProUGUI h = NewText("Header", page, null, 14, PrintColor, headers[c]);
            h.characterSpacing = 6;
            h.alignment = TextAlignmentOptions.BottomLeft;
            h.enableAutoSizing = true;
            h.fontSizeMin = 10;
            h.fontSizeMax = 14;
            TopLeft(h.rectTransform, x[c] + 10, top, x[c + 1] - x[c] - 20, HeaderRowH - 6);
        }

        for (int r = 0; r < rows.Length; r++)
        {
            var row = rows[r];
            if (row.ink < 0) continue;
            float y = top + HeaderRowH + RowH * r;
            string[] cells = { row.d, row.t, row.n, row.f };
            for (int c = 0; c < cells.Length; c++)
            {
                if (row.ink == 2 && c == 2)
                {
                    nameText = NewText("PlayerName", page, handFontBold, NameMaxSize, BlueInk, "");
                    nameText.alignment = TextAlignmentOptions.BottomLeft;
                    nameText.textWrappingMode = TextWrappingModes.NoWrap;
                    nameText.enableAutoSizing = true;
                    nameText.fontSizeMin = NameMinSize;
                    nameText.fontSizeMax = NameMaxSize;
                    nameText.margin = new Vector4(0, 0, 0, 6);
                    TopLeft(nameText.rectTransform, x[c] + 10, y, x[c + 1] - x[c] - 20, RowH);
                    continue;
                }
                if (string.IsNullOrEmpty(cells[c])) continue;
                TextMeshProUGUI cell = HandCell(page, cells[c], row.ink, r * 7 + c);
                TopLeft(cell.rectTransform, x[c] + 10, y, x[c + 1] - x[c] - 20, RowH);
            }
        }

        return top + tableH;
    }

    private TextMeshProUGUI HandCell(RectTransform page, string text, int ink, int seed)
    {
        TMP_FontAsset font = ink == 1 ? handFontBold : handFont;
        Color color = ink == 0 ? BlueInk : ink == 1 ? BlackInk : PencilInk;
        TextMeshProUGUI t = NewText("Entry", page, font, ink == 1 ? 31 : 30, color, text);
        t.alignment = TextAlignmentOptions.BottomLeft;
        t.textWrappingMode = TextWrappingModes.NoWrap;
        t.overflowMode = TextOverflowModes.Truncate;
        t.margin = new Vector4(0, 0, 0, 6);
        // Her kalem biraz farklı eğik yazar
        float tilt = ink == 0 ? 1f : ink == 1 ? -0.6f : 0f;
        t.rectTransform.pivot = new Vector2(0, 0);
        t.rectTransform.localRotation = Quaternion.Euler(0, 0, tilt + ((seed * 37) % 7 - 3) * 0.15f);
        return t;
    }

    private void BuildFooter(RectTransform page, float tableBottom)
    {
        float contentW = PageW - PadL - PadR;
        TextMeshProUGUI rule = NewText("Footer", page, monoFont, 13, new Color(0.43f, 0.47f, 0.5f),
            DinkLang.T("All visitors must sign in on arrival.", "Ziyaretçiler girişte kayıt yaptırmak zorundadır.", "Besucher müssen sich beim Betreten eintragen.",
                       "Les visiteurs doivent s'inscrire à l'entrée.", "Los visitantes deben registrarse al entrar.", "Os visitantes devem registrar-se na entrada.",
                       "Посетители обязаны отмечаться при входе."));
        rule.alignment = TextAlignmentOptions.BottomLeft;
        TopLeft(rule.rectTransform, PadL, PageH - PadB - 24, contentW * 0.65f, 24);

        stamp = new GameObject("Stamp", typeof(RectTransform));
        stamp.transform.SetParent(page, false);
        TextMeshProUGUI st = NewText("StampText", stamp.transform, null, 22, StampRed,
            DinkLang.T("LOGGED", "KAYDEDİLDİ", "EINGETRAGEN", "ENREGISTRÉ", "REGISTRADO", "REGISTRADO", "ЗАПИСАНО"));
        st.fontStyle = FontStyles.Bold;
        st.characterSpacing = 18;
        st.alignment = TextAlignmentOptions.Center;
        st.textWrappingMode = TextWrappingModes.NoWrap;
        Stretch(st.rectTransform);

        float w = st.GetPreferredValues().x + 40f;
        RectTransform sr = (RectTransform)stamp.transform;
        TopLeft(sr, PageW - PadR - w, PageH - PadB - 44, w, 42);
        sr.localRotation = Quaternion.Euler(0, 0, 5f);
        float b = 3f;
        foreach (var (pos, size) in new[] { (new Vector2(0, 0), new Vector2(w, b)), (new Vector2(0, 42 - b), new Vector2(w, b)),
                                            (new Vector2(0, 0), new Vector2(b, 42)), (new Vector2(w - b, 0), new Vector2(b, 42)) })
            TopLeft(NewImage("StampBorder", stamp.transform, StampRed).rectTransform, pos.x, pos.y, size.x, size.y);
        stamp.SetActive(false);
    }

    private static void BuildBindingHoles(RectTransform page)
    {
        Texture2D dot = DotTexture();
        for (int i = 0; i < 5; i++)
        {
            RawImage hole = NewRaw("Hole", page, dot);
            hole.color = new Color(0.055f, 0.047f, 0.04f);
            TopLeft(hole.rectTransform, 30, 90 + i * 134, 14, 14);
        }
    }

    // ════════════════════════════════════════════
    // YARDIMCILAR
    // ════════════════════════════════════════════

    internal static void Place(RectTransform r, Vector2 pos, Vector2 size, float rotZ)
    {
        r.anchorMin = r.anchorMax = r.pivot = new Vector2(0.5f, 0.5f);
        r.anchoredPosition = pos + new Vector2(0, -10);
        r.sizeDelta = size;
        r.localRotation = Quaternion.Euler(0, 0, rotZ);
    }

    /// <summary>Sayfanın sol-üst köşesine göre piksel konumlandırma (HTML taslağıyla birebir).</summary>
    internal static void TopLeft(RectTransform r, float x, float y, float w, float h)
    {
        Vector2 pivot = r.pivot;
        r.anchorMin = r.anchorMax = new Vector2(0, 1);
        r.sizeDelta = new Vector2(w, h);
        r.anchoredPosition = new Vector2(x + w * pivot.x, -y - h * (1 - pivot.y));
    }

    internal static void Stretch(RectTransform r)
    {
        r.anchorMin = Vector2.zero;
        r.anchorMax = Vector2.one;
        r.offsetMin = r.offsetMax = Vector2.zero;
    }

    internal static Image NewImage(string name, Transform parent, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        Image img = go.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
        return img;
    }

    internal static RawImage NewRaw(string name, Transform parent, Texture tex)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        RawImage img = go.AddComponent<RawImage>();
        img.texture = tex;
        img.raycastTarget = false;
        return img;
    }

    // Not: İsimde "text"/"hint"/"sub" geçmemeli kuralı yalnız oda sahneleri için; bu UI menüde yaşar.
    internal static TextMeshProUGUI NewText(string name, Transform parent, TMP_FontAsset font, float size, Color color, string text)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        TextMeshProUGUI t = go.AddComponent<TextMeshProUGUI>();
        if (font != null) t.font = font;
        t.fontSize = size;
        t.color = color;
        t.text = text;
        t.raycastTarget = false;
        t.richText = true;
        return t;
    }

    internal static TMP_FontAsset LoadFont(string resourcePath)
    {
        // Hazır TMP font dosyası (Resources/Fonts/TMP/<ad> SDF). Fontu oyun içinde
        // CreateFontAsset ile üretmek build'de Unity'yi çökertiyordu.
        string fileName = resourcePath.Substring(resourcePath.LastIndexOf('/') + 1);
        TMP_FontAsset asset = Resources.Load<TMP_FontAsset>($"Fonts/TMP/{fileName} SDF");
        if (asset == null)
        {
            Debug.LogError($"[NicknameLogbookUI] TMP font bulunamadı: Resources/Fonts/TMP/{fileName} SDF");
            return TMP_Settings.defaultFontAsset;
        }
        if (TMP_Settings.defaultFontAsset != null && (asset.fallbackFontAssetTable == null || asset.fallbackFontAssetTable.Count == 0))
            asset.fallbackFontAssetTable = new System.Collections.Generic.List<TMP_FontAsset> { TMP_Settings.defaultFontAsset };
        return asset;
    }

    internal static Color Hex(string hex)
    {
        ColorUtility.TryParseHtmlString("#" + hex, out Color c);
        return c;
    }

    private static Texture2D DeskTexture()
    {
        const int w = 256, h = 144;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
        Color center = Hex("2A241C"), mid = Hex("14110D"), edge = Hex("050404");
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float dx = (x / (float)w - 0.48f) / 0.7f, dy = (y / (float)h - 0.58f) / 0.8f;
                float d = Mathf.Sqrt(dx * dx + dy * dy) * 2f;
                Color c = d < 0.55f ? Color.Lerp(center, mid, d / 0.55f) : Color.Lerp(mid, edge, Mathf.Clamp01((d - 0.55f) / 0.45f));
                tex.SetPixel(x, y, c);
            }
        tex.Apply();
        return tex;
    }

    private static Texture2D GrainTexture()
    {
        const int s = 256;
        Texture2D tex = new Texture2D(s, s, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Repeat };
        System.Random rng = new System.Random(23);
        for (int y = 0; y < s; y++)
            for (int x = 0; x < s; x++)
            {
                float v = 0.95f + (float)rng.NextDouble() * 0.05f;
                tex.SetPixel(x, y, new Color(v, v, v, 1));
            }
        tex.Apply();
        return tex;
    }

    private static Texture2D FalloffTexture()
    {
        const int w = 128, h = 84;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float dx = (x / (float)w - 0.4f) / 0.8f, dy = (y / (float)h - 0.72f) / 0.9f;
                float d = Mathf.Sqrt(dx * dx + dy * dy) * 2f;
                float a = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.4f, 1f, d)) * 0.55f;
                tex.SetPixel(x, y, new Color(0.08f, 0.05f, 0.03f, a));
            }
        tex.Apply();
        return tex;
    }

    private static Texture2D DotTexture()
    {
        const int s = 32;
        Texture2D tex = new Texture2D(s, s, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
        for (int y = 0; y < s; y++)
            for (int x = 0; x < s; x++)
            {
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(s / 2f, s / 2f));
                tex.SetPixel(x, y, new Color(1, 1, 1, Mathf.Clamp01(s / 2f - 1f - d)));
            }
        tex.Apply();
        return tex;
    }
}
