using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Görev bitince açılan daire kapısı: kilit tıkı, kanat yavaşça içeri açılır. Kapının ardındaki karanlık
/// girişe adım atan oyuncu kısa bir kararmayla bir sonraki koridora (sahnedeki kopya kat) taşınır.
/// </summary>
public class HallwayDoorPassage : MonoBehaviour
{
    [SerializeField] private Transform hinge;          // dönen menteşe; +Y kapıyı içeri açar
    [SerializeField] private float openAngle = 80f;
    [SerializeField] private float openSeconds = 2.2f;
    [SerializeField] private Bounds passZone;          // dünya koordinatında: buraya giren oyuncu geçer
    [SerializeField] private Transform destination;    // varış noktası (ileri yönü = bakış)

    private Transform player;
    private bool open;
    private bool passing;
    private AudioSource source;
    private Image fade;

    private void Awake()
    {
        source = gameObject.AddComponent<AudioSource>();
        source.spatialBlend = 1f;
        source.maxDistance = 10f;
        var walker = FindFirstObjectByType<PlayerWalker>();
        if (walker != null) player = walker.transform;
    }

    /// <summary>Görev bitti: kapı aralanır.</summary>
    public void Open()
    {
        if (open) return;
        open = true;
        source.PlayOneShot(Latch());
        StartCoroutine(Swing());
    }

    private IEnumerator Swing()
    {
        yield return new WaitForSeconds(0.4f);
        for (float t = 0f; t < openSeconds; t += Time.deltaTime)
        {
            hinge.localRotation = Quaternion.Euler(0f, openAngle * Mathf.SmoothStep(0f, 1f, t / openSeconds), 0f);
            yield return null;
        }
        hinge.localRotation = Quaternion.Euler(0f, openAngle, 0f);
    }

    private void Update()
    {
        if (!open || passing || player == null) return;
        if (passZone.Contains(player.position)) StartCoroutine(Pass());
    }

    private IEnumerator Pass()
    {
        passing = true;
        if (fade == null) fade = CreateFade();
        yield return Fade(0f, 1f, 0.35f);

        var cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        player.SetPositionAndRotation(destination.position, Quaternion.Euler(0f, destination.eulerAngles.y, 0f));
        if (cc != null) cc.enabled = true;

        yield return new WaitForSeconds(0.25f);
        yield return Fade(1f, 0f, 0.6f);
    }

    private IEnumerator Fade(float from, float to, float seconds)
    {
        for (float t = 0f; t < seconds; t += Time.deltaTime)
        {
            fade.color = new Color(0f, 0f, 0f, Mathf.Lerp(from, to, t / seconds));
            yield return null;
        }
        fade.color = new Color(0f, 0f, 0f, to);
    }

    private static Image CreateFade()
    {
        var go = new GameObject("[GecisKararma]");
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 950;
        var img = new GameObject("Siyah").AddComponent<Image>();
        img.transform.SetParent(go.transform, false);
        img.raycastTarget = false;
        img.color = new Color(0f, 0f, 0f, 0f);
        var r = img.rectTransform;
        r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = r.offsetMax = Vector2.zero;
        return img;
    }

    /// <summary>Kilit dili çekilirken iki kısa metal tık.</summary>
    private static AudioClip Latch()
    {
        const int rate = 44100;
        int n = Mathf.CeilToInt(0.25f * rate);
        var data = new float[n];
        var rng = new System.Random(9);
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)rate;
            float a = Mathf.Exp(-t * 90f), b = t > 0.12f ? Mathf.Exp(-(t - 0.12f) * 70f) : 0f;
            float noise = (float)(rng.NextDouble() * 2 - 1);
            data[i] = 0.7f * (a + 0.8f * b) * (0.6f * noise + 0.4f * Mathf.Sin(2 * Mathf.PI * 1400f * t));
        }
        var clip = AudioClip.Create("Latch", n, 1, rate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
