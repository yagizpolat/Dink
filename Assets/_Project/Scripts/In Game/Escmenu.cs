using UnityEngine;

public class Escmenu : MonoBehaviour
{
    public GameObject escmenu;
    private bool oyundurdumu = false;
    public AudioSource escmenusesbileseni;
    public AudioClip escmenuacilmasesi;
    public AudioClip escmenukapanmasesi;

    //SCRİPT BAĞLANTILARI
    FenerKontrol fener;
    Kamera kamerascript;
    InventoryManager inventory;
    temaskontrol mektup_pil;

    private void Start()
    {
        ScriptConnect();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (oyundurdumu)
            {
                Devamet();
            }
            else
            {
                Durdur();
            }
        }
    }
    public void Devamet()
    {
        escmenu.SetActive(false);
        Time.timeScale = 1f;
        oyundurdumu = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        escmenusesbileseni.PlayOneShot(escmenukapanmasesi);

        // SCRİPT AÇMA KISMI
        CloseOpenScript(true);
    }
    public void Durdur()
    { 
        escmenu.SetActive(true);
        Time.timeScale = 0f;
        oyundurdumu = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        escmenusesbileseni.PlayOneShot(escmenuacilmasesi);

        // SCRİPT KAPATMA KISMI
        CloseOpenScript(false);
    }

    void ScriptConnect()
    {
        fener = FindAnyObjectByType<FenerKontrol>();
        kamerascript = FindAnyObjectByType<Kamera>();
        inventory = FindAnyObjectByType<InventoryManager>();
        mektup_pil = FindAnyObjectByType<temaskontrol>();
        
    }

    void CloseOpenScript(bool enable)
    {
        fener.enabled = enable;
        kamerascript.enabled = enable;
        inventory.enabled = enable;
        mektup_pil.enabled = enable;
    }
}
