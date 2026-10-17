using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] private DoorSequenceManager sequenceManager;
    [SerializeField] private bool isLeftDoor = true;
    private bool isCorrectDoor;
    private bool hasBeenSelected;

    public bool IsLeftDoor => isLeftDoor;

    private void Start()
    {
        AutoDetectDoorPosition();
        if (sequenceManager == null)
        {
            sequenceManager = FindAnyObjectByType<DoorSequenceManager>();
        }
    }

    private void AutoDetectDoorPosition()
    {
        string objName = gameObject.name.ToLower();
        if (objName.Contains("left") || objName.Contains("sol") || objName.Contains("l_"))
        {
            isLeftDoor = true;
        }
        else if (objName.Contains("right") || objName.Contains("sag") || objName.Contains("r_"))
        {
            isLeftDoor = false;
        }
    }

    public void Interact()
    {
        // Aynı kapının birden fazla kez seçilmesini ve akışın tekrarlanmasını engeller.
        if (hasBeenSelected)
        {
            return;
        }

        hasBeenSelected = true;

        if (sequenceManager == null)
        {
            sequenceManager = FindAnyObjectByType<DoorSequenceManager>();
        }

        if (sequenceManager != null)
        {
            sequenceManager.HandleDoorSelected(isCorrectDoor, isLeftDoor);
            return;
        }
    }

    public void SetCorrectDoor(bool value)
    {
        isCorrectDoor = value;
        hasBeenSelected = false;
    }
}

