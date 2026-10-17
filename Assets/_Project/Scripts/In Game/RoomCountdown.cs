using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Dinamik Geri Sayım Gerilimi (Room Countdown).
/// İlerleyen odalarda ekranın üstünde geri sayım gösterir; süre bitince yanlış kapı akışı
/// (Jumpscare -> Zindan) tetiklenir. Mektup okurken veya ESC menüsünde (timeScale = 0) durur.
/// </summary>
public class RoomCountdown : MonoBehaviour
{
    public static RoomCountdown instance { get; private set; }

    [Header("Geri Sayım Ayarları")]
    [Tooltip("Sayacın başladığı ilk oda numarası")]
    [SerializeField] private int firstTimedRoom = 8;
    [Tooltip("İlk zamanlı odadaki süre (saniye)")]
    [SerializeField] private float startDuration = 30f;
    [Tooltip("Son odadaki süre (saniye)")]
    [SerializeField] private float endDuration = 20f;
    [SerializeField] private int lastRoom = 15;
    [Tooltip("Bu saniyeden itibaren sayaç kırmızıya döner ve nabız gibi atar")]
    [SerializeField] private float dangerThreshold = 5f;

    private Canvas canvas;
    private TextMeshProUGUI timerText;
    private float remaining;
    private bool running;
    private int lastShownSecond = -1;

    private static readonly Color normalColor = new Color(0.85f, 0.85f, 0.85f, 0.85f);
    private static readonly Color dangerColor = new Color(0.9f, 0.1f, 0.1f, 1f);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void OtomatikOlustur()
    {
        if (instance != null) return;

        GameObject go = new GameObject("[RoomCountdown]");
        go.AddComponent<RoomCountdown>();
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        BuildUI();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Stop();

        int roomIndex = RoomGraphManager.GetRoomIndexFromScene(scene);
        if (roomIndex < firstTimedRoom) return;

        // Tutorial odalarında iki kapı da doğru, sayaç anlamsız
        if (RoomGraphManager.instance != null)
        {
            RoomGraphData data = RoomGraphManager.instance.GetRoomGraphData();
            RoomGraphData.RoomNode node = data != null ? data.GetRoomNode(roomIndex) : null;
            if (node != null && node.isTutorial) return;
        }

        float t = Mathf.InverseLerp(firstTimedRoom, lastRoom, roomIndex);
        StartCountdown(Mathf.Lerp(startDuration, endDuration, t));
    }

    public void StartCountdown(float duration)
    {
        remaining = duration;
        running = true;
        lastShownSecond = -1;
        timerText.color = normalColor;
        canvas.gameObject.SetActive(true);
        UpdateText();
    }

    /// <summary>
    /// Kapı seçildiğinde veya sahne değiştiğinde çağrılır.
    /// </summary>
    public void Stop()
    {
        running = false;
        if (canvas != null) canvas.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!running) return;

        // Scaled deltaTime: mektup paneli / ESC menüsü açıkken (timeScale = 0) süre akmaz
        remaining -= Time.deltaTime;
        if (remaining <= 0f)
        {
            remaining = 0f;
            UpdateText();
            OnTimeUp();
            return;
        }

        UpdateText();
    }

    private void UpdateText()
    {
        int seconds = Mathf.CeilToInt(remaining);
        if (seconds == lastShownSecond) return;
        lastShownSecond = seconds;

        timerText.text = seconds.ToString("00");

        if (remaining <= dangerThreshold && seconds > 0)
        {
            timerText.color = dangerColor;
            Tween.PunchScale(timerText.transform, new Vector3(0.35f, 0.35f, 0f), 0.3f);
        }
    }

    private void OnTimeUp()
    {
        Stop();
        Debug.Log("[RoomCountdown] ⏰ Süre doldu! Yanlış kapı akışı tetikleniyor.");

        DoorSequenceManager sequenceManager = FindAnyObjectByType<DoorSequenceManager>();
        if (sequenceManager != null)
        {
            sequenceManager.HandleDoorSelected(false);
        }
    }

    private void BuildUI()
    {
        GameObject canvasObj = new GameObject("CountdownCanvas");
        canvasObj.transform.SetParent(transform, false);
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        GameObject textObj = new GameObject("CountdownText");
        textObj.transform.SetParent(canvasObj.transform, false);
        RectTransform rect = textObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, -40f);
        rect.sizeDelta = new Vector2(300f, 120f);

        timerText = textObj.AddComponent<TextMeshProUGUI>();
        timerText.fontSize = 72;
        timerText.fontStyle = FontStyles.Bold;
        timerText.alignment = TextAlignmentOptions.Center;
        timerText.color = normalColor;
        timerText.raycastTarget = false;

        canvasObj.SetActive(false);
    }
}
