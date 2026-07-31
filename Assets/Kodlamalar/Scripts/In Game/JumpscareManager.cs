using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class JumpscareManager : MonoBehaviour
{
    [Header("Jumpscare Görseli")]
    public GameObject jumpscareHand;   // Karanlıktan fırlayan el objesi

    [Header("Ses Efektleri")]
    public AudioSource sfxSource;
    public AudioClip jumpscareSound;   // Jumpscare anı sesi
    public AudioClip ambientDrone;     // Gerilim sesi (opsiyonel)
    public AudioClip heartbeatSound;   // Kalp atışı sesi (opsiyonel)

    [Header("Post Processing")]
    public Volume globalVolume;
    private ColorAdjustments colorAdjust;
    private Vignette vignetteEffect;
    private ChromaticAberration chromaticAberration;

    [Header("Kamera Efektleri")]
    public Camera mainCamera;
    public float shakeIntensity = 0.5f;
    public float shakeDuration = 0.5f;

    [Header("Zamanlama")]
    public float preJumpscareDelay = 1.5f;    // Jumpscare öncesi gerilim süresi
    public float jumpscareFreezeTime = 0.1f;  // Ekran donma süresi
    public float jumpscareDuration = 2f;      // El ekranda kalma süresi
    public float postJumpscareDelay = 1f;     // Sonrasında bekleme

    [Header("Efekt Ayarları")]
    public float targetExposure = -0.6f; // Artık zifiri karanlık DEĞİL. Göz gözü görecek.
    public float targetVignette = 0.45f;
    public float targetChromatic = 1f;
    public float fogDensity = 0.25f; // Sis azaltıldı

    private Vector3 originalCameraPosition;
    private bool isJumpscareActive = false;

    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        originalCameraPosition = mainCamera.transform.localPosition;

        if (globalVolume != null)
        {
            globalVolume.profile.TryGet(out colorAdjust);
            globalVolume.profile.TryGet(out vignetteEffect);
            globalVolume.profile.TryGet(out chromaticAberration);
        }
        
        // Başlangıçta eli gizle
        if (jumpscareHand != null) jumpscareHand.SetActive(false);
    }

    public void TriggerJumpscare()
    {
        if (!isJumpscareActive)
        {
            StartCoroutine(JumpscareSequence());
        }
    }

    IEnumerator JumpscareSequence()
    {
        isJumpscareActive = true;

        // 1. ADIM: Gerilim sesi başlat (varsa)
        if (sfxSource != null && ambientDrone != null)
        {
            sfxSource.clip = ambientDrone;
            sfxSource.loop = true;
            sfxSource.volume = 0.3f;
            sfxSource.Play();
        }

        // 2. ADIM: Ekranı yavaşça karart (Artık çok kararmayacak)
        StartCoroutine(GradualDarkening());

        // 3. ADIM: Kalp atışı sesi (varsa)
        if (sfxSource != null && heartbeatSound != null)
        {
            yield return new WaitForSeconds(0.5f);
            sfxSource.PlayOneShot(heartbeatSound);
        }

        // 4. ADIM: Jumpscare öncesi bekleme
        yield return new WaitForSeconds(preJumpscareDelay);

        // 5. ADIM: Kısa ekran dondurma
        Time.timeScale = 0.01f;
        yield return new WaitForSecondsRealtime(jumpscareFreezeTime);
        Time.timeScale = 1f;

        // 6. ADIM: Kamera sarsıntısı
        StartCoroutine(CameraShake());

        // 7. ADIM: Jumpscare sesi
        if (sfxSource != null && jumpscareSound != null)
        {
            sfxSource.Stop();
            sfxSource.PlayOneShot(jumpscareSound);
        }

        // 8. ADIM: Eli göster ve KODLA HAREKET ETTİR!
        if (jumpscareHand != null)
        {
            jumpscareHand.SetActive(true);
            
            // DİNAMİK KONUM: Kamera kapıya zaten yanaştı. 
            // El kameranın baktığı yönün 6 metre uzağında (kapının hemen arkasında) doğacak.
            Vector3 startPos = mainCamera.transform.position + mainCamera.transform.forward * 6f + Vector3.down * 0.5f;
            
            // Bitiş noktası: Kameranın tam 1.3 metre önü
            Vector3 endPos = mainCamera.transform.position + mainCamera.transform.forward * 1.3f + Vector3.down * 0.5f;
            
            // Işınlanma yerine 1.2 saniyelik belirgin bir hareket!
            StartCoroutine(MoveHand(startPos, endPos, 1.2f)); 
        }

        // 9. ADIM: Post Processing efektlerini yoğunlaştır
        StartCoroutine(IntensifyEffects());

        // 10. ADIM: Jumpscare süresi bekle
        yield return new WaitForSeconds(jumpscareDuration);

        // 11. ADIM: Son bekleme ve menüye dön
        yield return new WaitForSeconds(postJumpscareDelay);
        ResetAndGoMenu();
    }

    IEnumerator MoveHand(Vector3 startPos, Vector3 endPos, float duration)
    {
        float elapsed = 0f;
        jumpscareHand.transform.position = startPos;
        
        // El, kameraya doğru baksın
        jumpscareHand.transform.rotation = Quaternion.LookRotation(mainCamera.transform.position - startPos);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // SmoothStep: Yavaş başlar, hızlanır ve bitişte hafifçe yavaşlar. Daha doğal görünür.
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            
            jumpscareHand.transform.position = Vector3.Lerp(startPos, endPos, smoothT);
            yield return null;
        }
        
        jumpscareHand.transform.position = endPos;
    }

    IEnumerator GradualDarkening()
    {
        float elapsed = 0f;
        while (elapsed < preJumpscareDelay)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / preJumpscareDelay;

            if (colorAdjust != null)
                colorAdjust.postExposure.value = Mathf.Lerp(0f, targetExposure, t);

            if (vignetteEffect != null)
                vignetteEffect.intensity.value = Mathf.Lerp(0f, targetVignette, t);

            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = Mathf.Lerp(0f, fogDensity, t);
            RenderSettings.fogColor = new Color(0.02f, 0.02f, 0.02f);

            yield return null;
        }
    }

    IEnumerator IntensifyEffects()
    {
        float elapsed = 0f;
        float duration = 0.3f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            if (chromaticAberration != null)
                chromaticAberration.intensity.value = Mathf.Lerp(0f, targetChromatic, t);

            yield return null;
        }
    }

    IEnumerator CameraShake()
    {
        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / shakeDuration;
            float currentIntensity = shakeIntensity * (1f - t);
            float x = Random.Range(-1f, 1f) * currentIntensity;
            float y = Random.Range(-1f, 1f) * currentIntensity;
            mainCamera.transform.localPosition = originalCameraPosition + new Vector3(x, y, 0f);
            yield return null;
        }
        mainCamera.transform.localPosition = originalCameraPosition;
    }

    void ResetAndGoMenu()
    {
        Time.timeScale = 1f;

        if (colorAdjust != null)
            colorAdjust.postExposure.value = 0f;

        if (vignetteEffect != null)
            vignetteEffect.intensity.value = 0f;

        if (chromaticAberration != null)
            chromaticAberration.intensity.value = 0f;

        RenderSettings.fog = false;

        if (jumpscareHand != null)
            jumpscareHand.SetActive(false);

        SceneManager.LoadScene(0);
    }
}