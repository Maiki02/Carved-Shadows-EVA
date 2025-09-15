using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BasementArrivalTrigger : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Door backDoor;               // la puerta que se cierra detrás
    [SerializeField] private GameObject nextDoorGO;       // objeto Door_NextLoop (inicialmente inactivo)

    [Header("Dizziness")]
    [SerializeField] private float dizzyDuration = 6f;
    [SerializeField] private float dizzyIntensity = 1f;

    [Header("Behavior")]
    [Tooltip("Si está activo, NextDoor se habilita recién cuando el jugador termine de 'revisar' la puerta cerrada.")]
    [SerializeField] private bool activateNextDoorAfterReview = true;

    private bool triggered = false;

    private void Reset()
    {
        var col = GetComponent<Collider>();
        if (col) col.isTrigger = true;
    }

    private void OnEnable()
    {
        // Por si entramos con la escena ya inicializada
        if (nextDoorGO != null && nextDoorGO.activeSelf)
            nextDoorGO.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;
        StartSequence(other.gameObject);
    }

    private void StartSequence(GameObject playerObj)
    {
        // 1) Mareo al jugador
        var pc = playerObj.GetComponent<PlayerController>();
        if (pc != null)
            pc.TriggerDizziness(dizzyDuration, dizzyIntensity);

        // 2) Cerrar puerta de atrás y ponerla en modo "Close"
        if (backDoor != null)
        {
            backDoor.SetType(TypeDoorInteract.Close);
            backDoor.StartFastClosing();

            // 3) Si activamos NextDoor luego de revisar la puerta, nos suscribimos al evento
            if (activateNextDoorAfterReview)
                backDoor.ClosedDoorSequenceCompleted += OnBackDoorReviewed;
        }

        // Asegurar que NextDoor arranque apagada
        if (nextDoorGO != null)
            nextDoorGO.SetActive(false);
    }

    private void OnBackDoorReviewed()
    {
        // Se invoca cuando la puerta cerrada terminó su secuencia de “está cerrada”
        ActivateNextDoor();

        // Nos desuscribimos y opcionalmente desactivamos el trigger
        if (backDoor != null)
            backDoor.ClosedDoorSequenceCompleted -= OnBackDoorReviewed;

        gameObject.SetActive(false);
    }

    private void ActivateNextDoor()
    {
        if (nextDoorGO != null && !nextDoorGO.activeSelf)
            nextDoorGO.SetActive(true);
    }

    private void OnDisable()
    {
        if (backDoor != null)
            backDoor.ClosedDoorSequenceCompleted -= OnBackDoorReviewed;
    }
}
