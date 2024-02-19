using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public AudioSource audioPlayer;

    public void PlaySound()
    {
        audioPlayer.Play();
    }
}
