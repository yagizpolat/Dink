using System.Collections;
using UnityEngine;

/// <summary>
/// Kapıcı masasındaki giriş-çıkış defteri. Oyuncu deftere bakıp tıklayınca (veya E) defter açılır,
/// adını yazar, KAYDEDİLDİ damgası basılır ve holde kalır. Oyun bu imzayla başlar.
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class LobbyLogbook : MonoBehaviour
{
    [SerializeField] private float reach = 1.8f;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip openClip;

    public static bool Signed { get; private set; }

    private Camera cam;
    private Collider col;
    private bool busy;

    private void Awake()
    {
        col = GetComponent<Collider>();
        Signed = false;
    }

    private void Update()
    {
        if (busy || Signed || Time.timeScale == 0f) return;
        if (!Input.GetMouseButtonDown(0) && !Input.GetKeyDown(KeyCode.E)) return;

        if (cam == null) cam = Camera.main;
        if (cam == null) return;

        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (col.Raycast(ray, out _, reach)) StartCoroutine(Sign());
    }

    private IEnumerator Sign()
    {
        busy = true;
        if (audioSource != null && openClip != null) audioSource.PlayOneShot(openClip);

        PlayerControls.SetEnabled(false);
        float previousScale = Time.timeScale;
        Time.timeScale = 0f;

        yield return NicknameLogbookUI.RunAtDesk(name =>
        {
            if (LeaderboardManager.instance != null) LeaderboardManager.instance.SetPlayerName(name);
        });

        Time.timeScale = previousScale;
        PlayerControls.SetEnabled(true);
        Signed = true;
        busy = false;
    }
}
