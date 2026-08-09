using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Unity Editor içinde tek tıkla sahnedeki tüm metin bileşenlerini (TextMeshPro ve UI Text) tarar,
/// eksik olanlara 'LocalizedText' bileşenini otomatik ekler ve Türkçe (trText) alanlarını
/// mevcut metinle otomatik doldurur.
/// </summary>
public class LocalizationHelper
{
    [MenuItem("Tools/Dink/Localization/Sahnedeki Tum Metinlere LocalizedText Ekle")]
    public static void AddLocalizationToAllTexts()
    {
        int addedCount = 0;
        int updatedCount = 0;

        // 1. TextMeshProUGUI Bileşenlerini Tara
        TextMeshProUGUI[] allTMPs = Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var tmp in allTMPs)
        {
            if (string.IsNullOrEmpty(tmp.text)) continue;

            LocalizedText loc = tmp.GetComponent<LocalizedText>();
            if (loc == null)
            {
                loc = Undo.AddComponent<LocalizedText>(tmp.gameObject);
                addedCount++;
            }

            if (string.IsNullOrEmpty(loc.trText))
            {
                Undo.RecordObject(loc, "Auto Fill TR Text");
                loc.trText = tmp.text;
                updatedCount++;
            }
        }

        // 2. Legacy UI Text Bileşenlerini Tara
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

            if (string.IsNullOrEmpty(loc.trText))
            {
                Undo.RecordObject(loc, "Auto Fill TR Text");
                loc.trText = txt.text;
                updatedCount++;
            }
        }

        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        EditorUtility.DisplayDialog(
            "Dil Altyapısı Tamamlandı",
            $"İşlem Başarılı!\n\n- Eklenecek LocalizedText Sayısı: {addedCount}\n- Otomatik Doldurulan Türkçe Metin Sayısı: {updatedCount}\n\nŞimdi tek yapman gereken Inspector'da İngilizce (enText) karşılıklarını girmek!",
            "Harika"
        );

        Debug.Log($"[Dink] Dil Otomasyonu: {addedCount} objeye LocalizedText eklendi, {updatedCount} metin trText olarak dolduruldu.");
    }
}
