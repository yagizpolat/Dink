using UnityEngine;

public class LevelProgressionManager : MonoBehaviour
{
    [Header("Bölüm sırası")]
    [Tooltip("Build Settings içindeki oynanış sahnelerini sırayla yazın.")]
    [SerializeField] private int[] levelSceneBuildIndexes;

    public bool TryGetNextLevel(out int nextSceneBuildIndex)
    {
        int currentSceneIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
        int currentLevelPosition = System.Array.IndexOf(levelSceneBuildIndexes, currentSceneIndex);

        if (currentLevelPosition < 0)
        {
            Debug.LogError($"Mevcut sahne bölüm listesinde bulunamadı: {currentSceneIndex}");
            nextSceneBuildIndex = -1;
            return false;
        }

        int nextLevelPosition = currentLevelPosition + 1;
        if (nextLevelPosition >= levelSceneBuildIndexes.Length)
        {
            nextSceneBuildIndex = -1;
            return false;
        }

        nextSceneBuildIndex = levelSceneBuildIndexes[nextLevelPosition];
        return true;
    }

    public bool IsFinalLevel()
    {
        int currentSceneIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
        int currentLevelPosition = System.Array.IndexOf(levelSceneBuildIndexes, currentSceneIndex);
        return currentLevelPosition >= 0 && currentLevelPosition == levelSceneBuildIndexes.Length - 1;
    }
}
