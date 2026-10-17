using UnityEngine;
using UnityEngine.EventSystems;
using PrimeTween;

/// <summary>
/// Herhangi bir UI butonuna takıldığında fare üzerine gelme ve tıklama animasyonu verir.
/// </summary>
public class UIButtonJuice : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    [Header("Animasyon Ayarları")]
    [SerializeField] private float hoverScale = 1.06f; // Fare üstüne gelince %6 büyüsün
    [SerializeField] private float duration = 0.12f;   // Geçiş süresi (saniye)

    private Vector3 originalScale;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    private void OnDisable()
    {
        // Menü kapanıp açılırsa boyutu sıfırla
        transform.localScale = originalScale;
    }

    // 1. Fare butonun üzerine gelince (Hover)
    public void OnPointerEnter(PointerEventData eventData)
    {
        Tween.Scale(transform, endValue: originalScale * hoverScale, duration: duration, ease: Ease.OutQuad);
    }

    // 2. Fare butondan ayrılınca (Exit)
    public void OnPointerExit(PointerEventData eventData)
    {
        Tween.Scale(transform, endValue: originalScale, duration: duration, ease: Ease.OutQuad);
    }

    // 3. Butona tıklandığı an (Click / Press)
    public void OnPointerDown(PointerEventData eventData)
    {
        // Buton içe doğru hafifçe esnesin (Fiziksel tuş hissi)
        Tween.PunchScale(transform, strength: new Vector3(-0.08f, -0.08f, 0f), duration: 0.15f);
    }
}