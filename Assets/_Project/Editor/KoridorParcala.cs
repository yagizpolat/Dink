using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Birleşik Koridor.fbx mesh'inden etkileşimli parçaları (kapı kanadı, petek, yangın dolabı) ayırır.
/// Bir kutunun içinde kalan üçgenler ayrı mesh asset'e taşınır; kalan koridor yeni bir mesh olarak kurulur
/// (yerinde değiştirilen mesh'in ekrana eski hali çizilebiliyordu). Menü: Dink/Koridoru Parçala.
/// </summary>
public static class KoridorParcala
{
    private const string Klasor = "Assets/_Project/Models/Koridor/";

    private struct Parca
    {
        public string ad;
        public Bounds kutu;      // dünya koordinatında
        public Vector3 pivot;    // parçanın kökü (kapı: menteşe)
        public Parca(string ad, Vector3 min, Vector3 max, Vector3 pivot)
        {
            this.ad = ad; kutu = new Bounds(); kutu.SetMinMax(min, max); this.pivot = pivot;
        }
    }

    // Blender (x, y, z) -> Unity (x, z, y)
    private static readonly Parca[] Parcalar =
    {
        new Parca("Kapi2_Kanat", new Vector3(1.87f, 0.005f, 6.025f), new Vector3(1.995f, 2.125f, 6.975f), new Vector3(1.99f, 0f, 6.03f)),
        new Parca("Petek", new Vector3(-7.995f, 0.03f, 10.5f), new Vector3(-7.7f, 0.86f, 11.6f), new Vector3(-7.86f, 0f, 11.05f)),
        new Parca("YanginDolabi", new Vector3(1.5f, 0.85f, 8.05f), new Vector3(1.905f, 1.7f, 8.75f), new Vector3(1.8f, 0.88f, 8.4f)),
        // L kolunun güney duvarındaki 6 numara: menteşe zil tarafında (batı), kanat iç yüzü z=10.01
        // Asansör kapısı: menteşe batıda (x 0.46), kol doğuda; lobiye (+z) açılır
        new Parca("Asansor_Kanat", new Vector3(0.455f, 0.005f, -0.075f), new Vector3(1.445f, 2.145f, 0.025f), new Vector3(0.46f, 0f, -0.05f)),
        // Kuyunun karanlık düzlemi: kabine yer açmak için yalnız kaldırılır (sahnede karşılığı yok)
        new Parca("Asansor_Kuyu", new Vector3(0.39f, -0.21f, -0.61f), new Vector3(1.51f, 2.61f, -0.49f), new Vector3(0.95f, 0f, -0.55f)),
        new Parca("Kapi6_Kanat", new Vector3(-6.075f, 0.005f, 10.005f), new Vector3(-5.125f, 2.125f, 10.13f), new Vector3(-6.07f, 0f, 10.01f)),
    };

    [MenuItem("Dink/Koridoru Parçala")]
    public static void Calistir()
    {
        var koridor = GameObject.Find("Koridor");
        var mf = koridor.GetComponent<MeshFilter>();
        var mr = koridor.GetComponent<MeshRenderer>();
        Mesh kaynak = null;
        foreach (var o in AssetDatabase.LoadAllAssetsAtPath(Klasor + "Koridor.fbx"))
            if (o is Mesh m) kaynak = m;
        Transform t = koridor.transform;

        var v = kaynak.vertices; var n = kaynak.normals; var tg = kaynak.tangents; var uv = kaynak.uv;
        var dunya = new Vector3[v.Length];
        for (int i = 0; i < v.Length; i++) dunya[i] = t.TransformPoint(v[i]);
        var malz = mr.sharedMaterials;

        var kalan = new List<int>[kaynak.subMeshCount];
        for (int s = 0; s < kaynak.subMeshCount; s++) kalan[s] = new List<int>(kaynak.GetTriangles(s));

        foreach (var p in Parcalar)
        {
            var harita = new Dictionary<int, int>();
            var pv = new List<Vector3>(); var pn = new List<Vector3>(); var pt = new List<Vector4>(); var pu = new List<Vector2>();
            var altlar = new List<int[]>(); var pm = new List<Material>();
            for (int s = 0; s < kalan.Length; s++)
            {
                var tri = kalan[s]; var kal = new List<int>(); var al = new List<int>();
                for (int i = 0; i < tri.Count; i += 3)
                {
                    if (!(Icinde(p, dunya[tri[i]]) && Icinde(p, dunya[tri[i + 1]]) && Icinde(p, dunya[tri[i + 2]])))
                    {
                        kal.Add(tri[i]); kal.Add(tri[i + 1]); kal.Add(tri[i + 2]);
                        continue;
                    }
                    for (int j = 0; j < 3; j++)
                    {
                        int a = tri[i + j];
                        if (!harita.TryGetValue(a, out int b))
                        {
                            b = pv.Count; harita[a] = b;
                            pv.Add(dunya[a] - p.pivot);
                            pn.Add(t.TransformDirection(n[a]));
                            Vector3 d = t.TransformDirection(new Vector3(tg[a].x, tg[a].y, tg[a].z));
                            pt.Add(new Vector4(d.x, d.y, d.z, tg[a].w));
                            pu.Add(uv[a]);
                        }
                        al.Add(b);
                    }
                }
                kalan[s] = kal;
                if (al.Count > 0) { altlar.Add(al.ToArray()); pm.Add(malz[s]); }
            }

            var mesh = new Mesh { name = p.ad };
            mesh.SetVertices(pv); mesh.SetNormals(pn); mesh.SetTangents(pt); mesh.SetUVs(0, pu);
            mesh.subMeshCount = altlar.Count;
            for (int s = 0; s < altlar.Count; s++) mesh.SetTriangles(altlar[s], s);
            mesh.RecalculateBounds();
            Kaydet(mesh, p.ad);
            Bagla(p, mesh, pm.ToArray());
            Debug.Log($"KoridorParcala: {p.ad} {altlar.Count} malzeme, {pv.Count} köşe");
        }

        var temiz = new Mesh { name = "Koridor_Temiz", indexFormat = kaynak.indexFormat };
        temiz.vertices = v; temiz.normals = n; temiz.tangents = tg; temiz.uv = uv;
        temiz.subMeshCount = kalan.Length;
        for (int s = 0; s < kalan.Length; s++) temiz.SetTriangles(kalan[s], s);
        temiz.RecalculateBounds();
        Kaydet(temiz, "Koridor_Temiz");
        foreach (var f in Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (f.name == "Koridor")
            {
                f.sharedMesh = temiz;
                f.GetComponent<MeshCollider>().sharedMesh = temiz;
            }
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(koridor.scene);
    }

    private static bool Icinde(Parca p, Vector3 w) => p.kutu.Contains(w);

    private static Mesh Kaydet(Mesh mesh, string ad)
    {
        string yol = Klasor + ad + ".asset";
        AssetDatabase.DeleteAsset(yol);
        AssetDatabase.CreateAsset(mesh, yol);
        return mesh;
    }

    /// <summary>Sahnedeki aynı adlı bütün nesnelere (her kat kopyasına) takar; kapı kanadı ise DaireN_Kapi/Menteşe/Kanat'a (yoksa kurar).</summary>
    private static void Bagla(Parca p, Mesh mesh, Material[] malz)
    {
        bool kapi = p.ad.EndsWith("_Kanat");
        string kok = !kapi ? p.ad : p.ad.StartsWith("Kapi") ? "Daire" + p.ad.Substring(4, 1) + "_Kapi" : p.ad.Replace("_Kanat", "_Kapi");
        var hedefler = new List<GameObject>();
        foreach (var r in Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            Transform t = r.transform;
            if (!kapi && t.name == p.ad) hedefler.Add(t.gameObject);
            if (kapi && t.name == "Kanat" && t.parent != null && t.parent.parent != null && t.parent.parent.name == kok) hedefler.Add(t.gameObject);
        }
        if (hedefler.Count == 0 && kapi)
        {
            var k = new GameObject(kok);
            var m = new GameObject("Menteşe"); m.transform.SetParent(k.transform, false);
            var h = new GameObject("Kanat"); h.transform.SetParent(m.transform, false);
            h.AddComponent<MeshFilter>(); h.AddComponent<MeshRenderer>();
            hedefler.Add(h);
        }
        if (hedefler.Count == 0) { Debug.Log("KoridorParcala: yalnız kaldırıldı " + p.ad); return; }
        foreach (var hedef in hedefler)
        {
            (kapi ? hedef.transform.parent.parent : hedef.transform).position = p.pivot;
            hedef.GetComponent<MeshFilter>().sharedMesh = mesh;
            hedef.GetComponent<MeshRenderer>().sharedMaterials = malz;
            if (!kapi) continue;
            var bc = hedef.GetComponent<BoxCollider>();
            if (bc != null) Object.DestroyImmediate(bc);
            hedef.AddComponent<BoxCollider>();
        }
    }
}
