using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Unity URP için sıfır-beyazlık (Zero-White Halo) ve sıfır-parlama özellikli
/// profesyonel şeffaf leke (PNG Decal) materyali oluşturur.
/// </summary>
public class CreateTrueAlphaDecal
{
    [MenuItem("Tools/Dink/Malzeme Ureticisi/Gercek Alpha Lekesi Olustur (Kare Kutuyu Yok Et)")]
    public static void GenerateTrueAlphaDecal()
    {
        string sourcePath = "Assets/_Project/Textures/isolated_mold_patch.jpg";
        string targetPngPath = "Assets/_Project/Textures/isolated_mold_patch_alpha.png";
        string matPath = "Assets/_Project/Materials";

        // 1. Kaynak dokunun okunabilir (Is Readable) olmasını sağla
        TextureImporter sourceImporter = AssetImporter.GetAtPath(sourcePath) as TextureImporter;
        if (sourceImporter != null && !sourceImporter.isReadable)
        {
            sourceImporter.isReadable = true;
            sourceImporter.SaveAndReimport();
        }

        Texture2D sourceTex = AssetDatabase.LoadAssetAtPath<Texture2D>(sourcePath);
        if (sourceTex == null)
        {
            Debug.LogError($"[Dink Decal] Kaynak görsel bulunamadı: {sourcePath}");
            return;
        }

        // 2. Yeni Alpha kanallı RGBA32 Texture2D oluştur
        int width = sourceTex.width;
        int height = sourceTex.height;
        Texture2D alphaTex = new Texture2D(width, height, TextureFormat.RGBA32, false);

        Color[] pixels = sourceTex.GetPixels();
        Color[] newPixels = new Color[pixels.Length];

        for (int i = 0; i < pixels.Length; i++)
        {
            Color p = pixels[i];
            float brightness = (p.r * 0.299f + p.g * 0.587f + p.b * 0.114f);

            float alpha = 0f;
            if (brightness < 0.85f)
            {
                alpha = Mathf.Clamp01((0.85f - brightness) / 0.85f * 1.8f);
            }

            // ÇÖZÜM: Pikselin RGB rengini doğrudan karanlık nem/küf rengi yapıyoruz.
            // Böylece şeffaf kenarlarda ASLA açık renk/beyaz harelenme kalmaz!
            newPixels[i] = new Color(0.03f, 0.02f, 0.02f, alpha);
        }

        alphaTex.SetPixels(newPixels);
        alphaTex.Apply();

        // 3. PNG olarak kaydet
        byte[] pngBytes = alphaTex.EncodeToPNG();
        File.WriteAllBytes(targetPngPath, pngBytes);
        AssetDatabase.Refresh();

        // 4. PNG İçe aktarma (Import Settings) ayarlarını düzelt
        TextureImporter pngImporter = AssetImporter.GetAtPath(targetPngPath) as TextureImporter;
        if (pngImporter != null)
        {
            pngImporter.alphaSource = TextureImporterAlphaSource.FromInput;
            pngImporter.alphaIsTransparency = true;
            pngImporter.wrapMode = TextureWrapMode.Clamp;
            pngImporter.SaveAndReimport();
        }

        // 5. URP Unlit/Lit Alpha Blend Materyali Oluştur
        if (!Directory.Exists(matPath))
        {
            Directory.CreateDirectory(matPath);
        }

        Shader targetShader = Shader.Find("Universal Render Pipeline/Unlit");
        if (targetShader == null)
        {
            targetShader = Shader.Find("Universal Render Pipeline/Lit");
        }

        Material alphaMat = new Material(targetShader);
        Texture2D savedPng = AssetDatabase.LoadAssetAtPath<Texture2D>(targetPngPath);
        alphaMat.SetTexture("_BaseMap", savedPng);
        alphaMat.SetTexture("_MainTex", savedPng);

        // URP Surface: Transparent, Blend: Alpha (Standard Alpha Blending)
        alphaMat.SetFloat("_Surface", 1); // Transparent
        alphaMat.SetFloat("_Blend", 0);   // Alpha Blend
        alphaMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        alphaMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        alphaMat.SetInt("_ZWrite", 0);
        alphaMat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

        // Işıkta parlama yapmaması için Smoothness değerini sıfırla
        if (alphaMat.HasProperty("_Smoothness"))
        {
            alphaMat.SetFloat("_Smoothness", 0.0f);
        }

        string saveMatPath = Path.Combine(matPath, "Isolated_Mold_TrueAlpha.mat");
        AssetDatabase.CreateAsset(alphaMat, saveMatPath);
        
        // 6. DUVAR ÇATLAĞI (Wall Crack Decal) İŞLEME
        ProcessDecal("Assets/_Project/Textures/wall_crack_decal.jpg", 
                     "Assets/_Project/Textures/wall_crack_decal_alpha.png", 
                     Path.Combine(matPath, "Wall_Crack_TrueAlpha.mat"));

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[Dink Decal] Kusursuz Sıfır-Hareli Alpha Materyalleri oluşturuldu!");
    }

    private static void ProcessDecal(string sourcePath, string targetPngPath, string saveMatPath)
    {
        TextureImporter sourceImporter = AssetImporter.GetAtPath(sourcePath) as TextureImporter;
        if (sourceImporter != null && !sourceImporter.isReadable)
        {
            sourceImporter.isReadable = true;
            sourceImporter.SaveAndReimport();
        }

        Texture2D sourceTex = AssetDatabase.LoadAssetAtPath<Texture2D>(sourcePath);
        if (sourceTex == null) return;

        int width = sourceTex.width;
        int height = sourceTex.height;
        Texture2D alphaTex = new Texture2D(width, height, TextureFormat.RGBA32, false);

        Color[] pixels = sourceTex.GetPixels();
        Color[] newPixels = new Color[pixels.Length];

        for (int i = 0; i < pixels.Length; i++)
        {
            Color p = pixels[i];
            float brightness = (p.r * 0.299f + p.g * 0.587f + p.b * 0.114f);

            float alpha = 0f;
            if (brightness < 0.85f)
            {
                alpha = Mathf.Clamp01((0.85f - brightness) / 0.85f * 1.8f);
            }

            newPixels[i] = new Color(0.03f, 0.02f, 0.02f, alpha);
        }

        alphaTex.SetPixels(newPixels);
        alphaTex.Apply();

        byte[] pngBytes = alphaTex.EncodeToPNG();
        File.WriteAllBytes(targetPngPath, pngBytes);
        AssetDatabase.Refresh();

        TextureImporter pngImporter = AssetImporter.GetAtPath(targetPngPath) as TextureImporter;
        if (pngImporter != null)
        {
            pngImporter.alphaSource = TextureImporterAlphaSource.FromInput;
            pngImporter.alphaIsTransparency = true;
            pngImporter.wrapMode = TextureWrapMode.Clamp;
            pngImporter.SaveAndReimport();
        }

        Shader targetShader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Universal Render Pipeline/Lit");
        Material alphaMat = new Material(targetShader);
        Texture2D savedPng = AssetDatabase.LoadAssetAtPath<Texture2D>(targetPngPath);
        alphaMat.SetTexture("_BaseMap", savedPng);
        alphaMat.SetTexture("_MainTex", savedPng);

        alphaMat.SetFloat("_Surface", 1);
        alphaMat.SetFloat("_Blend", 0);
        alphaMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        alphaMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        alphaMat.SetInt("_ZWrite", 0);
        alphaMat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

        if (alphaMat.HasProperty("_Smoothness"))
        {
            alphaMat.SetFloat("_Smoothness", 0.0f);
        }

        AssetDatabase.CreateAsset(alphaMat, saveMatPath);
    }
}
