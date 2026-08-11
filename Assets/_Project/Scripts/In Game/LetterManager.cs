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
        // letterpanel'in null olup olmadığını kontrol ederek her karede (Update) çökmesini engelliyoruz
        if (letterpanel != null && letterpanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                CloseLetter();
            }
        }
    }

    public void OpenLetter(Letter letter)
    {
        if (letterpanel == null)
        {
            Debug.LogWarning("[LetterManager] 'letterpanel' referansı Unity Inspector üzerinde atanmamış!");
            return;
        }

        if (letterpanel.activeSelf)
        {
            return;
        }
        letterpanel.SetActive(true);
        if (titleText != null) titleText.text = letter.GetTitle();
        if (contentText != null) contentText.text = letter.GetContent();
        if (escmenu != null) escmenu.enabled = false;
        Time.timeScale = 0f;

        // MEKTUP AÇMA SESİ
        float sfxVol = AudioManager.instance != null ? AudioManager.instance.GetSFXVolume() : PlayerPrefs.GetFloat("Dink_SFXVolume", 0.8f);
        if (sesbombasi != null && mektupacilis != null)
        {
            sesbombasi.PlayOneShot(mektupacilis, sfxVol);
        }

        // SCRİPT KAPATMA KISMI
        CloseOpenScript(false);
    }

    public void CloseLetter()
    {
        if (letterpanel != null)
        {
            letterpanel.SetActive(false);
        }
        if (escmenu != null) escmenu.enabled = true;
        Time.timeScale = 1f;

        // MEKTUP KAPAMA SESİ
        float sfxVol = AudioManager.instance != null ? AudioManager.instance.GetSFXVolume() : PlayerPrefs.GetFloat("Dink_SFXVolume", 0.8f);
        if (sesbombasi != null && mektupkapanis != null)
        {
            sesbombasi.PlayOneShot(mektupkapanis, sfxVol);
        }

        // SCRİPT AÇMA KISMI
        CloseOpenScript(true);
    }

    void CloseOpenScript(bool enable)
    {
        if (fener != null) fener.enabled = enable;
        if (kamerascript != null) kamerascript.enabled = enable;
        if (inventory != null) inventory.enabled = enable;
    }

    void ScriptConnect()
    {
        fener = FindAnyObjectByType<FenerKontrol>();
        kamerascript = FindAnyObjectByType<Kamera>();
        inventory = FindAnyObjectByType<InventoryManager>();
        if (escmenu == null) escmenu = FindAnyObjectByType<Escmenu>();
    }
}
