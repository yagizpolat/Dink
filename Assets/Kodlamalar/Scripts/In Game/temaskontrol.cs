using UnityEngine;
using UnityEngine.EventSystems;

public class temaskontrol : MonoBehaviour
{
    public Camera oyuncukamera;
    public LetterManager lettermanager;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = oyuncukamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));

            RaycastHit hit;

            if(Physics.Raycast(ray, out hit))
            {
                if (hit.collider.CompareTag("Mektup"))
                {
                    Letter letter = hit.collider.GetComponent<Letter>();
                    if(letter != null)
                    {
                        lettermanager.OpenLetter(letter);
                    }
                }

                if (hit.collider.CompareTag("Pil"))
                {
                    BatteryPickup pil = hit.collider.GetComponent<BatteryPickup>();
                    if(pil != null)
                    {
                        pil.Pickup();
                    }
                }
            }
        }
    }
}
