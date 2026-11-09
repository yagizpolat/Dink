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
        foreach (var eskiG in kok.GetComponents<PencereGecis>()) Object.DestroyImmediate(eskiG);
        foreach (var ad in new[] { "Mutfak_Not", "Mutfak_Not_Hedef", "Pencere_Hedef", "Sokak_Lamba", "Sokak_Dolgu", "Sokak_YanikPencere", "Sokak_PencereIsik" })
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

        // M3: pencere geçişi ve sokak ışığı
        var ph = new GameObject("Pencere_Hedef");
        ph.transform.SetParent(kok.transform, false);
        ph.transform.position = new Vector3(26.05f, 1.5f, 9.55f);
        var pc = ph.AddComponent<BoxCollider>();
        pc.isTrigger = true; pc.size = new Vector3(1.2f, 1.1f, 0.3f);
        ph.AddComponent<InteractionTarget>().Configure(InteractionTarget.Verb.Press, 1.7f);
        var pg = kok.AddComponent<PencereGecis>();
        var sg = new SerializedObject(pg);
        sg.FindProperty("note").objectReferenceValue = n;
        sg.FindProperty("target").objectReferenceValue = pc;
        var oy = GameObject.Find("Oyuncu");
        if (oy != null) sg.FindProperty("player").objectReferenceValue = oy.transform;
        sg.ApplyModifiedPropertiesWithoutUndo();

        var l1 = new GameObject("Sokak_Lamba").AddComponent<Light>();
        l1.transform.SetParent(kok.transform, false);
        l1.transform.SetPositionAndRotation(new Vector3(23f, 4.4f, 16.85f), Quaternion.Euler(90f, 0f, 0f));
        l1.type = LightType.Spot; l1.spotAngle = 150f; l1.range = 20f; l1.intensity = 60f;
        l1.color = new Color(1f, 0.72f, 0.42f); l1.shadows = LightShadows.Soft;
        var l2 = new GameObject("Sokak_Dolgu").AddComponent<Light>();
        l2.transform.SetParent(kok.transform, false);
        l2.transform.position = new Vector3(25f, 3f, 13.5f);
        l2.type = LightType.Point; l2.range = 16f; l2.intensity = 30f;
        l2.color = new Color(0.55f, 0.65f, 0.85f); l2.shadows = LightShadows.None;
        var kl = new SerializedObject(pg); var kp = kl.FindProperty("keepLights"); kp.arraySize = 3;
        kp.GetArrayElementAtIndex(0).objectReferenceValue = l1; kp.GetArrayElementAtIndex(1).objectReferenceValue = l2;
        // karşı cephede tek yanan pencere: kat 1, kapının üstündeki sıra
        var ym = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/HafizaMarket/HM_Sokak_YanikPencere.mat");
        if (ym == null)
        {
            ym = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            AssetDatabase.CreateAsset(ym, "Assets/_Project/Materials/HafizaMarket/HM_Sokak_YanikPencere.mat");
        }
        ym.SetColor("_BaseColor", new Color(1f, 0.68f, 0.34f));
        EditorUtility.SetDirty(ym);
        var yp = GameObject.CreatePrimitive(PrimitiveType.Quad);
        yp.name = "Sokak_YanikPencere";
        Object.DestroyImmediate(yp.GetComponent<Collider>());
        yp.transform.SetParent(kok.transform, false);
        yp.transform.SetPositionAndRotation(new Vector3(25.4f, 3.6f, 18.94f), Quaternion.Euler(0f, 0f, 0f));
        yp.transform.localScale = new Vector3(0.96f, 1.16f, 1f);
        yp.GetComponent<MeshRenderer>().sharedMaterial = ym;
        var l3 = new GameObject("Sokak_PencereIsik").AddComponent<Light>();
        l3.transform.SetParent(kok.transform, false);
        l3.transform.position = new Vector3(25.4f, 3.6f, 18.5f);
        l3.type = LightType.Point; l3.range = 5f; l3.intensity = 3f; l3.color = new Color(1f, 0.68f, 0.34f); l3.shadows = LightShadows.None;
        kl.FindProperty("keepLights").GetArrayElementAtIndex(2).objectReferenceValue = l3;
        kl.ApplyModifiedPropertiesWithoutUndo();
    }
}
