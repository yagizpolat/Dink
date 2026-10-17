using UnityEngine;
using UnityEngine.EventSystems;

public class temaskontrol : MonoBehaviour
{
    public Camera oyuncukamera;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = oyuncukamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));

            RaycastHit hit;

            if(Physics.Raycast(ray, out hit))
            {
                if (hit.collider.CompareTag("Kapi"))
                {
                    Door door = hit.collider.GetComponent<Door>();
                    if(door != null)
                    {
                        door.Interact();
                    }
                }

                HintPropComponent hintProp = hit.collider.GetComponentInParent<HintPropComponent>();
                if (hintProp != null)
                {
                    hintProp.Interact();
                }
            }
        }
    }
}
