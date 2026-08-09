using UnityEditor;
using UnityEngine;

/// <summary>
/// Dink projesinin Ana Şablon Odası (Master Template Room) için
/// 3 sinematik elemanı (Tavan Ampulü + Cızırdama, Kapı Işıkları, Zemin Izgarası)
/// tek tıkla sahnede oluşturan ve konumlandıran Editor scripti.
/// </summary>
public class MasterRoomPropsSetup
{
    [MenuItem("Tools/Dink/Atmosfer/Ana Sablon Oda Aksesuarlarini Yerlestir (Master Props)")]
    public static void SetupMasterProps()
    {
        // 1. ÖNCELİKLE 3D MESH'LERİN OLUŞTURULDUĞUNDAN EMİN OL
        ProceduralMeshGenerator.CreateHangingLampMesh();
        ProceduralMeshGenerator.CreateDoorLightMesh();
        ProceduralMeshGenerator.CreateFloorGrateMesh();

        // Sahnedeki Ana Ebeveyn
        GameObject envParent = GameObject.Find("[--- ÇEVRE & HARİTA ---]");

        // --- 1. UZUN LAMBA VE IZGARAYI TEMİZLE ---
        GameObject oldLamp = GameObject.Find("Hanging_Lamp_Prop");
        if (oldLamp != null) Undo.DestroyObjectImmediate(oldLamp);

        GameObject oldGrate = GameObject.Find("Floor_Grate_Prop");
        if (oldGrate != null) Undo.DestroyObjectImmediate(oldGrate);

        // --- 2. KAPILARI BUL VE IŞIKLARI KAPILARIN İÇİNE ÇOCUK (CHILD) OLARAK BAĞLA ---
        GameObject oldDoorLights = GameObject.Find("Door_Indicator_Lights");
        if (oldDoorLights != null) Undo.DestroyObjectImmediate(oldDoorLights);

        Mesh doorLightMesh = AssetDatabase.LoadAssetAtPath<Mesh>("Assets/_Project/Meshes/Door_Light_Mesh.asset");
        Material baseMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Baseboard_Material.mat");

        Renderer[] allRenderers = Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None);
        int addedCount = 0;

        foreach (Renderer r in allRenderers)
        {
            string name = r.gameObject.name.ToLower();
            if ((name.Contains("door") || name.Contains("kapi")) && !name.Contains("frame") && !name.Contains("indicator"))
            {
                // Önceki varsa sil
                Transform oldChild = r.transform.Find("Door_Indicator_Light");
                if (oldChild != null) Undo.DestroyObjectImmediate(oldChild.gameObject);

                Bounds b = r.bounds;
                bool isLeftDoor = b.center.x < 0;
                Color lightColor = isLeftDoor ? new Color(0.2f, 0.5f, 1.0f) : new Color(1.0f, 0.2f, 0.2f); // Mavi / Kırmızı

                // Armatür objesini KAPININ KENDİ TRANFORM'UNUN ALTINA BAĞLIYORUZ (Child)
                GameObject indicatorObj = new GameObject("Door_Indicator_Light");
                indicatorObj.transform.SetParent(r.transform); // KAPININ ÇOCUĞU YAPILDI!
                Undo.RegisterCreatedObjectUndo(indicatorObj, "Created Door Indicator Child");

                // Kapı yerel ekseninde üst merkez pozisyonu
                indicatorObj.transform.localPosition = new Vector3(0, 0.52f, 0.02f);
                indicatorObj.transform.localRotation = Quaternion.identity;

                MeshFilter mf = indicatorObj.AddComponent<MeshFilter>();
                mf.sharedMesh = doorLightMesh;

                MeshRenderer mr = indicatorObj.AddComponent<MeshRenderer>();
                if (baseMat != null) mr.sharedMaterial = baseMat;

                // Armatür Işığı (Point Light)
                GameObject spotChild = new GameObject("Indicator_Light");
                spotChild.transform.SetParent(indicatorObj.transform);
                spotChild.transform.localPosition = new Vector3(0, 0, -0.06f);

                Light indLight = spotChild.AddComponent<Light>();
                indLight.type = LightType.Point;
                indLight.color = lightColor;
                indLight.intensity = 8.0f;
                indLight.range = 3.5f;

                addedCount++;
            }
        }

        Debug.Log($"[Dink Master] Toplam {addedCount} kapı ışığı kapıların içine çocuk (Child) olarak bağlandı! Artık kapıyla birlikte süzülecekler.");
    }
}
