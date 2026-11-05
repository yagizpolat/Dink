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

    const string MakarnaFbx = "Assets/_Project/Models/HafizaMarket/HafizaMakarna.fbx";

    // P2: koli makarnaları + raf hedef yerleri ayrı nesneler; MarketShelfStocking bağlanır
    static void Makarnalar(UnityEngine.SceneManagement.Scene sahne)
    {
        var eski = GameObject.Find("HafizaMakarna");
        if (eski != null) Object.DestroyImmediate(eski);
        var model = AssetDatabase.LoadAssetAtPath<GameObject>(MakarnaFbx);
        if (model == null) return;
        var karton = AssetDatabase.LoadAssetAtPath<Material>($"{Mat}/HM_Urun_Karton.mat");
        var kok = (GameObject)PrefabUtility.InstantiatePrefab(model, sahne);
        kok.name = "HafizaMakarna";
        kok.transform.SetPositionAndRotation(Vector3.zero, Quaternion.Euler(0, 180, 0));

        var koli = new List<Transform>(); var raf = new List<Transform>();
        foreach (var r in kok.GetComponentsInChildren<MeshRenderer>(true))
        {
            r.sharedMaterials = new[] { karton };
            (r.name.StartsWith("Koli_") ? koli : raf).Add(r.transform);
        }
        koli.Sort((a, b) => string.CompareOrdinal(a.name, b.name));

        var oyun = new GameObject("Makarna_Dizme");
        oyun.transform.SetParent(kok.transform, false);
        var kb = Kutu(oyun.transform, "Koli_Al", koli, 0.06f); kb.gameObject.AddComponent<InteractionTarget>().Configure(InteractionTarget.Verb.Take, 2.2f);
        var rb = Kutu(oyun.transform, "Raf_Bosluk", raf, 0.03f); rb.gameObject.AddComponent<InteractionTarget>().Configure(InteractionTarget.Verb.Place, 2.2f);

        var d = oyun.AddComponent<MarketShelfStocking>();
        var so = new SerializedObject(d);
        so.FindProperty("box").objectReferenceValue = kb;
        so.FindProperty("shelfGap").objectReferenceValue = rb;
        void Liste(string ad, List<Transform> l) { var p = so.FindProperty(ad); p.arraySize = l.Count; for (int i = 0; i < l.Count; i++) p.GetArrayElementAtIndex(i).objectReferenceValue = l[i]; }
        Liste("boxPackets", koli); Liste("shelfSlots", raf);
        so.ApplyModifiedPropertiesWithoutUndo();

        // P3: raf dolunca karanlıktan çıkış yok
        var cik = oyun.AddComponent<MarketNoExit>();
        var so2 = new SerializedObject(cik);
        so2.FindProperty("stocking").objectReferenceValue = d;
        var oyuncu = Object.FindObjectOfType<PlayerWalker>();
        so2.FindProperty("player").objectReferenceValue = oyuncu != null ? oyuncu.transform : null;
        so2.ApplyModifiedPropertiesWithoutUndo();

        DepoKapi(sahne, d);
        MarketKapi(sahne);
    }

    // Ön kapı: iki kanat; oyuncu önüne (ya da içeriden arkasına) gelince içeri açılır, açık kalır
    static void MarketKapi(UnityEngine.SceneManagement.Scene sahne)
    {
        var eski = GameObject.Find("MarketKapi");
        if (eski != null) Object.DestroyImmediate(eski);
        var kok = new GameObject("MarketKapi");
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(kok, sahne);
        var oy = Object.FindFirstObjectByType<PlayerWalker>();
        var ses = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/gorseller/Sesler/Audios/Environment/Entrances/Door_Open_2.wav");
        (string fbx, float x, float aci)[] kanatlar = { ("Sol", 0.545f, 85f), ("Sag", 1.455f, -85f) };
        foreach (var k in kanatlar)
        {
            var model = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/_Project/Models/HafizaMarket/HafizaMarketKapi{k.fbx}.fbx");
            if (model == null) continue;
            var m = new GameObject("Kanat" + k.fbx + "_Mentese").transform;
            m.SetParent(kok.transform, false);
            m.SetPositionAndRotation(new Vector3(k.x, 0f, -0.08f), Quaternion.Euler(0, 180, 0));
            var g = (GameObject)PrefabUtility.InstantiatePrefab(model, sahne);
            g.transform.SetParent(m, false); g.transform.localPosition = Vector3.zero; g.transform.localRotation = Quaternion.identity;
            foreach (var r in g.GetComponentsInChildren<MeshRenderer>(true))
            {
                var ms = r.sharedMaterials;
                for (int i = 0; i < ms.Length; i++)
                {
                    if (ms[i] == null) continue;
                    string ad = ms[i].name.StartsWith("M_") ? ms[i].name.Substring(2) : ms[i].name;
                    var hm = AssetDatabase.LoadAssetAtPath<Material>($"{Mat}/HM_{ad}.mat");
                    if (hm != null) ms[i] = hm;
                }
                r.sharedMaterials = ms;
                r.gameObject.AddComponent<BoxCollider>();
            }
            var ik = m.gameObject.AddComponent<ItmeKapi>();
            var so = new SerializedObject(ik);
            so.FindProperty("leaf").objectReferenceValue = m;
            so.FindProperty("player").objectReferenceValue = oy != null ? oy.transform : null;
            so.FindProperty("pushPoint").vector3Value = new Vector3(1f, 0f, -0.08f);
            so.FindProperty("pushRadius").floatValue = 1.1f;
            so.FindProperty("openAngle").floatValue = k.aci;
            if (k.fbx == "Sol") so.FindProperty("openSound").objectReferenceValue = ses;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    const string DepoFbx = "Assets/_Project/Models/HafizaMarket/HafizaDepoKapi.fbx";

    // G1: raf dolunca sağ duvardaki depo kapısının ardında ışık yanar, sağ kanat aralanır
    static void DepoKapi(UnityEngine.SceneManagement.Scene sahne, MarketShelfStocking d)
    {
        var eski = GameObject.Find("DepoKapi");
        if (eski != null) Object.DestroyImmediate(eski);
        var model = AssetDatabase.LoadAssetAtPath<GameObject>(DepoFbx);
        if (model == null) return;

        var kok = new GameObject("DepoKapi");
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(kok, sahne);
        var mentese = new GameObject("KanatSag_Mentese").transform;      // Blender (6.08, 2.35, 0.02)
        mentese.SetParent(kok.transform, false);
        mentese.SetPositionAndRotation(new Vector3(6.08f, 0.02f, 2.35f), Quaternion.Euler(0, 180, 0));
        var kanat = (GameObject)PrefabUtility.InstantiatePrefab(model, sahne);
        kanat.transform.SetParent(mentese, false);
        kanat.transform.localPosition = Vector3.zero; kanat.transform.localRotation = Quaternion.identity;
        foreach (var r in kanat.GetComponentsInChildren<MeshRenderer>(true))
        {
            var ms = r.sharedMaterials;
            for (int i = 0; i < ms.Length; i++)
            {
                if (ms[i] == null) continue;
                string ad = ms[i].name.StartsWith("M_") ? ms[i].name.Substring(2) : ms[i].name;
                var hm = AssetDatabase.LoadAssetAtPath<Material>($"{Mat}/HM_{ad}.mat");
                if (hm != null) ms[i] = hm;
            }
            r.sharedMaterials = ms;
            var bc = r.gameObject.AddComponent<BoxCollider>();         // geçit G2'ye dek kapalı
        }

        var isik = new GameObject("Depo_Floresan").AddComponent<Light>();
        isik.transform.SetParent(kok.transform, false);
        isik.transform.position = new Vector3(7.4f, 2.45f, 1.85f);
        isik.type = LightType.Point; isik.range = 4.5f; isik.color = new Color(0.82f, 0.93f, 1f);
        isik.shadows = LightShadows.Soft;
        var ses = isik.gameObject.AddComponent<AudioSource>();
        ses.clip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/HafizaMarket/BuzdolabiVizilti.wav");
        ses.loop = true; ses.playOnAwake = false; ses.spatialBlend = 1f; ses.rolloffMode = AudioRolloffMode.Linear;
        ses.minDistance = 0.5f; ses.maxDistance = 9f; ses.pitch = 1.35f;

        var g = kok.AddComponent<DepoKapiSizinti>();
        var so = new SerializedObject(g);
        so.FindProperty("stocking").objectReferenceValue = d;
        so.FindProperty("leaf").objectReferenceValue = mentese;
        // Tek kanat 0.5 m: oyuncu sığmaz, itilince ikinci kanat da açılır
        var solModel = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Models/HafizaMarket/HafizaDepoKapiSol.fbx");
        if (solModel != null)
        {
            var sm2 = new GameObject("KanatSol_Mentese").transform;          // Blender (6.08, 1.35, 0.02)
            sm2.SetParent(kok.transform, false);
            sm2.SetPositionAndRotation(new Vector3(6.08f, 0.02f, 1.35f), Quaternion.Euler(0, 180, 0));
            var sg2 = (GameObject)PrefabUtility.InstantiatePrefab(solModel, sahne);
            sg2.transform.SetParent(sm2, false); sg2.transform.localPosition = Vector3.zero; sg2.transform.localRotation = Quaternion.identity;
            foreach (var r in sg2.GetComponentsInChildren<MeshRenderer>(true))
            {
                var ms = r.sharedMaterials;
                for (int i = 0; i < ms.Length; i++)
                {
                    if (ms[i] == null) continue;
                    string ad = ms[i].name.StartsWith("M_") ? ms[i].name.Substring(2) : ms[i].name;
                    var hm = AssetDatabase.LoadAssetAtPath<Material>($"{Mat}/HM_{ad}.mat");
                    if (hm != null) ms[i] = hm;
                }
                r.sharedMaterials = ms;
                r.gameObject.AddComponent<BoxCollider>();
            }
            so.FindProperty("leafB").objectReferenceValue = sm2;
        }
        so.FindProperty("backLight").objectReferenceValue = isik;
        so.FindProperty("hum").objectReferenceValue = ses;
        // G2: koridor ışıkları (URP nesne başına 8 ışık sınırı: market 5 + depo 1 + koridor 2)
        var kor = so.FindProperty("corridorLights"); kor.arraySize = 2;
        float[] xs = { 10.65f, 16.65f };
        for (int i = 0; i < xs.Length; i++)
        {
            var k = new GameObject("Koridor_Floresan_" + i).AddComponent<Light>();
            k.transform.SetParent(kok.transform, false);
            k.transform.position = new Vector3(xs[i], 3.35f, 1.85f);            // G3: okul tavanı 3.6
            k.type = LightType.Point; k.range = 6.5f; k.color = new Color(0.82f, 0.93f, 1f);
            k.intensity = i == 1 ? 2.5f : 5f; k.shadows = LightShadows.Soft;
            kor.GetArrayElementAtIndex(i).objectReferenceValue = k;
        }
        var oy = Object.FindFirstObjectByType<PlayerWalker>();
        so.FindProperty("player").objectReferenceValue = oy != null ? oy.transform : null;
        so.ApplyModifiedPropertiesWithoutUndo();

        // S1: sınıf kapısı (koridor sonu), itince sınıfa açılır; sınıf yalnız tahta ışığıyla
        var sm = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Models/HafizaMarket/HafizaSinifKapi.fbx");
        if (sm != null)
        {
            var sk = new GameObject("SinifKapi_Mentese").transform;          // Blender (24.17, 1.35, 0)
            sk.SetParent(kok.transform, false);
            sk.SetPositionAndRotation(new Vector3(24.17f, 0f, 1.35f), Quaternion.Euler(0, 180, 0));
            var sg = (GameObject)PrefabUtility.InstantiatePrefab(sm, sahne);
            sg.transform.SetParent(sk, false); sg.transform.localPosition = Vector3.zero; sg.transform.localRotation = Quaternion.identity;
            foreach (var r in sg.GetComponentsInChildren<MeshRenderer>(true))
            {
                var ms = r.sharedMaterials;
                for (int i = 0; i < ms.Length; i++)
                {
                    if (ms[i] == null) continue;
                    string ad = ms[i].name.StartsWith("M_") ? ms[i].name.Substring(2) : ms[i].name;
                    var hm = AssetDatabase.LoadAssetAtPath<Material>($"{Mat}/HM_{ad}.mat");
                    if (hm != null) ms[i] = hm;
                }
                r.sharedMaterials = ms;
                r.gameObject.AddComponent<BoxCollider>();
            }
            var ik = kok.AddComponent<ItmeKapi>();
            var so3 = new SerializedObject(ik);
            so3.FindProperty("leaf").objectReferenceValue = sk;
            so3.FindProperty("player").objectReferenceValue = oy != null ? oy.transform : null;
            so3.FindProperty("pushPoint").vector3Value = new Vector3(23.6f, 0f, 1.85f);
            so3.FindProperty("openAngle").floatValue = 85f;
            so3.FindProperty("openSound").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/gorseller/Sesler/Audios/Environment/Entrances/Door_Open_2.wav");
            so3.ApplyModifiedPropertiesWithoutUndo();

            var tl = new GameObject("Sinif_TahtaIsigi").AddComponent<Light>();
            tl.transform.SetParent(kok.transform, false);
            tl.transform.SetPositionAndRotation(new Vector3(31.4f, 3.4f, 1.85f), Quaternion.Euler(60, 90, 0));
            tl.type = LightType.Spot; tl.spotAngle = 110f; tl.range = 9f; tl.intensity = 6f;
            tl.color = new Color(0.85f, 0.93f, 1f); tl.shadows = LightShadows.Soft;
        }
        // S2: Hakan'ın sırasında yarım sınav kâğıdı (3. sıra, orta; kâğıdın üstü tahtaya bakar)
        var kag = GameObject.CreatePrimitive(PrimitiveType.Quad);
        Object.DestroyImmediate(kag.GetComponent<Collider>());
        kag.name = "Sinif_SinavKagidi";
        kag.transform.SetParent(kok.transform, false);
        kag.transform.SetPositionAndRotation(new Vector3(28.74f, 0.752f, 1.83f), Quaternion.Euler(90f, 97f, 0f));
        kag.transform.localScale = new Vector3(0.21f, 0.297f, 1f);
        string kyol = $"{Mat}/HM_SinavKagidi.mat";
        var km = AssetDatabase.LoadAssetAtPath<Material>(kyol);
        if (km == null) { km = new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(km, kyol); }
        km.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>($"{Tex}HM_SinavKagidi_Renk.png"));
        km.SetFloat("_Smoothness", 0.08f); EditorUtility.SetDirty(km);
        kag.GetComponent<MeshRenderer>().sharedMaterial = km;
        var hedef = new GameObject("Sinif_Kagit_Hedef");
        hedef.transform.SetParent(kok.transform, false);
        hedef.transform.position = new Vector3(28.74f, 0.78f, 1.83f);
        var hc = hedef.AddComponent<BoxCollider>(); hc.isTrigger = true; hc.size = new Vector3(0.4f, 0.08f, 0.45f);
        hedef.AddComponent<InteractionTarget>().Configure(InteractionTarget.Verb.Read, 1.8f);
        var sk2 = kok.AddComponent<SinifKagit>();
        var so4 = new SerializedObject(sk2);
        so4.FindProperty("paper").objectReferenceValue = kag.transform;
        so4.FindProperty("target").objectReferenceValue = hc;
        so4.ApplyModifiedPropertiesWithoutUndo();

        // S3: kâğıt okununca dolaplar sınıfa doğru çarpar, sonunda sınıf kapısı kapanır
        var dk = kok.AddComponent<DolapKapanma>();
        var so5 = new SerializedObject(dk);
        so5.FindProperty("paper").objectReferenceValue = sk2;
        so5.FindProperty("classDoor").objectReferenceValue = kok.GetComponent<ItmeKapi>();
        // Sınıf tavan floresanları: oda göz için okunur; kapı çarpınca sönükleşir, tamamen kararmaz
        var oda = so5.FindProperty("roomLights"); oda.arraySize = 2;
        float[] odaX = { 26.3f, 29.3f };
        for (int i = 0; i < odaX.Length; i++)
        {
            var ol = new GameObject("Sinif_Tavan_" + i).AddComponent<Light>();
            ol.transform.SetParent(kok.transform, false);
            ol.transform.position = new Vector3(odaX[i], 2.5f, 1.85f);       // tavanda leke yapmasın diye alçakta
            ol.type = LightType.Point; ol.range = 10f; ol.intensity = 5f;
            ol.color = new Color(0.86f, 0.93f, 1f); ol.shadows = LightShadows.Soft;
            oda.GetArrayElementAtIndex(i).objectReferenceValue = ol;
        }
        const string Ses = "Assets/gorseller/Sesler/Audios/Environment/Entrances/";
        var sl = so5.FindProperty("slams"); sl.arraySize = 2;
        sl.GetArrayElementAtIndex(0).objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(Ses + "Door_Close_1.wav");
        sl.GetArrayElementAtIndex(1).objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(Ses + "Door_Close_2.wav");
        so5.FindProperty("doorSlam").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(Ses + "Door_Close_1.wav");
        // S4: arka kapı (mutfağa), kilitli başlar; ardında sıcak ışık
        var am = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Models/HafizaMarket/HafizaArkaKapi.fbx");
        if (am != null)
        {
            var ah = new GameObject("ArkaKapi_Mentese").transform;           // Blender (25.0, 4.87, 0)
            ah.SetParent(kok.transform, false);
            ah.SetPositionAndRotation(new Vector3(25.0f, 0f, 4.87f), Quaternion.Euler(0, 180, 0));
            var ag = (GameObject)PrefabUtility.InstantiatePrefab(am, sahne);
            ag.transform.SetParent(ah, false); ag.transform.localPosition = Vector3.zero; ag.transform.localRotation = Quaternion.identity;
            foreach (var r in ag.GetComponentsInChildren<MeshRenderer>(true))
            {
                var ms = r.sharedMaterials;
                for (int i = 0; i < ms.Length; i++)
                {
                    if (ms[i] == null) continue;
                    string ad = ms[i].name.StartsWith("M_") ? ms[i].name.Substring(2) : ms[i].name;
                    var hm = AssetDatabase.LoadAssetAtPath<Material>($"{Mat}/HM_{ad}.mat");
                    if (hm != null) ms[i] = hm;
                }
                r.sharedMaterials = ms;
                r.gameObject.AddComponent<BoxCollider>();
            }
            var mi = new GameObject("Mutfak_Isigi").AddComponent<Light>();
            mi.transform.SetParent(kok.transform, false);
            mi.transform.position = new Vector3(26.0f, 2.0f, 7.3f);              // M1: mutfak masasının üstü
            mi.type = LightType.Point; mi.range = 6.5f; mi.intensity = 1.8f; mi.color = new Color(1f, 0.78f, 0.5f); mi.shadows = LightShadows.Soft;
            var holder = new GameObject("ArkaKapi"); holder.transform.SetParent(kok.transform, false);
            var ik2 = holder.AddComponent<ItmeKapi>();
            var so6 = new SerializedObject(ik2);
            so6.FindProperty("leaf").objectReferenceValue = ah;
            so6.FindProperty("player").objectReferenceValue = oy != null ? oy.transform : null;
            so6.FindProperty("pushPoint").vector3Value = new Vector3(25.45f, 0f, 4.3f);
            so6.FindProperty("openAngle").floatValue = -85f;
            so6.FindProperty("startLocked").boolValue = true;
            so6.FindProperty("revealLight").objectReferenceValue = mi;
            so6.FindProperty("openSound").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(Ses + "Door_Open_2.wav");
            so6.ApplyModifiedPropertiesWithoutUndo();
            so5.FindProperty("backDoor").objectReferenceValue = ik2;
        }
        var tahtaIsik = GameObject.Find("Sinif_TahtaIsigi");
        so5.FindProperty("boardLight").objectReferenceValue = tahtaIsik != null ? tahtaIsik.GetComponent<Light>() : null;
        so5.ApplyModifiedPropertiesWithoutUndo();

        // sınıf marketten ~29 m uzakta: çıkışsızlık yarıçapı onu kapsamalı
        var ne = Object.FindFirstObjectByType<MarketNoExit>();
        if (ne != null) { var sn = new SerializedObject(ne); sn.FindProperty("escapeRadius").floatValue = 34f; sn.ApplyModifiedPropertiesWithoutUndo(); }
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    // Nesnelerin dünya sınırlarını saran tetik kutu (oyuncuyu engellemez, ışın görür)
    static BoxCollider Kutu(Transform ust, string ad, List<Transform> l, float pay)
    {
        var b = l[0].GetComponent<Renderer>().bounds;
        foreach (var t in l) b.Encapsulate(t.GetComponent<Renderer>().bounds);
        b.Expand(pay * 2);
        var g = new GameObject(ad); g.transform.SetParent(ust, false);
        g.transform.SetPositionAndRotation(b.center, Quaternion.identity);
        var c = g.AddComponent<BoxCollider>(); c.isTrigger = true; c.size = b.size;
        return c;
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
        Makarnalar(sahne);

        EditorSceneManager.MarkSceneDirty(sahne);
        EditorSceneManager.SaveScene(sahne);
        Debug.Log("Hafıza marketi kuruldu.");
    }
}
