using UnityEngine;

/// <summary>
/// Apartman bölümlerinde etkileşimli bir nesneyi işaretler. Asıl tıklama işini nesnenin kendi script'i yapar;
/// bu bileşen yalnızca nişangâha (InteractionReticle) "burası etkileşimli, fiili şu" der.
/// </summary>
[RequireComponent(typeof(Collider))]
public class InteractionTarget : MonoBehaviour
{
    public enum Verb { Read, Sign, Call, Press, Take, Open }

    [SerializeField] private Verb verb = Verb.Read;
    [SerializeField] private float reach = 1.8f;

    public float Reach => reach;

    /// <summary>Bileşeni koddan ekleyen betikler fiili ve menzili buradan ayarlar.</summary>
    public void Configure(Verb newVerb, float newReach)
    {
        verb = newVerb;
        reach = newReach;
    }

    public string Label => verb switch
    {
        Verb.Open => DinkLang.T("OPEN", "AÇ", "ÖFFNEN", "OUVRIR", "ABRIR", "ABRIR", "ОТКРЫТЬ", "打开", "開ける", "열기"),
        Verb.Take => DinkLang.T("TAKE", "AL", "NEHMEN", "PRENDRE", "COGER", "PEGAR", "ВЗЯТЬ", "拿起", "取る", "줍기"),
        Verb.Sign => DinkLang.T("SIGN", "İMZALA", "UNTERSCHREIBEN", "SIGNER", "FIRMAR", "ASSINAR", "ПОДПИСАТЬ", "签名", "署名する", "서명"),
        Verb.Call => DinkLang.T("CALL", "ÇAĞIR", "RUFEN", "APPELER", "LLAMAR", "CHAMAR", "ВЫЗВАТЬ", "呼叫", "呼ぶ", "호출"),
        Verb.Press => DinkLang.T("PRESS", "BAS", "DRÜCKEN", "APPUYER", "PULSAR", "APERTAR", "НАЖАТЬ", "按下", "押す", "누르기"),
        _ => DinkLang.T("READ", "OKU", "LESEN", "LIRE", "LEER", "LER", "ЧИТАТЬ", "阅读", "読む", "읽기"),
    };
}
