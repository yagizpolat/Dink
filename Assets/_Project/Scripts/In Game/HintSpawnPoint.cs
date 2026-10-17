using UnityEngine;

/// <summary>
/// İpucu Doğma Noktası (Hint Spawn Point).
/// Odaya boş bir GameObject koyup bu bileşeni ekle; DynamicHintSystem ipucu objesini bu noktalardan
/// rastgele birinde doğurur. Hiç nokta yoksa obje oyuncunun önünde doğar ve altına masa kurulur.
/// Masa, raf gibi bir yüzeyin üstüne koyarsan masa kurulmaz.
/// </summary>
public class HintSpawnPoint : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.82f, 0.44f, 0.23f, 0.8f);
        Gizmos.DrawWireSphere(transform.position, 0.15f);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 0.4f);
    }
}
