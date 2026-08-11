using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Dink projesi 7 Dilli sinematik seslendirme ve altyazı (Subtitle) yönetim bileşeni.
/// Varsayılan Dil: İngilizce (EN).
/// Diller: EN, TR, DE, FR, ES, PT, RU.
/// </summary>
public class SubtitleManager : MonoBehaviour
{
    [Header("UI & Bileşen Bağlantıları")]
    public AudioSource voiceAudioSource;
    public TMP_Text subtitleText;
    public CanvasGroup subtitleCanvasGroup;

    [Header("7 Dil - Ses Kayıtları (AudioClip)")]
    public AudioClip enVoiceClip; // EN: "Where am I...?" (Varsayılan)
    public AudioClip trVoiceClip; // TR: "Neredeyim ben...?"
    public AudioClip deVoiceClip; // DE: "Wo bin ich...?"
    public AudioClip frVoiceClip; // FR: "Où suis-je...?"
    public AudioClip esVoiceClip; // ES: "¿Dónde estoy...?"
    public AudioClip ptVoiceClip; // PT: "Onde estou...?"
    public AudioClip ruVoiceClip; // RU: "Где я...?"

    [Header("7 Dil - Altyazı Metinleri (Subtitle)")]
    [TextArea(2, 4)] public string enSubtitleText = "Where am I...?";
    [TextArea(2, 4)] public string trSubtitleText = "Neredeyim ben...?";
    [TextArea(2, 4)] public string deSubtitleText = "Wo bin ich...?";
    [TextArea(2, 4)] public string frSubtitleText = "Où suis-je...?";
    [TextArea(2, 4)] public string esSubtitleText = "¿Dónde estoy...?";
    [TextArea(2, 4)] public string ptSubtitleText = "Onde estou...?";
    [TextArea(2, 4)] public string ruSubtitleText = "Где я...?";

    [Header("Legacy / Fallback")]
    public AudioClip introVoiceClip;
    [TextArea(2, 4)]
    public string defaultSubtitleText = "Where am I...?";

    [Header("Zamanlama Ayarları")]
    public float displayDuration = 10f;
    public float fadeInDuration = 0.4f;
    public float fadeOutDuration = 0.6f;

    [Header("Başlatma Modu")]
    [Tooltip("True ise sahne başında otomatik çalar. False ise IntroCinematic veya başka bir script tetikler.")]
    public bool otomatikBaslat = false;

    private Coroutine activeSubtitleCoroutine;

    private void Start()
    {
        if (voiceAudioSource != null)
            voiceAudioSource.playOnAwake = false;

        if (subtitleCanvasGroup != null)
        {
            subtitleCanvasGroup.gameObject.SetActive(true);
            subtitleCanvasGroup.alpha = 0f;
        }

        if (otomatikBaslat)
            ShowIntroSubtitle();
    }

    public string GetIntroSubtitleText()
    {
        LanguageManager.Language lang = GetActiveLanguage();

        switch (lang)
        {
            case LanguageManager.Language.EN: return !string.IsNullOrEmpty(enSubtitleText) ? enSubtitleText : defaultSubtitleText;
            case LanguageManager.Language.TR: return !string.IsNullOrEmpty(trSubtitleText) ? trSubtitleText : enSubtitleText;
            case LanguageManager.Language.DE: return !string.IsNullOrEmpty(deSubtitleText) ? deSubtitleText : enSubtitleText;
            case LanguageManager.Language.FR: return !string.IsNullOrEmpty(frSubtitleText) ? frSubtitleText : enSubtitleText;
            case LanguageManager.Language.ES: return !string.IsNullOrEmpty(esSubtitleText) ? esSubtitleText : enSubtitleText;
            case LanguageManager.Language.PT: return !string.IsNullOrEmpty(ptSubtitleText) ? ptSubtitleText : enSubtitleText;
            case LanguageManager.Language.RU: return !string.IsNullOrEmpty(ruSubtitleText) ? ruSubtitleText : enSubtitleText;
            default: return enSubtitleText;
        }
    }

    public AudioClip GetIntroVoiceClip()
    {
        LanguageManager.Language lang = GetActiveLanguage();

        AudioClip clip = null;
        switch (lang)
        {
            case LanguageManager.Language.EN: clip = enVoiceClip; break;
            case LanguageManager.Language.TR: clip = trVoiceClip; break;
            case LanguageManager.Language.DE: clip = deVoiceClip; break;
            case LanguageManager.Language.FR: clip = frVoiceClip; break;
            case LanguageManager.Language.ES: clip = esVoiceClip; break;
            case LanguageManager.Language.PT: clip = ptVoiceClip; break;
            case LanguageManager.Language.RU: clip = ruVoiceClip; break;
        }

        // Seçilen dilin ses klibi yoksa İngilizce'ye, o da yoksa varsayılana düş
        if (clip == null) clip = enVoiceClip;
        if (clip == null) clip = trVoiceClip;
        if (clip == null) clip = introVoiceClip;

        return clip;
    }

    private LanguageManager.Language GetActiveLanguage()
    {
        if (LanguageManager.instance != null)
        {
            return LanguageManager.instance.CurrentLanguage;
        }
        else
        {
            string saved = PlayerPrefs.GetString("Dink_Language", "EN");
            if (System.Enum.TryParse(saved, out LanguageManager.Language parsed))
                return parsed;
            return LanguageManager.Language.EN;
        }
    }

    public void ShowIntroSubtitle()
    {
        ShowSubtitle(GetIntroSubtitleText(), GetIntroVoiceClip(), displayDuration);
    }

    public void ShowSubtitle(string text, AudioClip voiceClip = null, float duration = 3.5f)
    {
        if (activeSubtitleCoroutine != null)
            StopCoroutine(activeSubtitleCoroutine);

        activeSubtitleCoroutine = StartCoroutine(SubtitleSequence(text, voiceClip, duration));
    }

    private IEnumerator SubtitleSequence(string text, AudioClip voiceClip, float duration)
    {
        if (voiceClip == null && voiceAudioSource != null)
            voiceClip = voiceAudioSource.clip;

        if (subtitleText != null)
            subtitleText.text = text;

        if (voiceClip != null && voiceAudioSource != null)
        {
            voiceAudioSource.Stop();
            voiceAudioSource.clip = voiceClip;
            voiceAudioSource.Play();
        }

        // Fade-In
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            if (subtitleCanvasGroup != null)
                subtitleCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInDuration);
            yield return null;
        }
        if (subtitleCanvasGroup != null) subtitleCanvasGroup.alpha = 1f;

        yield return new WaitForSeconds(duration);

        // Fade-Out
        elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            if (subtitleCanvasGroup != null)
                subtitleCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeOutDuration);
            yield return null;
        }

        if (subtitleCanvasGroup != null)
            subtitleCanvasGroup.alpha = 0f;
    }
}
