using UnityEngine;
using System.Collections;
using UnityEngine.Audio;
using DearVR;

public class DearVRScriptLoad : MonoBehaviour {

	[SerializeField] bool internalReverb = false;

	AudioSource myAudioSource;

	DearVRSource myDearVRSource;

#pragma warning disable 649
	
	// Assign in Inspector or in script
	[SerializeField] AudioClip myAudioClip;

	[SerializeField] DearVRSource.RoomList roomSelection;

	[SerializeField] DearVRSerializedReverb[] reverbSendList;

	[SerializeField] AudioMixerGroup audioMixer;

	[SerializeField] bool performanceMode = false;

	[SerializeField] bool loop = false;

	[SerializeField] AudioClip clipForOneShot;

#pragma warning restore 649

	void Awake () {

		// Create dearVR-Instance
		myDearVRSource = gameObject.AddComponent<DearVRSource>();

		myDearVRSource.PerformanceMode = performanceMode;

		// Assign and set AudioSource
		myAudioSource = myDearVRSource.currentAudioSource;

		myAudioSource.loop = loop;
		
		if (performanceMode) {
			myAudioSource.playOnAwake = false;
		}
		// Select Room Preset
		myDearVRSource.RoomPreset = roomSelection;

		// set audiomixer
		myAudioSource.outputAudioMixerGroup = audioMixer;

		myDearVRSource.InternalReverb = internalReverb;

		if (!internalReverb) {
			if (reverbSendList != null && reverbSendList.GetLength(0) > 0) {

				myDearVRSource.SetReverbSends(reverbSendList);

			}
		}

		// Set dearVR-Settings
		myDearVRSource.BassBoost = false;

		myDearVRSource.InputChannel = 1.0f;


		// Assign AudioClip
		if (myAudioClip) {

			myAudioSource.clip = myAudioClip;

		} else {

			Debug.LogWarning("DEARVR: AudioClip not assigned!");

		}
	}

	public void PlayStop(bool shouldPlay) {
		if (gameObject.activeSelf) {
			if (shouldPlay) {
				DearVRPlay ();
			} else {
				DearVRStop ();
			}
		}
	}

	void DearVRPlay() {
		if (performanceMode) {
			myDearVRSource.DearVRPlay();

		} else {
			myAudioSource.Play ();

		}
	}

	public void DearVRPlayOneShot() {
		if (performanceMode) {
			myDearVRSource.currentAudioSource.loop = false;
			myDearVRSource.DearVRPlayOneShot(clipForOneShot);

		}
	}

	void DearVRStop() {
		if (performanceMode) {
			myDearVRSource.DearVRStop();

		} else {
			myAudioSource.Stop ();

		}
	}
		
	public void Deactivate()
	{
		gameObject.SetActive(false);
	}

	public void Activate()
	{
		gameObject.SetActive(true);
	}
		

	public void PlayScript()
	{
		if (myAudioSource)
		{
			myAudioSource.Play();   
		}
	}
		
}
