using UnityEngine;

public class DoorChoice : MonoBehaviour
{

    [SerializeField] private Door leftDoor;
    [SerializeField] private Door rightDoor;
    private int randomDoor;

    void Start()
    {
        randomDoor = Random.Range(0, 2);
        if (randomDoor == 0)
        {
            Debug.Log("Sol Kapı Doğru");
            if(leftDoor && rightDoor != null)
            {

                leftDoor.SetCorrectDoor(true);
                rightDoor.SetCorrectDoor(false);
            }
        }
        else
        {
            Debug.Log("Sağ Kapı Doğru");
            if (leftDoor && rightDoor != null)
            {

                leftDoor.SetCorrectDoor(false);
                rightDoor.SetCorrectDoor(true);
            }
        }
    }
}
