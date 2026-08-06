using UnityEngine;
using System.Collections;

public class FlickerLight : MonoBehaviour
{
    private Light targetLight;
    private float baseIntensity;

    public float minIntensity = 0.1f;
    public float maxIntensity = 1.2f;
    public float minFlickerDelay = 0.05f;
    public float maxFlickerDelay = 0.3f;

    void Start()
    {
        targetLight = GetComponent<Light>();
        if (targetLight != null)
        {
            baseIntensity = targetLight.intensity;
            StartCoroutine(FlickerRoutine());
        }
    }

    IEnumerator FlickerRoutine()
    {
        while (true)
        {
            // %10 ihtimalle tamamen sönme (büyük arıza)
            if (Random.value < 0.1f)
            {
                targetLight.intensity = 0;
                yield return new WaitForSeconds(Random.Range(0.1f, 0.4f));
            }
            else
            {
                // Rastgele titreşim
                targetLight.intensity = Random.Range(minIntensity, maxIntensity) * baseIntensity;
                yield return new WaitForSeconds(Random.Range(minFlickerDelay, maxFlickerDelay));
            }
        }
    }
}
