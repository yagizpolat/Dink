using UnityEngine;

/// <summary>
/// Odayı belirlenen açı sınırları arasında (örneğin -25° ile +25° arasında)
/// sinüs dalgası şeklinde yumuşakça ileri-geri sallandırır (Salınım / Sarkaç etkisi).
/// Bu sayede oda asla kameranın açısından çıkmaz ve ters dönmez.
/// </summary>
public class RoomRotator : MonoBehaviour
{
    public enum Eksen { Z, X, Y }

    [Header("Salınım Ayarları")]
    [Tooltip("Hangi eksende sallanacağı (Varsayılan Z - Yanlara tatlı yatma)")]
    public Eksen sallanmaEkseni = Eksen.Z;

    [Tooltip("Maksimum açılma açısı (Örn: 25 ise -25° ile +25° arasında gidip gelir)")]
    public float maxAci = 25f;

    [Tooltip("Sallanma hızı (Yüksek değerler daha hızlı sallar)")]
    public float sallanmaHizi = 1.2f;

    private Vector3 baslangicRotasyonu;

    void Start()
    {
        // Odanın sahnedeki ilk rotasyonunu kaydet
        baslangicRotasyonu = transform.localEulerAngles;
    }

    void Update()
    {
        // Sinüs dalgası ile -maxAci ile +maxAci arasında yumuşak salınım (-25° ... +25°)
        float aci = Mathf.Sin(Time.time * sallanmaHizi) * maxAci;

        Vector3 yeniRotasyon = baslangicRotasyonu;

        switch (sallanmaEkseni)
        {
            case Eksen.Z:
                yeniRotasyon.z = baslangicRotasyonu.z + aci;
                break;
            case Eksen.X:
                yeniRotasyon.x = baslangicRotasyonu.x + aci;
                break;
            case Eksen.Y:
                yeniRotasyon.y = baslangicRotasyonu.y + aci;
                break;
        }

        transform.localEulerAngles = yeniRotasyon;
    }
}
