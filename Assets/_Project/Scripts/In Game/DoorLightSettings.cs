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
    /// <summary>Ayar satırının türü.</summary>
    public enum SettingKind { Cycle, Toggle, Slider, Action }

    /// <summary>Görünümden bağımsız ayar satırı verisi; hem bu ekran hem LobbySettings kullanır.</summary>
    public class SettingRow
    {
        public string label;
        public string desc;
        public SettingKind kind;
        public Func<string> value;
        public Func<float> sliderValue;
        public Action<int> change;
    }

    private class Row
    {
        public SettingRow d;
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

    // Ana menüden geçilmeden (editörde doğrudan bir kattan) açılınca SettingsManager hiç kurulmamış olur;
    // menü arayüzü olmadan da çalıştığı için burada kurulur
    private static SettingsManager SM
    {
        get
        {
            if (SettingsManager.instance != null) return SettingsManager.instance;
            SettingsManager found = FindAnyObjectByType<SettingsManager>(FindObjectsInactive.Include);
            if (found != null) return found;
            // Pasif kurulur: aktif olsaydı kendi Update'i ESC'yi yutar, Escmenu de ayarları hep açık sanırdı
            var go = new GameObject("[SettingsManager]");
            go.SetActive(false);
            return go.AddComponent<SettingsManager>();
        }
    }

    /// <summary>Ekranı açar; geri dönülünce onBack çağrılır.</summary>
    private void OnEnable() { LanguageManager.OnLanguageChanged += OnLanguageChanged; }
    private void OnDisable() { LanguageManager.OnLanguageChanged -= OnLanguageChanged; }

    // Dil değişince ekranı yeni dilde baştan kur, aynı sekme ve satırda kal
    private void OnLanguageChanged()
    {
        if (canvas == null) return;
        bool wasOpen = IsOpen;
        int keepTab = tab, keepRow = selected, order = canvas.sortingOrder;
        Destroy(canvas.gameObject);
        canvas = null;
        tabTexts.Clear();
        tabLines.Clear();
        if (!wasOpen) return;
        Build(order);
        canvas.sortingOrder = order;
        SetTab(keepTab);
        Select(Mathf.Clamp(keepRow, 0, tabs[tab].Count - 1));
    }

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
        foreach (List<SettingRow> list in BuildRows(Close))
        {
            var rows = new List<Row>();
            foreach (SettingRow r in list) rows.Add(new Row { d = r });
            tabs.Add(rows);
        }
    }

    /// <summary>Sekme başlıkları (GRAFİK / SES / OYUN), BuildRows ile aynı sırada.</summary>
    public static string[] TabNames()
    {
        return new[] { DinkLang.T("GRAPHICS", "GRAFİK", "GRAFIK", "GRAPHISMES", "GRÁFICOS", "GRÁFICOS", "ГРАФИКА", "画面", "グラフィック", "그래픽"), DinkLang.T("AUDIO", "SES", "AUDIO", "AUDIO", "AUDIO", "ÁUDIO", "ЗВУК", "音频", "サウンド", "오디오"), DinkLang.T("GAME", "OYUN", "SPIEL", "JEU", "JUEGO", "JOGO", "ИГРА", "游戏", "ゲーム", "게임") };
    }

    /// <summary>SettingsManager'ı hazırlar (yoksa kurar, ayarları yükler).</summary>
    public static void PrepareSettings()
    {
        if (SM != null) SM.PrepareForExternalUI();
    }

    /// <summary>Tüm ayar satırlarını sekme sekme kurar; GERİ satırı onBack'i çağırır.</summary>
    public static List<List<SettingRow>> BuildRows(Action onBack)
    {
        var tabs = new List<List<SettingRow>>();
        tabs.Add(new List<SettingRow>
        {
            new SettingRow { label = DinkLang.T("QUALITY", "KALİTE", "QUALITÄT", "QUALITÉ", "CALIDAD", "QUALIDADE", "КАЧЕСТВО", "画质", "画質", "품질"), desc = DinkLang.T("Shadow and lighting detail.", "Gölge ve ışık detayını belirler.", "Schatten- und Lichtdetails.", "Détail des ombres et de la lumière.", "Detalle de sombras e iluminación.", "Detalhe de sombras e iluminação.", "Детализация теней и света.", "阴影与光照的细节。", "影と光の細かさ。", "그림자와 조명의 세밀함."), kind = SettingKind.Cycle,
                value = () => SM != null ? QualityName(SM.QualityIndex) : "-", change = d => SM?.ChangeQuality(d) },
            new SettingRow { label = DinkLang.T("DISPLAY MODE", "EKRAN MODU", "ANZEIGEMODUS", "MODE D'AFFICHAGE", "MODO DE PANTALLA", "MODO DE TELA", "РЕЖИМ ЭКРАНА", "显示模式", "表示モード", "화면 모드"), desc = DinkLang.T("How the game fills the screen.", "Oyunun ekranı nasıl kaplayacağı.", "Wie das Spiel den Bildschirm füllt.", "Comment le jeu remplit l'écran.", "Cómo ocupa el juego la pantalla.", "Como o jogo ocupa a tela.", "Как игра заполняет экран.", "游戏如何占据屏幕。", "ゲームを画面にどう映すか。", "게임이 화면을 채우는 방식."), kind = SettingKind.Toggle,
                value = () => SM != null && SM.IsFullscreen ? DinkLang.T("FULLSCREEN", "TAM EKRAN", "VOLLBILD", "PLEIN ÉCRAN", "PANTALLA COMPLETA", "TELA CHEIA", "ПОЛНЫЙ ЭКРАН", "全屏", "フルスクリーン", "전체 화면") : DinkLang.T("WINDOWED", "PENCERE", "FENSTER", "FENÊTRÉ", "VENTANA", "JANELA", "ОКНО", "窗口", "ウィンドウ", "창 모드"), change = d => SM?.ToggleFullscreen() },
            new SettingRow { label = DinkLang.T("RESOLUTION", "ÇÖZÜNÜRLÜK", "AUFLÖSUNG", "RÉSOLUTION", "RESOLUCIÓN", "RESOLUÇÃO", "РАЗРЕШЕНИЕ", "分辨率", "解像度", "해상도"), desc = DinkLang.T("Screen resolution.", "Ekran çözünürlüğü.", "Bildschirmauflösung.", "Résolution de l'écran.", "Resolución de pantalla.", "Resolução da tela.", "Разрешение экрана.", "屏幕分辨率。", "画面の解像度。", "화면 해상도."), kind = SettingKind.Cycle,
                value = () => SM != null ? SM.ResolutionName : "-", change = d => SM?.ChangeResolution(d) },
            new SettingRow { label = DinkLang.T("FPS LIMIT", "FPS SINIRI", "FPS-LIMIT", "LIMITE FPS", "LÍMITE FPS", "LIMITE FPS", "ЛИМИТ FPS", "帧率上限", "FPS上限", "FPS 제한"), desc = DinkLang.T("Maximum frames per second.", "Saniyedeki kare üst sınırı.", "Maximale Bilder pro Sekunde.", "Images par seconde maximum.", "Fotogramas por segundo máximos.", "Quadros por segundo máximos.", "Максимум кадров в секунду.", "每秒最大帧数。", "1秒あたりの最大フレーム数。", "초당 최대 프레임 수."), kind = SettingKind.Cycle,
                value = () => SM != null ? FrameRateName(SM.FrameRateIndex) : "-", change = d => SM?.ChangeFrameRate(d) },
            new SettingRow { label = "V-SYNC", desc = DinkLang.T("Prevents screen tearing.", "Ekran yırtılmasını önler.", "Verhindert Bildreißen.", "Empêche le déchirement de l'image.", "Evita el desgarro de pantalla.", "Evita o rasgo de tela.", "Убирает разрывы изображения.", "防止画面撕裂。", "画面のちらつきを防ぐ。", "화면 찢김을 막는다."), kind = SettingKind.Toggle,
                value = () => SM != null && SM.IsVSync ? DinkLang.T("ON", "AÇIK", "AN", "OUI", "SÍ", "SIM", "ВКЛ", "开", "オン", "켜짐") : DinkLang.T("OFF", "KAPALI", "AUS", "NON", "NO", "NÃO", "ВЫКЛ", "关", "オフ", "꺼짐"), change = d => SM?.ToggleVSync() },
        });
        tabs.Add(new List<SettingRow>
        {
            VolumeRow(DinkLang.T("MASTER", "GENEL SES", "GESAMT", "GÉNÉRAL", "GENERAL", "GERAL", "ОБЩАЯ", "总音量", "全体", "전체"), DinkLang.T("Overall volume.", "Tüm seslerin seviyesi.", "Gesamtlautstärke.", "Volume général.", "Volumen general.", "Volume geral.", "Общая громкость.", "全部声音的音量。", "全体の音量。", "전체 음량."),
                () => AudioManager.instance != null ? AudioManager.instance.GetMasterVolume() : AudioListener.volume, v => SM?.SetMasterVolume(v)),
            VolumeRow(DinkLang.T("MUSIC", "MÜZİK", "MUSIK", "MUSIQUE", "MÚSICA", "MÚSICA", "МУЗЫКА", "音乐", "音楽", "음악"), DinkLang.T("Background music.", "Arka plan müziği.", "Hintergrundmusik.", "Musique de fond.", "Música de fondo.", "Música de fundo.", "Фоновая музыка.", "背景音乐。", "背景の音楽。", "배경 음악."),
                () => AudioManager.instance != null ? AudioManager.instance.GetMusicVolume() : PlayerPrefs.GetFloat("Dink_MusicVolume", 0.5f), v => SM?.SetMusicVolume(v)),
            VolumeRow(DinkLang.T("EFFECTS", "EFEKTLER", "EFFEKTE", "EFFETS", "EFECTOS", "EFEITOS", "ЭФФЕКТЫ", "音效", "効果音", "효과음"), DinkLang.T("Doors, footsteps and whispers.", "Kapı, adım ve fısıltılar.", "Türen, Schritte und Flüstern.", "Portes, pas et chuchotements.", "Puertas, pasos y susurros.", "Portas, passos e sussurros.", "Двери, шаги и шёпот.", "门声、脚步声和低语。", "扉、足音、ささやき。", "문소리, 발소리, 속삭임."),
                () => AudioManager.instance != null ? AudioManager.instance.GetSFXVolume() : PlayerPrefs.GetFloat("Dink_SFXVolume", 0.8f), v => SM?.SetSFXVolume(v)),
        });
        tabs.Add(new List<SettingRow>
        {
            new SettingRow { label = DinkLang.T("LANGUAGE", "DİL", "SPRACHE", "LANGUE", "IDIOMA", "IDIOMA", "ЯЗЫК", "语言", "言語", "언어"), desc = DinkLang.T("Interface and letter language.", "Arayüz ve mektup dili.", "Sprache von Oberfläche und Briefen.", "Langue de l'interface et des lettres.", "Idioma de la interfaz y las cartas.", "Idioma da interface e das cartas.", "Язык интерфейса и писем.", "界面与信件的语言。", "画面と手紙の言語。", "화면과 편지의 언어."), kind = SettingKind.Cycle,
                value = () => LanguageManager.instance != null ? LanguageManager.instance.GetCurrentLanguageName().ToUpperInvariant() : "-",
                change = d => { if (LanguageManager.instance != null) { int n = System.Enum.GetValues(typeof(LanguageManager.Language)).Length; LanguageManager.instance.SetLanguage((LanguageManager.Language)(((int)LanguageManager.instance.CurrentLanguage + (d < 0 ? -1 : 1) + n) % n)); } } },
            new SettingRow { label = DinkLang.T("RESTORE DEFAULTS", "VARSAYILANA DÖN", "STANDARD", "PAR DÉFAUT", "RESTABLECER", "PADRÃO", "ПО УМОЛЧАНИЮ", "恢复默认", "初期設定に戻す", "기본값 복원"), desc = DinkLang.T("Resets every setting.", "Tüm ayarları sıfırlar.", "Setzt alle Einstellungen zurück.", "Réinitialise tous les réglages.", "Restablece todos los ajustes.", "Redefine todas as configurações.", "Сбрасывает все настройки.", "重置所有设置。", "すべての設定を元に戻す。", "모든 설정을 초기화한다."), kind = SettingKind.Action,
                value = () => "", change = d => SM?.ResetToDefaults() },
            new SettingRow { label = DinkLang.T("BACK", "GERİ", "ZURÜCK", "RETOUR", "VOLVER", "VOLTAR", "НАЗАД", "返回", "戻る", "뒤로"), desc = DinkLang.T("Returns to the previous menu.", "Önceki menüye döner.", "Zurück zum vorherigen Menü.", "Retour au menu précédent.", "Vuelve al menú anterior.", "Volta ao menu anterior.", "Возврат в предыдущее меню.", "回到上一个菜单。", "前のメニューに戻る。", "이전 메뉴로 돌아간다."), kind = SettingKind.Action,
                value = () => "", change = d => onBack?.Invoke() },
        });
        return tabs;
    }

    private static string QualityName(int i)
    {
        string[] n = { DinkLang.T("LOW", "DÜŞÜK", "NIEDRIG", "BAS", "BAJO", "BAIXO", "НИЗКО", "低", "低", "낮음"), DinkLang.T("MEDIUM", "ORTA", "MITTEL", "MOYEN", "MEDIO", "MÉDIO", "СРЕДНЕ", "中", "中", "중간"),
                       DinkLang.T("HIGH", "YÜKSEK", "HOCH", "ÉLEVÉ", "ALTO", "ALTO", "ВЫСОКО", "高", "高", "높음"), DinkLang.T("ULTRA", "ULTRA", "ULTRA", "ULTRA", "ULTRA", "ULTRA", "УЛЬТРА", "极高", "最高", "최고") };
        return n[SettingsManager.QualityLabelIndex(i)];
    }

    private static string FrameRateName(int i)
    {
        string[] n = { "30", "60", "120", DinkLang.T("UNLIMITED", "SINIRSIZ", "UNBEGRENZT", "ILLIMITÉ", "ILIMITADO", "ILIMITADO", "БЕЗ ЛИМИТА", "无限制", "無制限", "무제한") };
        return n[Mathf.Clamp(i, 0, n.Length - 1)];
    }

    private static SettingRow VolumeRow(string label, string desc, Func<float> get, Action<float> set)
    {
        return new SettingRow
        {
            label = label, desc = desc, kind = SettingKind.Slider, sliderValue = get,
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

        NewText(canvasObj.transform, "Title", DinkLang.T("SETTINGS", "AYARLAR", "EINSTELLUNGEN", "PARAMÈTRES", "AJUSTES", "CONFIGURAÇÕES", "НАСТРОЙКИ", "设置", "設定", "설정"), 26, new Color(1f, 1f, 1f, 0.4f), 20f,
            new Vector2(ContentX, 330f), new Vector2(600f, 40f));

        for (int i = 0; i < 3; i++)
        {
            int index = i;
            string[] names = TabNames();
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

            row.labelText = NewText(rowObj.transform, "Label", row.d.label, 26, normalColor, 6f, Vector2.zero, new Vector2(RowWidth, RowHeight));
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
        if (row.d.kind == SettingKind.Action && delta < 0) return;
        row.d.change?.Invoke(delta);
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
            row.valueText.text = FormatValue(row.d, on);
        }
        descText.text = rows[selected].d.desc;
    }

    private static string FormatValue(SettingRow row, bool selected)
    {
        if (row.kind == SettingKind.Slider)
        {
            int filled = Mathf.RoundToInt(row.sliderValue() * 10f);
            return $"<color=#FFFFFF>{new string('■', filled)}</color><color=#FFFFFF2E>{new string('■', 10 - filled)}</color>  {filled * 10,3}";
        }
        string v = row.value();
        if (row.kind == SettingKind.Action) return selected ? "→" : "";
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
