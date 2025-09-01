using UnityEngine;

public class MenuController : MonoBehaviour
{
    private MenuInitializer initializer;

    private void Awake()
    {
        initializer = FindObjectOfType<MenuInitializer>();
    }

    public void OnStartButton()
    {
        initializer?.StartGame();
    }

    public void OnConfigurationButton()
    {
        // Mostrar la configuración usando el Singleton
        if (ConfigController.Instance != null)
        {
            ConfigController.Instance.ShowConfiguration();
        }
        else
        {
            Debug.LogWarning("ConfigController.Instance no encontrado en la escena del menú");
        }
    }
    

    public void OnExitButton()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }
}
