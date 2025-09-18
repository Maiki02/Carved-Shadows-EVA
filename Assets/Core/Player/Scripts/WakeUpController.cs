using System.Collections;
using NUnit.Framework.Constraints;
using UnityEngine;

public class WakeUpController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        StartCoroutine(AwakeningPlayer());
    }

    private IEnumerator AwakeningPlayer()
    {
// Configurar estados del juego
        GameController.Instance.SetGameStarted(true);
        GameFlowManager.Instance.SetTransitionStatus(true);

        // NUEVA FUNCIONALIDAD: Cambio instantáneo de cámaras usando Cinemachine
        //SwitchToPlayerCameraInstantly();

        Debug.Log("[MenuController] Cambiando a cámara del player sin transición");

        // Configurar jugador
        var playerGO = GameObject.FindWithTag("Player");
        if (playerGO != null)
        {
            var playerController = playerGO.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.SetCamaraActiva(true);
            }
        }

        Debug.Log("[MenuController] Activando canvas del juego y comenzando secuencia de despertar");
        // Activar canvas del juego
        ShowCanvasToActivate();

        GameFlowManager.Instance.SetTransitionStatus(false);

        // NUEVA FUNCIONALIDAD: Iniciar secuencia de despertar del protagonista
        if (playerGO != null)
        {
            var wakeUpComponent = playerGO.GetComponent<PlayerWakeUpSequence>();
            if (wakeUpComponent != null)
            {
                Debug.Log("[MenuController] Iniciando secuencia de despertar del protagonista");
                wakeUpComponent.StartWakeUpSequence();
            }
            else
            {
                Debug.LogWarning("[MenuController] PlayerWakeUpSequence no encontrado en el Player");
            }
        }

        yield return null;

    }

    private void ShowCanvasToActivate()
    {
        var point = GameObject.FindWithTag("CanvasPoint");
        if (point != null)
        {
            point.SetActive(true);
        }
    }
}
