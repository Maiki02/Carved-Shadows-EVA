using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class AnxietyFXController : MonoBehaviour
{
    public static AnxietyFXController Instance { get; private set; }

    [Header("Volume Global (puede ser tu Volume de post)")]
    [SerializeField] private Volume globalVolume;
    [SerializeField] private bool createRuntimeVolumeIfMissing = true;
    [SerializeField] private float volumePriority = 900f;

    [Header("Debug")]
    [SerializeField] private bool verbose = true;

    [Header("Intensidades máximas (peso=1)")]
    [SerializeField, Range(0f,1f)] private float maxChromaticAberration = 0.5f;
    [SerializeField, Range(0f,1f)] private float maxVignette = 0.6f;
    [SerializeField, Range(-1f,0f)] private float lensDistortionMin = -0.22f;
    [SerializeField, Range(0f,1f)] private float maxFilmGrain = 0.35f;
    [SerializeField, Range(0f,1f)] private float desaturate = 0.25f;
    [SerializeField] private float exposureCompensation = 0.2f;

    [Header("Suavizados")]
    [SerializeField] private float fadeInSeconds = 0.25f;
    [SerializeField] private float fadeOutSeconds = 0.30f;

    // Refs
    private ChromaticAberration ca;
    private Vignette vg;
    private LensDistortion ld;
    private FilmGrain grain;
    private ColorAdjustments colorAdj;
    private Exposure exposure;

    private Volume runtimeVolume;
    private VolumeProfile runtimeProfile;

    float currentWeight;
    Coroutine fadeCR;

    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (!globalVolume && createRuntimeVolumeIfMissing)
        {
            var go = new GameObject("Runtime_AnxietyFX_Volume");
            go.transform.SetParent(transform, false);
            runtimeVolume = go.AddComponent<Volume>();
            runtimeVolume.isGlobal = true;
            runtimeVolume.priority = volumePriority;
            runtimeProfile = ScriptableObject.CreateInstance<VolumeProfile>();
            runtimeVolume.profile = runtimeProfile;
            globalVolume = runtimeVolume;

            if (verbose) Debug.Log("[AnxietyFX] Creado Volume runtime (global, priority " + volumePriority + ")");
        }

        EnsureOverrides();
        if (verbose) DebugDump();
    }

    void EnsureOverrides()
    {
        if (!globalVolume)
        {
            Debug.LogWarning("[AnxietyFX] No hay Volume asignado y no se creó runtime.");
            return;
        }

        var profile = globalVolume.profile;
        if (!profile)
        {
            profile = ScriptableObject.CreateInstance<VolumeProfile>();
            globalVolume.profile = profile;
        }

        // Añadir (si faltan) y activar overrideState de cada property que vamos a tocar
        if (!profile.TryGet(out ca))      ca = profile.Add<ChromaticAberration>();
        ca.active = true;
        ca.intensity.overrideState = true;    ca.intensity.value = 0f;

        if (!profile.TryGet(out vg))      vg = profile.Add<Vignette>();
        vg.active = true;
        vg.intensity.overrideState = true;    vg.intensity.value = 0f;
        vg.smoothness.overrideState = true;   vg.smoothness.value = 0.75f;
        vg.color.overrideState = true;        vg.color.value = Color.black;

        if (!profile.TryGet(out ld))      ld = profile.Add<LensDistortion>();
        ld.active = true;
        ld.intensity.overrideState = true;    ld.intensity.value = 0f;
        ld.scale.overrideState = true;        ld.scale.value = 1f;

        if (!profile.TryGet(out grain))   grain = profile.Add<FilmGrain>();
        grain.active = true;
        grain.intensity.overrideState = true; grain.intensity.value = 0f;
        grain.response.overrideState = true;  grain.response.value = 0.8f;

        if (!profile.TryGet(out colorAdj)) colorAdj = profile.Add<ColorAdjustments>();
        colorAdj.active = true;
        colorAdj.saturation.overrideState = true; colorAdj.saturation.value = 0f;

        if (!profile.TryGet(out exposure)) exposure = profile.Add<Exposure>();
        exposure.active = true;
        exposure.compensation.overrideState = true; exposure.compensation.value = 0f;
    }

    void Apply(float w)
    {
        if (!globalVolume) return;

        if (ca)       ca.intensity.value        = maxChromaticAberration * w;
        if (vg)       vg.intensity.value        = maxVignette * w;
        if (ld)       ld.intensity.value        = Mathf.Lerp(0f, lensDistortionMin, w);
        if (grain)    grain.intensity.value     = maxFilmGrain * w;
        if (colorAdj) colorAdj.saturation.value = Mathf.Lerp(0f, -100f * desaturate, w);
        if (exposure) exposure.compensation.value = exposureCompensation * w;

        if (verbose)
            Debug.Log($"[AnxietyFX] Apply w={w:0.00}  CA={ca?.intensity.value:0.00}  Vignette={vg?.intensity.value:0.00}  LD={ld?.intensity.value:0.00}  Sat={colorAdj?.saturation.value:0.0}  Exp={exposure?.compensation.value:0.00}");
    }

    public void BeginAnxiety(float fade = -1f) { /* tu código igual */ }
    public void EndAnxiety(float fade = -1f)   { /* tu código igual */ }
    public void SetWeight(float w) { currentWeight = Mathf.Clamp01(w); Apply(currentWeight); }

    [ContextMenu("DEBUG Dump")]
    public void DebugDump()
    {
        if (!globalVolume) { Debug.Log("[AnxietyFX] SIN Volume"); return; }
        Debug.Log($"[AnxietyFX] Volume='{globalVolume.name}' isGlobal={globalVolume.isGlobal} priority={globalVolume.priority} layer={LayerMask.LayerToName(globalVolume.gameObject.layer)}");
        Debug.Log($"[AnxietyFX] Overrides: CA({ca!=null}), VG({vg!=null}), LD({ld!=null}), Grain({grain!=null}), ColorAdj({colorAdj!=null}), Exposure({exposure!=null})");
    }
}
