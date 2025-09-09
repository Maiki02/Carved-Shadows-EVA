using UnityEngine;
using System.Collections;

public class MenuController : MonoBehaviour
{
    private MenuInitializer initializer;

    private void Awake()
    {
        initializer = FindFirstObjectByType<MenuInitializer>();
    }

    public void OnStartButton()
    {
        StartCoroutine(StartGameWithIntro());
    }

    private IEnumerator StartGameWithIntro()
    {
        // 1. Buscar y ejecutar la intro
        IntroManager introManager = FindFirstObjectByType<IntroManager>();
        if (introManager != null)
        {
            Debug.Log("Iniciando secuencia de introducción...");
            
            // Esperar a que termine la intro completamente
            yield return StartCoroutine(WaitForIntroToComplete(introManager));
            
            Debug.Log("Introducción completada, iniciando juego...");
        }
        else
        {
            Debug.LogWarning("IntroManager no encontrado, iniciando juego directamente");
        }

        // 2. Iniciar el juego después de la intro
        initializer?.StartGame();
    }

    private IEnumerator WaitForIntroToComplete(IntroManager introManager)
    {
        // Obtener la duración total de la intro desde el IntroManager
        float introDuration = introManager.GetComponent<IntroManager>() ? 
            GetIntroDuration(introManager) : 10f; // Fallback de 10 segundos
        
        // Esperar a que termine la intro
        yield return new WaitForSeconds(introDuration);
    }

    private float GetIntroDuration(IntroManager introManager)
    {
        // Obtener la duración total del IntroManager
        return introManager.GetTotalIntroDuration();
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
