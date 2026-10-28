using UnityEngine;

/// <summary>
/// Oyun içi ESC duraklatma menüsü: karanlıkta aralık kapı ışığı menüsü (DoorLightMenu).
/// Ayarlar aynı tarzdaki DoorLightSettings ekranında açılır; geri dönülünce menü yeniden görünür.
/// </summary>
public class Escmenu : MonoBehaviour
{
    public AudioSource escmenusesbileseni;
    public AudioClip escmenuacilmasesi;
    public AudioClip escmenukapanmasesi;

    private bool oyundurdumu;
    private DoorLightMenu kapiMenusu;
    private DoorLightSettings kapiAyarlari;

    private void Start()
    {
        kapiMenusu = gameObject.AddComponent<DoorLightMenu>();
        kapiMenusu.Build(MenuYazilari(), new System.Action[] { Devamet, AyarlariAc, AnaMenuyeDon, OyundanCik });
        // Ayarlar açıkken (içerik gizli) W/S/Enter menüyü tetiklemesin
        kapiMenusu.IsInputBlocked = () => kapiMenusu.Content != null && !kapiMenusu.Content.activeSelf;
    }

    private static string[] MenuYazilari() => new[]
    {
        DinkLang.T("RESUME", "DEVAM ET", "FORTSETZEN", "REPRENDRE", "CONTINUAR", "CONTINUAR", "ПРОДОЛЖИТЬ", "继续", "再開", "계속"),
        DinkLang.T("SETTINGS", "AYARLAR", "EINSTELLUNGEN", "PARAMÈTRES", "AJUSTES", "CONFIGURAÇÕES", "НАСТРОЙКИ", "设置", "設定", "설정"),
        DinkLang.T("MAIN MENU", "ANA MENÜ", "HAUPTMENÜ", "MENU PRINCIPAL", "MENÚ PRINCIPAL", "MENU PRINCIPAL", "ГЛАВНОЕ МЕНЮ", "主菜单", "メインメニュー", "메인 메뉴"),
        DinkLang.T("QUIT", "OYUNDAN ÇIK", "BEENDEN", "QUITTER", "SALIR", "SAIR", "ВЫЙТИ", "退出", "終了", "종료")
    };

    private void OnEnable() { LanguageManager.OnLanguageChanged += DilDegisti; }
    private void OnDisable() { LanguageManager.OnLanguageChanged -= DilDegisti; }
    private void DilDegisti() { if (kapiMenusu != null) kapiMenusu.SetLabels(MenuYazilari()); }

    void Update()
    {
        // Başka bir ekran (mektup, defter) oyunu duraklatmışsa ESC'yi o işler
        if (PauseService.IsPaused && !oyundurdumu) return;
        // Sahne geçişinin karartmasında duraklatma yok (yoksa yeni sahne duraklatılmış gelir)
        if (SceneDoorTrigger.Transitioning) return;
        // Kapı ayarları açıkken ESC'yi o ekran işler (geri döner)
        if (kapiAyarlari != null && kapiAyarlari.IsOpen) return;

        if (DinkInput.EscapeDown)
        {
            DinkInput.ConsumeEscape();
            if (oyundurdumu) Devamet();
            else Durdur();
        }
    }

    public void Devamet()
    {
        if (kapiMenusu != null) kapiMenusu.Close();
        oyundurdumu = false;
        PauseService.Resume(this);
        PlaySound(escmenukapanmasesi);
    }

    public void Durdur()
    {
        if (kapiMenusu != null) kapiMenusu.Open();
        oyundurdumu = true;
        PauseService.Pause(this);
        PlaySound(escmenuacilmasesi);
    }

    /// <summary>Ayarları aynı kapı aralığı tarzında açar; menü içeriği gizlenir, karartı kalır.</summary>
    public void AyarlariAc()
    {
        if (kapiAyarlari == null) kapiAyarlari = gameObject.AddComponent<DoorLightSettings>();
        GameObject icerik = kapiMenusu.Content;
        if (icerik != null) icerik.SetActive(false);
        kapiAyarlari.Open(1110, () => { if (icerik != null) icerik.SetActive(true); });
    }

    public void AnaMenuyeDon()
    {
        PauseService.ResetAll();
        // Kat kaydı varışta alındı; burada yalnız süre güncellenir ve kronometre menüde durur
        DinkSave.SaveElapsedIfRunning();
        LeaderboardManager.instance.StopTimer();
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

    private void PlaySound(AudioClip clip)
    {
        if (escmenusesbileseni != null && clip != null)
            escmenusesbileseni.PlayOneShot(clip, DinkPrefs.SfxSourceScale);
    }
}
