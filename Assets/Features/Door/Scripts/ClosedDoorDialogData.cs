using UnityEngine;

[CreateAssetMenu(fileName = "New Closed Door Dialog", menuName = "Door System/Closed Door Dialog Data")]
public class ClosedDoorDialogData : ScriptableObject
{
    [Header("Audio Configuration")]
    [SerializeField] private AudioClip lockedDoorClip;
    [SerializeField] private AudioClip dialogAudioClip; // UN SOLO clip de audio para todos los diálogos
    
    [Header("Dialog Messages")]
    [SerializeField] private DialogData[] dialogMessages;
    
    [Header("Interaction Settings")]
    [SerializeField] private bool allowReinteraction = true; // Permitir interactuar nuevamente después de completar
    [SerializeField] private bool blockInteractionDuringPlayback = true; // Bloquear interacción durante reproducción
    
    /// <summary>
    /// Obtiene todos los mensajes de diálogo para reproducir en secuencia
    /// </summary>
    public DialogData[] GetAllDialogMessages()
    {
        if (dialogMessages == null || dialogMessages.Length == 0)
        {
            return new DialogData[0]; //new DialogData[] { new DialogData("Esta puerta está cerrada.", 2f) };
        }
        
        return dialogMessages;
    }
    
    /// <summary>
    /// Obtiene el clip de audio de puerta cerrada
    /// </summary>
    public AudioClip GetLockedDoorClip()
    {
        return lockedDoorClip;
    }
    
    /// <summary>
    /// Obtiene el clip de audio del diálogo
    /// </summary>
    public AudioClip GetDialogAudioClip()
    {
        return dialogAudioClip;
    }
    
    /// <summary>
    /// Indica si se permite volver a interactuar después de completar la secuencia
    /// </summary>
    public bool ShouldAllowReinteraction()
    {
        return allowReinteraction;
    }
    
    /// <summary>
    /// Indica si se debe bloquear la interacción durante la reproducción
    /// </summary>
    public bool ShouldBlockInteractionDuringPlayback()
    {
        return blockInteractionDuringPlayback;
    }
}
