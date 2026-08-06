
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    [SerializeField] private GameObject Inventory_Panel;
    bool IsPanelOpen = false;


    //RENKLER
    private Color selectedcolor;
    private Color normalcolor;

    //ITEM INFO EKRANI
    [SerializeField] private RawImage previewImage;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private GameObject itemInfoPanel;

    //PİL VE ENVANTER SİSTEMİ
    [SerializeField] private Texture batteryIcon;
    [SerializeField] private InventorySlot[] slots;
    private InventorySlot selectedSlot;

    //CURSOR SİSTEMİ
    [SerializeField] private GameObject CurrentCursorPanel;
    [SerializeField] private GameObject InventoryCursorPanel; 
    [SerializeField] private RectTransform inventoryCursor;

    //SES SİSTEMİ
    [SerializeField] private AudioSource SesBombasi;
    [SerializeField] private AudioClip InventoryOpen;
    [SerializeField] private AudioClip InventoryClose;
    bool IsOpenSound = false;

    //ÇİFT TIKLAMA SİSTEMİ
    private float lastclick;
    private float doubleclick = 0.2f;

    //SCRİPTLER
    private Kamera kamerascript;
    private FenerKontrol fenerscript;
    private Escmenu escmenu;
    private temaskontrol mektup;

    private void Awake()
    {
        ColorUtility.TryParseHtmlString("#2A3D45", out selectedcolor);
        ColorUtility.TryParseHtmlString("#7A6C5D", out normalcolor);
    }

    public void AddItem()
    {
        for(int i = 0; i < slots.Length; i++)
        {
            if (slots[i].icon.texture == null)
            {
                slots[i].icon.texture = batteryIcon;
                break;
            }
        }
    }

    public void UseItem()
    {
        if(Time.time - lastclick < doubleclick)
        {
            bool eklendimi = fenerscript.bataryaekle(20);

            if (eklendimi)
            {
                selectedSlot.icon.texture = null;
                itemInfoPanel.SetActive(false);
                Debug.Log("Pil Eklendi");
            }

        }
        lastclick = Time.time;
    }
    
    public void SelectSlot(InventorySlot slot)
    {
        if(selectedSlot != null)
        {
            selectedSlot.icon.color = normalcolor;
            selectedSlot.background.color = normalcolor;
            itemInfoPanel.SetActive(false);
        }

        //SEÇİLEN SLOTUN ICONUN TEXTURE'U BOŞ DEĞİLSE 
        if(slot.icon.texture != null)
        {
            
            previewImage.texture = batteryIcon;
            titleText.text = "Pil";
            descriptionText.text = "El fenerini çalıştırmak için kullanılan standart bir pil.";
            itemInfoPanel.SetActive(true);
            selectedSlot = slot;
            slot.background.color = selectedcolor;
            UseItem();

        }
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
