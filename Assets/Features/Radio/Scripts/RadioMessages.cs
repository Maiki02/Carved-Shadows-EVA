using UnityEngine;

/// <summary>
/// Enum para seleccionar el tipo de loop de radio
/// </summary>
public enum RadioLoopType
{
    PrimerLoop,     // Presentación del personaje
    SegundoLoop     // Decadencia del personaje
}

/// <summary>
/// ScriptableObject que contiene los arrays de diálogos predefinidos para las radios.
/// Esto permite tener los mensajes preconfigurados y reutilizarlos en múltiples RadioControllers.
/// </summary>
[CreateAssetMenu(fileName = "RadioMessages", menuName = "Radio/Radio Messages")]
public class RadioMessages : ScriptableObject
{
    [Header("Primer Loop - Presentación del Personaje")]
    [Tooltip("Mensajes del primer loop para la presentación del personaje")]
    public DialogMessage[] primerLoopMessages = new DialogMessage[]
    {
        new DialogMessage { text = "", duration = 13f },
        new DialogMessage { text = "Siendo la una y un minuto,", duration = 3f },
        new DialogMessage { text = "antes de continuar con la sección musical,", duration = 3f },
        new DialogMessage { text = "vamos con la noticia del día", duration = 2.5f },
        new DialogMessage { text = "", duration = 3.0f },
        new DialogMessage { text = "El panorama artístico argentino", duration = 2f },
        new DialogMessage { text = "ha dado un vuelco con la reciente obra", duration = 5f },
        new DialogMessage { text = "", duration = 3f },
        new DialogMessage { text = "un artista presuntamente desconocido", duration = 3.0f },
        new DialogMessage { text = "aunque según pudimos confirmar,", duration = 3f },
        new DialogMessage { text = "proviene de la famosa familia de artistas", duration = 3f }, //43
        new DialogMessage { text = "", duration = 2.5f },
        new DialogMessage { text = "De igual forma,", duration = 1.5f }, //47
        new DialogMessage { text = "sea cual sea el motivo por el cual", duration = 3f },
        new DialogMessage { text = "habría permanecido en el anonimato,", duration = 3f },
        new DialogMessage { text = "su futuro parece ser prometedor", duration = 4f },
        new DialogMessage { text = "y ahora si,", duration = 2.0f },
        new DialogMessage { text = "con la sección musical", duration = 3.5f },
        new DialogMessage { text = "", duration = 2.5f }
    };

    [Header("Segundo Loop - Decadencia del Personaje")]
    [Tooltip("Mensajes del segundo loop para contar la decadencia del personaje")]
    public DialogMessage[] segundoLoopMessages = new DialogMessage[]
    {
        new DialogMessage { text = "", duration = 9.0f },
        new DialogMessage { text = "Siendo las dos y dos minutos,", duration = 4.0f },
        new DialogMessage { text = "y continuando con las noticias del día,", duration = 4.0f },
        new DialogMessage { text = "", duration = 4.0f },
        new DialogMessage { text = "las críticas han destruido al famoso escultor...", duration = 4.0f },
        new DialogMessage { text = "", duration = 4.0f },
        new DialogMessage { text = "y la sociedad...", duration = 2.0f },
        new DialogMessage { text = "estaría conmocionada...", duration = 3.0f },
        new DialogMessage { text = "Ampliaremos...", duration = 3.0f }
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
    public DialogMessage[] GetMessagesByType(RadioLoopType type)
    {
        switch (type)
        {
            case RadioLoopType.PrimerLoop:
                return GetPrimerLoopMessages();
            case RadioLoopType.SegundoLoop:
                return GetSegundoLoopMessages();
            default:
                return GetPrimerLoopMessages();
        }
    }

    /// <summary>
    /// Obtiene la duración total de los mensajes de un loop específico
    /// </summary>
    public float GetTotalDuration(RadioLoopType type)
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
    public int GetMessageCount(RadioLoopType type)
    {
        return GetMessagesByType(type).Length;
    }
}