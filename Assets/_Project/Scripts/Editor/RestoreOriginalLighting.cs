using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class RestoreOriginalLighting
{
    [MenuItem("Tools/Dink/Atmosfer/Eski Aydinlatmaya Geri Don (Restore Original)")]
    public static void Restore()
    {
        // 1. Ortam ışığını orijinal siyah haline getir
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = Color.black;
        RenderSettings.ambientIntensity = 0f;

        // 2. Oluşturulan Global_Horror_Volume objesini sahneden sil
        GameObject volObj = GameObject.Find("Global_Horror_Volume");
        if (volObj != null)
        {
            Object.DestroyImmediate(volObj);
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("[Dink] Aydınlatma tamamen orijinal haline döndürüldü.");
    }
}
