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
    public DialogData[] primerLoopMessages = new DialogData[]
    {
        new DialogData("", 3f)
    };

    [Header("Segundo Loop - Segundo set de mensajes")]
    [Tooltip("Mensajes del segundo loop del teléfono")]
    public DialogData[] segundoLoopMessages = new DialogData[]
    {
        new DialogData("", 3f),
        new DialogData("Buenos dias señora @&:$-&/", 3f),
        new DialogData("llamo con el motivo de confirmarle la visita a su casa", 3f),
        new DialogData("por parte del museo de las bellas artes para...", 3f),
        new DialogData("¿cómo es que dijo usted?...", 2.5f),
        new DialogData("¡Ah si! deshacerse de las obras de su espo...", 4.5f),
        new DialogData("", 2f),
        new DialogData("No olvides...", 2f),
        new DialogData("", 2f),
        new DialogData("Lo que hiciste...", 2f),
        new DialogData("Ellas nunca te perdonarán...", 4f),
        new DialogData("Nunca...", 1.5f),
        new DialogData("te perdonaremos...", 3.5f),
        new DialogData("", 6f)
    };

    /// <summary>
    /// Obtiene una copia de los mensajes del primer loop
    /// </summary>
    public DialogData[] GetPrimerLoopMessages()
    {
        DialogData[] copy = new DialogData[primerLoopMessages.Length];
        System.Array.Copy(primerLoopMessages, copy, primerLoopMessages.Length);
        return copy;
    }

    /// <summary>
    /// Obtiene una copia de los mensajes del segundo loop
    /// </summary>
    public DialogData[] GetSegundoLoopMessages()
    {
        DialogData[] copy = new DialogData[segundoLoopMessages.Length];
        System.Array.Copy(segundoLoopMessages, copy, segundoLoopMessages.Length);
        return copy;
    }

    /// <summary>
    /// Obtiene una copia de los mensajes según el tipo especificado
    /// </summary>
    public DialogData[] GetMessagesByType(PhoneLoopType type)
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
        DialogData[] messages = GetMessagesByType(type);
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
