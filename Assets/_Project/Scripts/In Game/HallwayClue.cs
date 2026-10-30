using UnityEngine;

/// <summary>
/// Koridordaki daire kapısı ayrıntısı: zil, kapı dürbünü ya da isimlik. Yalnız ışık yanıkken incelenebilir.
/// Uyanık dairenin ayrıntısı ipucudur (HallwayClueTask'a bildirilir); diğerleri boş bir tepki verir.
/// </summary>
[RequireComponent(typeof(Collider))]
public class HallwayClue : MonoBehaviour
{
    public enum Kind { Bell, Peephole, Nameplate }

    [SerializeField] private HallwayClueTask task;
    [SerializeField] private HallwayLightTimer timer;
    [SerializeField] private Kind kind;
    [SerializeField] private bool isClue;
    [SerializeField] private float reach = 1.6f;

    private Collider col;
    private Camera cam;
    private AudioSource source;
    private bool found;

    public Kind ClueKind => kind;

    private void Awake()
    {
        col = GetComponent<Collider>();
        source = gameObject.AddComponent<AudioSource>();
        source.spatialBlend = 1f;
        source.minDistance = 0.6f;
        source.maxDistance = 8f;
        var target = GetComponent<InteractionTarget>();
        if (target == null) target = gameObject.AddComponent<InteractionTarget>();
        target.Configure(kind == Kind.Bell ? InteractionTarget.Verb.Press : InteractionTarget.Verb.Read, reach);
    }

    private void Update()
    {
        if (task == null || task.Done || PauseService.IsPaused || Time.timeScale == 0f) return;
        if (!Input.GetMouseButtonDown(0) && !Input.GetKeyDown(KeyCode.E)) return;

        if (cam == null) cam = Camera.main;
        if (cam == null) return;

        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (col.Raycast(ray, out _, reach)) Inspect();
    }

    private void Inspect()
    {
        if (!timer.IsLit)
        {
            ScreenHint.Show(DinkLang.T("Too dark to see.", "Görmek için çok karanlık.", "Zu dunkel, um etwas zu sehen.",
                "Trop sombre pour voir.", "Demasiado oscuro para ver.", "Escuro demais para ver.", "Слишком темно.",
                "太暗了，看不清。", "暗くて見えない。", "너무 어두워서 보이지 않는다."), 2f);
            return;
        }

        if (kind == Kind.Bell) source.PlayOneShot(task.BellClip);

        if (isClue)
        {
            if (kind == Kind.Bell) source.PlayOneShot(task.MuffledClip, 0.8f);
            ScreenHint.Show(ClueText(), 0.5f);
            if (!found)
            {
                found = true;
                task.Found(this);
            }
        }
        else
        {
            ScreenHint.Show(EmptyText(), 0.5f);
        }
    }

    private string ClueText() => kind switch
    {
        Kind.Bell => DinkLang.T("A TV goes quiet behind the door. Someone is home.", "Kapının ardında bir televizyon susuyor. İçeride biri var.",
            "Hinter der Tür verstummt ein Fernseher. Jemand ist zu Hause.", "Derrière la porte, une télé se tait. Quelqu'un est là.",
            "Tras la puerta, una tele se apaga. Hay alguien en casa.", "Atrás da porta, uma TV se cala. Tem alguém em casa.",
            "За дверью замолкает телевизор. Кто-то дома.", "门后的电视突然没了声音。有人在家。", "ドアの向こうでテレビが黙った。誰かいる。",
            "문 너머에서 TV 소리가 멎는다. 누군가 있다."),
        Kind.Peephole => DinkLang.T("Light moves behind the peephole.", "Dürbünün ardında bir ışık kıpırdıyor.",
            "Hinter dem Türspion bewegt sich Licht.", "Une lumière bouge derrière le judas.", "Una luz se mueve tras la mirilla.",
            "Uma luz se move atrás do olho mágico.", "За глазком движется свет.", "猫眼后面有光在晃动。", "のぞき穴の奥で光が動いた。",
            "문구멍 너머로 불빛이 움직인다."),
        _ => DinkLang.T("The name has been scratched out.", "İsim karalanmış.", "Der Name wurde durchgestrichen.",
            "Le nom a été rayé.", "El nombre está tachado.", "O nome foi riscado.", "Имя зачёркнуто.", "名字被划掉了。",
            "名前が塗りつぶされている。", "이름이 지워져 있다."),
    };

    private string EmptyText() => kind switch
    {
        Kind.Bell => DinkLang.T("No answer.", "Cevap yok.", "Keine Antwort.", "Pas de réponse.", "Nadie responde.", "Ninguém responde.",
            "Никто не отвечает.", "没有回应。", "応答はない。", "대답이 없다."),
        Kind.Peephole => DinkLang.T("Dark behind the peephole.", "Dürbünün ardı karanlık.", "Hinter dem Türspion ist es dunkel.",
            "Tout est noir derrière le judas.", "Oscuridad tras la mirilla.", "Escuridão atrás do olho mágico.", "За глазком темно.",
            "猫眼后面一片漆黑。", "のぞき穴の奥は真っ暗だ。", "문구멍 너머는 깜깜하다."),
        _ => DinkLang.T("Just a name.", "Sadece bir isim.", "Nur ein Name.", "Juste un nom.", "Solo un nombre.", "Só um nome.",
            "Просто имя.", "只是一个名字。", "ただの名前だ。", "그냥 이름이다."),
    };
}
