using UnityEngine;

/// <summary>
/// Script auxiliar para reenviar eventos de trigger desde colliders hijos
/// al script principal DoorStatueSequence en el padre.
/// </summary>
[RequireComponent(typeof(Collider))]
public class ChildTriggerRelay : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private DoorStatueSequence parentSequence;
    
    private void Reset()
    {
        // Asigna automáticamente el padre si es posible
        if (parentSequence == null)
        {
            parentSequence = GetComponentInParent<DoorStatueSequence>();
        }
        
        // Asegurar que el collider sea un trigger
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }
    
    private void Awake()
    {
        // Validar configuración
        if (parentSequence == null)
        {
            parentSequence = GetComponentInParent<DoorStatueSequence>();
        }
        // Asegurar que el collider sea un trigger
        Collider col = GetComponent<Collider>();
        if (col != null && !col.isTrigger)
        {
            col.isTrigger = true;
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (parentSequence != null)
        {
            parentSequence.OnChildTriggerEnter(other);
        }
    }
    
    /// <summary>
    /// Permite asignar manualmente la referencia al padre
    /// </summary>
    public void SetParentSequence(DoorStatueSequence parent)
    {
        parentSequence = parent;
    }
}