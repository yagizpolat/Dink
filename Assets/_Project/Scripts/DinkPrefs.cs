using UnityEngine;

/// <summary>
/// Dink'in PlayerPrefs anahtarlarının ve varsayılan değerlerinin tek kaynağı.
/// Başka dosyalar "Dink_..." anahtarını elle yazmaz; buradaki sabitleri ya da özellikleri kullanır.
/// </summary>
public static class DinkPrefs
{
    // Ayarlar
    public const string Language = "Dink_Language";
    public const string Quality = "Dink_QualityLevel";
    public const string Fullscreen = "Dink_Fullscreen";
    public const string VSync = "Dink_VSync";
    public const string FrameRate = "Dink_FrameRate";
    public const string ResWidth = "Dink_ResWidth";
    public const string ResHeight = "Dink_ResHeight";
    public const string MasterVolumeKey = "Dink_MasterVolume";
    public const string MusicVolumeKey = "Dink_MusicVolume";
    public const string SfxVolumeKey = "Dink_SFXVolume";

    public const float DefaultMasterVolume = 0.8f;
    public const float DefaultMusicVolume = 0.5f;
    public const float DefaultSfxVolume = 0.8f;
    public const int DefaultFrameRateIndex = 1; // 60 FPS

    // Kayıt
    public const string SaveLastFloor = "Dink_Save_LastFloor";
    public const string SaveTraces = "Dink_Save_Traces";
    public const string SavePlayerName = "Dink_Save_PlayerName";
    public const string SaveElapsed = "Dink_Save_Elapsed";
    public const string LoopCount = "Dink_LoopCount";
    public const string IntroSeen = "Dink_IntroSeen";

    // Skor tablosu
    public const string LeaderboardData = "Dink_LeaderboardData";
    public const string GameFinished = "Dink_GameFinished";

    // Eski skor adı anahtarı; ad artık yalnız SavePlayerName'de tutulur (ilk açılışta taşınır)
    const string LegacyNickname = "Dink_PlayerNickname";

    public static float MasterVolume
    {
        get => PlayerPrefs.GetFloat(MasterVolumeKey, DefaultMasterVolume);
        set => PlayerPrefs.SetFloat(MasterVolumeKey, value);
    }

    public static float MusicVolume
    {
        get => PlayerPrefs.GetFloat(MusicVolumeKey, DefaultMusicVolume);
        set => PlayerPrefs.SetFloat(MusicVolumeKey, value);
    }

    public static float SfxVolume
    {
        get => PlayerPrefs.GetFloat(SfxVolumeKey, DefaultSfxVolume);
        set => PlayerPrefs.SetFloat(SfxVolumeKey, value);
    }

    /// <summary>
    /// Kendi AudioSource'unu ses efekti ayarıyla kısan betikler için çarpan
    /// (apartmanda ayar zaten dinleyicide olduğu için 1).
    /// </summary>
    public static float SfxSourceScale => AudioManager.instance.SFXSourceScale;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void MigrateLegacyKeys()
    {
        if (!PlayerPrefs.HasKey(LegacyNickname)) return;
        string old = PlayerPrefs.GetString(LegacyNickname, "");
        if (string.IsNullOrWhiteSpace(PlayerPrefs.GetString(SavePlayerName, "")) && !string.IsNullOrWhiteSpace(old))
            PlayerPrefs.SetString(SavePlayerName, old.Trim());
        PlayerPrefs.DeleteKey(LegacyNickname);
        PlayerPrefs.Save();
    }
}
