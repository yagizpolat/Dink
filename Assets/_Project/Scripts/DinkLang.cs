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

    // Oyuncu adını büyük harfe çevirir; Türkçe ad ya da Türkçe dilde i/ı doğru kalır (YAĞIZ, İREM).
    static readonly System.Globalization.CultureInfo Tr = new System.Globalization.CultureInfo("tr-TR");
    public static string Upper(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        bool tr = s.IndexOfAny("ıİçÇğĞşŞöÖüÜ".ToCharArray()) >= 0
            || (LanguageManager.instance != null && (int)LanguageManager.instance.CurrentLanguage == 1);
        return tr ? s.ToUpper(Tr) : s.ToUpperInvariant();
    }
}
