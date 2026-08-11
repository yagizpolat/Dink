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

    // SCRİPT BAĞLANTILARI
    FenerKontrol fener;
    Kamera kamerascript;
    InventoryManager inventory;
    temaskontrol mektup_pil;

    private void Start()
    {
        ScriptConnect();
    }

    void Update()
    {
        // Eğer ayarlar paneli ekranda açıksa ESC tuşu basımını SettingsManager yönetir
        if (SettingsManager.instance != null && SettingsManager.instance.gameObject.activeSelf)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
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
        if (escmenu != null) escmenu.SetActive(true);
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
            sm.OpenSettingsFromPauseMenu(escmenu);
        }
        else if (settingsMenuPanel != null)
        {
            settingsMenuPanel.SetActive(true);
            if (escmenu != null) escmenu.SetActive(false);
        }
    }

    void ScriptConnect()
    {
        fener = FindAnyObjectByType<FenerKontrol>();
        kamerascript = FindAnyObjectByType<Kamera>();
        inventory = FindAnyObjectByType<InventoryManager>();
        mektup_pil = FindAnyObjectByType<temaskontrol>();
    }

    void CloseOpenScript(bool enable)
    {
        if (fener != null) fener.enabled = enable;
        if (kamerascript != null) kamerascript.enabled = enable;
        if (inventory != null) inventory.enabled = enable;
        if (mektup_pil != null) mektup_pil.enabled = enable;
    }
}
