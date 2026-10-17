using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Kapı Seçim & Yön Belirleyici (DoorChoice).
/// Tutorial odaları (Oda 1-3) için her iki kapıyı da doğru kabul eder.
/// Sonraki odalarda DynamicHintSystem tarafından belirlenen doğru kapı yönünü canlı olarak işler.
/// </summary>
public class DoorChoice : MonoBehaviour
{
    [Header("Tutorial / Öğretici Oda Ayarı")]
    [Tooltip("İşaretli ise Sol ve Sağ kapıların İKİSİ DE DOĞRU kabul edilir, oyuncu yanılmaz.")]
    [SerializeField] private bool isTutorialMap = false;

    [Header("Kapı Referansları")]
    [SerializeField] private Door leftDoor;
    [SerializeField] private Door rightDoor;
    [SerializeField] private DynamicHintSystem dynamicHintSystem;

    private void Start()
    {
        AutoFindReferences();
        ConfigureDoors();
    }

    private void AutoFindReferences()
    {
        // 1. Kapı referansları Inspector'da atanmamışsa sahnedeki Door bileşenlerinden bul
        if (leftDoor == null || rightDoor == null)
        {
            Door[] doors = FindObjectsByType<Door>(FindObjectsSortMode.None);
            foreach (var d in doors)
            {
                string doorName = d.gameObject.name.ToLower();
                bool looksLeft = doorName.Contains("left") || doorName.Contains("sol") || doorName.Contains("l_");
                bool looksRight = doorName.Contains("right") || doorName.Contains("sag") || doorName.Contains("r_");

                // Parentheses matter: without them "a && b || c" also overwrote an already assigned door
                if (leftDoor == null && looksLeft && d != rightDoor)
                {
                    leftDoor = d;
                }
                else if (rightDoor == null && looksRight && d != leftDoor)
                {
                    rightDoor = d;
                }
            }

            // Isimle ayırt edilemediyse kalan kapılardan ata (aynı kapı iki tarafa atanmasın)
            foreach (var d in doors)
            {
                if (d == leftDoor || d == rightDoor) continue;
                if (leftDoor == null) leftDoor = d;
                else if (rightDoor == null) rightDoor = d;
            }
        }

        // 2. DynamicHintSystem atanmamışsa sahneden veya Singleton'dan otomatik bul
        if (dynamicHintSystem == null)
        {
            dynamicHintSystem = DynamicHintSystem.instance ?? FindAnyObjectByType<DynamicHintSystem>();
        }
    }

    public void ConfigureDoors()
    {
        // Tutorial Tespiti ve Oda Index Senkronizasyonu
        Scene activeScene = SceneManager.GetActiveScene();
        string sceneName = activeScene.name;
        int sceneIndex = activeScene.buildIndex;

        int derivedRoomIndex = 1;
        if (sceneName.StartsWith("Game "))
        {
            string numStr = sceneName.Replace("Game ", "").Trim();
            if (int.TryParse(numStr, out int num))
            {
                derivedRoomIndex = num + 1;
            }
        }
        else if (sceneName.Equals("Game"))
        {
            derivedRoomIndex = 1;
        }
        else
        {
            derivedRoomIndex = sceneIndex + 1;
        }

        if (RoomGraphManager.instance == null)
        {
            // Instance yoksa bile tetikle
            RoomGraphManager mgr = FindAnyObjectByType<RoomGraphManager>();
            if (mgr == null)
            {
                GameObject go = new GameObject("[RoomGraphManager]");
                mgr = go.AddComponent<RoomGraphManager>();
            }
        }

        if (RoomGraphManager.instance != null)
        {
            RoomGraphManager.instance.currentRoomIndex = derivedRoomIndex;
        }

        // ItsGame (RoomGraphData) dosyasından ilgili odanın node verisini oku
        RoomGraphData graphData = RoomGraphManager.instance != null ? RoomGraphManager.instance.GetRoomGraphData() : null;
        bool isGraphTutorial = false;
        if (graphData != null)
        {
            RoomGraphData.RoomNode node = graphData.GetRoomNode(derivedRoomIndex);
            if (node != null)
            {
                isGraphTutorial = node.isTutorial;
            }
        }

        bool isTutorial = isTutorialMap || isGraphTutorial;

        int elementIndex = derivedRoomIndex - 1;
        Debug.Log($"[DoorChoice] Sahne: '{sceneName}' -> ItsGame Inspector: Element {elementIndex} (Room Index {derivedRoomIndex}), IsTutorial: {isGraphTutorial}, Sahne Inspector: {isTutorialMap} -> SONUÇ: {(isTutorial ? "TUTORIAL (İki Kapı da Doğru)" : "NORMAL OYUN (Yanlış Kapı Jumpscare)")}");

        if (isTutorial)
        {
            if (leftDoor != null) leftDoor.SetCorrectDoor(true);
            if (rightDoor != null) rightDoor.SetCorrectDoor(true);
            return;
        }

        // Dinamik İpucu Sisteminden Atanan Doğru Yönü Al
        bool isLeftCorrect = true;
        if (dynamicHintSystem != null)
        {
            isLeftCorrect = dynamicHintSystem.isLeftDoorCorrect;
        }
        else
        {
            isLeftCorrect = Random.value > 0.5f;
        }

        if (leftDoor != null) leftDoor.SetCorrectDoor(isLeftCorrect);
        if (rightDoor != null) rightDoor.SetCorrectDoor(!isLeftCorrect);

        Debug.Log($"[DoorChoice] Oda Kapıları Ayarlandı -> Sol Kapı: {(isLeftCorrect ? "DOĞRU" : "YANLIŞ")}, Sağ Kapı: {(!isLeftCorrect ? "DOĞRU" : "YANLIŞ")}");
    }
}

