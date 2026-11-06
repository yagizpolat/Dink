using System.Collections;
using UnityEngine;

/// <summary>
/// Giriş katının ana kapısı. Oyuncu içeri girip birkaç adım atınca kanatlar arkasından kendiliğinden kapanır.
/// Kapandıktan sonra camlar dışarıyı değil, aynı holün yansımasını gösterir (GlassReflection).
/// Kapıya geri dönen oyuncu kapıyı açamaz.
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class EntranceDoorCloser : MonoBehaviour
{
    [SerializeField] private Transform leftLeaf;
    [SerializeField] private Transform rightLeaf;
    [SerializeField] private float openAngle = 78f;

    [Header("Kapanma")]
    [SerializeField] private float closeDelay = 0.6f;
    [SerializeField] private float closeSeconds = 1.6f;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip creakClip;
    [SerializeField] private AudioClip slamClip;

    [Header("Kapandıktan Sonra")]
    [SerializeField] private GlassReflection reflection;
    [SerializeField] private Transform player;
    [SerializeField] private float wontBudgeDistance = 1.1f;
    [SerializeField] private AudioClip[] wontBudgeClips;

    private Quaternion leftClosed, rightClosed;
    private bool closing, closed;
    private float nextBudge;

    private void Reset()
    {
        GetComponent<BoxCollider>().isTrigger = true;
    }

    private void Awake()
    {
        leftClosed = leftLeaf.localRotation;
        rightClosed = rightLeaf.localRotation;
        OpenInward(leftLeaf);
        OpenInward(rightLeaf);
        if (reflection != null) reflection.enabled = false;
    }

    // Kanadı menteşesi etrafında, holün içine (bu nesnenin ileri yönüne) doğru açık bırakır
    private void OpenInward(Transform leaf)
    {
        Quaternion closedWorld = leaf.rotation;
        Vector3 center = leaf.GetComponent<Renderer>().bounds.center;
        float best = openAngle;
        float bestScore = float.MinValue;
        foreach (float a in new[] { openAngle, -openAngle })
        {
            Vector3 moved = leaf.position + Quaternion.AngleAxis(a, Vector3.up) * (center - leaf.position);
            float score = Vector3.Dot(moved - transform.position, transform.forward);
            if (score > bestScore) { bestScore = score; best = a; }
        }
        leaf.rotation = Quaternion.AngleAxis(best, Vector3.up) * closedWorld;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (closing || other.GetComponent<PlayerWalker>() == null) return;
        closing = true;
        StartCoroutine(Close());
    }

    private IEnumerator Close()
    {
        yield return new WaitForSeconds(closeDelay);
        if (audioSource != null && creakClip != null) audioSource.PlayOneShot(creakClip, 0.6f);

        Quaternion leftOpen = leftLeaf.localRotation, rightOpen = rightLeaf.localRotation;
        for (float t = 0f; t < closeSeconds; t += Time.deltaTime)
        {
            // Yavaş başlar, sonda hızlanır: biri itmiş gibi
            float k = Mathf.Pow(t / closeSeconds, 2.2f);
            leftLeaf.localRotation = Quaternion.Slerp(leftOpen, leftClosed, k);
            rightLeaf.localRotation = Quaternion.Slerp(rightOpen, rightClosed, k);
            yield return null;
        }
        leftLeaf.localRotation = leftClosed;
        rightLeaf.localRotation = rightClosed;

        if (audioSource != null && slamClip != null) audioSource.PlayOneShot(slamClip);
        if (reflection != null) reflection.enabled = true;
        closed = true;
    }

    private void Update()
    {
        if (!closed || player == null || wontBudgeClips == null || wontBudgeClips.Length == 0) return;
        if (Time.time < nextBudge) return;

        Vector3 d = player.position - transform.position;
        d.y = 0f;
        if (d.magnitude > wontBudgeDistance) return;

        // Oyuncu kapıya dayandı: kol oynar ama kapı açılmaz
        audioSource.PlayOneShot(wontBudgeClips[Random.Range(0, wontBudgeClips.Length)]);
        nextBudge = Time.time + 5f;
    }
}
