# Sistema de Despertar del Protagonista

## Descripción General
Este sistema implementa la secuencia de despertar del protagonista al inicio del juego, incluyendo:
- Despertar con mareo/jadeo
- Cierre lento de puerta
- Diálogos iniciales del protagonista

## Componentes Principales

### 1. PlayerWakeUpSequence.cs
Script principal que coordina toda la secuencia de despertar.

**Configuración en Inspector:**
- **Referencias:**
  - `playerController`: Referencia al PlayerController del protagonista
  - `doorToClose`: La puerta que se cerrará lentamente
  - `playerGameObject`: GameObject del player (alternativo para buscar PlayerController)
  - `wakeUpEffects`: Componente de efectos adicionales (opcional)

- **Configuración del Despertar:**
  - `wakeUpDizzyDuration`: Duración del mareo al despertar (recomendado: 3s)
  - `wakeUpDizzyIntensity`: Intensidad del mareo (0-1, recomendado: 0.6)
  - `delayBeforeDialog`: Pausa antes de los diálogos (recomendado: 1s)

- **Configuración de la Puerta:**
  - `doorCloseDelay`: Tiempo antes de cerrar la puerta (recomendado: 0.5s)

- **Audio del Despertar:**
  - `playerAudioSource`: AudioSource para sonidos del jugador
  - `breathingClip`: Sonido de respiración agitada
  - `wakeUpGaspClip`: Sonido de despertar exaltado

- **Diálogos Iniciales:**
  - Array configurable con los diálogos iniciales del protagonista

### 2. PlayerWakeUpEffects.cs (Opcional)
Añade efectos visuales adicionales al despertar.

**Características:**
- **Efectos de Respiración:** Movimiento sutil de cámara para simular respiración agitada
- **Efectos de FOV:** Cambio temporal del campo de visión para desorientación
- **Efectos de Visión:** Preparado para efectos de blur (requiere Post-Processing)

## Integración

### Integración con MenuController
El sistema se integra automáticamente con `MenuController.cs`. Después de la intro y el fade in, busca automáticamente un componente `PlayerWakeUpSequence` y ejecuta la secuencia.

### Integración con Door System
Utiliza el sistema de puertas existente (`Door.cs`) con tipo `SlowClosing` para cerrar la puerta lentamente.

### Integración con Dialog System
Utiliza `DialogController.Instance` para mostrar los diálogos del protagonista.

## Configuración de Escena

### Setup Básico:
1. **Añadir PlayerWakeUpSequence:**
   - Crear un GameObject vacío llamado "PlayerWakeUpSequence"
   - Añadir el componente `PlayerWakeUpSequence`
   - Configurar las referencias en el Inspector

2. **Configurar la Puerta:**
   - Asegurar que la puerta tiene el componente `Door`
   - Asignar la puerta en el campo `doorToClose` del PlayerWakeUpSequence
   - La puerta se configurará automáticamente como `SlowClosing`

3. **Configurar Audio:**
   - Asignar clips de audio para jadeo y respiración
   - Asegurar que hay un AudioSource disponible

### Setup Avanzado (con efectos adicionales):
1. **Añadir PlayerWakeUpEffects:**
   - En el mismo GameObject o en el Player
   - Añadir el componente `PlayerWakeUpEffects`
   - Configurar efectos deseados en el Inspector

2. **Configurar Cámara:**
   - Asegurar que la referencia a la cámara está configurada
   - Ajustar intensidad y duración de efectos

## Flujo de Ejecución

1. **MenuController termina la intro**
2. **Fade in del juego**
3. **PlayerWakeUpSequence.StartWakeUpSequence():**
   - Reproduce sonido de despertar exaltado
   - Activa mareo en PlayerController (TriggerDizziness)
   - Reproduce sonido de respiración agitada
   - Activa efectos adicionales (si están disponibles)
   - Inicia cierre lento de puerta (en paralelo)
   - Espera delay configurado
   - Muestra diálogos iniciales usando DialogController

## Diálogos Predeterminados
```
"Euri??... Hija??... Hay alguien en casa??" (4s)
"" (pausa de 1s)
"Juraría que alguien cerró la puerta..." (3s)
```

## Métodos Públicos Útiles

### PlayerWakeUpSequence:
- `StartWakeUpSequence()`: Inicia la secuencia completa
- `SetInitialDialogs(DialogData[])`: Configura diálogos desde código
- `SetDoorToClose(Door)`: Asigna puerta desde código
- `ResetSequence()`: Resetea para testing (también disponible en context menu)

### PlayerWakeUpEffects:
- `StartWakeUpEffects()`: Inicia efectos adicionales
- `StopAllEffects()`: Detiene todos los efectos inmediatamente

## Debugging
Ambos scripts incluyen logs detallados con prefijo [PlayerWakeUpSequence] y [PlayerWakeUpEffects] para facilitar el debugging.

## Notas de Implementación
- El sistema es completamente opcional - si no hay PlayerWakeUpSequence en la escena, el juego continúa normalmente
- Los efectos adicionales también son opcionales
- Compatible con el sistema de puertas, diálogos y player existente
- Utiliza corrutinas para un control preciso del timing
- Thread-safe y evita múltiples ejecuciones accidentales
