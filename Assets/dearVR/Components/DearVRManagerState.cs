using UnityEngine;
using System;
using DearVR;

[Serializable]
public class DearVRManagerState : ScriptableObject
{
    private static DearVRManagerState instance_;

    public static DearVRManagerState Instance
    {
        get
        {
            if (!instance_)
            {
                instance_ = Resources.Load<DearVRManagerState>("DearVRManagerState");
            }
            
            if (!instance_)
            {
                instance_ = CreateInstance<DearVRManagerState>();
            }

            return instance_;
        }
    }
    
    private static DearVRAudioPlugin audioPlugin_ = new DearVRAudioPlugin();
    public DearVRAudioPlugin AudioPlugin
    {
        get { return audioPlugin_; }
    }

    private static DearVRVersionInfo versionInfo_ = new DearVRVersionInfo();
    public string VersionInfo
    {
        get { return versionInfo_.DearVRGetVersionString(); }
    }

    private bool needRoomUpdate_;
    public bool NeedRoomUpdate
    {
        get { return needRoomUpdate_; }
        set { needRoomUpdate_ = value; }
    }
    
    private const float maxRoom_ = 40.0f;
    public float MaxRoom
    {
        get { return maxRoom_; }
    }
    
    private const float minRoom_ = -1.0f;
    public float MinRoom
    {
        get { return minRoom_; }
    }
    
    /// <summary>
    /// if set to true, room geometry is sent to the engine and used for reflections, turning this on will turn
    /// off <see cref="DearVR.DearVRManager.RoomAnalyzer"/>
    /// </summary>
    /// <value><c>true</c> if set room geo; otherwise, <c>false</c>.</value>
    [SerializeField] private bool setRoomGeo = false;
    public bool SetRoomGeo
    {
        get { return setRoomGeo; }
        set
        {
            AudioPlugin.DearVRSetExternalRoomGeo(value || roomAnalyzer);
            if (value)
            {
                roomAnalyzer = false;
            }
            setRoomGeo = value;
        }
    }
    
    /// <summary>
    /// if set to true, room geometry is calculated by ray tracing and sent to the engine and used for reflections
    /// turning this on would turn <see cref="DearVR.DearVRManager.DearVRManagerState.Instance.SetRoomGeo"/> off 
    /// </summary>
    /// <value><c>true</c> if set room geo; otherwise, <c>false</c>.</value>
    [SerializeField] private bool roomAnalyzer = false;
    public bool RoomAnalyzer
    {
        get { return roomAnalyzer; }
        set
        {
            AudioPlugin.DearVRSetExternalRoomGeo(value || setRoomGeo);
            if (value)
            {
                setRoomGeo = false;
            }
            else
            {
                debugRoomAnalyzer = false;
            }
            roomAnalyzer = value;
        }
    }

    /// <summary>
    /// turns room analyzer debuging rays off/on
    /// </summary>
    [SerializeField] private bool debugRoomAnalyzer = false;
    public bool DebugRoomAnalyzer
    {
        get { return debugRoomAnalyzer; }
        set { debugRoomAnalyzer = value; }
    }
    
    /// <summary>
    /// how often room geometries are updated with ray tracing
    /// </summary>
    [SerializeField] private float roomUpdateFreq = 3.0f;
    public float RoomUpdateFreq
    {
        get { return roomUpdateFreq; }
        set { roomUpdateFreq = Mathf.Clamp(value, 0.01f, 60.0f); }
    }

    /// <summary>
    /// Gets or sets up side and down side of room geometry.
    /// </summary>
    /// <value>Up down geo.</value>
    [SerializeField] private Vector2 upDownGeo = new Vector2(1.3f, 1.8f);
    public Vector2 UpDownGeo
    {
        get { return upDownGeo; }
        set
        {
            upDownGeo.x = Mathf.Clamp(value.x, MinRoom, MaxRoom);
            upDownGeo.y = Mathf.Clamp(value.y, MinRoom, MaxRoom);
            needRoomUpdate_ = true;
        }
    }
    
    /// <summary>
    /// Gets or sets the front side and back side of room geometry.
    /// </summary>
    /// <value>The front back geo.</value>
    [SerializeField] private Vector2 frontBackGeo = new Vector2(3.0f, 2.5f);
    public Vector2 FrontBackGeo
    {
        get { return frontBackGeo; }
        set
        {
            frontBackGeo.x = Mathf.Clamp(value.x, MinRoom, MaxRoom);
            frontBackGeo.y = Mathf.Clamp(value.y, MinRoom, MaxRoom);
            needRoomUpdate_ = true;
        }
    }

    /// <summary>
    /// Gets or sets the left side and the right side of room geometry.
    /// </summary>
    /// <value>The left right geo.</value>
    [SerializeField] private Vector2 leftRightGeo = new Vector2(2.2f, 2.3f);
    public Vector2 LeftRightGeo
    {
        get { return leftRightGeo; }
        set 
        { 
            leftRightGeo.x = Mathf.Clamp(value.x, MinRoom, MaxRoom);
            leftRightGeo.y = Mathf.Clamp(value.y, MinRoom, MaxRoom);
            needRoomUpdate_ = true;
        }
    }
    
    /// <summary>
    /// <see cref="DearVR.DearVRManager"/> Bypass the bniaural rendering for all sources, reflections and reverb processing
    /// are still active. effectively enabling speaker mode.
    /// Note: Loudspeaker Mode enables you to switch from 3D Audio for headphones to a loudspeaker compatible mix.
    /// Distance attenuation, Reflection, and Reverb levels are still maintained.
    /// </summary>
    /// <value><c>true</c> if bypass3D audio; otherwise, <c>false</c>.</value>
    [SerializeField] private bool bypass3DAudio = false;
    public bool Bypass3DAudio
    {
        get { return bypass3DAudio; }
        set
        {
            AudioPlugin.DearVRSetLoudspeakerMode(value);
            AudioPlugin.DearVRSetLoudspeakerModeReverb(value);
            bypass3DAudio = value;
        }
    }
    
    /// <summary>
    /// layers used for the raytracing of room geoemtries.
    /// </summary>
    /// <value>The room mask.</value>
    [SerializeField] private LayerMask roomMask = -1;
    public LayerMask RoomMask
    {
        get { return roomMask; }
        set { roomMask = value; }
    }
}
