using UnityEngine;

public class FinishGameController : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Verificar si el objeto que entró al trigger es el player
        if (other.CompareTag("Player"))
        {
            // Finalizar el juego llamando a NextLevel
            GameController.Instance.FinishGame();
        }
    }
}