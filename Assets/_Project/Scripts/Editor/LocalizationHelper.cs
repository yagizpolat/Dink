using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Unity Editor Otomatik Dil Tarama Aracı:
/// Dink Tools -> Dil Sistemini Tara ve LocalizedText Ekle
/// 
/// Sahnedeki tüm 2D UI ve 3D TextMeshPro metin bileşenlerini tarar.
/// Eksik olanlara 'LocalizedText' bileşenini ekler ve trText / enText alanlarını
/// mevcut metinle doldurur.
/// </summary>
public class LocalizationHelper
{
    [MenuItem("Dink Tools/Dil Sistemini Tara ve LocalizedText Ekle")]
    [MenuItem("Tools/Dink/Localization/Sahnedeki Tum Metinlere LocalizedText Ekle")]
    public static void AddLocalizationToAllTexts()
    {
        int addedCount = 0;
        int updatedCount = 0;

        // 1. TextMeshProUGUI Bileşenlerini Tara (2D UI)
        TextMeshProUGUI[] allUGUIs = Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var tmp in allUGUIs)
        {
            if (string.IsNullOrEmpty(tmp.text)) continue;

            LocalizedText loc = tmp.GetComponent<LocalizedText>();
            if (loc == null)
            {
                loc = Undo.AddComponent<LocalizedText>(tmp.gameObject);
                addedCount++;
            }

            if (string.IsNullOrEmpty(loc.trText) && string.IsNullOrEmpty(loc.enText))
            {
                Undo.RecordObject(loc, "Auto Fill Text");
                loc.trText = tmp.text;
                loc.enText = tmp.text;
                updatedCount++;
            }
        }

        // 2. TextMeshPro Bileşenlerini Tara (3D Kapı Yazıları vb.)
        TextMeshPro[] all3Ds = Object.FindObjectsByType<TextMeshPro>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var tmp in all3Ds)
        {
            if (string.IsNullOrEmpty(tmp.text)) continue;

            LocalizedText loc = tmp.GetComponent<LocalizedText>();
            if (loc == null)
            {
                loc = Undo.AddComponent<LocalizedText>(tmp.gameObject);
                addedCount++;
            }

            if (string.IsNullOrEmpty(loc.trText) && string.IsNullOrEmpty(loc.enText))
            {
                Undo.RecordObject(loc, "Auto Fill Text");
                loc.trText = tmp.text;
                loc.enText = tmp.text;
                updatedCount++;
            }
        }

        // 3. Legacy UI Text Bileşenlerini Tara
        Text[] allTexts = Object.FindObjectsByType<Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var txt in allTexts)
        {
            if (string.IsNullOrEmpty(txt.text)) continue;

            LocalizedText loc = txt.GetComponent<LocalizedText>();
            if (loc == null)
            {
                loc = Undo.AddComponent<LocalizedText>(txt.gameObject);
                addedCount++;
            }

            if (string.IsNullOrEmpty(loc.trText) && string.IsNullOrEmpty(loc.enText))
            {
                Undo.RecordObject(loc, "Auto Fill Text");
                loc.trText = txt.text;
                loc.enText = txt.text;
                updatedCount++;
            }
        }

        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        EditorUtility.DisplayDialog(
            "Dil Altyapısı Başarıyla Tarandı",
            $"İşlem Başarılı!\n\n- Eklenecek LocalizedText Sayısı: {addedCount}\n- Otomatik Doldurulan Metin Sayısı: {updatedCount}\n\nDesteklenen 7 Dil: EN (Varsayılan), TR, DE, FR, ES, PT, RU",
            "Tamam"
        );

        Debug.Log($"<color=green>[DINK] Dil Otomasyonu: {addedCount} objeye LocalizedText eklendi, {updatedCount} metin dolduruldu.</color>");
    }
}
