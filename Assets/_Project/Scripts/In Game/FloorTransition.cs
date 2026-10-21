using UnityEngine;

/// <summary>
/// Giriş katından üst kata geçiş: asansör çağırma butonu (tıkla / E) veya merdivenin tepesi (içinden yürü).
/// Oyuncu kapıcı masasındaki defteri imzalamadan ikisi de çalışmaz; merdivenin dibindeki engel de imzaya kadar kapalı kalır.
/// </summary>
[RequireComponent(typeof(Collider))]
public class FloorTransition : MonoBehaviour
{
    private enum Kind { ElevatorButton, StairsTop }

    [SerializeField] private Kind kind = Kind.ElevatorButton;
    [SerializeField] private string targetScene = "Kat1";
    [SerializeField] private float fadeSeconds = 1.4f;
    [SerializeField] private float reach = 1.8f;

    [Header("Merdiven")]
    [SerializeField] private GameObject unsignedBlocker;

    [Header("Ses")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip acceptClip;
    [SerializeField] private AudioClip deniedClip;

    private Collider col;
    private Camera cam;
    private bool used;
    private float nextDenied;

    private void Awake()
    {
        col = GetComponent<Collider>();
    }

    private void Update()
    {
        if (unsignedBlocker != null && unsignedBlocker.activeSelf == LobbyLogbook.Signed)
            unsignedBlocker.SetActive(!LobbyLogbook.Signed);

        if (kind != Kind.ElevatorButton || used || Time.timeScale == 0f) return;
        if (!Input.GetMouseButtonDown(0) && !Input.GetKeyDown(KeyCode.E)) return;

        if (cam == null) cam = Camera.main;
        if (cam == null) return;
        if (col.Raycast(cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f)), out _, reach)) TryGo();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (kind == Kind.StairsTop && other.GetComponent<PlayerWalker>() != null) TryGo();
    }

    private void TryGo()
    {
        if (used) return;

        if (!LobbyLogbook.Signed)
        {
            // İmza yok: buton tık eder ama yanmaz
            if (audioSource != null && deniedClip != null && Time.time >= nextDenied)
            {
                audioSource.PlayOneShot(deniedClip);
                nextDenied = Time.time + 0.6f;
            }
            return;
        }

        used = true;
        if (audioSource != null && acceptClip != null) audioSource.PlayOneShot(acceptClip);
        StartCoroutine(SceneDoorTrigger.FadeAndLoad(targetScene, fadeSeconds));
    }
}
