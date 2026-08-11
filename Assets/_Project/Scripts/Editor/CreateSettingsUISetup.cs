using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Unity Editor Otomatik Kurulum Aracı:
/// Dink Tools -> AAA Ayarlar Menusu UI Kur
/// 
/// AAA seviyesindeki ayar menüsü arayüzünü sahneye kurar.
/// ÖNEMLİ: Bu araç sadece UI hiyerarşisini oluşturur ve SettingsManager'a
/// serialized referanslar atar. Buton/Slider bağlantıları SettingsManager.Start()
/// içinde runtime'da kurulur — böylece Play modunda kesin çalışır.
/// </summary>
public class CreateSettingsUISetup : Editor
{
    [MenuItem("Dink Tools/AAA Ayarlar Menusu UI Kur")]
    public static void SetupAAAUI()
    {
        // 1. Varsa eski canvas'ı temizle
        GameObject canvasObj = GameObject.Find("[AAA AYARLAR MENÜSÜ (CANVAS)]");
        if (canvasObj != null)
            Undo.DestroyObjectImmediate(canvasObj);

        // 2. Canvas Oluştur (Sorting Order: 10)
        canvasObj = new GameObject("[AAA AYARLAR MENÜSÜ (CANVAS)]");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        // EventSystem Güvencesi
        if (FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        // 3. SettingsManager Bileşeni Ekle
        SettingsManager sm = canvasObj.AddComponent<SettingsManager>();

        // 4. Ana Koyu Panel
        GameObject mainPanel = MakePanel("MainSettingsPanel", canvasObj.transform,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero,
            new Vector2(1400, 800), new Color(0.06f, 0.06f, 0.08f, 0.94f), false);

        // 5. Üst Başlık
        MakeText("HeaderTitle", mainPanel.transform,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -40), new Vector2(600, 60),
            "AYARLAR", 36, TextAlignmentOptions.Center, Color.white, FontStyles.Bold);

        // 6. Sol Ayar Satırları Listesi
        GameObject leftList = MakePanel("LeftListPanel", mainPanel.transform,
            new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(50, 20), new Vector2(780, 660),
            Color.clear, false);
        VerticalLayoutGroup vlg = leftList.AddComponent<VerticalLayoutGroup>();
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.spacing = 10;

        // ═══════════ GÖRÜNTÜ SATIRLARI ═══════════

        // Grafik Kalitesi
        var qualityRow = MakeStepperRow(leftList.transform, "Grafik Kalitesi",
            "Grafik Kalitesi", "Genel görsel doku, gölge ve efekt detay seviyesini ayarlar.", sm,
            out Button qL, out Button qR, out TMP_Text qVal);
        sm.qualityLeftBtn = qL;
        sm.qualityRightBtn = qR;
        sm.qualityValueText = qVal;

        // Ekran Modu (sadece tek buton — tıklayınca toggle)
        var displayRow = MakeStepperRow(leftList.transform, "Ekran Modu",
            "Ekran Modu", "Tam ekran veya pencereli görüntüleme modunu seçer.", sm,
            out Button dmL, out Button dmR, out TMP_Text dmVal);
        sm.displayModeBtn = dmL;
        sm.displayModeRightBtn = dmR;
        sm.displayModeValueText = dmVal;

        // Çözünürlük
        var resRow = MakeStepperRow(leftList.transform, "Çözünürlük",
            "Ekran Çözünürlüğü", "Monitörünüzün çözünürlük piksel değerini ayarlar.", sm,
            out Button rL, out Button rR, out TMP_Text rVal);
        sm.resolutionLeftBtn = rL;
        sm.resolutionRightBtn = rR;
        sm.resolutionValueText = rVal;

        // Maksimum FPS
        var fpsRow = MakeStepperRow(leftList.transform, "Maksimum FPS",
            "Maksimum Kare Hızı", "Oyunun kare hızı (FPS) sınırını belirler.", sm,
            out Button fL, out Button fR, out TMP_Text fVal);
        sm.frameRateLeftBtn = fL;
        sm.frameRateRightBtn = fR;
        sm.frameRateValueText = fVal;

        // V-Sync
        var vsyncRow = MakeStepperRow(leftList.transform, "V-Sync",
            "Dikey Eşitleme (V-Sync)", "Ekran yırtılmalarını önlemek için dikey eşitlemeyi açar.", sm,
            out Button vL, out Button vR, out TMP_Text vVal);
        sm.vsyncBtn = vL;
        sm.vsyncRightBtn = vR;
        sm.vsyncValueText = vVal;

        // ═══════════ SES SATIRLARI ═══════════

        sm.masterVolumeSlider = MakeSliderRow(leftList.transform, "Genel Ses",
            "Genel Ses Seviyesi", "Oyunun tüm seslerinin genel şiddetini ayarlar.", sm,
            out TMP_Text masterPct);
        sm.masterVolumePercentText = masterPct;

        sm.musicVolumeSlider = MakeSliderRow(leftList.transform, "Müzik Sesi",
            "Müzik Seviyesi", "Arka plan gerilim müziğinin ses seviyesini ayarlar.", sm,
            out TMP_Text musicPct);
        sm.musicVolumePercentText = musicPct;

        sm.sfxVolumeSlider = MakeSliderRow(leftList.transform, "Efekt Sesi",
            "Ses Efektleri", "Kapı gıcırtıları, adımlar ve etkileşim seslerinin seviyesini ayarlar.", sm,
            out TMP_Text sfxPct);
        sm.sfxVolumePercentText = sfxPct;

        // 7. Sağ Taraf Canlı Açıklama Paneli
        GameObject descPanel = MakePanel("RightDescriptionPanel", mainPanel.transform,
            new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-40, 20), new Vector2(480, 660),
            new Color(0.1f, 0.1f, 0.13f, 0.85f), false);

        sm.descriptionTitleText = MakeText("DescTitle", descPanel.transform,
            new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(25, -45), new Vector2(430, 45),
            "AYARLAR", 26, TextAlignmentOptions.Left, new Color(0.95f, 0.35f, 0.35f), FontStyles.Bold);

        sm.descriptionBodyText = MakeText("DescBody", descPanel.transform,
            new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(25, -120), new Vector2(430, 480),
            "Detaylarını görmek için bir ayarın üzerine gelin.", 20,
            TextAlignmentOptions.TopLeft, new Color(0.85f, 0.85f, 0.85f), FontStyles.Normal);

        // 8. Alt Bar
        GameObject bottomBar = MakePanel("BottomActionBar", mainPanel.transform,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 35), new Vector2(1300, 50),
            Color.clear, false);

        // Varsayılana Dön butonu
        sm.resetButton = MakeButton("Btn_Reset", bottomBar.transform,
            new Vector2(1f, 0.5f), new Vector2(-220, 0), new Vector2(200, 45), "VARSAYILANA DÖN");

        // Geri Dön butonu
        sm.backButton = MakeButton("Btn_Back", bottomBar.transform,
            new Vector2(1f, 0.5f), new Vector2(-10, 0), new Vector2(160, 45), "GERİ DÖN");

        // 9. MainMenu3DController Bağlantısı
        MainMenu3DController mainCtrl = FindFirstObjectByType<MainMenu3DController>();
        if (mainCtrl != null)
        {
            mainCtrl.graphicsPanel = canvasObj;
            mainCtrl.audioPanel = canvasObj;
            EditorUtility.SetDirty(mainCtrl);
        }

        // 10. Başlangıçta Canvas kapalı kalsın
        canvasObj.SetActive(false);

        Undo.RegisterCreatedObjectUndo(canvasObj, "Create AAA Settings UI");
        EditorUtility.SetDirty(canvasObj);

        Debug.Log("<color=green>[DINK] AAA Ayarlar Menüsü UI kuruldu! Tüm bağlantılar runtime'da SettingsManager.Start() ile aktif olacak.</color>");
    }

    // ════════════════════════════════════════════
    // STEPPER SATIRI (< DEĞER >) — Buton referanslarını out ile döndürür
    // ════════════════════════════════════════════

    private static GameObject MakeStepperRow(Transform parent, string label,
        string descTitle, string descBody, SettingsManager sm,
        out Button leftBtn, out Button rightBtn, out TMP_Text valueText)
    {
        GameObject row = MakePanel("Row_" + label, parent,
            Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(780, 55),
            new Color(0.14f, 0.14f, 0.18f, 0.7f), true);
        row.AddComponent<LayoutElement>().preferredHeight = 55;

        // Sol Label
        MakeText("Label", row.transform,
            new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(20, 0), new Vector2(350, 40),
            label, 22, TextAlignmentOptions.Left, Color.white, FontStyles.Normal);

        // Stepper Container
        GameObject stepper = MakePanel("Stepper", row.transform,
            new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-20, 0), new Vector2(350, 45),
            Color.clear, false);

        // < Butonu
        leftBtn = MakeButton("Btn_Left", stepper.transform,
            new Vector2(0f, 0.5f), new Vector2(25, 0), new Vector2(40, 40), "<");

        // Değer Metni
        valueText = MakeText("ValueText", stepper.transform,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(220, 40),
            "---", 22, TextAlignmentOptions.Center, new Color(1.0f, 0.45f, 0.35f), FontStyles.Bold);

        // > Butonu
        rightBtn = MakeButton("Btn_Right", stepper.transform,
            new Vector2(1f, 0.5f), new Vector2(-25, 0), new Vector2(40, 40), ">");

        // Hover Açıklama Trigger'ı
        AddHoverTrigger(row, descTitle, descBody, sm);

        return row;
    }

    // ════════════════════════════════════════════
    // SLIDER SATIRI — Slider referansını döndürür
    // ════════════════════════════════════════════

    private static Slider MakeSliderRow(Transform parent, string label,
        string descTitle, string descBody, SettingsManager sm,
        out TMP_Text percentText)
    {
        GameObject row = MakePanel("Row_" + label, parent,
            Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(780, 55),
            new Color(0.14f, 0.14f, 0.18f, 0.7f), true);
        row.AddComponent<LayoutElement>().preferredHeight = 55;

        // Sol Label
        MakeText("Label", row.transform,
            new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(20, 0), new Vector2(300, 40),
            label, 22, TextAlignmentOptions.Left, Color.white, FontStyles.Normal);

        // Slider
        GameObject sliderObj = new GameObject("Slider");
        sliderObj.transform.SetParent(row.transform);
        RectTransform sliderRect = sliderObj.AddComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(1f, 0.5f);
        sliderRect.anchorMax = new Vector2(1f, 0.5f);
        sliderRect.anchoredPosition = new Vector2(-90, 0);
        sliderRect.sizeDelta = new Vector2(260, 25);

        Slider slider = sliderObj.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;

        // Background
        MakePanel("Background", sliderObj.transform,
            new Vector2(0, 0.5f), new Vector2(1, 0.5f), Vector2.zero, new Vector2(260, 10),
            new Color(0.2f, 0.2f, 0.25f), true);

        // Fill Area
        GameObject fillArea = MakePanel("FillArea", sliderObj.transform,
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, Color.clear, false);
        GameObject fill = MakePanel("Fill", fillArea.transform,
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
            new Color(0.95f, 0.35f, 0.35f), true);
        slider.fillRect = fill.GetComponent<RectTransform>();

        // Handle Slide Area (Sürükleme Kulpu)
        GameObject handleArea = MakePanel("HandleArea", sliderObj.transform,
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, Color.clear, false);
        GameObject handle = MakePanel("Handle", handleArea.transform,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(25, 25),
            new Color(1.0f, 0.45f, 0.35f), true);
        slider.handleRect = handle.GetComponent<RectTransform>();
        slider.targetGraphic = handle.GetComponent<Image>();

        // NOT: slider.onValueChanged bağlantısı SettingsManager.Start() içinde runtime'da yapılacak.

        // Percent Text
        percentText = MakeText("PercentText", row.transform,
            new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-15, 0), new Vector2(70, 40),
            "80%", 20, TextAlignmentOptions.Right, Color.white, FontStyles.Bold);

        // Hover Açıklama Trigger'ı
        AddHoverTrigger(row, descTitle, descBody, sm);

        return slider;
    }

    // ════════════════════════════════════════════
    // HOVER TRIGGER — Satır üzerine gelince açıklama panelini günceller
    // ════════════════════════════════════════════

    private static void AddHoverTrigger(GameObject obj, string title, string body, SettingsManager sm)
    {
        EventTrigger trigger = obj.AddComponent<EventTrigger>();

        EventTrigger.Entry entryEnter = new EventTrigger.Entry();
        entryEnter.eventID = EventTriggerType.PointerEnter;
        entryEnter.callback.AddListener((data) => sm.SetDescription(title, body));
        trigger.triggers.Add(entryEnter);

        EventTrigger.Entry entryExit = new EventTrigger.Entry();
        entryExit.eventID = EventTriggerType.PointerExit;
        entryExit.callback.AddListener((data) => sm.ClearDescription());
        trigger.triggers.Add(entryExit);
    }

    // ════════════════════════════════════════════
    // TEMEL BİLEŞEN OLUŞTURUCULAR
    // ════════════════════════════════════════════

    private static GameObject MakePanel(string name, Transform parent,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 size,
        Color color, bool raycastTarget)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent);
        RectTransform rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        if (color != Color.clear || raycastTarget)
        {
            Image img = panel.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = raycastTarget;
        }
        return panel;
    }

    private static TMP_Text MakeText(string name, Transform parent,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 size,
        string text, float fontSize, TextAlignmentOptions align, Color color, FontStyles style)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent);
        RectTransform rt = textObj.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = align;
        tmp.color = color;
        tmp.fontStyle = style;
        tmp.raycastTarget = false;
        return tmp;
    }

    /// <summary>
    /// Buton oluşturur ve Button bileşenini döndürür.
    /// NOT: onClick bağlantısı burada YAPILMAZ — SettingsManager.Start() içinde runtime'da bağlanır.
    /// </summary>
    private static Button MakeButton(string name, Transform parent,
        Vector2 anchor, Vector2 pos, Vector2 size, string text)
    {
        GameObject btnObj = MakePanel(name, parent, anchor, anchor, pos, size,
            new Color(0.25f, 0.25f, 0.32f, 0.95f), true);
        Button btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = btnObj.GetComponent<Image>();

        ColorBlock cb = btn.colors;
        cb.highlightedColor = new Color(0.95f, 0.35f, 0.35f);
        cb.pressedColor = new Color(0.6f, 0.15f, 0.15f);
        btn.colors = cb;

        MakeText("Text", btnObj.transform,
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
            text, 18, TextAlignmentOptions.Center, Color.white, FontStyles.Bold);

        return btn;
    }
}
