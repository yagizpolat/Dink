using System.Collections;
using UnityEngine;

/// <summary>Hafıza sınıfı S3: sınav kâğıdı okununca koridordaki dolap kapakları markete yakın uçtan başlayıp
/// sınıfa doğru sırayla çarpar; aralık kısalır, ses yaklaşır. Son çarpma sınıf kapısıdır: kapı oyuncunun
/// arkasından kapanır ve bir daha itilmez (çıkış artık başka yerden).</summary>
public class DolapKapanma : MonoBehaviour
{
    [SerializeField] private SinifKagit paper;
    [SerializeField] private ItmeKapi classDoor;
    [SerializeField] private ItmeKapi backDoor;                // S4: ön kapı kapanınca arka kapı (mutfak) açılır
    [SerializeField] private Light boardLight;
    [SerializeField] private AudioClip[] slams;               // gerçek kapı kapanma kayıtları, tiz çalınır (metal)
    [SerializeField] private AudioClip doorSlam;
    [SerializeField] private float startDelay = 2f;
    [SerializeField] private float xFrom = 11.6f, xTo = 23.4f;
    [SerializeField] private float zA = 1.0f, zB = 2.7f;
    [SerializeField] private int count = 14;
    [SerializeField] private Vector2 gap = new Vector2(1.1f, 0.22f); // ilk / son aralık

    private void OnEnable() { if (paper != null) paper.OnRead += Begin; }
    private void OnDisable() { if (paper != null) paper.OnRead -= Begin; }
    private void Begin() => StartCoroutine(Run());

    private IEnumerator Run()
    {
        yield return new WaitForSeconds(startDelay);
        for (int i = 0; i < count; i++)
        {
            float t = count > 1 ? i / (float)(count - 1) : 1f;
            var pos = new Vector3(Mathf.Lerp(xFrom, xTo, t), 1.2f, i % 2 == 0 ? zA : zB);
            Play(slams[Random.Range(0, slams.Length)], pos, Random.Range(1.35f, 1.6f), Mathf.Lerp(0.55f, 0.9f, t));
            yield return new WaitForSeconds(Mathf.Lerp(gap.x, gap.y, t * t));
        }
        yield return new WaitForSeconds(0.35f);
        // kapı yalnız oyuncu sınıfın içindeyken arkasından kapanır (dışarı kaçtıysa geri girmesini bekler: kilitli dışarıda kalmasın)
        var cam = Camera.main;
        while (cam != null && cam.transform.position.x < 24.6f) yield return null;
        Play(doorSlam, new Vector3(24.2f, 1.2f, 1.85f), 0.92f, 1f);
        if (classDoor != null) classDoor.SlamShut();
        yield return new WaitForSeconds(2.5f);
        if (boardLight != null)                                // tahta ışığı tökezleyip söner, arka kapının altından sıcak ışık
        {
            float b = boardLight.intensity;
            float[] f = { 0.2f, 1f, 0f, 0.6f, 0f };
            foreach (var k in f) { boardLight.intensity = b * k; yield return new WaitForSeconds(0.08f); }
            boardLight.enabled = false;
        }
        if (backDoor != null) backDoor.Unlock();
    }

    private void Play(AudioClip c, Vector3 pos, float pitch, float vol)
    {
        if (c == null) return;
        var g = new GameObject("DolapCarpma"); g.transform.position = pos;
        var s = g.AddComponent<AudioSource>();
        s.clip = c; s.pitch = pitch; s.volume = vol; s.spatialBlend = 1f;
        s.rolloffMode = AudioRolloffMode.Linear; s.minDistance = 1.5f; s.maxDistance = 30f;
        s.Play(); Destroy(g, c.length / pitch + 0.2f);
    }
}
