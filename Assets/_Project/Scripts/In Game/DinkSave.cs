using UnityEngine;

/// <summary>
/// Kayıt verisinin tek kaynağı (PlayerPrefs). Kat sahnesine varışta otomatik kaydedilir;
/// ana menüdeki Devam Et son kaydedilen katı açar. Tur sayısı da buradadır (<see cref="DinkLoop"/> buna yönlenir).
/// </summary>
public static class DinkSave
{
    private const string FloorKey = "Dink_Save_LastFloor";
    private const string TracesKey = "Dink_Save_Traces";
    private const string NameKey = "Dink_Save_PlayerName";
    private const string ElapsedKey = "Dink_Save_Elapsed";
    // Eski anahtar korunur, önceki kayıtlardaki tur sayısı kaybolmasın
    private const string LoopKey = "Dink_LoopCount";

    public const int TraceSlots = 5;

    public static bool HasSave => !string.IsNullOrEmpty(LastFloorScene);

    public static string LastFloorScene
    {
        get => PlayerPrefs.GetString(FloorKey, "");
        set => PlayerPrefs.SetString(FloorKey, value ?? "");
    }

    public static string PlayerName
    {
        get => PlayerPrefs.GetString(NameKey, "");
        set => PlayerPrefs.SetString(NameKey, value ?? "");
    }

    public static float ElapsedSeconds
    {
        get => PlayerPrefs.GetFloat(ElapsedKey, 0f);
        set => PlayerPrefs.SetFloat(ElapsedKey, Mathf.Max(0f, value));
    }

    public static int TracesMask
    {
        get => PlayerPrefs.GetInt(TracesKey, 0);
        set => PlayerPrefs.SetInt(TracesKey, value);
    }

    public static bool IsTraceFound(int id) => id >= 0 && id < TraceSlots && (TracesMask & (1 << id)) != 0;

    public static void MarkTraceFound(int id)
    {
        if (id < 0 || id >= TraceSlots) return;
        TracesMask |= 1 << id;
    }

    public static int TraceCount
    {
        get
        {
            int mask = TracesMask, count = 0;
            for (int i = 0; i < TraceSlots; i++) if ((mask & (1 << i)) != 0) count++;
            return count;
        }
    }

    public static int LoopCount
    {
        get => PlayerPrefs.GetInt(LoopKey, 0);
        set => PlayerPrefs.SetInt(LoopKey, Mathf.Max(0, value));
    }

    /// <summary>Kat sahnesi mi ("Kat1".."Kat9"); yalnız bunlarda kayıt alınır.</summary>
    public static bool IsFloorScene(string sceneName) =>
        !string.IsNullOrEmpty(sceneName) && sceneName.Length == 4 && sceneName.StartsWith("Kat") && char.IsDigit(sceneName[3]);

    /// <summary>Kata varışta çağrılır: sahne, ad ve o anki kronometre süresi yazılır.</summary>
    public static void SaveFloorArrival(string sceneName)
    {
        if (!IsFloorScene(sceneName)) return;
        LastFloorScene = sceneName;
        string name = LobbyLogbook.SignedName;
        if (string.IsNullOrEmpty(name) && LeaderboardManager.instance != null) name = LeaderboardManager.instance.GetPlayerName();
        if (!string.IsNullOrEmpty(name)) PlayerName = name;
        if (LeaderboardManager.instance != null && LeaderboardManager.instance.IsTimerRunning)
            ElapsedSeconds = LeaderboardManager.instance.ElapsedTime;
        Save();
    }

    /// <summary>Kayıttan devam ederken deftere yazılacak ad: kayıttaki ad, yoksa skor adı, yoksa "Anonymous Player".</summary>
    public static string ResumeName()
    {
        string ad = PlayerName;
        if (string.IsNullOrWhiteSpace(ad) && LeaderboardManager.instance != null) ad = LeaderboardManager.instance.GetPlayerName();
        return string.IsNullOrWhiteSpace(ad) ? "Anonymous Player" : ad;
    }

    /// <summary>Kronometre süresini günceller (çıkışta, ana menüye dönüşte); kat kaydına dokunmaz.</summary>
    public static void SaveElapsedIfRunning()
    {
        if (!HasSave || LeaderboardManager.instance == null || !LeaderboardManager.instance.IsTimerRunning) return;
        ElapsedSeconds = LeaderboardManager.instance.ElapsedTime;
        Save();
    }

    /// <summary>Oyun bitince kat ilerlemesini ve süreyi siler; izler ve tur sayısı kalır.</summary>
    public static void ClearProgress()
    {
        PlayerPrefs.DeleteKey(FloorKey);
        PlayerPrefs.DeleteKey(ElapsedKey);
        Save();
    }

    /// <summary>Yeni oyun: tur sayısı dahil her şeyi sıfırlar.</summary>
    public static void NewGame()
    {
        PlayerPrefs.DeleteKey(FloorKey);
        PlayerPrefs.DeleteKey(TracesKey);
        PlayerPrefs.DeleteKey(NameKey);
        PlayerPrefs.DeleteKey(ElapsedKey);
        PlayerPrefs.DeleteKey(LoopKey);
        Save();
    }

    public static void Save() => PlayerPrefs.Save();
}
