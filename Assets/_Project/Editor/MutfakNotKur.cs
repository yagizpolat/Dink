using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>Mutfak masasına notu ve okuma sekansını kurar; tekrar çalışınca eskisini siler.</summary>
public static class MutfakNotKur
{
    const string TexYol = "Assets/_Project/Textures/HafizaMarket/HM_MutfakNot_Renk.png";
    const string MatYol = "Assets/_Project/Materials/HafizaMarket/HM_MutfakNot.mat";

    [MenuItem("Dink/Mutfak Notunu Kur")]
    static void Menu()
    {
        var kok = GameObject.Find("DepoKapi");
        if (kok == null) { Debug.LogError("DepoKapi yok: HafizaMarket sahnesini aç."); return; }
        Kur(kok);
        EditorSceneManager.MarkSceneDirty(kok.scene);
    }

    public static void Kur(GameObject kok)
    {
        foreach (var ad in new[] { "Mutfak_Not", "Mutfak_Not_Hedef" })
        {
            var t = kok.transform.Find(ad);
            if (t != null) Object.DestroyImmediate(t.gameObject);
        }
        foreach (var eski in kok.GetComponents<MutfakNot>())
        {
            var es = new SerializedObject(eski).FindProperty("note").objectReferenceValue;
            if (es != null) Object.DestroyImmediate(es);
            Object.DestroyImmediate(eski);
        }

        var ti = (TextureImporter)AssetImporter.GetAtPath(TexYol);
        ti.textureCompression = TextureImporterCompression.Uncompressed;
        ti.filterMode = FilterMode.Trilinear; ti.anisoLevel = 8; ti.mipMapBias = -1f;
        ti.SaveAndReimport();

        var m = AssetDatabase.LoadAssetAtPath<Material>(MatYol);
        if (m == null)
        {
            m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(m, MatYol);
        }
        m.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(TexYol));
        m.SetFloat("_Smoothness", 0.08f);
        EditorUtility.SetDirty(m);

        var not = GameObject.CreatePrimitive(PrimitiveType.Quad);
        not.name = "Mutfak_Not";
        Object.DestroyImmediate(not.GetComponent<Collider>());
        not.transform.SetParent(kok.transform, false);
        not.transform.position = new Vector3(26.72f, 1.252f, 7.2f);
        not.transform.rotation = Quaternion.Euler(90f, -83f, 0f);
        not.transform.localScale = new Vector3(0.21f, 0.21f, 1f);
        not.GetComponent<MeshRenderer>().sharedMaterial = m;

        var hedef = new GameObject("Mutfak_Not_Hedef");
        hedef.transform.SetParent(kok.transform, false);
        hedef.transform.position = new Vector3(26.72f, 1.28f, 7.2f);
        var hc = hedef.AddComponent<BoxCollider>();
        hc.isTrigger = true; hc.size = new Vector3(0.32f, 0.08f, 0.32f);
        hedef.AddComponent<InteractionTarget>().Configure(InteractionTarget.Verb.Read, 1.8f);

        var k = kok.AddComponent<SinifKagit>();
        var so = new SerializedObject(k);
        so.FindProperty("paper").objectReferenceValue = not.transform;
        so.FindProperty("target").objectReferenceValue = hc;
        so.FindProperty("showHint").boolValue = false;
        so.FindProperty("holdOffset").vector3Value = new Vector3(0f, -0.01f, 0.27f);
        so.FindProperty("rustle").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/HafizaMarket/KagitHisirti.wav");
        so.ApplyModifiedPropertiesWithoutUndo();

        var n = kok.AddComponent<MutfakNot>();
        var sn = new SerializedObject(n);
        sn.FindProperty("note").objectReferenceValue = k;
        sn.FindProperty("whisper").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/Zeynep/Brother_Whisper_A.mp3");
        var lamba = GameObject.Find("Mutfak_Isigi");
        if (lamba != null) sn.FindProperty("lamp").objectReferenceValue = lamba.GetComponent<Light>();
        sn.ApplyModifiedPropertiesWithoutUndo();
    }
}
