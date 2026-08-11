using UnityEngine;
using UnityEngine.SceneManagement;

public class ButonEtkliesim : MonoBehaviour
{
    public void sahnedegis(int sahneno)
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene(sahneno);
    }

    public void AyarlariAc()
    {
        Escmenu escScript = FindAnyObjectByType<Escmenu>();
        if (escScript != null)
        {
            escScript.AyarlariAc();
        }
        else if (SettingsManager.instance != null)
        {
            SettingsManager.instance.OpenSettings();
        }
    }

    public void Quit()
    {
        Application.Quit();
    }
}
