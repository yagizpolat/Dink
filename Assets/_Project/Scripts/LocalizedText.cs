using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Oyundaki metin bileşenlerini (TextMeshPro ve Legacy UI Text)
/// Türkçe ve İngilizce dil seçimine göre dinamik olarak günceller.
/// </summary>
public class LocalizedText : MonoBehaviour
{
    [TextArea(2, 5)]
    public string trText; // Müfettişten (Inspector) Türkçe metni gir
    [TextArea(2, 5)]
    public string enText; // Müfettişten İngilizce metni gir

    private TextMeshProUGUI tmpText;
    private Text uiText;

    private void Awake()
    {
        CacheComponents();
    }

    private void OnEnable()
    {
        UpdateText();
    }

    private void CacheComponents()
    {
        if (tmpText == null) tmpText = GetComponent<TextMeshProUGUI>();
        if (uiText == null) uiText = GetComponent<Text>();
    }

    public void UpdateText()
    {
        CacheComponents();

        // PlayerPrefs kullanarak seçimi hafızada tutuyoruz (Varsayılan: TR)
        string lang = PlayerPrefs.GetString("Language", "TR");
        string selectedText = (lang == "TR") ? trText : enText;

        if (tmpText != null)
        {
            tmpText.text = selectedText;
        }
        else if (uiText != null)
        {
            uiText.text = selectedText;
        }
    }
}