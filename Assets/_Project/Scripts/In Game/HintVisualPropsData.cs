using UnityEngine;

/// <summary>
/// 15 Farklı 3D Görsel İpucu Obje ve Materyal Veri Yapısı (Hint Visual Props Data).
/// 15 ipucu tarzına ait 3D prefab'ları, materyalleri ve rotasyon offset'lerini barındırır.
/// </summary>
[CreateAssetMenu(fileName = "HintVisualPropsData", menuName = "Dink/Hint Visual Props Data", order = 2)]
public class HintVisualPropsData : ScriptableObject
{
    [System.Serializable]
    public class HintPropEntry
    {
        public DynamicHintSystem.HintType hintType;
        public string hintName = "İpucu Görseli";
        public GameObject propPrefab;
        public Material propMaterial;
        public Vector3 localPositionOffset = Vector3.zero;
        public Vector3 leftDirectionRotation = new Vector3(0, -45f, 0);
        public Vector3 rightDirectionRotation = new Vector3(0, 45f, 0);
    }

    [Header("15 İpucu Tarzına Özel 3D Görüntü Girişleri")]
    public HintPropEntry[] hintProps = new HintPropEntry[15];

    /// <summary>
    /// Belirtilen ipucu türüne ait görsel giriş verisini döndürür.
    /// </summary>
    public HintPropEntry GetPropEntry(DynamicHintSystem.HintType type)
    {
        if (hintProps == null || hintProps.Length == 0) return null;
        foreach (var entry in hintProps)
        {
            if (entry != null && entry.hintType == type)
            {
                return entry;
            }
        }
        return hintProps[0];
    }
}
