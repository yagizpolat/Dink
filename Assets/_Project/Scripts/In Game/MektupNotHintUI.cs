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
        if (isPanelOpen && Input.GetKeyDown(KeyCode.Escape))
        {
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

        letterBodyText.text = finalText;
        letterCanvas.gameObject.SetActive(true);
        isPanelOpen = true;

        // Zamanı dondur
        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        // Kamera rotasyonu ve etkileşimleri geçici olarak durdur (fare kaymasını önler)
        TogglePlayerScripts(false);

        // İmleci serbest bırak
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Lore puanı kazandır
        TotalLettersRead++;
        Debug.Log($"[MektupNotHintUI] 📖 Mektup #{TotalLettersRead} okundu (Index: {index}). Lore puanı kazanıldı.");
    }

    /// <summary>
    /// Mektup okuma panelini kapatır ve imleci merkeze kilitler.
    /// </summary>
    public void CloseLetterPanel()
    {
        if (!isPanelOpen) return;

        letterCanvas.gameObject.SetActive(false);
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

    /// <summary>
    /// Mektup okuma panelinin tüm UI hiyerarşisini runtime'da oluşturur.
    /// Sahne YAML dosyasına dokunulmaz.
    /// </summary>
    private void BuildRuntimeUI()
    {
        EnsureEventSystem();

        // === Canvas ===
        GameObject canvasGO = new GameObject("MektupNotCanvas");
        canvasGO.transform.SetParent(transform);
        letterCanvas = canvasGO.AddComponent<Canvas>();
        letterCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        letterCanvas.sortingOrder = 150;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();

        // === Karanlık Arka Plan (Overlay) ===
        GameObject overlayGO = new GameObject("DarkOverlay");
        overlayGO.transform.SetParent(canvasGO.transform, false);
        Image overlayImg = overlayGO.AddComponent<Image>();
        overlayImg.color = new Color(0f, 0f, 0f, 0.75f);

        // Overlay'e tıklanınca paneli kapat
        Button overlayBtn = overlayGO.AddComponent<Button>();
        overlayBtn.transition = Selectable.Transition.None;
        overlayBtn.onClick.AddListener(CloseLetterPanel);

        RectTransform overlayRect = overlayGO.GetComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;

        // === Kağıt Paneli (Buruşuk, Eskimiş Kağıt Görünümü) ===
        GameObject paperGO = new GameObject("PaperPanel");
        paperGO.transform.SetParent(overlayGO.transform, false);
        Image paperImg = paperGO.AddComponent<Image>();
        // Eskimiş, sararmış kağıt rengi
        paperImg.color = new Color(0.82f, 0.74f, 0.58f, 1f);

        RectTransform paperRect = paperGO.GetComponent<RectTransform>();
        paperRect.anchorMin = new Vector2(0.18f, 0.08f);
        paperRect.anchorMax = new Vector2(0.82f, 0.92f);
        paperRect.offsetMin = Vector2.zero;
        paperRect.offsetMax = Vector2.zero;

        // === Başlık: "Paslı Mektup" ===
        GameObject titleGO = new GameObject("TitleText");
        titleGO.transform.SetParent(paperGO.transform, false);
        TextMeshProUGUI titleText = titleGO.AddComponent<TextMeshProUGUI>();

        bool isTR = LanguageManager.instance != null &&
                     LanguageManager.instance.CurrentLanguage == LanguageManager.Language.TR;
        titleText.text = isTR ? "— Paslı Mektup —" : "— Rusty Letter —";
        titleText.fontSize = 28f;
        titleText.fontStyle = FontStyles.Bold | FontStyles.Italic;
        titleText.color = new Color(0.25f, 0.18f, 0.1f, 0.85f);
        titleText.alignment = TextAlignmentOptions.Center;

        RectTransform titleRect = titleGO.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.05f, 0.88f);
        titleRect.anchorMax = new Vector2(0.95f, 0.97f);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;

        // === Mektup Gövde Metni ===
        GameObject bodyGO = new GameObject("LetterBodyText");
        bodyGO.transform.SetParent(paperGO.transform, false);
        letterBodyText = bodyGO.AddComponent<TextMeshProUGUI>();
        letterBodyText.fontSize = 22f;
        letterBodyText.fontStyle = FontStyles.Italic;
        letterBodyText.color = new Color(0.15f, 0.1f, 0.05f, 1f);
        letterBodyText.alignment = TextAlignmentOptions.TopLeft;
        letterBodyText.enableWordWrapping = true;
        letterBodyText.overflowMode = TextOverflowModes.Overflow;
        letterBodyText.lineSpacing = 8f;

        RectTransform bodyRect = bodyGO.GetComponent<RectTransform>();
        bodyRect.anchorMin = new Vector2(0.08f, 0.12f);
        bodyRect.anchorMax = new Vector2(0.92f, 0.85f);
        bodyRect.offsetMin = Vector2.zero;
        bodyRect.offsetMax = Vector2.zero;

        // === Kapatma Butonu (Sağ Üst "X") ===
        GameObject closeBtnGO = new GameObject("CloseButton");
        closeBtnGO.transform.SetParent(paperGO.transform, false);
        Image closeBtnImg = closeBtnGO.AddComponent<Image>();
        closeBtnImg.color = new Color(0.45f, 0.3f, 0.2f, 0.6f);

        Button closeBtn = closeBtnGO.AddComponent<Button>();
        closeBtn.onClick.AddListener(CloseLetterPanel);

        // Hover efekti
        ColorBlock colors = closeBtn.colors;
        colors.highlightedColor = new Color(0.6f, 0.35f, 0.2f, 0.9f);
        colors.pressedColor = new Color(0.3f, 0.15f, 0.1f, 1f);
        closeBtn.colors = colors;

        RectTransform closeBtnRect = closeBtnGO.GetComponent<RectTransform>();
        closeBtnRect.anchorMin = new Vector2(0.9f, 0.9f);
        closeBtnRect.anchorMax = new Vector2(0.98f, 0.98f);
        closeBtnRect.offsetMin = Vector2.zero;
        closeBtnRect.offsetMax = Vector2.zero;

        // X yazısı
        GameObject closeTextGO = new GameObject("CloseText");
        closeTextGO.transform.SetParent(closeBtnGO.transform, false);
        TextMeshProUGUI closeText = closeTextGO.AddComponent<TextMeshProUGUI>();
        closeText.text = "X";
        closeText.fontSize = 24f;
        closeText.fontStyle = FontStyles.Bold;
        closeText.color = new Color(0.9f, 0.85f, 0.7f, 1f);
        closeText.alignment = TextAlignmentOptions.Center;

        RectTransform closeTextRect = closeTextGO.GetComponent<RectTransform>();
        closeTextRect.anchorMin = Vector2.zero;
        closeTextRect.anchorMax = Vector2.one;
        closeTextRect.offsetMin = Vector2.zero;
        closeTextRect.offsetMax = Vector2.zero;

        // === Alt Bilgi: "[ESC] Kapat" ===
        GameObject footerGO = new GameObject("FooterText");
        footerGO.transform.SetParent(paperGO.transform, false);
        TextMeshProUGUI footerText = footerGO.AddComponent<TextMeshProUGUI>();
        footerText.text = isTR ? "[ESC] Kapat" : "[ESC] Close";
        footerText.fontSize = 16f;
        footerText.color = new Color(0.35f, 0.28f, 0.18f, 0.6f);
        footerText.alignment = TextAlignmentOptions.Center;

        RectTransform footerRect = footerGO.GetComponent<RectTransform>();
        footerRect.anchorMin = new Vector2(0.3f, 0.02f);
        footerRect.anchorMax = new Vector2(0.7f, 0.09f);
        footerRect.offsetMin = Vector2.zero;
        footerRect.offsetMax = Vector2.zero;

        // Başlangıçta gizli
        letterCanvas.gameObject.SetActive(false);
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
