using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioController : MonoBehaviour
{
    public static AudioController Instance { get; private set; }

    [Header("Fuentes de Audio")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Clips de Sonido (orden según SoundType)")]
    [SerializeField] private AudioClip[] soundClips;

    [Header("Volúmenes")]
    [Range(0f, 1f)]
    private float musicVolume = 1f;
    [Range(0f, 1f)]
    private float sfxVolume = 1f;

    public float MusicVolume
    {
        get => musicVolume;
        set
        {
            musicVolume = value;
            musicSource.volume = musicVolume;
        }
    }

    public float SfxVolume
    {
        get => sfxVolume;
        set
        {
            sfxVolume = value;
            sfxSource.volume = sfxVolume;
        }
    }



    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        MusicVolume = musicSource.volume;
        Debug.Log("Music Volume: " + MusicVolume);

        SfxVolume = sfxSource.volume;
        Debug.Log("SFX Volume: " + SfxVolume);
    }

    public void PlaySFX(AudioType type, bool loop = false)
    {
        AudioClip clip = soundClips[(int)type];
        if (clip == null) return;

        if (loop)
        {
            sfxSource.clip = clip;
            sfxSource.loop = true;
            sfxSource.Play();
        }
        else
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    public void PlayMusic(AudioType type, bool loop = true)
    {

        AudioClip clip = soundClips[(int)type];
        if (clip != null)
        {
            PlayMusic(clip, loop);
        }
    }

    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (clip == null) return;
        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.Play();
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    public void PlaySFXClip(AudioClip clip, bool loop = false)
    {
        if (clip == null) return;

        if (loop)
        {
            sfxSource.clip = clip;
            sfxSource.loop = true;
            sfxSource.Play();
        }
        else
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    public void PlayMusicClip(AudioClip clip, bool loop = true)
    {
        if (clip == null) return;

        if (loop)
        {
            musicSource.clip = clip;
            musicSource.loop = true;
            musicSource.Play();
        }
        else
        {
            musicSource.PlayOneShot(clip);
        }
    }

    //Detectamos el cambio de escena para poner la música adecuada
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //Debug.Log("Escena cargada: " + scene.name);
        if (scene.name == "Game")
            PlayMusic(AudioType.BackgroundMusicMenu);
    }

}
