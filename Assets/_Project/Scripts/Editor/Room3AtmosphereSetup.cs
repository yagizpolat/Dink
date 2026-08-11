using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Seviye 3 için tek tıkla sahnede havadaki süzülen ışıklı sis zerrelerini (Volumetric Fog Motes)
/// ve lambaya organik titreme/cızırtı sistemini (FlickerLight) ekleyen editor scripti.
/// </summary>
public class Room3AtmosphereSetup
{
    [MenuItem("Tools/Dink/Atmosfer/Seviye 3 Sis ve Titreyen Isik Sistemini Ekle")]
    public static void CreateRoom3Atmosphere()
    {
        // 1. LAMBAYI VE IŞIĞI BUL
        GameObject lampLightObj = GameObject.Find("Upward_Lamp_Light");
        if (lampLightObj == null)
        {
            // Sahnede Light bileşeni olan objeleri ara
            Light[] allLights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
            foreach (Light l in allLights)
            {
                if (l.type == LightType.Point && l.gameObject.name != "Directional Light")
                {
                    lampLightObj = l.gameObject;
                    break;
                }
            }
        }

        // 2. FlickerLight kaldırıldı (script silindi). Gerekirse yeniden oluşturulacak.

        // 3. HAVADAKİ SÜZÜLEN IŞIKLI SİS ZERRELERİ (VOLUMETRIC FOG MOTES)
        GameObject fogObj = GameObject.Find("Atmospheric_Fog_Motes");
        if (fogObj == null)
        {
            fogObj = new GameObject("Atmospheric_Fog_Motes");
            Undo.RegisterCreatedObjectUndo(fogObj, "Created Atmospheric Fog Motes");
        }

        // Konumlandırma (Lambanın etrafını ve odayı kaplayacak konum)
        if (lampLightObj != null)
        {
            fogObj.transform.position = lampLightObj.transform.position + new Vector3(0, 0.5f, 0);
        }
        else
        {
            fogObj.transform.position = new Vector3(0, 1.5f, 0);
        }

        ParticleSystem ps = fogObj.GetComponent<ParticleSystem>();
        if (ps == null)
        {
            ps = fogObj.AddComponent<ParticleSystem>();
        }

        ParticleSystemRenderer psRenderer = fogObj.GetComponent<ParticleSystemRenderer>();

        // PARTİKÜL SİSTEMİ AYARLARI (SİS VE HAREKET)
        var main = ps.main;
        main.duration = 6f;
        main.loop = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(4f, 8f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.01f, 0.05f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.04f, 0.12f); // Havadaki sis katmanları
        main.startColor = new Color(1f, 0.90f, 0.70f, 0.25f); // Soluk sıcak ışıklı sis
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 120;

        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = 18f;

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(6f, 3.5f, 5f); // Odadaki tüm hacmi kaplasın

        var velocity = ps.velocityOverLifetime;
        velocity.enabled = true;
        velocity.x = new ParticleSystem.MinMaxCurve(-0.02f, 0.02f);
        velocity.y = new ParticleSystem.MinMaxCurve(0.005f, 0.03f); // Yavaşça yukarı süzülen sis
        velocity.z = new ParticleSystem.MinMaxCurve(-0.02f, 0.02f);

        // Renk ve Şeffaflık Yumuşatması (Fade In / Fade Out)
        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(new Color(1f, 0.88f, 0.68f), 0.0f),
                new GradientColorKey(new Color(1f, 0.88f, 0.68f), 1.0f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(0.0f, 0.0f),
                new GradientAlphaKey(0.35f, 0.3f),
                new GradientAlphaKey(0.35f, 0.7f),
                new GradientAlphaKey(0.0f, 1.0f)
            }
        );
        colorOverLifetime.color = gradient;

        // Hava Salınımı (Noise)
        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = 0.03f;
        noise.frequency = 0.35f;
        noise.scrollSpeed = 0.12f;

        // Renderer Materyali
        if (psRenderer != null)
        {
            psRenderer.renderMode = ParticleSystemRenderMode.Billboard;
            Material defaultMat = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Particle.mat");
            if (defaultMat == null)
            {
                defaultMat = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Material.mat");
            }
            if (defaultMat != null)
            {
                psRenderer.sharedMaterial = defaultMat;
            }
        }

        EditorSceneManager.MarkSceneDirty(fogObj.scene);
        Selection.activeGameObject = fogObj;
        EditorGUIUtility.PingObject(fogObj);

        Debug.Log("[Dink] Seviye 3 için havadaki ışıklı sis zerreleri (Atmospheric_Fog_Motes) ve titreyen ışık sistemi başarıyla kuruldu!");
    }
}
