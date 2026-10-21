using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Dink Projesi Otomatik Bağlantılı Ses ve Müzik Yöneticisi (AudioManager).
/// Bağımsız Müzik (Music), Ses Efekti (SFX) ve Genel Ses (Master) kontrolü sağlar.
/// </summary>
public class AudioManager : MonoBehaviour
{
    private static AudioManager _instance;
    public static AudioManager instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<AudioManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("[AudioManager]");
                    _instance = go.AddComponent<AudioManager>();
                }
            }
            return _instance;
        }
    }

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Müzikler & Ambiyans")]
    public AudioClip menuMusic;   // in_menu.mp3
    public AudioClip gameAmbience; // in_game.mp3

    [Header("Ortak Ses Efektleri")]
    public AudioClip buttonClickSFX;

    private bool _isFading = false;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        SetupAudioSources();
        AutoLoadAudioClips();
        LoadVolumeSettings();

        // Sahneye göre müziği başlat
        PlayMusic(MusicFor(SceneManager.GetActiveScene()));
    }

    // Apartman, ESC menüsü dahil, müziksizdir: oradaki her şey dünyanın kendi sesi
    private AudioClip MusicFor(Scene scene)
    {
        if (DinkScenes.IsApartment(scene)) return null;
        return DinkScenes.IsMainMenu(scene) ? menuMusic : gameAmbience;
    }

    // Apartman sahnelerinde ses kaynakları sahneye dağılmış durumda (asansör, floresan, kalem...);
    // müzik olmadığı için Ses Efekti ayarı hepsine dinleyici üzerinden uygulanır
    private void ApplyListenerVolume()
    {
        bool apartment = DinkScenes.IsApartment(SceneManager.GetActiveScene());
        AudioListener.volume = GetMasterVolume() * (apartment ? GetSFXVolume() : 1f);
        if (sfxSource != null) sfxSource.volume = SFXSourceScale;
    }

    /// <summary>
    /// Ses efekti kaynağına verilecek çarpan. Apartmanda Ses Efekti ayarı zaten dinleyicide olduğu için 1;
    /// başka yerde ayarın kendisi. Kendi AudioSource'unu SFX ayarıyla kısan betikler bunu kullanmalı.
    /// </summary>
    public float SFXSourceScale => DinkScenes.IsApartment(SceneManager.GetActiveScene()) ? 1f : GetSFXVolume();

    // Apartman sahnesi doğrudan açıldığında da (editör, Yeni Oyun) yönetici baştan kurulsun
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot()
    {
        if (DinkScenes.IsApartment(SceneManager.GetActiveScene())) _ = instance;
    }

    private void SetupAudioSources()
    {
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
        }
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void AutoLoadAudioClips()
    {
        if (menuMusic == null)
        {
            menuMusic = Resources.Load<AudioClip>("Audio/in_menu");
        }
        if (gameAmbience == null)
        {
            gameAmbience = Resources.Load<AudioClip>("Audio/in_game");
        }
    }

    public void LoadVolumeSettings()
    {
        SetMasterVolume(GetMasterVolume());
        SetMusicVolume(GetMusicVolume());
        SetSFXVolume(GetSFXVolume());
    }

    public float GetMasterVolume() => PlayerPrefs.GetFloat("Dink_MasterVolume", 0.8f);
    public float GetMusicVolume() => PlayerPrefs.GetFloat("Dink_MusicVolume", 0.5f);
    public float GetSFXVolume() => PlayerPrefs.GetFloat("Dink_SFXVolume", 0.8f);

    public void SetMasterVolume(float vol)
    {
        PlayerPrefs.SetFloat("Dink_MasterVolume", vol);
        ApplyListenerVolume();
    }

    public void SetMusicVolume(float vol)
    {
        PlayerPrefs.SetFloat("Dink_MusicVolume", vol);
        if (musicSource != null && !_isFading)
        {
            musicSource.volume = vol;
        }
    }

    public void SetSFXVolume(float vol)
    {
        PlayerPrefs.SetFloat("Dink_SFXVolume", vol);
        ApplyListenerVolume();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        AutoLoadAudioClips();
        ApplyListenerVolume();
        AudioClip targetClip = MusicFor(scene);

        if (targetClip == null)
        {
            if (musicSource != null && musicSource.isPlaying) StopMusicWithFade(1.2f);
        }
        else
        {
            if (musicSource.clip == null || !musicSource.isPlaying || IsSameClip(musicSource.clip, targetClip))
            {
                PlayMusic(targetClip);
            }
            else
            {
                ChangeMusicWithFade(targetClip, 1.2f);
            }
        }
    }

    private bool IsSameClip(AudioClip a, AudioClip b)
    {
        if (a == b) return true;
        if (a != null && b != null && a.name == b.name) return true;
        return false;
    }

    // ════════════════════════════════════════════
    // SES EFEKTİ VE MÜZİK YÖNETİMİ
    // ════════════════════════════════════════════

    public void PlayClickSFX()
    {
        PlaySFX(buttonClickSFX);
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            // Kaynağın volume'u zaten SFX ayarında; burada ikinci kez çarpılmaz
            sfxSource.PlayOneShot(clip);
        }
    }

    public void PlayMusic(AudioClip clip)
    {
        if (clip == null || musicSource == null) return;

        if (musicSource.isPlaying && IsSameClip(musicSource.clip, clip)) return;

        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.volume = GetMusicVolume();
        musicSource.Play();
    }

    public void StopMusicWithFade(float fadeDuration)
    {
        if (musicSource == null) return;
        StopAllCoroutines();
        StartCoroutine(FadeOutAndStop(fadeDuration));
    }

    private IEnumerator FadeOutAndStop(float duration)
    {
        _isFading = true;
        float currentTime = 0;
        float startVolume = musicSource.volume > 0 ? musicSource.volume : GetMusicVolume();

        while (currentTime < duration)
        {
            currentTime += Time.unscaledDeltaTime; // geçişlerde zaman donuk olabilir
            musicSource.volume = Mathf.Lerp(startVolume, 0, currentTime / duration);
            yield return null;
        }

        musicSource.Stop();
        musicSource.clip = null;
        musicSource.volume = GetMusicVolume();
        _isFading = false;
    }

    public void ChangeMusicWithFade(AudioClip newClip, float fadeDuration)
    {
        if (musicSource == null || newClip == null) return;
        if (musicSource.isPlaying && IsSameClip(musicSource.clip, newClip)) return;

        StopAllCoroutines();
        StartCoroutine(FadeOutAndIn(newClip, fadeDuration));
    }

    private IEnumerator FadeOutAndIn(AudioClip newClip, float duration)
    {
        _isFading = true;
        float currentTime = 0;
        float targetVolume = GetMusicVolume();
        float startVolume = musicSource.volume;

        while (currentTime < duration)
        {
            currentTime += Time.unscaledDeltaTime; // geçişlerde zaman donuk olabilir
            musicSource.volume = Mathf.Lerp(startVolume, 0, currentTime / duration);
            yield return null;
        }

        musicSource.Stop();
        musicSource.volume = 0;

        musicSource.clip = newClip;
        if (newClip != null)
        {
            musicSource.loop = true;
            musicSource.Play();
        }

        currentTime = 0;
        while (currentTime < duration)
        {
            currentTime += Time.unscaledDeltaTime; // geçişlerde zaman donuk olabilir
            musicSource.volume = Mathf.Lerp(0, targetVolume, currentTime / duration);
            yield return null;
        }

        musicSource.volume = targetVolume;
        _isFading = false;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}