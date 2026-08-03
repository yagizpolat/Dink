using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    [SerializeField] public RawImage background;
    [SerializeField] public RawImage icon;
    private InventoryManager inventoryManager;

    [SerializeField] private Texture icons;
    [SerializeField] private string itemName;
    [SerializeField] private string itemDescription;

    private void Start()
    {
        inventoryManager = FindAnyObjectByType<InventoryManager>();
    }
    public void Select()
    {
        inventoryManager.SelectSlot(this);
    }
}
