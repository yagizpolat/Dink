using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorSequenceManager : MonoBehaviour
{
    [Header("Doğru kapı geçişi")]
    [SerializeField] private int nextSceneBuildIndex = 1;
    [SerializeField] private LevelProgressionManager levelProgressionManager;
    [SerializeField] private CanvasGroup fadeGroup;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private JumpscareController jumpscareController;

    private bool choiceMade;

    private Kamera kamera;
    private FenerKontrol fener;
    private InventoryManager inventory;
    private Escmenu escMenu;
    private temaskontrol temasKontrol;

    private void Awake()
    {
        if (levelProgressionManager == null)
        {
            levelProgressionManager = FindAnyObjectByType<LevelProgressionManager>();
        }

        kamera = FindAnyObjectByType<Kamera>();
        fener = FindAnyObjectByType<FenerKontrol>();
        inventory = FindAnyObjectByType<InventoryManager>();
        escMenu = FindAnyObjectByType<Escmenu>();
        temasKontrol = FindAnyObjectByType<temaskontrol>();
    }

    public void HandleDoorSelected(bool correctDoor)
    {
        if (choiceMade)
        {
            return;
        }

        choiceMade = true;
        DisableGameplayScripts();

        if (correctDoor)
        {
            HandleCorrectDoor();
        }
        else
        {
            HandleWrongDoor();
        }
    }

    private void DisableGameplayScripts()
    {
        // Kapı seçildikten sonra oyuncunun yeni bir seçim veya mekanik başlatmasını engeller.
        SetScriptEnabled(kamera, false);
        SetScriptEnabled(fener, false);
        SetScriptEnabled(inventory, false);
        SetScriptEnabled(escMenu, false);
        SetScriptEnabled(temasKontrol, false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void SetScriptEnabled(MonoBehaviour script, bool enabled)
    {
        if (script != null)
        {
            script.enabled = enabled;
        }
    }

    private void HandleCorrectDoor()
    {
        Debug.Log("Doğru Kapı seçildi. Sonraki bölüme geçiliyor.");
        StartCoroutine(LoadNextScene());
    }

    private IEnumerator LoadNextScene()
    {
        Time.timeScale = 1f;

        if (fadeGroup != null)
        {
            fadeGroup.gameObject.SetActive(true);
            fadeGroup.alpha = 0f;
            yield return StartCoroutine(FadeToBlack());
        }
        else
        {
            // Fade paneli henüz bağlanmadıysa geçiş yine çalışmaya devam eder.
            yield return new WaitForSeconds(0.5f);
        }

        if (levelProgressionManager != null)
        {
            if (!levelProgressionManager.TryGetNextLevel(out nextSceneBuildIndex))
            {
                Debug.Log("Son bölüme ulaşıldı. Final sinematiği sonraki aşamada başlatılacak.");
                yield break;
            }
        }

        if (nextSceneBuildIndex < 0 || nextSceneBuildIndex >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogError($"Geçersiz sahne Build Index'i: {nextSceneBuildIndex}");
            yield break;
        }

        SceneManager.LoadScene(nextSceneBuildIndex);
    }

    private IEnumerator FadeToBlack()
    {
        float startAlpha = fadeGroup.alpha;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            fadeGroup.alpha = Mathf.Lerp(startAlpha, 1f, elapsed / fadeDuration);
            yield return null;
        }

        fadeGroup.alpha = 1f;
    }

    private void HandleWrongDoor()
    {
        Debug.Log("Yanlış Kapı seçildi. Jumpscare başlatılıyor.");

        if (jumpscareController != null)
        {
            jumpscareController.Play();
        }
        else
        {
            Debug.LogWarning("JumpscareController bağlı değil. Jumpscare oynatılamadı.");
        }
    }
}
