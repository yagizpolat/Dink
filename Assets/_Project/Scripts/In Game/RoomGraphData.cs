using System;
using UnityEngine;

/// <summary>
/// ScriptableObject tabanlı Esnek Oda Geçiş Ağı Veri Yapısı (Room Graph Data).
/// Unity Inspector üzerinden kod yazmadan odaları, düğüm bağlantılarını ve lore sahnelerini yönetmeyi sağlar.
/// </summary>
[CreateAssetMenu(fileName = "NewRoomGraphData", menuName = "Dink/Room Graph Data", order = 1)]
public class RoomGraphData : ScriptableObject
{
    [System.Serializable]
    public class RoomNode
    {
        [Tooltip("Oda Numarası (1 - 15)")]
        public int roomIndex = 1;

        [Tooltip("Oda Tanımı / Adı (Örn: Oda 4 - Market Vardiyası)")]
        public string roomName = "Oda";

        [Tooltip("İlk 3 Tutorial Odası mı? (True ise iki kapı da doğrudur, zindan riski yoktur)")]
        public bool isTutorial = false;

        [Tooltip("Kilit Flaş Geçiş Odası mı? (Oda 7 & 11: Başarılı oyuncu doğru kapıyı açtığında Hakan'ın dram odasına geçiş yapar)")]
        public bool isMilestoneFlashRoom = false;

        [Tooltip("Sol Kapı Doğru Seçilirse Geçilecek Hedef Oda Index'i (Geriye Dönüş / Loop Yapılabilir)")]
        public int leftTargetRoomIndex = 2;

        [Tooltip("Sağ Kapı Doğru Seçilirse Geçilecek Hedef Oda Index'i (Geriye Dönüş / Loop Yapılabilir)")]
        public int rightTargetRoomIndex = 3;

        [Tooltip("Bu Odadaki Ana Lore Objesinin İnsani Açıklaması")]
        public string loreObjectDescription = "Hakan'ın Geçmiş Anısı";
    }

    [Header("15 Odalık Geçiş Düğümleri Listesi")]
    public RoomNode[] roomNodes = new RoomNode[15];

    /// <summary>
    /// Verilen oda numarasına sahip düğümü getirir.
    /// </summary>
    public RoomNode GetRoomNode(int index)
    {
        if (roomNodes == null || roomNodes.Length == 0) return null;
        foreach (var node in roomNodes)
        {
            if (node != null && node.roomIndex == index)
            {
                return node;
            }
        }
        return roomNodes[0];
    }
}
