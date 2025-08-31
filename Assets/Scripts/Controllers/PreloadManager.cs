using UnityEngine;
using UnityEngine.SceneManagement;

public class PreloadManager : MonoBehaviour
{
    [SerializeField] private bool autoPreload = true;

    void Start()
    {
        if (autoPreload)
        {
            string currentScene = SceneManager.GetActiveScene().name;
            string nextScene = GetNextSceneName(currentScene);

            if (!string.IsNullOrEmpty(nextScene))
            {
                Debug.Log($"[PreloadManager] Precargando: {nextScene}");
                AsyncOperation preload = SceneManager.LoadSceneAsync(nextScene, LoadSceneMode.Single);
                preload.allowSceneActivation = false;

                GameFlowManager.Instance.SetPreloadedScene(preload, nextScene);
            }
            else
            {
                Debug.Log("[PreloadManager] No hay siguiente escena que precargar.");
            }
        }
    }

    private string GetNextSceneName(string currentScene)
    {
        if (currentScene.StartsWith("Loop_"))
        {
            string[] parts = currentScene.Split('_');
            if (parts.Length == 2 && int.TryParse(parts[1], out int currentNumber))
            {
                int nextNumber = currentNumber + 1;
                string nextScene = $"Loop_{nextNumber:D2}";

                if (Application.CanStreamedLevelBeLoaded(nextScene))
                {
                    return nextScene;
                }
            }
        }
        return null;
    }
}

