using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BasementArrivalTrigger : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Door backDoor;               // la puerta que se cierra detrás
    [SerializeField] private GameObject nextDoorGO;       // objeto Door_NextLoop (inicialmente inactivo)

    [Header("Dizziness")]
    [Tooltip("Si está activo, el mareo se mantiene infinito hasta mantener la tecla (Space) por 'holdToClearSeconds'.")]
    [SerializeField] private bool useHoldToClear = true;
    [SerializeField] private float holdToClearSeconds = 3f;
    [SerializeField, Range(0f,1f)] private float dizzyIntensity = 1f;

    [Tooltip("Si NO usás hold-to-clear, usá una duración finita.")]
    [SerializeField] private float dizzyDuration = 6f;

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
        {
            if (useHoldToClear)
                pc.TriggerDizzinessHoldToClear(dizzyIntensity, holdToClearSeconds);
            else
                pc.TriggerDizziness(dizzyDuration, dizzyIntensity);
        }

        // 2) Cerrar puerta de atrás y ponerla en modo "Close"
        if (backDoor != null)
        {
            backDoor.SetType(TypeDoorInteract.Close);
            backDoor.StartFastClosing();

            if (activateNextDoorAfterReview)
                backDoor.ClosedDoorSequenceCompleted += OnBackDoorReviewed;
        }

        // Asegurar que NextDoor arranque apagada
        if (nextDoorGO != null)
            nextDoorGO.SetActive(false);
    }

    private void OnBackDoorReviewed()
    {
        ActivateNextDoor();

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
