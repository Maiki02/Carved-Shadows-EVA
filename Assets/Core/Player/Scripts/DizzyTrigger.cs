using UnityEngine;

public class DizzyTrigger : MonoBehaviour
{
    [Header("Mareo Config")]
    public float duration = 5f;
    [Range(0f, 1f)]
    public float intensity = 0.8f; // 0 a 1

    private void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            player.TriggerDizziness(duration, intensity);
        }
    }
}
