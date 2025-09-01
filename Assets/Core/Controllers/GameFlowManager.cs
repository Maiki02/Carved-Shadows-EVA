using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager Instance { get; private set; }

    private AsyncOperation nextSceneLoadingOperation;
    private string nextSceneName = "";
    private GameObject player;

    public bool IsInTransition { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");

        string currentScene = SceneManager.GetActiveScene().name;
        string nextScene = GetNextSceneName(currentScene);

        if (!string.IsNullOrEmpty(nextScene))
        {
            PreloadNextScene(nextScene);
        }

        if (GameController.Instance.CurrentLevel > 1)
        {
            StartCoroutine(HandleLoopIntro());
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[GameFlowManager] Escena cargada: {scene.name}");

        player = GameObject.FindGameObjectWithTag("Player");

        if (GameController.Instance.CurrentLevel > 1)
        {
            StartCoroutine(HandleLoopIntro());
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
                return $"Loop_{nextNumber:D2}";
            }
        }

        Debug.LogWarning("[GameFlowManager] El nombre de la escena no tiene formato válido");
        return null;
    }

    public void SetPreloadedScene(AsyncOperation operation, string sceneName)
    {
        nextSceneLoadingOperation = operation;
        nextSceneName = sceneName;
    }

    public void PreloadNextScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return;

        Debug.Log($"[GameFlowManager] Precargando: {sceneName}");
        nextSceneLoadingOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        nextSceneLoadingOperation.allowSceneActivation = false;
    }

    public void ActivatePreloadedScene()
    {
        if (nextSceneLoadingOperation != null)
        {
            Debug.Log($"[GameFlowManager] Activando escena precargada: {nextSceneName}");
            nextSceneLoadingOperation.allowSceneActivation = true;
        }
        else
        {
            Debug.LogWarning("[GameFlowManager] No hay escena precargada para activar.");
        }
    }


    private IEnumerator HandleLoopIntro()
    {
        Debug.Log("[GameFlowManager] Inicio del loop...");

        SetTransitionStatus(true);

        var player = GameObject.FindWithTag("Player");
        if (player == null) yield break;

        var controller = player.GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.SetControlesActivos(false);
            controller.SetStatusCharacterController(false);
            controller.SetCamaraActiva(false);
        }

        var fakeDoorObj = GameObject.FindWithTag("FakeDoor");
        if (fakeDoorObj != null)
        {
            var fakeDoor = fakeDoorObj.GetComponentInChildren<FakeDoor>();
            if (fakeDoor != null)
            {
                fakeDoor.PlayDoorOpen();
            }
            else
            {
                Debug.LogWarning("[GameFlowManager] FakeDoor encontrada pero no tiene componente FakeDoor.cs en hijos.");
            }
        }
        else
        {
            Debug.LogWarning("[GameFlowManager] No se encontró GameObject con tag 'FakeDoor'");
        }


        yield return new WaitForSeconds(2f); // Ajustar según animación

        if (controller != null)
        {
            controller.SetControlesActivos(true);
            controller.SetStatusCharacterController(true);
            controller.SetCamaraActiva(true);
        }

        SetTransitionStatus(false);
        Debug.Log("[GameFlowManager] Loop listo.");
    }

    public void SetTransitionStatus(bool value)
    {
        IsInTransition = value;
    }

    //////////////////////////////////////////// FIN DE CAM HIZO NUEVO

    /// HOLA DON PEPITO HOLA DON JOSE
    public void ResetGameFlow()
    {
        Debug.Log("Reiniciando flujo del juego...");
        IsInTransition = false;
        if (SceneController.Instance != null)
        {
            SceneController.Instance.LoadInitialScene();
        }

    }


    /// Hay que ver que cosas a partir de aca ya no se usan para limpiar tal vez


    /*public void GoToNextLevel()
    {
        StartCoroutine(NextLevel(this.player));
    }

    private IEnumerator NextLevel(GameObject player)
    {
        IsInTransition = true;

        // Ejecuta toda la secuencia (oscurecer, caer, teleport, abrir ojos)

        yield return StartCoroutine(SubirNivel());


        yield return StartCoroutine(
            FadeManager.Instance.FaintAndLoadRoutine(player, pointToTeleport)
        );

        SetTypeOfDoor();
        IsInTransition = false;
    }

    private IEnumerator SubirNivel()
    {
        yield return new WaitForSeconds(0.5f); // Espera un segundo antes de subir de nivel

        // Ahora que ya terminó todo eso, sube de nivel, activa puertas y rooms
        GameController.Instance.NextLevel();

        ActivateRoom(GameController.Instance.CurrentLevel);
    }



    private void ActivateRoom(int level)
    {
        if (level <= 0 || level > rooms.Count)
        {
            Debug.LogError("Nivel fuera de rango: " + level);
            return;
        }

        // Desactivar todas las habitaciones
        foreach (GameObject room in rooms)
        {
            room.SetActive(false);
        }

        // Activar la habitación correspondiente al nivel actual
        rooms[level - 1].SetActive(true);
    }

    private void ActivateMusic(int level)
    {
        switch (level)
        {
            case 1:
                AudioController.Instance.PlayMusic(AudioType.MusicLevel1, true);
                break;
            case 2:
                AudioController.Instance.PlayMusic(AudioType.MusicLevel2, true);
                break;
            case 3:
                AudioController.Instance.PlayMusic(AudioType.MusicLevel3, true);
                break;
            default:
                Debug.LogWarning("Nivel no reconocido para la música: " + level);
                break;
        }
    }

    private void SetTypeOfDoor()
    {
        if (door == null || initialDoor == null)
        {
            Debug.LogError("No se ha asignado una puerta o puerta inicial al GameFlowManager.");
            return;
        }

        switch (GameController.Instance.CurrentLevel)
        {
            case 1:
                door.SetType(TypeDoorInteract.Key);
                break;
            case 2:
                door.SetType(TypeDoorInteract.None);
                break;
            case 3:
                door.SetType(TypeDoorInteract.None);
                break;
            case 4:
                door.SetType(TypeDoorInteract.None);
                break;
            default:
                Debug.LogWarning("Nivel no reconocido para la puerta: " + GameController.Instance.CurrentLevel);
                break;
        }
        if (GameController.Instance.CurrentLevel == 2)
        {
            this.puertaMuseo1.OpenOrCloseDoor(false); // Aseguramos que la puerta del museo 1 esté cerrada al inicio
            this.puertaMuseo2.OpenOrCloseDoor(false); // Aseguramos que la puerta del museo 2 esté cerrada al inicio
            this.initialDoor.OpenOrCloseDoor(false); // Aseguramos que la puerta inicial esté cerrada al inicio

            puertaMuseo1.SetType(TypeDoorInteract.None);
            puertaMuseo2.SetType(TypeDoorInteract.None);
            initialDoor.SetType(TypeDoorInteract.None); // La puerta inicial siempre es de tipo None

        }
    }*/

}
