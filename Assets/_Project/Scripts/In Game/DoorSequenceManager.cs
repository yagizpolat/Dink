using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
    private GameObject demoCompletedPanel;
    private Button mainMenuButton;
    private Button replayButton;
    private int fallbackClickFrame = -1;

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

        // Sonraki bölüm yoksa geçiş/fade başlatma; demo sahnesinde oyuncuyu siyah ekranda bırakma.
        // Geçiş yöneticisi korunur ve bölüm listesi hazır olduğunda mevcut akış yeniden çalışır.
        if (correctDoor && levelProgressionManager != null &&
            !levelProgressionManager.TryGetNextLevel(out _))
        {
            choiceMade = true;
            DisableGameplayScripts();

            float elapsedTime = LeaderboardManager.instance != null ? LeaderboardManager.instance.StopTimerAndSaveScore() : 0f;
            if (LeaderboardManager.instance != null)
            {
                LeaderboardManager.instance.ShowLeaderboardUIPanel(elapsedTime);
                return;
            }

            if (!ShowDemoCompletedPanel())
            {
                // UI bulunamazsa oyuncuyu kalıcı olarak kilitleme; mevcut gameplay'i geri aç.
                SetScriptEnabled(kamera, true);
                SetScriptEnabled(fener, true);
                SetScriptEnabled(inventory, true);
                SetScriptEnabled(escMenu, true);
                SetScriptEnabled(temasKontrol, true);
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
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

    private void Update()
    {
        if (demoCompletedPanel == null || !demoCompletedPanel.activeInHierarchy || !Input.GetMouseButtonDown(0))
        {
            return;
        }

        if (fallbackClickFrame == Time.frameCount)
        {
            return;
        }

        Vector2 pointerPosition = Input.mousePosition;
        if (mainMenuButton != null && IsButtonHit(mainMenuButton, pointerPosition))
        {
            fallbackClickFrame = Time.frameCount;
            mainMenuButton.onClick.Invoke();
        }
        else if (replayButton != null && IsButtonHit(replayButton, pointerPosition))
        {
            fallbackClickFrame = Time.frameCount;
            replayButton.onClick.Invoke();
        }
    }

    private bool IsButtonHit(Button button, Vector2 screenPosition)
    {
        return button != null && button.interactable &&
            RectTransformUtility.RectangleContainsScreenPoint(button.GetComponent<RectTransform>(), screenPosition, null);
    }

    private void SetScriptEnabled(MonoBehaviour script, bool enabled)
    {
        if (script != null)
        {
            script.enabled = enabled;
        }
    }

    private bool ShowDemoCompletedPanel()
    {
        if (demoCompletedPanel == null)
        {
            Canvas canvas = fadeGroup != null ? fadeGroup.GetComponent<Canvas>() : null;
            if (canvas == null && fadeGroup != null)
            {
                canvas = fadeGroup.GetComponentInParent<Canvas>();
            }

            if (canvas == null || !canvas.isActiveAndEnabled)
            {
                Canvas[] canvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                foreach (Canvas candidate in canvases)
                {
                    if (candidate.isActiveAndEnabled && candidate.gameObject.activeInHierarchy)
                    {
                        canvas = candidate;
                        break;
                    }
                }
            }

            if (canvas == null || !canvas.isActiveAndEnabled || !canvas.gameObject.activeInHierarchy)
            {
                Debug.LogError("Demo tamamlandı paneli için aktif Canvas bulunamadı.");
                return false;
            }

            if (canvas.GetComponent<GraphicRaycaster>() == null)
            {
                canvas.gameObject.AddComponent<GraphicRaycaster>();
            }

            if (EventSystem.current == null)
            {
                Debug.LogWarning("EventSystem bulunamadı; UI input sahnede doğrulanmalı.");
            }

            demoCompletedPanel = new GameObject("Demo-Tamamlandi-Panel");
            demoCompletedPanel.transform.SetParent(canvas.transform, false);
            Image background = demoCompletedPanel.AddComponent<Image>();
            background.color = new Color(0f, 0f, 0f, 0.94f);
            background.raycastTarget = true;
            RectTransform panelRect = demoCompletedPanel.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            GameObject titleObject = new GameObject("Baslik");
            titleObject.transform.SetParent(demoCompletedPanel.transform, false);
            Text title = titleObject.AddComponent<Text>();
            title.text = "Demo Tamamlandı";
            title.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            title.fontSize = 42;
            title.alignment = TextAnchor.MiddleCenter;
            title.color = Color.white;
            RectTransform titleRect = title.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.1f, 0.55f);
            titleRect.anchorMax = new Vector2(0.9f, 0.75f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;

            mainMenuButton = CreateDemoButton("Ana Menü", new Vector2(0.25f, 0.25f), () => SceneManager.LoadScene(0));
            replayButton = CreateDemoButton("Yeniden Oyna", new Vector2(0.55f, 0.25f), () => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex));
        }

        demoCompletedPanel.transform.SetAsLastSibling();
        demoCompletedPanel.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Debug.Log("Demo tamamlandı paneli gösterildi.");
        return true;
    }

    private Button CreateDemoButton(string label, Vector2 anchor, UnityEngine.Events.UnityAction action)
    {
        GameObject buttonObject = new GameObject(label);
        buttonObject.transform.SetParent(demoCompletedPanel.transform, false);
        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        Button button = buttonObject.AddComponent<Button>();
        button.interactable = true;
        button.onClick.AddListener(action);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor + new Vector2(0.2f, 0.12f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        GameObject textObject = new GameObject("Metin");
        textObject.transform.SetParent(buttonObject.transform, false);
        Text text = textObject.AddComponent<Text>();
        text.text = label;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 22;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.raycastTarget = false;
        RectTransform textRect = text.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        return button;
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
