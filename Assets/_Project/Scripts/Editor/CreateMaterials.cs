using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Projeye eklenen pil ve mektup kaplama görsellerini (Textures)
/// otomatik olarak Unity Materyallerine (Materials) dönüştürür.
/// </summary>
public class CreateMaterials
{
    [MenuItem("Tools/Dink/Malzeme Ureticisi/Pil ve Mektup Materyallerini Olustur")]
    public static void GenerateMaterials()
    {
        string matPath = "Assets/_Project/Materials";
        if (!Directory.Exists(matPath))
        {
            Directory.CreateDirectory(matPath);
            AssetDatabase.Refresh();
        }

        Shader targetShader = Shader.Find("Universal Render Pipeline/Lit");
        if (targetShader == null)
        {
            targetShader = Shader.Find("Standard");
        }

        // 1. PIL MATERYALI (Battery Material)
        Texture2D batteryTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Project/Textures/battery_label_texture.jpg");
        if (batteryTex != null)
        {
            Material batteryMat = new Material(targetShader);
            batteryMat.SetTexture("_BaseMap", batteryTex);
            batteryMat.SetTexture("_MainTex", batteryTex);
            AssetDatabase.CreateAsset(batteryMat, Path.Combine(matPath, "Battery_Material.mat"));
            Debug.Log("[Dink] Battery_Material.mat oluşturuldu.");
        }

        // 2. MEKTUP MATERYALI (Letter Material)
        Texture2D letterTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Project/Textures/letter_paper_texture.jpg");
        if (letterTex != null)
        {
            Material letterMat = new Material(targetShader);
            letterMat.SetTexture("_BaseMap", letterTex);
            letterMat.SetTexture("_MainTex", letterTex);
            AssetDatabase.CreateAsset(letterMat, Path.Combine(matPath, "Letter_Material.mat"));
            Debug.Log("[Dink] Letter_Material.mat oluşturuldu.");
        }

        // 3. TEKİNSİZ TABLO MATERYALİ (Portrait Material - Yüksek Kontrast)
        Texture2D portraitTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Project/Textures/creepy_portrait_texture.jpg");
        if (portraitTex != null)
        {
            Material portraitMat = new Material(targetShader);
            portraitMat.SetTexture("_BaseMap", portraitTex);
            portraitMat.SetTexture("_MainTex", portraitTex);

            // Karanlık odada soluk yüzün hafifçe belirmesi için çok ince Emission (Işıma) haritası
            if (portraitMat.HasProperty("_EmissionMap"))
            {
                portraitMat.EnableKeyword("_EMISSION");
                portraitMat.SetTexture("_EmissionMap", portraitTex);
                portraitMat.SetColor("_EmissionColor", new Color(0.12f, 0.12f, 0.10f)); // Çok hafif, loş bir ışıma
            }

            AssetDatabase.CreateAsset(portraitMat, Path.Combine(matPath, "Portrait_Material.mat"));
            Debug.Log("[Dink] Yüksek kontrastlı Portrait_Material.mat başarıyla oluşturuldu.");
        }

        // 4. AHŞAP TABLO ÇERÇEVESİ MATERYALİ (Frame Material)
        Texture2D frameWoodTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Project/Textures/dark_wood_frame_texture.jpg");
        if (frameWoodTex != null)
        {
            Material frameMat = new Material(targetShader);
            frameMat.SetTexture("_BaseMap", frameWoodTex);
            frameMat.SetTexture("_MainTex", frameWoodTex);

            if (frameMat.HasProperty("_Smoothness"))
            {
                frameMat.SetFloat("_Smoothness", 0.3f); // Cilalı ahşap parlaması
            }

            AssetDatabase.CreateAsset(frameMat, Path.Combine(matPath, "Frame_Material.mat"));
            Debug.Log("[Dink] Frame_Material.mat başarıyla oluşturuldu.");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
