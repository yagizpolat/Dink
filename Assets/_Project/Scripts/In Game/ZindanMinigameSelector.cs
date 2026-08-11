using UnityEngine;

/// <summary>
/// Zindan sahnesinde (Zindan.unity) oyuncu her düştüğünde
/// 4 farklı minigame panelinden birini RASTGELE seçip aktifleştirir.
/// </summary>
public class ZindanMinigameSelector : MonoBehaviour
{
    [Header("Zindan Minigame Panelleri")]
    [Tooltip("0: Kilit Pimleri, 1: Kablo Bağlama, 2: Vana Basıncı, 3: Antik Rün")]
    [SerializeField] private GameObject[] minigamePanelleri = new GameObject[4];

    private void Start()
    {
        // Kamera sabit kalsın, oyun içi crosshair fareyi takip etsin
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = false;

        if (minigamePanelleri == null || minigamePanelleri.Length == 0)
        {
            Debug.LogWarning("[ZindanSelector] Atanmış minigame paneli bulunamadı!");
            return;
        }

        // Önce tüm panelleri kapat
        for (int i = 0; i < minigamePanelleri.Length; i++)
        {
            if (minigamePanelleri[i] != null)
            {
                minigamePanelleri[i].SetActive(false);
            }
        }

        // Rastgele 1 panel seç ve aç
        int secilenIndex = Random.Range(0, minigamePanelleri.Length);
        if (minigamePanelleri[secilenIndex] != null)
        {
            minigamePanelleri[secilenIndex].SetActive(true);
            Debug.Log($"[ZindanSelector] Rastgele Minigame Seçildi: Index {secilenIndex} ({minigamePanelleri[secilenIndex].name})");
        }
    }
}
