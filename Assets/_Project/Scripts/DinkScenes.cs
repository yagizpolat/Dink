using UnityEngine.SceneManagement;

/// <summary>
/// Dink sahne isimlerinin tek kaynağı.
/// "Game" = Ana Menü; apartman bölümü: "Prolog" -> "GirisKati" -> "Kat1".."Kat5".
/// Sahnelere numarayla (LoadScene(0) / LoadScene(1)) gitmek yerine buradaki metotları kullan;
/// Build Settings sırası değişse bile doğru sahne açılır.
/// </summary>
public static class DinkScenes
{
    public const string MainMenu = "Game";
    public const string Prologue = "Prolog";

    public static bool IsMainMenu(Scene scene) => scene.name == MainMenu;

    /// <summary>Karanfil Apt. bölümü: prolog, giriş katı ve "KatN" sahneleri. Bu sahnelerde müzik çalmaz.</summary>
    public static bool IsApartment(Scene scene) =>
        scene.name == Prologue || scene.name == "GirisKati" || (scene.name.StartsWith("Kat") && scene.name.Length > 3 && char.IsDigit(scene.name[3]));

    public static void LoadMainMenu() => SceneManager.LoadScene(MainMenu);

    /// <summary>Yeni oyunun başı: sokaktaki Karanfil Apt. önü.</summary>
    public static void LoadPrologue() => SceneManager.LoadScene(Prologue);
}
