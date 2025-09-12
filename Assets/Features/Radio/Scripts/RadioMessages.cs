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
    public DialogData[] primerLoopMessages = new DialogData[]
    {
        new DialogData { dialogText = "", duration = 13f },
        new DialogData { dialogText = "Siendo la una y un minuto,", duration = 3f },
        new DialogData { dialogText = "antes de continuar con la sección musical,", duration = 3f },
        new DialogData { dialogText = "vamos con la noticia del día", duration = 2.5f },
        new DialogData { dialogText = "", duration = 3.0f },
        new DialogData { dialogText = "El panorama artístico argentino", duration = 2f },
        new DialogData { dialogText = "ha dado un vuelco con la reciente obra", duration = 5f },
        new DialogData { dialogText = "", duration = 3f },
        new DialogData { dialogText = "un artista presuntamente desconocido", duration = 3.0f },
        new DialogData { dialogText = "aunque según pudimos confirmar,", duration = 3f },
        new DialogData { dialogText = "proviene de la famosa familia de artistas", duration = 3f }, //43
        new DialogData { dialogText = "", duration = 2.5f },
        new DialogData { dialogText = "De igual forma,", duration = 1.5f }, //47
        new DialogData { dialogText = "sea cual sea el motivo por el cual", duration = 3f },
        new DialogData { dialogText = "habría permanecido en el anonimato,", duration = 3f },
        new DialogData { dialogText = "su futuro parece ser prometedor", duration = 4f },
        new DialogData { dialogText = "y ahora si,", duration = 2.0f },
        new DialogData { dialogText = "con la sección musical", duration = 3.5f },
        new DialogData { dialogText = "", duration = 2.5f }
    };

    [Header("Segundo Loop - Decadencia del Personaje")]
    [Tooltip("Mensajes del segundo loop para contar la decadencia del personaje")]
    public DialogData[] segundoLoopMessages = new DialogData[]
    {
        new DialogData { dialogText = "", duration = 9.0f },
        new DialogData { dialogText = "Siendo las dos y dos minutos,", duration = 4.0f },
        new DialogData { dialogText = "y continuando con las noticias del día,", duration = 4.0f },
        new DialogData { dialogText = "", duration = 4.0f },
        new DialogData { dialogText = "las críticas han destruido al famoso escultor...", duration = 4.0f },
        new DialogData { dialogText = "", duration = 4.0f },
        new DialogData { dialogText = "y la sociedad...", duration = 2.0f },
        new DialogData { dialogText = "estaría conmocionada...", duration = 3.0f },
        new DialogData { dialogText = "Ampliaremos...", duration = 3.0f }
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
    public DialogData[] GetMessagesByType(RadioLoopType type)
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
        DialogData[] messages = GetMessagesByType(type);
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