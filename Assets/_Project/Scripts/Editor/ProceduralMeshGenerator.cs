using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Unity Editor içinde çalışarak dışarıdan hazır 3D model indirmeye gerek kalmadan
/// özelleştirilmiş Pil (AA Battery) ve Mektup (Curved Paper) 3D mesh assetleri üretir.
/// </summary>
public class ProceduralMeshGenerator
{
    private const string MESH_FOLDER_PATH = "Assets/_Project/Meshes";

    [MenuItem("Tools/Dink/Model Ureticisi/Pil Mesh'i Olustur (AA Battery)")]
    public static void CreateBatteryMesh()
    {
        EnsureFolderExists();

        Mesh mesh = new Mesh();
        mesh.name = "Battery_Mesh";

        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> triangles = new List<int>();

        // Parametreler
        int radialSegments = 24; // Silindir dairesellik yumuşaklığı
        float bodyRadius = 0.10f; // Pil ana gövde yarıçapı
        float bodyHeight = 0.40f; // Pil ana gövde yüksekliği
        float capRadius = 0.04f;  // Artı kutbu başlık yarıçapı
        float capHeight = 0.04f;  // Artı kutbu başlık yüksekliği

        float halfHeight = bodyHeight / 2f;

        // 1. SİLİNDİR GÖVDE (Yan Yüzeyler - Body Sides)
        int bodyStartIdx = vertices.Count;
        for (int i = 0; i <= radialSegments; i++)
        {
            float u = (float)i / radialSegments;
            float angle = u * Mathf.PI * 2f;
            float x = Mathf.Cos(angle) * bodyRadius;
            float z = Mathf.Sin(angle) * bodyRadius;

            // Alt köşe
            vertices.Add(new Vector3(x, -halfHeight, z));
            uvs.Add(new Vector2(u, 0f));

            // Üst köşe (ana gövde omzu)
            vertices.Add(new Vector3(x, halfHeight, z));
            uvs.Add(new Vector2(u, 0.85f)); // Etiket alanı %85'e kadar
        }

        // Gövde Yan Üçgenleri
        for (int i = 0; i < radialSegments; i++)
        {
            int current = bodyStartIdx + i * 2;
            int next = current + 2;

            triangles.Add(current);
            triangles.Add(current + 1);
            triangles.Add(next);

            triangles.Add(next);
            triangles.Add(current + 1);
            triangles.Add(next + 1);
        }

        // 2. ARTI KUTBU BAŞLIĞI (Positive Pole Cap Sides)
        int capStartIdx = vertices.Count;
        for (int i = 0; i <= radialSegments; i++)
        {
            float u = (float)i / radialSegments;
            float angle = u * Mathf.PI * 2f;
            float x = Mathf.Cos(angle) * capRadius;
            float z = Mathf.Sin(angle) * capRadius;

            // Başlık altı
            vertices.Add(new Vector3(x, halfHeight, z));
            uvs.Add(new Vector2(u, 0.86f));

            // Başlık üstü
            vertices.Add(new Vector3(x, halfHeight + capHeight, z));
            uvs.Add(new Vector2(u, 0.98f));
        }

        // Başlık Yan Üçgenleri
        for (int i = 0; i < radialSegments; i++)
        {
            int current = capStartIdx + i * 2;
            int next = current + 2;

            triangles.Add(current);
            triangles.Add(current + 1);
            triangles.Add(next);

            triangles.Add(next);
            triangles.Add(current + 1);
            triangles.Add(next + 1);
        }

        // 3. ÜST KAPAK (Artı başlığının tepesi)
        int topCenterIdx = vertices.Count;
        vertices.Add(new Vector3(0, halfHeight + capHeight, 0));
        uvs.Add(new Vector2(0.5f, 1f));

        int topRimStartIdx = vertices.Count;
        for (int i = 0; i <= radialSegments; i++)
        {
            float u = (float)i / radialSegments;
            float angle = u * Mathf.PI * 2f;
            float x = Mathf.Cos(angle) * capRadius;
            float z = Mathf.Sin(angle) * capRadius;

            vertices.Add(new Vector3(x, halfHeight + capHeight, z));
            uvs.Add(new Vector2(0.5f + Mathf.Cos(angle) * 0.1f, 0.95f + Mathf.Sin(angle) * 0.05f));
        }

        for (int i = 0; i < radialSegments; i++)
        {
            triangles.Add(topCenterIdx);
            triangles.Add(topRimStartIdx + i + 1);
            triangles.Add(topRimStartIdx + i);
        }

        // 4. ALT KAPAK (Eksi kutbu alt yüzey)
        int bottomCenterIdx = vertices.Count;
        vertices.Add(new Vector3(0, -halfHeight, 0));
        uvs.Add(new Vector2(0.5f, 0f));

        int bottomRimStartIdx = vertices.Count;
        for (int i = 0; i <= radialSegments; i++)
        {
            float u = (float)i / radialSegments;
            float angle = u * Mathf.PI * 2f;
            float x = Mathf.Cos(angle) * bodyRadius;
            float z = Mathf.Sin(angle) * bodyRadius;

            vertices.Add(new Vector3(x, -halfHeight, z));
            uvs.Add(new Vector2(0.5f + Mathf.Cos(angle) * 0.1f, 0.05f + Mathf.Sin(angle) * 0.05f));
        }

        for (int i = 0; i < radialSegments; i++)
        {
            triangles.Add(bottomCenterIdx);
            triangles.Add(bottomRimStartIdx + i);
            triangles.Add(bottomRimStartIdx + i + 1);
        }

        mesh.vertices = vertices.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.triangles = triangles.ToArray();

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.RecalculateTangents();

        string savePath = Path.Combine(MESH_FOLDER_PATH, "Battery_Mesh.asset");
        SaveMeshAsset(mesh, savePath);
    }

    [MenuItem("Tools/Dink/Model Ureticisi/Mektup Mesh'i Olustur (Curved Paper)")]
    public static void CreateLetterMesh()
    {
        EnsureFolderExists();

        Mesh mesh = new Mesh();
        mesh.name = "Letter_Mesh";

        int gridX = 12;
        int gridY = 16;

        float width = 0.30f;  // Genişlik (X)
        float height = 0.42f; // Boy (Y - mektup kağıdı orantısı)

        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> triangles = new List<int>();

        // 1. ÖN YÜZ (Front Face) - Hafif bükülmüş dalgalı kağıt yapısı
        for (int y = 0; y <= gridY; y++)
        {
            float v = (float)y / gridY;
            float posY = (v - 0.5f) * height;

            for (int x = 0; x <= gridX; x++)
            {
                float u = (float)x / gridX;
                float posX = (u - 0.5f) * width;

                // Kağıda kıvrım/büküm vermek için Z ekseninde hafif bir sinüs dalgası ve köşe bükülmesi
                float waveZ = Mathf.Sin(u * Mathf.PI) * 0.015f + Mathf.Cos(v * Mathf.PI * 2f) * 0.006f;
                
                // Köşelerin hafif yukarı bükülmesi (organik yıpranmış kağıt kıvrılması)
                float cornerCurl = (Mathf.Abs(u - 0.5f) * Mathf.Abs(v - 0.5f)) * 0.035f;

                float posZ = waveZ + cornerCurl;

                vertices.Add(new Vector3(posX, posY, posZ));
                uvs.Add(new Vector2(u, v));
            }
        }

        int gridWidth = gridX + 1;
        for (int y = 0; y < gridY; y++)
        {
            for (int x = 0; x < gridX; x++)
            {
                int current = y * gridWidth + x;
                int next = current + gridWidth;

                // Ön yüz üçgenleri (Saat yönü)
                triangles.Add(current);
                triangles.Add(next);
                triangles.Add(current + 1);

                triangles.Add(current + 1);
                triangles.Add(next);
                triangles.Add(next + 1);
            }
        }

        // 2. ARKA YÜZ (Back Face - Çift taraflı görünüm)
        int frontVertCount = vertices.Count;
        for (int i = 0; i < frontVertCount; i++)
        {
            // Aynı noktaları kopyala ama arka yüz için mikro milimetre arkaya koy
            Vector3 vert = vertices[i];
            vertices.Add(new Vector3(vert.x, vert.y, vert.z - 0.0005f));
            uvs.Add(uvs[i]);
        }

        for (int y = 0; y < gridY; y++)
        {
            for (int x = 0; x < gridX; x++)
            {
                int current = frontVertCount + y * gridWidth + x;
                int next = current + gridWidth;

                // Arka yüz üçgenleri (Ters saat yönü - arkadan bakınca görünmesi için)
                triangles.Add(current);
                triangles.Add(current + 1);
                triangles.Add(next);

                triangles.Add(current + 1);
                triangles.Add(next + 1);
                triangles.Add(next);
            }
        }

        mesh.vertices = vertices.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.triangles = triangles.ToArray();

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.RecalculateTangents();

        string savePath = Path.Combine(MESH_FOLDER_PATH, "Letter_Mesh.asset");
        SaveMeshAsset(mesh, savePath);
    }

    [MenuItem("Tools/Dink/Model Ureticisi/3D Tablo Cercevesi Mesh'i Olustur (Picture Frame)")]
    public static void CreatePictureFrameMesh()
    {
        EnsureFolderExists();

        Mesh mesh = new Mesh();
        mesh.name = "Frame_Mesh";

        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> triangles = new List<int>();

        // Parametreler (Metre cinsinden)
        float outerWidth = 0.85f;   // Dış genişlik (85 cm)
        float outerHeight = 1.15f;  // Dış yükseklik (1.15 m)
        float borderWidth = 0.10f;  // Çerçeve ahşap kenar kalınlığı (10 cm)
        float frameDepth = 0.07f;   // Duvardan öne taşma derinliği (7 cm)
        float insetDepth = 0.02f;   // İç tuval girintisi (2 cm)

        float halfW = outerWidth / 2f;
        float halfH = outerHeight / 2f;
        float innerW = halfW - borderWidth;
        float innerH = halfH - borderWidth;

        // 1. ÖN AHŞAP ÇERÇEVE YÜZEYLERİ (4 Parça Kenarlık)
        // Dış köşeler
        Vector3 v0 = new Vector3(-halfW, halfH, 0);       // Sol Üst Dış
        Vector3 v1 = new Vector3(halfW, halfH, 0);        // Sağ Üst Dış
        Vector3 v2 = new Vector3(halfW, -halfH, 0);       // Sağ Alt Dış
        Vector3 v3 = new Vector3(-halfW, -halfH, 0);      // Sol Alt Dış

        // Dış ön köşeler (Derinlikli)
        Vector3 v0_front = new Vector3(-halfW, halfH, frameDepth);
        Vector3 v1_front = new Vector3(halfW, halfH, frameDepth);
        Vector3 v2_front = new Vector3(halfW, -halfH, frameDepth);
        Vector3 v3_front = new Vector3(-halfW, -halfH, frameDepth);

        // İç ön köşeler (Girintili)
        Vector3 v0_in = new Vector3(-innerW, innerH, frameDepth - 0.015f);
        Vector3 v1_in = new Vector3(innerW, innerH, frameDepth - 0.015f);
        Vector3 v2_in = new Vector3(innerW, -innerH, frameDepth - 0.015f);
        Vector3 v3_in = new Vector3(-innerW, -innerH, frameDepth - 0.015f);

        // İç arka arka-panel (Derinlikteki tuval yüzeyi)
        Vector3 v0_back = new Vector3(-innerW, innerH, insetDepth);
        Vector3 v1_back = new Vector3(innerW, innerH, insetDepth);
        Vector3 v2_back = new Vector3(innerW, -innerH, insetDepth);
        Vector3 v3_back = new Vector3(-innerW, -innerH, insetDepth);

        // --- MESH OLUŞTURMA KODLARI ---
        // Üst Kenar
        AddQuad(vertices, uvs, triangles, v0_front, v1_front, v1_in, v0_in);
        // Sağ Kenar
        AddQuad(vertices, uvs, triangles, v1_front, v2_front, v2_in, v1_in);
        // Alt Kenar
        AddQuad(vertices, uvs, triangles, v2_front, v3_front, v3_in, v2_in);
        // Sol Kenar
        AddQuad(vertices, uvs, triangles, v3_front, v0_front, v0_in, v3_in);

        // Dış Yan Yüzeyler (Duvardan öne çıkan 3D derinliklikler)
        AddQuad(vertices, uvs, triangles, v0, v1, v1_front, v0_front); // Üst Yan
        AddQuad(vertices, uvs, triangles, v1, v2, v2_front, v1_front); // Sağ Yan
        AddQuad(vertices, uvs, triangles, v2, v3, v3_front, v2_front); // Alt Yan
        AddQuad(vertices, uvs, triangles, v3, v0, v0_front, v3_front); // Sol Yan

        // İç Tuval Paneli (Boş Derinlikli Tuval Yüzeyi)
        AddQuad(vertices, uvs, triangles, v0_back, v1_back, v2_back, v3_back);

        // İç Girinti Duvarları
        AddQuad(vertices, uvs, triangles, v0_in, v1_in, v1_back, v0_back);
        AddQuad(vertices, uvs, triangles, v1_in, v2_in, v2_back, v1_back);
        AddQuad(vertices, uvs, triangles, v2_in, v3_in, v3_back, v2_back);
        AddQuad(vertices, uvs, triangles, v3_in, v0_in, v0_back, v3_back);

        mesh.vertices = vertices.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.triangles = triangles.ToArray();

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.RecalculateTangents();

        string savePath = Path.Combine(MESH_FOLDER_PATH, "Frame_Mesh.asset");
        SaveMeshAsset(mesh, savePath);
    }

    private static void AddQuad(List<Vector3> verts, List<Vector2> uvs, List<int> tris, Vector3 topLeft, Vector3 topRight, Vector3 bottomRight, Vector3 bottomLeft)
    {
        int idx = verts.Count;
        verts.Add(topLeft);
        verts.Add(topRight);
        verts.Add(bottomRight);
        verts.Add(bottomLeft);

        uvs.Add(new Vector2(0, 1));
        uvs.Add(new Vector2(1, 1));
        uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(0, 0));

        tris.Add(idx);
        tris.Add(idx + 1);
        tris.Add(idx + 2);

        tris.Add(idx);
        tris.Add(idx + 2);
        tris.Add(idx + 3);
    }

    [MenuItem("Tools/Dink/Model Ureticisi/Tavan Ampulu Mesh'i Olustur (Hanging Lamp)")]
    public static void CreateHangingLampMesh()
    {
        EnsureFolderExists();
        Mesh mesh = new Mesh();
        mesh.name = "Hanging_Lamp_Mesh";

        List<Vector3> verts = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> tris = new List<int>();

        // 1. Tavan Rozeti (Küçük Silindir)
        AddCylinder(verts, uvs, tris, new Vector3(0, 0, 0), 0.08f, 0.03f, 8);
        // 2. Sarkıt Kablo (İnce Uzun Silindir)
        AddCylinder(verts, uvs, tris, new Vector3(0, -0.03f, 0), 0.008f, 0.90f, 6);
        // 3. Metal Ampul Duyu (Daha Kalın Silindir)
        AddCylinder(verts, uvs, tris, new Vector3(0, -0.93f, 0), 0.04f, 0.08f, 8);
        // 4. Cam Ampul (Küre/Kutu Yaklaşımı)
        AddCylinder(verts, uvs, tris, new Vector3(0, -1.01f, 0), 0.05f, 0.10f, 8);

        mesh.vertices = verts.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        SaveMeshAsset(mesh, Path.Combine(MESH_FOLDER_PATH, "Hanging_Lamp_Mesh.asset"));
    }

    [MenuItem("Tools/Dink/Model Ureticisi/Kapi Ustu Armatur Mesh'i Olustur (Door Light)")]
    public static void CreateDoorLightMesh()
    {
        EnsureFolderExists();
        Mesh mesh = new Mesh();
        mesh.name = "Door_Light_Mesh";

        List<Vector3> verts = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> tris = new List<int>();

        // Koyu döküm metal kutu (Genişlik: 25cm, Yükseklik: 12cm, Derinlik: 10cm)
        AddBox(verts, uvs, tris, Vector3.zero, new Vector3(0.25f, 0.12f, 0.10f));

        mesh.vertices = verts.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        SaveMeshAsset(mesh, Path.Combine(MESH_FOLDER_PATH, "Door_Light_Mesh.asset"));
    }

    [MenuItem("Tools/Dink/Model Ureticisi/Zemin Izgarasi Mesh'i Olustur (Floor Grate)")]
    public static void CreateFloorGrateMesh()
    {
        EnsureFolderExists();
        Mesh mesh = new Mesh();
        mesh.name = "Floor_Grate_Mesh";

        List<Vector3> verts = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> tris = new List<int>();

        // Paslı zemin ızgarası kutusu (60cm x 40cm, yükseklik 2cm)
        AddBox(verts, uvs, tris, Vector3.zero, new Vector3(0.60f, 0.02f, 0.40f));

        mesh.vertices = verts.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        SaveMeshAsset(mesh, Path.Combine(MESH_FOLDER_PATH, "Floor_Grate_Mesh.asset"));
    }

    private static void AddCylinder(List<Vector3> verts, List<Vector2> uvs, List<int> tris, Vector3 topCenter, float radius, float height, int segments)
    {
        int startIdx = verts.Count;
        for (int i = 0; i <= segments; i++)
        {
            float angle = (i % segments) * Mathf.PI * 2f / segments;
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;

            verts.Add(topCenter + new Vector3(x, 0, z));
            verts.Add(topCenter + new Vector3(x, -height, z));

            uvs.Add(new Vector2((float)i / segments, 1));
            uvs.Add(new Vector2((float)i / segments, 0));
        }

        for (int i = 0; i < segments; i++)
        {
            int idx = startIdx + (i * 2);
            tris.Add(idx);
            tris.Add(idx + 1);
            tris.Add(idx + 2);

            tris.Add(idx + 1);
            tris.Add(idx + 3);
            tris.Add(idx + 2);
        }
    }

    private static void AddBox(List<Vector3> verts, List<Vector2> uvs, List<int> tris, Vector3 center, Vector3 size)
    {
        Vector3 half = size / 2f;
        Vector3 v0 = center + new Vector3(-half.x, half.y, -half.z);
        Vector3 v1 = center + new Vector3(half.x, half.y, -half.z);
        Vector3 v2 = center + new Vector3(half.x, -half.y, -half.z);
        Vector3 v3 = center + new Vector3(-half.x, -half.y, -half.z);

        Vector3 v4 = center + new Vector3(-half.x, half.y, half.z);
        Vector3 v5 = center + new Vector3(half.x, half.y, half.z);
        Vector3 v6 = center + new Vector3(half.x, -half.y, half.z);
        Vector3 v7 = center + new Vector3(-half.x, -half.y, half.z);

        AddQuad(verts, uvs, tris, v0, v1, v2, v3); // Ön
        AddQuad(verts, uvs, tris, v5, v4, v7, v6); // Arka
        AddQuad(verts, uvs, tris, v4, v0, v3, v7); // Sol
        AddQuad(verts, uvs, tris, v1, v5, v6, v2); // Sağ
        AddQuad(verts, uvs, tris, v4, v5, v1, v0); // Üst
        AddQuad(verts, uvs, tris, v3, v2, v6, v7); // Alt
    }

    private static void EnsureFolderExists()
    {
        if (!Directory.Exists(MESH_FOLDER_PATH))
        {
            Directory.CreateDirectory(MESH_FOLDER_PATH);
            AssetDatabase.Refresh();
        }
    }

    private static void SaveMeshAsset(Mesh mesh, string path)
    {
        Mesh existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if (existing != null)
        {
            existing.Clear();
            EditorUtility.CopySerialized(mesh, existing);
            AssetDatabase.SaveAssets();
            Debug.Log($"[Dink Mesh Generator] Mevcut mesh güncellendi: {path}");
        }
        else
        {
            AssetDatabase.CreateAsset(mesh, path);
            AssetDatabase.SaveAssets();
            Debug.Log($"[Dink Mesh Generator] Yeni 3D Mesh asseti oluşturuldu: {path}");
        }

        Selection.activeObject = AssetDatabase.LoadAssetAtPath<Mesh>(path);
    }
}
