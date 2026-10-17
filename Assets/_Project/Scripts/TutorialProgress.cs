using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Tutorial yalnızca ilk oyunda oynanır.
/// Oyuncu tutorial olmayan ilk odaya bir kez ulaştığında bu bilgisayara kaydedilir;
/// sonraki her yeni oyun (Oyna, Yeniden Oyna, kötü son döngüsü) doğrudan o odadan başlar.
/// Hangi odaların tutorial olduğu RoomGraph'taki "isTutorial" işaretinden okunur.
/// </summary>
public static class TutorialProgress
{
    private const string TutorialDoneKey = "Dink_TutorialDone";

    public static bool IsDone => PlayerPrefs.GetInt(TutorialDoneKey, 0) == 1;

    /// <summary>
    /// RoomGraph'ta tutorial işaretli olmayan ilk oda. Oda ağı yoksa 1.
    /// </summary>
    public static int FirstPlayableRoom
    {
        get
        {
            RoomGraphData data = RoomGraphManager.instance != null ? RoomGraphManager.instance.GetRoomGraphData() : null;
            if (data == null || data.roomNodes == null) return DinkScenes.FirstRoom;

            int first = int.MaxValue;
            foreach (var node in data.roomNodes)
            {
                if (node != null && !node.isTutorial && node.roomIndex < first)
                {
                    first = node.roomIndex;
                }
            }
            return first == int.MaxValue ? DinkScenes.FirstRoom : first;
        }
    }

    /// <summary>
    /// Yeni bir oyunun başlayacağı oda: tutorial bitmişse ilk gerçek oda, değilse Oda 1.
    /// </summary>
    public static int NewGameRoom => IsDone ? FirstPlayableRoom : DinkScenes.FirstRoom;

    /// <summary>
    /// Tutorial'ı tekrar oynatmak için kaydı siler (ör. ayarlardan "Tutorial'ı tekrar oyna").
    /// </summary>
    public static void Reset()
    {
        PlayerPrefs.DeleteKey(TutorialDoneKey);
        PlayerPrefs.Save();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (IsDone) return;

        int roomIndex = DinkScenes.GetRoomIndex(scene);
        if (roomIndex >= FirstPlayableRoom && roomIndex > 0)
        {
            PlayerPrefs.SetInt(TutorialDoneKey, 1);
            PlayerPrefs.Save();
            Debug.Log($"[TutorialProgress] Tutorial tamamlandı (Oda {roomIndex}). Yeni oyunlar artık Oda {FirstPlayableRoom}'den başlayacak.");
        }
    }
}
