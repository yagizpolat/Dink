using System.Collections;
using UnityEngine;

/// <summary>Oyuncu kapının önüne yürüyünce kanat itilir gibi açılır ve öyle kalır (kol/E yok).
/// Kanat menteşe kökünde döner; açılırken çarpışması kapanır.</summary>
public class ItmeKapi : MonoBehaviour
{
    [SerializeField] private Transform leaf;
    [SerializeField] private Transform player;
    [SerializeField] private Vector3 pushPoint;
    [SerializeField] private float pushRadius = 0.9f;
    [SerializeField] private float openAngle = -85f;
    [SerializeField] private float seconds = 0.9f;
    [SerializeField] private AudioClip openSound;
    [SerializeField] private bool startLocked;
    [SerializeField] private Light revealLight;               // kilit açılınca yanar (kapı ardındaki sızıntı)
    [SerializeField] private Light revealLight2;              // aynı anda yanan ikinci ışık (oda dolgusu)

    private bool opened, locked;
    private Quaternion closedRot;
    private bool closedSet;

    private void Awake()
    {
        if (leaf != null) { closedRot = leaf.localRotation; closedSet = true; }
        locked = startLocked;
        if (revealLight != null) revealLight.enabled = !startLocked;
        if (revealLight2 != null) revealLight2.enabled = !startLocked;
    }

    /// <summary>Kilidi açar; ardındaki ışık yanar, kapı artık itilebilir.</summary>
    public void Unlock()
    {
        locked = false;
        if (revealLight != null) revealLight.enabled = true;
        if (revealLight2 != null) revealLight2.enabled = true;
    }

    /// <summary>Kapıyı bir anda kapatır ve kilitler (bir daha itilmez).</summary>
    public void SlamShut()
    {
        locked = true; opened = true;
        StopAllCoroutines();
        if (leaf == null || !closedSet) return;
        StartCoroutine(Shut());
    }

    private IEnumerator Shut()
    {
        var from = leaf.localRotation;
        for (float t = 0f; t < 1f; t += Time.deltaTime / 0.22f)
        {
            leaf.localRotation = Quaternion.Slerp(from, closedRot, t * t);
            yield return null;
        }
        leaf.localRotation = closedRot;
        foreach (var c in leaf.GetComponentsInChildren<Collider>()) c.enabled = true;
    }

    private void Update()
    {
        if (opened || locked || player == null || leaf == null) return;
        var d = player.position - pushPoint; d.y = 0f;
        if (d.magnitude > pushRadius) return;
        opened = true;
        foreach (var c in leaf.GetComponentsInChildren<Collider>()) c.enabled = false;
        if (openSound != null) AudioSource.PlayClipAtPoint(openSound, leaf.position, 0.5f);
        StartCoroutine(Open());
    }

    private IEnumerator Open()
    {
        var from = leaf.localRotation; var to = from * Quaternion.Euler(0f, openAngle, 0f);
        for (float t = 0f; t < 1f; t += Time.deltaTime / seconds)
        {
            leaf.localRotation = Quaternion.Slerp(from, to, 1f - Mathf.Pow(1f - t, 2f));
            yield return null;
        }
        leaf.localRotation = to;
    }
}
