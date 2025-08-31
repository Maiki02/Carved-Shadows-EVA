# Sistema de Radio con Trigger

Este sistema permite activar automáticamente una radio cuando el jugador entra en un área específica (trigger).

## Componentes

### RadioMessages.cs (ScriptableObject)
**Propósito**: Contiene arrays predefinidos de diálogos para evitar configuración manual repetitiva.

**Funcionalidades**:
- Arrays predefinidos de diálogos con texto y duración
- Primer Loop: Presentación del personaje (12 mensajes)
- Segundo Loop: Decadencia del personaje (pendiente de definir)
- Métodos para obtener copias de los arrays

**Creación del Asset**:
1. Click derecho en Project → Create → Radio → Radio Messages
2. Los mensajes ya están preconfigurados, listos para usar

### RadioController.cs
**Propósito**: Controla la activación de la radio mediante triggers.

**Funcionalidades**:
- Detecta cuando el jugador entra en un collider trigger
- Activa la reproducción de la radio con parámetros configurables
- Puede usar mensajes predefinidos (RadioMessages) o personalizados
- Puede configurarse para activarse solo una vez
- Selección de tipo de loop (Primer/Segundo Loop)

**Configuración en Inspector**:
- **Referencias**:
  - `Radio`: Referencia al componente Radio que se activará
  - `Player Controller`: Se busca automáticamente si no se asigna
- **Audio Configuration**:
  - `Radio Clip`: Clip de audio que se reproducirá
  - `Audio Duration`: Duración del audio en segundos
- **Dialog Configuration**:
  - `Radio Messages`: ScriptableObject con mensajes predefinidos (RECOMENDADO)
  - `Loop Type`: PrimerLoop o SegundoLoop
  - `Radio Dialog Sequence`: Array personalizado (solo si RadioMessages está vacío)
- **Trigger Settings**:
  - `Trigger Once`: Si true, solo se activa una vez

### Radio.cs (Actualizado con DearVR)
**Propósito**: Ejecuta la reproducción de audio espacializado y diálogos usando DearVR.

**Funcionalidades**:
- Integración completa con DearVRSource para audio espacial de alta calidad
- Reproducción con clip proporcionado por el controller
- Desactiva controles del jugador durante reproducción  
- Muestra secuencia de diálogos en paralelo
- Reactiva controles al finalizar
- Configuraciones automáticas optimizadas para radio
- Compatibilidad con AudioSource estándar como fallback

**Configuración en Inspector**:
- `DearVRSource`: Componente DearVR para audio espacial (se busca automáticamente)
- `Audio Source`: AudioSource requerido por DearVR (se busca automáticamente)
- `Use DearVR Playback`: Si usar DearVR o AudioSource estándar

**Configuración DearVR Automática**:
- Performance Mode: Activado para mejor rendimiento
- Internal Reverb: Activado
- Room Preset: Room_Medium (ambiente de habitación)
- Niveles de audio optimizados para radio

## Uso

### Configuración Rápida (RECOMENDADA):
1. Crear RadioMessages asset: Click derecho → Create → Radio → Radio Messages
2. Crear un GameObject con `RadioController`
3. Asegurar que tiene un Collider configurado como Trigger
4. **Crear un GameObject con `Radio`, `DearVRSource` y `AudioSource`**
5. En el RadioController, asignar:
   - Referencia al componente Radio
   - AudioClip a reproducir
   - Duración del audio
   - RadioMessages asset creado en paso 1
   - Seleccionar Loop Type (PrimerLoop/SegundoLoop)

### Configuración Manual:
1. Crear un GameObject con `RadioController`
2. Asegurar que tiene un Collider configurado como Trigger
3. **Crear un GameObject con `Radio`, `DearVRSource` y `AudioSource`**
4. En el RadioController, asignar:
   - Referencia al componente Radio
   - AudioClip a reproducir
   - Duración del audio
   - Dejar RadioMessages vacío
   - Configurar manualmente el array Radio Dialog Sequence

### Configuración de DearVR:
- **Automática**: El script Radio.cs configura automáticamente DearVR
- **Manual**: Puedes ajustar configuraciones en DearVRSource según necesites:
  - Room Preset: Para diferentes ambientes
  - Direct/Reflection/Reverb Levels: Para control de espacialización
  - Occlusion/Obstruction: Para interacciones físicas con el entorno

## Creación de Assets

### Método 1 - Menú contextual:
1. Click derecho en Project → Create → Radio → Radio Messages
2. Nombra el asset (ej: "PrimerLoopMessages")

### Método 2 - Menú de editor:
1. Menu bar → Radio → Create Default Radio Messages Asset
2. Selecciona ubicación y nombre

### Verificar información del asset:
1. Selecciona el RadioMessages asset
2. Menu bar → Radio → Log Current Radio Messages Info
3. Verifica en Console la duración total y cantidad de mensajes

## Configuración de Loops

### Primer Loop - Presentación del Personaje (CONFIGURADO):
```
"..." (0.5s)
"..." (0.5s)
"AM, República Argentina" (2.5s)
"Es la una y un minuto, antes de continuar con la sección musical, vamos con la noticia del día" (4.5s)
"El panorama artístico argentino ha dado un vuelco con la reciente obra de..." (4.0s)
"..." (0.5s)
"…un artista presuntamente desconocido" (3.0s)
"aunque según pudimos confirmar, proviene de la conocida casa de artistas" (4.0s)
"..." (0.5s)
"De igual forma, sea cual sea el motivo por el cual había permanecido en el anonimato," (4.5s)
"su futuro parece ser prometedor..." (3.0s)
"y ahora los dejamos con la sección musical" (3.5s)

Total: 12 mensajes, ~34.5 segundos
```

### Segundo Loop - Decadencia del Personaje (PENDIENTE):
- Placeholder configurado
- Listo para agregar mensajes cuando estén definidos

### Ejemplo de Configuración de Diálogos:
```csharp
// En el array radioDialogSequence:
[0] text: "Primera transmisión..." duration: 3.0
[1] text: "Continúa la transmisión..." duration: 4.0
[2] text: "Final de la transmisión." duration: 2.0
```

### Funciones Públicas:

**RadioController**:
- `StartRadioSequence()`: Inicia manualmente la secuencia
- `ResetTrigger()`: Resetea el trigger para testing

**Radio**:
- `PlayRadioWithParameters()`: Reproduce con parámetros específicos usando DearVR
- `ResetRadio()`: Resetea el estado para testing (incluye DearVR)
- `IsRadioPlayed` (propiedad): Verifica si ya fue reproducida
- `ConfigureForRadio()`: Aplica configuraciones optimizadas de DearVR

## Notas de Implementación

- **DearVR Integration**: Usa DearVRSource para audio espacializado de alta calidad
- **Fallback**: Si DearVR no está disponible, usa AudioSource estándar
- **Auto-configuration**: DearVR se configura automáticamente para radios
- El sistema requiere que el jugador tenga tag "Player"
- Los controles del jugador se desactivan durante la reproducción
- Los diálogos se muestran usando DialogController.Instance
- **Performance Mode**: DearVR se configura en modo performance para mejor rendimiento

## Debugging

- Usa `ResetTrigger()` y `ResetRadio()` para testing
- Los logs indican el estado del sistema
- Verifica que todas las referencias estén asignadas en el Inspector
