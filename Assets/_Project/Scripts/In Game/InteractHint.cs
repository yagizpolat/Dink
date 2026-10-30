using UnityEngine;

/// <summary>Bakıp E / sol tık ile kısa bir tepki yazısı (ve tık sesi) veren nesne: kilitli kapı kolu, asansör çağırma düğmesi.</summary>
[RequireComponent(typeof(Collider))]
public class InteractHint : MonoBehaviour
{
    public enum Kind { LockedDoor, ElevatorCall }

    [SerializeField] private Kind kind;
    [SerializeField] private float reach = 1.8f;

    private Collider col;
    private Camera cam;
    private AudioSource source;
    private AudioClip click;

    private void Awake()
    {
        col = GetComponent<Collider>();
        source = gameObject.AddComponent<AudioSource>();
        source.spatialBlend = 1f;
        source.maxDistance = 6f;
        click = Click(kind == Kind.LockedDoor ? 0.09f : 0.04f, kind == Kind.LockedDoor ? 320f : 1800f);
        var target = GetComponent<InteractionTarget>();
        if (target == null) target = gameObject.AddComponent<InteractionTarget>();
        target.Configure(InteractionTarget.Verb.Press, reach);
    }

    private void Update()
    {
        if (PauseService.IsPaused || Time.timeScale == 0f) return;
        if (!Input.GetMouseButtonDown(0) && !Input.GetKeyDown(KeyCode.E)) return;
        if (cam == null) cam = Camera.main;
        if (cam == null) return;
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (!col.Raycast(ray, out _, reach)) return;

        source.PlayOneShot(click);
        ScreenHint.Show(kind == Kind.LockedDoor
            ? DinkLang.T("Locked.", "Kilitli.", "Abgeschlossen.", "Fermé à clé.", "Cerrado con llave.", "Trancada.", "Заперто.",
                "锁着。", "鍵がかかっている。", "잠겨 있다.")
            : DinkLang.T("The elevator doesn't come.", "Asansör gelmiyor.", "Der Aufzug kommt nicht.", "L'ascenseur ne vient pas.",
                "El ascensor no viene.", "O elevador não vem.", "Лифт не едет.", "电梯没有来。", "エレベーターは来ない。", "엘리베이터가 오지 않는다."), 2f);
    }

    /// <summary>Kısa mekanik tık: kilitli kolda tok, düğmede ince.</summary>
    private static AudioClip Click(float length, float tone)
    {
        const int rate = 44100;
        int n = Mathf.CeilToInt(length * rate);
        var data = new float[n];
        var rng = new System.Random(5);
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)rate;
            float env = Mathf.Exp(-t * 60f);
            data[i] = 0.8f * env * (0.5f * (float)(rng.NextDouble() * 2 - 1) + 0.5f * Mathf.Sin(2 * Mathf.PI * tone * t));
        }
        var c = AudioClip.Create("Click", n, 1, rate, false);
        c.SetData(data, 0);
        return c;
    }
}
