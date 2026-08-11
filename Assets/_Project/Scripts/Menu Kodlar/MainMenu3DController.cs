using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

/// <summary>
/// 3D Dev Kapılı Menü Yöneticisi.
/// Raycast ile fare altındaki kapıyı algılar, hover/click olaylarını iletir.
/// Ana Kapılar ↔ Ayarlar Kapıları arasında kamerayı yumuşakça kaydırır.
/// </summary>
public class MainMenu3DController : MonoBehaviour
{
    [Header("Kamera")]
    public Camera mainCamera;

    [Header("Ana Görünüm Konumu")]
    public Vector3 mainViewPos = new Vector3(0f, 0.6f, 0f);
    public Vector3 mainViewRot = new Vector3(-10f, 0f, 0f);

    [Header("Ayarlar Görünüm Konumu")]
    public Vector3 settingsViewPos = new Vector3(0f, 0.6f, 15f);
    public Vector3 settingsViewRot = new Vector3(-10f, 0f, 0f);

    [Header("Kapı Grupları")]
    public GameObject mainDoorsGroup;
    public GameObject settingsDoorsGroup;

    [Header("Ayar Panelleri (Opsiyonel)")]
    public GameObject graphicsPanel;
    public GameObject audioPanel;
    public GameObject languagePanel;

    [Header("Sahne Geçişi")]
    public SceneTransition sceneTransition;

    [Header("Kamera Geçiş Hızı")]
    public float cameraLerpSpeed = 2.5f;

    // ─── İç Durum ───
    private Menu3DDoor _currentHover;
    private Coroutine _cameraRoutine;

    // ════════════════════════════════════════════

    private void Start()
    {
        // Fare imlecini menü etkileşimi için serbest ve görünür yap
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (sceneTransition == null)
            sceneTransition = FindFirstObjectByType<SceneTransition>();

        // Başlangıçta ana kapılar görünsün
        ShowMainDoors();
    }

    private void Update()
    {
        HandleRaycast();
    }

    private void HandleRaycast()
    {
        if (mainCamera == null) return;

        // Ayarlar UI paneli ekranda AÇIK ise 3D kapı etkileşimini durdur, kapalıysa kapılar sorunsuz çalışsın!
        bool isAnyPanelOpen = (graphicsPanel != null && graphicsPanel.activeSelf) ||
                              (audioPanel != null && audioPanel.activeSelf) ||
                              (languagePanel != null && languagePanel.activeSelf);

        if (isAnyPanelOpen)
        {
            if (_currentHover != null)
            {
                _currentHover.HoverExit();
                _currentHover = null;
            }
            return;
        }

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 50f))
        {
            // Collider'ın kendisinde veya parent'ında Menu3DDoor ara
            Menu3DDoor door = hit.collider.GetComponent<Menu3DDoor>();
            if (door == null)
                door = hit.collider.GetComponentInParent<Menu3DDoor>();

            if (door != null)
            {
                // Yeni kapıya geçiş
                if (_currentHover != door)
                {
                    if (_currentHover != null) _currentHover.HoverExit();
                    _currentHover = door;
                    _currentHover.HoverEnter();
                }

                // Sol tık
                if (Input.GetMouseButtonDown(0))
                {
                    _currentHover.Click();
                    OnDoorClicked(_currentHover);
                }
                return;
            }
        }

        // Hiçbir kapının üzerinde değilsek mevcut hover'ı kapat
        if (_currentHover != null)
        {
            _currentHover.HoverExit();
            _currentHover = null;
        }
    }

    // ─── Kapı Tıklama Yönlendirmesi ───

    public void OnDoorClicked(Menu3DDoor door)
    {
        switch (door.doorType)
        {
            case Menu3DDoor.DoorType.NewGame:
                StartGame();
                break;

            case Menu3DDoor.DoorType.Settings:
                ShowSettingsDoors();
                break;

            case Menu3DDoor.DoorType.Quit:
                QuitGame();
                break;

            case Menu3DDoor.DoorType.SubGeneral:
            case Menu3DDoor.DoorType.SubGraphics:
            case Menu3DDoor.DoorType.SubAudio:
                OpenPanel(graphicsPanel);
                break;

            case Menu3DDoor.DoorType.SubLanguage:
                if (languagePanel != null)
                {
                    OpenPanel(languagePanel);
                }
                else if (LanguageManager.instance != null)
                {
                    LanguageManager.instance.NextLanguage();
                }
                break;

            case Menu3DDoor.DoorType.BackToMain:
                ShowMainDoors();
                break;
        }
    }

    // ─── Görünüm Geçişleri ───

    public void ShowMainDoors()
    {
        if (mainDoorsGroup != null) mainDoorsGroup.SetActive(true);
        if (settingsDoorsGroup != null) settingsDoorsGroup.SetActive(false);
        CloseAllPanels();
        MoveCameraTo(mainViewPos, mainViewRot);
    }

    public void ShowSettingsDoors()
    {
        if (mainDoorsGroup != null) mainDoorsGroup.SetActive(false);
        if (settingsDoorsGroup != null) settingsDoorsGroup.SetActive(true);
        CloseAllPanels();
        MoveCameraTo(settingsViewPos, settingsViewRot);
    }

    // ─── Oyun Başlat / Çıkış ───

    private void StartGame()
    {
        if (sceneTransition != null)
        {
            sceneTransition.ButonlaSahneyeGit(1);
        }
        else
        {
            SceneManager.LoadScene(1);
        }
    }

    private void QuitGame()
    {
        Debug.Log("[DINK] Oyundan çıkılıyor...");
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    // ─── Panel Yönetimi ───

    private void OpenPanel(GameObject panel)
    {
        CloseAllPanels();

        if (panel == null)
        {
            if (SettingsManager.instance != null)
            {
                panel = SettingsManager.instance.gameObject;
            }
            else
            {
                SettingsManager sm = FindAnyObjectByType<SettingsManager>(FindObjectsInactive.Include);
                if (sm != null) panel = sm.gameObject;
            }
        }

        if (panel != null)
        {
            panel.SetActive(true);
            if (SettingsManager.instance != null)
            {
                SettingsManager.instance.LoadAndApplySettings();
            }
        }
    }

    private void CloseAllPanels()
    {
        if (graphicsPanel != null) graphicsPanel.SetActive(false);
        if (audioPanel != null) audioPanel.SetActive(false);
        if (languagePanel != null) languagePanel.SetActive(false);

        if (SettingsManager.instance != null)
        {
            SettingsManager.instance.gameObject.SetActive(false);
        }
        else
        {
            SettingsManager sm = FindAnyObjectByType<SettingsManager>(FindObjectsInactive.Include);
            if (sm != null) sm.gameObject.SetActive(false);
        }
    }

    // ─── Kamera Yumuşak Geçişi ───

    private void MoveCameraTo(Vector3 pos, Vector3 rot)
    {
        if (mainCamera == null) return;
        if (_cameraRoutine != null) StopCoroutine(_cameraRoutine);
        _cameraRoutine = StartCoroutine(LerpCamera(pos, Quaternion.Euler(rot)));
    }

    private IEnumerator LerpCamera(Vector3 targetPos, Quaternion targetRot)
    {
        float t = 0f;
        Vector3 startPos = mainCamera.transform.position;
        Quaternion startRot = mainCamera.transform.rotation;

        while (t < 1f)
        {
            t += Time.deltaTime * cameraLerpSpeed;
            mainCamera.transform.position = Vector3.Lerp(startPos, targetPos, t);
            mainCamera.transform.rotation = Quaternion.Slerp(startRot, targetRot, t);
            yield return null;
        }

        mainCamera.transform.position = targetPos;
        mainCamera.transform.rotation = targetRot;
    }
}
