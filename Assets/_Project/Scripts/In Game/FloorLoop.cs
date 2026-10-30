using UnityEngine;

/// <summary>
/// Kat döngüsü (Exit 8 tarzı): L kolunun ucundaki köşe geçidi iki kez sağa döner; lobinin batısında birebir aynı
/// bir geçit daha var. Görev bittiyse oyuncu giriş geçidinin iki dönemeci arasındaki çizgiyi geçtiği an, iki ağız da
/// görünmezken çıkış geçidinde aynı noktaya taşınır ve kat yenilenir (levha FLOOR n, nesneler yerinde, görev baştan).
/// Görev bitmeden giriş geçidi çıkmaz sokaktır.
/// </summary>
public class FloorLoop : MonoBehaviour
{
    [SerializeField] private FocusAnomaly anomaly;
    [SerializeField] private Transform entryGate;         // giriş geçidi ağzı: ileri (z) = içeri, sağ (x) = dönüş yönü
    [SerializeField] private Transform exitGate;          // çıkış geçidinin aynı noktası
    [SerializeField] private float crossRight = 1.4f;     // iki dönemecin ortası (yerel x)
    [SerializeField] private float minForward = 2.15f;    // dönemeçler arası bölüm başlangıcı (yerel z)
    [SerializeField] private GameObject[] targets;        // kat yenilenince yerine dönen nesneler
    [SerializeField] private Light gateLight;             // görev bitince geçidin ağzında yanan ışık
    [SerializeField] private Renderer koridor;            // kat levhasını taşıyan model
    [SerializeField] private Texture2D[] signs;           // FLOOR 1, FLOOR 2, ...
    [SerializeField] private string signMaterialPrefix = "M_K_KatLevha";

    private bool open;
    private int floorNumber = 1;
    private float lastRight = float.NegativeInfinity;
    private Transform player;
    private static readonly int BaseMap = Shader.PropertyToID("_BaseMap");

    private void Awake()
    {
        var walker = FindFirstObjectByType<PlayerWalker>();
        if (walker != null) player = walker.transform;
        if (gateLight != null) gateLight.enabled = false;
        SetSign(floorNumber);
    }

    /// <summary>Görev bitti (FocusAnomaly.onFinished): geçit bir sonraki kata açılır.</summary>
    public void Open()
    {
        open = true;
        if (gateLight != null) gateLight.enabled = true;
    }

    private void LateUpdate()
    {
        if (player == null) return;
        Vector3 local = entryGate.InverseTransformPoint(player.position);
        bool between = local.z > minForward && local.z < minForward + 1.3f && local.x > -0.6f && local.x < 3.4f;
        float right = between ? local.x : float.NegativeInfinity;

        // Yalnız ileri yönde (sağa doğru) çizgiyi geçerken
        if (open && between && lastRight < crossRight && right >= crossRight) Pass(local);
        else lastRight = right;
    }

    private void Pass(Vector3 local)
    {
        var cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        Quaternion turn = exitGate.rotation * Quaternion.Inverse(entryGate.rotation);
        player.SetPositionAndRotation(exitGate.TransformPoint(local), turn * player.rotation);
        if (cc != null) cc.enabled = true;
        lastRight = float.NegativeInfinity;

        // Yeni kat: aynı koridor yerinde yenilenir (birikme yok)
        open = false;
        if (gateLight != null) gateLight.enabled = false;
        foreach (var t in targets) t.SetActive(true);
        floorNumber++;
        SetSign(floorNumber);
        anomaly.Restart(targets);
    }

    private void SetSign(int n)
    {
        if (signs == null || signs.Length == 0 || koridor == null) return;
        Texture2D tex = signs[Mathf.Clamp(n - 1, 0, signs.Length - 1)];
        foreach (var m in koridor.materials)
            if (m.name.StartsWith(signMaterialPrefix)) m.SetTexture(BaseMap, tex);
    }
}
