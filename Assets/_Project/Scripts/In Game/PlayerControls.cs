using UnityEngine;

/// <summary>
/// Oyuncu kontrollerini (yürüme ve fareyle bakış) tek yerden açıp kapatır.
/// ESC menüsü ve mektup paneli bunu çağırır.
/// </summary>
public static class PlayerControls
{
    public static void SetEnabled(bool enable)
    {
        PlayerWalker walker = Object.FindAnyObjectByType<PlayerWalker>();
        if (walker != null) walker.enabled = enable;
    }
}
