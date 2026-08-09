using System.Collections;
using UnityEngine;

public class DoorEffects : MonoBehaviour
{
    [Header("Kapı Kanadı")]
    [Tooltip("Döndürülecek kapı kanadı (L_Door veya R_Door). Boş bırakılırsa açılma/sarsılma efektleri devre dışı kalır.")]
    [SerializeField] private Transform kapiKanadi;

    [Header("Süzülme (Floating)")]
    [SerializeField] private float suzulmeHizi = 1.0f;
    [SerializeField] private float suzulmeMiktari = 0.08f;
    private Vector3 baslangicPozisyonu;
    private float rastgeleOffset;
    private bool suzulmeAktif = true;

    [Header("Açılma (Doğru Kapı)")]
    [SerializeField] private float acilmaHizi = 3.0f;
    [SerializeField] private float acilmaAcisi = -90f;
    [SerializeField] private float gecisGecikmesi = 1.2f;
    private bool kapiAciliyor = false;
    private Quaternion kanatHedefRot;

    [Header("Sarsılma (Yanlış Kapı)")]
    [SerializeField] private float sarsilmaSuresi = 0.35f;
    [SerializeField] private float sarsilmaGucu = 0.04f;

    private void Start()
    {
        baslangicPozisyonu = transform.localPosition;
        rastgeleOffset = Random.Range(0f, 100f);
    }

    private void Update()
    {
        // Süzülme: Parent obje (çerçeve) süzülür, child (kapı kanadı) otomatik olarak onunla birlikte süzülür.
        // Kapı seçildiğinde suzulmeAktif false olur ve bu blok çalışmaz -> her şey olduğu yerde donar.
        if (suzulmeAktif)
        {
            float yeniY = baslangicPozisyonu.y + Mathf.Sin((Time.time + rastgeleOffset) * suzulmeHizi) * suzulmeMiktari;
            transform.localPosition = new Vector3(baslangicPozisyonu.x, yeniY, baslangicPozisyonu.z);
        }

        // Doğru kapı açılma animasyonu: Sadece kapı kanadının rotasyonunu değiştirir.
        // Parent (çerçeve) ve kapı kanadının pozisyonu hiç değişmez.
        if (kapiAciliyor && kapiKanadi != null)
        {
            kapiKanadi.localRotation = Quaternion.Slerp(kapiKanadi.localRotation, kanatHedefRot, Time.deltaTime * acilmaHizi);
        }
    }

    /// <summary>
    /// Door.cs tarafından çağrılır. Süzülmeyi durdurur, görsel efektleri oynatır
    /// ve sahne geçişini gecikmeli olarak tetikler.
    /// </summary>
    public void PlayEffects(bool isCorrectDoor, DoorSequenceManager sequenceManager)
    {
        // Süzülmeyi durdur: Çerçeve ve kapı kanadı o anki yüksekliğinde sabit kalır.
        suzulmeAktif = false;

        if (isCorrectDoor)
        {
            // Doğru kapı: Kapı kanadını yavaşça döndür, sonra sahne geçişi yap.
            if (kapiKanadi != null)
            {
                kanatHedefRot = kapiKanadi.localRotation * Quaternion.Euler(0f, acilmaAcisi, 0f);
                kapiAciliyor = true;
            }
            StartCoroutine(GecikmeliGecis(sequenceManager, true));
        }
        else
        {
            // Yanlış kapı: Kapıyı sars, sonra jumpscare tetikle.
            StartCoroutine(SarsilVeGecis(sequenceManager));
        }
    }

    private IEnumerator GecikmeliGecis(DoorSequenceManager manager, bool isCorrect)
    {
        // Oyuncu kapının açılışını görsün diye bekle.
        yield return new WaitForSeconds(gecisGecikmesi);

        if (manager != null)
        {
            manager.HandleDoorSelected(isCorrect);
        }
    }

    private IEnumerator SarsilVeGecis(DoorSequenceManager manager)
    {
        float elapsed = 0f;
        Vector3 durduguPos = transform.localPosition;

        while (elapsed < sarsilmaSuresi)
        {
            float offsetX = Random.Range(-1f, 1f) * sarsilmaGucu;
            transform.localPosition = new Vector3(durduguPos.x + offsetX, durduguPos.y, durduguPos.z);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Sarsıntı bitince kapıyı durduğu yere geri koy.
        transform.localPosition = durduguPos;

        if (manager != null)
        {
            manager.HandleDoorSelected(false);
        }
    }
}
