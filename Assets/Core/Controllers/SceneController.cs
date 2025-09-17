using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public static SceneController Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /*public void LoadIntroScene()
    {
        // Carga la escena de introducción
        LoadScene("IntroScene");
    }*/

    public void LoadInitialScene()
    {
        SceneManager.LoadScene("Loop_01");
    }

    //La pasamos por parametro para testear distintas.
    public void LoadGameScene(string sceneGame)
    {
        LoadScene(sceneGame);
    }

    public void LoadGameOverScene()
    {
        LoadScene("GameOverScene");
    }

    /*public void LoadHowToPlayScene()
    {
        LoadScene("TutorialScene");
    }*/

    /*public void LoadConfigurationScene()
    {
        LoadScene("ConfigScene");
    }*/

    // Carga la escena de configuración de forma aditiva sobre la escena actual.
    // Esto permite mantener la escena Loop en memoria sin perder el progreso.
    /*public void LoadConfigurationSceneWithAdditive()
    {
        Debug.Log("Loading ConfigScene...");
        // Carga la escena de configuración sin descargar la escena actual
        // LoadSceneMode.Additive hace que ambas escenas coexistan en memoria
        SceneManager.LoadScene("ConfigScene", LoadSceneMode.Additive);
    }

    // Descarga la escena de configuración y retorna el control a la escena Loop anterior.
    // Utiliza una operación asíncrona para evitar bloqueos en el hilo principal.
    public void UnloadConfigAsync()
    {
        Debug.Log("Unloading ConfigScene...");
        // Descarga solo la escena de configuración de forma asíncrona
        // La operación completed se ejecuta cuando la descarga termine
        SceneManager.UnloadSceneAsync("ConfigScene").completed += _ =>
        {
            // Obtiene la escena que Unity considera como "activa" en este momento
            Scene activeScene = SceneManager.GetActiveScene();
            
            // Verifica si necesitamos cambiar la escena activa
            // Esto puede ocurrir si Unity no restauró automáticamente la escena Loop como activa
            if (!activeScene.isLoaded || activeScene.name == "ConfigScene")
            {
                // Itera a través de todas las escenas cargadas en memoria
                for (int i = 0; i < SceneManager.sceneCount; i++)
                {
                    // Obtiene la escena en la posición 'i'
                    Scene scene = SceneManager.GetSceneAt(i);
                    
                    // Verifica si la escena está cargada y es una escena Loop
                    // StartsWith("Loop_") detecta Loop_01, Loop_02, Loop_03, Loop_04, etc.
                    if (scene.isLoaded && scene.name.StartsWith("Loop_"))
                    {
                        // Establece esta escena Loop como la escena activa
                        // Esto asegura que el jugador retorne al gameplay
                        SceneManager.SetActiveScene(scene);
                        break; // Sale del bucle una vez encontrada la primera escena Loop
                    }
                }
            }
        };
    }*/

    public void LoadMenuScene()
    {
        LoadScene("MenuScene");
    }

    public void QuitGame()
    {
        Debug.Log("Saliendo...");
        Application.Quit();
    }



    public void LoadScene(string sceneName)
    {
        //previusSceneName = SceneManager.GetActiveScene().name;

        SceneManager.LoadScene(sceneName);

    }

    public bool IsInScene(string sceneName)
    {
        return SceneManager.GetActiveScene().name == sceneName;
    }

}
