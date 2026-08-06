using UnityEngine;
using System.Collections;

public class IntroManager : MonoBehaviour
{
    private static bool hasPlayedIntro = false;

    [Header("Canvas Grupları")]
    public CanvasGroup headphoneGroup;
    public CanvasGroup warningGroup;
    public CanvasGroup pressToContinueGroup;
    public CanvasGroup blackFadeGroup;

    [Header("3D Sahne Elemanları")]
    public Transform mainCamera;
    public Transform leftDoor, rightDoor;
    public float targetZ = 2.5f;
    public AudioSource doorCreakSound;
    public GameObject mainMenuUI;

    void Start()
    {
        if (hasPlayedIntro) { SkipIntro(); return; }
        StartIntro();
    }

    void StartIntro()
    {
        hasPlayedIntro = true;

        blackFadeGroup.gameObject.SetActive(true);
        blackFadeGroup.alpha = 1;
        blackFadeGroup.blocksRaycasts = false;
        blackFadeGroup.interactable = false;

        SetGroupHidden(headphoneGroup);
        SetGroupHidden(warningGroup);
        SetGroupHidden(pressToContinueGroup);

        mainMenuUI.SetActive(false);

        StartCoroutine(MasterSequence());
    }

    void SkipIntro()
    {
        // Oyun içinden ana menüye dönüldüğünde gameplay kilitlerini temizle.
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        blackFadeGroup.gameObject.SetActive(true);
        blackFadeGroup.alpha = 0;
        blackFadeGroup.blocksRaycasts = false;
        blackFadeGroup.interactable = false;

        SetGroupHidden(headphoneGroup);
        SetGroupHidden(warningGroup);
        SetGroupHidden(pressToContinueGroup);

        mainMenuUI.SetActive(true);
        //this.gameObject.SetActive(false);
    }

    IEnumerator MasterSequence()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // 1. Kulaklık uyarısı (Hızlandırıldı)
        SetGroupVisible(headphoneGroup);
        yield return StartCoroutine(Fade(headphoneGroup, 1, 1.0f));
        yield return new WaitForSeconds(1.0f);
        yield return StartCoroutine(Fade(headphoneGroup, 0, 1.0f));
        SetGroupHidden(headphoneGroup);

        // 2. Siyah ekran açılır, koridor görünür (Hızlandırıldı)
        yield return StartCoroutine(Fade(blackFadeGroup, 0, 1.8f));

        // 3. Logo + "Tuşa bas" belirir (Hızlandırıldı)
        SetGroupVisible(pressToContinueGroup);
        yield return StartCoroutine(Fade(pressToContinueGroup, 1, 1.2f));

        while (!Input.anyKeyDown) { yield return null; }

        yield return StartCoroutine(Fade(pressToContinueGroup, 0, 0.6f));
        SetGroupHidden(pressToContinueGroup);

        // 4. Kamera ilerler (paralel), ardından siyahlık çöker (Hızlandırıldı)
        StartCoroutine(CameraAndDoorAction());
        yield return new WaitForSeconds(0.2f);
        yield return StartCoroutine(Fade(blackFadeGroup, 1, 1.5f));

        // 5. İçerik uyarısı (Hızlandırıldı)
        SetGroupVisible(warningGroup);
        yield return StartCoroutine(Fade(warningGroup, 1, 1.2f));
        yield return new WaitForSeconds(1.5f);
        yield return StartCoroutine(Fade(warningGroup, 0, 1.2f));
        SetGroupHidden(warningGroup);

        // 6. Ana menü açılır, siyah ekran çekilir (Hızlandırıldı)
        mainMenuUI.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        yield return StartCoroutine(Fade(blackFadeGroup, 0, 1.2f));
    }

    IEnumerator CameraAndDoorAction()
    {
        while (mainCamera.position.z < targetZ)
        {
            mainCamera.Translate(Vector3.forward * 1.5f * Time.deltaTime);
            yield return null;
        }

        if (doorCreakSound != null) doorCreakSound.Play();

        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * 0.15f;
            if (leftDoor != null)
                leftDoor.rotation = Quaternion.Slerp(leftDoor.rotation, Quaternion.Euler(0, 90, 0), t);
            if (rightDoor != null)
                rightDoor.rotation = Quaternion.Slerp(rightDoor.rotation, Quaternion.Euler(0, 90, 0), t);
            yield return null;
        }
    }

    private void SetGroupHidden(CanvasGroup cg)
    {
        cg.alpha = 0;
        cg.blocksRaycasts = false;
        cg.interactable = false;
    }

    private void SetGroupVisible(CanvasGroup cg)
    {
        cg.alpha = 0;
        cg.blocksRaycasts = true;
        cg.interactable = true;
    }

    IEnumerator Fade(CanvasGroup cg, float target, float duration)
    {
        float start = cg.alpha;
        float time = 0;
        while (time < duration)
        {
            time += Time.deltaTime;
            cg.alpha = Mathf.Lerp(start, target, time / duration);
            yield return null;
        }
        cg.alpha = target;
    }
}