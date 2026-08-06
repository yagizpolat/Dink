using UnityEngine;
using TMPro;

public class LocalizedText : MonoBehaviour
{
    public string trText; // Müfettişten (Inspector) Türkçe metni gir
    public string enText; // Müfettişten İngilizce metni gir

    private TextMeshProUGUI textElement;

    void Start()
    {
        textElement = GetComponent<TextMeshProUGUI>();
        UpdateText();
    }

    public void UpdateText()
    {

        // Bileşen atanmamışsa hata verme, sadece dön
        if (textElement == null && GetComponent<UnityEngine.UI.Text>() == null)
        {
            Debug.LogWarning(gameObject.name + " üzerinde metin bileşeni bulunamadı!");
            return;
        }
        // PlayerPrefs kullanarak seçimi hafızada tutacağız
        string lang = PlayerPrefs.GetString("Language", "TR");
        textElement.text = (lang == "TR") ? trText : enText;
    }
}