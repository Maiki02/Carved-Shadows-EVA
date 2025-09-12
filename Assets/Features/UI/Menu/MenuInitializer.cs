using UnityEngine;
using System.Collections;

public class MenuInitializer : MonoBehaviour
{
    [SerializeField] private GameObject menuRoot;
    [SerializeField] private GameObject[] canvasToActivate;
    [SerializeField] private GameObject menuCamera;
    [SerializeField] private float fadeDuration = 3f;


    private PlayerController playerController;

    IEnumerator Start()
    {
        var playerGO = GameObject.FindWithTag("Player");
        if (playerGO != null)
            playerController = playerGO.GetComponent<PlayerController>();

        if (menuRoot != null) menuRoot.SetActive(false);
        if (menuCamera != null) menuCamera.SetActive(true);

        if (playerController != null)
            playerController.SetControlesActivos(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        yield return null;

        if (menuRoot != null)
            menuRoot.SetActive(true);
    }

    public void StartGame()
    {
        StartCoroutine(StartGameCoroutine());
    }

    /// <summary>
    /// Función separada para hacer solo el fade out del menú sin iniciar el juego completo
    /// </summary>
    public IEnumerator FadeOutMenuOnly()
    {
        GameController.Instance.SetGameStarted(true);
        AudioController.Instance.StopMusic();
        FadeManager.Instance.FadeIn(fadeDuration);

        if (menuRoot != null) menuRoot.SetActive(false);
        if (menuCamera != null) menuCamera.SetActive(false);

        GameFlowManager.Instance.SetTransitionStatus(true);

        if (playerController != null)
        {
            playerController.SetControlesActivos(false);
            playerController.SetCamaraActiva(false);
        }

        yield return new WaitForSeconds(1f); // Tiempo para que el fade comience
    }

    private IEnumerator StartGameCoroutine()
    {
        GameController.Instance.SetGameStarted(true);
        AudioController.Instance.StopMusic();
        FadeManager.Instance.FadeIn(fadeDuration);


        if (menuRoot != null) menuRoot.SetActive(false);
        if (menuCamera != null) menuCamera.SetActive(false);

        GameFlowManager.Instance.SetTransitionStatus(true);

        if (playerController != null)
        {
            playerController.SetControlesActivos(false);
            playerController.SetCamaraActiva(false);
        }

        yield return new WaitForSeconds(2f);

        if (playerController != null)
        {
            playerController.SetControlesActivos(true);
            playerController.SetCamaraActiva(true);
            ShowCanvasToActivate();
            
        }

        GameFlowManager.Instance.SetTransitionStatus(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }



    public void ShowCanvasToActivate()
    {
        if (canvasToActivate != null)
        {
            foreach (var canvas in canvasToActivate)
            {
                if (canvas != null)
                {
                    canvas.SetActive(true);
                }
            }
        }
    }
}
