# Sistema de Audio con AudioMixer - Guía de Migración

## 📋 Resumen de Cambios

Tu `AudioController` ha sido actualizado para trabajar con **AudioMixer Groups** en lugar de AudioSources directos. Esto te permite tener un control más granular del audio y aprovechar el audio 3D.

## 🎯 Nuevas Características

### AudioController Actualizado
- ✅ **Control por AudioMixer**: Usa AudioMixer Groups para controlar volumen
- ✅ **Volumen Master**: Nuevo control de volumen general
- ✅ **Conversión dB**: Convierte automáticamente valores 0-1 a decibeles
- ✅ **Compatibilidad hacia atrás**: Los métodos antiguos siguen funcionando

### Nuevos Scripts de Ayuda
- ✅ **AudioMixerHelper**: Configura automáticamente AudioSources con grupos apropiados
- ✅ **AudioMixerSetup**: Script de configuración para migración automática

## 🚀 Cómo Configurar el Sistema

### Paso 1: Configurar el AudioMixer
1. Ve a tu `MainMixer.mixer` en el proyecto
2. Asegúrate de que tienes estos grupos (nombres exactos):
   ```
   Master
   ├── Music
   ├── SFX
   ├── Radio
   ├── FootSteps
   ├── Door
   ├── Ambient sounds
   ├── Organic
   └── Respiracion
   ```

### Paso 2: Configurar AudioController
1. Selecciona tu GameObject con `AudioController`
2. En el inspector, asigna:
   - **Audio Mixer**: Tu `MainMixer`
   - **Group Names**: Los nombres exactos de tus grupos

### Paso 3: Configurar AudioMixerHelper
1. Crea un GameObject vacío llamado "AudioMixerHelper"
2. Agrégale el script `AudioMixerHelper`
3. En el inspector, asigna las referencias de **AudioMixerGroup** arrastrándolas desde tu mixer

### Paso 4: Usar AudioMixerSetup (Automático)
1. Crea un GameObject y agrégale `AudioMixerSetup`
2. Asigna tu `MainMixer`
3. Verifica los nombres de grupos
4. Asigna las referencias de AudioMixerGroup
5. Al ejecutar, configurará todo automáticamente

## 🎮 Cómo Usar en ConfigController

El `ConfigController` ya está actualizado y soporta:

```csharp
// Nuevos sliders disponibles
[SerializeField] private Slider musicVolumeSlider;
[SerializeField] private Slider sfxVolumeSlider;
[SerializeField] private Slider masterVolumeSlider; // ¡NUEVO!
```

### Configuración en Unity Inspector
1. Asigna los 3 sliders en el inspector
2. El sistema se conectará automáticamente

## 📝 Nombres de Grupos de Mixer

**IMPORTANTE**: Los nombres deben coincidir exactamente:

```csharp
// En AudioController
private string masterGroupName = "Master";
private string musicGroupName = "Music";
private string sfxGroupName = "SFX";
```

Asegúrate de que estos nombres coincidan con los de tu AudioMixer.

## 🔧 Configuración Automática de AudioSources

El `AudioMixerHelper` configurará automáticamente AudioSources basado en:

### Por Tag del GameObject:
- `music` → Grupo Music
- `sfx` → Grupo SFX
- `radio` → Grupo Radio
- `footsteps` → Grupo FootSteps
- `door` → Grupo Door
- `ambient` → Grupo Ambient sounds

### Por Nombre del GameObject:
- Contiene "radio" → Grupo Radio
- Contiene "door"/"puerta" → Grupo Door
- Contiene "footstep"/"paso" → Grupo FootSteps
- Contiene "music"/"musica" → Grupo Music
- Contiene "ambient"/"viento" → Grupo Ambient sounds

## 💡 Uso Avanzado

### Control Manual de AudioSources
```csharp
// Configurar un AudioSource específico
AudioMixerHelper.Instance.SetRadioGroup(myAudioSource);
AudioMixerHelper.Instance.SetSFXGroup(myAudioSource);
```

### Control Directo de Volumen
```csharp
// El nuevo sistema
AudioController.Instance.MasterVolume = 0.8f;  // 80%
AudioController.Instance.MusicVolume = 0.6f;   // 60%
AudioController.Instance.SfxVolume = 0.7f;     // 70%
```

### Obtener Referencias de Grupos
```csharp
AudioMixerGroup radioGroup = AudioMixerHelper.Instance.GetRadioGroup();
myAudioSource.outputAudioMixerGroup = radioGroup;
```

## ⚠️ Notas Importantes

1. **Volúmenes en dB**: El sistema convierte automáticamente 0-1 a -80dB/0dB
2. **Compatibilidad**: Los métodos `PlaySFX()` y `PlayMusic()` antiguos siguen funcionando
3. **3D Audio**: Los AudioSources individuales mantienen su configuración 3D
4. **Mixer Control**: El volumen se controla a nivel de mixer, no de AudioSource individual

## 🐛 Solución de Problemas

### El volumen no cambia
- Verifica que los nombres de grupos coincidan exactamente
- Asegúrate de que el AudioMixer está asignado

### AudioSources no usan el grupo correcto
- Ejecuta `AudioMixerHelper.ConfigureAllAudioSources()`
- Verifica tags y nombres de GameObjects

### ConfigController no encuentra AudioController
- Asegúrate de que AudioController.Instance no es null
- Verifica que AudioController esté en la escena

## 📚 Métodos de Migración

### Opción 1: Automática
1. Usa `AudioMixerSetup` para configuración automática
2. Ejecuta en Play Mode para pruebas

### Opción 2: Manual
1. Configura AudioController manualmente
2. Configura AudioMixerHelper manualmente
3. Ejecuta `ConfigureAllAudioSources()` para AudioSources existentes

---

¡El nuevo sistema te dará mucho más control sobre el audio y soporte completo para audio 3D mientras mantiene la compatibilidad con tu código existente!
