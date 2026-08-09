using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Dink projesi için sinematik Türkçe seslendirme ve altyazı (Subtitle) yönetim bileşeni.
/// </summary>
public class SubtitleManager : MonoBehaviour
{
    [Header("UI & Component Connections")]
    public AudioSource voiceAudioSource;
    public TMP_Text subtitleText;
    public CanvasGroup subtitleCanvasGroup;

    [Header("Intro Voice Settings")]
    public AudioClip introVoiceClip;
    [TextArea(2, 4)]
    public string defaultSubtitleText = "Neredeyim ben...?";
    public float displayDuration = 10f;
    public float fadeInDuration = 0.4f;
    public float fadeOutDuration = 0.6f;

    [Header("Başlatma Modu")]
    [Tooltip("True ise sahne başında otomatik çalar. False ise IntroCinematic veya başka bir script tetikler.")]
    public bool otomatikBaslat = false;

    private Coroutine activeSubtitleCoroutine;

    private void Start()
    {
        // Sahne açılır açılmaz sesin kendiliğinden çalmasını engelle
        if (voiceAudioSource != null)
            voiceAudioSource.playOnAwake = false;

        // Panel her zaman aktif; sadece alpha ile göster/gizle
        if (subtitleCanvasGroup != null)
        {
            subtitleCanvasGroup.gameObject.SetActive(true);
            subtitleCanvasGroup.alpha = 0f;
        }

        if (otomatikBaslat)
            ShowSubtitle(defaultSubtitleText, introVoiceClip, displayDuration);
    }

    public void ShowSubtitle(string text, AudioClip voiceClip = null, float duration = 3.5f)
    {
        if (activeSubtitleCoroutine != null)
            StopCoroutine(activeSubtitleCoroutine);

        activeSubtitleCoroutine = StartCoroutine(SubtitleSequence(text, voiceClip, duration));
    }

    private IEnumerator SubtitleSequence(string text, AudioClip voiceClip, float duration)
    {
        // Ses klibi script alanına değil doğrudan AudioSource'a atanmışsa, oradan al
        if (voiceClip == null && voiceAudioSource != null)
            voiceClip = voiceAudioSource.clip;

        // Metni yaz
        if (subtitleText != null)
            subtitleText.text = text;

        // Sesi çal
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

        // displayDuration kadar bekle
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
        // SetActive(false) YOK — panel her zaman aktif kalır, sadece alpha=0
    }
}
