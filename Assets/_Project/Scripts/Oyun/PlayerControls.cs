using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Oyuncu kontrollerini (yürüme ve fareyle bakış) tek yerden açıp kapatır.
/// SetEnabled: asansör, defter, kapı geçişi, sinematik gibi senaryo adımları çağırır. Kilit sayaçlıdır:
/// iki adım aynı anda kilitlerse oyuncu ikisi de bitince açılır.
/// SetPaused: ESC menüsü ve mektup paneli çağırır; kapanınca kontrolü yalnız senaryo kilidi yoksa geri açar
/// (yoksa asansör yolculuğunda ESC → Devam oyuncuyu kabinin ortasında yürütüyordu).
/// </summary>
public static class PlayerControls
{
    static int scriptedLocks;
    static bool paused;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Init()
    {
        ResetLocks();
        // Domain reload kapalı (Enter Play Mode Options); her Play'de abonelik birikmesin
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // Yeni sahnede oyuncu baştan kurulur; önceki sahnenin kilitleri taşınmaz
    static void OnSceneLoaded(Scene scene, LoadSceneMode mode) => ResetLocks();

    static void ResetLocks()
    {
        scriptedLocks = 0;
        paused = false;
    }

    public static void SetEnabled(bool enable)
    {
        scriptedLocks = enable ? Mathf.Max(0, scriptedLocks - 1) : scriptedLocks + 1;
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
        if (walker != null) walker.enabled = scriptedLocks == 0 && !paused;
    }
}
