using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Görünmez bir engele (ör. giriş katındaki MerdivenEngeli) dayanan oyuncuya kısa bir ipucu gösterir.
/// Sahnede elle eklenebilir; "MerdivenEngeli" adlı nesnelere sahne yüklenince kendiliğinden eklenir.
/// </summary>
[RequireComponent(typeof(Collider))]
public class BlockedPassageHint : MonoBehaviour
{
    [SerializeField] private float touchDistance = 0.55f;

    private Collider col;
    private Transform player;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Hook()
    {
        // Domain reload kapalıyken her Play'de abonelik birikmesin
        SceneManager.sceneLoaded -= AttachToBlockers;
        SceneManager.sceneLoaded += AttachToBlockers;
    }

    private static void AttachToBlockers(Scene scene, LoadSceneMode mode)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
            foreach (Collider c in root.GetComponentsInChildren<Collider>(true))
                if (c.gameObject.name == "MerdivenEngeli" && c.GetComponent<BlockedPassageHint>() == null)
                    c.gameObject.AddComponent<BlockedPassageHint>();
    }

    private void Awake()
    {
        col = GetComponent<Collider>();
    }

    private void Update()
    {
        if (Time.timeScale == 0f || !col.enabled) return;
        if (player == null)
        {
            player = FindAnyObjectByType<PlayerWalker>()?.transform;
            if (player == null) return;
        }
        Vector3 p = player.position + Vector3.up;
        if ((col.ClosestPoint(p) - p).sqrMagnitude > touchDistance * touchDistance) return;
        ScreenHint.Show(DinkLang.T("The stairs are closed.", "Merdiven kapalı.", "Die Treppe ist gesperrt.",
            "L'escalier est fermé.", "La escalera está cerrada.", "A escada está fechada.", "Лестница закрыта.", "楼梯被封住了。", "階段は閉ざされている。", "계단이 막혀 있다."));
    }
}
