# Sistema de Puerta-Estatua con Trigger (LOOP 3)

## Descripción
Esta funcionalidad permite crear una secuencia donde:
1. El jugador abre una puerta (tipo `OpenAndClose`)
2. Cruza la puerta y activa un trigger
3. La puerta se cierra rápidamente con un sonido específico
4. Una estatua aparece

## Componentes

### 1. `TypeDoorInteract.cs` (Actualizado)
- Se agregó el tipo `FastClosing` para puertas que se cierran rápidamente

### 2. `Door.cs` (Actualizado)
**Nuevas características:**
- `fastCloseClip`: AudioClip para el sonido de cierre rápido
- `fastCloseDuration`: Duración del cierre rápido (por defecto 0.5s)
- `StartFastClosing()`: Método público para iniciar el cierre rápido
- `SetFastCloseClip()`: Método para asignar el clip de audio desde código

### 3. `StatueController.cs` (Nuevo)
**Funcionalidades:**
- Controla la interacción entre puerta y estatua
- Se activa cuando el jugador (tag "Player") entra al trigger
- Cierra la puerta rápidamente y activa la estatua
- Opción `triggerOnce` para activarse solo una vez

### 4. `DoorStatueSequenceSetup.cs` (Nuevo - Opcional)
**Utilidades:**
- Script de ayuda para configurar la secuencia fácilmente
- Configuración automática desde el inspector
- Validaciones y debug logs

## Configuración paso a paso

### Opción A: Configuración manual

1. **Preparar la puerta:**
   ```csharp
   // La puerta debe tener tipo OpenAndClose
   door.SetType(TypeDoorInteract.OpenAndClose);
   ```

2. **Crear el trigger:**
   - Crear un GameObject vacío donde quieras el trigger
   - Agregar un Collider (marcar como "Is Trigger")
   - Agregar el componente `StatueController`
   - Asignar la puerta y estatua en el inspector

3. **Configurar la estatua:**
   - La estatua debe estar inicialmente desactivada
   - El `StatueController` la activará automáticamente

### Opción B: Configuración automática

1. **Usar DoorStatueSequenceSetup:**
   - Crear un GameObject vacío para el setup
   - Agregar el componente `DoorStatueSequenceSetup`
   - Asignar las referencias (door, statue, triggerObject)
   - Asignar el `fastCloseClip` si es necesario
   - Ejecutar "Setup Sequence" desde el menú contextual o dejar que se ejecute automáticamente

## Ejemplo de uso en código

```csharp
// Obtener referencias
Door puerta = GameObject.Find("MiPuerta").GetComponent<Door>();
GameObject estatua = GameObject.Find("MiEstatua");
GameObject trigger = GameObject.Find("MiTrigger");

// Configurar la secuencia
StatueController controller = trigger.GetComponent<StatueController>();
if (controller == null)
    controller = trigger.AddComponent<StatueController>();

controller.SetReferences(puerta, estatua);

// Opcional: Configurar audio personalizado
puerta.SetFastCloseClip(miAudioClip);
```

## Configuración recomendada

### Inspector de Door:
- **Type:** OpenAndClose
- **Fast Close Clip:** Tu AudioClip de cierre rápido
- **Fast Close Duration:** 0.5f (o el valor deseado)
- **Open Degrees Y:** 110f (o el ángulo deseado)

### Inspector de StatueController:
- **Target Door:** Referencia a tu puerta
- **Target Statue:** Referencia a tu estatua
- **Trigger Once:** true (recomendado)

### Configuración del Trigger:
- **Collider:** Configurado como "Is Trigger"
- **Tag:** Cualquiera (el script busca "Player")
- **Posición:** Algunos metros después de la puerta

## Notas importantes

- El jugador debe tener el tag "Player"
- La estatua debe estar inicialmente desactivada
- El trigger debe tener un Collider marcado como "Is Trigger"
- La puerta debe ser tipo `OpenAndClose` para que funcione la interacción normal
- El cierre rápido usa una interpolación lineal (más brusca) vs el cierre lento que usa SmoothStep

## Debug

Todos los componentes incluyen logs de debug para facilitar el seguimiento:
- `[Door]`: Logs de apertura/cierre de puertas
- `[StatueController]`: Logs de activación de triggers y secuencias
- `[DoorStatueSequenceSetup]`: Logs de configuración automática
