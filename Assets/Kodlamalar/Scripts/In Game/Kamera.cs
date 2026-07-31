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


    private void Start()
    {

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        
            // 1. ADIM: Önce fare girdilerini güncel olarak alıyoruz.
            fareX = Input.GetAxis("Mouse Y");
            fareY = Input.GetAxis("Mouse X");

            // Hesaplayacağımız ham hareket miktarını önden belirleyelim
            float hareketX = fareX * -hassasiyet;
            float hareketY = fareY * hassasiyet;

            // 2. ADIM: Sınır kontrolü ve "Koşullu Toplama" (Titremeyi engelleyen sihirli kısım)
            // Eğer üst sınırdaysak VE fareyi daha da yukarı itmeye çalışıyorsan (hareketX > 0)
            // VEYA alt sınırdaysak VE fareyi daha da aşağı itmeye çalışıyorsan (hareketX < 0)
            // O zaman bu hareketi toplama EKLEME!
            if ((toplamrotasyonX >= sinirlar && hareketX > 0) || (toplamrotasyonX <= -sinirlar && hareketX < 0))
            {
                // Duvara çarptık, fareyi dışarı zorluyorsun. Hareketi yutuyoruz, eklemiyoruz.
            }
            else
            {
                // Sınırda değiliz veya içeri doğru (güvenli yöne) çekiyoruz, ekleyebiliriz!
                toplamrotasyonX += hareketX;
            }

            if ((toplamrotasyonY >= sinirlar && hareketY > 0) || (toplamrotasyonY <= -sinirlar && hareketY < 0))
            {

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
        
        
    }
}