using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Unity Editor içinde tek tıkla 'Hanging_Lamp_Mesh' objesini bulup,
/// zemin seviyesinden tavana bakan ideal cılız loş ışığı (Spotlight)
/// ve materyali otomatik bağlayan ve ayarlayan editor scripti.
/// </summary>
public class SingleLampLightSetup
{
    [MenuItem("Tools/Dink/Lamba Isik Kurulumu/Tekli Lamba Isigini Yapilandir (Hanging_Lamp_Mesh)")]
    public static void SetupSingleLampLight()
    {
        // 1. Sahnede 'Hanging_Lamp_Mesh' objesini ara, yoksa seçili objeyi kullan
        GameObject targetLampObj = GameObject.Find("Hanging_Lamp_Mesh");
        
        if (targetLampObj == null)
        {
            // İsim tam eşleşmediyse hiyerarşide içinde 'Lamp' geçen objelere bak
            GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            foreach (GameObject go in allObjects)
            {
                if (go.name.ToLower().Contains("lamp") || go.name.ToLower().Contains("lamba"))
                {
                    targetLampObj = go;
                    break;
                }
            }
        }

        // Yine bulunamadıysa kullanıcının Hiyerarşide seçtiği objeyi al
        if (targetLampObj == null && Selection.activeGameObject != null)
        {
            targetLampObj = Selection.activeGameObject;
        }

        if (targetLampObj == null)
        {
            EditorUtility.DisplayDialog(
                "Lamba Bulunamadı",
                "Sahnede 'Hanging_Lamp_Mesh' adında bir obje bulunamadı!\n\nLütfen hiyerarşide lamba objeni seçip menüyü tekrar çalıştır.",
                "Tamam"
            );
            return;
        }

        // 2. MATERYAL ATAMASI (MeshRenderer varsa Hanging_Lamp_Material'ı ata)
        MeshRenderer renderer = targetLampObj.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            Material lampMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Hanging_Lamp_Material.mat");
            if (lampMat != null)
            {
                renderer.sharedMaterial = lampMat;
                Debug.Log($"[Dink] '{targetLampObj.name}' objesine Hanging_Lamp_Material başarıyla atandı.");
            }
        }

        // 3. IŞIK BİLEŞENİNİ ÇOCUK OBJE OLARAK BUL VEYA OLUŞTUR
        Transform childLightTransform = targetLampObj.transform.Find("Upward_Lamp_Light");
        GameObject lightObj;

        if (childLightTransform != null)
        {
            lightObj = childLightTransform.gameObject;
        }
        else
        {
            lightObj = new GameObject("Upward_Lamp_Light");
            lightObj.transform.SetParent(targetLampObj.transform, false);
            Undo.RegisterCreatedObjectUndo(lightObj, "Created Upward Lamp Light");
        }

        // Ampulün tam ucu hizasında konum ve sıfırlanmış rotasyon
        lightObj.transform.localPosition = new Vector3(0f, -0.5f, 0f);
        lightObj.transform.localRotation = Quaternion.identity;

        // 4. POINT LIGHT (360 DERECE HER YÖNE YAYILAN GÜÇLÜ VE ATMOSFERİK IŞIK)
        Light lampLight = lightObj.GetComponent<Light>();
        if (lampLight == null)
        {
            lampLight = lightObj.AddComponent<Light>();
        }

        lampLight.type = LightType.Point;
        lampLight.range = 10f;       // Tüm odayı kaplayacak menzil
        lampLight.intensity = 25f;   // URP için seviyeyi aydınlatacak doğru güç
        
        // Sıcak Kehribar (#E6A15C) Rengi
        lampLight.color = new Color(230f / 255f, 161f / 255f, 92f / 255f, 1f);

        // Yumuşak Gölgeler
        lampLight.shadows = LightShadows.Soft;
        lampLight.shadowStrength = 0.85f;
        lampLight.shadowBias = 0.05f;
        lampLight.shadowNormalBias = 0.4f;

        // Sahneyi güncellendi olarak işaretle ve objeyi seç
        EditorSceneManager.MarkSceneDirty(targetLampObj.scene);
        Selection.activeGameObject = lightObj;
        EditorGUIUtility.PingObject(lightObj);

        Debug.Log($"[Dink] '{targetLampObj.name}' için 360° Point Light (Şiddet: 25, Menzil: 10m) başarıyla kuruldu!");
    }
}
