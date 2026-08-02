using UnityEngine;

public class CrosshairManager : MonoBehaviour
{
    [SerializeField] private Camera playercamera;
    [SerializeField] private RectTransform crosshair;
    [SerializeField] private Vector3 normalscale = Vector3.one;
    [SerializeField] private Vector3 hoverScale = new Vector3(2f, 2f, 2f);
    [SerializeField] private float scalespeed = 8f;

    // Update is called once per frame
    void Update()
    {
        Vector3 targetscale = normalscale;


        Ray ray = playercamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));

        RaycastHit hit;

        if(Physics.Raycast(ray, out hit))
        {
            if (hit.collider.CompareTag("Mektup"))
            {
                targetscale = hoverScale;
            }

            if (hit.collider.CompareTag("Pil"))
            {
                targetscale = hoverScale;
            }
        }

        crosshair.localScale = Vector3.Lerp(crosshair.localScale, targetscale, Time.deltaTime * scalespeed);
    }
}
