using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    [SerializeField] private float maxDistance = 5f;
    private IInteractable currentInteractable;
    public Camera cameraChildren;

    public float interactionDistance = 5f;
    public LayerMask interactableLayer;
    public Transform inspectionPoint;

    //private PadlockInspectable candadoActual;

    void Awake()
    {
        cameraChildren = GetComponentInChildren<Camera>();
        if (cameraChildren == null)
        {
            Debug.LogError("Camera transform not found in PlayerInteractor.");
        }
    }

    void Update()
    {
       if(GameController.Instance.IsPaused() || GameFlowManager.Instance.IsInTransition) return;
        // Lanzar rayo desde el centro de la pantalla
        Ray ray = cameraChildren.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f));
        Debug.DrawRay(ray.origin, ray.direction * interactionDistance, Color.red, 1f);

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance))
        {
            var interactable = hit.collider.GetComponent<IInteractable>();
            Debug.Log("Raycast hit: " + hit.collider.name);
            if (interactable != null)
            {
                
                // Si es distinto al anterior, actualizar hover
                if (interactable != currentInteractable)
                {
                    // currentInteractable?.OnHoverExit();
                    currentInteractable = interactable;
                    // currentInteractable.OnHoverEnter();
                    Debug.Log("NEW interactable detected: " + interactable.GetType().Name);
                    
                    // Mostrar indicador UI animado
                    if (CrosshairAnimator.Instance != null)
                    {
                        CrosshairAnimator.Instance.StartInteractionAnimation();
                    }
                }

                // Interactuar al presionar E
                if (EstaInteractuando())
                {
                    currentInteractable.OnInteract();
                    //Debug.Log("Interactuamos con: " + currentInteractable.GetType().Name);
                }

                return;
            }

            /*var candado = hit.collider.GetComponentInParent<PadlockInspectable>();
            if (candado is PadlockInspectable padlock)
            {
                candadoActual = padlock;
                candadoActual.OnHoverEnter();

                if (EstaInteractuando())
                {
                    candadoActual.OnInteract();
                    //Debug.Log("Interactuamos con: " + currentInteractable.GetType().Name);
                }
            }
            else
            {
                candadoActual?.OnHoverExit();
            }*/

        }

        // Si no hay ningún interactable bajo el rayo, desactivar outline
        if (currentInteractable != null)
        {
            Debug.Log("No interactable under ray. Exiting hover state.");
            // currentInteractable.OnHoverExit();
            currentInteractable = null;
            
            // Ocultar indicador UI
            if (CrosshairAnimator.Instance != null)
            {
                CrosshairAnimator.Instance.StopInteractionAnimation();
            }
        }
    }

    private bool EstaInteractuando() {
        return Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0);
    }
}
