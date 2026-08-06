using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [Header("Paneller")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject languagePanel; // image_f003f7.jpg'deki o meşhur panel
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject creditsPanel;

    private void Start()
    {
        // Başlangıçta her şeyin kapalı olduğundan emin olalım
        if (pausePanel != null) pausePanel.SetActive(false);
        if (languagePanel != null) languagePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);

        ApplyLanguage();
    }

    private void Update()
    {
        if (SceneManager.GetActiveScene().buildIndex != 0)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                TogglePauseMenu();
            }
        }
    }

    // --- PANEL KONTROLLERİ ---

    public void TogglePauseMenu()
    {
        if (pausePanel != null)
        {
            bool isActive = pausePanel.activeSelf;
            pausePanel.SetActive(!isActive);
            Time.timeScale = isActive ? 1f : 0f;
        }
    }

    public void ToggleLanguageMenu()
    {
        if (languagePanel != null)
        {
            languagePanel.SetActive(!languagePanel.activeSelf);
        }
    }

    public void ToggleSettingsMenu()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(!settingsPanel.activeSelf);
        }
    }

    public void ToggleCreditsMenu()
    {
        if(creditsPanel != null)
        {
            creditsPanel.SetActive(!creditsPanel.activeSelf);
        }
    }

    // --- DİL SEÇİMİ (Language Butonları İçin) ---

    public void SetTurkish()
    {
        PlayerPrefs.SetString("Language", "TR");
        ApplyLanguage();
    }

    public void SetEnglish()
    {
        PlayerPrefs.SetString("Language", "EN");
        ApplyLanguage();
    }

    private void ApplyLanguage()
    {
        LocalizedText[] allTexts = Object.FindObjectsByType<LocalizedText>(FindObjectsSortMode.None);
        foreach (LocalizedText lt in allTexts)
        {
            lt.UpdateText();
        }
    }

    // --- DİĞER BUTONLAR ---

    public void mainmenupress()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }

    public void OnRestartPress()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnQuitPress()
    {
        Application.Quit();
    }
}