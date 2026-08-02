using UnityEngine;
using UnityEngine.UI;

public class BatteryPickup : MonoBehaviour
{

    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private AudioSource sesbombasi;
    private InventoryManager inventorymanager;

    private void Start()
    {
        inventorymanager = FindAnyObjectByType<InventoryManager>();
    }

    public void Pickup()
    {
        sesbombasi.PlayOneShot(pickupSound);
        inventorymanager.AddItem();
        Destroy(gameObject);
    }
}
