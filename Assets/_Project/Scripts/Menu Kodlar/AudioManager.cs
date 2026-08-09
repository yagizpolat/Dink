using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Audio Clips")]
    public AudioClip menuMusic;
    public AudioClip gameMusic;
    public AudioClip buttonClickSFX;

    private void Awake()
    {
        // Singleton Yapısı: Sahneler arası müziğin kopmaması için
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Eğer Ana Menüye dönüldüyse (Index 0) ve şu an çalan menü müziği değilse
        if (scene.buildIndex == 0 && musicSource.clip != menuMusic)
        {
            ChangeMusicWithFade(menuMusic, 2f);
        }
    }

    private void Start()
    {
        // Oyun ilk açıldığında menü müziğini başlat
        PlayMusic(menuMusic);
    }

    // --- SES EFEKTLERİ ---
    public void PlayClickSFX()
    {
        if (buttonClickSFX != null)
            sfxSource.PlayOneShot(buttonClickSFX);
    }

    // --- MÜZİK GEÇİŞ SİSTEMİ ---
    public void PlayMusic(AudioClip clip)
    {
        musicSource.clip = clip;
        musicSource.Play();
    }

    public void ChangeMusicWithFade(AudioClip newClip, float fadeDuration)
    {
        // Eğer zaten o müzik çalıyorsa tekrar başlatma
        if (musicSource.clip == newClip) return;

        StopAllCoroutines(); // Çakışmaları önlemek için eski fade'leri durdur
        StartCoroutine(FadeOutAndIn(newClip, fadeDuration));
    }

    /// <summary>
    /// Müziği yumuşakça kısarak (Fade Out) tamamen durdurur. Oyuna geçerken sessizlik sağlamak için kullanılır.
    /// </summary>
    public void StopMusicWithFade(float fadeDuration)
    {
        StopAllCoroutines();
        StartCoroutine(FadeOutAndStop(fadeDuration));
    }

    private IEnumerator FadeOutAndStop(float duration)
    {
        float currentTime = 0;
        float startVolume = musicSource.volume > 0 ? musicSource.volume : 0.5f;

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0, currentTime / duration);
            yield return null;
        }

        musicSource.Stop();
        musicSource.clip = null;
        musicSource.volume = 0.5f; // Bir sonraki müzik çalınacağı zaman varsayılan ses seviyesi
    }

    private IEnumerator FadeOutAndIn(AudioClip newClip, float duration)
    {
        float currentTime = 0;
        float startVolume = 0.5f;

        // 1. SESİ YAVAŞÇA KIS (Fade Out)
        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0, currentTime / duration);
            yield return null;
        }

        musicSource.Stop();
        musicSource.volume = 0;

        // --- TAM SESSİZLİK ANI ---
        // Burada yeni klibi ata
        musicSource.clip = newClip;
        musicSource.Play();

        // 2. YENİ SESİ YAVAŞÇA AÇ (Fade In)
        currentTime = 0;
        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(0, startVolume, currentTime / duration);
            yield return null;
        }
    }
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}