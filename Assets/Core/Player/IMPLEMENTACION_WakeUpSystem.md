# 🎯 Guía de Implementación - Sistema de Despertar

## 📋 **PASOS DE IMPLEMENTACIÓN**

### **Paso 1: Agregar scripts al Player**
1. Selecciona tu **GameObject Player** en la jerarquía
2. Click en **"Add Component"**
3. Busca y añade **"Player Wake Up Sequence"**
4. (Opcional) Añade también **"Player Wake Up Effects"** para efectos visuales adicionales

### **Paso 2: Configurar en el Inspector**

#### **PlayerWakeUpSequence:**
```
Referencias del Player:
🎯 Player Controller: [ARRASTRAR PlayerController del Player]
✅ Wake Up Effects: [ARRASTRAR PlayerWakeUpEffects si está disponible]

Puerta a Cerrar:
🎯 Door To Close: [ARRASTRAR AQUÍ LA PUERTA ESPECÍFICA QUE SE DEBE CERRAR]

Configuración del Despertar:
✅ Wake Up Dizzy Duration: 3
✅ Wake Up Dizzy Intensity: 0.6
✅ Delay Before Dialog: 1

Configuración de la Puerta:
✅ Door Close Delay: 0.5

Audio del Despertar:
🎯 Player Audio Source: [Arrastra AudioSource del Player]
🎯 Breathing Clip: [Arrastra clip de respiración agitada]
🎯 Wake Up Gasp Clip: [Arrastra clip de jadeo inicial]

Diálogos Iniciales:
✅ [Ya preconfigurados con el texto del guión]
```

#### **PlayerWakeUpEffects (EN GameObject Player - Opcional):**
```
🎯 Main Camera: [Arrastra tu MainCam (CinemachineCamera)]
```
```
Referencias:
🎯 Main Camera: [ARRASTRAR CinemachineCamera principal (MainCam)]

Efectos de Respiración:
✅ Enable Breathing Effect: ☑
✅ Breathing Duration: 5
✅ Breathing Intensity: 0.02
✅ Breathing Frequency: 1.2

Efectos de FOV:
✅ Enable FOV Effect: ☑
✅ FOV Change Duration: 2
✅ Target FOV: 70
```

### **Paso 3: Identificar y Asignar Referencias**

#### **🎯 REFERENCIAS OBLIGATORIAS:**
1. **PlayerController**: Arrastra desde el mismo GameObject Player
2. **Door To Close**: Identifica la puerta específica que debe cerrarse y arrástrala
3. **AudioSource**: Arrastra el AudioSource del Player
4. **Main Camera (para efectos)**: Solo si usas PlayerWakeUpEffects

#### **⚠️ NO HAY AUTO-DETECCIÓN - TODO DEBE ASIGNARSE MANUALMENTE**

### **Paso 4: Configurar Audio**
```
🎵 Clips de Audio Necesarios:
- Wake Up Gasp Clip: Sonido de despertar exaltado/jadeo inicial
- Breathing Clip: Respiración agitada durante el mareo

📁 Ubicación sugerida: Assets/Audio/Player/
```

### **Paso 5: Testing**

#### **En el Editor:**
1. El script tiene **validaciones automáticas**
2. Si algo falta, aparecerán **warnings en Console**
3. Usa el botón **"Reset Wake Up Sequence"** en el context menu para testing

#### **Durante el Juego:**
- El sistema se activa automáticamente después de la intro
- Revisa la **Console** para logs de debug con prefijo `[PlayerWakeUpSequence]`

## 🔧 **Configuraciones Recomendadas**

### **Para Efecto Sutil:**
```
Wake Up Dizzy Duration: 2-3s
Wake Up Dizzy Intensity: 0.4-0.6
Breathing Effect: Activado
FOV Effect: Activado
```

### **Para Efecto Intenso:**
```
Wake Up Dizzy Duration: 4-5s
Wake Up Dizzy Intensity: 0.7-0.9
Breathing Effect: Activado con intensidad alta
FOV Effect: Activado con target FOV más alto
```

## ⚠️ **Problemas Comunes y Soluciones**

### **"PlayerController no encontrado"**
- Asegúrate de que el GameObject Player tiene el componente PlayerController
- Verifica que el Player tiene el tag "Player"

### **"Puerta no se cierra"**
- Verifica que la puerta asignada tiene el componente Door
- Asegúrate de que la puerta no está ya en estado SlowClosing
- Revisa que doorToClose está asignado en el inspector

### **"Audio no se reproduce"**
- Verifica que hay un AudioSource en el Player
- Asigna los clips de audio en el inspector
- Revisa que el AudioSource no está muteado

### **"Diálogos no aparecen"**
- Verifica que DialogController está en la escena
- Asegúrate de que el Canvas de diálogos está configurado

## 🎮 **Resultado Esperado**

Al iniciar el juego después de la intro:
1. **0.0s**: Fade in completo, sonido de despertar exaltado
2. **0.3s**: Inicia mareo + respiración agitada + efectos visuales
3. **0.5s**: Puerta comienza a cerrarse lentamente
4. **2.0s**: Aparece primer diálogo "Euri??... Hija??..."
5. **6.0s**: Diálogo "Juraría que alguien cerró la puerta..."
6. **9.0s**: Secuencia completa

¿Necesitas ayuda con algún paso específico?
