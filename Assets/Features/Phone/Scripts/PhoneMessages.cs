using UnityEngine;

/// <summary>
/// Enum para seleccionar el tipo de loop del teléfono
/// </summary>
public enum PhoneLoopType
{
    PrimerLoop,     // Primer set de mensajes telefónicos
    SegundoLoop     // Segundo set de mensajes telefónicos
}

/// <summary>
/// ScriptableObject que contiene los arrays de diálogos predefinidos para el teléfono.
/// Esto permite tener los mensajes preconfigurados y reutilizarlos en múltiples PhoneControllers.
/// </summary>
[CreateAssetMenu(fileName = "PhoneMessages", menuName = "Phone/Phone Messages")]
public class PhoneMessages : ScriptableObject
{
    [Header("Primer Loop - Primer set de mensajes")]
    [Tooltip("Mensajes del primer loop del teléfono")]
    public DialogMessage[] primerLoopMessages = new DialogMessage[]
    {
        new DialogMessage { text = "", duration = 3f }
    };

    [Header("Segundo Loop - Segundo set de mensajes")]
    [Tooltip("Mensajes del segundo loop del teléfono")]
    public DialogMessage[] segundoLoopMessages = new DialogMessage[]
    {
        new DialogMessage { text = "", duration = 3f },
        new DialogMessage { text = "Buenos dias señora @&:$-&/", duration = 3f },
        new DialogMessage { text = "llamo con el motivo de confirmarle la visita a su casa", duration = 3f },
        new DialogMessage { text = "por parte del museo de las bellas artes para...", duration = 3f },
        new DialogMessage { text = "¿cómo es que dijo usted?...", duration = 2.5f },
        new DialogMessage { text = "¡Ah si! deshacerse de las obras de su espo...", duration = 4.5f },
        new DialogMessage { text = "", duration = 2f },
        new DialogMessage { text = "No olvides...", duration = 2f },
        new DialogMessage { text = "", duration = 2f },
        new DialogMessage { text = "Lo que hiciste...", duration = 2f },
        new DialogMessage { text = "Ellas nunca te perdonarán...", duration = 4f },
        new DialogMessage { text = "Nunca...", duration = 1.5f },
        new DialogMessage { text = "te perdonaremos...", duration = 3.5f },
        new DialogMessage { text = "", duration = 6f }
    };

    /// <summary>
    /// Obtiene una copia de los mensajes del primer loop
    /// </summary>
    public DialogMessage[] GetPrimerLoopMessages()
    {
        DialogMessage[] copy = new DialogMessage[primerLoopMessages.Length];
        System.Array.Copy(primerLoopMessages, copy, primerLoopMessages.Length);
        return copy;
    }

    /// <summary>
    /// Obtiene una copia de los mensajes del segundo loop
    /// </summary>
    public DialogMessage[] GetSegundoLoopMessages()
    {
        DialogMessage[] copy = new DialogMessage[segundoLoopMessages.Length];
        System.Array.Copy(segundoLoopMessages, copy, segundoLoopMessages.Length);
        return copy;
    }

    /// <summary>
    /// Obtiene una copia de los mensajes según el tipo especificado
    /// </summary>
    public DialogMessage[] GetMessagesByType(PhoneLoopType type)
    {
        switch (type)
        {
            case PhoneLoopType.PrimerLoop:
                return GetPrimerLoopMessages();
            case PhoneLoopType.SegundoLoop:
                return GetSegundoLoopMessages();
            default:
                return GetPrimerLoopMessages();
        }
    }

    /// <summary>
    /// Obtiene la duración total de los mensajes de un loop específico
    /// </summary>
    public float GetTotalDuration(PhoneLoopType type)
    {
        DialogMessage[] messages = GetMessagesByType(type);
        float totalDuration = 0f;
        foreach (var msg in messages)
            totalDuration += msg.duration;
        return totalDuration;
    }

    /// <summary>
    /// Obtiene el número de mensajes de un loop específico
    /// </summary>
    public int GetMessageCount(PhoneLoopType type)
    {
        return GetMessagesByType(type).Length;
    }
}
