using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Oyuncu bu tetikleyiciye girince ekran kararır ve hedef sahne yüklenir.
/// Apartman bölümlerinde kapıdan kapıya (prolog → giriş katı) geçiş için.
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class SceneDoorTrigger : MonoBehaviour
{
    [SerializeField] private string targetScene = "GirisKati";
    [SerializeField] private float fadeSeconds = 1.2f;

    private bool used;

    private void Reset()
    {
        GetComponent<BoxCollider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (used || other.GetComponent<PlayerWalker>() == null) return;
        used = true;
        StartCoroutine(FadeAndLoad(targetScene, fadeSeconds));
    }

    /// <summary>Kontrolleri kapatır, ekranı karartır ve sahneyi yükler. Kat geçişleri de bunu kullanır.</summary>
    public static IEnumerator FadeAndLoad(string targetScene, float fadeSeconds)
    {
        PlayerControls.SetEnabled(false);

        var canvasGo = new GameObject("KapiGecisKarartma");
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;
        var image = new GameObject("Siyah").AddComponent<Image>();
        image.transform.SetParent(canvasGo.transform, false);
        image.rectTransform.anchorMin = Vector2.zero;
        image.rectTransform.anchorMax = Vector2.one;
        image.color = new Color(0f, 0f, 0f, 0f);

        for (float t = 0f; t < fadeSeconds; t += Time.unscaledDeltaTime)
        {
            image.color = new Color(0f, 0f, 0f, t / fadeSeconds);
            yield return null;
        }
        image.color = Color.black;

        SceneManager.LoadScene(targetScene);
    }
}
