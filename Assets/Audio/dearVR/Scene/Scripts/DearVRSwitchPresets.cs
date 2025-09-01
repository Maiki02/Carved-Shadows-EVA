using UnityEngine;
using System.Collections;
using DearVR;

public class DearVRSwitchPresets : MonoBehaviour {

	[SerializeField] int RoomIndex = 4;

	DearVRSource dearVRInstance;

	[SerializeField] UnityEngine.UI.Text presetLabel = null;

	void Start () {
		
		Debug.Log("DEARVR Demo Scene: press R or T to switch virtual acoustic presets");

		dearVRInstance = GetComponent<DearVRSource>();

		RoomIndex = (int)dearVRInstance.RoomPreset;

	}

	public void NextPreset() {

		if (RoomIndex >= (int)DearVRSource.RoomList.String_Plate) {

			RoomIndex = 0;

		} else {

			RoomIndex++;

		}

		dearVRInstance.RoomPreset = (DearVRSource.RoomList)(RoomIndex);

		if (presetLabel != null) {
			presetLabel.text = dearVRInstance.RoomPreset.ToString ();
		}

	}

	public void PrevPreset() {

		if (RoomIndex <= 0) {

			RoomIndex = (int)DearVRSource.RoomList.String_Plate;

		} else {

			RoomIndex--;	

		}

		dearVRInstance.RoomPreset = (DearVRSource.RoomList)(RoomIndex);

		if (presetLabel != null) {
			presetLabel.text = dearVRInstance.RoomPreset.ToString ();
		}
	}



	void Update () {


		if(Input.GetKeyDown((KeyCode.T))) {
			NextPreset ();
		}

		if(Input.GetKeyDown((KeyCode.R))) {
			PrevPreset ();
		}

	}
		
}
