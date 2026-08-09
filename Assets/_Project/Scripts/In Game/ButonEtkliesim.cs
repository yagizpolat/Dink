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

    public void Quit()
    {
        Application.Quit();
    }
}
