/// <summary>Tur sayısı için ince sarmalayıcı; veri <see cref="DinkSave.LoopCount"/> içinde tutulur.</summary>
public static class DinkLoop
{
    public static int Count => DinkSave.LoopCount;

    public static void Increment()
    {
        DinkSave.LoopCount = DinkSave.LoopCount + 1;
        DinkSave.Save();
    }

    public static void Reset()
    {
        DinkSave.LoopCount = 0;
        DinkSave.Save();
    }
}
