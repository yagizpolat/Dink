using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class JumpscareController : MonoBehaviour
{
    [Header("Jumpscare UI")]
    [SerializeField] private GameObject jumpscarePanel;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip jumpscareSound;
    [SerializeField] private float jumpscareDuration = 2f;

    [Header("Görsel Efektler (Ambiyans)")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private FenerKontrol fenerKontrol;
    [SerializeField] private float shakeMagnitude = 0.5f;

    [Header("Sonuç")]
    [SerializeField] private int mainMenuBuildIndex = 0;

    public void Play()
    {
        StartCoroutine(PlayRoutine());
    }

    private IEnumerator PlayRoutine()
    {
        // 1. Feneri Şiddetle Devre Dışı Bırakma
        if (fenerKontrol != null && fenerKontrol.fenerisik != null)
        {
            fenerKontrol.fenerisik.enabled = false;
        }

        // 2. Kamera Sarsıntısı Başlatma
        Vector3 originalCamPos = Vector3.zero;
        if (mainCamera != null)
        {
            originalCamPos = mainCamera.transform.localPosition;
            StartCoroutine(ShakeCamera(originalCamPos));
        }

        // 3. Jumpscare Panel ve Ses Aktifleştirme (Gecikmesiz, Anında)
        if (jumpscarePanel != null)
        {
            jumpscarePanel.SetActive(true);
        }

        if (audioSource != null && jumpscareSound != null)
        {
            audioSource.PlayOneShot(jumpscareSound);
        }

        // Jumpscare'in oyun zamanından bağımsız çalışmasını sağlar.
        yield return new WaitForSecondsRealtime(jumpscareDuration);

        // Kamerayı eski konumuna geri getirme (Sahne geçişi öncesi temizlik)
        if (mainCamera != null)
        {
            mainCamera.transform.localPosition = originalCamPos;
        }

        if (mainMenuBuildIndex < 0 ||
            mainMenuBuildIndex >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogError($"Geçersiz ana menü Build Index'i: {mainMenuBuildIndex}");
            yield break;
        }

        SceneManager.LoadScene(mainMenuBuildIndex);
    }

    private IEnumerator ShakeCamera(Vector3 originalPos)
    {
        float elapsed = 0.0f;

        while (elapsed < jumpscareDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeMagnitude;
            float y = Random.Range(-1f, 1f) * shakeMagnitude;

            if (mainCamera != null)
            {
                mainCamera.transform.localPosition = new Vector3(originalPos.x + x, originalPos.y + y, originalPos.z);
            }

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
    }
}
