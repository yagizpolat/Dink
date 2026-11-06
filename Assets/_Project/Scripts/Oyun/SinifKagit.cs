using System;
using System.Collections;
using UnityEngine;

/// <summary>Hafıza sınıfı S2: Hakan'ın sırasındaki yarım sınav kâğıdı. Bakıp E/tık = kâğıt göz hizasına kalkar,
/// okunur; dördüncü soru boştur. Birkaç saniye sonra kâğıt yerine iner (ya da ikinci tıkla). İlk okumada OnRead
/// olayı (S3: koridordaki dolaplar arkada kapanmaya başlar).</summary>
public class SinifKagit : MonoBehaviour
{
    [SerializeField] private Transform paper;
    [SerializeField] private Collider target;
    [SerializeField] private float reach = 1.8f;
    [SerializeField] private Vector3 holdOffset = new Vector3(0f, -0.01f, 0.34f);
    [SerializeField] private float holdSeconds = 6f;
    [SerializeField] private AudioClip rustle;
    [SerializeField] private float readGlow = 0.55f;          // elde arkadan ışık alır; okunur kalsın diye hafif kendi parlaklığı

    private Material mat;

    public bool Read { get; private set; }
    public event Action OnRead;

    private Camera cam;
    private bool held, busy;
    private int holdId;
    private Vector3 restPos; private Quaternion restRot; private Transform restParent;

    private void Start()
    {
        restParent = paper.parent; restPos = paper.localPosition; restRot = paper.localRotation;
        var r = paper.GetComponent<Renderer>();
        if (r != null) { mat = r.material; mat.EnableKeyword("_EMISSION"); mat.SetTexture("_EmissionMap", mat.GetTexture("_BaseMap")); mat.SetColor("_EmissionColor", Color.black); }
        if (r != null) r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    }

    private void Update()
    {
        var it = target.GetComponent<InteractionTarget>(); if (it != null) it.enabled = !held && !busy;
        if (busy || Time.timeScale == 0f || PauseService.IsPaused) return;
        if (!Input.GetMouseButtonDown(0) && !Input.GetKeyDown(KeyCode.E)) return;
        if (cam == null) cam = Camera.main;
        if (cam == null) return;
        if (held) { StartCoroutine(PutDown()); return; }
        if (target.Raycast(cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f)), out _, reach)) StartCoroutine(PickUp());
    }

    private IEnumerator PickUp()
    {
        busy = true;
        if (rustle != null) AudioSource.PlayClipAtPoint(rustle, paper.position, 0.4f);
        var fromP = paper.position; var fromR = paper.rotation;
        paper.SetParent(cam.transform, true);
        for (float t = 0f; t < 1f; t += Time.deltaTime / 0.6f)
        {
            float e = 1f - Mathf.Pow(1f - t, 3f);
            paper.localPosition = Vector3.Lerp(cam.transform.InverseTransformPoint(fromP), holdOffset, e);
            paper.localRotation = Quaternion.Slerp(Quaternion.Inverse(cam.transform.rotation) * fromR, Quaternion.identity, e);
            if (mat != null) mat.SetColor("_EmissionColor", Color.white * readGlow * e);
            yield return null;
        }
        paper.localPosition = holdOffset; paper.localRotation = Quaternion.identity;
        held = true; busy = false;
        if (!Read)
        {
            Read = true;
            yield return new WaitForSeconds(1.2f);
            ScreenHint.Show(DinkLang.T("Question 4 was never answered.", "Dördüncü soru hiç cevaplanmamış.", "Frage 4 wurde nie beantwortet.",
                "La question 4 n'a jamais eu de réponse.", "La pregunta 4 nunca se respondió.", "A questão 4 nunca foi respondida.",
                "На вопрос 4 так и не ответили.", "第四题从没被回答。", "問4は答えられないままだ。", "4번 문제는 끝내 답이 없다."));
            OnRead?.Invoke();
        }
        // indirilip yeniden alınırsa eski sayaç yenisini erken indirmesin
        int id = ++holdId;
        float until = Time.time + holdSeconds;
        while (held && id == holdId && Time.time < until) yield return null;
        if (held && !busy && id == holdId) StartCoroutine(PutDown());
    }

    private IEnumerator PutDown()
    {
        busy = true; held = false;
        if (rustle != null) AudioSource.PlayClipAtPoint(rustle, paper.position, 0.3f);
        paper.SetParent(restParent, true);
        var fromP = paper.localPosition; var fromR = paper.localRotation;
        for (float t = 0f; t < 1f; t += Time.deltaTime / 0.5f)
        {
            float e = t * t;
            paper.localPosition = Vector3.Lerp(fromP, restPos, e);
            paper.localRotation = Quaternion.Slerp(fromR, restRot, e);
            if (mat != null) mat.SetColor("_EmissionColor", Color.white * readGlow * (1f - e));
            yield return null;
        }
        paper.localPosition = restPos; paper.localRotation = restRot;
        busy = false;
    }
}
