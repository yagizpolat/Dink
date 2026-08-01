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
        Kamera kamerascript = FindAnyObjectByType<Kamera>();
        kamerascript.enabled = false;

        FenerKontrol fener = FindAnyObjectByType<FenerKontrol>();
        fener.enabled = false;

    }

    public void CloseLetter()
    {
        letterpanel.SetActive(false);
        escmenu.enabled = true;
        Time.timeScale = 1f;

        //MEKTUP KAPAMA SESİ
        sesbombasi.PlayOneShot(mektupkapanis);

        //SCRİPT AÇMA KISMI
        Kamera kamerascript = FindAnyObjectByType<Kamera>();
        kamerascript.enabled = true;

        FenerKontrol fener = FindAnyObjectByType<FenerKontrol>();
        fener.enabled = true;
    }
}
