# Sistema de Teléfono - Carved Shadows EVA

Este sistema maneja las interacciones del teléfono en el juego, incluyendo llamadas entrantes, salientes y diálogos.

## Archivos Principales

- **PhoneMessages.cs**: ScriptableObject que contiene los mensajes predefinidos
- **PhoneMessagesUtility.cs**: Utilidades de editor para crear y gestionar assets de PhoneMessages
- **PhoneController.cs**: [LEGACY] Controller genérico - mantenido como fallback
- **Call_Loop_01.cs**: Controller específico para el primer loop de llamadas
- **PhoneClose.cs**: Maneja la interacción con el teléfono cerrado
- **PhoneOpen.cs**: Maneja la llamada telefónica y los diálogos

## Funcionalidad Principal

### PhoneClose - Teléfono Cerrado
- **Interacción SIEMPRE disponible** independientemente del estado
- **Llamada entrante** (teléfono sonando): Reproduce clip y diálogos del Call_Loop_01
- **Llamada saliente** (sin llamada previa): Reproduce tono de "sin contestar"

### Audio Clips Requeridos
- `ringClip`: Sonido del teléfono sonando
- `pickupClip`: Sonido de levantar el auricular  
- `noAnswerToneClip`: Tono cuando no hay llamada entrante

## Configuración

### 1. Crear Asset de PhoneMessages
- Ir a menú `Phone > Create Default Phone Messages Asset`
- Configurar los mensajes para cada loop (PrimerLoop y SegundoLoop)

### 2. Configurar Call_Loop_01 (Recomendado)
- Asignar referencia a Door
- Asignar referencia a PhoneClose
- Asignar `phoneCallClip` para el audio de la llamada
- Asignar asset de `PhoneMessages` o usar diálogos hardcodeados
- Configurar `ringDuration` (duración del timbrado)

### 3. Configurar PhoneController (Legacy - Solo como Fallback)
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
