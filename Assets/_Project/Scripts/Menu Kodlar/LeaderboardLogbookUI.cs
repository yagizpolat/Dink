using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using L = NicknameLogbookUI;

/// <summary>
/// Oyun sonu skor tablosu: oyuncunun adını yazdığı giriş-çıkış defterinin bir sonraki sayfası.
/// Buluttan gelen en iyi süreler farklı kalemlerle yazılmış kayıtlar olarak listelenir; oyuncunun kendi satırı vurgulanır.
/// Enter = tekrar gir (yeni oyun), Esc = ana menü. Time.timeScale = 0 iken çalışır.
/// </summary>
public class LeaderboardLogbookUI : MonoBehaviour
{
    private const int MaxRows = 8;
    private const float HeaderRowH = 40f, RowH = 54f;
    private static readonly float[] ColumnShares = { 0.1f, 0.46f, 0.22f, 0.22f };

    private TMP_FontAsset handFont, handFontBold, monoFont;
    private RectTransform page;
    private readonly List<GameObject> rowObjects = new List<GameObject>();
    private float[] colX;
    private float tableTop;
    private Image blackout;
    private bool leaving;
    private string playerName;
    private float playerTime;

    public static void Show(float currentScoreTime, string currentPlayerName)
    {
        GameObject existing = GameObject.Find("[SKOR DEFTERİ]");
        if (existing != null) Destroy(existing);

        var ui = new GameObject("[SKOR DEFTERİ]").AddComponent<LeaderboardLogbookUI>();
        ui.playerTime = currentScoreTime;
        ui.playerName = currentPlayerName;
        ui.Build();
    }

    /// <summary>Bulut/yerel liste geldiğinde satırları yazar.</summary>
    public void SetEntries(List<LeaderboardManager.LeaderboardEntry> entries)
    {
        foreach (var go in rowObjects) Destroy(go);
        rowObjects.Clear();

        if (entries == null || entries.Count == 0)
        {
            Cell(0, 1, DinkLang.T("— no one has made it out yet —", "— henüz kimse çıkamadı —", "— noch niemand ist herausgekommen —",
                "— personne n'est encore sorti —", "— nadie ha salido todavía —", "— ninguém saiu ainda —", "— ещё никто не вышел —"), 2, false);
            return;
        }

        int ownIndex = entries.FindIndex(e => e.playerName == playerName && Mathf.Abs(e.timeInSeconds - playerTime) < 0.5f);
        int shown = Mathf.Min(entries.Count, MaxRows);
        bool appendOwn = ownIndex >= MaxRows && playerTime > 0f;
        if (appendOwn) shown = MaxRows - 1;

        for (int i = 0; i < shown; i++)
            WriteRow(i, (i + 1).ToString(), entries[i], i == ownIndex);

        if (appendOwn)
            WriteRow(MaxRows - 1, (ownIndex + 1).ToString(), entries[ownIndex], true);
    }

    private void WriteRow(int row, string rank, LeaderboardManager.LeaderboardEntry e, bool own)
    {
        // Her kayıt başka bir elden: sıraya göre mavi tükenmez / siyah kalem, oyuncunun satırı kurşun kalem değil, kendi mavisi
        int ink = own ? 3 : (row % 3 == 1 ? 1 : 0);
        if (own)
        {
            RectTransform hl = L.NewImage("OwnRow", page, new Color(0.96f, 0.94f, 0.82f, 0.25f)).rectTransform;
            L.TopLeft(hl, L.PadL, tableTop + HeaderRowH + RowH * row, colX[colX.Length - 1] - L.PadL, RowH);
            hl.SetAsFirstSibling();
            hl.SetSiblingIndex(1);
            rowObjects.Add(hl.gameObject);
        }
        Cell(row, 0, rank, ink, own);
        Cell(row, 1, e.playerName, ink, own);
        Cell(row, 2, LeaderboardManager.FormatTime(e.timeInSeconds), ink, own);
        Cell(row, 3, e.dateString, ink, own);
    }

    private void Cell(int row, int col, string text, int ink, bool own)
    {
        TMP_FontAsset font = ink == 1 || own ? handFontBold : handFont;
        Color color = ink == 1 ? L.BlackInk : ink == 2 ? L.PencilInk : L.BlueInk;
        int span = ink == 2 ? colX.Length - 2 : 1;
        TextMeshProUGUI t = L.NewText("Entry", page, font, own ? 30 : 27, color, text);
        t.alignment = TextAlignmentOptions.BottomLeft;
        t.textWrappingMode = TextWrappingModes.NoWrap;
        t.enableAutoSizing = true;
        t.fontSizeMin = 16;
        t.fontSizeMax = own ? 30 : 27;
        t.margin = new Vector4(0, 0, 0, 5);
        t.rectTransform.pivot = Vector2.zero;
        t.rectTransform.localRotation = Quaternion.Euler(0, 0, ink == 1 ? -0.6f : ((row * 5 + col) % 3 - 1) * 0.5f);
        float x0 = colX[col], x1 = colX[Mathf.Min(col + span, colX.Length - 1)];
        L.TopLeft(t.rectTransform, x0 + 10, tableTop + HeaderRowH + RowH * row, x1 - x0 - 20, RowH);
        rowObjects.Add(t.gameObject);
    }

    private void Update()
    {
        if (leaving) return;
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) StartCoroutine(Leave(true));
        else if (Input.GetKeyDown(KeyCode.Escape)) StartCoroutine(Leave(false));
    }

    /// <summary>Kararıp sahneyi değiştirir. Yeni oyunda kronometre baştan başlar (eskiden başlamıyordu, 00:00:00 skor yazılıyordu).</summary>
    private IEnumerator Leave(bool replay)
    {
        leaving = true;
        float t = 0f;
        while (t < 0.9f)
        {
            t += Time.unscaledDeltaTime;
            blackout.color = new Color(0, 0, 0, Mathf.Clamp01(t / 0.9f));
            yield return null;
        }
        Time.timeScale = 1f;
        if (replay)
        {
            if (LeaderboardManager.instance != null) LeaderboardManager.instance.StartTimer();
            DinkScenes.LoadPrologue();
        }
        else
        {
            DinkScenes.LoadMainMenu();
        }
    }

    // ════════════════════════════════════════════
    // UI KURULUMU
    // ════════════════════════════════════════════

    private void Build()
    {
        DontDestroyOnLoad(gameObject); // sahne değişene kadar karartma ekranda kalsın, sonra kendini siler
        handFont = L.LoadFont("Fonts/Caveat-Regular");
        handFontBold = L.LoadFont("Fonts/Caveat-SemiBold");
        monoFont = L.LoadFont("Fonts/PTMono-Regular");

        gameObject.AddComponent<Canvas>();
        page = L.BuildDeskAndPage(gameObject);
        gameObject.AddComponent<GraphicRaycaster>();
        EnsureEventSystem();

        float contentW = L.PageW - L.PadL - L.PadR;

        TextMeshProUGUI title = L.NewText("Title", page, null, 30, L.PrintColor,
            DinkLang.T("VISITOR LOG", "GİRİŞ – ÇIKIŞ DEFTERİ", "BESUCHERBUCH", "REGISTRE DES VISITEURS", "REGISTRO DE VISITAS", "LIVRO DE VISITAS", "ЖУРНАЛ ПОСЕТИТЕЛЕЙ"));
        title.fontStyle = FontStyles.Bold;
        title.characterSpacing = 8;
        title.enableAutoSizing = true;
        title.fontSizeMin = 20;
        title.fontSizeMax = 30;
        title.textWrappingMode = TextWrappingModes.NoWrap;
        L.TopLeft(title.rectTransform, L.PadL, L.PadT, contentW * 0.62f, 36);

        TextMeshProUGUI sub = L.NewText("Subtitle", page, null, 17, L.PrintColor,
            DinkLang.T("Departures · fastest first", "Çıkışlar · en hızlıdan", "Ausgänge · die schnellsten zuerst", "Sorties · les plus rapides d'abord",
                       "Salidas · los más rápidos primero", "Saídas · os mais rápidos primeiro", "Выходы · самые быстрые сверху"));
        sub.textWrappingMode = TextWrappingModes.NoWrap;
        L.TopLeft(sub.rectTransform, L.PadL, L.PadT + 40, contentW * 0.62f, 24);

        TextMeshProUGUI meta = L.NewText("Meta", page, monoFont, 13, L.PrintColor,
            DinkLang.T("Form G-2 · Vol. 4\nPage 24", "Form G-2 · Cilt 4\nSayfa 24", "Formular G-2 · Bd. 4\nSeite 24", "Formulaire G-2 · Vol. 4\nPage 24",
                       "Formulario G-2 · Vol. 4\nPágina 24", "Formulário G-2 · Vol. 4\nPágina 24", "Форма Г-2 · Том 4\nСтр. 24"));
        meta.alignment = TextAlignmentOptions.BottomRight;
        meta.lineSpacing = 10;
        L.TopLeft(meta.rectTransform, L.PadL + contentW * 0.62f, L.PadT, contentW * 0.38f, 64);

        L.TopLeft(L.NewImage("HeaderRule", page, L.PrintColor).rectTransform, L.PadL, L.PadT + 72, contentW, 3);

        BuildTable(contentW);
        BuildExitStamp(contentW);
        BuildFooter(contentW);

        L.AddLampFalloff(page);

        blackout = L.NewImage("Blackout", transform, new Color(0, 0, 0, 0));
        L.Stretch(blackout.rectTransform);

        // Bulut listesi gelene kadar kurşun kalemle bekleme satırı
        Cell(0, 1, DinkLang.T("checking the log…", "defter kontrol ediliyor…", "Buch wird geprüft…", "vérification du registre…",
                              "revisando el registro…", "verificando o livro…", "проверяем журнал…"), 2, false);
    }

    private void BuildTable(float contentW)
    {
        tableTop = L.PadT + 72 + 3 + 16;
        colX = new float[ColumnShares.Length + 1];
        colX[0] = L.PadL;
        for (int i = 0; i < ColumnShares.Length; i++) colX[i + 1] = colX[i] + contentW * ColumnShares[i];
        float tableH = HeaderRowH + RowH * MaxRows;

        for (int i = 0; i < colX.Length; i++)
            L.TopLeft(L.NewImage("VRule", page, L.RuleColor).rectTransform, colX[i], tableTop, 1, tableH);
        for (int r = 0; r <= MaxRows; r++)
            L.TopLeft(L.NewImage("HRule", page, L.RuleColor).rectTransform, L.PadL, tableTop + HeaderRowH + RowH * r, contentW, 1);

        string[] headers =
        {
            "№",
            DinkLang.T("FULL NAME", "AD SOYAD", "VOR- UND NACHNAME", "NOM ET PRÉNOM", "NOMBRE Y APELLIDO", "NOME COMPLETO", "ФАМИЛИЯ, ИМЯ"),
            DinkLang.T("TIME INSIDE", "İÇERİDE KALDI", "DAUER", "DURÉE", "TIEMPO DENTRO", "TEMPO DENTRO", "ВРЕМЯ ВНУТРИ"),
            DinkLang.T("DATE", "TARİH", "DATUM", "DATE", "FECHA", "DATA", "ДАТА")
        };
        for (int c = 0; c < headers.Length; c++)
        {
            TextMeshProUGUI h = L.NewText("Header", page, null, 14, L.PrintColor, headers[c]);
            h.characterSpacing = 6;
            h.alignment = TextAlignmentOptions.BottomLeft;
            h.enableAutoSizing = true;
            h.fontSizeMin = 10;
            h.fontSizeMax = 14;
            L.TopLeft(h.rectTransform, colX[c] + 10, tableTop, colX[c + 1] - colX[c] - 20, HeaderRowH - 6);
        }
    }

    /// <summary>Oyuncunun kendi çıkışı: sağ üstte kırmızı kaşe.</summary>
    private void BuildExitStamp(float contentW)
    {
        if (playerTime <= 0f) return;
        string label = DinkLang.T("OUT", "ÇIKTI", "RAUS", "SORTI", "SALIÓ", "SAIU", "ВЫШЕЛ") + "  " + LeaderboardManager.FormatTime(playerTime);

        GameObject stamp = new GameObject("ExitStamp", typeof(RectTransform));
        stamp.transform.SetParent(page, false);
        TextMeshProUGUI st = L.NewText("StampText", stamp.transform, monoFont, 22, L.StampRed, label);
        st.fontStyle = FontStyles.Bold;
        st.characterSpacing = 6;
        st.alignment = TextAlignmentOptions.Center;
        st.textWrappingMode = TextWrappingModes.NoWrap;
        L.Stretch(st.rectTransform);

        float w = st.GetPreferredValues().x + 36f, h = 42f;
        RectTransform sr = (RectTransform)stamp.transform;
        L.TopLeft(sr, L.PageW - L.PadR - w - 8, L.PageH - L.PadB - h - 4, w, h);
        sr.localRotation = Quaternion.Euler(0, 0, 4f);
        float b = 3f;
        foreach (var (x, y, bw, bh) in new[] { (0f, 0f, w, b), (0f, h - b, w, b), (0f, 0f, b, h), (w - b, 0f, b, h) })
            L.TopLeft(L.NewImage("StampBorder", stamp.transform, L.StampRed).rectTransform, x, y, bw, bh);
    }

    private void BuildFooter(float contentW)
    {
        Color footColor = new Color(0.36f, 0.4f, 0.42f);
        FooterButton(DinkLang.T("[Enter]  Go in again", "[Enter]  Tekrar gir", "[Enter]  Noch einmal hinein", "[Entrée]  Entrer à nouveau",
                                "[Enter]  Entrar de nuevo", "[Enter]  Entrar de novo", "[Enter]  Войти снова"),
                     L.PadL, footColor, () => { if (!leaving) StartCoroutine(Leave(true)); });
        FooterButton(DinkLang.T("[Esc]  Main menu", "[Esc]  Ana menü", "[Esc]  Hauptmenü", "[Échap]  Menu principal",
                                "[Esc]  Menú principal", "[Esc]  Menu principal", "[Esc]  Главное меню"),
                     L.PadL + 300, footColor, () => { if (!leaving) StartCoroutine(Leave(false)); });
    }

    private void FooterButton(string label, float x, Color color, UnityEngine.Events.UnityAction onClick)
    {
        TextMeshProUGUI t = L.NewText("FooterButton", page, monoFont, 16, color, label);
        t.alignment = TextAlignmentOptions.BottomLeft;
        t.textWrappingMode = TextWrappingModes.NoWrap;
        t.raycastTarget = true;
        L.TopLeft(t.rectTransform, x, L.PageH - L.PadB - 30, 280, 30);
        Button b = t.gameObject.AddComponent<Button>();
        b.targetGraphic = t;
        ColorBlock cb = b.colors;
        cb.normalColor = Color.white;
        cb.highlightedColor = new Color(0.55f, 0.35f, 0.3f);
        cb.pressedColor = new Color(0.45f, 0.25f, 0.2f);
        cb.selectedColor = Color.white;
        b.colors = cb;
        b.onClick.AddListener(onClick);
    }

    private static void EnsureEventSystem()
    {
        if (FindAnyObjectByType<EventSystem>() != null) return;
        var es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
        es.AddComponent<StandaloneInputModule>();
    }

    private void OnEnable() => UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene s, UnityEngine.SceneManagement.LoadSceneMode m)
    {
        if (leaving) Destroy(gameObject);
    }
}
