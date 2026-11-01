using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Blender'dan gelen HafizaMarket.fbx'i oyuna hazırlar (Kaynak/BlenderBetikleri/hafiza_market_aktar.py'den sonra çalıştır):
/// pişmiş dokulardan URP malzemeleri kurar, FBX'e bağlar, cam parçasını gölgesiz/çarpışmasız yapar ve
/// açık sahnedeki "HafizaMarket" nesnesini yeniler. Işıklar ve sesler sahnede elle durur, dokunulmaz.
/// </summary>
public static class HafizaMarketKur
{
    const string Fbx = "Assets/_Project/Models/HafizaMarket/HafizaMarket.fbx";
    const string Tex = "Assets/_Project/Textures/HafizaMarket/";
    const string Mat = "Assets/_Project/Materials/HafizaMarket";
    
    // Blender'daki ışık gücü; Unity'de 0.6 ile çarpılır
    static readonly Dictionary<string, float> Isikli = new Dictionary<string, float>
    {
        {"Armatur_Difuzor_0", 9}, {"Armatur_Difuzor_1", 9}, {"Armatur_Difuzor_2", 9}, {"Armatur_Difuzor_3", 3.5f},
        {"Armatur_Difuzor_4", 9}, {"Dolap_Baslik", 1.2f}, {"Dolap_Isik", 6}, {"Dolap_LED", 8}, {"YK_LCD", 1.6f}
    };
    static readonly Dictionary<string, float> Metal = new Dictionary<string, float>
    {
        {"Aluminyum", 1}, {"Krom", 1}, {"Urun_Kapak", 1}, {"Urun_Teneke", 0.35f}
    };
    static readonly Dictionary<string, Color> Cam = new Dictionary<string, Color>
    {
        {"Cam", new Color(0.8f, 0.9f, 0.95f, 0.08f)}, {"Vitrin_Cam", new Color(0.8f, 0.9f, 0.95f, 0.08f)},
        {"Sise_Su", new Color(0.75f, 0.88f, 1f, 0.3f)}, {"Sise_Yag", new Color(0.9f, 0.75f, 0.2f, 0.45f)}
    };

    [MenuItem("Dink/Hafıza Marketini Kur")]
    public static void Kur()
    {
        Malzemeler();
        Yerlestir();
    }

    static void Malzemeler()
    {
        if (!AssetDatabase.IsValidFolder(Mat)) AssetDatabase.CreateFolder("Assets/_Project/Materials", "HafizaMarket");
        var imp = (ModelImporter)AssetImporter.GetAtPath(Fbx);
        imp.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
        var lit = Shader.Find("Universal Render Pipeline/Lit");

        foreach (var o in AssetDatabase.LoadAllAssetRepresentationsAtPath(Fbx))
        {
            if (!(o is Material kaynak)) continue;
            string ad = kaynak.name.StartsWith("M_") ? kaynak.name.Substring(2) : kaynak.name;
            string yol = $"{Mat}/HM_{ad}.mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(yol);
            if (m == null) { m = new Material(lit); AssetDatabase.CreateAsset(m, yol); }
            m.shader = lit;

            var renk = AssetDatabase.LoadAssetAtPath<Texture2D>($"{Tex}HM_{ad}_Renk.png");
            m.SetTexture("_BaseMap", renk);
            m.SetColor("_BaseColor", Color.white);
            string nor = $"{Tex}HM_{ad}_Normal.png";
            if (File.Exists(nor))
            {
                var ti = (TextureImporter)AssetImporter.GetAtPath(nor);
                if (ti.textureType != TextureImporterType.NormalMap) { ti.textureType = TextureImporterType.NormalMap; ti.SaveAndReimport(); }
                m.SetTexture("_BumpMap", AssetDatabase.LoadAssetAtPath<Texture2D>(nor));
            }
            m.SetFloat("_Metallic", Metal.TryGetValue(ad, out var met) ? met : 0f);
            m.SetFloat("_Smoothness", Metal.ContainsKey(ad) ? 0.6f : 0.35f);
            m.SetFloat("_Surface", 0);
            m.SetFloat("_AlphaClip", ad == "Kapi_Etiketleri" ? 1 : 0);
            m.SetFloat("_Cutoff", 0.5f);

            if (Isikli.TryGetValue(ad, out var guc))
            {
                m.SetColor("_EmissionColor", Color.white * guc * 0.6f);
                m.SetTexture("_EmissionMap", renk);
            }
            else m.SetColor("_EmissionColor", Color.black);

            if (Cam.TryGetValue(ad, out var ton))
            {
                m.SetFloat("_Surface", 1); m.SetFloat("_Blend", 0);
                m.SetColor("_BaseColor", ton); m.SetFloat("_Smoothness", 0.95f);
            }

            AnahtarlariKur(m);
            if (Isikli.ContainsKey(ad)) { m.EnableKeyword("_EMISSION"); m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive; }
            EditorUtility.SetDirty(m);
            imp.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), kaynak.name), m);
        }
        imp.SaveAndReimport();
        AssetDatabase.SaveAssets();
    }

    // URP malzeme anahtarları (yüzey türü, emisyon, normal) ancak kendi GUI yardımcısıyla kalıcı yazılıyor
    static void AnahtarlariKur(Material m)
    {
        var t = System.Type.GetType("UnityEditor.BaseShaderGUI, Unity.RenderPipelines.Universal.Editor");
        var mi = t.GetMethod("SetMaterialKeywords", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
        var args = new object[mi.GetParameters().Length]; args[0] = m;
        mi.Invoke(null, args);
    }

    static void Yerlestir()
    {
        var sahne = EditorSceneManager.GetActiveScene();
        var eski = GameObject.Find("HafizaMarket");
        Transform engel = eski != null ? eski.transform.Find("Vitrin_Engel") : null;

        var model = AssetDatabase.LoadAssetAtPath<GameObject>(Fbx);
        var kok = (GameObject)PrefabUtility.InstantiatePrefab(model, sahne);
        kok.name = "HafizaMarket";
        kok.transform.SetPositionAndRotation(Vector3.zero, Quaternion.Euler(0, 180, 0));  // Blender (x,y,z) -> Unity (x,z,y)

        foreach (var r in kok.GetComponentsInChildren<MeshRenderer>())
        {
            // Cam (aktarımda ayrı nesne) gölge düşürmez, çarpışmaz: vitrin ışığı dışarı vurur, kapıdan girilir
            bool cam = r.name.EndsWith("_Cam");
            r.shadowCastingMode = cam ? ShadowCastingMode.Off : ShadowCastingMode.On;
            if (!cam) r.gameObject.AddComponent<MeshCollider>().sharedMesh = r.GetComponent<MeshFilter>().sharedMesh;
        }
        if (engel != null) engel.SetParent(kok.transform, true);
        if (eski != null) Object.DestroyImmediate(eski);

        EditorSceneManager.MarkSceneDirty(sahne);
        EditorSceneManager.SaveScene(sahne);
        Debug.Log("Hafıza marketi kuruldu.");
    }
}
