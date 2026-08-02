using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    [SerializeField] private GameObject Inventory_Panel;
    bool IsPanelOpen = false;

    //PİL VE ENVANTER SİSTEMİ
    [SerializeField] private Texture batteryIcon;
    [SerializeField] private RawImage[] slots;
    //CURSOR SİSTEMİ
    [SerializeField] private GameObject CurrentCursorPanel;
    [SerializeField] private GameObject InventoryCursorPanel; 
    [SerializeField] private RectTransform inventoryCursor;

    //SES SİSTEMİ
    [SerializeField] private AudioSource SesBombasi;
    [SerializeField] private AudioClip InventoryOpen;
    [SerializeField] private AudioClip InventoryClose;
    bool IsOpenSound = false;

    //SCRİPTLER
    private Kamera kamerascript;
    private FenerKontrol fenerscript;
    private Escmenu escmenu;
    private temaskontrol mektup;


    public void AddItem()
    {
        Debug.Log("Envantere eklendi");
        slots[0].texture = batteryIcon;
    }
    

    private void Start()
    {
        FindObject();
    }
    void Update()
    {
        if (IsPanelOpen)
        {
            inventoryCursor.position = Input.mousePosition;
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            IsOpenSound = !IsOpenSound;
            IsPanelOpen = !IsPanelOpen;
            Cursors(IsPanelOpen);
            Inventory_Panel.SetActive(IsPanelOpen);
            //SCRİPT AÇMA KAPAMA + SES SİSTEMİ
            GameplayScripts(!IsPanelOpen);

         }
    }

    void GameplayScripts(bool enabled)
    {
        kamerascript.enabled = enabled;
        fenerscript.enabled = enabled;
        escmenu.enabled = enabled;
        mektup.enabled = enabled;
        if (IsOpenSound)
        {
            SesBombasi.PlayOneShot(InventoryOpen);
        }
        else
        {
            SesBombasi.PlayOneShot(InventoryClose);
        }
    }
    void FindObject()
    {
        kamerascript = FindAnyObjectByType<Kamera>();
        fenerscript = FindAnyObjectByType<FenerKontrol>();
        escmenu = FindAnyObjectByType<Escmenu>();
        mektup = FindAnyObjectByType<temaskontrol>();
    }

    void Cursors(bool enable)
    {
        if (enable)
        {
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        CurrentCursorPanel.SetActive(!enable);
        InventoryCursorPanel.SetActive(enable);
    }
}
