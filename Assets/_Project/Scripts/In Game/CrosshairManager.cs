using UnityEngine;
using UnityEngine.UI;

public class CrosshairManager : MonoBehaviour
{
    [SerializeField] private Camera playercamera;
    [SerializeField] private RectTransform crosshair;
    [SerializeField] private Vector3 normalscale = Vector3.one;
    [SerializeField] private Vector3 hoverScale = new Vector3(2f, 2f, 2f);
    [SerializeField] private float scalespeed = 8f;

    private void Awake()
    {
        // Crosshair'ın fareyi takip ederken altındaki UI tıklamalarını engellemesini engelle (raycastTarget = false)
        KapatsanaRaycastTarget();
    }

    private void OnEnable()
    {
        KapatsanaRaycastTarget();
    }

    private void KapatsanaRaycastTarget()
    {
        if (crosshair != null)
        {
            foreach (Graphic g in crosshair.GetComponentsInChildren<Graphic>(true))
            {
                g.raycastTarget = false;
            }
        }
    }

    void Update()
    {
        Vector3 targetscale = normalscale;

        // İmleç kilitli değilse (UI / Zindan modu), oyun içi crosshair fareyi takip eder
        if (Cursor.lockState != CursorLockMode.Locked && crosshair != null)
        {
            crosshair.position = Input.mousePosition;
        }

        Vector3 screenPoint = Cursor.lockState == CursorLockMode.Locked
            ? new Vector3(Screen.width / 2f, Screen.height / 2f)
            : Input.mousePosition;

        if (playercamera != null)
        {
            Ray ray = playercamera.ScreenPointToRay(screenPoint);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.CompareTag("Mektup") || hit.collider.CompareTag("Pil") || hit.collider.CompareTag("Kapi"))
                {
                    targetscale = hoverScale;
                }
            }
        }

        if (crosshair != null)
        {
            crosshair.localScale = Vector3.Lerp(crosshair.localScale, targetscale, Time.deltaTime * scalespeed);
        }
    }

    public void SetCrosshairVisible(bool isVisible)
    {
        if (crosshair != null)
        {
            crosshair.gameObject.SetActive(isVisible);
        }
    }
}
