using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawningSoundObject : MonoBehaviour {

    public Coroutine spawn(AudioClip clip, List<AudioClip> additionalParts = null) {
        GameObject voiceObj = Instantiate(this.gameObject, transform);
        voiceObj.transform.parent = null;
        return voiceObj.GetComponent<SpawningSoundObject>().playAndDestroy(clip, additionalParts);
    }

    private Coroutine playAndDestroy(AudioClip clip, List<AudioClip> additionalParts) {
        AudioSource audioSource = GetComponent<AudioSource>();
        audioSource.clip = clip;
        if (clip != null) {
            audioSource.Play();
        }
        return StartCoroutine(destroyWhenDone(additionalParts));
    }

    private IEnumerator destroyWhenDone(List<AudioClip> additionalParts) {
        AudioSource audioSource = GetComponent<AudioSource>();
        yield return new WaitForSeconds(audioSource.clip.length + 0.01f);
        while (audioSource.isPlaying) {
            yield return new WaitForSeconds(0.2f);
        }
        if (additionalParts != null && additionalParts.Count > 0) {
            AudioClip nextInQueue = additionalParts[0];
            additionalParts.RemoveAt(0);
            audioSource.clip = nextInQueue;
            audioSource.Play();
            yield return destroyWhenDone(additionalParts);
        } else {
            Destroy(this.gameObject);
        }
    }
}
