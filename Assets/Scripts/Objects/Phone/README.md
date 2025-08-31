# Sistema de Mensajes del Teléfono

Este sistema permite configurar mensajes predefinidos para las llamadas telefónicas, similar al sistema de la radio.

## Archivos Principales

- **PhoneMessages.cs**: ScriptableObject que contiene los mensajes predefinidos
- **PhoneMessagesUtility.cs**: Utilidades de editor para crear y gestionar assets de PhoneMessages
- **PhoneController.cs**: Controller principal que maneja la configuración de mensajes
- **PhoneClose.cs**: Maneja la interacción con el teléfono cerrado
- **PhoneOpen.cs**: Maneja la llamada telefónica y los diálogos

## Configuración

### 1. Crear Asset de PhoneMessages
- Ir a menú `Phone > Create Default Phone Messages Asset`
- Configurar los mensajes para cada loop (PrimerLoop y SegundoLoop)

### 2. Configurar PhoneController
- Asignar el asset de `PhoneMessages`
- Seleccionar el `PhoneLoopType` (PrimerLoop o SegundoLoop)
- Asignar el `AudioClip` de la llamada telefónica
- Como alternativa, usar `phoneDialogSequence` para mensajes personalizados

### 3. Referencias
- `PhoneController` debe tener referencia a `PhoneClose`
- `PhoneClose` debe tener referencia a `PhoneController` y `PhoneOpen`

## Configuración Loop2 (Segundo Loop)

Los mensajes del Loop2 están preconfigurados con la secuencia solicitada:

```
3 segundos: ""
3 segundos: "Buenos dias señora @&:$-&/" 
3 segundos: "llamo con el motivo de confirmarle la visita a su casa" 
3 segundos: "por parte del museo de las bellas artes para..."
2.5 segundos: "¿cómo es que dijo usted?..."
4.5 segundos: "¡Ah si! deshacerse de las obras de su espo..."
2 segundos: ""
2 segundos: "No olvides..."
2 segundos: ""
2 segundos: "Lo que hiciste..."
4 segundos: "Ellas nunca te perdonarán..."
1.5 segundos: "Nunca..."
3.5 segundos: "te perdonaremos..."
6 segundos: ""
```

## Flujo de Funcionamiento

1. `PhoneController` detecta al jugador (OnTriggerEnter)
2. Inicia secuencia: cierra puerta → teléfono suena
3. Jugador interactúa con `PhoneClose`
4. `PhoneClose` obtiene parámetros de `PhoneController` (audio clip + diálogos)
5. Se activa `PhoneOpen` con los parámetros configurados
6. Se reproduce la llamada con los diálogos del loop seleccionado
7. Al terminar, se reactiva el sistema normal

## Utilidades de Debugging

- `Phone > Log Current Phone Messages Info`: Muestra información del asset seleccionado
- Los logs del sistema indican qué tipo de diálogos se están usando
- Validación automática de referencias en todos los componentes
