using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Dink Projesi AAA Seviyesi Ayarlar Yöneticisi:
/// 3D Kapılara tıklandığında açılan AAA kalitesindeki ayar panellerini yönetir.
/// Tüm buton ve slider bağlantıları Start() içinde runtime'da kurulur — Play modunda kesin çalışır.
/// </summary>
public class SettingsManager : MonoBehaviour
{
    public static SettingsManager instance;

    [Header("Sağ Taraf Canlı Açıklama Paneli")]
    public TMP_Text descriptionTitleText;
    public TMP_Text descriptionBodyText;

    [Header("Görüntü - Değer Metinleri")]
    public TMP_Text qualityValueText;
    public TMP_Text resolutionValueText;
    public TMP_Text displayModeValueText;
    public TMP_Text vsyncValueText;
    public TMP_Text frameRateValueText;

    [Header("Görüntü - Stepper Butonları")]
    public Button qualityLeftBtn;
    public Button qualityRightBtn;
    public Button displayModeBtn;      // Sol < buton (toggle)
    public Button displayModeRightBtn; // Sağ > buton (toggle)
    public Button resolutionLeftBtn;
    public Button resolutionRightBtn;
    public Button frameRateLeftBtn;
    public Button frameRateRightBtn;
    public Button vsyncBtn;            // Sol < buton (toggle)
    public Button vsyncRightBtn;       // Sağ > buton (toggle)

    [Header("Ses - Sliderlar")]
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    public TMP_Text masterVolumePercentText;
    public TMP_Text musicVolumePercentText;
    public TMP_Text sfxVolumePercentText;

    [Header("Alt Bar Butonları")]
    public Button resetButton;
    public Button backButton;

    // Dahili Seçenek Listeleri
    private Resolution[] resolutions;
    private int currentResIndex = 0;
    private int currentQualityIndex = 2;
    private int currentFrameRateIndex = 1;
    private bool isFullscreen = true;
    private bool isVSync = true;

    private readonly string[] qualityNames = { "DÜŞÜK", "ORTA", "YÜKSEK", "ULTRA" };
    private readonly string[] frameRateOptions = { "30 FPS", "60 FPS", "120 FPS", "SINIRSIZ" };
    private readonly int[] frameRateValues = { 30, 60, 120, -1 };

    // PlayerPrefs Sabitleri
    private const string PREF_QUALITY = "Dink_QualityLevel";
    private const string PREF_FULLSCREEN = "Dink_Fullscreen";
    private const string PREF_VSYNC = "Dink_VSync";
    private const string PREF_FRAMERATE = "Dink_FrameRate";
    private const string PREF_RES_WIDTH = "Dink_ResWidth";
    private const string PREF_RES_HEIGHT = "Dink_ResHeight";
    private const string PREF_MASTER_VOL = "Dink_MasterVolume";
    private const string PREF_MUSIC_VOL = "Dink_MusicVolume";
    private const string PREF_SFX_VOL = "Dink_SFXVolume";

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        SetupResolutions();
        WireUpListeners();
        LoadAndApplySettings();
    }

    // ════════════════════════════════════════════
    // 🔌 RUNTIME BAĞLANTI KURULUMU
    // ════════════════════════════════════════════

    /// <summary>
    /// Tüm buton onClick ve slider onValueChanged bağlantılarını runtime'da kurar.
    /// Editor aracı sadece referansları atar, bağlantıyı bu metot yapar.
    /// </summary>
    private void WireUpListeners()
    {
        // Stepper butonları
        if (qualityLeftBtn != null) qualityLeftBtn.onClick.AddListener(() => ChangeQuality(-1));
        if (qualityRightBtn != null) qualityRightBtn.onClick.AddListener(() => ChangeQuality(1));
        if (displayModeBtn != null) displayModeBtn.onClick.AddListener(() => ToggleFullscreen());
        if (displayModeRightBtn != null) displayModeRightBtn.onClick.AddListener(() => ToggleFullscreen());
        if (resolutionLeftBtn != null) resolutionLeftBtn.onClick.AddListener(() => ChangeResolution(-1));
        if (resolutionRightBtn != null) resolutionRightBtn.onClick.AddListener(() => ChangeResolution(1));
        if (frameRateLeftBtn != null) frameRateLeftBtn.onClick.AddListener(() => ChangeFrameRate(-1));
        if (frameRateRightBtn != null) frameRateRightBtn.onClick.AddListener(() => ChangeFrameRate(1));
        if (vsyncBtn != null) vsyncBtn.onClick.AddListener(() => ToggleVSync());
        if (vsyncRightBtn != null) vsyncRightBtn.onClick.AddListener(() => ToggleVSync());

        // Slider'lar
        if (masterVolumeSlider != null) masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
        if (musicVolumeSlider != null) musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        if (sfxVolumeSlider != null) sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);

        // Alt bar
        if (resetButton != null) resetButton.onClick.AddListener(ResetToDefaults);
        if (backButton != null) backButton.onClick.AddListener(GoBack);
    }

    private GameObject _previousMenuToReturn;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && gameObject.activeSelf)
        {
            GoBack();
        }
    }

    public void OpenSettingsFromPauseMenu(GameObject previousMenu)
    {
        _previousMenuToReturn = previousMenu;
        if (previousMenu != null) previousMenu.SetActive(false);

        gameObject.SetActive(true);
        LoadAndApplySettings();
    }

    public void OpenSettings()
    {
        _previousMenuToReturn = null;
        gameObject.SetActive(true);
        LoadAndApplySettings();
    }

    public void GoBack()
    {
        gameObject.SetActive(false);

        if (_previousMenuToReturn != null)
        {
            _previousMenuToReturn.SetActive(true);
            _previousMenuToReturn = null;
        }
        else
        {
            MainMenu3DController ctrl = FindFirstObjectByType<MainMenu3DController>();
            if (ctrl != null) ctrl.ShowMainDoors();
        }
    }

    // ════════════════════════════════════════════
    // 📖 SAĞ TARAF CANLI AÇIKLAMA PANELİ
    // ════════════════════════════════════════════

    public void SetDescription(string title, string description)
    {
        if (descriptionTitleText != null) descriptionTitleText.text = title;
        if (descriptionBodyText != null) descriptionBodyText.text = description;
    }

    public void ClearDescription()
    {
        if (descriptionTitleText != null) descriptionTitleText.text = "AYARLAR";
        if (descriptionBodyText != null) descriptionBodyText.text = "Detaylarını görmek için bir ayarın üzerine gelin.";
    }

    // ════════════════════════════════════════════
    // 🖥️ GÖRÜNTÜ AYARLARI
    // ════════════════════════════════════════════

    private void SetupResolutions()
    {
        resolutions = Screen.resolutions;
        if (resolutions == null || resolutions.Length == 0) return;

        int savedW = PlayerPrefs.GetInt(PREF_RES_WIDTH, Screen.currentResolution.width);
        int savedH = PlayerPrefs.GetInt(PREF_RES_HEIGHT, Screen.currentResolution.height);

        for (int i = 0; i < resolutions.Length; i++)
        {
            if (resolutions[i].width == savedW && resolutions[i].height == savedH)
            { currentResIndex = i; break; }
        }
    }

    public void ChangeQuality(int delta)
    {
        currentQualityIndex = Mathf.Clamp(currentQualityIndex + delta, 0, qualityNames.Length - 1);
        QualitySettings.SetQualityLevel(currentQualityIndex, true);
        PlayerPrefs.SetInt(PREF_QUALITY, currentQualityIndex);
        PlayerPrefs.Save();
        UpdateQualityText();
        PlayClickSFX();
    }

    public void ChangeResolution(int delta)
    {
        if (resolutions == null || resolutions.Length == 0) return;
        currentResIndex = Mathf.Clamp(currentResIndex + delta, 0, resolutions.Length - 1);
        Resolution res = resolutions[currentResIndex];
        Screen.SetResolution(res.width, res.height, isFullscreen);
        PlayerPrefs.SetInt(PREF_RES_WIDTH, res.width);
        PlayerPrefs.SetInt(PREF_RES_HEIGHT, res.height);
        PlayerPrefs.Save();
        UpdateResolutionText();
        PlayClickSFX();
    }

    public void ToggleFullscreen()
    {
        isFullscreen = !isFullscreen;
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt(PREF_FULLSCREEN, isFullscreen ? 1 : 0);
        PlayerPrefs.Save();
        UpdateDisplayModeText();
        PlayClickSFX();
    }

    public void ToggleVSync()
    {
        isVSync = !isVSync;
        QualitySettings.vSyncCount = isVSync ? 1 : 0;
        PlayerPrefs.SetInt(PREF_VSYNC, isVSync ? 1 : 0);
        PlayerPrefs.Save();
        UpdateVSyncText();
        PlayClickSFX();
    }

    public void ChangeFrameRate(int delta)
    {
        currentFrameRateIndex = Mathf.Clamp(currentFrameRateIndex + delta, 0, frameRateOptions.Length - 1);
        Application.targetFrameRate = frameRateValues[currentFrameRateIndex];
        PlayerPrefs.SetInt(PREF_FRAMERATE, currentFrameRateIndex);
        PlayerPrefs.Save();
        UpdateFrameRateText();
        PlayClickSFX();
    }

    // Metin Güncelleyicileri
    private void UpdateQualityText() { if (qualityValueText != null) qualityValueText.text = qualityNames[currentQualityIndex]; }
    private void UpdateDisplayModeText() { if (displayModeValueText != null) displayModeValueText.text = isFullscreen ? "TAM EKRAN" : "PENCERELİ"; }
    private void UpdateResolutionText()
    {
        if (resolutionValueText != null && resolutions != null && resolutions.Length > currentResIndex)
            resolutionValueText.text = $"{resolutions[currentResIndex].width} x {resolutions[currentResIndex].height}";
    }
    private void UpdateFrameRateText() { if (frameRateValueText != null) frameRateValueText.text = frameRateOptions[currentFrameRateIndex]; }
    private void UpdateVSyncText() { if (vsyncValueText != null) vsyncValueText.text = isVSync ? "AÇIK" : "KAPALI"; }

    // ════════════════════════════════════════════
    // 🔊 SES AYARLARI
    // ════════════════════════════════════════════

    public void SetMasterVolume(float vol)
    {
        if (AudioManager.instance != null) AudioManager.instance.SetMasterVolume(vol);
        else AudioListener.volume = vol;
        if (masterVolumePercentText != null) masterVolumePercentText.text = $"{(int)(vol * 100)}%";
    }

    public void SetMusicVolume(float vol)
    {
        if (AudioManager.instance != null) AudioManager.instance.SetMusicVolume(vol);
        else PlayerPrefs.SetFloat(PREF_MUSIC_VOL, vol);
        if (musicVolumePercentText != null) musicVolumePercentText.text = $"{(int)(vol * 100)}%";
    }

    public void SetSFXVolume(float vol)
    {
        if (AudioManager.instance != null) AudioManager.instance.SetSFXVolume(vol);
        else PlayerPrefs.SetFloat(PREF_SFX_VOL, vol);
        if (sfxVolumePercentText != null) sfxVolumePercentText.text = $"{(int)(vol * 100)}%";
    }

    // ════════════════════════════════════════════
    // 🔄 VARSAYILANA DÖN & YÜKLE
    // ════════════════════════════════════════════

    public void ResetToDefaults()
    {
        PlayerPrefs.DeleteKey(PREF_QUALITY); PlayerPrefs.DeleteKey(PREF_FULLSCREEN);
        PlayerPrefs.DeleteKey(PREF_VSYNC); PlayerPrefs.DeleteKey(PREF_FRAMERATE);
        PlayerPrefs.DeleteKey(PREF_MASTER_VOL); PlayerPrefs.DeleteKey(PREF_MUSIC_VOL);
        PlayerPrefs.DeleteKey(PREF_SFX_VOL); PlayerPrefs.Save();
        LoadAndApplySettings();
        PlayClickSFX();
        Debug.Log("<color=green>[DINK] Tüm ayarlar varsayılana sıfırlandı.</color>");
    }

    public void LoadAndApplySettings()
    {
        currentQualityIndex = PlayerPrefs.GetInt(PREF_QUALITY, 2);
        QualitySettings.SetQualityLevel(currentQualityIndex, true);
        UpdateQualityText();

        isFullscreen = PlayerPrefs.GetInt(PREF_FULLSCREEN, 1) == 1;
        Screen.fullScreen = isFullscreen;
        UpdateDisplayModeText();

        isVSync = PlayerPrefs.GetInt(PREF_VSYNC, 1) == 1;
        QualitySettings.vSyncCount = isVSync ? 1 : 0;
        UpdateVSyncText();

        currentFrameRateIndex = PlayerPrefs.GetInt(PREF_FRAMERATE, 1);
        Application.targetFrameRate = frameRateValues[currentFrameRateIndex];
        UpdateFrameRateText();

        UpdateResolutionText();

        float masterVol = PlayerPrefs.GetFloat(PREF_MASTER_VOL, 0.8f);
        AudioListener.volume = masterVol;
        if (masterVolumeSlider != null) masterVolumeSlider.SetValueWithoutNotify(masterVol);
        if (masterVolumePercentText != null) masterVolumePercentText.text = $"{(int)(masterVol * 100)}%";

        float musicVol = PlayerPrefs.GetFloat(PREF_MUSIC_VOL, 0.5f);
        if (AudioManager.instance != null && AudioManager.instance.musicSource != null)
            AudioManager.instance.musicSource.volume = musicVol;
        if (musicVolumeSlider != null) musicVolumeSlider.SetValueWithoutNotify(musicVol);
        if (musicVolumePercentText != null) musicVolumePercentText.text = $"{(int)(musicVol * 100)}%";

        float sfxVol = PlayerPrefs.GetFloat(PREF_SFX_VOL, 0.8f);
        if (AudioManager.instance != null && AudioManager.instance.sfxSource != null)
            AudioManager.instance.sfxSource.volume = sfxVol;
        if (sfxVolumeSlider != null) sfxVolumeSlider.SetValueWithoutNotify(sfxVol);
        if (sfxVolumePercentText != null) sfxVolumePercentText.text = $"{(int)(sfxVol * 100)}%";

        ClearDescription();
    }

    private void PlayClickSFX()
    {
        if (AudioManager.instance != null) AudioManager.instance.PlayClickSFX();
    }
}
