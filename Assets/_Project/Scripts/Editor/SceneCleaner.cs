using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

public class SceneCleaner : EditorWindow
{
    [MenuItem("Tools/Dink Projesi/Sahneyi Temizle ve Düzenle")]
    public static void CleanAndOrganizeScene()
    {
        // 1. Kategoriler için boş Parent objeleri oluşturalım (Varsa bulalım)
        GameObject managersParent = GetOrCreateParent("[--- YÖNETİCİLER & SİSTEM ---]");
        GameObject camLightParent = GetOrCreateParent("[--- KAMERA & IŞIKLAR ---]");
        GameObject environmentParent = GetOrCreateParent("[--- ÇEVRE & HARİTA ---]");
        GameObject interactablesParent = GetOrCreateParent("[--- ETKİLEŞİMLİ OBJELER ---]");
        GameObject uiParent = GetOrCreateParent("[--- ARAYÜZ (UI) ---]");

        // 2. Sahnedeki tüm root objeleri tarayalım
        GameObject[] rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();

        foreach (GameObject go in rootObjects)
        {
            // Kategorilerin kendilerini es geç
            if (go == managersParent || go == camLightParent || go == environmentParent || go == interactablesParent || go == uiParent)
                continue;

            string nameLower = go.name.ToLower();

            // --- YÖNETİCİLER & SİSTEM ---
            if (nameLower.Contains("manager") || nameLower.Contains("system") || nameLower.Contains("controller") || go.GetComponent<EventSystemHelper>() != null || go.name == "EventSystem")
            {
                go.transform.SetParent(managersParent.transform);
                if (go.name == "EventSystem") go.name = "Event System";
                continue;
            }

            // --- KAMERA & IŞIKLAR ---
            if (nameLower.Contains("camera") || nameLower.Contains("light") || nameLower.Contains("volume") || go.GetComponent<Camera>() != null || go.GetComponent<Light>() != null)
            {
                go.transform.SetParent(camLightParent.transform);
                
                // İsim düzenleme
                if (go.name == "Main Camera") go.name = "Ana Kamera (Main Camera)";
                if (go.name == "Directional Light") go.name = "Genel Işık (Directional Light)";
                if (go.name == "Spot Light") go.name = "Spot Işığı (Spot Light)";
                continue;
            }

            // --- ARAYÜZ ---
            if (nameLower.Contains("canvas") || go.GetComponent<Canvas>() != null)
            {
                go.transform.SetParent(uiParent.transform);
                if (go.name == "Canvas") go.name = "Oyun Arayüzü (Canvas)";
                continue;
            }

            // --- ETKİLEŞİMLİLER (Kapılar vb.) ---
            if (nameLower.Contains("door") || nameLower.Contains("kapı") || nameLower.Contains("gate"))
            {
                go.transform.SetParent(interactablesParent.transform);
                
                // Kapıları kendi içinde de düzenle
                if (go.name == "left door") go.name = "Sol Kapı (Door_Left)";
                if (go.name == "right door") go.name = "Sağ Kapı (Door_Right)";
                if (go.name == "Kapılar") go.name = "Kapı Grubu (Doors)";
                continue;
            }

            // --- ÇEVRE & HARİTA ---
            if (nameLower.Contains("wall") || nameLower.Contains("zemin") || nameLower.Contains("tavan") || nameLower.Contains("aksesuar") || nameLower.Contains("floor") || nameLower.Contains("ceiling") || nameLower.Contains("prop"))
            {
                go.transform.SetParent(environmentParent.transform);

                // Türkçe ve anlaşılır isimler yapalım
                if (go.name == "zemin") go.name = "Yer / Zemin (Floor)";
                if (go.name == "tavan") go.name = "Tavan (Ceiling)";
                if (go.name == "wall") go.name = "Duvar (Wall)";
                if (go.name == "wall (1)") go.name = "Duvar_1 (Wall_1)";
                if (go.name == "aksesuarlar") go.name = "Dekorasyonlar / Aksesuarlar";
                continue;
            }
        }

        // Kapı Grubu içindeki kapıları da organize edelim
        OrganizeDoorsHierarchy(interactablesParent);

        Debug.Log("Sahne başarıyla temizlendi ve kategorilere ayrıldı!");
    }

    private static GameObject GetOrCreateParent(string name)
    {
        GameObject go = GameObject.Find(name);
        if (go == null)
        {
            go = new GameObject(name);
            go.transform.position = Vector3.zero;
        }
        return go;
    }

    private static void OrganizeDoorsHierarchy(GameObject interactablesParent)
    {
        // Eğer sahnede "Kapı Grubu (Doors)" varsa, Sol ve Sağ kapıları onun içine taşıyalım
        Transform doorsGroup = interactablesParent.transform.Find("Kapı Grubu (Doors)");
        if (doorsGroup != null)
        {
            Transform leftDoor = interactablesParent.transform.Find("Sol Kapı (Door_Left)");
            Transform rightDoor = interactablesParent.transform.Find("Sağ Kapı (Door_Right)");

            if (leftDoor != null) leftDoor.SetParent(doorsGroup);
            if (rightDoor != null) rightDoor.SetParent(doorsGroup);
        }
    }
}

// Yardımcı boş sınıf
public class EventSystemHelper : MonoBehaviour {}
