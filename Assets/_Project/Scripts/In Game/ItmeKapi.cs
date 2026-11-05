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

    private bool opened;

    private void Update()
    {
        if (opened || player == null || leaf == null) return;
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
