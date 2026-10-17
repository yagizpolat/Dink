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
    private DoorLightSettings kapiAyarlari;

    private void Start()
    {
        if (kapiAraligiMenusu)
        {
            kapiMenusu = gameObject.AddComponent<DoorLightMenu>();
            kapiMenusu.Build(MenuYazilari(), new System.Action[] { Devamet, AyarlariAc, AnaMenuyeDon, OyundanCik });
            // Ayarlar açıkken (içerik gizli) W/S/Enter menüyü tetiklemesin
            kapiMenusu.IsInputBlocked = () => kapiMenusu.Content != null && !kapiMenusu.Content.activeSelf;
        }
    }

    private static string[] MenuYazilari() => new[]
    {
        DinkLang.T("RESUME", "DEVAM ET", "FORTSETZEN", "REPRENDRE", "CONTINUAR", "CONTINUAR", "ПРОДОЛЖИТЬ"),
        DinkLang.T("SETTINGS", "AYARLAR", "EINSTELLUNGEN", "PARAMÈTRES", "AJUSTES", "CONFIGURAÇÕES", "НАСТРОЙКИ"),
        DinkLang.T("MAIN MENU", "ANA MENÜ", "HAUPTMENÜ", "MENU PRINCIPAL", "MENÚ PRINCIPAL", "MENU PRINCIPAL", "ГЛАВНОЕ МЕНЮ"),
        DinkLang.T("QUIT", "OYUNDAN ÇIK", "BEENDEN", "QUITTER", "SALIR", "SAIR", "ВЫЙТИ")
    };

    private void OnEnable() { LanguageManager.OnLanguageChanged += DilDegisti; }
    private void OnDisable() { LanguageManager.OnLanguageChanged -= DilDegisti; }
    private void DilDegisti() { if (kapiMenusu != null) kapiMenusu.SetLabels(MenuYazilari()); }

    /// <summary>Aktif duraklatma menüsünün kökü (kapı menüsü ya da eski buton paneli).</summary>
    private GameObject AktifMenu => kapiMenusu != null ? kapiMenusu.Root : escmenu;

    /// <summary>Ayarlar açılırken gizlenecek kısım: kapı menüsünde yalnızca içerik, karartı kalır.</summary>
    private GameObject AyarlardaGizlenecek => kapiMenusu != null ? kapiMenusu.Content : escmenu;

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
        // Kapı ayarları açıkken ESC'yi o ekran işler (geri döner)
        if (kapiAyarlari != null && kapiAyarlari.IsOpen) return;

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
        // Kapı menüsü kullanılıyorsa ayarlar da aynı tarzda açılır (eski panel kullanılmaz)
        if (kapiMenusu != null)
        {
            if (kapiAyarlari == null) kapiAyarlari = gameObject.AddComponent<DoorLightSettings>();
            GameObject icerik = kapiMenusu.Content;
            if (icerik != null) icerik.SetActive(false);
            kapiAyarlari.Open(160, () => { if (icerik != null) icerik.SetActive(true); });
            return;
        }

        SettingsManager sm = SettingsManager.instance;
        if (sm == null)
        {
            sm = FindAnyObjectByType<SettingsManager>(FindObjectsInactive.Include);
        }

        if (sm != null)
        {
            sm.OpenSettingsFromPauseMenu(AyarlardaGizlenecek);
        }
        else if (settingsMenuPanel != null)
        {
            settingsMenuPanel.SetActive(true);
            if (AyarlardaGizlenecek != null) AyarlardaGizlenecek.SetActive(false);
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
