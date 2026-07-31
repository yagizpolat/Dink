using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEditor.Events;
using UnityEngine.Events;
using UnityEngine.Rendering;

public class MenuBuilder : EditorWindow
{
    [MenuItem("Tools/Dink Projesi/Menü Kurulumunu Yap (Menu Builder)")]
    public static void BuildMenu()
    {
        Debug.Log("Menü kurulumu başlıyor...");

        // =====================================================
        // 1. MANAGER OBJESİ
        // NOT: Global Volume KAPATILMIYOR. Kırmızılık sahne atmosferinin parçası.
        // Intro sırasında blackFade örtüyor, menüde koridor görünür.
        // =====================================================
        GameObject managerObj = new GameObject("[--- MENÜ SİSTEMİ ---]");
        IntroManager introManager = managerObj.AddComponent<IntroManager>();
        UIManager uiManager = managerObj.AddComponent<UIManager>();
        SceneTransition sceneTransition = managerObj.AddComponent<SceneTransition>();

        // =====================================================
        // 2. SAHNEDEKİ KAMERA VE KAPILARI BUL
        // =====================================================
        Transform mainCamera = null, leftDoor = null, rightDoor = null;
        foreach (GameObject go in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            Camera cam = go.GetComponentInChildren<Camera>();
            if (cam != null) mainCamera = cam.transform;

            foreach (Transform t in go.GetComponentsInChildren<Transform>(true))
            {
                string n = t.name.ToLower();
                if (n.Contains("left") && (n.Contains("door") || n.Contains("kapı"))) leftDoor = t;
                if (n.Contains("right") && (n.Contains("door") || n.Contains("kapı"))) rightDoor = t;
            }
        }

        introManager.mainCamera = mainCamera;
        introManager.leftDoor = leftDoor;
        introManager.rightDoor = rightDoor;
        introManager.targetZ = 2.5f;

        // =====================================================
        // 3. CANVAS OLUŞTUR
        // =====================================================
        GameObject canvasObj = new GameObject("[--- MENÜ ARAYÜZÜ (CANVAS) ---]");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100; // Her şeyin önünde olsun
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();

        if (Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject esObj = new GameObject("Event System");
            esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // =====================================================
        // UI Z-ORDER SIRASI (Altta oluşturulan arkada kalır)
        // =====================================================

        // --- KATMAN 1: ANA MENÜ PANELİ ---
        GameObject mainMenuUI = CreateMainMenuPanel(canvasObj.transform);

        // --- KATMAN 2: AYARLAR PANELİ ---
        GameObject settingsPanel = CreateSettingsPanel(canvasObj.transform);

        // --- KATMAN 3: SCENE TRANSITION YAZILARI (Günlük) ---
        GameObject quoteGroupObj = CreateUIElement("SceneQuoteGroup", canvasObj.transform);
        SetStretchAll(quoteGroupObj.GetComponent<RectTransform>());
        CanvasGroup quoteTextGroup = quoteGroupObj.AddComponent<CanvasGroup>();
        quoteGroupObj.SetActive(false);

        // Günlük Yazısı
        GameObject journalBg = CreateUIElement("JournalBg", quoteGroupObj.transform);
        SetStretchAll(journalBg.GetComponent<RectTransform>());
        // Arka plan tamamen siyah - blackFade zaten altında
        // (journalBg aslında şeffaf, blackFade'in siyahlığını kullanıyoruz)

        GameObject journalTextObj = CreateUIElement("JournalText", quoteGroupObj.transform);
        Text journalText = journalTextObj.AddComponent<Text>();
        journalText.text = "Bugün yine aynı koridor...\n\nNe kadar denesem de çıkamıyorum.\nSanki zihnimin içinde bir labirentteyim.\n\n— Günlük, 3. Gün";
        journalText.color = new Color(0.9f, 0.85f, 0.8f, 1f);
        journalText.alignment = TextAnchor.MiddleCenter;
        journalText.fontSize = 38;
        journalText.fontStyle = FontStyle.Italic;
        journalText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        RectTransform jTxtRt = journalTextObj.GetComponent<RectTransform>();
        SetStretchAll(jTxtRt);
        jTxtRt.offsetMin = new Vector2(200, 150);
        jTxtRt.offsetMax = new Vector2(-200, -150);

        // "Geçmek için tuşa basın" yazısı
        GameObject continueTextObj = CreateUIElement("DateAndPressText", quoteGroupObj.transform);
        SetStretchAll(continueTextObj.GetComponent<RectTransform>());
        CanvasGroup dateandpressTextGroup = continueTextObj.AddComponent<CanvasGroup>();
        continueTextObj.SetActive(false);

        GameObject ctTextObj = CreateUIElement("Text", continueTextObj.transform);
        Text ctText = ctTextObj.AddComponent<Text>();
        ctText.text = "Geçmek için bir tuşa basın...";
        ctText.color = new Color(0.6f, 0.6f, 0.6f, 1f);
        ctText.alignment = TextAnchor.LowerRight;
        ctText.fontSize = 26;
        ctText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        RectTransform ctRt = ctTextObj.GetComponent<RectTransform>();
        SetStretchAll(ctRt);
        ctRt.offsetMax = new Vector2(-80, -60);

        // --- KATMAN 4: BLACK FADE (Her şeyin ÜSTÜNDE - menü ve yazıları kapatabilir) ---
        // ÇÖZÜM: Bu obje ASLA SetActive(false) yapılmayacak, sadece alpha değişecek!
        GameObject blackFadeObj = CreateUIElement("BlackFade", canvasObj.transform);
        Image blackFadeImg = blackFadeObj.AddComponent<Image>();
        blackFadeImg.color = Color.black;
        blackFadeImg.raycastTarget = false; // Tıklamaları geçirsin
        SetStretchAll(blackFadeObj.GetComponent<RectTransform>());
        CanvasGroup blackFadeGroup = blackFadeObj.AddComponent<CanvasGroup>();
        blackFadeGroup.alpha = 1; // Başlangıçta tamamen siyah
        blackFadeGroup.blocksRaycasts = false;
        blackFadeGroup.interactable = false;

        // --- KATMAN 5: INTRO YAZILARI (BlackFade'in de ÜSTÜNDE, siyah ekranda görünürler) ---
        // Kulaklık Grubu
        GameObject headphoneObj = CreateUIElement("HeadphoneWarning", canvasObj.transform);
        SetStretchAll(headphoneObj.GetComponent<RectTransform>());
        CanvasGroup headphoneGroup = headphoneObj.AddComponent<CanvasGroup>();
        headphoneGroup.alpha = 0;
        headphoneGroup.blocksRaycasts = false; // ← Görünmez iken tıklamaları YUTMASIN!
        headphoneGroup.interactable = false;

        // Kulaklık RawImage - native size KULLANMIYORUZ, sabit küçük boyut kullan (yazıyla çakışmasın!)
        GameObject hpImgObj = CreateUIElement("HeadphoneIcon", headphoneObj.transform);
        RawImage hpImg = hpImgObj.AddComponent<RawImage>();
        Texture2D hpTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Graphics/headphones.png");
        if (hpTex != null) hpImg.texture = hpTex;
        RectTransform hpRt = hpImgObj.GetComponent<RectTransform>();
        hpRt.anchorMin = new Vector2(0.5f, 0.5f);
        hpRt.anchorMax = new Vector2(0.5f, 0.5f);
        hpRt.pivot = new Vector2(0.5f, 0.5f);
        hpRt.anchoredPosition = new Vector2(0, 100); // Metnin ÜSTÜNDE konumlandır
        hpRt.sizeDelta = new Vector2(120, 120);      // Sabit küçük boyut

        // Kulaklık yazısı — ikonun ALTINDA, sabit yükseklikte
        GameObject headphoneTextObj = CreateUIElement("Text", headphoneObj.transform);
        Text headphoneText = headphoneTextObj.AddComponent<Text>();
        headphoneText.text = "KULAKLIK ÖNERİLİR\n(Headphones Recommended)";
        headphoneText.color = Color.white;
        headphoneText.alignment = TextAnchor.UpperCenter;
        headphoneText.fontSize = 34;
        headphoneText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        RectTransform hpTxtRt = headphoneTextObj.GetComponent<RectTransform>();
        hpTxtRt.anchorMin = new Vector2(0.2f, 0.5f);
        hpTxtRt.anchorMax = new Vector2(0.8f, 0.5f);
        hpTxtRt.pivot = new Vector2(0.5f, 1f);
        hpTxtRt.anchoredPosition = new Vector2(0, 52);  // İkonun alt kenarından başla
        hpTxtRt.sizeDelta = new Vector2(0, 90);          // 2 satır metin için yeterli yükseklik

        // İçerik Uyarısı Grubu
        GameObject warningObj = CreateUIElement("ContentWarning", canvasObj.transform);
        SetStretchAll(warningObj.GetComponent<RectTransform>());
        CanvasGroup warningGroup = warningObj.AddComponent<CanvasGroup>();
        warningGroup.alpha = 0;
        warningGroup.blocksRaycasts = false; // ← Görünmez iken tıklamaları YUTMASIN!
        warningGroup.interactable = false;

        GameObject warningTextObj = CreateUIElement("Text", warningObj.transform);
        Text warningText = warningTextObj.AddComponent<Text>();
        warningText.text = "BU OYUN RAHATSIZ EDİCİ GÖRÜNTÜLER\nVE ANİ SESLER İÇERMEKTEDİR.";
        warningText.color = Color.white;
        warningText.alignment = TextAnchor.MiddleCenter;
        warningText.fontSize = 40;
        warningText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        SetStretchAll(warningTextObj.GetComponent<RectTransform>());

        // Dink Logo + "Tuşa Bas" Grubu
        GameObject pressToContinueObj = CreateUIElement("PressToContinue", canvasObj.transform);
        SetStretchAll(pressToContinueObj.GetComponent<RectTransform>());
        CanvasGroup pressToContinueGroup = pressToContinueObj.AddComponent<CanvasGroup>();
        pressToContinueGroup.alpha = 0;
        pressToContinueGroup.blocksRaycasts = false; // ← Görünmez iken tıklamaları YUTMASIN!
        pressToContinueGroup.interactable = false;

        GameObject logoObj = CreateUIElement("GameLogo", pressToContinueObj.transform);
        RawImage logoRaw = logoObj.AddComponent<RawImage>();
        Texture2D logoTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Graphics/Menü Başlık.png");
        if (logoTexture != null) logoRaw.texture = logoTexture;
        logoRaw.raycastTarget = false;
        RectTransform logoRt = logoObj.GetComponent<RectTransform>();
        logoRt.anchorMin = new Vector2(0.5f, 0.5f);
        logoRt.anchorMax = new Vector2(0.5f, 0.5f);
        logoRt.pivot = new Vector2(0.5f, 0.5f);
        logoRt.anchoredPosition = new Vector2(0, 250); // Metinden daha yukarıda konumlandırıldı
        logoRt.sizeDelta = new Vector2(700, 300); // Daha net ve büyük boyutta

        GameObject pressTextObj = CreateUIElement("PressAnyKey", pressToContinueObj.transform);
        Text pressText = pressTextObj.AddComponent<Text>();
        pressText.text = "Devam etmek için bir tuşa basın";
        pressText.color = new Color(1, 1, 1, 0.55f);
        pressText.alignment = TextAnchor.LowerCenter;
        pressText.fontSize = 30;
        pressText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        RectTransform pressRt = pressTextObj.GetComponent<RectTransform>();
        SetStretchAll(pressRt);
        pressRt.offsetMin = new Vector2(0, 90);

        // =====================================================
        // BAŞLANGIÇ GÖRÜNÜRLÜKLERİ
        // =====================================================
        mainMenuUI.SetActive(false);
        settingsPanel.SetActive(false);
        quoteGroupObj.SetActive(false);
        continueTextObj.SetActive(false);

        // =====================================================
        // REFERANSLARI BAĞLA
        // =====================================================
        introManager.blackFadeGroup = blackFadeGroup;
        introManager.headphoneGroup = headphoneGroup;
        introManager.warningGroup = warningGroup;
        introManager.pressToContinueGroup = pressToContinueGroup;
        introManager.mainMenuUI = mainMenuUI;

        sceneTransition.globalFadeGroup = blackFadeGroup;
        sceneTransition.quoteTextGroup = quoteTextGroup;
        sceneTransition.dateandpressTextGroup = dateandpressTextGroup;
        sceneTransition.mainMenuUI = mainMenuUI; // Geçiş başlayınca menü kapansın

        // UIManager - settings paneli bağla
        SerializedObject uiManagerSO = new SerializedObject(uiManager);
        uiManagerSO.FindProperty("settingsPanel").objectReferenceValue = settingsPanel;
        uiManagerSO.ApplyModifiedProperties();

        // =====================================================
        // BUTON OLAYLARI
        // Find() sadece direkt çocuklara bakar - GetComponentsInChildren ile isim eşleştirme yapıyoruz
        // =====================================================
        Button startBtnComp = FindButtonByName(mainMenuUI, "Button_START");
        Button settingsBtnComp = FindButtonByName(mainMenuUI, "Button_SETTINGS");
        Button quitBtnComp = FindButtonByName(mainMenuUI, "Button_QUIT");
        Button backBtnComp = FindButtonByName(settingsPanel, "Button_BACK");

        if (startBtnComp != null)
            UnityEventTools.AddIntPersistentListener(startBtnComp.onClick,
                new UnityAction<int>(sceneTransition.ButonlaSahneyeGit), 1);
        else Debug.LogError("Button_START bulunamadı!");

        if (settingsBtnComp != null)
            UnityEventTools.AddVoidPersistentListener(settingsBtnComp.onClick,
                new UnityAction(uiManager.ToggleSettingsMenu));
        else Debug.LogError("Button_SETTINGS bulunamadı!");

        if (quitBtnComp != null)
            UnityEventTools.AddVoidPersistentListener(quitBtnComp.onClick,
                new UnityAction(uiManager.OnQuitPress));
        else Debug.LogError("Button_QUIT bulunamadı!");

        if (backBtnComp != null)
            UnityEventTools.AddVoidPersistentListener(backBtnComp.onClick,
                new UnityAction(uiManager.ToggleSettingsMenu));
        else Debug.LogError("Button_BACK bulunamadı!");

        // =====================================================
        // KORKUTUCU DETAY: Işık Titremesi
        // =====================================================
        Light[] allLights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
        foreach (Light l in allLights)
        {
            if (l.type == LightType.Spot || l.type == LightType.Point)
                if (l.gameObject.GetComponent<FlickerLight>() == null)
                    l.gameObject.AddComponent<FlickerLight>();
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("Menü başarıyla kuruldu!");
    }

    // =====================================================
    // ANA MENÜ PANELİ OLUŞTURUCU
    // =====================================================
    private static GameObject CreateMainMenuPanel(Transform parent)
    {
        GameObject panel = CreateUIElement("MainMenuPanel", parent);
        RectTransform rt = panel.GetComponent<RectTransform>();
        // TAM EKRAN simsiyah arka plan
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(1f, 1f); // Tam ekran
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Image bg = panel.AddComponent<Image>();
        bg.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        bg.type = Image.Type.Sliced;
        bg.color = new Color(0f, 0f, 0f, 1f); // TAMAMEN SİYAH

        // Üst dekorasyon çizgisi kaldırıldı (Kenarlık kırmızılığı olmaması için)

        // Oyun adı — sol tarafta, ekranın %30'unda konumlandır
        GameObject gameTitleObj = CreateUIElement("GameTitle", panel.transform);
        Text gameTitle = gameTitleObj.AddComponent<Text>();
        gameTitle.text = "DINK";
        gameTitle.color = new Color(0.8f, 0, 0, 0.55f);
        gameTitle.fontSize = 110;
        gameTitle.fontStyle = FontStyle.Bold;
        gameTitle.alignment = TextAnchor.LowerLeft;
        gameTitle.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        RectTransform gtRt = gameTitleObj.GetComponent<RectTransform>();
        gtRt.anchorMin = new Vector2(0, 0.45f);
        gtRt.anchorMax = new Vector2(0.32f, 0.85f); // Sol %32'sinde
        gtRt.offsetMin = new Vector2(70, 0);
        gtRt.offsetMax = Vector2.zero;

        // Küçük alt yazı
        GameObject subtitleObj = CreateUIElement("Subtitle", panel.transform);
        Text subtitle = subtitleObj.AddComponent<Text>();
        subtitle.text = "Kapıyı seç. Sonuçlara katlan.";
        subtitle.color = new Color(0.5f, 0.5f, 0.5f, 0.65f);
        subtitle.fontSize = 22;
        subtitle.fontStyle = FontStyle.Italic;
        subtitle.alignment = TextAnchor.UpperLeft;
        subtitle.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        RectTransform stRt = subtitleObj.GetComponent<RectTransform>();
        stRt.anchorMin = new Vector2(0, 0.42f);
        stRt.anchorMax = new Vector2(0.32f, 0.48f);
        stRt.offsetMin = new Vector2(75, 0);
        stRt.offsetMax = Vector2.zero;

        // Buton grubu — sol altta, ekranın sol %30'unda
        GameObject btnGroup = CreateUIElement("ButtonGroup", panel.transform);
        RectTransform bgRt = btnGroup.GetComponent<RectTransform>();
        bgRt.anchorMin = new Vector2(0, 0);
        bgRt.anchorMax = new Vector2(0.32f, 0.38f); // Sol %32'sinde
        bgRt.offsetMin = new Vector2(70, 50);
        bgRt.offsetMax = Vector2.zero;

        VerticalLayoutGroup vlg = btnGroup.AddComponent<VerticalLayoutGroup>();
        vlg.childAlignment = TextAnchor.LowerLeft;
        vlg.childControlHeight = false;
        vlg.childControlWidth = false;
        vlg.spacing = 14;

        CreateMenuButton("BAŞLA (START)", "Button_START", btnGroup.transform);
        CreateMenuButton("AYARLAR (SETTINGS)", "Button_SETTINGS", btnGroup.transform);
        CreateMenuButton("ÇIKIŞ (QUIT)", "Button_QUIT", btnGroup.transform);

        // Alt dekorasyon çizgisi kaldırıldı (Kenarlık kırmızılığı olmaması için)

        // Sürüm numarası (sol alt köşe)
        GameObject versionObj = CreateUIElement("Version", panel.transform);
        Text versionText = versionObj.AddComponent<Text>();
        versionText.text = "v0.1 - Early Build";
        versionText.color = new Color(0.4f, 0.4f, 0.4f, 0.5f);
        versionText.fontSize = 20;
        versionText.alignment = TextAnchor.LowerLeft;
        versionText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        RectTransform vrRt = versionObj.GetComponent<RectTransform>();
        vrRt.anchorMin = new Vector2(0, 0);
        vrRt.anchorMax = new Vector2(1, 0);
        vrRt.pivot = new Vector2(0, 0);
        vrRt.anchoredPosition = new Vector2(55, 12);
        vrRt.sizeDelta = new Vector2(300, 30);

        return panel;
    }

    // =====================================================
    // AYARLAR PANELİ OLUŞTURUCU
    // =====================================================
    private static GameObject CreateSettingsPanel(Transform parent)
    {
        GameObject panel = CreateUIElement("SettingsPanel", parent);
        SetStretchAll(panel.GetComponent<RectTransform>());
        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.95f);

        GameObject titleObj = CreateUIElement("Title", panel.transform);
        Text title = titleObj.AddComponent<Text>();
        title.text = "AYARLAR\n\nÇok yakında...";
        title.color = Color.white;
        title.alignment = TextAnchor.MiddleCenter;
        title.fontSize = 50;
        title.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        SetStretchAll(titleObj.GetComponent<RectTransform>());

        CreateMenuButton("GERİ (BACK)", "Button_BACK", panel.transform);
        RectTransform backRt = panel.transform.Find("Button_BACK").GetComponent<RectTransform>();
        backRt.anchorMin = new Vector2(0.5f, 0);
        backRt.anchorMax = new Vector2(0.5f, 0);
        backRt.pivot = new Vector2(0.5f, 0);
        backRt.anchoredPosition = new Vector2(0, 100);

        return panel;
    }

    private static GameObject CreateUIElement(string name, Transform parent)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        return go;
    }

    private static void SetStretchAll(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static void CreateMenuButton(string label, string goName, Transform parent)
    {
        GameObject btnObj = new GameObject(goName);
        btnObj.transform.SetParent(parent, false);
        RectTransform btnRt = btnObj.AddComponent<RectTransform>();
        btnRt.sizeDelta = new Vector2(420, 46);

        Button btn = btnObj.AddComponent<Button>();
        Image img = btnObj.AddComponent<Image>();
        img.color = new Color(0, 0, 0, 0.01f);

        GameObject txtObj = new GameObject("Text");
        txtObj.transform.SetParent(btnObj.transform, false);
        RectTransform tRt = txtObj.AddComponent<RectTransform>();
        SetStretchAll(tRt);
        Text txt = txtObj.AddComponent<Text>();
        txt.text = label;
        txt.color = new Color(0.85f, 0.85f, 0.85f, 1f);
        txt.alignment = TextAnchor.MiddleLeft;
        txt.fontSize = 34;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        btn.targetGraphic = txt;
        ColorBlock cb = btn.colors;
        cb.normalColor = new Color(0.85f, 0.85f, 0.85f, 1f);
        cb.highlightedColor = new Color(0.85f, 0, 0, 1f); // Kan kırmızısı hover
        cb.pressedColor = new Color(0.5f, 0, 0, 1f);
        cb.colorMultiplier = 1f;
        btn.colors = cb;
    }

    // GetComponentsInChildren ile isim eşleştirme (Find() aksine tüm alt objelerde arar)
    private static Button FindButtonByName(GameObject root, string name)
    {
        Button[] all = root.GetComponentsInChildren<Button>(true);
        foreach (Button b in all)
            if (b.gameObject.name == name) return b;
        Debug.LogError($"[MenuBuilder] '{name}' isimli buton {root.name} içinde bulunamadı!");
        return null;
    }
}
