using UnityEngine;

public class FenerKontrol : MonoBehaviour
{

    public Light fenerisik;
    public AudioClip fenerses;
    public AudioSource fenersesbilesen;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            fenerisik.enabled = !fenerisik.enabled;
            fenersesbilesen.PlayOneShot(fenerses);
        }
    }
}
