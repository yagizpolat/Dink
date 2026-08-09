using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class Kamera : MonoBehaviour
{
    public GameObject cam;
    int hassasiyet = 3;
    float toplamrotasyonX = 0f;
    float toplamrotasyonY = 0f;
    public float fareX;
    public float fareY;
    public quaternion rotasyon;
    public float sinirlar = 30f;

    [Header("Idle Breathing (Nefes Alma)")]
    [SerializeField] private bool nefesAlmaAktif = true;
    [SerializeField] private float nefesHizi = 1.5f;
    [SerializeField] private float nefesMiktariX = 0.03f;
    [SerializeField] private float nefesMiktariY = 0.015f;
    private Vector3 baslangicPozisyonu;
    private float nefesZamani = 0f;

    [Header("Fener Gecikmesi (Sway/Lag)")]
    [SerializeField] private Transform fenerTransform;
    [SerializeField] private float fenerSwayHizi = 8f;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        if (cam != null)
        {
            baslangicPozisyonu = cam.transform.localPosition;
        }
    }

    void Update()
    {
        // 1. ADIM: Önce fare girdilerini güncel olarak alıyoruz.
        fareX = Input.GetAxis("Mouse Y");
        fareY = Input.GetAxis("Mouse X");

        // Hesaplayacağımız ham hareket miktarını önden belirleyelim
        float hareketX = fareX * -hassasiyet;
        float hareketY = fareY * hassasiyet;

        // 2. ADIM: Sınır kontrolü ve "Koşullu Toplama"
        if ((toplamrotasyonX >= sinirlar && hareketX > 0) || (toplamrotasyonX <= -sinirlar && hareketX < 0))
        {
            // Duvara çarptık, hareketi yutuyoruz.
        }
        else
        {
            toplamrotasyonX += hareketX;
        }

        if ((toplamrotasyonY >= sinirlar && hareketY > 0) || (toplamrotasyonY <= -sinirlar && hareketY < 0))
        {
            // Sınırda yut
        }
        else
        {
            toplamrotasyonY += hareketY;
        }

        // 3. ADIM: Güvenlik kilidi (Clamp)
        toplamrotasyonX = Mathf.Clamp(toplamrotasyonX, -sinirlar, sinirlar);
        toplamrotasyonY = Mathf.Clamp(toplamrotasyonY, -sinirlar, sinirlar);

        // 4. ADIM: Rotasyonu oluştur ve kameraya uygula
        rotasyon = Quaternion.Euler(toplamrotasyonX, toplamrotasyonY, 0);
        cam.transform.rotation = rotasyon;

        // 5. ADIM: Nefes Alma Efekti (Position Bobbing)
        ApplyBreathing();

        // 6. ADIM: Fener Gecikmesi (Sway/Lag)
        ApplyFlashlightSway();
    }

    void ApplyBreathing()
    {
        if (!nefesAlmaAktif || cam == null) return;

        nefesZamani += Time.deltaTime * nefesHizi;
        
        // Sinüs ve kosinüs dalgası ile yumuşak 8 çizim veya elips hareketi
        float yeniX = baslangicPozisyonu.x + Mathf.Cos(nefesZamani) * nefesMiktariX;
        float yeniY = baslangicPozisyonu.y + Mathf.Sin(nefesZamani * 2f) * nefesMiktariY;
        
        cam.transform.localPosition = new Vector3(yeniX, yeniY, baslangicPozisyonu.z);
    }

    void ApplyFlashlightSway()
    {
        // Fener referansı atanmışsa ve sahnedeyse
        if (fenerTransform == null) return;

        // Fener rotasyonunu, kameranın rotasyonuna yumuşak bir şekilde Lerp/Slerp ile yaklaştırıyoruz
        fenerTransform.rotation = Quaternion.Slerp(fenerTransform.rotation, cam.transform.rotation, Time.deltaTime * fenerSwayHizi);
    }
}