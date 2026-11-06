using UnityEngine;

/// <summary>
/// Hafıza marketi P3: raf dolduktan sonra marketten kaçılamaz. Oyuncu karanlıkta marketten
/// belli bir uzaklığa varınca, yürüdüğü yöne bakarken marketin önüne taşınır:
/// hangi yöne giderse gitsin market yine önünde, aynı uzaklıkta belirir. Zemin düz ve
/// dokusuz siyah olduğundan taşınma yalnız marketin önde yeniden görünmesiyle fark edilir.
/// </summary>
public class MarketNoExit : MonoBehaviour
{
    [SerializeField] private MarketShelfStocking stocking;
    [SerializeField] private Transform player;
    [SerializeField] private Vector3 marketCenter = new Vector3(3f, 0f, 2f);
    [SerializeField] private float escapeRadius = 22f;
    [SerializeField] private float freeRadius = 150f;

    public int Returns { get; private set; }

    private bool active;

    private void OnEnable()
    {
        if (stocking != null) stocking.OnCompleted += Arm;
    }

    private void OnDisable()
    {
        if (stocking != null) stocking.OnCompleted -= Arm;
    }

    private void Arm() => active = true;

    private void LateUpdate()
    {
        if (player == null) return;
        Vector3 away = player.position - marketCenter; away.y = 0f;
        float r = away.magnitude;
        // raf dolmadan boşluk serbest, ama zeminin kenarına (300 m) varıp düşmeden geri gelir
        if (r < (active ? escapeRadius : freeRadius)) return;

        // Market yalnız önden (ışıklı vitrin) görünür: oyuncu hep ön cepheye konur ve bakışı,
        // kaçtığı yön marketin içine bakacak biçimde döndürülür (yürüdüğü yöne göre bakış farkı korunur)
        Vector3 target = marketCenter + Vector3.back * (escapeRadius - 1.5f);
        target.y = player.position.y;
        float turn = Vector3.SignedAngle(away / r, Vector3.forward, Vector3.up);
        var cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        player.SetPositionAndRotation(target, Quaternion.Euler(0f, turn, 0f) * player.rotation);
        if (cc != null) cc.enabled = true;
        Physics.SyncTransforms();
        Returns++;
    }
}
