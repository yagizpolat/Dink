using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Zamanı (Time.timeScale) ve imleci yöneten tek yer. Duraklatan her ekran (ESC menüsü, mektup, defter,
/// skor defteri, menü geçişi) kendini sahip olarak bildirir; son sahip bırakınca oyun kaldığı yerden sürer.
/// Böylece mektup defterin üstünde açılıp kapansa da zaman ve imleç doğru hâle döner.
/// </summary>
public static class PauseService
{
    static readonly HashSet<object> owners = new HashSet<object>();

    public static bool IsPaused => owners.Count > 0;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
        owners.Clear();
        // Domain reload kapalıyken her Play'de abonelik birikmesin
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // Yeni sahne duraklatılmış gelmez; önceki sahnenin ekranları artık yok
    static void OnSceneLoaded(Scene scene, LoadSceneMode mode) => ResetAll();

    /// <summary>Zamanı durdurur, imleci serbest bırakır, yürümeyi kapatır.</summary>
    public static void Pause(object owner)
    {
        if (!owners.Add(owner) || owners.Count > 1) return;
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        PlayerControls.SetPaused(true);
    }

    /// <summary>Sahibi bırakır; başka duraklatan yoksa zaman akar, yürüyen oyuncu varsa imleç kilitlenir.</summary>
    public static void Resume(object owner)
    {
        if (!owners.Remove(owner) || owners.Count > 0) return;
        Time.timeScale = 1f;
        PlayerControls.SetPaused(false);
        if (Object.FindAnyObjectByType<PlayerWalker>() != null)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    /// <summary>Sahne değişirken: tüm duraklatmaları bırakır, zamanı akıtır (imlece dokunmaz).</summary>
    public static void ResetAll()
    {
        owners.Clear();
        Time.timeScale = 1f;
    }
}
