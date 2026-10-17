using UnityEngine;

/// <summary>
/// Oyun içi ESC duraklatma menüsü yöneticisi.
/// Ayarlar butonuna basıldığında AAA Ayarlar panelini açar ve geri dönüldüğünde tekrar duraklatma menüsünü getirir.
/// </summary>
public class Escmenu : MonoBehaviour
{
    public GameObject escmenu;
    public GameObject settingsMenuPanel;

    private bool oyundurdumu = false;
    public AudioSource escmenusesbileseni;
    public AudioClip escmenuacilmasesi;
    public AudioClip escmenukapanmasesi;

    [Header("Kapı Aralığı Menüsü")]
    [Tooltip("İşaretliyse eski buton panelinin yerine karanlıkta aralık kapı ışığı menüsü açılır")]
    [SerializeField] private bool kapiAraligiMenusu = true;
    private DoorLightMenu kapiMenusu;

    private void Start()
    {
        if (kapiAraligiMenusu)
        {
            kapiMenusu = gameObject.AddComponent<DoorLightMenu>();
            bool isTR = LanguageManager.instance == null || LanguageManager.instance.CurrentLanguage == LanguageManager.Language.TR;
            string[] labels = isTR
                ? new[] { "DEVAM ET", "AYARLAR", "ANA MENÜ", "OYUNDAN ÇIK" }
                : new[] { "RESUME", "SETTINGS", "MAIN MENU", "QUIT" };
            kapiMenusu.Build(labels, new System.Action[] { Devamet, AyarlariAc, AnaMenuyeDon, OyundanCik });
        }
    }

    /// <summary>Aktif duraklatma menüsünün kökü (kapı menüsü ya da eski buton paneli).</summary>
    private GameObject AktifMenu => kapiMenusu != null ? kapiMenusu.Root : escmenu;

    void Update()
    {
        // Eğer ayarlar paneli ekranda açıksa ESC tuşu basımını SettingsManager yönetir
        if (SettingsManager.instance != null && SettingsManager.instance.gameObject.activeSelf)
        {
            return;
        }

        // Mektup açıkken ESC mektubu kapatır; intro oynarken ESC introyu atlar
        if (MektupNotHintUI.instance != null && MektupNotHintUI.instance.IsPanelOpen) return;
        if (IntroCinematic.IsPlaying) return;

        if (DinkInput.EscapeDown)
        {
            DinkInput.ConsumeEscape();
            if (oyundurdumu)
            {
                Devamet();
            }
            else
            {
                Durdur();
            }
        }
    }

    public void Devamet()
    {
        if (escmenu != null) escmenu.SetActive(false);
        if (kapiMenusu != null) kapiMenusu.Close();
        if (RoomCountdown.instance != null) RoomCountdown.instance.SetHidden(false);
        Time.timeScale = 1f;
        oyundurdumu = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (escmenusesbileseni != null && escmenukapanmasesi != null)
        {
            float sfxVol = AudioManager.instance != null ? AudioManager.instance.GetSFXVolume() : PlayerPrefs.GetFloat("Dink_SFXVolume", 0.8f);
            escmenusesbileseni.PlayOneShot(escmenukapanmasesi, sfxVol);
        }

        // SCRİPT AÇMA KISMI
        CloseOpenScript(true);
    }

    public void Durdur()
    {
        if (kapiMenusu != null)
        {
            if (escmenu != null) escmenu.SetActive(false);
            kapiMenusu.Open();
        }
        else if (escmenu != null)
        {
            escmenu.SetActive(true);
        }
        if (RoomCountdown.instance != null) RoomCountdown.instance.SetHidden(true);
        Time.timeScale = 0f;
        oyundurdumu = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (escmenusesbileseni != null && escmenuacilmasesi != null)
        {
            float sfxVol = AudioManager.instance != null ? AudioManager.instance.GetSFXVolume() : PlayerPrefs.GetFloat("Dink_SFXVolume", 0.8f);
            escmenusesbileseni.PlayOneShot(escmenuacilmasesi, sfxVol);
        }

        // SCRİPT KAPATMA KISMI
        CloseOpenScript(false);
    }

    /// <summary>
    /// Duraklatma menüsündeki AYARLAR butonuna bağlanan metot.
    /// AAA Ayarlar menüsünü açar.
    /// </summary>
    public void AyarlariAc()
    {
        SettingsManager sm = SettingsManager.instance;
        if (sm == null)
        {
            sm = FindAnyObjectByType<SettingsManager>(FindObjectsInactive.Include);
        }

        if (sm != null)
        {
            sm.OpenSettingsFromPauseMenu(AktifMenu);
        }
        else if (settingsMenuPanel != null)
        {
            settingsMenuPanel.SetActive(true);
            if (AktifMenu != null) AktifMenu.SetActive(false);
        }
    }

    /// <summary>
    /// Ana menüye döner. (Eski paneldeki buton, silinmiş ButonEtkliesim scriptine bağlı olduğu için çalışmıyordu.)
    /// </summary>
    public void AnaMenuyeDon()
    {
        Time.timeScale = 1f;
        DinkScenes.LoadMainMenu();
    }

    public void OyundanCik()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void CloseOpenScript(bool enable)
    {
        PlayerControls.SetEnabled(enable);
    }
}
