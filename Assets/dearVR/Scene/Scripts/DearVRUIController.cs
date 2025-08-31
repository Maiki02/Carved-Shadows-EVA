using UnityEngine;

public class DearVRUIController : MonoBehaviour
{
#pragma warning disable 649

	[SerializeField] private GameObject internalReverbObject;
	[SerializeField] private GameObject reverbSendsObject;
	[SerializeField] private GameObject performanceModeObject;

	[SerializeField] private CanvasGroup canvasGroupPlayStop;
	[SerializeField] private CanvasGroup canvasGroupPlay;
	[SerializeField] private CanvasGroup canvaspresetChange;
	[SerializeField] private CanvasGroup canvasGroupSnapshots;

#pragma warning restore 649

	public void SetInternalReverbObject(bool shouldBeActive)
	{
		internalReverbObject.SetActive(shouldBeActive);
		canvasGroupPlayStop.interactable = false;
		canvasGroupPlay.interactable = false;
		canvaspresetChange.interactable = true;
		canvasGroupSnapshots.interactable = false;
	}

	public void SetReverbSendsObject(bool shouldBeActive)
	{
		reverbSendsObject.SetActive(shouldBeActive);
		canvasGroupPlayStop.interactable = false;
		canvasGroupPlay.interactable = false;
		canvaspresetChange.interactable = false;
		canvasGroupSnapshots.interactable = true;
	}

	public void SetPerformanceModeObject(bool shouldBeActive)
	{
		performanceModeObject.SetActive(shouldBeActive);
		canvasGroupPlayStop.interactable = true;
		canvasGroupPlay.interactable = true;
		canvaspresetChange.interactable = false;
		canvasGroupSnapshots.interactable = false;
	}
}
