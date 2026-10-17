using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Speedrunner Hibrit Tespit Motoru (Speedrunner Detector Engine).
/// Hem Ölçüt A (Lore nesnesi etkileşim sayısı) hem de Ölçüt B (Odalarda kalma süresi) takibi yapar.
/// </summary>
public class SpeedrunnerDetector : MonoBehaviour
{
    public static SpeedrunnerDetector instance;

    [Header("Speedrunner Eşik Ayarları")]
    [Tooltip("İyi Son için gereken minimum lore obje etkileşim sayısı")]
    [SerializeField] private int minLoreInteractionsRequired = 6;

    [Tooltip("Speedrunner sayılmak için oda başına ortalama minimum kalış süresi (Saniye)")]
    [SerializeField] private float minAverageTimePerRoom = 3.0f;

    [Header("Canlı İstatistikler")]
    public int totalLoreInteractionsCount = 0;
    public float totalGameplayTime = 0f;
    public int totalRoomsVisitedCount = 0;

    private float currentRoomStartTime;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void OnRoomEntered()
    {
        totalRoomsVisitedCount++;
        currentRoomStartTime = Time.time;
    }

    public void OnRoomExited()
    {
        float timeSpentInRoom = Time.time - currentRoomStartTime;
        totalGameplayTime += timeSpentInRoom;
        Debug.Log($"[SpeedrunnerDetector] Odayı Terk Etti. Odada Kalma Süresi: {timeSpentInRoom:F1}s, Toplam Lore Etkileşimi: {totalLoreInteractionsCount}");
    }

    public void RegisterLoreInteraction()
    {
        totalLoreInteractionsCount++;
        Debug.Log($"[SpeedrunnerDetector] Lore Obje Etkileşimi Kaydedildi! (Toplam: {totalLoreInteractionsCount})");
    }

    /// <summary>
    /// Oyuncunun Speedrunner (Hikaye Atlayan) olup olmadığını hibrit (A+B) ölçütlerle kontrol eder.
    /// </summary>
    public bool IsSpeedrunner()
    {
        bool loreDeficit = totalLoreInteractionsCount < minLoreInteractionsRequired;

        float averageTime = totalRoomsVisitedCount > 0 ? (totalGameplayTime / totalRoomsVisitedCount) : 0f;
        bool timeDeficit = averageTime < minAverageTimePerRoom;

        bool speedrunnerDetected = loreDeficit || timeDeficit;

        Debug.Log($"[SpeedrunnerDetector] Speedrunner Kontrolü -> Lore Yetersiz mi (Ölçüt A): {loreDeficit}, Süre Yetersiz mi (Ölçüt B): {timeDeficit} (Ortalama: {averageTime:F1}s) => SONUÇ: {(speedrunnerDetected ? "SPEEDRUNNER TESPİT EDİLDİ!" : "NORMAL OYUNCU (HİKAYE %100)")}");

        return speedrunnerDetected;
    }

    public void ResetStats()
    {
        totalLoreInteractionsCount = 0;
        totalGameplayTime = 0f;
        totalRoomsVisitedCount = 0;
    }
}
