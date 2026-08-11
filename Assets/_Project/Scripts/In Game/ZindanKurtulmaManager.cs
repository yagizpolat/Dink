using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Oyuncunun kaybetme durumunu, zindan geçişini ve kurtulma haklarını yönetir.
/// Sahneler arası taşınır (DontDestroyOnLoad). Oyun boyunca tek instance çalışır.
/// </summary>
public class ZindanKurtulmaManager : MonoBehaviour
{
    public static ZindanKurtulmaManager instance { get; private set; }

    [Header("Zindan Ayarları")]
    [Tooltip("Zindan sahnesinin Build Settings'teki index numarası.")]
    [SerializeField] private int zindanSahnesiIndex = 6;

    [Tooltip("Oyun başına verilen toplam kurtulma hakkı.")]
    [SerializeField] private int maksimumKurtulmaHakki = 2;

    // Kalan hak sayısı — dışarıdan sadece okunabilir
    public int KalanHak => kalanHak;
    private int kalanHak;

    // Oyuncunun kaybettiği sahne index'i — zindandan çıkınca buraya dönecek
    private int kayipSahneIndex = -1;

    // Zindan mücadelesi aktif mi kontrolü
    public bool ZindandaMi { get; private set; }

    /// <summary>
    /// Oyun ilk açıldığında otomatik olarak instance oluşturur.
    /// Sahnede elle eklemeye gerek kalmaz.
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void OtomatikOlustur()
    {
        if (instance != null) return;

        GameObject go = new GameObject("[ZindanKurtulmaManager]");
        go.AddComponent<ZindanKurtulmaManager>();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Ana Menüye (Build Index 0) dönüldüğünde kurtulma haklarını otomatik sıfırla
        if (scene.buildIndex == 0)
        {
            HaklariSifirla();
        }
    }

    private void Awake()
    {
        // Singleton: Zaten varsa kopyayı yok et
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        kalanHak = maksimumKurtulmaHakki;
    }

    /// <summary>
    /// Oyuncunun yanlış kapı seçmesi veya süre bitmesi durumunda çağrılır.
    /// Kurtulma hakkı varsa zindana gönderir, yoksa kalıcı oyun bitişi tetikler.
    /// </summary>
    /// <param name="mevcutSahneIndex">Oyuncunun şu an bulunduğu sahne index'i.</param>
    /// <returns>true: zindana gidildi, false: hak kalmadı (kalıcı oyun bitti).</returns>
    public bool KaybetmeyiIsle(int mevcutSahneIndex)
    {
        if (kalanHak <= 0)
        {
            Debug.Log($"[Zindan] Kurtulma hakkı kalmadı ({kalanHak}). Kalıcı oyun bitti.");
            return false;
        }

        kalanHak--;
        kayipSahneIndex = mevcutSahneIndex;
        ZindandaMi = true;

        Debug.Log($"[Zindan] Oyuncu zindana düşüyor. Kalan hak: {kalanHak}, Dönüş sahnesi: {kayipSahneIndex}");

        SceneManager.LoadScene(zindanSahnesiIndex);
        return true;
    }

    /// <summary>
    /// Zindan içindeki kurtulma mücadelesi başarılı olduğunda çağrılır.
    /// Oyuncuyu kaybettiği odaya geri götürür.
    /// </summary>
    public void ZindandanKurtul()
    {
        if (kayipSahneIndex < 0)
        {
            Debug.LogError("[Zindan] Dönüş sahnesi kayıtlı değil!");
            return;
        }

        ZindandaMi = false;
        int donusSahnesi = kayipSahneIndex;
        kayipSahneIndex = -1;

        Debug.Log($"[Zindan] Oyuncu kurtuldu! Sahne {donusSahnesi}'e dönülüyor. Kalan hak: {kalanHak}");

        SceneManager.LoadScene(donusSahnesi);
    }

    /// <summary>
    /// Yeni oyun başlatıldığında hakları sıfırlamak için çağrılır.
    /// (Örn: Ana Menüden yeni oyun başlatıldığında)
    /// </summary>
    public void HaklariSifirla()
    {
        kalanHak = maksimumKurtulmaHakki;
        kayipSahneIndex = -1;
        ZindandaMi = false;
        Debug.Log($"[Zindan] Haklar sıfırlandı: {kalanHak}");
    }
}
