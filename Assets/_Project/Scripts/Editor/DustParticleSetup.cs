using UnityEditor;
using UnityEngine;

/// <summary>
/// Unity Editor içinde tek tıkla sahneye havadaki süzülen toz zerreleri (floating dust motes)
/// sistemini ideal psikolojik korku oyunu ayarlarıyla ekleyen editor scripti.
/// </summary>
public class DustParticleSetup
{
    [MenuItem("Tools/Dink/Atmosfer/Toz Partikul Sistemi Ekle (Floating Dust Motes)")]
    public static void CreateDustParticles()
    {
        // Sahnede önceden oluşturulmuş bir toz objesi var mı kontrol et
        GameObject existingDust = GameObject.Find("Atmospheric_Dust");
        if (existingDust != null)
        {
            Selection.activeGameObject = existingDust;
            EditorGUIUtility.PingObject(existingDust);
            Debug.Log("[Dink] Sahnedeki mevcut 'Atmospheric_Dust' objesi seçildi.");
            return;
        }

        // Yeni toz partikülü objesi oluştur
        GameObject dustObj = new GameObject("Atmospheric_Dust");
        
        // Kamera veya oda merkezine yerleştir
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            dustObj.transform.position = mainCam.transform.position + mainCam.transform.forward * 1.5f;
        }
        else
        {
            dustObj.transform.position = new Vector3(0, 1.5f, 0);
        }

        ParticleSystem ps = dustObj.AddComponent<ParticleSystem>();
        ParticleSystemRenderer psRenderer = dustObj.GetComponent<ParticleSystemRenderer>();

        // 1. ANA AYARLAR (Main Module)
        var main = ps.main;
        main.duration = 5f;
        main.loop = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(4f, 9f); // Tozlar uzun süre süzülsün
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.01f, 0.04f); // Çok yavaş hareket
        main.startSize = new ParticleSystem.MinMaxCurve(0.008f, 0.022f); // İnce mikro toz taneleri
        main.startColor = new Color(1f, 0.95f, 0.85f, 0.40f); // Hafif sıcak saydam beyaz
        main.simulationSpace = ParticleSystemSimulationSpace.World; // Dünya uzayında bağımsız süzülme
        main.maxParticles = 80;

        // 2. YAYILMA (Emission Module)
        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = 12f; // Kalabalık olmadan zarif toz ortamı

        // 3. ŞEKİL (Shape Module - Odayı kaplayacak kutu alan)
        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(4f, 3f, 4f); // Kamera görüş alanını kaplayan hacim

        // 4. HIZ VE HAREKET (Velocity over Lifetime)
        var velocity = ps.velocityOverLifetime;
        velocity.enabled = true;
        velocity.x = new ParticleSystem.MinMaxCurve(-0.015f, 0.015f);
        velocity.y = new ParticleSystem.MinMaxCurve(-0.008f, 0.025f); // Yavaşça yukarı süzülme
        velocity.z = new ParticleSystem.MinMaxCurve(-0.015f, 0.015f);

        // 5. ŞEFFAFLIK GECİŞİ (Color over Lifetime - Fade in / Fade out)
        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;

        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(new Color(1f, 0.95f, 0.85f), 0.0f),
                new GradientColorKey(new Color(1f, 0.95f, 0.85f), 1.0f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(0.0f, 0.0f),   // Doğarken tamamen saydam
                new GradientAlphaKey(0.45f, 0.25f), // %25 sürede belirginleş
                new GradientAlphaKey(0.45f, 0.75f), // %75 süreye kadar kal
                new GradientAlphaKey(0.0f, 1.0f)    // Ölürken yumuşakça kaybol
            }
        );
        colorOverLifetime.color = gradient;

        // 6. RÜZGAR VE ÇALKANTI (Noise Module - Hava akımı simülasyonu)
        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = 0.025f;  // Hafif salınım
        noise.frequency = 0.4f;   // Yumuşak türbülans
        noise.scrollSpeed = 0.15f;
        noise.damping = true;

        // 7. RENDERER (Default Particle Material)
        if (psRenderer != null)
        {
            psRenderer.renderMode = ParticleSystemRenderMode.Billboard;
            
            // Unity varsayılan sprite/particle materyalini bul ve ata
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

        // Hiyerarşide seçili hale getir
        Selection.activeGameObject = dustObj;
        Undo.RegisterCreatedObjectUndo(dustObj, "Created Atmospheric Dust");

        Debug.Log("[Dink] 'Atmospheric_Dust' partikül sistemi sahneye eklendi ve ayarlandı!");
    }
}
