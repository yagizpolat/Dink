using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Telsiz Paraziti & Fısıltı (TelsizFisilti) ipucu sistemi.
/// Odada beliren telsiz objesine tıklandığında LED yanıp söner,
/// 3D Spatial Audio ile fısıltı çalar ve altyazı gösterilir.
/// Ses dosyaları yoksa altyazı-only modda çalışır (graceful fallback).
public class TelsizFisiltiController : MonoBehaviour
{
    private static TelsizFisiltiController _instance;
    public static TelsizFisiltiController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<TelsizFisiltiController>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("[TelsizFisiltiController]");
                    _instance = go.AddComponent<TelsizFisiltiController>();
                }
            }
            return _instance;
        }
    }
    public static TelsizFisiltiController instance => Instance;

    private Canvas subtitleCanvas;
    private TextMeshProUGUI subtitleText;
    private Image subtitleBg;
    private bool isRadioActive;
    private Coroutine activeSequence;

    private const float SUBTITLE_DURATION = 5f;
    private const float LED_BLINK_INTERVAL = 0.3f;

    // =========================================================================
    // 10 Fısıltı Replik Havuzu — Türkçe
    // [SOL / SAĞ] etiketleri runtime'da değiştirilir.
    // =========================================================================
    private static readonly string[] whisperPoolTR = new string[]
    {
        // 1 — Çaresiz Son Nefes
        "...Kan sızıyor... beni dinle... sakın durma... [SOL / SAĞ] kapıya yönel...",
        // 2 — Döngüde Çıldıran Biri
        "Duvarlar konuşuyor... fısıltıları duyuyor musun? Yanılma... kurtuluş [SOL / SAĞ] tarafta...",
        // 3 — Gözlerini Kaybetmiş Kurban
        "Karanlık... hiçbir şey göremiyorum... ama hissediyorum... [SOL / SAĞ] kapıdan yürü...",
        // 4 — Zindandaki Kurban
        "Beni aşağıya çekti... zindandayım... oraya düşmek istemiyorsan [SOL / SAĞ] kapıyı aç...",
        // 5 — Donmak Üzere Olan Kurban
        "Burası çok soğuk... nefesim kesiliyor... ısınmak istiyorsan [SOL / SAĞ] kapıdan geç...",
        // 6 — Telsiz Anonsu Yapan Son Görevli
        "Cızıızt... kims... kimse var mı?... Son sinyal... doğru hat... [SOL / SAĞ] kapı...",
        // 7 — Zihni Bulanmış Biri
        "Işıklar... ışıklar beni kandırdı... sen aynı hatayı yapma... [SOL / SAĞ] kapı güvenli...",
        // 8 — Korkudan Sesi Kısılan Genç
        "Arkamda... hemen arkamdalar... çabuk ol... [SOL / SAĞ] kapıyı aç ve kaç...",
        // 9 — Son Telsiz Mesajı
        "Pillerim bitiyor... bu sana son uyarım... sakın şaşırma... [SOL / SAĞ] kapı...",
        // 10 — Döngüden Çıkamamış Ruh
        "Yıllardır buradayım... kapılar birbirine çıkıyor... tek çıkış [SOL / SAĞ] tarafta..."
    };

    // =========================================================================
    // 10 Whisper Subtitle Pool — English
    // [LEFT / RIGHT] tags replaced at runtime.
    // =========================================================================
    private static readonly string[] whisperPoolEN = new string[]
    {
        // 1 — Desperate Last Breath
        "...Blood is seeping... listen to me... don't stop... head to the [LEFT / RIGHT] door...",
        // 2 — Someone Gone Mad in the Loop
        "The walls are talking... can you hear the whispers? Don't be deceived... salvation is on the [LEFT / RIGHT] side...",
        // 3 — Victim Who Lost Their Eyes
        "Darkness... I can't see anything... but I can feel it... walk through the [LEFT / RIGHT] door...",
        // 4 — Victim in the Dungeon
        "It pulled me down... I'm in the dungeon... if you don't want to fall there, open the [LEFT / RIGHT] door...",
        // 5 — Victim About to Freeze
        "It's so cold here... I can't breathe... if you want warmth, go through the [LEFT / RIGHT] door...",
        // 6 — Last Officer's Radio Announcement
        "Crzzzzt... any... anyone there?... Last signal... correct line... [LEFT / RIGHT] door...",
        // 7 — Someone Whose Mind is Clouded
        "The lights... the lights deceived me... don't make the same mistake... the [LEFT / RIGHT] door is safe...",
        // 8 — A Youth Whose Voice Fades from Fear
        "Behind me... right behind me... hurry... open the [LEFT / RIGHT] door and run...",
        // 9 — Final Radio Message
        "My batteries are dying... this is my last warning... don't get confused... [LEFT / RIGHT] door...",
        // 10 — Spirit Trapped in the Loop
        "I've been here for years... the doors lead to each other... the only exit is on the [LEFT / RIGHT] side..."
    };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoCreate()
    {
        if (instance != null) return;
        GameObject go = new GameObject("[TelsizFisiltiController]");
        go.AddComponent<TelsizFisiltiController>();
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
        BuildSubtitleUI();
    }

    private void OnEnable() { UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded; }
    private void OnDisable() { UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded; }

    /// <summary>
    /// Telsiz konuşurken oda değişirse (yanlış kapı -> zindan) altyazı yeni sahnede asılı kalmasın.
    /// </summary>
    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        if (activeSequence != null) StopCoroutine(activeSequence);
        activeSequence = null;
        isRadioActive = false;
        HideSubtitle();
    }

    /// <summary>
    /// Telsiz etkileşimini başlatır. LED yanıp sönme, ses çalma ve altyazı gösterimini tetikler.
    /// </summary>
    /// <param name="shownDirLeft">Gösterilecek yön (Fake Hint hesaplanmış hali)</param>
    /// <param name="radioObject">Sahnedeki telsiz objesi (LED efekti için Renderer aranır)</param>
    public void ActivateRadio(bool shownDirLeft, GameObject radioObject)
    {
        if (isRadioActive) return;

        // 10 replikten rastgele birini seç
        int index = Random.Range(0, whisperPoolTR.Length);

        bool isTR = LanguageManager.instance != null &&
                     LanguageManager.instance.CurrentLanguage == LanguageManager.Language.TR;

        string template = isTR ? whisperPoolTR[index] : whisperPoolEN[index];
        string finalSubtitle = ApplyDirectionTags(template, shownDirLeft, isTR);

        // Sekansı başlat
        if (activeSequence != null) StopCoroutine(activeSequence);
        activeSequence = StartCoroutine(RadioSequence(finalSubtitle, radioObject, index));

        Debug.Log($"[TelsizFisilti] Telsiz aktif! Replik #{index + 1}, Yön: {(shownDirLeft ? "SOL" : "SAĞ")}");
    }

    /// <summary>
    /// Telsiz etkileşim sekansı: LED blink + Ses + Altyazı → Otomatik kapanış.
    /// </summary>
    private IEnumerator RadioSequence(string subtitle, GameObject radioObject, int clipIndex)
    {
        isRadioActive = true;

        // LED yanıp sönme coroutine'ini başlat
        Coroutine ledCoroutine = null;
        Renderer radioRenderer = radioObject != null ? radioObject.GetComponentInChildren<Renderer>() : null;
        if (radioRenderer != null)
        {
            ledCoroutine = StartCoroutine(BlinkLED(radioRenderer, SUBTITLE_DURATION));
        }

        // 3D Spatial Audio kurulumu ve çalma
        AudioSource radioAudio = null;
        if (radioObject != null)
        {
            radioAudio = radioObject.GetComponent<AudioSource>();
            if (radioAudio == null)
            {
                radioAudio = radioObject.AddComponent<AudioSource>();
            }

            // 3D Spatial ayarları: Ses telsizden yayılır, uzaklaştıkça azalır
            radioAudio.spatialBlend = 1.0f;
            radioAudio.minDistance = 1f;
            radioAudio.maxDistance = 8f;
            radioAudio.rolloffMode = AudioRolloffMode.Linear;
            radioAudio.playOnAwake = false;

            // Ses dosyasını Resources'tan yüklemeyi dene
            // Beklenen yol: Resources/Audio/Hints/Radio/whisper_XX
            AudioClip clip = Resources.Load<AudioClip>($"Audio/Hints/Radio/whisper_{clipIndex:D2}");
            if (clip != null)
            {
                radioAudio.clip = clip;
                radioAudio.Play();
            }
            else
            {
                Debug.Log($"[TelsizFisilti] Ses dosyası bulunamadı (Resources/Audio/Hints/Radio/whisper_{clipIndex:D2}). Altyazı-only modda devam ediliyor.");
            }
        }

        // Altyazıyı göster
        ShowSubtitle(subtitle);

        // Süre boyunca bekle (realtime, TimeScale'den bağımsız)
        yield return new WaitForSeconds(SUBTITLE_DURATION);

        // Temizlik
        HideSubtitle();

        if (radioAudio != null && radioAudio.isPlaying)
        {
            radioAudio.Stop();
        }

        isRadioActive = false;
        activeSequence = null;
    }

    /// <summary>
    /// Telsiz objesinin Renderer materyalinde Emission rengini
    /// kırmızı olarak yanıp söndürür (LED efekti).
    /// </summary>
    private IEnumerator BlinkLED(Renderer renderer, float totalDuration)
    {
        if (renderer == null) yield break;

        Material mat = renderer.material;
        // Orijinal Emission durumunu sakla
        bool hadEmission = mat.IsKeywordEnabled("_EMISSION");
        Color originalEmission = mat.HasProperty("_EmissionColor")
            ? mat.GetColor("_EmissionColor")
            : Color.black;

        float elapsed = 0f;
        bool isOn = false;
        Color ledRedColor = new Color(1f, 0.1f, 0.05f, 1f) * 2f; // HDR kırmızı

        mat.EnableKeyword("_EMISSION");

        while (elapsed < totalDuration)
        {
            isOn = !isOn;
            mat.SetColor("_EmissionColor", isOn ? ledRedColor : Color.black);

            yield return new WaitForSeconds(LED_BLINK_INTERVAL);
            elapsed += LED_BLINK_INTERVAL;
        }

        // Orijinal Emission'ı geri yükle
        if (!hadEmission) mat.DisableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", originalEmission);
    }

    /// <summary>
    /// Metin şablonundaki yön etiketlerini aktif yön ve dile göre değiştirir.
    /// </summary>
    private string ApplyDirectionTags(string template, bool isLeft, bool isTR)
    {
        if (isTR)
        {
            return template.Replace("[SOL / SAĞ]", isLeft ? "SOL" : "SAĞ");
        }
        else
        {
            return template.Replace("[LEFT / RIGHT]", isLeft ? "LEFT" : "RIGHT");
        }
    }

    private void ShowSubtitle(string text)
    {
        if (subtitleCanvas == null) BuildSubtitleUI();

        subtitleText.text = text;
        subtitleCanvas.gameObject.SetActive(true);
    }

    private void HideSubtitle()
    {
        if (subtitleCanvas != null)
        {
            subtitleCanvas.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Altyazı gösterim panelini runtime'da oluşturur.
    /// Ekranın alt-ortasında yarı saydam siyah arka plan üzerinde beyaz/soluk yeşil metin.
    /// </summary>
    private void BuildSubtitleUI()
    {
        // === Canvas ===
        GameObject canvasGO = new GameObject("TelsizSubtitleCanvas");
        canvasGO.transform.SetParent(transform);
        subtitleCanvas = canvasGO.AddComponent<Canvas>();
        subtitleCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        subtitleCanvas.sortingOrder = 140;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();

        // === Altyazı Arka Plan Paneli ===
        GameObject bgGO = new GameObject("SubtitleBackground");
        bgGO.transform.SetParent(canvasGO.transform, false);
        subtitleBg = bgGO.AddComponent<Image>();
        subtitleBg.color = new Color(0f, 0f, 0f, 0.65f);

        RectTransform bgRect = bgGO.GetComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0.1f, 0.03f);
        bgRect.anchorMax = new Vector2(0.9f, 0.15f);
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        // === Altyazı Metni ===
        GameObject textGO = new GameObject("SubtitleText");
        textGO.transform.SetParent(bgGO.transform, false);
        subtitleText = textGO.AddComponent<TextMeshProUGUI>();
        subtitleText.fontSize = 22f;
        subtitleText.fontStyle = FontStyles.Italic;
        // Telsiz fısıltısı için soluk yeşilimsi beyaz renk (parazitli his)
        subtitleText.color = new Color(0.75f, 0.92f, 0.72f, 1f);
        subtitleText.alignment = TextAlignmentOptions.Center;
        subtitleText.enableWordWrapping = true;

        RectTransform textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.05f, 0.1f);
        textRect.anchorMax = new Vector2(0.95f, 0.9f);
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        // Başlangıçta gizli
        subtitleCanvas.gameObject.SetActive(false);
    }
}
