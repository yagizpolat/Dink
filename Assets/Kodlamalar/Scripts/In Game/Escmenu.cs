using UnityEngine;

public class Escmenu : MonoBehaviour
{
    public GameObject escmenu;
    private bool oyundurdumu = false;
    public AudioSource escmenusesbileseni;
    public AudioClip escmenuacilmasesi;
    public AudioClip escmenukapanmasesi;

    private void Start()
    {
       
        
        //KapıSeçme kamerascript = FindAnyObjectByType<KapıSeçme>();
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

        // SCRİPT KAPATMA KISMI
        Kamera kamerascript = FindAnyObjectByType<Kamera>();
        kamerascript.enabled = true;
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
        Kamera kamerascript = FindAnyObjectByType<Kamera>();
        kamerascript.enabled = false;
    }

}
