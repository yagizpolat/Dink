using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Kapı Seçim Dizisi & Sahne Geçiş Yöneticisi (DoorSequenceManager).
/// Oyuncu kapıyı seçtiğinde oyunu dondurur, doğru kapıda RoomGraphManager ile sonraki odaya geçer,
/// yanlış kapıda Jumpscare ve Zindan sürecini başlatır.
/// </summary>
public class DoorSequenceManager : MonoBehaviour
{
    [Header("Doğru Kapı Geçiş Ayarları")]
    [SerializeField] private CanvasGroup fadeGroup;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private JumpscareController jumpscareController;

    private bool choiceMade;

    private Kamera kamera;
    private Escmenu escMenu;
    private temaskontrol temasKontrol;
    private GameObject demoCompletedPanel;
    private Button mainMenuButton;
    private Button replayButton;

    private void Awake()
    {
        kamera = FindAnyObjectByType<Kamera>();
        escMenu = FindAnyObjectByType<Escmenu>();
        temasKontrol = FindAnyObjectByType<temaskontrol>();
    }

    /// <summary>
    /// Kapı seçildiğinde çağrılır.
    /// </summary>
    public void HandleDoorSelected(bool correctDoor, bool isLeftDoor = true)
    {
        if (choiceMade)
        {
            return;
        }

        choiceMade = true;
        DisableGameplayScripts();

        // Speedrunner Motoruna Oda Çıkışını Bildir
        if (SpeedrunnerDetector.instance != null)
        {
            SpeedrunnerDetector.instance.OnRoomExited();
        }

        if (correctDoor)
        {
            HandleCorrectDoor(isLeftDoor);
        }
        else
        {
            HandleWrongDoor();
        }
    }

    private void DisableGameplayScripts()
    {
        SetScriptEnabled(kamera, false);
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

    private void HandleCorrectDoor(bool isLeftDoor)
    {
        Debug.Log("Doğru Kapı seçildi. Sonraki bölüme geçiliyor.");
        StartCoroutine(LoadNextScene(isLeftDoor));
    }

    private IEnumerator LoadNextScene(bool isLeftDoor)
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
            yield return new WaitForSeconds(0.5f);
        }

        // RoomGraphManager varsa oda ağı üzerinden sonraki düğüme geç
        if (RoomGraphManager.instance != null)
        {
            RoomGraphManager.instance.NavigateToNextRoom(isLeftDoor);
        }
        else
        {
            // Fallback: Sıradaki Build Index
            int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;
            if (nextIndex < SceneManager.sceneCountInBuildSettings)
            {
                SceneManager.LoadScene(nextIndex);
            }
            else
            {
                ShowDemoCompletedPanel();
            }
        }
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
        Debug.Log("Yanlış Kapı seçildi. Jumpscare ve Zindan süreci başlatılıyor.");

        if (jumpscareController != null)
        {
            jumpscareController.Play();
        }
        else
        {
            Debug.LogWarning("JumpscareController bağlı değil.");
        }
    }

    private bool ShowDemoCompletedPanel()
    {
        float elapsedTime = LeaderboardManager.instance != null ? LeaderboardManager.instance.StopTimerAndSaveScore() : 0f;
        if (LeaderboardManager.instance != null)
        {
            LeaderboardManager.instance.ShowLeaderboardUIPanel(elapsedTime);
        }

        if (demoCompletedPanel == null)
        {
            Canvas canvas = fadeGroup != null ? fadeGroup.GetComponent<Canvas>() : null;
            if (canvas == null && fadeGroup != null) canvas = fadeGroup.GetComponentInParent<Canvas>();
            if (canvas == null) canvas = Object.FindAnyObjectByType<Canvas>();

            if (canvas == null) return false;

            demoCompletedPanel = new GameObject("Demo-Tamamlandi-Panel");
            demoCompletedPanel.transform.SetParent(canvas.transform, false);
            Image background = demoCompletedPanel.AddComponent<Image>();
            background.color = new Color(0f, 0f, 0f, 0.94f);

            RectTransform panelRect = demoCompletedPanel.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
        }

        demoCompletedPanel.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        return true;
    }
}
