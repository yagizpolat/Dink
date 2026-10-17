using UnityEngine;

/// <summary>
/// Zindan sahnesinde (Zindan.unity) oyuncu her düştüğünde
/// 9 farklı zorlu minigame panelinden birini RASTGELE seçip aktifleştirir.
/// </summary>
public class ZindanMinigameSelector : MonoBehaviour
{
    [Header("Zindan Minigame Panelleri (9 Adet)")]
    [Tooltip("0: Antik Rün, 1: Kasa Kadranı, 2: Sigorta Şalteri, 3: Radyo Frekansı, 4: Asit Nötrleme, 5: Boru Hattı, 6: Antik Terazi, 7: İğne Yakalama, 8: Çip Devresi")]
    [SerializeField] private GameObject[] minigamePanelleri = new GameObject[9];

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
            Debug.Log($"[ZindanSelector] 9 Minigame arasından rastgele seçildi: Index {secilenIndex} ({minigamePanelleri[secilenIndex].name})");
        }
    }
}
