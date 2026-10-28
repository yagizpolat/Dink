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

    private const string PREF_LANGUAGE = "Dink_Language";
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
        string savedLang = PlayerPrefs.GetString(PREF_LANGUAGE, "EN");
        if (Enum.TryParse(savedLang, out Language loadedLang))
        {
            currentLanguage = loadedLang;
        }
        else
        {
            currentLanguage = Language.EN;
        }
        ApplyCjkFontOrder();
    }

    // Çince, Japonca ve Korece aynı Han karakterlerini farklı biçimde çizer; seçili dilin
    // Noto fontu TMP yedek listesinde başa alınır (yedekler: NotoSansSC, NotoSansJP, NotoSansKR)
    private void ApplyCjkFontOrder()
    {
        var fallbacks = TMPro.TMP_Settings.fallbackFontAssets;
        if (fallbacks == null || fallbacks.Count < 2) return;
        string tag = currentLanguage == Language.JA ? "JP" : currentLanguage == Language.KO ? "KR" : "SC";
        int i = fallbacks.FindIndex(f => f != null && f.name.Contains("NotoSans" + tag));
        if (i <= 0) return;
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

    public void NextLanguage()
    {
        int totalLangs = Enum.GetValues(typeof(Language)).Length;
        int nextIndex = ((int)currentLanguage + 1) % totalLangs;
        SetLanguage((Language)nextIndex);
    }

    public void PreviousLanguage()
    {
        int totalLangs = Enum.GetValues(typeof(Language)).Length;
        int prevIndex = ((int)currentLanguage - 1 + totalLangs) % totalLangs;
        SetLanguage((Language)prevIndex);
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
