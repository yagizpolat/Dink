using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

/// <summary>
/// Sinematik giriş sekansı için gerekli UI elemanlarını (siyah ekran, göz bantları),
/// URP Volume (Depth of Field bulanıklık) ve IntroCinematic bileşenini tek tıkla kurar.
/// Renk overlay yok, saf URP Gaussian Blur kullanır.
/// </summary>
public class IntroCinematicSetup
{
    [MenuItem("Tools/Dink/Arayuz/Sinematik Giris Sekansini Kur (Intro Cinematic)")]
    public static void SetupIntroCinematic()
    {
        // 1. CANVAS BUL
        Canvas canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("[Dink Sinematik] Sahnede Canvas bulunamadı!");
            return;
        }

        Transform canvasTransform = canvas.transform;

        // 2. ESKİ INTRO ELEMANLARINI TEMİZLE
        DestroyIfExists(canvasTransform, "Cinematic_BlackScreen");
        DestroyIfExists(canvasTransform, "Cinematic_EyeTopBar");
        DestroyIfExists(canvasTransform, "Cinematic_EyeBottomBar");
        DestroyIfExists(canvasTransform, "Cinematic_BlurOverlay"); // Eski renkli overlay varsa sil

        // 3. SİYAH TAM EKRAN PANELİ
        GameObject blackScreen = CreateFullScreenPanel(canvasTransform, "Cinematic_BlackScreen",
            new Color(0f, 0f, 0f, 1f), 100);

        // 4. GÖZ AÇILIŞI ÜST BANT (Eye Top Bar)
        GameObject eyeTop = new GameObject("Cinematic_EyeTopBar");
        eyeTop.transform.SetParent(canvasTransform, false);
        RectTransform eyeTopRect = eyeTop.AddComponent<RectTransform>();
        eyeTopRect.anchorMin = new Vector2(0f, 1f);
        eyeTopRect.anchorMax = new Vector2(1f, 1f);
        eyeTopRect.pivot = new Vector2(0.5f, 1f);
        eyeTopRect.anchoredPosition = Vector2.zero;
        eyeTopRect.sizeDelta = new Vector2(0f, Screen.height);
        Image eyeTopImg = eyeTop.AddComponent<Image>();
        eyeTopImg.color = Color.black;
        eyeTopImg.raycastTarget = false;
        SetSortOrder(eyeTop, 101);

        // 5. GÖZ AÇILIŞI ALT BANT (Eye Bottom Bar)
        GameObject eyeBottom = new GameObject("Cinematic_EyeBottomBar");
        eyeBottom.transform.SetParent(canvasTransform, false);
        RectTransform eyeBottomRect = eyeBottom.AddComponent<RectTransform>();
        eyeBottomRect.anchorMin = new Vector2(0f, 0f);
        eyeBottomRect.anchorMax = new Vector2(1f, 0f);
        eyeBottomRect.pivot = new Vector2(0.5f, 0f);
        eyeBottomRect.anchoredPosition = Vector2.zero;
        eyeBottomRect.sizeDelta = new Vector2(0f, Screen.height);
        Image eyeBottomImg = eyeBottom.AddComponent<Image>();
        eyeBottomImg.color = Color.black;
        eyeBottomImg.raycastTarget = false;
        SetSortOrder(eyeBottom, 101);

        // 6. URP VOLUME - DEPTH OF FIELD (SAF BULANIKLIK, RENK YOK)
        // Sahnede zaten bir Volume varsa onu kullan, yoksa yeni oluştur
        Volume existingVolume = null;
        GameObject blurVolumeObj = GameObject.Find("Cinematic_BlurVolume");
        if (blurVolumeObj != null)
        {
            Undo.DestroyObjectImmediate(blurVolumeObj);
        }

        blurVolumeObj = new GameObject("Cinematic_BlurVolume");
        blurVolumeObj.transform.SetParent(canvasTransform.parent, false); // Canvas dışında, sahne kökünde
        existingVolume = blurVolumeObj.AddComponent<Volume>();
        existingVolume.isGlobal = true;
        existingVolume.priority = 10; // Diğer volume'ların üstüne geçsin

        // Yeni bir VolumeProfile oluştur
        VolumeProfile profile = ScriptableObject.CreateInstance<VolumeProfile>();
        DepthOfField dof = profile.Add<DepthOfField>(true);
        dof.mode.Override(DepthOfFieldMode.Gaussian);
        dof.gaussianStart.Override(0.1f);
        dof.gaussianEnd.Override(5f);
        dof.gaussianMaxRadius.Override(1.0f);

        existingVolume.profile = profile;
        Undo.RegisterCreatedObjectUndo(blurVolumeObj, "Created Cinematic Blur Volume");

        // 7. INTRO CINEMATIC MANAGER OLUŞTUR
        GameObject managersParent = GameObject.Find("Managers");
        if (managersParent == null)
        {
            managersParent = GameObject.Find("[--- YÖNETİCİLER & SİSTEM ---]");
        }

        Transform managerTarget = managersParent != null ? managersParent.transform : canvasTransform;

        // Eski varsa sil
        IntroCinematic existingIntro = Object.FindObjectOfType<IntroCinematic>();
        if (existingIntro != null)
        {
            Undo.DestroyObjectImmediate(existingIntro.gameObject);
        }

        GameObject introObj = new GameObject("IntroCinematic");
        introObj.transform.SetParent(managerTarget, false);
        IntroCinematic intro = introObj.AddComponent<IntroCinematic>();
        Undo.RegisterCreatedObjectUndo(introObj, "Created IntroCinematic");

        // 8. OTOMATİK REFERANS BAĞLAMA
        Kamera kameraScript = Object.FindObjectOfType<Kamera>();
        if (kameraScript != null)
        {
            intro.kameraScript = kameraScript;
            if (kameraScript.cam != null)
            {
                intro.cameraTransform = kameraScript.cam.transform;
            }
        }

        FenerKontrol fenerScript = Object.FindObjectOfType<FenerKontrol>();
        if (fenerScript != null)
        {
            intro.fenerScript = fenerScript;
        }

        SubtitleManager subManager = Object.FindObjectOfType<SubtitleManager>();
        if (subManager != null)
        {
            intro.subtitleManager = subManager;
        }

        // UI ve Volume Referansları
        intro.blackScreen = blackScreen.GetComponent<Image>();
        intro.eyeTopBar = eyeTopRect;
        intro.eyeBottomBar = eyeBottomRect;
        intro.blurVolume = existingVolume;

        EditorUtility.SetDirty(intro);
        Selection.activeGameObject = introObj;

        Debug.Log("[Dink Sinematik] Giriş sekansı (URP Depth of Field ile saf bulanıklık) başarıyla kuruldu!");
    }

    private static GameObject CreateFullScreenPanel(Transform parent, string name, Color color, int sortOrder)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);

        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;

        Image img = panel.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = false;

        SetSortOrder(panel, sortOrder);
        return panel;
    }

    private static void SetSortOrder(GameObject obj, int order)
    {
        Canvas c = obj.AddComponent<Canvas>();
        c.overrideSorting = true;
        c.sortingOrder = order;
    }

    private static void DestroyIfExists(Transform parent, string name)
    {
        Transform child = parent.Find(name);
        if (child != null) Undo.DestroyObjectImmediate(child.gameObject);
    }
}
