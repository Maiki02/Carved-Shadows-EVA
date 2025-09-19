using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class BlackoutController : MonoBehaviour
{
    [Header("Luces principales (se apagan)")]
    [SerializeField] private Light[] mainLights;

    [Header("Flicker")]
    [SerializeField] private float flickerSeconds = 1.1f;
    [SerializeField] private float flickerMinInterval = 0.06f;
    [SerializeField] private float flickerMaxInterval = 0.14f;

    [Header("Camera Fill (suave, NO linterna)")]
    [Tooltip("Activa un point light MUY tenue que sigue a la cámara. Sin sombras ni especular.")]
    [SerializeField] private bool useCameraFill = true;
    [SerializeField] private Color fillColor = new Color(0.70f, 0.82f, 1f);
    [Tooltip("Intensidad en lúmenes (HDRP)")]
    [SerializeField] private float fillIntensityLumens = 180f;
    [SerializeField] private float fillRange = 6.5f;
    [SerializeField] private float fillFadeSeconds = 0.25f;

    [Header("Moonlight direccional (opcional)")]
    [SerializeField] private bool useDirectionalMoon = false;
    [SerializeField] private Light moonLight;
    [SerializeField] private Vector3 moonDirectionEuler = new Vector3(15f, -35f, 0f);
    [SerializeField] private Color moonLightColor = new Color(0.75f, 0.85f, 1f);
    [SerializeField] private float moonLightIntensityLux = 0.8f;
    [SerializeField] private bool moonCastShadows = false;
    [SerializeField] private float moonFadeSeconds = 0.35f;

    [Header("HDRP Exposure")]
    [SerializeField] private Volume globalVolume; // Global Volume si ya existe
    [SerializeField] private bool tweakAutoExposure = true;
    [SerializeField] private bool createExposureIfMissing = true;
    [SerializeField] private float exposureEaseSeconds = 0.25f;
    [SerializeField] private float autoExposureMinEV = -8.0f;
    [SerializeField] private float autoExposureMaxEV = 0.0f;
    [SerializeField] private float exposureCompensation = 1.3f;

    [Header("SFX (opcional)")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip sfxFlicker;
    [SerializeField] private AudioClip sfxPowerDown;

    [Header("Auto-descubrimiento")]
    [SerializeField] private bool autoCollectAllSceneLights = true;
    [Tooltip("No apagar luces con estos tags (ej: la linterna del player).")]
    [SerializeField] private string[] excludeTags = new string[] { "PlayerLight", "DontBlackout" };
    [Tooltip("Exclusiones explícitas (se ignoran del apagón).")]
    [SerializeField] private Light[] explicitExclusions;

    // backups luces
    private float[] mainOrigIntensity;
    private bool[] mainOrigEnabled;
    private bool hasBackup;

    // exposure
    private Exposure exp;
    private bool hasExposure;
    private bool usingRuntimeVolume;
    private Volume runtimeVolume;
    private VolumeProfile runtimeProfile;
    private ExposureMode expModeBackup;
    private float expCompBackup, limitMinBackup, limitMaxBackup, fixedEVBackup;

    // moon
    private bool createdMoonLightAtRuntime = false;
    private HDAdditionalLightData moonHd;

    // camera fill
    private Light fillLight;
    private HDAdditionalLightData fillHd;
    private bool createdFillAtRuntime = false;

    void Awake()
    {
        if (autoCollectAllSceneLights)
            CollectSceneLights();

        BackupLights();

        if (!sfxSource) sfxSource = GetComponent<AudioSource>();
        SetupExposureRefIfAvailable();

        // Preconfig moon si está asignada pero desactivada por defecto
        if (moonLight)
        {
            EnsureHDData(moonLight, out moonHd);
            moonLight.color = moonLightColor;
            moonLight.shadows = moonCastShadows ? LightShadows.Soft : LightShadows.None;
            SetDirectionalLux(moonLight, moonHd, 0f);
        }
    }

    private void BackupLights()
    {
        if (mainLights == null) return;
        mainOrigIntensity = new float[mainLights.Length];
        mainOrigEnabled   = new bool[mainLights.Length];
        for (int i = 0; i < mainLights.Length; i++)
        {
            var L = mainLights[i];
            if (!L) continue;
            mainOrigIntensity[i] = L.intensity;
            mainOrigEnabled[i]   = L.enabled;
        }
        hasBackup = mainLights.Length > 0;
    }

    private void SetupExposureRefIfAvailable()
    {
        if (globalVolume && globalVolume.profile && globalVolume.profile.TryGet(out exp))
        {
            hasExposure     = true;
            usingRuntimeVolume = false;
            expModeBackup   = exp.mode.value;
            expCompBackup   = exp.compensation.value;
            limitMinBackup  = exp.limitMin.value;
            limitMaxBackup  = exp.limitMax.value;
            fixedEVBackup   = exp.fixedExposure.value;
        }
    }

    public Coroutine PlayBlackout(MonoBehaviour host) => host.StartCoroutine(BlackoutRoutine());

    public IEnumerator BlackoutRoutine()
    {
        EnsureExposureExistsIfNeeded();

        // 1) Flicker SFX + parpadeo
        if (sfxSource && sfxFlicker) sfxSource.PlayOneShot(sfxFlicker);
        yield return StartCoroutine(FlickerCoroutine());

        // 2) Apagar principales + SFX power down
        if (sfxSource && sfxPowerDown) sfxSource.PlayOneShot(sfxPowerDown);
        SetMainLightsEnabled(false);

        // 3) Encender fill (suave) y/o moon si se pide
        if (useCameraFill)
            yield return StartCoroutine(EnableCameraFill());

        if (useDirectionalMoon)
            yield return StartCoroutine(EnableDirectionalMoon());

        // 4) Subir un toque la exposición para que “se vea”
        if (tweakAutoExposure && hasExposure)
            yield return StartCoroutine(EaseExposureAuto());
    }

    private IEnumerator FlickerCoroutine()
    {
        float t = 0f;
        while (t < flickerSeconds)
        {
            bool off = Random.value > 0.5f;
            ApplyFlickerFrame(off);
            float wait = Random.Range(flickerMinInterval, flickerMaxInterval);
            yield return new WaitForSeconds(wait);
            t += wait;
        }
    }

    private void ApplyFlickerFrame(bool off)
    {
        if (mainLights == null) return;
        for (int i = 0; i < mainLights.Length; i++)
        {
            var L = mainLights[i];
            if (!L) continue;
            if (off)
            {
                L.enabled = true;
                L.intensity = 0f;
            }
            else
            {
                L.enabled = true;
                L.intensity = (hasBackup ? mainOrigIntensity[i] : 500f);
            }
        }
    }

    private void SetMainLightsEnabled(bool on)
    {
        if (mainLights == null) return;
        for (int i = 0; i < mainLights.Length; i++)
        {
            var L = mainLights[i];
            if (!L) continue;

            if (on)
            {
                L.enabled   = (hasBackup ? mainOrigEnabled[i]   : true);
                L.intensity = (hasBackup ? mainOrigIntensity[i] : L.intensity);
            }
            else
            {
                L.enabled = true;
                L.intensity = 0f;
            }
        }
    }

    // === Camera Fill ===
    private IEnumerator EnableCameraFill()
    {
        if (!fillLight)
            CreateFill();

        float start = GetPointLumens(fillLight, fillHd);
        float t = 0f;
        while (t < fillFadeSeconds)
        {
            float a = (fillFadeSeconds <= 0f) ? 1f : (t / fillFadeSeconds);
            float lm = Mathf.Lerp(start, fillIntensityLumens, a);
            SetPointLumens(fillLight, fillHd, lm);
            t += Time.deltaTime;
            yield return null;
        }
        SetPointLumens(fillLight, fillHd, fillIntensityLumens);
    }

    private void CreateFill()
    {
        var cam = Camera.main;
        Transform anchor = cam ? cam.transform : transform;

        var go = new GameObject("Blackout Camera Fill");
        go.transform.SetParent(anchor, false);
        go.transform.localPosition = Vector3.zero;

        fillLight = go.AddComponent<Light>();
        fillLight.type = LightType.Point;
        fillLight.color = fillColor;
        fillLight.range = fillRange;
        fillLight.shadows = LightShadows.None;

        EnsureHDData(fillLight, out fillHd);
        // sin especular para no “plastificar”
        fillHd.affectDiffuse = true;
        fillHd.affectSpecular = false;

        SetPointLumens(fillLight, fillHd, 0f);
        createdFillAtRuntime = true;
    }

    // === Moon ===
    private IEnumerator EnableDirectionalMoon()
    {
        if (!moonLight)
        {
            var go = new GameObject("Runtime Moon Light (Blackout)");
            go.transform.rotation = Quaternion.Euler(moonDirectionEuler);
            moonLight = go.AddComponent<Light>();
            moonLight.type = LightType.Directional;
            moonLight.color = moonLightColor;
            moonLight.shadows = moonCastShadows ? LightShadows.Soft : LightShadows.None;
            EnsureHDData(moonLight, out moonHd);
            createdMoonLightAtRuntime = true;
        }

        float start = GetDirectionalLux(moonLight, moonHd);
        float t = 0f;
        while (t < moonFadeSeconds)
        {
            float a = (moonFadeSeconds <= 0f) ? 1f : (t / moonFadeSeconds);
            SetDirectionalLux(moonLight, moonHd, Mathf.Lerp(start, moonLightIntensityLux, a));
            t += Time.deltaTime;
            yield return null;
        }
        SetDirectionalLux(moonLight, moonHd, moonLightIntensityLux);
    }

    // === Exposure ===
    private void EnsureExposureExistsIfNeeded()
    {
        if (!tweakAutoExposure || hasExposure || !createExposureIfMissing) return;

        var go = new GameObject("Runtime Exposure (Blackout)");
        runtimeVolume = go.AddComponent<Volume>();
        runtimeVolume.isGlobal = true;
        runtimeVolume.priority = 999f;
        runtimeProfile = ScriptableObject.CreateInstance<VolumeProfile>();
        runtimeVolume.profile = runtimeProfile;

        runtimeProfile.TryGet(out exp);
        if (exp == null) exp = runtimeProfile.Add<Exposure>();
        exp.active = true;

        hasExposure = true;
        usingRuntimeVolume = true;
    }

    private IEnumerator EaseExposureAuto()
    {
        exp.mode.value = ExposureMode.Automatic;

        float t = 0f;
        float startMin = exp.limitMin.value;
        float startMax = exp.limitMax.value;
        float startComp = exp.compensation.value;

        while (t < exposureEaseSeconds)
        {
            float a = (exposureEaseSeconds <= 0f) ? 1f : (t / exposureEaseSeconds);
            exp.limitMin.value     = Mathf.Lerp(startMin, autoExposureMinEV, a);
            exp.limitMax.value     = Mathf.Lerp(startMax, autoExposureMaxEV, a);
            exp.compensation.value = Mathf.Lerp(startComp, exposureCompensation, a);
            t += Time.deltaTime;
            yield return null;
        }

        exp.limitMin.value     = autoExposureMinEV;
        exp.limitMax.value     = autoExposureMaxEV;
        exp.compensation.value = exposureCompensation;
    }

    // === HDRP helpers ===
    private static void EnsureHDData(Light L, out HDAdditionalLightData hd)
    {
        hd = L.GetComponent<HDAdditionalLightData>();
        if (!hd) hd = L.gameObject.AddComponent<HDAdditionalLightData>();
    }

    private static void SetDirectionalLux(Light L, HDAdditionalLightData hd, float lux)
    {
        if (hd) hd.SetIntensity(lux, LightUnit.Lux);
        else    L.intensity = lux; // fallback
    }
    private static float GetDirectionalLux(Light L, HDAdditionalLightData hd) => hd ? hd.intensity : L.intensity;

    private static void SetPointLumens(Light L, HDAdditionalLightData hd, float lumens)
    {
        if (hd) hd.SetIntensity(lumens, LightUnit.Lumen);
        else    L.intensity = lumens; // fallback
    }
    private static float GetPointLumens(Light L, HDAdditionalLightData hd) => hd ? hd.intensity : L.intensity;

    // === Restore ===
    public void Restore()
    {
        if (hasBackup) SetMainLightsEnabled(true);

        if (useDirectionalMoon && moonLight)
        {
            SetDirectionalLux(moonLight, moonHd, 0f);
            if (createdMoonLightAtRuntime)
            {
                Destroy(moonLight.gameObject);
                moonLight = null;
                moonHd = null;
                createdMoonLightAtRuntime = false;
            }
        }

        if (useCameraFill && fillLight)
        {
            SetPointLumens(fillLight, fillHd, 0f);
            if (createdFillAtRuntime)
            {
                Destroy(fillLight.gameObject);
                fillLight = null;
                fillHd = null;
                createdFillAtRuntime = false;
            }
        }

        if (hasExposure)
        {
            if (usingRuntimeVolume && runtimeVolume)
            {
                Destroy(runtimeVolume.gameObject);
                runtimeVolume = null;
                runtimeProfile = null;
                usingRuntimeVolume = false;
            }
            else
            {
                exp.mode.value         = expModeBackup;
                exp.compensation.value = expCompBackup;
                exp.limitMin.value     = limitMinBackup;
                exp.limitMax.value     = limitMaxBackup;
                exp.fixedExposure.value= fixedEVBackup;
            }
        }
    }

    // === Auto-collect helpers ===
    private bool IsExcluded(Light L)
    {
        if (!L) return true;

        // Excluir las creadas por este controlador
        if (L == moonLight || L == fillLight) return true;

        // Exclusiones explícitas
        if (explicitExclusions != null)
        {
            for (int i = 0; i < explicitExclusions.Length; i++)
                if (explicitExclusions[i] == L) return true;
        }

        // Exclusiones por tag
        if (excludeTags != null && excludeTags.Length > 0)
        {
            var tag = L.tag;
            for (int i = 0; i < excludeTags.Length; i++)
                if (!string.IsNullOrEmpty(excludeTags[i]) && tag == excludeTags[i])
                    return true;
        }

        return false;
    }

    private void CollectSceneLights()
    {
        Light[] all;
#if UNITY_2022_2_OR_NEWER
        all = FindObjectsByType<Light>(FindObjectsSortMode.None);
#else
        all = FindObjectsOfType<Light>(true); // incluye inactivas
#endif
        var list = new List<Light>(all.Length);
        for (int i = 0; i < all.Length; i++)
        {
            var L = all[i];
            if (!IsExcluded(L))
                list.Add(L);
        }
        mainLights = list.ToArray();
    }

    // === Debug context menu ===
    [ContextMenu("DEBUG/Recollect Lights")]
    private void DEBUG_RecollectLights()
    {
        CollectSceneLights();
        BackupLights();
        Debug.Log($"[Blackout] Recolectadas {mainLights?.Length ?? 0} luces.");
    }

    [ContextMenu("DEBUG/Blackout Now")]
    private void DEBUG_BlackoutNow() { StartCoroutine(BlackoutRoutine()); }

    [ContextMenu("DEBUG/Restore")]
    private void DEBUG_Restore() { Restore(); }
}
