using UnityEngine;

/// <summary>
/// İpucu objeleri üzerindeki akıllı etkileşim bileşeni.
/// temaskontrol.cs Raycast'i bu bileşeni tespit ettiğinde Interact() çağırır.
/// İpucu tarzına göre doğru sisteme (MektupNotHintUI, TelsizFisiltiController vb.) yönlendirir.
/// </summary>
public class HintPropComponent : MonoBehaviour
{
    private DynamicHintSystem.HintType assignedType;
    private bool shownDirLeft;
    private bool isFakeHint;
    private bool hasBeenInteracted;

    private void Awake()
    {
        EnsureClickableCollider();
    }

    /// <summary>
    /// DynamicHintSystem tarafından spawn sonrası çağrılır.
    /// İpucu objesinin tipini, yönünü ve Fake Hint durumunu yapılandırır.
    /// </summary>
    public void Configure(DynamicHintSystem.HintType type, bool dirLeft, bool isFake)
    {
        assignedType = type;
        shownDirLeft = dirLeft;
        isFakeHint = isFake;
        hasBeenInteracted = false;
        EnsureClickableCollider();
    }

    /// <summary>
    /// Tıklamanın rahat yapılabilmesi için obje ve çocuklarına uygun boyutta katı Collider ekler.
    /// </summary>
    private void EnsureClickableCollider()
    {
        Collider[] childColliders = GetComponentsInChildren<Collider>();
        foreach (var c in childColliders)
        {
            c.isTrigger = false; // Physics.Raycast'in rahat yakalayabilmesi için
        }

        // Ana objede Collider yoksa cömert bir BoxCollider ekle
        Collider mainCol = GetComponent<Collider>();
        if (mainCol == null && (childColliders == null || childColliders.Length == 0))
        {
            BoxCollider box = gameObject.AddComponent<BoxCollider>();
            box.size = new Vector3(0.6f, 0.6f, 0.6f);
            box.isTrigger = false;
        }
    }

    /// <summary>
    /// temaskontrol.cs tarafından Raycast tıklamasında çağrılır.
    /// İpucu tarzına göre uygun etkileşim sistemini tetikler.
    /// </summary>
    public void Interact()
    {
        Debug.Log($"[HintPropComponent] 🖱️ İpucu Objeksiyonuna Tıklandı! -> Tarz: {assignedType}, Gösterilen Yön: {(shownDirLeft ? "SOL" : "SAĞ")}, Fake: {isFakeHint}");

        switch (assignedType)
        {
            case DynamicHintSystem.HintType.MektupNot:
                MektupNotHintUI.Instance.OpenLetterPanel(shownDirLeft);
                break;

            case DynamicHintSystem.HintType.TelsizFisilti:
                if (!hasBeenInteracted)
                {
                    hasBeenInteracted = true;
                    TelsizFisiltiController.Instance.ActivateRadio(shownDirLeft, gameObject);
                }
                break;

            default:
                // Henüz özelleştirilmemiş tarzlar: Eski davranış (tek satır altyazı)
                if (DynamicHintSystem.instance != null)
                {
                    DynamicHintSystem.instance.ShowHintText();
                }
                break;
        }
    }
}
