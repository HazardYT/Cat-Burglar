using UnityEngine;
using System.Collections.Generic;
public class RatFootstepManager : MonoBehaviour
{
    public AudioSource audioSource;
    public List<AudioClip> Footsteps = new List<AudioClip>();

    public void PlayFootstep()
    {
        int randomIndex = Random.Range(0, Footsteps.Count);
        audioSource.clip = Footsteps[randomIndex];
        audioSource.pitch = Random.Range(0.9f, 1.1f);
        audioSource.Play();
    }

}