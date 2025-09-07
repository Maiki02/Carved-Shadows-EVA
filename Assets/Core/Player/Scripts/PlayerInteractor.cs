using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class PlayerInteractor : MonoBehaviour
{
    [SerializeField] private float maxDistance = 5f;
    [SerializeField] private Camera cameraChildren;

    [Header("Input System")]
    [SerializeField] private InputActionReference interactAction; // Gameplay/Interact

    private IInteractable currentInteractable;

    void Awake()
    {
        if (cameraChildren == null)
            cameraChildren = GetComponentInChildren<Camera>();

        if (cameraChildren == null)
            Debug.LogError("Camera transform not found in PlayerInteractor.");
    }

    void OnEnable()  => interactAction?.action.Enable();
    void OnDisable() => interactAction?.action.Disable();

    void Update()
    {
        if (GameController.Instance.IsPaused() || GameFlowManager.Instance.IsInTransition) return;
        if (cameraChildren == null) return;

        // 1) Rayo desde el centro
        var ray = cameraChildren.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f));

        // 2) Buscamos el PRIMER hit (por distancia) que tenga un IInteractable en su jerarquía
        IInteractable found = null;
        RaycastHit pickedHit = default;

        var hits = Physics.RaycastAll(ray, maxDistance, ~0, QueryTriggerInteraction.Ignore);
        if (hits != null && hits.Length > 0)
        {
            Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            foreach (var h in hits)
            {
                if (TryFindInteractable(h, out found))
                {
                    pickedHit = h;
                    break;
                }
            }
        }

        if (found != null)
        {
            // Si cambió el objetivo, refrescá el hover
            if (found != currentInteractable)
            {
                // currentInteractable?.OnHoverExit(); // si lo usás
                currentInteractable = found;
                // currentInteractable.OnHoverEnter(); // si lo usás
                CrosshairAnimator.Instance?.StartInteractionAnimation();
            }

            if (interactAction != null && interactAction.action.WasPressedThisFrame())
            {
                currentInteractable.OnInteract();
            }

            Debug.DrawRay(ray.origin, ray.direction * pickedHit.distance, Color.green, 0f);
            return;
        }

        // 3) No hay interactuable bajo el rayo
        if (currentInteractable != null)
        {
            // currentInteractable.OnHoverExit(); // si lo usás
            currentInteractable = null;
            CrosshairAnimator.Instance?.StopInteractionAnimation();
        }

        Debug.DrawRay(ray.origin, ray.direction * maxDistance, Color.red, 0f);
    }

    private static bool TryFindInteractable(in RaycastHit hit, out IInteractable interactable)
    {
        // a) collider directo
        if (hit.collider.TryGetComponent(out interactable)) return true;

        // b) attached rigidbody (útil cuando los colliders están en hijos)
        var rb = hit.collider.attachedRigidbody;
        if (rb && rb.TryGetComponent(out interactable)) return true;

        // c) padres del collider
        interactable = hit.collider.GetComponentInParent<IInteractable>();
        if (interactable != null) return true;

        // d) hijos del collider
        interactable = hit.collider.GetComponentInChildren<IInteractable>();
        return interactable != null;
    }
}
