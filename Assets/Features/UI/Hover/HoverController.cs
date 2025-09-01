using UnityEngine;

public class HoverController : MonoBehaviour
{
    [Header("Hover Audio")]
    [SerializeField] private AudioClip hoverClip;

    public void PlayHoverSound()
    {
        if (hoverClip != null && AudioController.Instance != null)
        {
            AudioController.Instance.PlaySFXClip(hoverClip);
        }
    }
}