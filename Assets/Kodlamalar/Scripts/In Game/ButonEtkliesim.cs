using UnityEngine;
using UnityEngine.SceneManagement;

public class ButonEtkliesim : MonoBehaviour
{

    public void sahnedegis(int sahneno)
    {
        SceneManager.LoadScene(sahneno);
    }
}
