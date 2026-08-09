using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Unity Editor içinde çalışarak sahnedeki tüm duvar nesnelerinin (Wall)
/// Renderer.bounds verilerini okur ve zemin hizasına (bounds.min.y) milimetrik
/// olarak oturan 3D süpürgelik (baseboard) çıtalarını C# algoritmasıyla üretir.
/// </summary>
public class BaseboardGenerator
{
    private const string MAT_FOLDER_PATH = "Assets/_Project/Materials";

    [MenuItem("Tools/Dink/Mimari/Otomatik Supurgelik Olustur (Baseboards)")]
    public static void GenerateBaseboards()
    {
        // 1. ADIM: EĞER SAHNEDE DAHA ÖNCE OLUŞTURULMUŞ 'Baseboards' VARSA SİL veya YENİLE
        GameObject parentObj = GameObject.Find("Baseboards");
        if (parentObj != null)
        {
            Undo.DestroyObjectImmediate(parentObj);
        }

        parentObj = new GameObject("Baseboards");
        Undo.RegisterCreatedObjectUndo(parentObj, "Created Baseboards");

        // 2. ADIM: MATERYAL HAZIRLAMA (Baseboard_Material.mat)
        Material baseboardMat = GetOrCreateBaseboardMaterial();

        // 3. ADIM: DUVARLARI BULMA (Sahnedeki adında 'wall' veya 'zemin' olan objeler)
        Renderer[] allRenderers = Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None);
        int generatedCount = 0;

        foreach (Renderer wallRenderer in allRenderers)
        {
            string objName = wallRenderer.gameObject.name.ToLower();
            
            // Sadece duvar olan nesneleri işle (zemin, tavan, kapı veya dekorasyonlar harici)
            if (!objName.Contains("wall") || objName.Contains("door") || objName.Contains("kapi"))
            {
                continue;
            }

            // 4. ADIM: VEKTÖREL HESAPLAMA (Duvar Bounds ve Boyutları)
            Bounds bounds = wallRenderer.bounds;
            Vector3 center = bounds.center;
            Vector3 size = bounds.size;
            float bottomY = bounds.min.y; // Duvarın tam zemin birleşim hizası

            // Süpürgelik fiziksel ölçüleri
            float trimHeight = 0.16f; // 16 cm yükseklik
            float trimDepth = 0.05f;  // 5 cm kalınlık

            // Duvarın yönünü anlama (X boyunca mı Z boyunca mı uzanıyor?)
            bool isAlignedWithX = size.x > size.z;

            GameObject trimObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            trimObj.name = $"Baseboard_{wallRenderer.gameObject.name}";
            trimObj.transform.SetParent(parentObj.transform);

            // 5. ADIM: POZİSYON VE SCALE HESABI (Oda İç Yüzeyine Kaydırma)
            // Süpürgelikleri duvarın betonunun ortasından odanın içine doğru kaydırıyoruz
            Vector3 roomCenter = Vector3.zero; // Oda merkezi (0,0,0)
            Vector3 dirToRoomCenter = (roomCenter - center);
            dirToRoomCenter.y = 0;
            dirToRoomCenter.Normalize();

            // Duvarın kalınlığının yarısı kadar odanın içine doğru kaydırma miktarı (offset)
            float wallThicknessOffset = isAlignedWithX ? (size.z / 2f) - (trimDepth / 2f) : (size.x / 2f) - (trimDepth / 2f);
            Vector3 insidePosition = center + (dirToRoomCenter * wallThicknessOffset);

            if (isAlignedWithX)
            {
                // Duvar X ekseninde uzanıyorsa
                float trimWidth = size.x;
                trimObj.transform.position = new Vector3(insidePosition.x, bottomY + (trimHeight / 2f), insidePosition.z);
                trimObj.transform.localScale = new Vector3(trimWidth, trimHeight, trimDepth);
            }
            else
            {
                // Duvar Z ekseninde uzanıyorsa
                float trimWidth = size.z;
                trimObj.transform.position = new Vector3(insidePosition.x, bottomY + (trimHeight / 2f), insidePosition.z);
                trimObj.transform.localScale = new Vector3(trimDepth, trimHeight, trimWidth);
            }

            // Rotasyon: Duvarın kendi transform rotasyonunu kopyala
            trimObj.transform.rotation = wallRenderer.transform.rotation;

            // 6. ADIM: MATERYAL ATAMA VE COLLIDER TEMİZLİĞİ
            MeshRenderer mr = trimObj.GetComponent<MeshRenderer>();
            if (mr != null && baseboardMat != null)
            {
                mr.sharedMaterial = baseboardMat;
            }

            // Süpürgeliğe fiziksel çarpışma gerekmediği için collider'ı kaldırıyoruz
            BoxCollider boxCol = trimObj.GetComponent<BoxCollider>();
            if (boxCol != null)
            {
                Object.DestroyImmediate(boxCol);
            }

            generatedCount++;
        }

        if (generatedCount > 0)
        {
            Selection.activeGameObject = parentObj;
            Debug.Log($"[Dink Mimari] Toplam {generatedCount} duvar için süpürgelikler milimetrik olarak oluşturuldu!");
        }
        else
        {
            Debug.LogWarning("[Dink Mimari] Sahnede ismi 'wall' içeren duvar nesnesi bulunamadı. Lütfen duvar objelerinizin adında 'wall' geçtiğinden emin olun.");
        }
    }

    /// <summary>
    /// URP Uyumlu Koyu Ahşap/Siyah Süpürgelik Materyali (Baseboard_Material.mat) oluşturur.
    /// </summary>
    private static Material GetOrCreateBaseboardMaterial()
    {
        if (!Directory.Exists(MAT_FOLDER_PATH))
        {
            Directory.CreateDirectory(MAT_FOLDER_PATH);
        }

        string matPath = Path.Combine(MAT_FOLDER_PATH, "Baseboard_Material.mat");
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);

        if (mat == null)
        {
            Shader targetShader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            mat = new Material(targetShader);

            // Koyu ahşap/antrasit renk tonu (RGB 0.1, 0.08, 0.07)
            Color darkWoodColor = new Color(0.10f, 0.08f, 0.07f);

            if (mat.HasProperty("_BaseColor"))
            {
                mat.SetColor("_BaseColor", darkWoodColor);
            }
            if (mat.HasProperty("_Color"))
            {
                mat.SetColor("_Color", darkWoodColor);
            }

            // Hafif mat ahşap cilası (Smoothness = 0.25)
            if (mat.HasProperty("_Smoothness"))
            {
                mat.SetFloat("_Smoothness", 0.25f);
            }

            AssetDatabase.CreateAsset(mat, matPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        return mat;
    }
}
