using UnityEngine;

/// <summary>
/// Ortak tuş yardımcıları. Aynı karede ESC'yi birden fazla sistemin işlemesini engeller
/// (mektup, ayarlar ve intro ESC'yi "tüketir", ESC menüsü o karede açılmaz).
/// </summary>
public static class DinkInput
{
    private static int escapeConsumedFrame = -1;

    /// <summary>ESC bu karede basıldı ve henüz başka bir sistem tarafından işlenmedi.</summary>
    public static bool EscapeDown => Input.GetKeyDown(KeyCode.Escape) && escapeConsumedFrame != Time.frameCount;

    public static void ConsumeEscape()
    {
        escapeConsumedFrame = Time.frameCount;
    }
}
