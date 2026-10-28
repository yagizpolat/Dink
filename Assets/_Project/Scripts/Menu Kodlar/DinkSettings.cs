using UnityEngine;

/// <summary>
/// Görüntü ve ses ayarlarının mantığı: değerleri okur, uygular, kaydeder. Arayüzü yoktur;
/// ana menüde LobbySettings, oyun içinde DoorLightSettings ekranı buradan okur ve buraya yazar.
/// Kayıtlı görüntü ayarları oyun açılırken bir kez uygulanır.
/// </summary>
public static class DinkSettings
{
    static readonly int[] frameRateValues = { 30, 60, 120, -1 };

    static Resolution[] resolutions;
    static int resIndex;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void ApplyOnStartup() => LoadAndApply();

    // ── Okuma ─────────────────────────────────────────

    public static int QualityIndex => Mathf.Clamp(PlayerPrefs.GetInt(DinkPrefs.Quality, QualitySettings.names.Length - 1), 0, QualitySettings.names.Length - 1);
    public static bool IsFullscreen => PlayerPrefs.GetInt(DinkPrefs.Fullscreen, 1) == 1;
    public static bool IsVSync => PlayerPrefs.GetInt(DinkPrefs.VSync, 1) == 1;
    public static int FrameRateIndex => Mathf.Clamp(PlayerPrefs.GetInt(DinkPrefs.FrameRate, DinkPrefs.DefaultFrameRateIndex), 0, frameRateValues.Length - 1);
    public static bool IsUnlimitedFrameRate => frameRateValues[FrameRateIndex] < 0;
    public static int FrameRateValue => frameRateValues[FrameRateIndex];

    public static string ResolutionName
    {
        get
        {
            SetupResolutions();
            return resolutions.Length > 0 ? $"{resolutions[resIndex].width}×{resolutions[resIndex].height}" : $"{Screen.width}×{Screen.height}";
        }
    }

    /// <summary>
    /// Unity kalite seviyesini DÜŞÜK/ORTA/YÜKSEK/ULTRA etiketlerinden birine eşler.
    /// İki seviye varsa DÜŞÜK ve YÜKSEK, daha fazlası varsa etiketlere orantılı dağıtılır.
    /// </summary>
    public static int QualityLabelIndex(int level)
    {
        int levels = QualitySettings.names.Length;
        if (levels <= 1) return 2;
        if (levels == 2) return level <= 0 ? 0 : 2;
        return Mathf.Clamp(Mathf.RoundToInt(level * 3f / (levels - 1)), 0, 3);
    }

    // ── Değiştirme ────────────────────────────────────
    // Seçenekler uçlarda başa sarar: Enter/tık hep +1 gönderir, en üstteyken takılıp kalmasın

    static int Wrap(int i, int count) => count <= 0 ? 0 : ((i % count) + count) % count;

    public static void ChangeQuality(int delta)
    {
        int level = Wrap(QualityIndex + delta, QualitySettings.names.Length);
        QualitySettings.SetQualityLevel(level, true);
        SaveInt(DinkPrefs.Quality, level);
    }

    public static void ToggleFullscreen()
    {
        bool full = !IsFullscreen;
        Screen.fullScreen = full;
        SaveInt(DinkPrefs.Fullscreen, full ? 1 : 0);
    }

    public static void ChangeResolution(int delta)
    {
        SetupResolutions();
        if (resolutions.Length == 0) return;
        resIndex = Wrap(resIndex + delta, resolutions.Length);
        Resolution res = resolutions[resIndex];
        Screen.SetResolution(res.width, res.height, IsFullscreen);
        PlayerPrefs.SetInt(DinkPrefs.ResWidth, res.width);
        SaveInt(DinkPrefs.ResHeight, res.height);
    }

    public static void ChangeFrameRate(int delta)
    {
        int i = Wrap(FrameRateIndex + delta, frameRateValues.Length);
        Application.targetFrameRate = frameRateValues[i];
        SaveInt(DinkPrefs.FrameRate, i);
    }

    public static void ToggleVSync()
    {
        bool on = !IsVSync;
        QualitySettings.vSyncCount = on ? 1 : 0;
        SaveInt(DinkPrefs.VSync, on ? 1 : 0);
    }

    public static void SetMasterVolume(float v) => AudioManager.instance.SetMasterVolume(v);
    public static void SetMusicVolume(float v) => AudioManager.instance.SetMusicVolume(v);
    public static void SetSfxVolume(float v) => AudioManager.instance.SetSFXVolume(v);

    public static void ResetToDefaults()
    {
        foreach (string key in new[] { DinkPrefs.Quality, DinkPrefs.Fullscreen, DinkPrefs.VSync, DinkPrefs.FrameRate,
                                       DinkPrefs.ResWidth, DinkPrefs.ResHeight,
                                       DinkPrefs.MasterVolumeKey, DinkPrefs.MusicVolumeKey, DinkPrefs.SfxVolumeKey })
            PlayerPrefs.DeleteKey(key);
        PlayerPrefs.Save();

        // Ekranın kendi çözünürlüğüne döner
        resolutions = null;
        Resolution native = Screen.currentResolution;
        Screen.SetResolution(native.width, native.height, IsFullscreen);
        LoadAndApply();
    }

    /// <summary>Kayıtlı ayarları uygular (görüntü + ses).</summary>
    public static void LoadAndApply()
    {
        QualitySettings.SetQualityLevel(QualityIndex, true);
        Screen.fullScreen = IsFullscreen;
        QualitySettings.vSyncCount = IsVSync ? 1 : 0;
        Application.targetFrameRate = FrameRateValue;
        resolutions = null;
        SetupResolutions();
        AudioManager.instance.LoadVolumeSettings();
    }

    static void SetupResolutions()
    {
        if (resolutions != null) return;
        resolutions = Screen.resolutions ?? new Resolution[0];
        int w = PlayerPrefs.GetInt(DinkPrefs.ResWidth, Screen.currentResolution.width);
        int h = PlayerPrefs.GetInt(DinkPrefs.ResHeight, Screen.currentResolution.height);
        resIndex = Mathf.Max(0, resolutions.Length - 1);
        for (int i = 0; i < resolutions.Length; i++)
            if (resolutions[i].width == w && resolutions[i].height == h) { resIndex = i; break; }
    }

    static void SaveInt(string key, int value)
    {
        PlayerPrefs.SetInt(key, value);
        PlayerPrefs.Save();
    }
}
