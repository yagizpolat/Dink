using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Apartman katlarındaki kâğıtları (fiş, defter sayfası, not, fotoğraf arkası) okuma paneli.
/// UI runtime'da kurulur; sahneler arası tek örnek olarak yaşar.
/// </summary>
public class MektupNotHintUI : MonoBehaviour
{
    private static MektupNotHintUI _instance;
    public static MektupNotHintUI Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<MektupNotHintUI>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("[MektupNotHintUI]");
                    _instance = go.AddComponent<MektupNotHintUI>();
                }
            }
            return _instance;
        }
    }
    public static MektupNotHintUI instance => Instance;

    private Canvas letterCanvas;
    private TextMeshProUGUI letterBodyText;
    private bool isPanelOpen;
    public bool IsPanelOpen => isPanelOpen;
    private float previousTimeScale;
    private CursorLockMode previousLockState = CursorLockMode.Locked;
    private bool previousCursorVisible;

    /// <summary>
    /// Toplam okunan mektup sayısı (countsAsLetter ile açılan kâğıtlar).
    /// </summary>
    public int TotalLettersRead { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoCreate()
    {
        if (instance != null) return;
        GameObject go = new GameObject("[MektupNotHintUI]");
        go.AddComponent<MektupNotHintUI>();
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
        BuildRuntimeUI();
    }

    private void Update()
    {
        // ESC tuşu ile mektup panelini kapat
        if (isPanelOpen && DinkInput.EscapeDown)
        {
            DinkInput.ConsumeEscape();
            CloseLetterPanel();
        }
    }

    /// <summary>
    /// Apartman katlarındaki elle yerleştirilmiş kâğıtlar için: verilen metni market fişi ya da
    /// çizgili defter sayfası olarak açar.
    /// </summary>
    public void OpenText(string body, bool asReceipt, bool countsAsLetter)
    {
        OpenText(body, asReceipt ? LetterForm.Receipt : LetterForm.RuledPage, countsAsLetter);
    }

    /// <summary>Verilen metni istenen kâğıt biçiminde açar.</summary>
    public void OpenText(string body, LetterForm form, bool countsAsLetter)
    {
        if (isPanelOpen) return;
        if (letterCanvas == null) BuildRuntimeUI();
        Show(form, body);
        if (countsAsLetter) TotalLettersRead++;
    }

    private void Show(LetterForm form, string text)
    {
        ApplyForm(form);
        letterBodyText.text = text;
        if (animating != null) StopCoroutine(animating);
        letterCanvas.gameObject.SetActive(true);
        JitterHandwriting(form);
        animating = StartCoroutine(AnimateOpen());
        PlayRustle();
        isPanelOpen = true;

        // Zamanı dondur
        previousTimeScale = Time.timeScale;
        previousLockState = Cursor.lockState;
        previousCursorVisible = Cursor.visible;
        Time.timeScale = 0f;

        // Kamera rotasyonu ve etkileşimleri geçici olarak durdur (fare kaymasını önler)
        TogglePlayerScripts(false);

        // İmleci serbest bırak
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    /// <summary>
    /// Mektup okuma panelini kapatır ve imleci merkeze kilitler.
    /// </summary>
    public void CloseLetterPanel()
    {
        if (!isPanelOpen) return;

        if (animating != null) StopCoroutine(animating);
        animating = StartCoroutine(AnimateClose());
        PlayRustle();
        isPanelOpen = false;

        // Zamanı geri yükle
        Time.timeScale = previousTimeScale > 0f ? previousTimeScale : 1f;

        // İmleç mektup açılmadan önceki hâline döner (yürürken kilitli; defter gibi serbest imleçli ekranda serbest)
        Cursor.lockState = previousLockState;
        Cursor.visible = previousCursorVisible;
        Input.ResetInputAxes();

        // Kamera ve kontrol script'lerini tekrar aktif et
        TogglePlayerScripts(true);
    }

    /// <summary>
    /// Mektup okurken kameranın dönmesini ve etkileşimlerin çakışmasını önlemek için scriptleri açar/kapatır.
    /// </summary>
    private void TogglePlayerScripts(bool enable)
    {
        PlayerControls.SetPaused(!enable);

        // Mektup açıkken ESC menüsü de kapalı: yoksa ESC → "Devam" kamerayı mektubun arkasında açıyordu
        Escmenu esc = FindAnyObjectByType<Escmenu>();
        if (esc != null) esc.enabled = enable;
    }

    // =========================================================================
    // Mektubun fiziksel biçimi: her mektup ne olduğuna göre görünür
    // (market fişi, çizgili defter sayfası, Zeynep'in pastel notu, fotoğraf arkası)
    // =========================================================================
    public enum LetterForm { RuledPage, Receipt, ZeynepNote, PhotoBack }

    private const float OpenDuration = 0.45f;
    private const float CloseDuration = 0.25f;
    private const float EscHintDelay = 1.2f;

    private CanvasGroup overlayGroup;
    private RawImage paperImage;
    private RectTransform paperRect;
    private TextMeshProUGUI escHint;
    private TMP_FontAsset handFont, handFontBold, monoFont;
    private readonly System.Collections.Generic.Dictionary<LetterForm, Texture2D> paperTextures =
        new System.Collections.Generic.Dictionary<LetterForm, Texture2D>();
    private AudioSource rustleSource;
    private AudioClip rustleClip;
    private Coroutine animating;

    /// <summary>
    /// Mektup okuma ekranının UI hiyerarşisini runtime'da oluşturur. Sahne YAML dosyasına dokunulmaz.
    /// Panel, başlık ya da X butonu yok: oda kararır ve kağıt Hakan'ın elinde gibi alttan kalkar.
    /// </summary>
    private void BuildRuntimeUI()
    {
        EnsureEventSystem();

        GameObject canvasGO = new GameObject("MektupNotCanvas");
        canvasGO.transform.SetParent(transform);
        letterCanvas = canvasGO.AddComponent<Canvas>();
        letterCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        letterCanvas.sortingOrder = 300; // oda girişinin karartmasının (200) üstünde

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 1f;
        canvasGO.AddComponent<GraphicRaycaster>();

        // Odayı karartan katman ve kenar gölgesi; tıklanınca mektup kapanır
        GameObject overlayGO = new GameObject("DarkOverlay");
        overlayGO.transform.SetParent(canvasGO.transform, false);
        Stretch(overlayGO.AddComponent<RectTransform>());
        RawImage overlayImg = overlayGO.AddComponent<RawImage>();
        overlayImg.texture = VignetteTexture();
        Button overlayBtn = overlayGO.AddComponent<Button>();
        overlayBtn.transition = Selectable.Transition.None;
        overlayBtn.onClick.AddListener(CloseLetterPanel);
        overlayGroup = overlayGO.AddComponent<CanvasGroup>();

        // Kağıt (biçimi her açılışta değişir)
        GameObject paperGO = new GameObject("Paper");
        paperGO.transform.SetParent(overlayGO.transform, false);
        paperRect = paperGO.AddComponent<RectTransform>();
        paperRect.anchorMin = paperRect.anchorMax = new Vector2(0.5f, 0.5f);
        paperImage = paperGO.AddComponent<RawImage>();
        paperImage.raycastTarget = true; // kağıda tıklamak mektubu kapatmasın

        GameObject bodyGO = new GameObject("LetterBody");
        bodyGO.transform.SetParent(paperGO.transform, false);
        letterBodyText = bodyGO.AddComponent<TextMeshProUGUI>();
        letterBodyText.textWrappingMode = TextWrappingModes.Normal;
        letterBodyText.overflowMode = TextOverflowModes.Overflow;
        letterBodyText.raycastTarget = false;

        // Küçük, geç beliren kapatma ipucu
        GameObject cueGO = new GameObject("EscCue");
        cueGO.transform.SetParent(overlayGO.transform, false);
        RectTransform cueRect = cueGO.AddComponent<RectTransform>();
        cueRect.anchorMin = cueRect.anchorMax = new Vector2(0.5f, 0f);
        cueRect.pivot = new Vector2(0.5f, 0f);
        cueRect.anchoredPosition = new Vector2(0f, 26f);
        cueRect.sizeDelta = new Vector2(300f, 30f);
        escHint = cueGO.AddComponent<TextMeshProUGUI>();
        escHint.text = "Esc";
        escHint.fontSize = 17f;
        escHint.characterSpacing = 12f;
        escHint.alignment = TextAlignmentOptions.Bottom;
        escHint.color = new Color(0.85f, 0.82f, 0.75f, 0.5f);
        escHint.raycastTarget = false;

        handFont = LoadFont("Fonts/Caveat-Regular");
        handFontBold = LoadFont("Fonts/Caveat-SemiBold");
        monoFont = LoadFont("Fonts/PTMono-Regular");

        rustleSource = gameObject.AddComponent<AudioSource>();
        rustleSource.playOnAwake = false;
        rustleSource.spatialBlend = 0f;
        rustleSource.ignoreListenerPause = true;
        rustleClip = MakeRustleClip();

        letterCanvas.gameObject.SetActive(false);
    }

    /// <summary>
    /// Kağıdı mektubun biçimine göre boyutlandırır, döndürür ve yazısını ayarlar (1080p referansı).
    /// </summary>
    private void ApplyForm(LetterForm form)
    {
        Vector2 size; float tilt; Rect textArea; // textArea: kağıt içinde 0-1 aralığında
        TMP_FontAsset font; float fontSize; Color ink; float lineSpacing = 0f;
        switch (form)
        {
            case LetterForm.Receipt:
                size = new Vector2(500f, 680f); tilt = 1.5f; textArea = new Rect(0.1f, 0.14f, 0.8f, 0.68f);
                font = monoFont; fontSize = 25f; ink = new Color32(0x3A, 0x36, 0x31, 0xFF);
                break;
            case LetterForm.PhotoBack:
                size = new Vector2(780f, 585f); tilt = 2f; textArea = new Rect(0.1f, 0.1f, 0.8f, 0.62f);
                font = handFont; fontSize = 38f; ink = new Color32(0x4A, 0x46, 0x3F, 0xFF);
                break;
            case LetterForm.ZeynepNote:
                size = new Vector2(806f, 756f); tilt = -2f; textArea = new Rect(0.2f, 0.08f, 0.72f, 0.84f);
                font = handFontBold; fontSize = 50f; ink = new Color32(0xA8, 0x39, 0x2E, 0xFF); lineSpacing = 8f;
                break;
            default:
                size = new Vector2(806f, 756f); tilt = -2f; textArea = new Rect(0.17f, 0.08f, 0.76f, 0.84f);
                font = handFont; fontSize = 38f; ink = new Color32(0x3B, 0x3A, 0x44, 0xFF); lineSpacing = 12f;
                break;
        }

        paperRect.sizeDelta = size;
        paperRect.localEulerAngles = new Vector3(0f, 0f, tilt);
        paperImage.texture = PaperTexture(form);

        RectTransform bodyRect = letterBodyText.rectTransform;
        bodyRect.anchorMin = new Vector2(textArea.xMin, textArea.yMin);
        bodyRect.anchorMax = new Vector2(textArea.xMax, textArea.yMax);
        bodyRect.offsetMin = bodyRect.offsetMax = Vector2.zero;

        if (font != null) letterBodyText.font = font;
        letterBodyText.fontStyle = FontStyles.Normal;
        letterBodyText.enableAutoSizing = true;
        letterBodyText.fontSizeMax = fontSize;
        letterBodyText.fontSizeMin = fontSize * 0.7f; // okunabilirlik sınırı
        letterBodyText.color = ink;
        letterBodyText.lineSpacing = lineSpacing;
        letterBodyText.alignment = TextAlignmentOptions.TopLeft;
    }

    /// <summary>
    /// El yazısı biçimlerde her harfi biraz döndürüp kaydırır; harfler bir fontun düzgün dizilişi gibi durmasın.
    /// Aynı mektup her açıldığında aynı titreme çıkar (sabit tohum).
    /// </summary>
    private void JitterHandwriting(LetterForm form)
    {
        if (form == LetterForm.Receipt) return;

        float maxAngle = form == LetterForm.ZeynepNote ? 5f : 2.5f;
        float maxShift = form == LetterForm.ZeynepNote ? 0.08f : 0.04f;

        letterBodyText.ForceMeshUpdate();
        TMP_TextInfo info = letterBodyText.textInfo;
        var random = new System.Random(letterBodyText.text.Length);
        for (int i = 0; i < info.characterCount; i++)
        {
            TMP_CharacterInfo c = info.characterInfo[i];
            if (!c.isVisible) continue;

            Vector3[] v = info.meshInfo[c.materialReferenceIndex].vertices;
            int vi = c.vertexIndex;
            Vector3 center = (v[vi] + v[vi + 2]) * 0.5f;
            float angle = ((float)random.NextDouble() * 2f - 1f) * maxAngle;
            float shift = ((float)random.NextDouble() * 2f - 1f) * maxShift * c.pointSize;
            Quaternion rot = Quaternion.Euler(0f, 0f, angle);
            for (int k = 0; k < 4; k++)
            {
                v[vi + k] = rot * (v[vi + k] - center) + center + new Vector3(0f, shift, 0f);
            }
        }
        letterBodyText.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
    }

    private IEnumerator AnimateOpen()
    {
        overlayGroup.alpha = 0f;
        escHint.alpha = 0f;
        float rise = 1080f * 0.12f;
        for (float t = 0f; t < OpenDuration; t += Time.unscaledDeltaTime)
        {
            float k = 1f - Mathf.Pow(1f - t / OpenDuration, 3f); // ease-out cubic
            overlayGroup.alpha = Mathf.Clamp01(t / 0.3f);
            paperRect.anchoredPosition = new Vector2(0f, Mathf.Lerp(-rise, 0f, k));
            paperRect.localScale = Vector3.one * Mathf.Lerp(0.94f, 1f, k);
            yield return null;
        }
        overlayGroup.alpha = 1f;
        paperRect.anchoredPosition = Vector2.zero;
        paperRect.localScale = Vector3.one;

        yield return new WaitForSecondsRealtime(EscHintDelay - OpenDuration);
        for (float t = 0f; t < 0.4f; t += Time.unscaledDeltaTime)
        {
            escHint.alpha = 0.5f * t / 0.4f;
            yield return null;
        }
        escHint.alpha = 0.5f;
        animating = null;
    }

    private IEnumerator AnimateClose()
    {
        float rise = 1080f * 0.12f;
        for (float t = 0f; t < CloseDuration; t += Time.unscaledDeltaTime)
        {
            float k = t / CloseDuration;
            overlayGroup.alpha = 1f - k;
            paperRect.anchoredPosition = new Vector2(0f, Mathf.Lerp(0f, -rise, k * k));
            yield return null;
        }
        letterCanvas.gameObject.SetActive(false);
        animating = null;
    }

    private void PlayRustle()
    {
        float sfx = AudioManager.instance != null ? AudioManager.instance.SFXSourceScale : PlayerPrefs.GetFloat("Dink_SFXVolume", 0.8f);
        rustleSource.PlayOneShot(rustleClip, sfx * 0.45f);
    }

    // ------------------------------------------------------------------
    // Prosedürel dokular ve ses
    // ------------------------------------------------------------------

    private Texture2D PaperTexture(LetterForm form)
    {
        if (paperTextures.TryGetValue(form, out Texture2D cached)) return cached;

        int w, h; Color baseColor;
        switch (form)
        {
            case LetterForm.Receipt: w = 300; h = 408; baseColor = new Color32(0xE2, 0xDD, 0xD0, 0xFF); break;
            case LetterForm.PhotoBack: w = 468; h = 351; baseColor = new Color32(0xCF, 0xC8, 0xB2, 0xFF); break;
            default: w = 484; h = 454; baseColor = new Color32(0xD9, 0xD0, 0xB8, 0xFF); break;
        }

        var random = new System.Random((int)form + 3);
        Color[] px = new Color[w * h];
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float u = (float)x / w, v = (float)y / h;
                float grain = (float)random.NextDouble() * 0.035f + Mathf.PerlinNoise(u * 18f, v * 18f) * 0.04f;
                float edge = Mathf.Clamp01(Mathf.Min(Mathf.Min(u, 1f - u), Mathf.Min(v, 1f - v)) * 14f);
                Color c = Color.Lerp(baseColor * new Color(0.86f, 0.8f, 0.68f, 1f), baseColor, edge) * (1f - grain);
                c.a = 1f;

                if (form == LetterForm.Receipt)
                {
                    // Termal kağıtta soluk yatay şeritler, altı yırtık
                    c *= 1f - Mathf.PerlinNoise(0.5f, v * 40f) * 0.05f;
                    c.a = 1f;
                    float tear = 6f + 5f * Mathf.Abs(Mathf.Sin(x * 0.55f)) + (float)random.NextDouble() * 2f;
                    if (y < tear) c.a = 0f;
                    // Üstte ve altta kesik çizgili ayraç
                    if ((Mathf.Abs(v - 0.9f) < 0.002f || Mathf.Abs(v - 0.1f) < 0.002f) && (x / 6) % 2 == 0 && u > 0.08f && u < 0.92f)
                        c = Color.Lerp(c, new Color(0.23f, 0.21f, 0.19f, c.a), 0.6f);
                }
                else if (form == LetterForm.PhotoBack)
                {
                    // Fotoğraf kağıdının arkasındaki soluk çapraz baskı deseni
                    if (((x + y) % 46) < 1) { c *= 0.965f; c.a = 1f; }
                }
                else
                {
                    // Defter sayfası: mavi-gri satır çizgileri ve kırmızı kenar boşluğu çizgisi
                    float lineGap = 30f;
                    float fromTop = h - y;
                    if (fromTop > 40f && (fromTop % lineGap) < 1.1f) c = Color.Lerp(c, new Color(0.48f, 0.55f, 0.66f, 1f), 0.45f);
                    if (Mathf.Abs(x - w * 0.13f) < 1f) c = Color.Lerp(c, new Color(0.72f, 0.33f, 0.3f, 1f), 0.5f);
                }

                px[y * w + x] = c;
            }
        }

        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, true);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.SetPixels(px);
        tex.Apply();
        paperTextures[form] = tex;
        return tex;
    }

    private static Texture2D VignetteTexture()
    {
        const int size = 128;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = (x + 0.5f) / size * 2f - 1f, dy = (y + 0.5f) / size * 2f - 1f;
                float d = Mathf.Clamp01(Mathf.Sqrt(dx * dx + dy * dy) / 1.414f);
                tex.SetPixel(x, y, new Color(0f, 0f, 0f, Mathf.Lerp(0.68f, 0.92f, d * d)));
            }
        }
        tex.Apply();
        return tex;
    }

    /// <summary>
    /// Kağıt hışırtısı: zarfı düzensiz, yüksek frekanslı kısa gürültü.
    /// </summary>
    private static AudioClip MakeRustleClip()
    {
        const int rate = 44100;
        int length = (int)(rate * 0.32f);
        float[] data = new float[length];
        var random = new System.Random(11);
        float prev = 0f;
        for (int i = 0; i < length; i++)
        {
            float t = (float)i / rate;
            float noise = (float)(random.NextDouble() * 2.0 - 1.0);
            float high = noise - prev * 0.6f; // alçak frekansları kıs
            prev = noise;
            float envelope = Mathf.Sin(Mathf.PI * t / 0.32f) * (0.6f + 0.4f * Mathf.PerlinNoise(t * 60f, 0.3f));
            data[i] = Mathf.Clamp(high * envelope * 0.35f, -1f, 1f);
        }
        AudioClip clip = AudioClip.Create("LetterRustle", length, 1, rate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private static TMP_FontAsset LoadFont(string resourcePath)
    {
        // Hazır TMP font dosyası (Resources/Fonts/TMP/<ad> SDF). Fontu oyun içinde
        // CreateFontAsset ile üretmek build'de Unity'yi çökertiyordu.
        string fileName = resourcePath.Substring(resourcePath.LastIndexOf('/') + 1);
        TMP_FontAsset asset = Resources.Load<TMP_FontAsset>($"Fonts/TMP/{fileName} SDF");
        if (asset == null)
        {
            Debug.LogError($"[MektupNotHintUI] TMP font bulunamadı: Resources/Fonts/TMP/{fileName} SDF");
            return TMP_Settings.defaultFontAsset;
        }
        if (TMP_Settings.defaultFontAsset != null && (asset.fallbackFontAssetTable == null || asset.fallbackFontAssetTable.Count == 0))
            asset.fallbackFontAssetTable = new System.Collections.Generic.List<TMP_FontAsset> { TMP_Settings.defaultFontAsset };
        return asset;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }

    /// <summary>
    /// Sahnede EventSystem yoksa oluşturur (UI tıklama desteği için gerekli).
    /// </summary>
    private void EnsureEventSystem()
    {
        if (FindAnyObjectByType<EventSystem>() == null)
        {
            GameObject esGO = new GameObject("EventSystem");
            esGO.AddComponent<EventSystem>();
            esGO.AddComponent<StandaloneInputModule>();
        }
    }
}
