using UnityEngine;

/// <summary>
/// Oyuncu kontrollerini (fareyle bakış, tıklama, nişangah) tek yerden açıp kapatır.
/// ESC menüsü, mektup paneli ve kapı seçimi aynı işi ayrı ayrı yapıyordu; artık hepsi bunu çağırır.
/// </summary>
public static class PlayerControls
{
    public static void SetEnabled(bool enable)
    {
        Kamera kamera = Object.FindAnyObjectByType<Kamera>();
        if (kamera != null) kamera.enabled = enable;

        temaskontrol tiklama = Object.FindAnyObjectByType<temaskontrol>();
        if (tiklama != null) tiklama.enabled = enable;

        CrosshairManager crosshair = Object.FindAnyObjectByType<CrosshairManager>();
        if (crosshair != null)
        {
            crosshair.enabled = enable;
            crosshair.SetCrosshairVisible(enable);
        }
    }
}
