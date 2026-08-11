using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CreateZindanMinigameSetup
{
    [MenuItem("Dink Tools/Zindan Minigamelerini Olustur (4 Rastgele Minigame)")]
    public static void SetupZindanMinigames()
    {
        // 1. Zindan Kilit Canvas Arama / Oluşturma
        GameObject canvasGo = GameObject.Find("ZindanKilitCanvas");
        if (canvasGo != null)
        {
            Object.DestroyImmediate(canvasGo);
        }

        canvasGo = new GameObject("ZindanKilitCanvas");
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasGo.AddComponent<GraphicRaycaster>();

        // EventSystem kontrolü
        if (Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        GameObject[] paneller = new GameObject[4];

        // --- MINIGAME 1: KİLİT PİMLERİ ---
        paneller[0] = OlusturKilitPanel(canvasGo.transform);

        // --- MINIGAME 2: KABLO BAĞLAMA ---
        paneller[1] = OlusturKabloPanel(canvasGo.transform);

        // --- MINIGAME 3: VANA BASINCI ---
        paneller[2] = OlusturVanaPanel(canvasGo.transform);

        // --- MINIGAME 4: ANTİK RÜN ---
        paneller[3] = OlusturRunPanel(canvasGo.transform);

        // Minigame Selector Ekleme
        ZindanMinigameSelector selector = canvasGo.AddComponent<ZindanMinigameSelector>();
        SerializedObject selectorSo = new SerializedObject(selector);
        SerializedProperty panellerProp = selectorSo.FindProperty("minigamePanelleri");
        panellerProp.arraySize = 4;
        for (int i = 0; i < 4; i++)
        {
            panellerProp.GetArrayElementAtIndex(i).objectReferenceValue = paneller[i];
        }
        selectorSo.ApplyModifiedProperties();

        Selection.activeGameObject = canvasGo;
        Debug.Log("4 Adet Zindan Minigames paneli ve Rastgele Seçici (ZindanMinigameSelector) başarıyla oluşturuldu!");
    }

    // --- PANEL 1: KİLİT PİMLERİ ---
    private static GameObject OlusturKilitPanel(Transform parent)
    {
        GameObject panel = OlusturAnaArkaPlan(parent, "Minigame1_KilitPaneli", new Color(0.08f, 0.06f, 0.06f, 0.9f));
        TextMeshProUGUI title = OlusturMetin(panel, "Baslik", "PASLI ZİNDAN KİLİDİ", 28, new Vector2(0f, 0.85f), new Vector2(1f, 0.98f), new Color(0.85f, 0.7f, 0.4f));
        TextMeshProUGUI sayac = OlusturMetin(panel, "Sayac", "KALAN SÜRE: 20.0s", 30, new Vector2(0f, 0.72f), new Vector2(1f, 0.85f), new Color(1f, 0.3f, 0.2f));
        TextMeshProUGUI durum = OlusturMetin(panel, "Durum", "PİM HEDEFE GELİNCE TIKLA!", 18, new Vector2(0.05f, 0.03f), new Vector2(0.95f, 0.15f), Color.yellow);

        Slider[] sliders = new Slider[3];
        Image[] hedefler = new Image[3];
        Image[] dolgular = new Image[3];

        float startX = -180f;
        for (int i = 0; i < 3; i++)
        {
            GameObject pim = new GameObject($"Pim_{i + 1}");
            pim.transform.SetParent(panel.transform, false);
            RectTransform pRect = pim.AddComponent<RectTransform>();
            pRect.anchorMin = new Vector2(0.5f, 0.45f);
            pRect.anchorMax = new Vector2(0.5f, 0.45f);
            pRect.anchoredPosition = new Vector2(startX + (i * 180f), 0);
            pRect.sizeDelta = new Vector2(60, 200);

            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(pim.transform, false);
            Image bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0.2f, 0.2f, 0.2f, 1f);
            OlusturTamStretch(bg);

            GameObject tgt = new GameObject("TargetZone");
            tgt.transform.SetParent(pim.transform, false);
            Image tgtImg = tgt.AddComponent<Image>();
            tgtImg.color = new Color(1f, 0.8f, 0f, 0.6f);
            RectTransform tgtRect = tgt.GetComponent<RectTransform>();
            tgtRect.anchorMin = new Vector2(0f, 0.4f);
            tgtRect.anchorMax = new Vector2(1f, 0.65f);
            tgtRect.offsetMin = Vector2.zero; tgtRect.offsetMax = Vector2.zero;
            hedefler[i] = tgtImg;

            GameObject fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(pim.transform, false);
            OlusturTamStretch(fillArea);

            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            Image fillImg = fill.AddComponent<Image>();
            fillImg.color = new Color(0.8f, 0.3f, 0.1f, 1f);
            OlusturTamStretch(fill);
            dolgular[i] = fillImg;

            Slider s = pim.AddComponent<Slider>();
            s.direction = Slider.Direction.BottomToTop;
            s.fillRect = fill.GetComponent<RectTransform>();
            s.interactable = false;
            sliders[i] = s;
        }

        ZindanKilitMinigame script = panel.AddComponent<ZindanKilitMinigame>();
        SerializedObject so = new SerializedObject(script);
        so.FindProperty("sayaçMetni").objectReferenceValue = sayac;
        so.FindProperty("durumMetni").objectReferenceValue = durum;

        SerializedProperty sProp = so.FindProperty("pimSliderlari");
        sProp.arraySize = 3;
        for (int i = 0; i < 3; i++) sProp.GetArrayElementAtIndex(i).objectReferenceValue = sliders[i];

        SerializedProperty hProp = so.FindProperty("hedefAlanGorselleri");
        hProp.arraySize = 3;
        for (int i = 0; i < 3; i++) hProp.GetArrayElementAtIndex(i).objectReferenceValue = hedefler[i];

        SerializedProperty dProp = so.FindProperty("pimDolguGorselleri");
        dProp.arraySize = 3;
        for (int i = 0; i < 3; i++) dProp.GetArrayElementAtIndex(i).objectReferenceValue = dolgular[i];

        so.FindProperty("audioSource").objectReferenceValue = panel.AddComponent<AudioSource>();
        so.ApplyModifiedProperties();

        return panel;
    }

    // --- PANEL 2: KABLO BAĞLAMA ---
    private static GameObject OlusturKabloPanel(Transform parent)
    {
        GameObject panel = OlusturAnaArkaPlan(parent, "Minigame2_KabloPaneli", new Color(0.06f, 0.08f, 0.08f, 0.9f));
        OlusturMetin(panel, "Baslik", "ELEKTRİK KABLO TAMİRİ", 28, new Vector2(0f, 0.85f), new Vector2(1f, 0.98f), new Color(0.4f, 0.85f, 0.85f));
        TextMeshProUGUI sayac = OlusturMetin(panel, "Sayac", "KALAN SÜRE: 20.0s", 30, new Vector2(0f, 0.72f), new Vector2(1f, 0.85f), new Color(1f, 0.3f, 0.2f));
        TextMeshProUGUI durum = OlusturMetin(panel, "Durum", "SOLDAN KABLO SEÇ SAĞA BAĞLA!", 18, new Vector2(0.05f, 0.03f), new Vector2(0.95f, 0.15f), Color.yellow);

        Button[] solBtns = new Button[4];
        Button[] sagBtns = new Button[4];
        Image[] solImgs = new Image[4];
        Image[] sagImgs = new Image[4];

        Color[] renkler = new Color[] { new Color(0.9f, 0.2f, 0.2f), new Color(0.2f, 0.5f, 1f), new Color(1f, 0.8f, 0.1f), new Color(0.2f, 0.9f, 0.3f) };

        float startY = 60f;
        for (int i = 0; i < 4; i++)
        {
            // Sol Kablo Button
            GameObject sBtn = new GameObject($"SolKablo_{i + 1}");
            sBtn.transform.SetParent(panel.transform, false);
            Image sImg = sBtn.AddComponent<Image>();
            sImg.color = renkler[i];
            Button sb = sBtn.AddComponent<Button>();
            RectTransform sr = sBtn.GetComponent<RectTransform>();
            sr.anchorMin = new Vector2(0.2f, 0.5f); sr.anchorMax = new Vector2(0.2f, 0.5f);
            sr.anchoredPosition = new Vector2(0, startY - (i * 65f));
            sr.sizeDelta = new Vector2(140, 45);
            solBtns[i] = sb; solImgs[i] = sImg;

            // Sağ Soket Button
            GameObject gBtn = new GameObject($"SagSoket_{i + 1}");
            gBtn.transform.SetParent(panel.transform, false);
            Image gImg = gBtn.AddComponent<Image>();
            gImg.color = renkler[i];
            Button gb = gBtn.AddComponent<Button>();
            RectTransform gr = gBtn.GetComponent<RectTransform>();
            gr.anchorMin = new Vector2(0.8f, 0.5f); gr.anchorMax = new Vector2(0.8f, 0.5f);
            gr.anchoredPosition = new Vector2(0, startY - (i * 65f));
            gr.sizeDelta = new Vector2(140, 45);
            sagBtns[i] = gb; sagImgs[i] = gImg;
        }

        ZindanKabloMinigame script = panel.AddComponent<ZindanKabloMinigame>();
        SerializedObject so = new SerializedObject(script);
        so.FindProperty("sayaçMetni").objectReferenceValue = sayac;
        so.FindProperty("durumMetni").objectReferenceValue = durum;

        SerializedProperty solProp = so.FindProperty("solKabloButonlari"); solProp.arraySize = 4;
        SerializedProperty sagProp = so.FindProperty("sagSoketButonlari"); sagProp.arraySize = 4;
        SerializedProperty solImgProp = so.FindProperty("solKabloGorselleri"); solImgProp.arraySize = 4;
        SerializedProperty sagImgProp = so.FindProperty("sagSoketGorselleri"); sagImgProp.arraySize = 4;

        for (int i = 0; i < 4; i++)
        {
            solProp.GetArrayElementAtIndex(i).objectReferenceValue = solBtns[i];
            sagProp.GetArrayElementAtIndex(i).objectReferenceValue = sagBtns[i];
            solImgProp.GetArrayElementAtIndex(i).objectReferenceValue = solImgs[i];
            sagImgProp.GetArrayElementAtIndex(i).objectReferenceValue = sagImgs[i];
        }

        so.FindProperty("audioSource").objectReferenceValue = panel.AddComponent<AudioSource>();
        so.ApplyModifiedProperties();

        return panel;
    }

    // --- PANEL 3: VANA BASINCI ---
    private static GameObject OlusturVanaPanel(Transform parent)
    {
        GameObject panel = OlusturAnaArkaPlan(parent, "Minigame3_VanaPaneli", new Color(0.09f, 0.07f, 0.05f, 0.9f));
        OlusturMetin(panel, "Baslik", "VANA BASINÇ DENGELEME", 28, new Vector2(0f, 0.85f), new Vector2(1f, 0.98f), new Color(0.9f, 0.6f, 0.3f));
        TextMeshProUGUI sayac = OlusturMetin(panel, "Sayac", "KALAN SÜRE: 22.0s", 30, new Vector2(0f, 0.72f), new Vector2(1f, 0.85f), new Color(1f, 0.3f, 0.2f));
        TextMeshProUGUI durum = OlusturMetin(panel, "Durum", "VANALARA BASILI TUTARAK YEŞİLDE TUT!", 18, new Vector2(0.05f, 0.03f), new Vector2(0.95f, 0.15f), Color.yellow);

        Slider[] sliders = new Slider[3];
        Button[] buttons = new Button[3];
        Image[] hedefler = new Image[3];

        float startX = -180f;
        for (int i = 0; i < 3; i++)
        {
            GameObject vana = new GameObject($"Vana_{i + 1}");
            vana.transform.SetParent(panel.transform, false);
            RectTransform vr = vana.AddComponent<RectTransform>();
            vr.anchorMin = new Vector2(0.5f, 0.45f); vr.anchorMax = new Vector2(0.5f, 0.45f);
            vr.anchoredPosition = new Vector2(startX + (i * 180f), 0);
            vr.sizeDelta = new Vector2(70, 200);

            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(vana.transform, false);
            Image bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0.2f, 0.2f, 0.2f, 1f);
            OlusturTamStretch(bg);

            GameObject tgt = new GameObject("TargetZone");
            tgt.transform.SetParent(vana.transform, false);
            Image tgtImg = tgt.AddComponent<Image>();
            tgtImg.color = new Color(0.2f, 0.9f, 0.2f, 0.6f);
            RectTransform tgtRect = tgt.GetComponent<RectTransform>();
            tgtRect.anchorMin = new Vector2(0f, 0.45f); tgtRect.anchorMax = new Vector2(1f, 0.70f);
            tgtRect.offsetMin = Vector2.zero; tgtRect.offsetMax = Vector2.zero;
            hedefler[i] = tgtImg;

            GameObject fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(vana.transform, false);
            OlusturTamStretch(fillArea);

            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            Image fillImg = fill.AddComponent<Image>();
            fillImg.color = new Color(0.9f, 0.4f, 0.1f, 1f);
            OlusturTamStretch(fill);

            Slider s = vana.AddComponent<Slider>();
            s.direction = Slider.Direction.BottomToTop;
            s.fillRect = fill.GetComponent<RectTransform>();
            s.interactable = false;
            sliders[i] = s;

            Button b = vana.AddComponent<Button>();
            buttons[i] = b;
        }

        ZindanVanaMinigame script = panel.AddComponent<ZindanVanaMinigame>();
        SerializedObject so = new SerializedObject(script);
        so.FindProperty("sayaçMetni").objectReferenceValue = sayac;
        so.FindProperty("durumMetni").objectReferenceValue = durum;

        SerializedProperty sProp = so.FindProperty("vanaSliderlari"); sProp.arraySize = 3;
        SerializedProperty bProp = so.FindProperty("vanaButonlari"); bProp.arraySize = 3;
        SerializedProperty hProp = so.FindProperty("hedefAlanGorselleri"); hProp.arraySize = 3;

        for (int i = 0; i < 3; i++)
        {
            sProp.GetArrayElementAtIndex(i).objectReferenceValue = sliders[i];
            bProp.GetArrayElementAtIndex(i).objectReferenceValue = buttons[i];
            hProp.GetArrayElementAtIndex(i).objectReferenceValue = hedefler[i];
        }

        so.FindProperty("audioSource").objectReferenceValue = panel.AddComponent<AudioSource>();
        so.ApplyModifiedProperties();

        return panel;
    }

    // --- PANEL 4: ANTİK RÜN ---
    private static GameObject OlusturRunPanel(Transform parent)
    {
        GameObject panel = OlusturAnaArkaPlan(parent, "Minigame4_RunPaneli", new Color(0.08f, 0.05f, 0.09f, 0.9f));
        OlusturMetin(panel, "Baslik", "ANTİK RÜN MÜHÜRÜ", 28, new Vector2(0f, 0.85f), new Vector2(1f, 0.98f), new Color(0.85f, 0.4f, 0.9f));
        TextMeshProUGUI sayac = OlusturMetin(panel, "Sayac", "KALAN SÜRE: 20.0s", 30, new Vector2(0f, 0.72f), new Vector2(1f, 0.85f), new Color(1f, 0.3f, 0.2f));
        TextMeshProUGUI durum = OlusturMetin(panel, "Durum", "RÜN SIRASINI İZLE!", 18, new Vector2(0.05f, 0.03f), new Vector2(0.95f, 0.15f), Color.yellow);

        Button[] buttons = new Button[4];
        Image[] images = new Image[4];

        Vector2[] pos = new Vector2[] { new Vector2(-75f, 40f), new Vector2(75f, 40f), new Vector2(-75f, -90f), new Vector2(75f, -90f) };

        for (int i = 0; i < 4; i++)
        {
            GameObject runGo = new GameObject($"Run_{i + 1}");
            runGo.transform.SetParent(panel.transform, false);
            Image img = runGo.AddComponent<Image>();
            img.color = new Color(0.3f, 0.2f, 0.4f, 1f);
            Button b = runGo.AddComponent<Button>();
            RectTransform r = runGo.GetComponent<RectTransform>();
            r.anchorMin = new Vector2(0.5f, 0.45f); r.anchorMax = new Vector2(0.5f, 0.45f);
            r.anchoredPosition = pos[i];
            r.sizeDelta = new Vector2(120, 110);

            buttons[i] = b; images[i] = img;
        }

        ZindanRunMinigame script = panel.AddComponent<ZindanRunMinigame>();
        SerializedObject so = new SerializedObject(script);
        so.FindProperty("sayaçMetni").objectReferenceValue = sayac;
        so.FindProperty("durumMetni").objectReferenceValue = durum;

        SerializedProperty bProp = so.FindProperty("runButonlari"); bProp.arraySize = 4;
        SerializedProperty iProp = so.FindProperty("runGorselleri"); iProp.arraySize = 4;

        for (int i = 0; i < 4; i++)
        {
            bProp.GetArrayElementAtIndex(i).objectReferenceValue = buttons[i];
            iProp.GetArrayElementAtIndex(i).objectReferenceValue = images[i];
        }

        so.FindProperty("audioSource").objectReferenceValue = panel.AddComponent<AudioSource>();
        so.ApplyModifiedProperties();

        return panel;
    }

    private static GameObject OlusturAnaArkaPlan(Transform parent, string isim, Color renk)
    {
        GameObject panel = new GameObject(isim);
        panel.transform.SetParent(parent, false);
        Image img = panel.AddComponent<Image>();
        img.color = renk;
        RectTransform r = panel.GetComponent<RectTransform>();
        r.anchorMin = new Vector2(0.5f, 0.5f); r.anchorMax = new Vector2(0.5f, 0.5f);
        r.sizeDelta = new Vector2(650, 500);
        return panel;
    }

    private static TextMeshProUGUI OlusturMetin(GameObject parent, string isim, string metin, int fontBoyut, Vector2 min, Vector2 max, Color renk)
    {
        GameObject go = new GameObject(isim);
        go.transform.SetParent(parent.transform, false);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = metin;
        tmp.fontSize = fontBoyut;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = renk;
        RectTransform r = go.GetComponent<RectTransform>();
        r.anchorMin = min; r.anchorMax = max;
        r.offsetMin = Vector2.zero; r.offsetMax = Vector2.zero;
        return tmp;
    }

    private static void OlusturTamStretch(GameObject go)
    {
        RectTransform r = go.GetComponent<RectTransform>();
        if (r == null) r = go.AddComponent<RectTransform>();
        r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
        r.offsetMin = Vector2.zero; r.offsetMax = Vector2.zero;
    }
}
