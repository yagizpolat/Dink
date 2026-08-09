using UnityEngine;

public class Letter : MonoBehaviour
{
    [Header("Türkçe İçerik")]
    public string trTitle = "Mektup";
    [TextArea(4, 12)]
    public string trContent;

    [Header("İngilizce İçerik")]
    public string enTitle = "Letter";
    [TextArea(4, 12)]
    public string enContent;

    public string GetTitle()
    {
        string lang = PlayerPrefs.GetString("Language", "TR");
        if (lang == "TR")
        {
            return string.IsNullOrEmpty(trTitle) ? "Mektup" : trTitle;
        }
        else
        {
            return string.IsNullOrEmpty(enTitle) ? trTitle : enTitle;
        }
    }

    public string GetContent()
    {
        string lang = PlayerPrefs.GetString("Language", "TR");
        if (lang == "TR")
        {
            return string.IsNullOrEmpty(trContent) ? "" : trContent;
        }
        else
        {
            return string.IsNullOrEmpty(enContent) ? trContent : enContent;
        }
    }
}
