using UnityEngine.SceneManagement;

/// <summary>
/// Dink sahne isimlerinin tek kaynağı.
/// "Game" = Ana Menü, "Game N" = Oda N, "Zindan" = minigame odası.
/// Sahnelere numarayla (LoadScene(0) / LoadScene(1)) gitmek yerine buradaki metotları kullan;
/// Build Settings sırası değişse bile doğru sahne açılır.
/// </summary>
public static class DinkScenes
{
    public const string MainMenu = "Game";
    public const string Zindan = "Zindan";
    public const int FirstRoom = 1;
    public const int LastRoom = 15;

    public static string RoomSceneName(int roomIndex) => $"Game {roomIndex}";

    /// <summary>
    /// Sahne adından oda numarasını çıkarır. Ana menü 0, oda olmayan sahneler -1 döner.
    /// </summary>
    public static int GetRoomIndex(Scene scene)
    {
        string sceneName = scene.name;
        if (sceneName == MainMenu) return 0;
        if (sceneName.StartsWith("Game ") && int.TryParse(sceneName.Substring(5).Trim(), out int num))
        {
            return num;
        }
        return -1;
    }

    public static bool IsMainMenu(Scene scene) => scene.name == MainMenu;

    /// <summary>Karanfil Apt. bölümü: prolog, giriş katı ve "KatN" sahneleri. Bu sahnelerde müzik çalmaz.</summary>
    public static bool IsApartment(Scene scene) =>
        scene.name == Prologue || scene.name == "GirisKati" || (scene.name.StartsWith("Kat") && scene.name.Length > 3 && char.IsDigit(scene.name[3]));

    public static void LoadMainMenu() => SceneManager.LoadScene(MainMenu);

    public const string Prologue = "Prolog";

    /// <summary>Apartman bölümünün başı: sokaktaki Karanfil Apt. önü.</summary>
    public static void LoadPrologue() => SceneManager.LoadScene(Prologue);

    public static void LoadRoom(int roomIndex) => SceneManager.LoadScene(RoomSceneName(roomIndex));

    /// <summary>
    /// Yeni oyun başlatır: ilk oyunda Oda 1 (tutorial), sonrakilerde tutorial olmayan ilk oda.
    /// </summary>
    public static void LoadNewGame() => LoadRoom(TutorialProgress.NewGameRoom);
}
