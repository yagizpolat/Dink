using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Oyuncu kontrollerini (yürüme ve fareyle bakış) tek yerden açıp kapatır.
/// SetEnabled: asansör, defter, kapı geçişi, sinematik gibi senaryo adımları çağırır.
/// SetPaused: ESC menüsü ve mektup paneli çağırır; kapanınca kontrolü yalnız senaryo kilidi yoksa geri açar
/// (yoksa asansör yolculuğunda ESC → Devam oyuncuyu kabinin ortasında yürütüyordu).
/// </summary>
public static class PlayerControls
{
    static bool scriptedLock;
    static bool paused;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Init()
    {
        scriptedLock = false;
        paused = false;
        // Yeni sahnede oyuncu baştan kurulur; önceki sahnenin kilitleri taşınmaz
        SceneManager.sceneLoaded += (s, m) => { scriptedLock = false; paused = false; };
    }

    public static void SetEnabled(bool enable)
    {
        scriptedLock = !enable;
        Apply();
    }

    public static void SetPaused(bool pause)
    {
        paused = pause;
        Apply();
    }

    static void Apply()
    {
        PlayerWalker walker = Object.FindAnyObjectByType<PlayerWalker>();
        if (walker != null) walker.enabled = !scriptedLock && !paused;
    }
}
