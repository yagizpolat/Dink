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

    [Header("Sonuç")]
    [SerializeField] private int mainMenuBuildIndex = 0;

    public void Play()
    {
        StartCoroutine(PlayRoutine());
    }

    private IEnumerator PlayRoutine()
    {
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

        if (mainMenuBuildIndex < 0 ||
            mainMenuBuildIndex >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogError($"Geçersiz ana menü Build Index'i: {mainMenuBuildIndex}");
            yield break;
        }

        SceneManager.LoadScene(mainMenuBuildIndex);
    }
}
