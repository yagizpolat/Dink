using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Oyundaki tüm metin bileşenlerini (TextMeshPro 3D, TextMeshProUGUI ve UI Text)
/// 7 dil seçeneğine göre canlı olarak günceller.
/// Varsayılan Dil: İngilizce (EN).
/// </summary>
public class LocalizedText : MonoBehaviour
{
    [Header("7 Dil Metin Karşılıkları")]
    [TextArea(2, 5)] public string enText; // İngilizce (Varsayılan)
    [TextArea(2, 5)] public string trText; // Türkçe
    [TextArea(2, 5)] public string deText; // Almanca
    [TextArea(2, 5)] public string frText; // Fransızca
    [TextArea(2, 5)] public string esText; // İspanyolca
    [TextArea(2, 5)] public string ptText; // Portekizce
    [TextArea(2, 5)] public string ruText; // Rusça

    private TextMeshProUGUI tmpUGUI;
    private TextMeshPro tmp3D;
    private Text uiText;

    private void Awake()
    {
        CacheComponents();
    }

    private void OnEnable()
    {
        LanguageManager.OnLanguageChanged += UpdateText;
        UpdateText();
    }

    private void OnDisable()
    {
        LanguageManager.OnLanguageChanged -= UpdateText;
    }

    private void CacheComponents()
    {
        if (tmpUGUI == null) tmpUGUI = GetComponent<TextMeshProUGUI>();
        if (tmp3D == null) tmp3D = GetComponent<TextMeshPro>();
        if (uiText == null) uiText = GetComponent<Text>();
    }

    public void UpdateText()
    {
        CacheComponents();

        LanguageManager.Language activeLang = LanguageManager.Language.EN;
        if (LanguageManager.instance != null)
        {
            activeLang = LanguageManager.instance.CurrentLanguage;
        }
        else
        {
            string saved = PlayerPrefs.GetString("Dink_Language", "EN");
            System.Enum.TryParse(saved, out activeLang);
        }

        string selectedText = GetTextForLanguage(activeLang);

        // Seçilen dilde metin boşsa İngilizce'ye düş, o da boşsa Türkçe'ye düş
        if (string.IsNullOrEmpty(selectedText))
        {
            selectedText = !string.IsNullOrEmpty(enText) ? enText : trText;
        }

        if (tmpUGUI != null)
        {
            tmpUGUI.text = selectedText;
        }
        else if (tmp3D != null)
        {
            tmp3D.text = selectedText;
        }
        else if (uiText != null)
        {
            uiText.text = selectedText;
        }
    }

    private string GetTextForLanguage(LanguageManager.Language lang)
    {
        switch (lang)
        {
            case LanguageManager.Language.EN: return enText;
            case LanguageManager.Language.TR: return trText;
            case LanguageManager.Language.DE: return deText;
            case LanguageManager.Language.FR: return frText;
            case LanguageManager.Language.ES: return esText;
            case LanguageManager.Language.PT: return ptText;
            case LanguageManager.Language.RU: return ruText;
            default: return enText;
        }
    }
}