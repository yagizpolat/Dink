using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Unity Editor Otomatik Kurulum Aracı:
/// Dink Tools -> Oyuncu Adi Giris Paneli Kur
/// 
/// Oyuncudan kullanıcı adı isteyen şık, karanlık temalı paneli 7 dilde (LocalizedText) otomatik kurar.
/// </summary>
public class CreateNicknameUISetup : Editor
{
    [MenuItem("Dink Tools/Oyuncu Adi Giris Paneli Kur")]
    public static void SetupNicknameUI()
    {
        // 1. Varsa eski paneli temizle
        GameObject oldPanel = GameObject.Find("[OYUNCU İSİM GİRİŞİ (CANVAS)]");
        if (oldPanel != null)
            Undo.DestroyObjectImmediate(oldPanel);

        // 2. Canvas Oluştur (Sorting Order: 15)
        GameObject canvasObj = new GameObject("[OYUNCU İSİM GİRİŞİ (CANVAS)]");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 15;

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

        // 3. Arka Plan Koyu Panel
        GameObject bgPanel = new GameObject("BackgroundPanel");
        bgPanel.transform.SetParent(canvasObj.transform, false);
        RectTransform bgRect = bgPanel.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;

        Image bgImage = bgPanel.AddComponent<Image>();
        bgImage.color = new Color(0.04f, 0.05f, 0.07f, 0.95f);

        // 4. Orta İçerik Kutusu
        GameObject boxObj = new GameObject("ContentBox");
        boxObj.transform.SetParent(bgPanel.transform, false);
        RectTransform boxRect = boxObj.AddComponent<RectTransform>();
        boxRect.anchorMin = new Vector2(0.5f, 0.5f);
        boxRect.anchorMax = new Vector2(0.5f, 0.5f);
        boxRect.sizeDelta = new Vector2(650, 400);

        Image boxImg = boxObj.AddComponent<Image>();
        boxImg.color = new Color(0.1f, 0.12f, 0.16f, 0.98f);

        Outline outline = boxObj.AddComponent<Outline>();
        outline.effectColor = new Color(0.4f, 0.45f, 0.55f, 0.3f);
        outline.effectDistance = new Vector2(2, -2);

        // 5. Başlık Yazısı (7 Dilli)
        GameObject titleObj = new GameObject("TitleText");
        titleObj.transform.SetParent(boxObj.transform, false);
        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0, -30);
        titleRect.sizeDelta = new Vector2(0, 50);

        TextMeshProUGUI titleTMP = titleObj.AddComponent<TextMeshProUGUI>();
        titleTMP.fontSize = 26;
        titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.alignment = TextAlignmentOptions.Center;
        titleTMP.color = new Color(0.9f, 0.92f, 0.95f);

        LocalizedText titleLoc = titleObj.AddComponent<LocalizedText>();
        titleLoc.enText = "ENTER YOUR USERNAME";
        titleLoc.trText = "KULLANICI ADINIZI GİRİN";
        titleLoc.deText = "GEBEN SIE IHREN BENUTZERNAMEN EIN";
        titleLoc.frText = "ENTREZ VOTRE NOM D'UTILISATEUR";
        titleLoc.esText = "INGRESA TU NOMBRE DE USUARIO";
        titleLoc.ptText = "INSIRA SEU NOME DE USUÁRIO";
        titleLoc.ruText = "ВВЕДИТЕ ВАШЕ ИМЯ ПОЛЬЗОВАТЕЛЯ";
        titleLoc.UpdateText();

        // Alt Bilgi Metni (7 Dilli)
        GameObject subTitleObj = new GameObject("SubTitleText");
        subTitleObj.transform.SetParent(boxObj.transform, false);
        RectTransform subTitleRect = subTitleObj.AddComponent<RectTransform>();
        subTitleRect.anchorMin = new Vector2(0f, 1f);
        subTitleRect.anchorMax = new Vector2(1f, 1f);
        subTitleRect.pivot = new Vector2(0.5f, 1f);
        subTitleRect.anchoredPosition = new Vector2(0, -85);
        subTitleRect.sizeDelta = new Vector2(0, 40);

        TextMeshProUGUI subTitleTMP = subTitleObj.AddComponent<TextMeshProUGUI>();
        subTitleTMP.fontSize = 18;
        subTitleTMP.alignment = TextAlignmentOptions.Center;
        subTitleTMP.color = new Color(0.65f, 0.7f, 0.75f);

        LocalizedText subTitleLoc = subTitleObj.AddComponent<LocalizedText>();
        subTitleLoc.enText = "Enter your name before starting the story:";
        subTitleLoc.trText = "Hikayeye başlamadan önce isminizi belirtin:";
        subTitleLoc.deText = "Geben Sie Ihren Namen ein, bevor die Geschichte beginnt:";
        subTitleLoc.frText = "Entrez votre nom avant de commencer l'histoire :";
        subTitleLoc.esText = "Ingresa tu nombre antes de comenzar la historia:";
        subTitleLoc.ptText = "Insira seu nome antes de começar a história:";
        subTitleLoc.ruText = "Введите ваше имя перед началом истории:";
        subTitleLoc.UpdateText();

        // 6. TMP Girdi Kutusu (InputField)
        GameObject inputObj = new GameObject("NicknameInputField");
        inputObj.transform.SetParent(boxObj.transform, false);
        RectTransform inputRect = inputObj.AddComponent<RectTransform>();
        inputRect.anchorMin = new Vector2(0.5f, 0.5f);
        inputRect.anchorMax = new Vector2(0.5f, 0.5f);
        inputRect.anchoredPosition = new Vector2(0, -10);
        inputRect.sizeDelta = new Vector2(500, 55);

        Image inputBg = inputObj.AddComponent<Image>();
        inputBg.color = new Color(0.06f, 0.07f, 0.09f, 1f);

        TMP_InputField inputField = inputObj.AddComponent<TMP_InputField>();

        GameObject textArea = new GameObject("TextArea");
        textArea.transform.SetParent(inputObj.transform, false);
        RectTransform taRect = textArea.AddComponent<RectTransform>();
        taRect.anchorMin = Vector2.zero;
        taRect.anchorMax = Vector2.one;
        taRect.sizeDelta = new Vector2(-20, -10);

        // Placeholder (7 Dilli)
        GameObject placeholderObj = new GameObject("Placeholder");
        placeholderObj.transform.SetParent(textArea.transform, false);
        RectTransform phRect = placeholderObj.AddComponent<RectTransform>();
        phRect.anchorMin = Vector2.zero;
        phRect.anchorMax = Vector2.one;
        phRect.sizeDelta = Vector2.zero;

        TextMeshProUGUI phTMP = placeholderObj.AddComponent<TextMeshProUGUI>();
        phTMP.fontSize = 20;
        phTMP.fontStyle = FontStyles.Italic;
        phTMP.color = new Color(0.4f, 0.45f, 0.5f);
        phTMP.alignment = TextAlignmentOptions.Left;

        LocalizedText phLoc = placeholderObj.AddComponent<LocalizedText>();
        phLoc.enText = "Username...";
        phLoc.trText = "Kullanıcı Adı...";
        phLoc.deText = "Benutzername...";
        phLoc.frText = "Nom d'utilisateur...";
        phLoc.esText = "Nombre de usuario...";
        phLoc.ptText = "Nome de usuário...";
        phLoc.ruText = "Имя пользователя...";
        phLoc.UpdateText();

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(textArea.transform, false);
        RectTransform tRect = textObj.AddComponent<RectTransform>();
        tRect.anchorMin = Vector2.zero;
        tRect.anchorMax = Vector2.one;
        tRect.sizeDelta = Vector2.zero;

        TextMeshProUGUI textTMP = textObj.AddComponent<TextMeshProUGUI>();
        textTMP.fontSize = 22;
        textTMP.color = Color.white;
        textTMP.alignment = TextAlignmentOptions.Left;

        inputField.textViewport = taRect;
        inputField.textComponent = textTMP;
        inputField.placeholder = phTMP;

        // 7. DEVAM ET Butonu (7 Dilli)
        GameObject btnObj = new GameObject("SubmitButton");
        btnObj.transform.SetParent(boxObj.transform, false);
        RectTransform btnRect = btnObj.AddComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.5f, 0f);
        btnRect.anchorMax = new Vector2(0.5f, 0f);
        btnRect.pivot = new Vector2(0.5f, 0f);
        btnRect.anchoredPosition = new Vector2(0, 30);
        btnRect.sizeDelta = new Vector2(250, 50);

        Image btnImg = btnObj.AddComponent<Image>();
        btnImg.color = new Color(0.85f, 0.2f, 0.2f);

        Button btn = btnObj.AddComponent<Button>();

        GameObject btnTextObj = new GameObject("ButtonText");
        btnTextObj.transform.SetParent(btnObj.transform, false);
        RectTransform btRect = btnTextObj.AddComponent<RectTransform>();
        btRect.anchorMin = Vector2.zero;
        btRect.anchorMax = Vector2.one;
        btRect.sizeDelta = Vector2.zero;

        TextMeshProUGUI btnTMP = btnTextObj.AddComponent<TextMeshProUGUI>();
        btnTMP.fontSize = 22;
        btnTMP.fontStyle = FontStyles.Bold;
        btnTMP.color = Color.white;
        btnTMP.alignment = TextAlignmentOptions.Center;

        LocalizedText btnLoc = btnTextObj.AddComponent<LocalizedText>();
        btnLoc.enText = "CONTINUE";
        btnLoc.trText = "DEVAM ET";
        btnLoc.deText = "WEITER";
        btnLoc.frText = "CONTINUER";
        btnLoc.esText = "CONTINUAR";
        btnLoc.ptText = "CONTINUAR";
        btnLoc.ruText = "ПРОДОЛЖИТЬ";
        btnLoc.UpdateText();

        canvasObj.SetActive(false);

        Undo.RegisterCreatedObjectUndo(canvasObj, "7 Dilli Oyuncu İsim Paneli Kuruldu");
        Selection.activeGameObject = canvasObj;

        Debug.Log("<color=green>[DINK] 7 Dilli Oyuncu Adı Giriş Paneli UI başarıyla kuruldu!</color>");
    }
}
