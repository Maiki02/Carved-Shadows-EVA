using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeatherTrunk : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private AudioSource audioSource;

    public void Open()
    {
        if (animator != null)
        {
            animator.SetBool("isOpen", true);
        }
        if (audioSource != null)
        {
            audioSource.Play();
        }
    }

    public void Close()
    {
        if (animator != null)
        {
            animator.SetBool("isOpen", false);
        }
        if (audioSource != null)
        {
            audioSource.Play();
        }
    }
    
}
