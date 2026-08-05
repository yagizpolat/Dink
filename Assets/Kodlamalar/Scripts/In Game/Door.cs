using UnityEngine;

public class Door : MonoBehaviour
{
    private bool iscorrectdoor;
    public void Interact()
    {
        if (iscorrectdoor)
        {
            Debug.Log("Doğru Kapı");
        }
        else
        {
            Debug.Log("Yanlış Kapı");
        }
    }

    public void SetCorrectDoor(bool value)
    {
        iscorrectdoor = value;
    }
}
