using UnityEngine;

/// <summary>
/// Ana menü yöneticisi.
/// Karanlıkta aralık kapı ışığı menüsünü (DoorLightMenu) kurar; Yeni Oyun / Ayarlar / Çıkış.
/// (Eski 3D kapılı menü kaldırıldı.)
/// </summary>
public class MainMenu3DController : MonoBehaviour
{
    [Header("Ayar Panelleri (Opsiyonel)")]
    public GameObject graphicsPanel;
    public GameObject audioPanel;
    public GameObject languagePanel;

    [Header("Sahne Geçişi")]
    public SceneTransition sceneTransition;

    private DoorLightMenu kapiMenusu;
    private bool oyunBasliyor;

    // ════════════════════════════════════════════

    private void Start()
    {
        // Fare imlecini menü etkileşimi için serbest ve görünür yap
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (sceneTransition == null)
            sceneTransition = FindFirstObjectByType<SceneTransition>();

        CloseAllPanels();
        BuildDoorLightMenu();
    }

    private void BuildDoorLightMenu()
    {
        bool isTR = LanguageManager.instance == null || LanguageManager.instance.CurrentLanguage == LanguageManager.Language.TR;
        string[] labels = isTR
            ? new[] { "YENİ OYUN", "AYARLAR", "ÇIKIŞ" }
            : new[] { "NEW GAME", "SETTINGS", "QUIT" };

        kapiMenusu = gameObject.AddComponent<DoorLightMenu>();
        kapiMenusu.Build(labels, new System.Action[]
        {
            () => { oyunBasliyor = true; StartGame(); },
            () => OpenPanel(graphicsPanel),
            QuitGame
        }, new DoorLightMenu.Layout
        {
            title = "DINK",
            darkness = 1f,          // 3D kapı sahnesi arkada hiç görünmesin
            stripHeight = 980f,
            stripX = -380f,
            itemsCenterY = -150f,
            sortingOrder = 0,       // ayarlar (10) ve isim girişi (15) panelleri bunun üstünde açılır
            openDuration = 1.1f
        });

        // Oyun başlarken ya da üstte ayarlar paneli açıkken menü tuşları çalışmasın
        kapiMenusu.IsInputBlocked = () => oyunBasliyor || IsAnySettingsPanelOpen();
        kapiMenusu.Open();
    }

    private bool IsAnySettingsPanelOpen()
    {
        return (graphicsPanel != null && graphicsPanel.activeInHierarchy) ||
               (audioPanel != null && audioPanel.activeInHierarchy) ||
               (languagePanel != null && languagePanel.activeInHierarchy) ||
               (SettingsManager.instance != null && SettingsManager.instance.gameObject.activeInHierarchy);
    }

    /// <summary>
    /// Ayarlar panelinden geri dönülünce SettingsManager çağırır; paneller kapanır, kapı menüsü zaten arkada açık.
    /// </summary>
    public void ShowMainDoors()
    {
        CloseAllPanels();
    }

    // ─── Oyun Başlat / Çıkış ───

    private void StartGame()
    {
        if (sceneTransition != null)
        {
            sceneTransition.ButonlaSahneyeGit(TutorialProgress.NewGameRoom);
        }
        else
        {
            DinkScenes.LoadNewGame();
        }
    }

    private void QuitGame()
    {
        Debug.Log("[DINK] Oyundan çıkılıyor...");
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    // ─── Panel Yönetimi ───

    private void OpenPanel(GameObject panel)
    {
        CloseAllPanels();

        if (panel == null)
        {
            if (SettingsManager.instance != null)
            {
                panel = SettingsManager.instance.gameObject;
            }
            else
            {
                SettingsManager sm = FindAnyObjectByType<SettingsManager>(FindObjectsInactive.Include);
                if (sm != null) panel = sm.gameObject;
            }
        }

        if (panel != null)
        {
            panel.SetActive(true);
            if (SettingsManager.instance != null)
            {
                SettingsManager.instance.LoadAndApplySettings();
            }
        }
    }

    private void CloseAllPanels()
    {
        if (graphicsPanel != null) graphicsPanel.SetActive(false);
        if (audioPanel != null) audioPanel.SetActive(false);
        if (languagePanel != null) languagePanel.SetActive(false);

        if (SettingsManager.instance != null)
        {
            SettingsManager.instance.gameObject.SetActive(false);
        }
        else
        {
            SettingsManager sm = FindAnyObjectByType<SettingsManager>(FindObjectsInactive.Include);
            if (sm != null) sm.gameObject.SetActive(false);
        }
    }
}
