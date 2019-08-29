using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayRandomAudioPosition : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake() {
        AudioSource audioSource = GetComponent<AudioSource>();
        audioSource.time = Misc.randomRange(0, audioSource.clip.length - 1);
        Debug.Log(audioSource.time);
        Debug.Log(audioSource.clip.length);
    }

}
