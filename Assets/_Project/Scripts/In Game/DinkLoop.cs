using UnityEngine;

/// <summary>Oyunun kaçıncı turda olduğunu PlayerPrefs'te tutar.</summary>
public static class DinkLoop
{
    private const string Key = "Dink_LoopCount";

    public static int Count => PlayerPrefs.GetInt(Key, 0);

    public static void Increment()
    {
        PlayerPrefs.SetInt(Key, Count + 1);
        PlayerPrefs.Save();
    }

    public static void Reset()
    {
        PlayerPrefs.DeleteKey(Key);
        PlayerPrefs.Save();
    }
}
