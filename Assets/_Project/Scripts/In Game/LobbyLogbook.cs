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

    [Header("3D Defter Sayfası")]
    [SerializeField] private Material pageMaterial;

    public static bool Signed { get; private set; }

    // Domain reload kapalıyken editörde önceki Play'in imzası taşınmasın
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        Signed = false;
        SignedName = "";
    }


    /// <summary>Defterde imzalanan ad; üst katlardaki fiş gibi yerler bunu kullanır.</summary>
    public static string SignedName { get; private set; } = "";

    /// <summary>Devam Et: kayıttaki adla imza atılmış sayılır (asansör vb. imza ister).</summary>
    public static void RestoreSignature(string signedName)
    {
        if (string.IsNullOrWhiteSpace(signedName)) return;
        Signed = true;
        SignedName = signedName;
        if (LeaderboardManager.instance != null) LeaderboardManager.instance.SetPlayerName(signedName);
    }

    private Camera cam;
    private Collider col;
    private bool busy;
    private RenderTexture pageTexture;

    private void Awake()
    {
        col = GetComponent<Collider>();
        Signed = false;
        SignedName = "";
    }

    private void Start()
    {
        // Masadaki defter, ekranda açılan sayfanın aynısını gösterir
        if (pageMaterial == null) return;
        pageTexture = new RenderTexture(2048, 1152, 24) { name = "DefterSayfasi", anisoLevel = 4 };
        pageMaterial.SetTexture("_BaseMap", pageTexture);
        Vector4 st = NicknameLogbookUI.PageRectST;
        pageMaterial.SetTextureScale("_BaseMap", new Vector2(st.x, st.y));
        pageMaterial.SetTextureOffset("_BaseMap", new Vector2(st.z, st.w));
        NicknameLogbookUI.RenderPageTexture("", pageTexture);
    }

    private void OnDestroy()
    {
        if (pageMaterial != null) pageMaterial.SetTexture("_BaseMap", null);
        if (pageTexture != null)
        {
            pageTexture.Release();
            Destroy(pageTexture);
        }
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

        PauseService.Pause(this);

        string signedName = "";
        yield return NicknameLogbookUI.RunAtDesk(name =>
        {
            signedName = name;
            if (LeaderboardManager.instance != null) LeaderboardManager.instance.SetPlayerName(name);
        });

        // İmza masadaki deftere de geçer
        if (pageTexture != null) NicknameLogbookUI.RenderPageTexture(signedName, pageTexture);

        PauseService.Resume(this);
        Signed = true;
        SignedName = signedName;
        busy = false;
    }
}
