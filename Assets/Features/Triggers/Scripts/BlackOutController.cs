using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class BlackoutController : MonoBehaviour
{
    [Header("Luces principales (se apagan)")]
    [SerializeField] private Light[] mainLights;

    [Header("Moonlight (se crea si falta)")]
    [SerializeField] private Light moonLight;
    [SerializeField] private bool enableMoonLight = true;
    [SerializeField] private bool createMoonLightIfMissing = true;
    [SerializeField] private Vector3 moonDirectionEuler = new Vector3(15f, -35f, 0f);
    [SerializeField] private bool moonCastShadows = false;
    [SerializeField] private Color moonLightColor = new Color(0.75f, 0.85f, 1f);
    [Tooltip("Directional en HDRP: Lux")]
    [SerializeField] private float moonLightIntensityLux = 0.8f;

    [Header("Flicker")]
    [SerializeField] private float flickerSeconds = 1.1f;
    [SerializeField] private float flickerMinInterval = 0.06f;
    [SerializeField] private float flickerMaxInterval = 0.14f;

    [Header("HDRP Exposure")]
    [SerializeField] private Volume globalVolume;
    [SerializeField] private bool tweakAutoExposure = true;
    [SerializeField] private bool createExposureIfMissing = true;
    [SerializeField] private float autoExposureMinEV = -6.0f;
    [SerializeField] private float autoExposureMaxEV = 0.0f;
    [SerializeField] private float exposureCompensation = 1.0f;
    [SerializeField] private float exposureEaseSeconds = 0.25f;

    [Header("SFX (opcional)")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip sfxFlicker;
    [SerializeField] private AudioClip sfxPowerDown;

    // backups luces
    private float[] mainOrigIntensity;
    private bool[] mainOrigEnabled;
    private bool hasBackup;

    // exposición
    private Exposure exp;
    private bool hasExposure;
    private bool usingRuntimeVolume;
    private Volume runtimeVolume;
    private VolumeProfile runtimeProfile;

    private ExposureMode expModeBackup;
    private float expCompBackup, limitMinBackup, limitMaxBackup, fixedEVBackup;

    // moon runtime
    private bool createdMoonLightAtRuntime = false;
    private HDAdditionalLightData moonHd;

    void Awake()
    {
        BackupLights();
        if (!sfxSource) sfxSource = GetComponent<AudioSource>();
        SetupExposureRefIfAvailable();

        if (moonLight && enableMoonLight)
        {
            moonLight.color = moonLightColor;
            EnsureHDData(moonLight, out moonHd);
            SetDirectionalLux(moonLight, moonHd, 0f); // empezar apagada
            moonLight.shadows = moonCastShadows ? LightShadows.Soft : LightShadows.None;
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
        hasBackup = true;
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
        EnsureMoonlightExistsIfNeeded();
        EnsureExposureExistsIfNeeded();

        if (sfxSource && sfxFlicker) sfxSource.PlayOneShot(sfxFlicker);
        yield return StartCoroutine(FlickerCoroutine());

        if (sfxSource && sfxPowerDown) sfxSource.PlayOneShot(sfxPowerDown);
        SetMainLightsEnabled(false);

        if (enableMoonLight && moonLight)
            yield return StartCoroutine(FadeMoonlight(moonLightIntensityLux, 0.35f));

        if (tweakAutoExposure && hasExposure)
        {
            exp.mode.value = ExposureMode.Automatic;

            float t = 0f;
            float startMin = exp.limitMin.value;
            float startMax = exp.limitMax.value;
            float startComp= exp.compensation.value;

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
    }

    private void EnsureMoonlightExistsIfNeeded()
    {
        if (!enableMoonLight) return;
        if (moonLight != null) return;
        if (!createMoonLightIfMissing) return;

        var go = new GameObject("Runtime Moon Light (Blackout)");
        go.transform.rotation = Quaternion.Euler(moonDirectionEuler);
        moonLight = go.AddComponent<Light>();
        moonLight.type = LightType.Directional;
        moonLight.color = moonLightColor;
        moonLight.shadows = moonCastShadows ? LightShadows.Soft : LightShadows.None;

        EnsureHDData(moonLight, out moonHd);
        SetDirectionalLux(moonLight, moonHd, 0f);
        createdMoonLightAtRuntime = true;
    }

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
                L.enabled = (hasBackup ? mainOrigEnabled[i] : true);
                L.intensity = (hasBackup ? mainOrigIntensity[i] : L.intensity);
            }
            else
            {
                L.enabled = true;
                L.intensity = 0f;
            }
        }
    }

    private IEnumerator FadeMoonlight(float targetLux, float seconds)
    {
        if (!moonLight) yield break;
        EnsureHDData(moonLight, out moonHd);

        float start = GetDirectionalLux(moonLight, moonHd);
        float t = 0f;
        while (t < seconds)
        {
            float a = seconds <= 0f ? 1f : (t / seconds);
            SetDirectionalLux(moonLight, moonHd, Mathf.Lerp(start, targetLux, a));
            t += Time.deltaTime;
            yield return null;
        }
        SetDirectionalLux(moonLight, moonHd, targetLux);
    }

    private static void EnsureHDData(Light L, out HDAdditionalLightData hd)
    {
        hd = L.GetComponent<HDAdditionalLightData>();
        if (!hd) hd = L.gameObject.AddComponent<HDAdditionalLightData>();
        // No tocamos propiedades de unidad aquí; siempre seteamos con SetIntensity(..., LightUnit.Lux)
        hd.affectDiffuse = true;
        hd.affectSpecular = false;
    }

    private static void SetDirectionalLux(Light L, HDAdditionalLightData hd, float lux)
    {
        if (hd) hd.SetIntensity(lux, LightUnit.Lux);
        else    L.intensity = lux; // fallback
    }

    private static float GetDirectionalLux(Light L, HDAdditionalLightData hd)
    {
        return hd ? hd.intensity : L.intensity;
    }

    public void Restore()
    {
        if (hasBackup) SetMainLightsEnabled(true);

        if (enableMoonLight && moonLight)
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

    [ContextMenu("DEBUG/Blackout Now")]
    private void DEBUG_BlackoutNow() { StartCoroutine(BlackoutRoutine()); }

    [ContextMenu("DEBUG/Restore")]
    private void DEBUG_Restore() { Restore(); }
}
