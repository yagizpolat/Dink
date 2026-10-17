using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 15 Farklı Dinamik İpucu Sunum Sistemi & %4 Artan Fake Hint Algoritması.
/// Sahne yüklendiğinde ipucunun kapı yönünü (Sol/Sağ), 15 sunum tarzından birini ve odadaki konumunu oluşturur.
/// 7 Dilli (TR, EN, DE, FR, ES, PT, RU) çeviri altyapısıyla canlı entegredir.
/// </summary>
public class DynamicHintSystem : MonoBehaviour
{
    public enum HintType
    {
        MektupNot = 0,             // 1. Paslı Mektup & Not
        MorIsikUV = 1,             // 2. UV Mor Işık
        DuvarKufuCatlak = 2,       // 3. Duvar Küfü & Sıva Çatlağı
        TelsizFisilti = 3,         // 4. Telsiz Paraziti & Fısıltı
        BuguAynaMetal = 4,         // 5. Buğulu Ayna & Metal Kazıması
        PelusOyuncak = 6,          // 6. Zeynep'in Peluş Oyuncağı Yönü
        DurmusSaat = 7,            // 7. Durmuş Duvar Saati (Saat 9 = Sol, 3 = Sağ)
        MumAleviGolge = 8,         // 8. Titreyen Mum Alevi & Gölge
        RontgenPano = 9,           // 9. Röntgen Filmi & Işıklı Pano
        TerstenGramofon = 10,      // 10. Tersten Çalan Gramofon
        PusulaIgnesi = 11,         // 11. Paslı Pusula İğnesi
        TabloBakisi = 12,          // 12. Tablodaki Portre Bakışı
        AyakIzi = 13,              // 13. Tozlu Zemindeki Ayak İzi
        SatrencTasi = 14,          // 14. Satranç Tahtası Taş Yönü
        SuDamlasıYansima = 15      // 15. Damlayan Su & Yansıma
    }

    [Header("Odadaki Rastgele İpucu Konumları")]
    [SerializeField] private Transform[] hintSpawnPoints;

    [Header("İpucu Obje Referansları")]
    [SerializeField] private GameObject mektupObj;
    [SerializeField] private GameObject uvIsikYazisiObj;
    [SerializeField] private GameObject pelusOyuncakObj;
    [SerializeField] private GameObject saatObj;
    [SerializeField] private GameObject mumObj;
    [SerializeField] private TextMeshProUGUI ipucuMetni;
    [Header("3D İpucu Obje Verisi")]
    [SerializeField] private HintVisualPropsData hintPropsData;

    [Header("Mevcut Durum")]
    public bool isLeftDoorCorrect = true;
    public bool isFakeHint = false;
    public HintType activeHintType;
    public int currentLetterTextIndex = 0; // O odadaki sabit mektup text index'i

    public static DynamicHintSystem instance { get; private set; }

    private GameObject currentSpawnedProp;
    private GameObject currentSpawnedTable;

    [Header("İpucu Masası")]
    [Tooltip("İpucu objesi havada doğarsa altına loş ışıklı paslı masa kurulur")]
    [SerializeField] private bool useHintTable = true;

    /// <summary>
    /// Objeyi, görsel sınırlarının alt-ortası verilen yüzey noktasına gelecek şekilde taşır.
    /// </summary>
    private static void PlaceOnSurface(GameObject obj, Vector3 surfacePoint)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            obj.transform.position = surfacePoint;
            return;
        }

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);

        Vector3 bottomCenter = new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
        obj.transform.position += surfacePoint - bottomCenter;
    }

    /// <summary>
    /// Nokta zeminden belirgin yüksekteyse (bir mobilyanın üstünde değilse) true döner.
    /// </summary>
    private static bool IsFloating(Vector3 position)
    {
        if (Physics.Raycast(position + Vector3.up * 0.05f, Vector3.down, out RaycastHit hit, 5f, ~0, QueryTriggerInteraction.Ignore))
        {
            return hit.distance > 0.5f;
        }
        return true;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void OtomatikOlustur()
    {
        if (instance != null) return;

        GameObject go = new GameObject("[DynamicHintSystem]");
        go.AddComponent<DynamicHintSystem>();
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
    }

    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        AutoFindSceneReferences();
        GenerateDynamicHint();
    }

    public HintVisualPropsData GetPropsData()
    {
        if (hintPropsData != null) return hintPropsData;

        HintVisualPropsData[] found = Resources.FindObjectsOfTypeAll<HintVisualPropsData>();
        if (found != null && found.Length > 0)
        {
            hintPropsData = found[0];
            return hintPropsData;
        }

#if UNITY_EDITOR
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:HintVisualPropsData");
        if (guids.Length > 0)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
            hintPropsData = UnityEditor.AssetDatabase.LoadAssetAtPath<HintVisualPropsData>(path);
            if (hintPropsData != null) return hintPropsData;
        }
#endif

        return null;
    }

    private void AutoFindSceneReferences()
    {
        // Önceki sahneden kalma ölü (destroyed) objeleri null olarak değerlendirir.
        if (ipucuMetni == null)
        {
            // "true" parametresi, hiyerarşide kapalı (SetActive(false)) olan UI objelerini de bulur.
            TextMeshProUGUI[] texts = FindObjectsOfType<TextMeshProUGUI>(true);
            foreach (var t in texts)
            {
                string tName = t.gameObject.name.ToLower();
                // Olası isimlere göre sahnede ipucu metnini otomatik bul:
                if (tName.Contains("ipucu") || tName.Contains("hint") || tName.Contains("sub") || tName.Contains("text"))
                {
                    ipucuMetni = t;
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Odaya giriş yapıldığında ipucunu, doğru kapı yönünü ve Fake Hint durumunu oluşturur.
    /// </summary>
    public void GenerateDynamicHint()
    {
        UnityEngine.SceneManagement.Scene activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        string sceneName = activeScene.name;

        // Zindan, Menü, Lore veya Ending sahnelerinde ipucu üretme
        if (sceneName.Contains("Zindan") || sceneName.Contains("Menu") || sceneName.Contains("Lore") || sceneName.Contains("Ending"))
        {
            Debug.Log($"[DynamicHint] Oyun dışı sahne ('{sceneName}') algılandı. İpucu üretilmedi.");
            return;
        }

        // Sahne adından gerçek Oda Index'ini hesapla (Zindandan dönüldüğünde hatalı tutorial algısını çözer)
        // "Game" = ana menü, "Game N" = Oda N
        int derivedRoomIndex = DinkScenes.GetRoomIndex(activeScene);
        if (derivedRoomIndex <= 0)
        {
            Debug.Log($"[DynamicHint] Oda olmayan sahne ('{sceneName}') algılandı. İpucu üretilmedi.");
            return;
        }

        if (RoomGraphManager.instance != null)
        {
            RoomGraphManager.instance.currentRoomIndex = derivedRoomIndex;
        }

        bool isTutorial = (derivedRoomIndex <= 3);

        // 1. Doğru Kapı Yönünü Rastgele Seç
        isLeftDoorCorrect = Random.value > 0.5f;

        if (isTutorial)
        {
            Debug.Log($"[DynamicHint] Oda {derivedRoomIndex} Tutorial (Öğretici) olduğu için ipucu objesi üretilmedi.");
            return;
        }

        // 2. %4'ten Başlayıp +%2/Oda Artan Fake Hint Algoritması
        isFakeHint = false;
        if (derivedRoomIndex >= 4)
        {
            float fakeHintChance = 4f + ((derivedRoomIndex - 4) * 2f);
            float roll = Random.Range(0f, 100f);
            if (roll < fakeHintChance)
            {
                isFakeHint = true;
                Debug.Log($"[DynamicHint] ⚠️ FAKE HINT TETİKLENDİ! (Şans: %{fakeHintChance:F1}, Oda: {derivedRoomIndex})");
            }
        }

        // 3. TEST MODU: Sadece ilk 2 ipucu tarzından biri (%50 MektupNot / %50 TelsizFisilti) seçilsin
        activeHintType = (Random.value > 0.5f) ? HintType.MektupNot : HintType.TelsizFisilti;

        // MektupNot seçildiyse bu oda için sabit bir mektup text index'i belirle (her tıklamada değişmesini önler)
        currentLetterTextIndex = Random.Range(0, 18);

        // 4. İpucunu Konumlandır, 3D Objesini Doğru Açıyla Çat ve Metni Hazırla
        ApplyHintVisualsAndText();
    }

    public void ShowHintText()
    {
        if (ipucuMetni != null)
        {
            ipucuMetni.gameObject.SetActive(true);
        }
    }

    private void ApplyHintVisualsAndText()
    {
        // Eski 3D Obje Kalıntısını Temizle
        if (currentSpawnedProp != null)
        {
            Destroy(currentSpawnedProp);
            currentSpawnedProp = null;
        }
        if (currentSpawnedTable != null)
        {
            Destroy(currentSpawnedTable);
            currentSpawnedTable = null;
        }

        // İpucunun gösterdiği görünür yön (Fake Hint ise gerçeğin tersi gösterilir)
        bool shownDirectionLeft = isFakeHint ? !isLeftDoorCorrect : isLeftDoorCorrect;
        LanguageManager.Language activeLang = LanguageManager.instance != null ? LanguageManager.instance.CurrentLanguage : LanguageManager.Language.TR;

        string directionTR = shownDirectionLeft ? "SOL" : "SAĞ";
        string directionEN = shownDirectionLeft ? "LEFT" : "RIGHT";

        string hintStringTR = GetHintTextForType(activeHintType, directionTR, true);
        string hintStringEN = GetHintTextForType(activeHintType, directionEN, false);

        string finalHintText = (activeLang == LanguageManager.Language.TR) ? hintStringTR : hintStringEN;

        if (ipucuMetni != null)
        {
            ipucuMetni.text = finalHintText;
            ipucuMetni.gameObject.SetActive(false);
        }

        // 3D Obje Konumlandırma — Rastgele ve Dinamik Spawn Noktası Seçimi
        HintVisualPropsData data = GetPropsData();
        if (data != null)
        {
            var entry = data.GetPropEntry(activeHintType);
            if (entry != null && entry.propPrefab != null)
            {
                Vector3 propPos = GetRandomSpawnPosition() + entry.localPositionOffset;
                Quaternion propRotation = Quaternion.Euler(shownDirectionLeft ? entry.leftDirectionRotation : entry.rightDirectionRotation);

                // Havada mı kontrolü objeyi oluşturmadan önce yapılır, yoksa ışın objenin kendi collider'ına çarpar
                bool needsTable = useHintTable && IsFloating(propPos);
                currentSpawnedProp = Instantiate(entry.propPrefab, propPos, propRotation);

                // Obje havada kalıyorsa altına loş ışıklı paslı bir masa kur ve objeyi üstüne oturt
                if (needsTable)
                {
                    Quaternion facePlayer = Quaternion.identity;
                    Kamera oyuncuKamera = FindAnyObjectByType<Kamera>();
                    if (oyuncuKamera != null && oyuncuKamera.cam != null)
                    {
                        Vector3 toPlayer = oyuncuKamera.cam.transform.position - propPos;
                        toPlayer.y = 0f;
                        if (toPlayer.sqrMagnitude > 0.001f) facePlayer = Quaternion.LookRotation(toPlayer);
                    }

                    currentSpawnedTable = HintTable.Create(propPos, facePlayer, out Vector3 tableTop).gameObject;
                    PlaceOnSurface(currentSpawnedProp, tableTop);
                }
                HintPropComponent hpc = currentSpawnedProp.GetComponent<HintPropComponent>();
                if (hpc == null)
                {
                    hpc = currentSpawnedProp.AddComponent<HintPropComponent>();
                }
                hpc.Configure(activeHintType, shownDirectionLeft, isFakeHint);
            }
        }

        Debug.Log($"[DynamicHint] 🎨 3D İpucu Objesi Oluşturuldu -> Tarz: {activeHintType}, MektupIndex: {currentLetterTextIndex}, Gösterilen Yön: {(shownDirectionLeft ? "SOL" : "SAĞ")}, Fake mi: {isFakeHint}");
    }

    /// <summary>
    /// Odada dinamik ve değişken ipucu konumu seçer.
    /// Sahnedeki spawn noktalarından veya kameraya göre (oyuncunun baktığı yöne göre) 4 farklı noktadan birini belirler.
    /// Bu sayede ters dönmüş veya farklı koordinattaki odalarda obje harita dışına taşmaz.
    /// </summary>
    private Vector3 GetRandomSpawnPosition()
    {
        // 1. Inspector'da veya sahneden atanmış hintSpawnPoints dizisi varsa içinden rastgele seç
        if (hintSpawnPoints != null && hintSpawnPoints.Length > 0)
        {
            int validCount = 0;
            foreach (var p in hintSpawnPoints) if (p != null) validCount++;
            if (validCount > 0)
            {
                int rIdx = Random.Range(0, hintSpawnPoints.Length);
                if (hintSpawnPoints[rIdx] != null) return hintSpawnPoints[rIdx].position;
            }
        }

        // 2. Sahnedeki "HintSpawnPoint" isimli GameObjects varsa bul ve rastgele seç
        GameObject[] scenePoints = GameObject.FindGameObjectsWithTag("Untagged");
        System.Collections.Generic.List<Vector3> foundPositions = new System.Collections.Generic.List<Vector3>();
        foreach (var go in scenePoints)
        {
            if (go.name.ToLower().Contains("hint") || go.name.ToLower().Contains("spawn"))
            {
                foundPositions.Add(go.transform.position);
            }
        }
        if (foundPositions.Count > 0)
        {
            return foundPositions[Random.Range(0, foundPositions.Count)];
        }

        // 3. Varsayılan Konum (Kameraya / Oyuncuya Göre Bağıl)
        // Odanın koordinatı veya rotasyonu ne olursa olsun obje oyuncunun görüş alanına yakın spawn olur.
        Kamera oyuncuKamera = FindObjectOfType<Kamera>();
        if (oyuncuKamera != null && oyuncuKamera.cam != null)
        {
            Transform camT = oyuncuKamera.cam.transform;
            Vector3 forward = camT.forward;
            Vector3 right = camT.right;
            
            // Y eksenini (yukarı/aşağı bakışı) düzleştir ki obje havada veya yerin dibinde spawn olmasın
            forward.y = 0;
            forward.Normalize();
            right.y = 0;
            right.Normalize();

            // Oyuncunun bulunduğu konuma göre 4 farklı ofset
            Vector3[] relativeOffsets = new Vector3[]
            {
                camT.position + (forward * 1.5f) - (right * 0.5f) + new Vector3(0, -0.2f, 0), // Sol ön (Masa/Duvar hizası)
                camT.position + (forward * 1.5f) + (right * 0.5f) + new Vector3(0, -0.2f, 0), // Sağ ön (Masa/Duvar hizası)
                camT.position + (forward * 1.2f) - new Vector3(0, 0.8f, 0),                   // Tam ön aşağı (Zemin hizası)
                camT.position + (forward * 1.8f) + (right * 0.8f)                             // Biraz daha uzak sağ
            };

            return relativeOffsets[Random.Range(0, relativeOffsets.Length)];
        }

        // Eğer kamera bulunamazsa (çok düşük ihtimal), esneklik için sıfır noktasına yakın spawnla
        return new Vector3(0, 0.5f, 1.0f);
    }

    private string GetHintTextForType(HintType type, string dir, bool isTR)
    {
        switch (type)
        {
            case HintType.MektupNot:
                return isTR ? $"Paslı mektupta 'Yolun sonu {dir} kapıda...' yazıyor." : $"The rusty note reads 'The end lies at the {dir} door...'";
            case HintType.MorIsikUV:
                return isTR ? $"UV Mor ışık {dir} kapı kolunda parlıyor..." : $"UV Purple light glows on the {dir} door handle...";
            case HintType.DuvarKufuCatlak:
                return isTR ? $"Duvar küfü {dir} kapıya doğru tırmanmış..." : $"Wall mold creeps toward the {dir} door...";
            case HintType.TelsizFisilti:
                return isTR ? $"Telsizden bir fısıltı yükseliyor: '{dir} kapıya git...'" : $"A whisper crackles from the radio: 'Go to the {dir} door...'";
            case HintType.BuguAynaMetal:
                return isTR ? $"Buğulu aynada {dir} ok işareti kazınmış..." : $"An arrow pointing {dir} is etched on the foggy mirror...";
            case HintType.PelusOyuncak:
                return isTR ? $"Peluş oyuncağın gözleri {dir} kapıya bakıyor..." : $"The teddy bear's eyes are fixed on the {dir} door...";
            case HintType.DurmusSaat:
                return isTR ? $"Durmuş saatin akrebi {dir} kapıyı gösteriyor..." : $"The stopped clock points to the {dir} door...";
            case HintType.MumAleviGolge:
                return isTR ? $"Mum alevinin gölgesi {dir} kapıya uzanıyor..." : $"The candle flame shadow stretches toward the {dir} door...";
            case HintType.RontgenPano:
                return isTR ? $"Röntgen panosundaki işaret {dir} kapıyı hedefliyor..." : $"The mark on the X-ray panel targets the {dir} door...";
            case HintType.TerstenGramofon:
                return isTR ? $"Gramofondan yükselen melodi {dir} kapıdan yankılanıyor..." : $"The reversed melody echoes from the {dir} door...";
            case HintType.PusulaIgnesi:
                return isTR ? $"Paslı pusulanın iğnesi {dir} kapıya kilitlendi..." : $"The rusty compass needle is locked on the {dir} door...";
            case HintType.TabloBakisi:
                return isTR ? $"Tablodaki portrenin bakışları {dir} kapıya sabitlenmiş..." : $"The portrait's gaze is set on the {dir} door...";
            case HintType.AyakIzi:
                return isTR ? $"Tozlu zemindeki adımlar {dir} kapıya doğru ilerliyor..." : $"Footprints in the dust lead toward the {dir} door...";
            case HintType.SatrencTasi:
                return isTR ? $"Satranç tahtasındaki şah {dir} kapıyı tehdit ediyor..." : $"The king on the chessboard threatens the {dir} door...";
            case HintType.SuDamlasıYansima:
                return isTR ? $"Tavandan damlayan suyun yansıması {dir} kapıyı aydınlatıyor..." : $"Water dripping from the ceiling reflects toward the {dir} door...";
            default:
                return isTR ? $"Bir esinti {dir} kapıdan geliyor..." : $"A breeze comes from the {dir} door...";
        }
    }
}
