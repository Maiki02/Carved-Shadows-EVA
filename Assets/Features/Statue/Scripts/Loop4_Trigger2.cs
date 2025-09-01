using UnityEngine;

public class Loop4_Trigger2 : MonoBehaviour
{
    [SerializeField] private Statue statue;
    [SerializeField] private AudioSource ceramicaCrash;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player has triggered Loop4_Trigger2");
            Debug.Log("Ceramica crash sound played");

            statue.TriggerNextStep();

            ceramicaCrash.Play();
            gameObject.SetActive(false);
        }
    }
}