using UnityEngine;

/// <summary>
/// Paslı İpucu Masası (Hint Table).
/// İpucu objesinin altına zemine oturan eski bir masa ve üstüne yukarıdan vuran loş, titrek bir spot ışığı kurar.
/// Model dosyası gerektirmez; masa primitive parçalardan çalışma anında oluşturulur.
/// </summary>
public class HintTable : MonoBehaviour
{
    private const float TableHeight = 0.78f;
    private const float TopWidth = 0.9f;
    private const float TopDepth = 0.6f;
    private const float TopThickness = 0.05f;
    private const float LegSize = 0.05f;

    private static readonly Color woodColor = new Color(0.22f, 0.15f, 0.10f);
    private static readonly Color rustColor = new Color(0.30f, 0.16f, 0.08f);
    private static readonly Color lightColor = new Color(1f, 0.82f, 0.58f);

    private Light spot;
    private float baseIntensity;
    private float flickerSeed;

    /// <summary>
    /// Verilen noktanın altındaki zemine masa kurar ve objenin konacağı masa üstü noktasını döndürür.
    /// </summary>
    public static HintTable Create(Vector3 aroundPosition, Quaternion facing, out Vector3 tableTopPoint)
    {
        Vector3 floor = FindFloor(aroundPosition);

        GameObject root = new GameObject("HintTable");
        root.transform.SetPositionAndRotation(floor, Quaternion.Euler(0f, facing.eulerAngles.y, 0f));
        HintTable table = root.AddComponent<HintTable>();

        Material wood = CreateMaterial(woodColor, 0.15f);
        Material rust = CreateMaterial(rustColor, 0.35f);

        // Masa üstü (tıklamayı ipucu objesine bırakmak için collider'ı kaldırılmaz; obje masanın üstünde durur)
        CreatePart(root.transform, "Top", new Vector3(0f, TableHeight - TopThickness / 2f, 0f),
            new Vector3(TopWidth, TopThickness, TopDepth), wood, Quaternion.identity);

        // Hafif yamuk paslı metal ayaklar
        float lx = TopWidth / 2f - LegSize;
        float lz = TopDepth / 2f - LegSize;
        float legHeight = TableHeight - TopThickness;
        Vector3[] legPositions =
        {
            new Vector3(-lx, legHeight / 2f, -lz),
            new Vector3(lx, legHeight / 2f, -lz),
            new Vector3(-lx, legHeight / 2f, lz),
            new Vector3(lx, legHeight / 2f, lz)
        };
        for (int i = 0; i < legPositions.Length; i++)
        {
            Quaternion tilt = Quaternion.Euler(Random.Range(-1.5f, 1.5f), 0f, Random.Range(-1.5f, 1.5f));
            CreatePart(root.transform, $"Leg{i}", legPositions[i], new Vector3(LegSize, legHeight, LegSize), rust, tilt);
        }

        // Ayakları birbirine bağlayan alt çıta
        CreatePart(root.transform, "Brace", new Vector3(0f, 0.18f, lz),
            new Vector3(TopWidth - LegSize * 2f, 0.04f, 0.03f), rust, Quaternion.identity);

        table.CreateSpotLight();

        tableTopPoint = floor + Vector3.up * (TableHeight + 0.02f);
        return table;
    }

    private static Vector3 FindFloor(Vector3 around)
    {
        // Objenin yukarısından aşağı ışın at; zemin bulunamazsa göz hizasından masa yüksekliği kadar aşağıyı kullan
        if (Physics.Raycast(around + Vector3.up * 0.5f, Vector3.down, out RaycastHit hit, 5f, ~0, QueryTriggerInteraction.Ignore))
        {
            return hit.point;
        }
        return around - Vector3.up * TableHeight;
    }

    private static void CreatePart(Transform parent, string partName, Vector3 localPos, Vector3 size, Material mat, Quaternion localRot)
    {
        GameObject part = GameObject.CreatePrimitive(PrimitiveType.Cube);
        part.name = partName;
        part.transform.SetParent(parent, false);
        part.transform.localPosition = localPos;
        part.transform.localRotation = localRot;
        part.transform.localScale = size;
        part.GetComponent<Renderer>().sharedMaterial = mat;
    }

    private static Material CreateMaterial(Color color, float smoothness)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");

        Material mat = new Material(shader);
        mat.SetColor("_BaseColor", color);
        mat.color = color;
        mat.SetFloat("_Smoothness", smoothness);
        mat.SetFloat("_Glossiness", smoothness);
        return mat;
    }

    private void CreateSpotLight()
    {
        GameObject lightObj = new GameObject("TableSpotLight");
        lightObj.transform.SetParent(transform, false);
        lightObj.transform.localPosition = new Vector3(0f, TableHeight + 1.6f, 0f);
        lightObj.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

        spot = lightObj.AddComponent<Light>();
        spot.type = LightType.Spot;
        spot.color = lightColor;
        spot.range = 3.5f;
        spot.spotAngle = 55f;
        spot.innerSpotAngle = 25f;
        spot.intensity = 2.2f;
        spot.shadows = LightShadows.Soft;

        baseIntensity = spot.intensity;
        flickerSeed = Random.value * 100f;
    }

    private void Update()
    {
        if (spot == null) return;

        // Eski ampul gibi yumuşak dalgalanma, arada bir kısa sönme
        float noise = Mathf.PerlinNoise(flickerSeed, Time.time * 3f);
        float dip = Mathf.PerlinNoise(flickerSeed + 50f, Time.time * 0.7f) > 0.82f ? 0.25f : 1f;
        spot.intensity = baseIntensity * Mathf.Lerp(0.75f, 1.05f, noise) * dip;
    }
}
