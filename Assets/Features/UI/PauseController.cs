using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PauseController : MonoBehaviour
{
    public static PauseController Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private GameObject firstPauseSelected; // botón “Resume”, por ejemplo

    [Header("Input System")]
    [SerializeField] private InputActionReference pauseAction;    // Gameplay/Pause  (Esc / Start)
    [SerializeField] private InputActionReference uiCancelAction; // UI/Cancel      (Esc / B)

    [Header("Binder (opcional)")]
    [SerializeField] private UIInputBinder uiBinder; // si no lo asignás, lo busca solo

    private bool isShowConfig = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    void OnEnable()
    {
        pauseAction?.action.Enable();
        uiCancelAction?.action.Enable();
    }

    void OnDisable()
    {
        pauseAction?.action.Disable();
        uiCancelAction?.action.Disable();
    }

    void Update()
    {
        // Abrir/cerrar pausa desde gameplay
        if (pauseAction != null && pauseAction.action.WasPressedThisFrame())
        {
            TogglePauseMenu();
        }

        // Si ya estoy en pausa, la tecla Cancel hace "atrás"
        if (GameController.Instance != null && GameController.Instance.IsPaused())
        {
            if (uiCancelAction != null && uiCancelAction.action.WasPressedThisFrame())
            {
                if (isShowConfig) CloseConfiguration();
                else              ResumeGame();
            }
        }
    }

    public void SetShowPauseUI(bool show)
    {
        if (!pauseMenuUI) return;
        pauseMenuUI.SetActive(show);

        if (show)
        {
            GetBinder()?.SwitchToUI();
            if (firstPauseSelected) StartCoroutine(SelectNextFrame(firstPauseSelected));
        }
        else
        {
            if (EventSystem.current) EventSystem.current.SetSelectedGameObject(null);
        }
    }

    public void ResumeGame() => TogglePauseMenu();

    public void OpenConfiguration()
    {
        SetShowPauseUI(false);
        isShowConfig = true;
        ConfigController.Instance?.ShowConfiguration();
    }

    public void CloseConfiguration()
    {
        ConfigController.Instance?.HideConfiguration();
        isShowConfig = false;
        SetShowPauseUI(true);
    }

    public void ExitToMainMenu()
    {
        TogglePauseMenu();
        GameController.Instance.SetGameStarted(false);
        GameController.Instance.ResetValues();
        GameFlowManager.Instance.ResetGameFlow();
        SceneController.Instance.LoadMenuScene();
    }

    public void TogglePauseMenu()
    {
        if (!GameController.Instance.IsGameStarted()) return;

        bool isPaused = !GameController.Instance.IsPaused();
        GameController.Instance.SetIsPaused(isPaused);
        SetShowPauseUI(isPaused);

        if (isPaused)
        {
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible   = true;

            GetBinder()?.SwitchToUI();
            if (firstPauseSelected) StartCoroutine(SelectNextFrame(firstPauseSelected));
        }
        else
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible   = false;

            GetBinder()?.SwitchToGameplay();
            if (EventSystem.current) EventSystem.current.SetSelectedGameObject(null);
        }
    }

    // -------- helpers --------
    private IEnumerator SelectNextFrame(GameObject go)
    {
        yield return null;
        if (go && EventSystem.current)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(go);
        }
    }

    private UIInputBinder GetBinder()
    {
        if (uiBinder) return uiBinder;
#if UNITY_2023_1_OR_NEWER
        uiBinder = FindAnyObjectByType<UIInputBinder>();
#else
        uiBinder = FindObjectOfType<UIInputBinder>();
#endif
        return uiBinder;
    }
}
