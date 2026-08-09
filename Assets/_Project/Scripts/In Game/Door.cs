using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] private DoorSequenceManager sequenceManager;
    private bool isCorrectDoor;
    private bool hasBeenSelected;

    public void Interact()
    {
        // Aynı kapının birden fazla kez seçilmesini ve akışın tekrarlanmasını engeller.
        if (hasBeenSelected)
        {
            return;
        }

        hasBeenSelected = true;

        // Efekt scripti varsa görsel efektleri oynat, sahne geçişini ona bırak.
        DoorEffects effects = GetComponent<DoorEffects>();
        if (effects != null)
        {
            effects.PlayEffects(isCorrectDoor, sequenceManager);
            return;
        }

        // Efekt scripti yoksa orijinal davranış.
        if (sequenceManager != null)
        {
            sequenceManager.HandleDoorSelected(isCorrectDoor);
            return;
        }

        // Yönetici henüz Inspector'da bağlanmadıysa mevcut sistemi bozmadan bilgi verir.
        Debug.Log(isCorrectDoor ? "Doğru Kapı" : "Yanlış Kapı");
    }

    public void SetCorrectDoor(bool value)
    {
        isCorrectDoor = value;
        hasBeenSelected = false;
    }
}

