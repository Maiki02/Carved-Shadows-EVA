public enum CandleType
{
    Normal,          // Vela normal, siempre encendida
    TurnOffPermanent, // Se apaga y queda apagada
    TurnOn,          // Se enciende (estaba apagada)
    Flickering,      // Parpadea (sube/baja intensidad)
    Intermittent     // Se apaga/prende según lista de tiempos
}
