using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    [Header("Canvas Grupları")]
    public CanvasGroup globalFadeGroup;
    public CanvasGroup quoteTextGroup;
    public CanvasGroup dateandpressTextGroup;

    [Header("Referanslar")]
    public GameObject mainMenuUI; // Geçiş başlayınca kapatılacak menü

    [Header("Ayarlar")]
    public float fadeDuration = 1.0f;

    // Başlangıç ayarlarını IntroManager yönetiyor, Start() burada yok.

    public void ButonlaSahneyeGit(int sahneNo)
    {
        StartCoroutine(GecisRoutine(sahneNo));
    }

    IEnumerator GecisRoutine(int sahneNo)
    {
        Time.timeScale = 1f;
        Debug.Log("1 - Coroutine başladı");
        // ADIM 1: Önce ekranı karart (fade başla)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (AudioManager.instance != null)
            AudioManager.instance.ChangeMusicWithFade(AudioManager.instance.gameMusic, 2f);

        globalFadeGroup.alpha = 0;
        yield return StartCoroutine(Fade(globalFadeGroup, 1, fadeDuration));
        Debug.Log("2 - Fade bitti");

        // --- EKRAN KAPKARA ---
        // ADIM 0: Ekran tamamen karardıktan sonra menüyü kapat
        if (mainMenuUI != null) mainMenuUI.SetActive(false);

        yield return new WaitForSeconds(0.5f);

        // ADIM 2: Günlük yazısını yavaşça çıkart (blackFade'in üstünde olduğu için görünür)
        quoteTextGroup.alpha = 0;
        dateandpressTextGroup.alpha = 0;
        quoteTextGroup.gameObject.SetActive(true);
        dateandpressTextGroup.gameObject.SetActive(true);
        yield return StartCoroutine(Fade(quoteTextGroup, 1f, fadeDuration));
        yield return StartCoroutine(Fade(dateandpressTextGroup, 1f, fadeDuration));

        // ADIM 3: Tuşa basılmasını bekle
        yield return new WaitForSeconds(0.5f); // Yanlışlıkla basılmayı engelle
        while (!Input.anyKeyDown) { yield return null; }

        // ADIM 4: Yazıyı yavaşça sil (arka siyah kalıyor)
        yield return StartCoroutine(Fade(quoteTextGroup, 0, fadeDuration));
        yield return StartCoroutine(Fade(dateandpressTextGroup, 0, fadeDuration));
        quoteTextGroup.gameObject.SetActive(false);
        dateandpressTextGroup.gameObject.SetActive(false);

        // ADIM 5: Kısa karanlık bekle ve sahneyi yükle
        yield return new WaitForSeconds(0.3f);
        SceneManager.LoadScene(sahneNo);
        Debug.Log("3 - Sahne yükleniyor");
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