using UnityEngine;

/// <summary>
/// Ana menü yöneticisi.
/// Gece lobisi menüsünü (LobbyMenu) kurar; Devam Et / Yeni Oyun / Ayarlar / Çıkış.
/// </summary>
public class MainMenu3DController : MonoBehaviour
{
    [Header("Sahne Geçişi")]
    public SceneTransition sceneTransition;

    [Header("Menü Ambiyansı")]
    [Tooltip("Sokak gece ambiyansı (ör. Audio/Prolog/GeceSehir.mp3)")]
    public AudioClip menuAmbience;

    private LobbyMenu kapiMenusu;
    private LobbySettings lobiAyarlari;
    private bool oyunBasliyor;

    // ════════════════════════════════════════════

    private void Start()
    {
        // Fare imlecini menü etkileşimi için serbest ve görünür yap
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (sceneTransition == null)
            sceneTransition = FindFirstObjectByType<SceneTransition>();

        BuildDoorLightMenu();
    }

    // Menü kurulurken kayıt varsa Devam Et en üste eklenir
    private bool devamVar;

    private string[] MenuYazilari()
    {
        var yazilar = new System.Collections.Generic.List<string>();
        if (devamVar) yazilar.Add(DinkLang.T("CONTINUE", "DEVAM ET", "FORTSETZEN", "CONTINUER", "CONTINUAR", "CONTINUAR", "ПРОДОЛЖИТЬ", "继续", "続ける", "이어하기"));
        yazilar.Add(DinkLang.T("NEW GAME", "YENİ OYUN", "NEUES SPIEL", "NOUVELLE PARTIE", "NUEVA PARTIDA", "NOVO JOGO", "НОВАЯ ИГРА", "新游戏", "ニューゲーム", "새 게임"));
        yazilar.Add(DinkLang.T("SETTINGS", "AYARLAR", "EINSTELLUNGEN", "PARAMÈTRES", "AJUSTES", "CONFIGURAÇÕES", "НАСТРОЙКИ", "设置", "設定", "설정"));
        yazilar.Add(DinkLang.T("QUIT", "ÇIKIŞ", "BEENDEN", "QUITTER", "SALIR", "SAIR", "ВЫЙТИ", "退出", "終了", "종료"));
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

        // Oyunu başlatan öğelerde giriş kapısı önce açılır
        var kapiAcar = new System.Collections.Generic.List<bool>();
        if (devamVar) kapiAcar.Add(true);
        kapiAcar.Add(true);
        kapiAcar.Add(false);
        kapiAcar.Add(false);

        kapiMenusu = gameObject.AddComponent<LobbyMenu>();
        kapiMenusu.ambience = menuAmbience;
        kapiMenusu.Build(MenuYazilari(), eylemler.ToArray(), kapiAcar.ToArray());

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

    /// <summary>Ayarları kapının sağ camında açar; sol camdaki menü yazıları solar.</summary>
    private void OpenSettings()
    {
        if (lobiAyarlari == null)
        {
            lobiAyarlari = gameObject.AddComponent<LobbySettings>();
            lobiAyarlari.menu = kapiMenusu;
        }
        lobiAyarlari.Open(null);
    }

    private bool IsAnySettingsPanelOpen()
    {
        return lobiAyarlari != null && (lobiAyarlari.IsOpen || Time.frameCount == lobiAyarlari.LastClosedFrame);
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
            LobbyLogbook.RestoreSignature(DinkSave.ResumeName());
            if (LeaderboardManager.instance != null) LeaderboardManager.instance.StartTimer(DinkSave.ElapsedSeconds);
            PauseService.ResetAll();
            StartCoroutine(SceneDoorTrigger.FadeAndLoad(sahne, 0.6f));
        }
    }

    private void StartGame()
    {
        DinkSave.NewGame();
        if (sceneTransition != null)
        {
            sceneTransition.YeniOyunaGec();
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
}
