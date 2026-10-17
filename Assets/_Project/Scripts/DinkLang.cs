/// <summary>
/// Kodla kurulan menüler için kısa çeviri yardımcısı.
/// Metinler LanguageManager.Language sırasıyla verilir: EN, TR, DE, FR, ES, PT, RU.
/// </summary>
public static class DinkLang
{
    public static string T(params string[] byLanguage)
    {
        int index = LanguageManager.instance != null ? (int)LanguageManager.instance.CurrentLanguage : 0;
        return index < byLanguage.Length ? byLanguage[index] : byLanguage[0];
    }
}
