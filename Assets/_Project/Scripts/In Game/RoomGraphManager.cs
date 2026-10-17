using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Doğrusal Olmayan Oda Ağı Yöneticisi (Room Graph Manager).
/// Odalar arası 1 -> 8 -> 17 -> 3 vb. geçişleri, Kilit Oda (Oda 7 & 11) flaş efektlerini yönetir.
/// </summary>
public class RoomGraphManager : MonoBehaviour
{
    public static RoomGraphManager instance;

    [Header("ScriptableObject Oda Ağı Verisi")]
    [SerializeField] private RoomGraphData roomGraphData;

    [Header("Mevcut Durum")]
    public int currentRoomIndex = 1;
    private bool isTransitioning = false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void OtomatikOlustur()
    {
        if (instance != null) return;

        GameObject go = new GameObject("[RoomGraphManager]");
        go.AddComponent<RoomGraphManager>();
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Oyuncu kapı seçtiğinde sıradaki hedef odaya geçiş yapar.
    /// </summary>
    public void NavigateToNextRoom(bool isLeftDoor)
    {
        if (isTransitioning) return;

        RoomGraphData.RoomNode currentNode = GetCurrentNode();
        int targetRoomIndex = 1;

        if (currentNode != null)
        {
            targetRoomIndex = isLeftDoor ? currentNode.leftTargetRoomIndex : currentNode.rightTargetRoomIndex;

            // Kilit Flaş Geçiş Odası mı? (Oda 7 & 11)
            if (currentNode.isMilestoneFlashRoom)
            {
                StartCoroutine(ExecuteMilestoneFlashTransition(targetRoomIndex));
                return;
            }
        }

        currentRoomIndex = targetRoomIndex;
        LoadRoomScene(currentRoomIndex);
    }

    /// <summary>
    /// Sahne adından oda numarasını çıkarır. "Game" = Ana Menü (0), "Game N" = Oda N (Build Index N).
    /// Oda sahnesi değilse -1 döner.
    /// </summary>
    public static int GetRoomIndexFromScene(Scene scene)
    {
        string sceneName = scene.name;
        if (sceneName == "Game") return 0;
        if (sceneName.StartsWith("Game ") && int.TryParse(sceneName.Substring(5).Trim(), out int num))
        {
            return num;
        }
        return -1;
    }

    public RoomGraphData GetRoomGraphData()
    {
        if (roomGraphData != null) return roomGraphData;

        RoomGraphData[] found = Resources.FindObjectsOfTypeAll<RoomGraphData>();
        if (found != null && found.Length > 0)
        {
            roomGraphData = found[0];
            return roomGraphData;
        }

#if UNITY_EDITOR
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:RoomGraphData");
        if (guids.Length > 0)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
            roomGraphData = UnityEditor.AssetDatabase.LoadAssetAtPath<RoomGraphData>(path);
            if (roomGraphData != null)
            {
                return roomGraphData;
            }
        }
#endif

        return null;
    }

    public RoomGraphData.RoomNode GetCurrentNode()
    {
        RoomGraphData data = GetRoomGraphData();
        if (data != null)
        {
            return data.GetRoomNode(currentRoomIndex);
        }
        return null;
    }

    public bool IsCurrentRoomTutorial()
    {
        RoomGraphData.RoomNode node = GetCurrentNode();
        return node != null && node.isTutorial;
    }

    private IEnumerator ExecuteMilestoneFlashTransition(int targetRoomIndex)
    {
        isTransitioning = true;
        Debug.Log($"[RoomGraphManager] Kilit Flaş Geçiş Odası Tetiklendi! (Oda {currentRoomIndex}) -> Hakan'ın Dram Odasına Aktarılıyor...");

        // Flaş sesi veya ekran beyazlaşması
        yield return new WaitForSecondsRealtime(0.5f);

        currentRoomIndex = targetRoomIndex;
        LoadRoomScene(currentRoomIndex);
        isTransitioning = false;
    }

    private void LoadRoomScene(int roomIndex)
    {
        // Speedrunner veya Final kontrolü
        if (roomIndex > 15 && EndingManager.instance != null)
        {
            EndingManager.instance.CheckEndingAndTransition();
            return;
        }

        // Oda N = "Game N" sahnesi (Build Index 0 = "Game" ana menü)
        string sceneName = $"Game {roomIndex}";
        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.Log($"[RoomGraphManager] Oda {roomIndex} Sahnesine Geçiliyor... (Sahne Adı: {sceneName})");
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError($"[RoomGraphManager] '{sceneName}' sahnesi Build Settings'te yok! Ana menüye dönülüyor.");
            SceneManager.LoadScene(0);
        }
    }
}
