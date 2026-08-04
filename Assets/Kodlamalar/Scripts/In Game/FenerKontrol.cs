using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class FenerKontrol : MonoBehaviour
{

    public Light fenerisik;
    public AudioClip fenerses;
    public AudioSource fenersesbilesen;
    [SerializeField] private float battery = 100f;
    [SerializeField] private float tuketmehizi = 80f;
    public GameObject PilUyarısı;
    private InventorySlot selectedSlot;

    // Update is called once per frame
    void Update()
    {
        bataryatukenis();
        feneracilis();
        
    }

    public void bataryaekle(float miktar)
    {
        if(battery == 100)
        {
            StartCoroutine(warning());
            
            return;
        }
        else
        {
            battery += miktar;
            if (battery > 100)
            {
                battery = 100;
            }
            selectedSlot.icon.texture = null;
        }
    }

    IEnumerator warning()
    {
        PilUyarısı.SetActive(true);
        yield return new WaitForSeconds(2);
        PilUyarısı.SetActive(false);
    }

    void bataryatukenis()
    {
        if (fenerisik.enabled == true)
        {
            battery -= tuketmehizi * Time.deltaTime;
            Debug.Log(battery);
            if (battery < 0)
            {
                fenerisik.enabled = false;
            }
        }
    }

    void feneracilis()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (battery > 0)
            {
                fenerisik.enabled = !fenerisik.enabled;
                fenersesbilesen.PlayOneShot(fenerses);
            }
        }
    }
}
