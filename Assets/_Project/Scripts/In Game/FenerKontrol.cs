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

    private float originalIntensity;
    private float flickerTimer;
    private float nextFlickerTime;

    void Start()
    {
        if (fenerisik != null)
        {
            originalIntensity = fenerisik.intensity;
        }
    }

    // Update is called once per frame
    void Update()
    {
        bataryatukenis();
        feneracilis();
        ApplyFlickerAndDecay();
    }

    public bool bataryaekle(float miktar)
    {
        if(battery == 100)
        {
            StartCoroutine(warning());
            return false;
        }
        else
        {
            battery += miktar;
            if (battery > 100)
            {
                battery = 100;
            }
            return true;
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
                battery = 0;
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

    void ApplyFlickerAndDecay()
    {
        if (fenerisik == null || !fenerisik.enabled) return;

        // 1. Pil Seviyesine Göre Işık Gücünü Azaltma (Decay)
        // Pil %100-50 arasındayken tam güç, %50'nin altına indikçe kademeli olarak zayıflar (min %15 güce kadar)
        float batteryRatio = battery / 100f;
        float targetIntensity = originalIntensity;

        if (batteryRatio < 0.5f)
        {
            // %50'den %0'a indikçe intensity orijinalinin %15'ine kadar düşer
            float t = (batteryRatio) / 0.5f; // 1'den 0'a
            targetIntensity = Mathf.Lerp(originalIntensity * 0.15f, originalIntensity, t);
        }

        // 2. Titreme (Flicker) Mekanizması
        flickerTimer += Time.deltaTime;
        
        if (flickerTimer >= nextFlickerTime)
        {
            flickerTimer = 0f;

            if (batteryRatio <= 0.25f)
            {
                // Pil kritik seviyede (%25 ve altı) -> Sık ve sert titreme
                if (UnityEngine.Random.value < 0.4f)
                {
                    // Işığı anlık olarak neredeyse kapat veya çok kıs
                    fenerisik.intensity = UnityEngine.Random.Range(0f, targetIntensity * 0.3f);
                    nextFlickerTime = UnityEngine.Random.Range(0.05f, 0.15f); // Kısa cızırdama süresi
                }
                else
                {
                    fenerisik.intensity = targetIntensity;
                    nextFlickerTime = UnityEngine.Random.Range(0.1f, 0.5f);
                }
            }
            else
            {
                // Pil normal seviyede -> Çok nadir, atmosferik mikro titreme
                if (UnityEngine.Random.value < 0.02f)
                {
                    fenerisik.intensity = targetIntensity * UnityEngine.Random.Range(0.5f, 0.8f);
                    nextFlickerTime = UnityEngine.Random.Range(0.05f, 0.2f);
                }
                else
                {
                    fenerisik.intensity = targetIntensity;
                    nextFlickerTime = UnityEngine.Random.Range(1f, 5f); // Nadir aralıklar
                }
            }
        }
    }
}
