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
    private DoorLightSettings kapiAyarlari;
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

    // Menü kurulurken kayıt varsa Devam Et en üste eklenir
    private bool devamVar;

    private string[] MenuYazilari()
    {
        var yazilar = new System.Collections.Generic.List<string>();
        if (devamVar) yazilar.Add(DinkLang.T("CONTINUE", "DEVAM ET", "FORTSETZEN", "CONTINUER", "CONTINUAR", "CONTINUAR", "ПРОДОЛЖИТЬ"));
        yazilar.Add(DinkLang.T("NEW GAME", "YENİ OYUN", "NEUES SPIEL", "NOUVELLE PARTIE", "NUEVA PARTIDA", "NOVO JOGO", "НОВАЯ ИГРА"));
        yazilar.Add(DinkLang.T("SETTINGS", "AYARLAR", "EINSTELLUNGEN", "PARAMÈTRES", "AJUSTES", "CONFIGURAÇÕES", "НАСТРОЙКИ"));
        yazilar.Add(DinkLang.T("QUIT", "ÇIKIŞ", "BEENDEN", "QUITTER", "SALIR", "SAIR", "ВЫЙТИ"));
        return yazilar.ToArray();
    }

    private void OnEnable() { LanguageManager.OnLanguageChanged += DilDegisti; }
    private void OnDisable() { LanguageManager.OnLanguageChanged -= DilDegisti; }
    private void DilDegisti() { if (kapiMenusu != null) kapiMenusu.SetLabels(MenuYazilari()); }

    private void BuildDoorLightMenu()
    {
        devamVar = DinkSave.HasSave;
        var eylemler = new System.Collections.Generic.List<System.Action>();
        if (devamVar) eylemler.Add(() => { oyunBasliyor = true; ContinueGame(); });
        eylemler.Add(() => { oyunBasliyor = true; StartGame(); });
        eylemler.Add(OpenSettings);
        eylemler.Add(QuitGame);

        kapiMenusu = gameObject.AddComponent<DoorLightMenu>();
        kapiMenusu.Build(MenuYazilari(), eylemler.ToArray(), new DoorLightMenu.Layout
        {
            title = "DINK",
            darkness = 1f,          // 3D kapı sahnesi arkada hiç görünmesin
            stripHeight = 980f,
            stripX = -380f,
            itemsCenterY = -150f,
            sortingOrder = 5,       // intro Canvas (1) tıklamaları yutmasın; ayarlar (10) ve isim girişi (15) yine üstte açılır
            openDuration = 1.1f
        });

        // Oyun başlarken ya da üstte ayarlar paneli açıkken menü tuşları çalışmasın
        kapiMenusu.IsInputBlocked = () => oyunBasliyor || IsAnySettingsPanelOpen();
        kapiMenusu.Open();
    }

    /// <summary>
    /// Oyun başlarken menüyü gizler. Menü tuvali (5) intro'nun siyah perdesinden (1) üstte çizildiği için
    /// gizlenmezse hikaye yazısı menünün üstüne biner.
    /// </summary>
    public void HideMenu()
    {
        if (kapiMenusu != null && kapiMenusu.Content != null) kapiMenusu.Content.SetActive(false);
    }

    /// <summary>Kapı aralığı tarzı ayarlar ekranını menünün üstünde açar.</summary>
    private void OpenSettings()
    {
        if (kapiAyarlari == null) kapiAyarlari = gameObject.AddComponent<DoorLightSettings>();
        GameObject icerik = kapiMenusu != null ? kapiMenusu.Content : null;
        if (icerik != null) icerik.SetActive(false);
        kapiAyarlari.Open(10, () => { if (icerik != null) icerik.SetActive(true); });
    }

    private bool IsAnySettingsPanelOpen()
    {
        return (kapiAyarlari != null && kapiAyarlari.IsOpen) ||
               (graphicsPanel != null && graphicsPanel.activeInHierarchy) ||
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

    /// <summary>Kayıttaki kat sahnesini açar; sahne yüklenemiyorsa Yeni Oyun akışına düşer.</summary>
    private void ContinueGame()
    {
        string sahne = DinkSave.LastFloorScene;
        if (!DinkSave.IsFloorScene(sahne) || !Application.CanStreamedLevelBeLoaded(sahne))
        {
            StartGame();
            return;
        }

        if (sceneTransition != null)
        {
            sceneTransition.KayittanDevamEt(sahne);
        }
        else
        {
            LobbyLogbook.RestoreSignature(DinkSave.PlayerName);
            if (LeaderboardManager.instance != null) LeaderboardManager.instance.StartTimer(DinkSave.ElapsedSeconds);
            UnityEngine.SceneManagement.SceneManager.LoadScene(sahne);
        }
    }

    private void StartGame()
    {
        DinkSave.NewGame();
        if (sceneTransition != null)
        {
            sceneTransition.ButonlaSahneyeGit(0);
        }
        else
        {
            DinkScenes.LoadPrologue();
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
