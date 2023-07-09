using UnityEngine;
using System.Collections.Generic;
public class FootstepManager : MonoBehaviour
{
    public AudioSource audioSource;
    public List<AudioClip> Footsteps = new List<AudioClip>();
    public float walkingVolume = 0.3f;
    public float runningVolume = 0.5f;

    public void PlayFootstep(bool running)
    {
        int randomIndex = Random.Range(0,Footsteps.Count);
        audioSource.clip = Footsteps[randomIndex];
        if (!running)
        {
            audioSource.volume = walkingVolume;
        }
        else if (running)
        {
            audioSource.volume = runningVolume;
        }
        audioSource.pitch = Random.Range(0.9f, 1.1f);
        audioSource.Play();
    }

}