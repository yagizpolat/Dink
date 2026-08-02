using UnityEngine;
using UnityEngine.SceneManagement;

public class ButonEtkliesim : MonoBehaviour
{

    public void sahnedegis(int sahneno)
    {
        SceneManager.LoadScene(sahneno);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
