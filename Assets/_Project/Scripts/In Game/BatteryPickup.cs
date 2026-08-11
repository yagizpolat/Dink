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
        if (sesbombasi != null && pickupSound != null)
        {
            float sfxVol = AudioManager.instance != null ? AudioManager.instance.GetSFXVolume() : PlayerPrefs.GetFloat("Dink_SFXVolume", 0.8f);
            sesbombasi.PlayOneShot(pickupSound, sfxVol);
        }

        if (inventorymanager != null)
        {
            inventorymanager.AddItem();
        }
        Destroy(gameObject);
    }
}
