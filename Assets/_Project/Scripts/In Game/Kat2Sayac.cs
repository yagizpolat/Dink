using System.Collections;
using UnityEngine;

/// <summary>
/// Kat 2, "Sayaç dönüyor": koridor lambaları cızırdayıp göz kırpar, asansör yanındaki açık panoda bir sayacın
/// diski deli gibi döner. Her daire kapısında oyuncunun adına, adresi "Daire 11" olan bir kesme ihbarı bantlı.
/// Biri okununca lambalar asansörden uzaktan başlayarak sırayla söner; geriye asansör ışığı ve sayaç sesi kalır,
/// kabinde 3 yanar. Sesler (cızırtı, sayaç tıkırtısı, lamba sönmesi) burada üretilir.
/// </summary>
public class Kat2Sayac : MonoBehaviour
{
    [SerializeField] private Collider[] notices;
    [SerializeField] private float reach = 1.8f;

    [Header("Sayaç")]
    [SerializeField] private Transform disk;
    [SerializeField] private float diskSpeed = 1400f;
    [SerializeField] private AudioSource meterSource;

    [Header("Lambalar (sönme sırasıyla)")]
    [SerializeField] private Light[] lamps;
    [SerializeField] private AudioSource buzzSource;
    [SerializeField] private AudioSource clunkSource;

    [Header("Asansör")]
    [SerializeField] private GameObject nextFloorLight;
    [SerializeField] private AudioSource nextFloorDing;

    private float[] baseIntensity;
    private bool blackout;
    private Camera cam;

    private void Awake()
    {
        if (nextFloorLight != null) nextFloorLight.SetActive(false);
        baseIntensity = new float[lamps.Length];
        for (int i = 0; i < lamps.Length; i++) baseIntensity[i] = lamps[i].intensity;

        if (meterSource != null)
        {
            // Sayaç tıkırtısı: disk her turda bir tık, altında ince bir vızıltı
            meterSource.clip = Synth(1f, t => (Mathf.Repeat(t, 0.125f) < 0.004f ? 0.5f : 0f) * Mathf.Sin(2f * Mathf.PI * 2400f * t)
                                              + Mathf.Sin(2f * Mathf.PI * 50f * t) * 0.05f);
            meterSource.loop = true;
            meterSource.Play();
        }
        if (buzzSource != null)
        {
            var rng = new System.Random(3);
            buzzSource.clip = Synth(2f, t => Mathf.Sign(Mathf.Sin(2f * Mathf.PI * 100f * t)) * 0.05f + ((float)rng.NextDouble() - 0.5f) * 0.04f);
            buzzSource.loop = true;
            buzzSource.Play();
        }
        if (clunkSource != null)
            clunkSource.clip = Synth(0.5f, t => (Mathf.PerlinNoise(t * 900f, 0.2f) - 0.5f) * Mathf.Exp(-t * 18f) * 1.2f
                                               + Mathf.Sin(2f * Mathf.PI * 70f * t) * Mathf.Exp(-t * 10f) * 0.4f);
        if (nextFloorDing != null && nextFloorDing.clip == null)
            nextFloorDing.clip = Synth(1.4f, t => Mathf.Sin(2f * Mathf.PI * 880f * t) * 0.35f * Mathf.Exp(-t * 2.5f));
    }

    private void Update()
    {
        if (disk != null) disk.Rotate(Vector3.up, diskSpeed * Time.deltaTime, Space.Self);
        if (!blackout) Flicker();

        if (Time.timeScale == 0f) return;
        if (!Input.GetMouseButtonDown(0) && !Input.GetKeyDown(KeyCode.E)) return;
        if (MektupNotHintUI.instance != null && MektupNotHintUI.instance.IsPanelOpen) return;

        if (cam == null) cam = Camera.main;
        if (cam == null) return;
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        foreach (Collider notice in notices)
        {
            if (notice == null || !notice.Raycast(ray, out _, reach)) continue;
            MektupNotHintUI.Instance.OpenText(NoticeText(), true, false);
            if (!blackout) StartCoroutine(Blackout());
            return;
        }
    }

    // Her lamba kendi gürültüsüyle titrer; arada bir kısa sönüp yanar
    private void Flicker()
    {
        for (int i = 0; i < lamps.Length; i++)
        {
            float n = Mathf.PerlinNoise(Time.time * 7f, i * 3.1f);
            float drop = n < 0.18f ? 0.15f : 1f;
            lamps[i].intensity = baseIntensity[i] * drop * (0.9f + 0.1f * Mathf.PerlinNoise(Time.time * 40f, i));
        }
    }

    private IEnumerator Blackout()
    {
        blackout = true;
        while (MektupNotHintUI.instance != null && MektupNotHintUI.instance.IsPanelOpen) yield return null;
        yield return new WaitForSeconds(0.8f);

        for (int i = 0; i < lamps.Length; i++)
        {
            lamps[i].intensity = 0f;
            if (clunkSource != null) { clunkSource.transform.position = lamps[i].transform.position; clunkSource.Play(); }
            if (buzzSource != null) buzzSource.volume *= 0.6f;
            yield return new WaitForSeconds(0.9f);
        }
        if (buzzSource != null) buzzSource.Stop();

        yield return new WaitForSeconds(1.5f);
        diskSpeed *= 1.6f;
        if (meterSource != null) meterSource.pitch = 1.25f;
        ElevatorController.DepartureUnlocked = true;
        if (nextFloorLight != null) nextFloorLight.SetActive(true);
        if (nextFloorDing != null) nextFloorDing.Play();
    }

    private static bool IsTR => LanguageManager.instance != null && LanguageManager.instance.CurrentLanguage == LanguageManager.Language.TR;

    private static string NoticeText()
    {
        string name = LobbyLogbook.SignedName;
        if (string.IsNullOrEmpty(name) && LeaderboardManager.instance != null) name = LeaderboardManager.instance.GetPlayerName();
        if (string.IsNullOrEmpty(name)) name = "Hakan Kaya";
        name = name.ToUpperInvariant();

        return IsTR
            ? "ELEKTRİK KESME İHBARNAMESİ\n--------------------------\nABONE: " + name +
              "\nADRES: KARANFİL APT. DAİRE 11\nSAYAÇ NO: 11111\n\nÖDENMEMİŞ DÖNEM\nKASIM 2023\n\nTÜKETİM        11111 kWh\nBORÇ                  —\n--------------------------\nÖdenmediği takdirde\nenerjiniz kesilecektir.\n\nKESİM: 14.11.23  23:42"
            : "NOTICE OF DISCONNECTION\n--------------------------\nACCOUNT: " + name +
              "\nADDRESS: KARANFIL APT. FLAT 11\nMETER NO: 11111\n\nUNPAID PERIOD\nNOVEMBER 2023\n\nUSAGE          11111 kWh\nOWED                  —\n--------------------------\nIf left unpaid, your\npower will be cut off.\n\nCUT-OFF: 14.11.23  23:42";
    }

    private static AudioClip Synth(float seconds, System.Func<float, float> f)
    {
        const int rate = 44100;
        int n = Mathf.RoundToInt(seconds * rate);
        float[] data = new float[n];
        for (int i = 0; i < n; i++) data[i] = f(i / (float)rate);
        AudioClip clip = AudioClip.Create("Kat2Ses", n, 1, rate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
