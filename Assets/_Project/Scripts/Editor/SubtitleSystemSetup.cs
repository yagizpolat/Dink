using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Dink projesinin Canvas UI yapısı altına altyazı panelini (Subtitle_Panel),
/// TextMeshPro altyazı yazısını ve SubtitleManager C# sistemini otomatik kuran Editor scripti.
/// </summary>
public class SubtitleSystemSetup
{
    [MenuItem("Tools/Dink/Arayuz/Altyazi ve Ses Sistemini Olustur (Subtitle System)")]
    public static void SetupSubtitleSystem()
    {
        // 1. SAHNEDEKİ CANVAS'I VEYA UI PARENTİ BUL
        Canvas canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("[Dink Altyazı] Sahnede Canvas (UI) bulunamadı! Lütfen sahnede bir Canvas olduğundan emin olun.");
            return;
        }

        Transform canvasTransform = canvas.transform;

        // 2. ESKİ ALTYAZI PANELİ VARSA YENİLE
        Transform oldPanel = canvasTransform.Find("Subtitle_Panel");
        if (oldPanel != null)
        {
            Undo.DestroyObjectImmediate(oldPanel.gameObject);
        }

        // 3. ALTYAZI PANELİ (Subtitle_Panel) OLUŞTURMA
        GameObject panelObj = new GameObject("Subtitle_Panel");
        panelObj.transform.SetParent(canvasTransform, false);
        Undo.RegisterCreatedObjectUndo(panelObj, "Created Subtitle Panel");

        RectTransform panelRect = panelObj.AddComponent<RectTransform>();
        // Alt-Orta Hizalama (Bottom-Center Anchor)
        panelRect.anchorMin = new Vector2(0.5f, 0f);
        panelRect.anchorMax = new Vector2(0.5f, 0f);
        panelRect.pivot = new Vector2(0.5f, 0f);
        panelRect.anchoredPosition = new Vector2(0f, 50f); // Zeminden 50 piksel yukarıda
        panelRect.sizeDelta = new Vector2(750f, 65f);      // Genişlik: 750px, Yükseklik: 65px

        // Yarı Şeffaf Siyah Arka Plan (Image)
        Image bgImage = panelObj.AddComponent<Image>();
        bgImage.color = new Color(0f, 0f, 0f, 0.55f);

        // Fade Efekti İçin CanvasGroup
        CanvasGroup cg = panelObj.AddComponent<CanvasGroup>();
        cg.alpha = 0f; // Başlangıçta görünmez

        // 4. ALTYAZI METNİ (TMP_Text) OLUŞTURMA
        GameObject textObj = new GameObject("Subtitle_Text");
        textObj.transform.SetParent(panelObj.transform, false);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero; // Tam paneli kapla

        TextMeshProUGUI tmpText = textObj.AddComponent<TextMeshProUGUI>();
        tmpText.text = "Neredeyim ben...?";
        tmpText.fontSize = 26f;
        tmpText.fontStyle = FontStyles.Italic | FontStyles.Bold;
        tmpText.alignment = TextAlignmentOptions.Center;
        tmpText.color = Color.white;

        // 5. SUBTITLE MANAGER VE AUDIOSOURCE KURULUMU
        GameObject managerParent = GameObject.Find("Managers");
        if (managerParent == null)
        {
            managerParent = GameObject.Find("[--- YÖNETİCİLER & SİSTEM ---]");
        }

        Transform managerTarget = managerParent != null ? managerParent.transform : canvasTransform;

        SubtitleManager subManager = managerTarget.GetComponentInChildren<SubtitleManager>();
        if (subManager == null)
        {
            GameObject subManagerObj = new GameObject("SubtitleManager");
            subManagerObj.transform.SetParent(managerTarget, false);
            subManager = subManagerObj.AddComponent<SubtitleManager>();
            Undo.RegisterCreatedObjectUndo(subManagerObj, "Created SubtitleManager");
        }

        // AudioSource Bağlantısı
        AudioSource audioSource = subManager.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = subManager.gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        // Script Değişkenlerini Otomatik Bağlama
        subManager.voiceAudioSource = audioSource;
        subManager.subtitleText = tmpText;
        subManager.subtitleCanvasGroup = cg;
        subManager.defaultSubtitleText = "Neredeyim ben...?";

        EditorUtility.SetDirty(subManager);
        Selection.activeGameObject = subManager.gameObject;

        Debug.Log("[Dink Altyazı] Türkçe Ses ve Altyazı Sistemi başarıyla kuruldu! 'SubtitleManager' objesinden ses dosyanızı atayabilirsiniz.");
    }
}
