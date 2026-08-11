using System;
using UnityEngine;

/// <summary>
/// Dink Projesi 7 Dilli Dil Yöneticisi.
/// Varsayılan Dil: İngilizce (EN).
/// Diller: EN (English), TR (Türkçe), DE (Deutsch), FR (Français), ES (Español), PT (Português), RU (Русский).
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
        RU  // Русский
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
        "РУССКИЙ"
    };

    private void Awake()
    {
        if (_instance == null)
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
    }

    public void SetLanguage(Language lang)
    {
        currentLanguage = lang;
        PlayerPrefs.SetString(PREF_LANGUAGE, currentLanguage.ToString());
        PlayerPrefs.Save();
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
