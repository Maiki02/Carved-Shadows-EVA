using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnapshotSelector : MonoBehaviour {

#pragma warning disable 649

	[SerializeField] UnityEngine.Audio.AudioMixerSnapshot audioMixerSnapShot1;
	[SerializeField] UnityEngine.Audio.AudioMixerSnapshot audioMixerSnapShot2;

#pragma warning restore 649

	public void Select(int snapshot) {
		if (snapshot == 1) {
			audioMixerSnapShot1.TransitionTo (1f);
		} else if (snapshot == 2) {
			audioMixerSnapShot2.TransitionTo (1f);
		}
	}
}
