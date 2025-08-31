using System.Collections;
using UnityEngine;

/// <summary>
/// Controla una secuencia completa de puerta-estatua mediante triggers.
/// Cuando el jugador entra a cualquier trigger hijo, cierra la puerta rápidamente y activa la estatua.
/// Los triggers pueden estar en GameObjects hijos con ChildTriggerRelay.
/// </summary>
public class DoorStatueSequence : MonoBehaviour
{
    [Header("Referencias principales")]
    [SerializeField] private Door targetDoor; // La puerta que se cerrará
    [SerializeField] private GameObject targetStatue; // La estatua que aparecerá
    
    [Header("Configuración del trigger")]
    [SerializeField] private bool triggerOnce = true; // Si se activa solo una vez
    
    [Header("Configuración de audio (opcional)")]
    [SerializeField] private float volumeBoostMultiplier = 1.5f; // Multiplicador del volumen para el clip de cierre
    [SerializeField] private bool useVolumeBoost = true; // Si aplicar el boost de volumen
    
    // ...existing code...
    private bool hasTriggered = false; // Control para evitar múltiples activaciones

    private void Start()
    {
        if(targetStatue != null)
        {
            targetStatue.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Verificar si es el jugador y si no se ha activado antes (si triggerOnce está activo)



        if (!other.CompareTag("Player")) return;
        if (triggerOnce && hasTriggered) return;



        // Marcar como activado si es de una sola vez
        if (triggerOnce)
        {
            hasTriggered = true;
        }

        // Ejecutar la secuencia
        ActivateSequence();
    }
    
    /// <summary>
    /// Método público para ser llamado por los ChildTriggerRelay
    /// </summary>
    public void OnChildTriggerEnter(Collider other)
    {
        // Reutilizar la misma lógica del trigger principal
        OnTriggerEnter(other);
    }
    
    /// <summary>
    /// Ejecuta la secuencia principal: cierra la puerta rápidamente y activa la estatua
    /// </summary>
    private void ActivateSequence()
    {
        StartCoroutine(ActivateSequenceCoroutine());
    }

    private IEnumerator ActivateSequenceCoroutine()
    {
        // Cerrar la puerta rápidamente con boost de volumen opcional
        if (targetDoor != null)
        {
            float originalVolume = 1f;
            AudioSource doorAudioSource = null;
            
            // Aplicar boost de volumen temporal si está habilitado
            if (useVolumeBoost)
            {
                doorAudioSource = targetDoor.GetComponent<AudioSource>();
                if (doorAudioSource != null)
                {
                    originalVolume = doorAudioSource.volume;
                    doorAudioSource.volume = originalVolume * volumeBoostMultiplier;
                }
            }
            
            targetDoor.StartFastClosing();
            
            // Esperar a que termine el cierre rápido
            yield return new WaitForSeconds(targetDoor.FastCloseDuration);
            
            // Restaurar el volumen original
            if (useVolumeBoost && doorAudioSource != null)
            {
                doorAudioSource.volume = originalVolume;
            }
        }
        else
        {
            yield break;
        }

        // Activar la estatua
        if (targetStatue != null)
        {
            targetStatue.SetActive(true);
        }
    }    /// <summary>
    /// Permite resetear el trigger desde código si es necesario
    /// </summary>
    public void ResetTrigger()
    {
        hasTriggered = false;
    // ...existing code...
    }
    
    /// <summary>
    /// Permite cambiar las referencias desde código
    /// </summary>
    public void SetReferences(Door door, GameObject statue)
    {
        targetDoor = door;
        targetStatue = statue;
    // ...existing code...
    }
    
    #region Gizmos
    
    private void OnDrawGizmos()
    {
        DrawTriggerGizmos(false);
    }
    
    private void OnDrawGizmosSelected()
    {
        DrawTriggerGizmos(true);
    }
    
    /// <summary>
    /// Dibuja los gizmos del trigger para visualización en el editor
    /// </summary>
    /// <param name="isSelected">Si el objeto está seleccionado</param>
    private void DrawTriggerGizmos(bool isSelected)
    {
        // Dibujar triggers del propio objeto (si los tiene)
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            DrawColliderGizmo(col, isSelected, transform);
        }
        
        // Dibujar triggers de los hijos
        ChildTriggerRelay[] childTriggers = GetComponentsInChildren<ChildTriggerRelay>();
        foreach (ChildTriggerRelay childTrigger in childTriggers)
        {
            Collider childCol = childTrigger.GetComponent<Collider>();
            if (childCol != null)
            {
                DrawColliderGizmo(childCol, isSelected, childTrigger.transform);
            }
        }
        
        // Dibujar conexiones con referencias (solo cuando está seleccionado)
        if (isSelected)
        {
            DrawConnectionGizmos();
        }
    }
    
    /// <summary>
    /// Dibuja un gizmo para un collider específico
    /// </summary>
    private void DrawColliderGizmo(Collider col, bool isSelected, Transform colliderTransform)
    {
        if (col == null) return;
        
        // Colores diferentes según el estado
        Color gizmoColor;
        if (!col.isTrigger)
        {
            gizmoColor = Color.red; // Rojo si no es trigger (error)
        }
        else if (hasTriggered && Application.isPlaying)
        {
            gizmoColor = Color.gray; // Gris si ya se activó
        }
        else if (isSelected)
        {
            gizmoColor = Color.yellow; // Amarillo cuando está seleccionado
        }
        else
        {
            gizmoColor = Color.green; // Verde normal
        }
        
        // Ajustar transparencia
        gizmoColor.a = isSelected ? 0.6f : 0.3f;
        Gizmos.color = gizmoColor;
        
        // Dibujar según el tipo de collider
        if (col is BoxCollider boxCol)
        {
            Gizmos.matrix = Matrix4x4.TRS(colliderTransform.position, colliderTransform.rotation, colliderTransform.lossyScale);
            Gizmos.DrawCube(boxCol.center, boxCol.size);
            
            // Wireframe siempre visible
            Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 1f);
            Gizmos.DrawWireCube(boxCol.center, boxCol.size);
        }
        else if (col is SphereCollider sphereCol)
        {
            Gizmos.matrix = Matrix4x4.TRS(colliderTransform.position + sphereCol.center, colliderTransform.rotation, colliderTransform.lossyScale);
            Gizmos.DrawSphere(Vector3.zero, sphereCol.radius);
            
            // Wireframe siempre visible
            Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 1f);
            Gizmos.DrawWireSphere(Vector3.zero, sphereCol.radius);
        }
        else if (col is CapsuleCollider capsuleCol)
        {
            // Para capsule, dibujaremos una esfera aproximada
            Gizmos.matrix = Matrix4x4.TRS(colliderTransform.position + capsuleCol.center, colliderTransform.rotation, colliderTransform.lossyScale);
            float radius = Mathf.Max(capsuleCol.radius, capsuleCol.height * 0.5f);
            Gizmos.DrawSphere(Vector3.zero, radius);
            
            Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 1f);
            Gizmos.DrawWireSphere(Vector3.zero, radius);
        }
        
        // Resetear matrix
        Gizmos.matrix = Matrix4x4.identity;
    }
    
    /// <summary>
    /// Dibuja líneas conectando el trigger con la puerta y estatua
    /// </summary>
    private void DrawConnectionGizmos()
    {
        Vector3 triggerPos = transform.position;
        
        // Conexión con la puerta
        if (targetDoor != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(triggerPos, targetDoor.transform.position);
            
            // Dibujar un pequeño icono en la puerta
            Gizmos.DrawWireCube(targetDoor.transform.position + Vector3.up * 2f, Vector3.one * 0.5f);
        }
        
        // Conexión con la estatua
        if (targetStatue != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(triggerPos, targetStatue.transform.position);
            
            // Dibujar un pequeño icono en la estatua
            Gizmos.DrawWireSphere(targetStatue.transform.position + Vector3.up * 2f, 0.3f);
        }
    }
    
    #endregion
    
    private void OnValidate()
    {
        // Validaciones en el editor
        if (targetDoor != null && targetDoor.GetComponent<Door>() == null)
        {
            Debug.LogError("[DoorStatueSequence] El objeto asignado como puerta no tiene componente Door");
        }
        
        // Verificar que haya al menos un trigger (propio o en hijos)
        Collider ownCol = GetComponent<Collider>();
        ChildTriggerRelay[] childTriggers = GetComponentsInChildren<ChildTriggerRelay>();
        
        bool hasTrigger = false;
        
        if (ownCol != null)
        {
            if (!ownCol.isTrigger)
            {
                Debug.LogWarning("[DoorStatueSequence] El Collider propio debe estar marcado como 'Is Trigger'");
            }
            else
            {
                hasTrigger = true;
            }
        }
        
        if (childTriggers.Length > 0)
        {
            hasTrigger = true;
            foreach (ChildTriggerRelay childTrigger in childTriggers)
            {
                Collider childCol = childTrigger.GetComponent<Collider>();
                if (childCol != null && !childCol.isTrigger)
                {
                    Debug.LogWarning($"[DoorStatueSequence] El Collider en {childTrigger.name} debe estar marcado como 'Is Trigger'");
                }
            }
        }
        
        if (!hasTrigger)
        {
            Debug.LogWarning("[DoorStatueSequence] Se necesita al menos un Collider configurado como Trigger (propio o en objetos hijos con ChildTriggerRelay)");
        }
    }
}
