using UnityEngine;

/// <summary>
/// Apartman bölümlerinde etkileşimli bir nesneyi işaretler. Asıl tıklama işini nesnenin kendi script'i yapar;
/// bu bileşen yalnızca nişangâha (InteractionReticle) "burası etkileşimli, fiili şu" der.
/// </summary>
[RequireComponent(typeof(Collider))]
public class InteractionTarget : MonoBehaviour
{
    public enum Verb { Read, Sign, Call, Press }

    [SerializeField] private Verb verb = Verb.Read;
    [SerializeField] private float reach = 1.8f;

    public float Reach => reach;

    public string Label => verb switch
    {
        Verb.Sign => DinkLang.T("SIGN", "İMZALA", "UNTERSCHREIBEN", "SIGNER", "FIRMAR", "ASSINAR", "ПОДПИСАТЬ"),
        Verb.Call => DinkLang.T("CALL", "ÇAĞIR", "RUFEN", "APPELER", "LLAMAR", "CHAMAR", "ВЫЗВАТЬ"),
        Verb.Press => DinkLang.T("PRESS", "BAS", "DRÜCKEN", "APPUYER", "PULSAR", "APERTAR", "НАЖАТЬ"),
        _ => DinkLang.T("READ", "OKU", "LESEN", "LIRE", "LEER", "LER", "ЧИТАТЬ"),
    };
}
