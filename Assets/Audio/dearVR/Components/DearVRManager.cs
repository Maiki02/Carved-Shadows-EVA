using UnityEngine;
using System.Collections.Generic;
using System.IO;

namespace DearVR
{
    /// <summary>
    /// Dear VR manager. A singleton manager class for dearVR global settings
    /// </summary>
    public class DearVRManager : MonoBehaviour
    {
        private float roomUpdateTime_ = 1.0f;
        private int roomUpdateSide_;

        private AudioListener managerListener_;

        /// <summary>
        /// Gets or sets the listener used by dearVR.
        /// </summary>
        /// <value>The dear listener.</value>
        public static AudioListener DearListener
        {
            get { return instance_.managerListener_; }
            set { instance_.managerListener_ = value; }
        }

        #region Singleton

        private static DearVRManager instance_;

        /// <summary>
        /// Use this to access the singleton instance of DearVRManager
        /// </summary>
        /// <value>The instance.</value>
        public static DearVRManager Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = FindObjectOfType<DearVRManager>();
                    instance_.AwakeSingleton();
                }

                return instance_;
            }
        }

        void Awake()
        {

            if (instance_ == null)
            {
                instance_ = this;
                AwakeSingleton();
            }
            else
            {
                if (this != instance_)
                {
                    Destroy(gameObject);
                }
            }
        }

        void AwakeSingleton()
        {
            if (DearListener == null)
            {
                if (FindObjectOfType<AudioListener>() != null)
                {
                    DearListener = FindObjectOfType<AudioListener>();
                }
                else
                {
                    Debug.LogWarning("DEARVR: No AudioListener found in scene!");
                }
            }

            if (Application.platform == RuntimePlatform.Android)
            {
                string pathToData;

                List<string> filesNeeded = new List<string>();
                filesNeeded.Add("DVRdata120.dat");
                filesNeeded.Add("DVRdata121.dat");
                filesNeeded.Add("DVRdata122.dat");
                filesNeeded.Add("DVRdata510.dat");
                filesNeeded.Add("DVRdata511.dat");
                filesNeeded.Add("DVRdata512.dat");

                // On Android, we need to unpack the StreamingAssets from the .jar file in which
                // they're archived into the native file system.
                // Set "filesNeeded" to a list of files you want unpacked.
                pathToData = Application.temporaryCachePath + "/";

                foreach (string basename in filesNeeded)
                {
                    if (!File.Exists(pathToData + basename))
                    {
                        WWW unpackerWww =
                            new WWW("jar:file://" + Application.dataPath + "!/assets/dearVR/data/" + basename);
                        while (!unpackerWww.isDone)
                        {
                        } // This will block in the webplayer.

                        if (!string.IsNullOrEmpty(unpackerWww.error))
                        {
                            pathToData = "";
                            break;
                        }

                        File.WriteAllBytes(pathToData + basename,
                            unpackerWww.bytes); // 64MB limit on File.WriteAllBytes.
                    }
                }

                DearVRManagerState.Instance.AudioPlugin.DearVRSetIRDataPath(pathToData);
            }
            else
            {

                string pathToData = Application.streamingAssetsPath;
                DearVRManagerState.Instance.AudioPlugin.DearVRSetIRDataPath(pathToData + "/dearVR/data");

            }
        }

        #endregion

        void Start()
        {
            var errorCode = DearVRManagerState.Instance.AudioPlugin.DearVRGetErrorCode();
            if (errorCode == 0)
                return;

            switch (errorCode)
            {
                case 1:
                    Debug.LogError("DEARVR: No Hrir files found in StreamingAssets folder. Reinstall the package.");
                    break;

            }
        }

        void Update()
        {
            UpdateRoom();
        }

        void UpdateRoom()
        {
            if (DearVRManagerState.Instance.RoomAnalyzer)
            {
                roomUpdateTime_ += Time.deltaTime;

                if (roomUpdateTime_ >= DearVRManagerState.Instance.RoomUpdateFreq / 6.0f)
                {
                    roomUpdateTime_ -= DearVRManagerState.Instance.RoomUpdateFreq / 6.0f;
                    AnalyzeRoom();
                    Instance.roomUpdateSide_++;
                }

                if (Instance.roomUpdateSide_ >= 6 && DearVRManagerState.Instance.NeedRoomUpdate)
                {
                    DearVRManagerState.Instance.AudioPlugin.DearVRSetRoomDistances(
                        DearVRManagerState.Instance.UpDownGeo.x, DearVRManagerState.Instance.UpDownGeo.y,
                        DearVRManagerState.Instance.FrontBackGeo.x, DearVRManagerState.Instance.FrontBackGeo.y,
                        DearVRManagerState.Instance.LeftRightGeo.x, DearVRManagerState.Instance.LeftRightGeo.y);
                    Instance.roomUpdateTime_ = 0.0f;
                    Instance.roomUpdateSide_ = 0;
                    DearVRManagerState.Instance.NeedRoomUpdate = false;
                }
            }

            if (!DearVRManagerState.Instance.SetRoomGeo || !DearVRManagerState.Instance.NeedRoomUpdate) return;

            DearVRManagerState.Instance.AudioPlugin.DearVRSetRoomDistances(DearVRManagerState.Instance.UpDownGeo.x,
                DearVRManagerState.Instance.UpDownGeo.y, DearVRManagerState.Instance.FrontBackGeo.x,
                DearVRManagerState.Instance.FrontBackGeo.y, DearVRManagerState.Instance.LeftRightGeo.x,
                DearVRManagerState.Instance.LeftRightGeo.y);

            DearVRManagerState.Instance.NeedRoomUpdate = false;
        }

        void AnalyzeRoom()
        {
            RaycastHit hitRoom;

            var distanceTemp = Vector2.zero;

            switch (roomUpdateSide_)
            {
                case 0:

                    if (Physics.Raycast(managerListener_.transform.position, managerListener_.transform.forward,
                        out hitRoom, DearVRManagerState.Instance.MaxRoom, DearVRManagerState.Instance.RoomMask.value))
                    {
                        distanceTemp.x = hitRoom.distance;
                        if (DearVRManagerState.Instance.DebugRoomAnalyzer)
                        {
                            Debug.DrawRay(managerListener_.transform.position,
                                hitRoom.point - managerListener_.transform.position, Color.blue,
                                DearVRManagerState.Instance.RoomUpdateFreq, true);
                        }
                    }
                    else
                    {
                        distanceTemp.x = -1.0f;
                    }

                    distanceTemp.y = DearVRManagerState.Instance.FrontBackGeo.y;
                    DearVRManagerState.Instance.FrontBackGeo = distanceTemp;
                    break;

                case 1:
                    if (Physics.Raycast(managerListener_.transform.position, -managerListener_.transform.forward,
                        out hitRoom, DearVRManagerState.Instance.MaxRoom, DearVRManagerState.Instance.RoomMask.value))
                    {
                        distanceTemp.y = hitRoom.distance;
                        if (DearVRManagerState.Instance.DebugRoomAnalyzer)
                        {
                            Debug.DrawRay(managerListener_.transform.position,
                                hitRoom.point - managerListener_.transform.position, Color.blue,
                                DearVRManagerState.Instance.RoomUpdateFreq, true);
                        }
                    }
                    else
                    {
                        distanceTemp.y = -1.0f;
                    }

                    distanceTemp.x = DearVRManagerState.Instance.FrontBackGeo.x;
                    DearVRManagerState.Instance.FrontBackGeo = distanceTemp;
                    break;

                case 2:
                    if (Physics.Raycast(managerListener_.transform.position, managerListener_.transform.up, out hitRoom,
                        DearVRManagerState.Instance.MaxRoom, DearVRManagerState.Instance.RoomMask.value))
                    {
                        distanceTemp.x = hitRoom.distance;
                        if (DearVRManagerState.Instance.DebugRoomAnalyzer)
                        {
                            Debug.DrawRay(managerListener_.transform.position,
                                hitRoom.point - managerListener_.transform.position, Color.green,
                                DearVRManagerState.Instance.RoomUpdateFreq, true);
                        }
                    }
                    else
                    {
                        distanceTemp.x = -1.0f;
                    }

                    distanceTemp.y = DearVRManagerState.Instance.UpDownGeo.y;
                    DearVRManagerState.Instance.UpDownGeo = distanceTemp;
                    break;

                case 3:
                    if (Physics.Raycast(managerListener_.transform.position, -managerListener_.transform.up,
                        out hitRoom,
                        DearVRManagerState.Instance.MaxRoom, DearVRManagerState.Instance.RoomMask.value))
                    {
                        distanceTemp.y = hitRoom.distance;
                        if (DearVRManagerState.Instance.DebugRoomAnalyzer)
                        {
                            Debug.DrawRay(managerListener_.transform.position,
                                hitRoom.point - managerListener_.transform.position, Color.green,
                                DearVRManagerState.Instance.RoomUpdateFreq, true);
                        }
                    }
                    else
                    {
                        distanceTemp.y = -1.0f;
                    }

                    distanceTemp.x = DearVRManagerState.Instance.UpDownGeo.x;
                    DearVRManagerState.Instance.UpDownGeo = distanceTemp;
                    break;

                case 4:
                    if (Physics.Raycast(managerListener_.transform.position, -managerListener_.transform.right,
                        out hitRoom, DearVRManagerState.Instance.MaxRoom, DearVRManagerState.Instance.RoomMask.value))
                    {
                        distanceTemp.x = hitRoom.distance;
                        if (DearVRManagerState.Instance.DebugRoomAnalyzer)
                        {
                            Debug.DrawRay(managerListener_.transform.position,
                                hitRoom.point - managerListener_.transform.position, Color.red,
                                DearVRManagerState.Instance.RoomUpdateFreq, true);
                        }
                    }
                    else
                    {
                        distanceTemp.x = -1.0f;
                    }

                    distanceTemp.y = DearVRManagerState.Instance.LeftRightGeo.y;
                    DearVRManagerState.Instance.LeftRightGeo = distanceTemp;
                    break;

                case 5:
                    if (Physics.Raycast(managerListener_.transform.position, managerListener_.transform.right,
                        out hitRoom, DearVRManagerState.Instance.MaxRoom, DearVRManagerState.Instance.RoomMask.value))
                    {
                        distanceTemp.y = hitRoom.distance;
                        if (DearVRManagerState.Instance.DebugRoomAnalyzer)
                        {
                            Debug.DrawRay(managerListener_.transform.position,
                                hitRoom.point - managerListener_.transform.position, Color.red,
                                DearVRManagerState.Instance.RoomUpdateFreq, true);
                        }
                    }
                    else
                    {
                        distanceTemp.y = -1.0f;
                    }

                    distanceTemp.x = DearVRManagerState.Instance.LeftRightGeo.x;
                    DearVRManagerState.Instance.LeftRightGeo = distanceTemp;
                    break;

                default:
                    break;
            }
        }
    }
}
