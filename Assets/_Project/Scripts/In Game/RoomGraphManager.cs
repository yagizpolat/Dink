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

    private const string RoomGraphResourcePath = "Dink/RoomGraph";

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

    public RoomGraphData GetRoomGraphData()
    {
        if (roomGraphData != null) return roomGraphData;

        // Resources klasöründe olduğu için build'e de girer (AssetDatabase sadece editörde çalışır)
        roomGraphData = Resources.Load<RoomGraphData>(RoomGraphResourcePath);
        if (roomGraphData == null)
        {
            Debug.LogError($"[RoomGraphManager] Oda ağı bulunamadı: Resources/{RoomGraphResourcePath}.asset");
        }
        return roomGraphData;
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
        if (roomIndex > DinkScenes.LastRoom && EndingManager.instance != null)
        {
            EndingManager.instance.CheckEndingAndTransition();
            return;
        }

        string sceneName = DinkScenes.RoomSceneName(roomIndex);
        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.Log($"[RoomGraphManager] Oda {roomIndex} Sahnesine Geçiliyor... (Sahne Adı: {sceneName})");
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError($"[RoomGraphManager] '{sceneName}' sahnesi Build Settings'te yok! Ana menüye dönülüyor.");
            DinkScenes.LoadMainMenu();
        }
    }
}
