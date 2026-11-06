using System;
using UnityEngine;

/// <summary>
/// Dink Projesi 10 Dilli Dil Yöneticisi.
/// Varsayılan Dil: İngilizce (EN).
/// Diller: EN (English), TR (Türkçe), DE (Deutsch), FR (Français), ES (Español), PT (Português), RU (Русский), ZH (简体中文), JA (日本語), KO (한국어).
/// </summary>
public class LanguageManager : MonoBehaviour
{
    private static LanguageManager _instance;
    public static LanguageManager instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<LanguageManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("[LanguageManager]");
                    _instance = go.AddComponent<LanguageManager>();
                }
                else
                {
                    // Sahnedeki yönetici Awake'inden önce sorulduysa kayıtlı dili henüz yüklememiştir;
                    // yüklenmezse metinler (hikaye yazısı, defter) her dilde İngilizce çıkıyordu
                    _instance.LoadSavedLanguage();
                }
            }
            return _instance;
        }
    }

    public enum Language
    {
        EN, // English (Default)
        TR, // Türkçe
        DE, // Deutsch
        FR, // Français
        ES, // Español
        PT, // Português
        RU, // Русский
        ZH, // 简体中文
        JA, // 日本語
        KO  // 한국어
    }

    public static event Action OnLanguageChanged;

    private const string PREF_LANGUAGE = DinkPrefs.Language;
    private Language currentLanguage = Language.EN;

    public Language CurrentLanguage => currentLanguage;

    private readonly string[] languageNames = new string[]
    {
        "ENGLISH",
        "TÜRKÇE",
        "DEUTSCH",
        "FRANÇAIS",
        "ESPAÑOL",
        "PORTUGUÊS",
        "РУССКИЙ",
        "简体中文",
        "日本語",
        "한국어"
    };

    private void Awake()
    {
        if (_instance == null || _instance == this)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            LoadSavedLanguage();
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void LoadSavedLanguage()
    {
        // Oyuncu ayarlardan dil seçtiyse o kullanılır; seçmediyse bilgisayarın dili, oyunda yoksa İngilizce
        string savedLang = PlayerPrefs.GetString(PREF_LANGUAGE, "");
        if (Enum.TryParse(savedLang, out Language loadedLang))
        {
            currentLanguage = loadedLang;
        }
        else
        {
            currentLanguage = FromSystemLanguage(Application.systemLanguage);
        }
        ApplyCjkFontOrder();
    }

    private static Language FromSystemLanguage(SystemLanguage system)
    {
        switch (system)
        {
            case SystemLanguage.Turkish: return Language.TR;
            case SystemLanguage.German: return Language.DE;
            case SystemLanguage.French: return Language.FR;
            case SystemLanguage.Spanish: return Language.ES;
            case SystemLanguage.Portuguese: return Language.PT;
            case SystemLanguage.Russian: return Language.RU;
            case SystemLanguage.Chinese:
            case SystemLanguage.ChineseSimplified: return Language.ZH;
            case SystemLanguage.Japanese: return Language.JA;
            case SystemLanguage.Korean: return Language.KO;
            default: return Language.EN;
        }
    }

#if UNITY_EDITOR
    private static System.Collections.Generic.List<TMPro.TMP_FontAsset> editorOriginalOrder;

    private static void RestoreFontOrder(UnityEditor.PlayModeStateChange change)
    {
        if (change != UnityEditor.PlayModeStateChange.ExitingPlayMode || editorOriginalOrder == null) return;
        var fallbacks = TMPro.TMP_Settings.fallbackFontAssets;
        fallbacks.Clear();
        fallbacks.AddRange(editorOriginalOrder);
        editorOriginalOrder = null;
        UnityEditor.EditorApplication.playModeStateChanged -= RestoreFontOrder;
    }
#endif

    // Çince, Japonca ve Korece aynı Han karakterlerini farklı biçimde çizer; seçili dilin
    // Noto fontu TMP yedek listesinde başa alınır (yedekler: NotoSansSC, NotoSansJP, NotoSansKR)
    private void ApplyCjkFontOrder()
    {
        var fallbacks = TMPro.TMP_Settings.fallbackFontAssets;
        if (fallbacks == null || fallbacks.Count < 2) return;
        string tag = currentLanguage == Language.JA ? "JP" : currentLanguage == Language.KO ? "KR" : "SC";
        int i = fallbacks.FindIndex(f => f != null && f.name.Contains("NotoSans" + tag));
        if (i <= 0) return;
#if UNITY_EDITOR
        // Liste TMP Settings asset'inin kendisi; editörde Play bitince asıl sıraya döner, git'e değişiklik sızmaz
        if (editorOriginalOrder == null)
        {
            editorOriginalOrder = new System.Collections.Generic.List<TMPro.TMP_FontAsset>(fallbacks);
            UnityEditor.EditorApplication.playModeStateChanged += RestoreFontOrder;
        }
#endif
        var first = fallbacks[i];
        fallbacks.RemoveAt(i);
        fallbacks.Insert(0, first);
    }

    public void SetLanguage(Language lang)
    {
        currentLanguage = lang;
        PlayerPrefs.SetString(PREF_LANGUAGE, currentLanguage.ToString());
        PlayerPrefs.Save();
        ApplyCjkFontOrder();
        OnLanguageChanged?.Invoke();
        Debug.Log($"<color=cyan>[DINK] Dil Değiştirildi: {GetLanguageName(currentLanguage)} ({currentLanguage})</color>");
    }

    /// <summary>Dil listesinde ileri (+1) ya da geri (-1) gider; uçlarda başa sarar.</summary>
    public void ShiftLanguage(int delta)
    {
        int total = Enum.GetValues(typeof(Language)).Length;
        SetLanguage((Language)(((int)currentLanguage + (delta < 0 ? -1 : 1) + total) % total));
    }

    public string GetLanguageName(Language lang)
    {
        int index = (int)lang;
        if (index >= 0 && index < languageNames.Length)
            return languageNames[index];
        return lang.ToString();
    }

    public string GetCurrentLanguageName()
    {
        return GetLanguageName(currentLanguage);
    }
}
