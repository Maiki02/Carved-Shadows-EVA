using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public static GameController Instance { get; private set; }
    private bool isGameStarted = false; // Indica si el juego ha comenzado
    private bool isPaused = false; // Indica si el juego está en pausa

    public int Score { get; private set; } = 0; // Puntaje del jugador //Creo que no se usa, pero lo dejo por si acaso

    public int CurrentLevel { get; private set; } = 1; // Nivel actual del juego //Con esto seteamos distintas rooms
    [SerializeField] private int maxLevels = 4; // Número máximo de niveles

    public float MouseSensitivity { get; set; } = 400f; // Sensibilidad del mouse
    public bool InvertMouse { get; set; } = false; // Invertir el movimiento del mouse
    public bool IsInspecting { get; set; } = false; // Indica si el jugador está inspeccionando un objeto

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /*public void StartGame()
    {
        this.ResetValues();

        SceneController.Instance.LoadIntroScene();
        this.SetGameStarted(true);
    }*/

    public void FinishGame()
    {
        //Hacemos un FadeIn
        FadeManager.Instance.FadeIn(0.5f);

        this.ResetValues();

        //Cambiamos de escena
        SceneController.Instance.LoadGameOverScene();

        

        //SceneController.Instance.LoadGameOverScene();
    }

    /*public void ResetGame(bool reloadScene = false)
    {
        if (reloadScene)
        {
            this.StartGame();
        }
    }*/

    public void SetGameStarted(bool started)
    {
        isGameStarted = started;
    }

    public void ResetValues()
    {
        Score = 0;
        CurrentLevel = 1;
        isPaused = false;
        Time.timeScale = 1f; // Asegurarse de que el tiempo se reanude
    }

    public bool IsGameStarted()
    {
        return isGameStarted;
    }

    public void SetIsPaused(bool paused)
    {
        isPaused = paused;
    }

    public bool IsPaused()
    {
        return isPaused;
    }

    public void NextLevel()
    {
        CurrentLevel++;

        if (CurrentLevel > maxLevels)
        {
            this.FinishGame();
        }
    }

}
