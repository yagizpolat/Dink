using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Paslı Mektup & Not (MektupNot) ipucu sistemi.
/// 18 dramatik mektup metninden rastgele birini seçerek runtime UI panelinde gösterir.
/// Okunan her mektup Lore puanı kazandırır (SpeedrunnerDetector entegrasyonu).
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
    private CursorLockMode previousCursorLock;
    private bool previousCursorVisible;

    /// <summary>
    /// Toplam okunan mektup sayısı. SpeedrunnerDetector bu değeri sorgulayarak
    /// oyuncunun hikayeye ne kadar ilgi gösterdiğini hesaplar.
    /// </summary>
    public int TotalLettersRead { get; private set; }

    // =========================================================================
    // 18 Mektup Metin Havuzu — Türkçe
    // [SOL / SAĞ] ve [SOLUMDAN / SAĞIMDAN] etiketleri runtime'da değiştirilir.
    // =========================================================================
    private static readonly string[] letterPoolTR = new string[]
    {
        // 1
        "Elektrik faturası yine gecikmeye girmiş... Son maaşın yarısını buna yatırdım ama yetmedi. Küçük kız kardeşim karanlıktan korkuyor, bu gece lambayı açık bırakmak zorundaydım. Özür dilerim... Işığın süzüldüğü [SOL / SAĞ] kapı, faturanın ödenmediği o karanlık evden daha güvenli duruyor.",
        // 2
        "Yeşil önlükteki leke bir türlü çıkmıyor. Vardiya amiri 'Daha hızlı diz rafları' diye bağırdı. Yüzüme bile bakmıyor. 16 yaşındayım ama 60 yaşında gibi hissediyorum. Sağır edici mesai bitiminde amirim hep [SOLUMDAN / SAĞIMDAN] kulağımın dibine girip o cırtlak sesiyle bana zulüm ederdi.",
        // 3
        "Kız kardeşimin en sevdiği peluş ayı... Cebimde unutmuşum. Evden çıkarken bana 'Abi erken gel, korkuyorum' demişti. Şimdi neredeyim ben? Oyuncak ayının tek gözü yok mu? [SOL / SAĞ] gözü duruyor sadece, diğerine ne oldu böyle?..",
        // 4
        "Bu döngüde aklımı kaybetmek üzereyim. Benden sonra buraya düşen olursa diye yazıyorum. Duvarlardaki küflere ve çatlaklara dikkat et. Rutubetin ve küfün biriktiği [SOL / SAĞ] kapı seni zindandan koruyacak.",
        // 5
        "Masada iki tabak var ama birinde sadece zeytin ve bayat ekmek. 'Abi sen yemiyor musun?' diye sordu. 'Ben işte yedim' dedim. Yalan söyledim. O büyüsün yeter. Açlıkla geçen gecelerde kardeşime ekmeği hep [SOL / SAĞ] elimle uzatırdım.",
        // 6
        "Okul müdürü 'Yeteneğin var Hakan, okulu bırakma' demişti. Ama müdür faturaları ödemiyordu. Sıramı en son ne zaman sildim hatırlamıyorum bile. Sınıftan çıkarken her zaman [SOL / SAĞ] taraftaki kapıyı kullanırdım.",
        // 7
        "Fişin arkasına karalanmış bir not: 'Son kullanma tarihi geçmiş konserveleri çöpe atma, eve götür.' Yaşamak bu olmamalıydı. Marketteki kasada 'İPTAL' tuşu hep [SOL / SAĞ] taraftaki kapıyı işaret ederdi.",
        // 8
        "Aşağıya, o soğuk zindana düşersen paslı şalterlere dikkat et. Oradan kaçmayı başardım ama ruhumun bir parçası orada kaldı. Paslı borulardan uzak durmak istiyorsan [SOL / SAĞ] kapıdan yürü.",
        // 9
        "Annemle babamın yan yana gülümsediği tek kare. Kenarı hafifçe yanmış. Onlar gittiğinden beri ev hiç ısınmadı. Fotoğrafta babamın durduğu [SOL / SAĞ] taraf her zaman daha güvenliydi.",
        // 10
        "Aynaya baktım. Karşımdaki kişi 16 yaşında bir çocuk değil. Gözlerinin altı morarmış, bakışları ruhsuz bir yabancı. Aynadaki yansımama baktığımda kalbim [SOL / SAĞ] taraftaki kapı için atıyor.",
        // 11
        "Çok hızlı koştum... Etraftaki hiçbir notu okumadan sadece kapıları açıp geçtim. 15. kapıya ulaştığımda beni başa attılar! Acele etme. Yavaşça yaklaş ve [SOL / SAĞ] kapıyı aç.",
        // 12
        "Çatı yine akıtıyor. Kovayı tam yatağın yanına koydum. Tıp tıp tıp... Bu ses kapıların arkasından gelen yankıyla aynı. Su damlalarının sesinin azaldığı [SOL / SAĞ] kapı seni kurtaracak.",
        // 13
        "Abi, dolaptaki süt bitti. Bir de resim çizdim sana, buzdolabına yapıştırdım. Çabuk gel olur mu? Işıklar yanıp sönüyor. Resimde çizdiğim evin kapısı [SOL / SAĞ] taraftaydı, çabuk gel abi...",
        // 14
        "Cebimde sadece üç beş kuruş bozuk para kalmış. Marketten çıkarken kardeşime küçük bir çikolata bile alamadım. Yürürken ceplerimdeki paraların sesi kafamın içinde çınlıyor... Sessizce yürü; harçlığımı her zaman montumun [SOL / SAĞ] cebinde saklardım.",
        // 15
        "Kurmalı saatin yayı kopmuş, gecenin 03:15'inde takılı kalmış. Akrep de yelkovan da hareket etmiyor. Burada zaman durdu mu, yoksa ben mi çok çabuk yaşlandım? Zamanın durduğu o saatte yelkovanın gösterdiği [SOL / SAĞ] kapı seni bekliyor.",
        // 16
        "Yüzümdeki morluk hâlâ sızlıyor. 'Raflar eksik' diye üzerime yürüdü, çelme takıp yere düşürdü. Sokak lambasının altında kanayan dudağımı silerken tek düşündüğüm eve varabilmekti. Kaçarken arkama baktığımda saldırganım hep [SOL / SAĞ] tarafımda kalıyordu.",
        // 17
        "Sokaktaki evlerin bacalarından kömür kokusu yükseliyor. Bizim ev ise buz gibi. Kardeşime iki kat kazak giydirdim, elleri yine de sıcacık. Tek tesellim bu. Dumanın sürüklendiği [SOL / SAĞ] kapı seni o soğuktan uzak tutacak.",
        // 18
        "Marketin depo penceresinden dışarı bakarken yağmur başladı. Herkes şemsiyesini açıp evine, sıcak çorbasına koşuyordu. Ben ise sadece bir sonraki faturanın tarihini hesaplıyordum. Depo penceresinden baktığımda caddenin [SOL / SAĞ] tarafı her zaman daha aydınlıktı."
    };

    // =========================================================================
    // 18 Letter Text Pool — English
    // [LEFT / RIGHT] and [FROM MY LEFT / FROM MY RIGHT] tags replaced at runtime.
    // =========================================================================
    private static readonly string[] letterPoolEN = new string[]
    {
        // 1
        "The electricity bill is overdue again... I spent half my paycheck on it but it wasn't enough. My little sister is afraid of the dark, I had to leave the light on tonight. I'm sorry... The [LEFT / RIGHT] door where light seeps through looks safer than that dark house where the bills went unpaid.",
        // 2
        "The stain on the green apron won't come out. The shift supervisor yelled 'Stock the shelves faster.' He won't even look at my face. I'm 16 but I feel like 60. At the end of every deafening shift, my supervisor would always come [FROM MY LEFT / FROM MY RIGHT] and torment me with that shrill voice right in my ear.",
        // 3
        "My little sister's favorite teddy bear... I forgot it in my pocket. When I left home she said 'Come back early, I'm scared.' Where am I now? The teddy bear only has one eye, doesn't it? Only the [LEFT / RIGHT] eye remains, what happened to the other one?..",
        // 4
        "I'm about to lose my mind in this loop. I'm writing this in case someone falls in here after me. Pay attention to the mold and cracks on the walls. The [LEFT / RIGHT] door where moisture and mold accumulate will protect you from the dungeon.",
        // 5
        "There are two plates on the table but one only has olives and stale bread. 'Aren't you eating?' she asked. 'I already ate at work' I said. I lied. As long as she grows up healthy. During those hungry nights, I always handed the bread to my sister with my [LEFT / RIGHT] hand.",
        // 6
        "The principal said 'You have talent Hakan, don't drop out.' But the principal wasn't paying the bills. I can't even remember the last time I wiped my desk. When leaving the classroom, I always used the door on the [LEFT / RIGHT] side.",
        // 7
        "A note scribbled on the back of a receipt: 'Don't throw away the expired canned food, take it home.' Living shouldn't be like this. The 'CANCEL' button at the store register always pointed to the [LEFT / RIGHT] door.",
        // 8
        "If you fall down to that cold dungeon, watch out for the rusty circuit breakers. I managed to escape from there but a piece of my soul stayed behind. If you want to stay away from the rusty pipes, walk through the [LEFT / RIGHT] door.",
        // 9
        "The only photo where my mom and dad are smiling side by side. The edge is slightly burned. The house has never been warm since they left. The [LEFT / RIGHT] side where my father stood in the photo was always safer.",
        // 10
        "I looked in the mirror. The person in front of me isn't a 16-year-old kid. Dark circles under the eyes, a soulless stranger staring back. When I look at my reflection, my heart beats for the [LEFT / RIGHT] door.",
        // 11
        "I ran too fast... I just opened doors and passed through without reading any notes. When I reached the 15th door, they sent me back to the start! Don't rush. Approach slowly and open the [LEFT / RIGHT] door.",
        // 12
        "The roof is leaking again. I put the bucket right next to the bed. Drip drip drip... This sound is the same as the echo from behind the doors. The [LEFT / RIGHT] door where the dripping sound fades will save you.",
        // 13
        "Bro, the milk in the fridge is gone. I also drew a picture for you, I stuck it on the fridge. Come quickly, okay? The lights keep flickering. The door of the house I drew was on the [LEFT / RIGHT] side, come quickly bro...",
        // 14
        "Only a few coins left in my pocket. I couldn't even buy my sister a small chocolate on the way out of the store. The sound of coins jingling in my pockets rings inside my head as I walk... Walk quietly; I always kept my pocket money in the [LEFT / RIGHT] pocket of my coat.",
        // 15
        "The spring of the wind-up clock is broken, stuck at 3:15 AM. Neither the hour hand nor the minute hand moves. Has time stopped here, or did I age too quickly? The [LEFT / RIGHT] door that the minute hand points to at that frozen moment is waiting for you.",
        // 16
        "The bruise on my face still stings. He came at me saying 'Shelves are incomplete,' tripped me and knocked me down. Under the streetlight, wiping my bleeding lip, all I could think about was getting home. When I looked back while running, my attacker was always on my [LEFT / RIGHT] side.",
        // 17
        "Smoke from coal rises from the chimneys of the houses on the street. Our house is freezing cold. I put two sweaters on my sister, her hands are still warm. That's my only comfort. The [LEFT / RIGHT] door where the smoke drifts will keep you away from that cold.",
        // 18
        "I was looking outside from the store's warehouse window when the rain started. Everyone opened their umbrellas and ran to their homes, to their warm soup. I was just calculating the due date of the next bill. Looking from the warehouse window, the [LEFT / RIGHT] side of the street was always brighter."
    };

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
    /// Mektup okuma panelini açar. 18 metinden rastgele birini seçip yön etiketlerini doldurur.
    /// </summary>
    /// <param name="shownDirLeft">Gösterilecek yön (Fake Hint hesaplanmış hali)</param>
    public void OpenLetterPanel(bool shownDirLeft)
    {
        if (isPanelOpen) return;
        if (letterCanvas == null) BuildRuntimeUI();

        // O oda için önceden belirlenmiş mektup metnini çek (her açılışta değişmesini önler)
        int index = (DynamicHintSystem.instance != null) ? DynamicHintSystem.instance.currentLetterTextIndex : 0;
        if (index < 0 || index >= letterPoolTR.Length) index = 0;

        bool isTR = LanguageManager.instance != null &&
                     LanguageManager.instance.CurrentLanguage == LanguageManager.Language.TR;

        string template = isTR ? letterPoolTR[index] : letterPoolEN[index];
        string finalText = ApplyDirectionTags(template, shownDirLeft, isTR);

        Show(FormFor(index), finalText);

        // Lore puanı kazandır
        TotalLettersRead++;
        Debug.Log($"[MektupNotHintUI] 📖 Mektup #{TotalLettersRead} okundu (Index: {index}). Lore puanı kazanıldı.");
    }

    /// <summary>
    /// Apartman katlarındaki elle yerleştirilmiş kâğıtlar için: verilen metni market fişi ya da
    /// çizgili defter sayfası olarak açar (oda mektup havuzunu kullanmaz).
    /// </summary>
    public void OpenText(string body, bool asReceipt, bool countsAsLetter)
    {
        if (isPanelOpen) return;
        if (letterCanvas == null) BuildRuntimeUI();
        Show(asReceipt ? LetterForm.Receipt : LetterForm.RuledPage, body);
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

        // İmleci KESİNLİKLE merkeze kitle ve gizle
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Input.ResetInputAxes();

        // Kamera ve kontrol script'lerini tekrar aktif et
        TogglePlayerScripts(true);
    }

    /// <summary>
    /// Mektup okurken kameranın dönmesini ve etkileşimlerin çakışmasını önlemek için scriptleri açar/kapatır.
    /// </summary>
    private void TogglePlayerScripts(bool enable)
    {
        PlayerControls.SetEnabled(enable);
        if (RoomCountdown.instance != null) RoomCountdown.instance.SetHidden(!enable);

        // Mektup açıkken ESC menüsü de kapalı: yoksa ESC → "Devam" kamerayı mektubun arkasında açıyordu
        Escmenu esc = FindAnyObjectByType<Escmenu>();
        if (esc != null) esc.enabled = enable;
    }

    /// <summary>
    /// Metin şablonundaki [SOL / SAĞ] ve [SOLUMDAN / SAĞIMDAN] etiketlerini
    /// aktif yön ve dile göre değiştirir.
    /// </summary>
    private string ApplyDirectionTags(string template, bool isLeft, bool isTR)
    {
        if (isTR)
        {
            template = template.Replace("[SOL / SAĞ]", isLeft ? "SOL" : "SAĞ");
            template = template.Replace("[SOLUMDAN / SAĞIMDAN]", isLeft ? "SOLUMDAN" : "SAĞIMDAN");
        }
        else
        {
            template = template.Replace("[LEFT / RIGHT]", isLeft ? "LEFT" : "RIGHT");
            template = template.Replace("[FROM MY LEFT / FROM MY RIGHT]", isLeft ? "FROM MY LEFT" : "FROM MY RIGHT");
        }
        return template;
    }

    // =========================================================================
    // Mektubun fiziksel biçimi: her mektup ne olduğuna göre görünür
    // (market fişi, çizgili defter sayfası, Zeynep'in pastel notu, fotoğraf arkası)
    // =========================================================================
    private enum LetterForm { RuledPage, Receipt, ZeynepNote, PhotoBack }

    private static LetterForm FormFor(int index)
    {
        switch (index)
        {
            case 1: case 6: case 13: case 17: return LetterForm.Receipt;   // market, fiş, bozuk para, depo
            case 8: return LetterForm.PhotoBack;                           // anne ile babanın fotoğrafı
            case 12: return LetterForm.ZeynepNote;                         // "Abi, dolaptaki süt bitti"
            default: return LetterForm.RuledPage;
        }
    }

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
        float sfx = AudioManager.instance != null ? AudioManager.instance.GetSFXVolume() : PlayerPrefs.GetFloat("Dink_SFXVolume", 0.8f);
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
