using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LetterManager : MonoBehaviour
{
    public GameObject letterpanel;
    public TMP_Text titleText;
    public TMP_Text contentText;
    [SerializeField] private Escmenu escmenu;
    public AudioSource sesbombasi;
    public AudioClip mektupacilis;
    public AudioClip mektupkapanis;

    //SCRİPT BAĞLANTILARI
    FenerKontrol fener;
    Kamera kamerascript;
    InventoryManager inventory;

    private void Start()
    {
        ScriptConnect();
    }

    private void Update()
    {
        if(letterpanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                CloseLetter();
            }
        }
    }

    public void OpenLetter(Letter letter)
    {
        if (letterpanel.activeSelf)
        {
            return;
        }
        letterpanel.SetActive(true);
        titleText.text = letter.title;
        contentText.text = letter.content;
        escmenu.enabled = false;
        Time.timeScale = 0f;

        //MEKTUP AÇMA SESİ
        sesbombasi.PlayOneShot(mektupacilis);

        //SCRİPT KAPATMA KISMI
        CloseOpenScript(false);
    }

    public void CloseLetter()
    {
        letterpanel.SetActive(false);
        escmenu.enabled = true;
        Time.timeScale = 1f;

        //MEKTUP KAPAMA SESİ
        sesbombasi.PlayOneShot(mektupkapanis);

        //SCRİPT AÇMA KISMI
        CloseOpenScript(true);
    }

    void CloseOpenScript(bool enable)
    {
        fener.enabled = enable;
        kamerascript.enabled = enable;
        inventory.enabled = enable;
    }

    void ScriptConnect()
    {
        fener = FindAnyObjectByType<FenerKontrol>();
        kamerascript = FindAnyObjectByType<Kamera>();
        inventory = FindAnyObjectByType<InventoryManager>();
    }
}
