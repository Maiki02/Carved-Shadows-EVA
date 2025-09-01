#define ENABLE_SPATIALIZER_API

using UnityEngine;
using UnityEngine.Serialization;
using System.Collections;
using System.Collections.Generic;

namespace DearVR
{
    /// <summary>
    /// Dear VR source, equivalent to Unity's AudioSource, 
    /// is responsible for object based binaural sounds
    /// This is the class you would need to put on your objects, 
    /// almost all aspects of sound can be adjusted
    /// from this class.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class DearVRSource : MonoBehaviour
    {
        /// Note: This ENUM is duplicated in UnityRoomList.h
        /// It was too difficult to move so some duplication is required
        public enum RoomList
        {
            ___No_Reverb___ = 0,
            Direct_Signal,
            Reflections_Only,
            ___RoomsHalls___,
            Concert_Hall1,
            Concert_Hall2,
            Recording_Hall_Large,
            Recording_Hall_Medium,
            Kings_Hall,
            Cathedral,
            Church,
            Chapel,
            Room_Large,
            Room_Medium,
            Room_Small,
            ___PostProduction___,
            Office1,
            Office2,
            Studio_Small,
            Conference_Hall_Medium,
            Cellar,
            Empty_Room,
            Livingroom_Small,
            Staircase,
            Corridor,
            Bathroom,
            Restroom,
            Car1,
            Car2,
            Booth,
            Cinema,
            Warehouse,
            Outdoor_Street,
            Outdoor_Alley,
            ___MusicProduction___,
            Live_Studio_Room,
            Live_Stage,
            Live_Arena,
            Ambience_Heavy,
            Ambience_Plate,
            Ambience_Medium,
            Ambience_Small,
            Vocal_Hall1,
            Vocal_Hall2,
            Vocal_Plate,
            Drum_Room1,
            Drum_Room2,
            Percussion_Plate,
            Percussion_Ambience,
            Acoustic_Hall,
            String_Hall,
            String_Plate
        };

        /// <summary>
        /// The occlusion mask. Used for ray tracing occluding objects.
        /// all objects having this layer, will be considered for occlussion 
        /// </summary>
        public LayerMask occlusionWallMask = -1;

        /// <summary>
        /// The obstruction mask. Used for ray tracing obstructing objects.
        /// all objects having this layer, will be considered for obstruction 
        /// </summary>
        public LayerMask obstructionWallMask = -1;

        /// <summary>
        /// how often should ray casting be performed for occlusion.
        /// </summary>
        public float occlusionRayUpdateTime = 1.0f;

        /// <summary>
        /// how often should ray casting be performed for obstruction.
        /// </summary>
        public float obstructionRayUpdateTime = 1.0f;

        private AudioSource auSource;

        /// <summary>
        /// Gets the current audio source, being processed by dearVR
        /// </summary>
        /// <value>The current audio source.</value>
        public AudioSource currentAudioSource
        {
			get { return auSource;}
			private set { currentAudioSource = value; }
        }

        /// <summary>
        /// The clip on the source is currenlty playing.
        /// </summary>
        public bool clipIsPlaying = false;

        private bool clipHasPlayedOnAwake = false;
        private bool isOverlapping = false;

        private float OldOcclusionLevel = 0.0f;
        private float OldObstructionLevel = 0.0f;


        [SerializeField] private float gainLevel = 0.0f;

        /// <summary>
        /// master gain of audio.
        /// </summary>
        /// <value>The gain level.</value>
        public float GainLevel
        {
			get { return gainLevel; }
            set
            {
                gainLevel = value;
                if (auSource)
                    auSource.SetSpatializerFloat((int)SpatializerParameter.MASTER_GAIN, value);
            }
        }

        [SerializeField] private float directLevel = 0.0f;

        /// <summary>
        /// Direct gain of the source, before reverb and reflection.
        /// </summary>
        /// <value>The direct level.</value>
        public float DirectLevel
        {
			get { return directLevel; }
            set
            {
                directLevel = value;
                if (auSource)
                    auSource.SetSpatializerFloat((int)SpatializerParameter.DIRECT_LEVEL, value);
            }
        }

        [SerializeField] private float reflectionLevel = 0.0f;

        /// <summary>
        /// Gets or sets the reflection level.
        /// </summary>
        /// <value>The reflection level.</value>
        public float ReflectionLevel
        {
            get { return reflectionLevel; }
            set
            {
                reflectionLevel = value;
                if (auSource)
                    auSource.SetSpatializerFloat((int)SpatializerParameter.REFLECTION_LEVEL, value);
            }
        }

        [SerializeField] private float reverbLevel = 0.0f;

        /// <summary>
        /// Gets or sets the reverb level.
        /// </summary>
        /// <value>The reverb level.</value>
        public float ReverbLevel
        {
            get { return reverbLevel; }
            set
            {
                reverbLevel = value;
                if (auSource)
                {
                    auSource.SetSpatializerFloat((int)SpatializerParameter.REVERB_LEVEL, value);
                }
            }
        }

        [SerializeField] private float azimuthCorrection = 1.0f;

        /// <summary>
        /// Gets or sets the azimuth correction. Can be used to scale the positioning of sounds 
        /// </summary>
        /// <value>The azimuth correction.</value>
        public float AzimuthCorrection
        {
            get { return azimuthCorrection; }
            set
            {
                azimuthCorrection = value;
                if (auSource)
                    auSource.SetSpatializerFloat((int)SpatializerParameter.AZIMUTH_CORRECTION, value);
            }
        }

        [SerializeField] private float distanceCorrection = 1.0f;

        /// <summary>
        /// Gets or sets the distance correction. Can be used to scale the positioning of sounds
        /// </summary>
        /// <value>The distance correction.</value>
        public float DistanceCorrection
        {
            get { return distanceCorrection; }
            set
            {
                distanceCorrection = value;
                if (auSource)
                    auSource.SetSpatializerFloat((int)SpatializerParameter.DISTANCE_CORRECTION, value);
            }
        }

        [SerializeField] private bool useUnityDistance = false;

        /// <summary>
        ///  should the engnine use unity distancec attenaution?.
        /// </summary>
        /// <value><c>true</c> if use unity distance; otherwise, <c>false</c>.</value>
        public bool UseUnityDistance
        {
            get { return useUnityDistance; }
            set
            {
                useUnityDistance = value;
                if (auSource)
                    auSource.SetSpatializerFloat((int)SpatializerParameter.USE_UNITY_DISTANCE, toFloat(value));
            }
        }

        [SerializeField] private bool auralization = false;

        /// <summary>
        /// turns realtime auralization. 
        /// This uses realtime room geometries to calculate realtime reflections. 
        /// NOTE: only works if either <see cref="DearVRManager.RoomAnalyzer"/> or
        /// <see cref="DearVRManager.SetRoomGeo"/> are set to true.
        /// </summary>
        /// <value><c>true</c> if auralization; otherwise, <c>false</c>.</value>
        public bool Auralization
        {
            get { return auralization; }
            set
            {
                auralization = value;
                if (auSource)
                {
                    auSource.SetSpatializerFloat((int)SpatializerParameter.AURALIZATION, toFloat(value));
                }
            }
        }

        [SerializeField]
        RoomList roomPreset = RoomList.Recording_Hall_Medium;

        /// <summary>
        /// Gets or sets the room preset.(aka virtual acoustic preset)
        /// </summary>
        /// <value>The room preset.</value>
        public RoomList RoomPreset
        {
            get { return roomPreset; }
            set
            {
                roomPreset = value;

                if (auSource)
                    auSource.SetSpatializerFloat((int)SpatializerParameter.ROOM_PRESET, (float)value);
            }
        }

        [SerializeField] private bool internalReverb = true;

        /// <summary>
        /// if set to true, this instance uses its own reverb engine, independent of the reverb send plugin
        /// </summary>
        /// <value><c>true</c> if internal reverb; otherwise, <c>false</c>.</value>
        public bool InternalReverb
        {
            get { return internalReverb; }
            set
            {
                SetReverbSends();
                internalReverb = value;
                if (auSource)
                    auSource.SetSpatializerFloat((int)SpatializerParameter.INTERNAL_REVERB, toFloat(value));
            }
        }

        [SerializeField] private bool performanceMode = false;

        /// <summary>
        /// activating this bypasses processing during idle state.
        ///  Play Audio Sources only with <see cref="DearVRPlay"/> ,
        /// <see cref="DearVRPlayOneShot(AudioClip)"/> or
        /// <see cref="DearVRPlayOnAwake"/>  flag. 
        /// WARNING: Do not use the Play() or PlayOnAwake flag in Performace Mode!"
        /// </summary>
        /// <value><c>true</c> if performance mode; otherwise, <c>false</c>.</value>
        public bool PerformanceMode
        {
            get { return performanceMode; }
            set
            {
                performanceMode = value;
                if (!performanceMode)
                {
                    BypassPerformance = false;
                }
                else
                {
                    if (!DearVRPlayOnAwake && !clipIsPlaying)
                    {
                        BypassPerformance = true;
                    }
                    else if (!clipHasPlayedOnAwake)
                    {
                        BypassPerformance = false;
                    }
                }
            }
        }

        [SerializeField] bool dearVRPlayOnAwake = false;

        /// <summary>
        /// should the audio play on awake
        /// </summary>
        /// <value><c>true</c> if dear VR play on awake; otherwise, <c>false</c>.</value>
        public bool DearVRPlayOnAwake
        {
			get { return dearVRPlayOnAwake; }
			set { dearVRPlayOnAwake = value; }
        }


        [SerializeField] private bool bypass = false;

        /// <summary>
        /// if set to true bypasses the dearVR engine.
        /// </summary>
        /// <value><c>true</c> if bypass; otherwise, <c>false</c>.</value>
        public bool Bypass
        {
			get { return bypass; }
            set
            {
                bypass = value;
                if (!auSource) return;
                if (!PerformanceMode || (PerformanceMode && value))
                {
                    auSource.SetSpatializerFloat((int)SpatializerParameter.BYPASS, toFloat(value));
                }
            }
        }


        [SerializeField] private bool bypassPerformance = false;

        private bool BypassPerformance
        {
			get { return bypassPerformance; }
            set
            {
                bypassPerformance = value;
                if (auSource && !Bypass && PerformanceMode)
                {
                    auSource.SetSpatializerFloat((int)SpatializerParameter.BYPASS, toFloat(value));
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the engine is actually processing audio.
        /// </summary>
        /// <value><c>true</c> if this instance is processing; otherwise, <c>false</c>.</value>
		public bool IsProcessing 
		{
			get { return !BypassPerformance; }
		}

        [SerializeField] private float reverbStopOverlap = 1.0f;

        /// <summary>
        /// Gets or sets the reverb tail in seconds, if using <see cref="DearVR.DearVRSource.DearVRPlay"/> or 
        /// <see cref="DearVR.DearVRSource.DearVRPlayOneShot(AudioClip)"/>, this variable is used to deactivate processing in engine 
        /// </summary>
        /// <value>The reverb stop overlap.</value>
        public float ReverbStopOverlap
        {
			get { return reverbStopOverlap; }
			set { reverbStopOverlap = value; }
        }

        [SerializeField] private float inputChannel = 0.0f;

        /// <summary>
        /// which side of stereo channel to use for binaural processing.
        /// NOTE: Due to an update in Unity's Spatial framework, changing this would not have any effect.
        /// Even with stereo files, only the left channel is used as input
        /// </summary>
        /// <value>The input channel.</value>
        public float InputChannel
        {
			get { return inputChannel; }
            set
            {
                inputChannel = 0.0f;
                if (auSource)
                    auSource.SetSpatializerFloat((int)SpatializerParameter.INPUT_CHANNEL, 0.0f);
            }
        }

        [SerializeField] private float reflectionLP = 20000.0f;

        /// <summary>
        /// lowpass filter frequency on reflections
        /// </summary>
        /// <value>The reflection L.</value>
        public float ReflectionLP
        {
			get { return reflectionLP; }
            set
            {
                reflectionLP = value;
                if (auSource)
                    auSource.SetSpatializerFloat((int)SpatializerParameter.REFLECTION_LOWPASS, value);
            }
        }

        [SerializeField] private float reverbLP = 20000.0f;

        /// <summary>
        /// lowpass filter frequency on reverb
        /// </summary>
        /// <value>The reverb L.</value>
        public float ReverbLP
        {
			get { return reverbLP; }
            set
            {
                reverbLP = value;
                if (auSource)
                    auSource.SetSpatializerFloat((int)SpatializerParameter.REVERB_LOWPASS, value);
            }
        }

        [SerializeField] private float roomSize = 100.0f;

        /// <summary>
        /// Sets the roomsize, that can be used to adjust the reverb tail. 
        /// If <see cref="DearVR.DearVRSource.Auralization"/> is on, then
        /// this also has an effect on reflections
        /// </summary>
        /// <value>The size of the room.</value>
        public float RoomSize
        {
			get { return roomSize; }
            set
            {
                roomSize = value;
                if (auSource)
                    auSource.SetSpatializerFloat((int)SpatializerParameter.ROOM_SIZE, value);
            }
        }


        [SerializeField] private bool bassBoost = false;

        /// <summary>
        /// filter, used to boost up the low end. Sometimes this is necessary 
        /// to build up more low end, when binauralising sounds.
        /// </summary>
        /// <value><c>true</c> if bass boost; otherwise, <c>false</c>.</value>
        public bool BassBoost
        {
			get { return bassBoost; }
            set
            {
                bassBoost = value;
                if (auSource)
                    auSource.SetSpatializerFloat((int)SpatializerParameter.BASS_BOOST, toFloat(value));
            }
        }

        [SerializeField] private float occlusionLevel = 0.6f;

        /// <summary>
        /// How much occlusion should have and effect on the sound
        /// </summary>
        /// <value>The occlusion level.</value>
        public float OcclusionLevel
        {
			get { return occlusionLevel; }
            set { occlusionLevel = value; }
        }

        [SerializeField] private float obsturctionLevel = 0.6f;

        /// <summary>
        /// how much obstruction should have an effect on the sound
        /// </summary>
        /// <value>The obstruction level.</value>
        public float ObstructionLevel
        {
			get { return obsturctionLevel; }
            set { obsturctionLevel = value; }
        }

        [FormerlySerializedAs("occlusionActiv")]
        [SerializeField] private bool occlusionActive = false;

        /// <summary>
        /// activates occlusion for this source
        /// </summary>
        /// <value><c>true</c> if occlusion activ; otherwise, <c>false</c>.</value>
        public bool OcclusionActive
        {
			get { return occlusionActive; }
            set
            {
                if (value == false)
                {
                    if (occlusionActive == true && auSource)
                    {
                        auSource.SetSpatializerFloat((int)SpatializerParameter.OCCLUSION, 0.0f);
                    }

                    OcclusionDebugRayCast = false;
                }

                occlusionActive = value;
            }
        }

        [FormerlySerializedAs("obstructionActiv")]
        [SerializeField] private bool obstructionActive = false;

        /// <summary>
        /// activates obstruction for this source
        /// </summary>
        /// <value><c>true</c> if obstruction activ; otherwise, <c>false</c>.</value>
        public bool ObstructionActive
        {
			get { return obstructionActive; }
            set
            {
                if (value == false)
                {
                    if (obstructionActive == true && auSource)
                    {
                        auSource.SetSpatializerFloat((int)SpatializerParameter.OBSTRUCTION, 0.0f);
                    }

                    ObstructionDebugRayCast = false;
                }

                obstructionActive = value;
            }
        }

        [SerializeField] private bool forceOcclusion = false;

        /// <summary>
        /// occlusion should be active irrespective of ray casting.
        /// </summary>
        /// <value><c>true</c> if force occlusion; otherwise, <c>false</c>.</value>
        public bool ForceOcclusion
        {
			get { return forceOcclusion; }
            set
            {
                forceOcclusion = value;
                if (forceOcclusion && occlusionActive && auSource)
                    auSource.SetSpatializerFloat((int)SpatializerParameter.OCCLUSION, OcclusionLevel);
            }
        }

        [SerializeField] private bool forceObstruction = false;

        /// <summary>
        /// obstruction should be active for this source, irrespective of ray casting. 
        /// </summary>
        /// <value><c>true</c> if force obstruction; otherwise, <c>false</c>.</value>
        public bool ForceObstruction
        {
			get { return forceObstruction; }
            set
            {
                forceObstruction = value;
                if (forceObstruction && obstructionActive && auSource)
                    auSource.SetSpatializerFloat((int)SpatializerParameter.OBSTRUCTION, ObstructionLevel);
            }
        }

        [SerializeField] private bool occlusionDebugRayCast = false;

        /// <summary>
        /// Show debug ray casts used for occlusion
        /// </summary>
        /// <value><c>true</c> if occlusion debug ray cast; otherwise, <c>false</c>.</value>
        public bool OcclusionDebugRayCast
        {
			get { return occlusionDebugRayCast; }
			set { occlusionDebugRayCast = value; }
        }

        [SerializeField] private float occlusionRayUpdateFreq = 0.2f;

        /// <summary>
        /// ray casting frequency used for occlusion detection
        /// </summary>
        /// <value>The occlusion ray update freq.</value>
        public float OcclusionRayUpdateFreq
        {
			get { return occlusionRayUpdateFreq; }
			set { occlusionRayUpdateFreq = value; }
        }

        [SerializeField] float obstructionRayUpdateFreq = 0.2f;

        /// <summary>
        /// ray casting frequency used for obstruction detection
        /// </summary>
        /// <value>The obstruction ray update freq.</value>
        public float ObstructionRayUpdateFreq
        {
			get { return obstructionRayUpdateFreq; }
			set { obstructionRayUpdateFreq = value; }
        }

        [SerializeField] private bool obstructionDebugRayCast = false;

        /// <summary>
        /// Show debug ray casts used for obstruction
        /// </summary>
        /// <value><c>true</c> if occlusion debug ray cast; otherwise, <c>false</c>.</value>
        public bool ObstructionDebugRayCast
        {
			get { return obstructionDebugRayCast; }
			set { obstructionDebugRayCast = value; }
        }

        [SerializeField] private DearVRSerializedReverb[] reverbSends;

        private DearVRSerializedReverb[] reverbSendsTemp;

        /// <summary>
        /// use this fuction to set the the reverb sends, 
        /// these will be used to send the binauralised sound to the reverb bus
        /// only used if internal reverb is inactive
        /// </summary>
        /// <param name="rl">Rl.</param>
        public void SetReverbSends(DearVRSerializedReverb[] rl)
        {
            reverbSends = rl;
            SetReverbSends();
        }

        /// <summary>
        /// returns an instance of current reverb send structure, 
        /// can be used to edit the reverb structure with a subsequent call to
        /// <see cref="DearVRSource.SetReverbSends"/>
        /// </summary>
        /// <returns>The reverb send list.</returns>
        public DearVRSerializedReverb[] GetReverbSendList()
        {
            return reverbSends;
        }

        /// <summary>
        /// used internaly by dearVR, do not use this function to set reverb sends
        /// instead use <see cref="GetReverbSendList"/>
        ///  and <see cref="DearVRSource.SetReverbSends"/>
        /// </summary>
        public void SetReverbSends()
        {
            if (reverbSends == null)
            {
                reverbSendsTemp = null;
                return;
            }

            reverbSendsTemp = new DearVRSerializedReverb[reverbSends.GetLength(0)];

            for (int i = 0; i < reverbSends.GetLength(0); ++i)
            {
                reverbSendsTemp[i] = new DearVRSerializedReverb();
                reverbSendsTemp[i].roomIndex = reverbSends[i].roomIndex;
                reverbSendsTemp[i].send = reverbSends[i].send;
            }

            if (auSource)
            {
                auSource.SetSpatializerFloat((int)SpatializerParameter.REVERB_SIZE, reverbSends.GetLength(0));

                for (int i = 0; i < reverbSends.GetLength(0); i++)
                {
                    
                    auSource.SetSpatializerFloat((int)SpatializerParameter.REVERB_ID, reverbSends[i].roomIndex);
                    auSource.SetSpatializerFloat((int)SpatializerParameter.REVERB_SEND, reverbSends[i].send);
                }
            }
        }

        /// <summary>
        /// used internaly by dearVR, calling this function would not have any effect from client side
        /// </summary>
        /// <returns><c>true</c>, if sends changed was reverbed, <c>false</c> otherwise.</returns>
        public bool ReverbSendsChanged()
        {
            if (reverbSends == null)
            {
                return false;
            }

            if (reverbSendsTemp == null)
            {
                return true;
            }

            if (reverbSends.GetLength(0) != reverbSendsTemp.GetLength(0))
            {
                return true;
            }

            for (int i = 0; i < reverbSends.GetLength(0); ++i)
            {
                if (reverbSends[i].roomIndex != reverbSendsTemp[i].roomIndex ||
                    System.Math.Abs(reverbSends[i].send - reverbSendsTemp[i].send) > .01f)
                {
                    return true;
                }
            }

            return false;
        }

        void OnEnable()
        {
            auSource = GetComponent<AudioSource>();
            if (DearVRManager.Instance)
            {
                auSource.spatialize = true;
                auSource.spatialBlend = 0.0f;

                StartCoroutine(AwakePresets());
                if (DearVRPlayOnAwake && PerformanceMode && BypassPerformance)
                {
                    DearVRPlay();
                }
            }
            else
            {
                Debug.LogWarning(
                    "DEARVR: No DearVRManager-Script in Scene. dearVR won't work!" +
                    " Please add the DearVRManager-Script or delete all active DearVRSource-Objects!");
            }
        }

        void Awake()
        {
#if UNITY_5_0 || UNITY_5_1 //|| UNITY_5_4 || UNITY_5_5 || UNITY_EDITOR
            UnityEngine.Debug.LogError("DEARVR: DearVR Engine is not supported for Unity 5.0 or 5.1 since Unity Spatializer SDK was released with Unity 5.2+");
#endif

            auSource = GetComponent<AudioSource>();

            if (DearVRManager.Instance)
            {
                auSource.spatialize = true;
                auSource.spatialBlend = 0.0f;

                if (DearVRPlayOnAwake && PerformanceMode)
                {
                    DearVRPlay();
                }
            }
            else
            {
                Debug.LogWarning("DEARVR: No DearVRManager-component found in Scene." +
                                 " dearVR won't work!");
            }

            StartCoroutine(AwakePresets());
        }

        void Start()
        {
            if (auSource.clip)
            {
                if (PerformanceMode && auSource.playOnAwake)
                {
                    Debug.LogWarning(
                        "DEARVR: In Performance-Mode Audio Source PlayOnAwake incurs high dsp load. " +
                        "Only play AudioSources with DearVRPlay(), DearVRPlayOneShot(AudioClip) " +
                        "or DearVRPlayOnAwake-Flag!");
                }
            }

            if (!DearVRManager.Instance)
            {
                Debug.LogWarning("DEARVR: No DearVRManager-component found in Scene." +
                                 " dearVR won't work!");
            }


            if (auSource)
            {
                auSource.spatialize = true;
                if (!auSource.outputAudioMixerGroup && !InternalReverb)
                {
                    Debug.LogWarning(
                        "DEARVR: If implementing dearVR Reverb Plugin, " +
                        "then dearVR Source has to be routed to the Audio Mixers" +
                        " master group or a group with no dearVR Reverb Plugin!");
                }
            }
        }

        /// <summary>
        /// plays the source in performance mode, 
        /// Unity processes all audio sources irrespective of them playing or not,
        /// this method circumvents that problem, by deactivating the processing in dearVR
        /// if you use this method you have to use <see cref="DearVRStop"/> 
        /// since this accounts for the 
        /// reverb tail before stopping dearVR processing,
        ///  Also you can change <see cref="ReverbStopOverlap"/> 
        ///  to account for different reverb tails
        /// after which the actual stop takes place.
        /// </summary>
        public void DearVRPlay()
        {
            if (auSource)
            {
                clipIsPlaying = true;
                BypassPerformance = false;
                if (performanceMode)
                    clipHasPlayedOnAwake = false;
                auSource.Play();
            }
            else
            {
                Debug.LogWarning("DEARVR: Audio Source not found!");
            }
        }

        /// <summary>
        /// plays the source in performance mode and with oneshot, essentially calling unity's 
        /// PlayOneShot <see>
        ///         <cref>UnityEngine.AudioSource.PlayOneShot</cref>
        ///     </see>
        ///     <see cref="DearVR.DearVRSource.DearVRPlay"/> 
        /// </summary>
        /// <param name="clip">Clip.</param>
        public void DearVRPlayOneShot(AudioClip clip)
        {
            if (auSource)
            {
                clipIsPlaying = true;
                BypassPerformance = false;
                auSource.PlayOneShot(clip);
            }
            else
            {
                Debug.LogWarning("DEARVR: Audio Source not found!");
            }
        }

        /// <summary>
        /// Stops the audio and then processing after <see cref="DearVR.DearVRSource.ReverbStopOverlap"/> (in seconds) is over
        /// </summary>
        public void DearVRStop()
        {
            if (auSource)
            {
                BypassPerformance = true;
                auSource.Stop();
            }
            else
            {
                Debug.LogWarning("DEARVR: Can't stop, Audio Source not playing, or doesnt exist anymore!");
            }
        }


        IEnumerator AwakePresets()
        {
            float roomPresetInEngine = -1;

            while ((int)roomPresetInEngine == -1)
            {
                auSource.GetSpatializerFloat((int)SpatializerParameter.ROOM_PRESET, out roomPresetInEngine);
                RoomPreset = roomPreset;

                yield return new WaitForEndOfFrame();
            }

            GainLevel = gainLevel;
            DirectLevel = directLevel;
            ReflectionLevel = reflectionLevel;
            ReverbLevel = reverbLevel;
            AzimuthCorrection = azimuthCorrection;
            DistanceCorrection = distanceCorrection;
            UseUnityDistance = useUnityDistance;
            Auralization = auralization;
            RoomPreset = roomPreset;
            OcclusionLevel = occlusionLevel;
            ObstructionLevel = obsturctionLevel;
            InternalReverb = internalReverb;
            InputChannel = inputChannel;
            ReflectionLP = reflectionLP;
            ReverbLP = reverbLP;
            RoomSize = roomSize;
            BassBoost = bassBoost;
            Bypass = bypass;
            BypassPerformance = bypassPerformance;
            PerformanceMode = performanceMode;
            DearVRPlayOnAwake = dearVRPlayOnAwake;
            ReverbStopOverlap = reverbStopOverlap;

            OcclusionActive = occlusionActive;
            ObstructionActive = obstructionActive;

            OcclusionDebugRayCast = occlusionDebugRayCast;
            OcclusionRayUpdateFreq = occlusionRayUpdateFreq;

            ObstructionDebugRayCast = obstructionDebugRayCast;
            ObstructionRayUpdateFreq = obstructionRayUpdateFreq;
        }

        /// <summary>
        /// Enables the bypass performance.
        /// </summary>
        void EnableBypassPerformance()
        {
            if (!auSource.isPlaying)
            {
                BypassPerformance = true;
                clipIsPlaying = false;
            }

            if (DearVRPlayOnAwake)
            {
                clipHasPlayedOnAwake = true;
            }

            isOverlapping = false;
        }


        void OnDisable()
        {
            if (PerformanceMode)
            {
                DearVRStop();
            }
        }

        void Update()
        {
            if (PerformanceMode)
            {
                if (auSource.isPlaying)
                {
                    clipIsPlaying = true;
                }
                else if (clipIsPlaying == true && !isOverlapping)
                {
                    isOverlapping = true;
                    Invoke("EnableBypassPerformance", ReverbStopOverlap);
                }
            }

            if (occlusionActive)
            {
                if (ForceOcclusion && auSource)
                {
                    auSource.SetSpatializerFloat((int)SpatializerParameter.OCCLUSION, OcclusionLevel);
                }
                else
                {
                    if (OcclusionLevel > 0.0f || OldOcclusionLevel > 0.0f)
                    {
                        occlusionRayUpdateTime += Time.deltaTime;
                        if (occlusionRayUpdateTime >= OcclusionRayUpdateFreq)
                        {
                            occlusionRayUpdateTime -= OcclusionRayUpdateFreq;
                            OcclusionCast();
                        }
                    }
                }
            }
        
            if (ObstructionActive)
            {
                if (ForceObstruction && auSource)
                {
                    auSource.SetSpatializerFloat((int)SpatializerParameter.OBSTRUCTION, ObstructionLevel);
                }
                else
                { 
                    if (ObstructionLevel > 0.0f || OldObstructionLevel > 0.0f)
                    {
                        obstructionRayUpdateTime += Time.deltaTime;
                        if (obstructionRayUpdateTime >= ObstructionRayUpdateFreq)
                        {
                            obstructionRayUpdateTime -= ObstructionRayUpdateFreq;
                            ObstructionCast();
                        }
                    }
                }
            }
        }

        void OcclusionCast()
        {
            OldOcclusionLevel = OcclusionLevel;
            RaycastHit hit = new RaycastHit();

            Vector3 listenerPosition;

            if (DearVRManager.DearListener != null)
            {
                listenerPosition = DearVRManager.DearListener.transform.position;
            }
            else if (Camera.main != null)
            {
                listenerPosition = Camera.main.transform.position;
            }
            else
            {
                listenerPosition = Vector3.zero;
            }

            Vector3 dir = transform.position - listenerPosition;

            if (Physics.Raycast(listenerPosition, dir, out hit, dir.magnitude, occlusionWallMask.value) &&
                (hit.collider.gameObject != gameObject))
            {
                if (OcclusionDebugRayCast)
                {
                    Debug.DrawRay(listenerPosition, auSource.gameObject.transform.position - listenerPosition,
                        Color.red, OcclusionRayUpdateFreq, true);
                }

                auSource.SetSpatializerFloat((int)SpatializerParameter.OCCLUSION, OcclusionLevel);
            }
            else
            {
                if (OcclusionDebugRayCast)
                {
                    Debug.DrawRay(listenerPosition, auSource.gameObject.transform.position - listenerPosition,
                        Color.green, OcclusionRayUpdateFreq, true);
                }

                auSource.SetSpatializerFloat((int)SpatializerParameter.OCCLUSION, 0.0f);
            }
        }

        void ObstructionCast()
        {
            OldObstructionLevel = ObstructionLevel;
            RaycastHit hit = new RaycastHit();

            Vector3 listenerPosition;

            if (DearVRManager.DearListener != null)
            {
                listenerPosition = DearVRManager.DearListener.transform.position;
            }
            else if (Camera.main != null)
            {
                listenerPosition = Camera.main.transform.position;
            }
            else
            {
                listenerPosition = Vector3.zero;
            }

            Vector3 dir = transform.position - listenerPosition;

            if (Physics.Raycast(listenerPosition, dir, out hit, dir.magnitude, obstructionWallMask.value) &&
                (hit.collider.gameObject != gameObject))
            {
                if (ObstructionDebugRayCast)
                {
                    Debug.DrawRay(listenerPosition, auSource.gameObject.transform.position - listenerPosition,
                        Color.red, ObstructionRayUpdateFreq, true);
                }

                auSource.SetSpatializerFloat((int)SpatializerParameter.OBSTRUCTION, ObstructionLevel);
            }
            else
            {
                if (ObstructionDebugRayCast)
                {
                    Debug.DrawRay(listenerPosition, auSource.gameObject.transform.position - listenerPosition,
                        Color.green, ObstructionRayUpdateFreq, true);
                }

                auSource.SetSpatializerFloat((int)SpatializerParameter.OBSTRUCTION, 0.0f);
            }
        }

        private float toFloat(bool val)
        {
            return val == true ? 1.0f : 0.0f;
        }
    }
}