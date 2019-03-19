using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawningSoundObject : MonoBehaviour {

    public Coroutine spawn(AudioClip clip) {
        GameObject voiceObj = Instantiate(this.gameObject, transform);
        voiceObj.transform.parent = null;
        return voiceObj.GetComponent<SpawningSoundObject>().playAndDestroy(clip);
    }

    private Coroutine playAndDestroy(AudioClip clip) {
        AudioSource audioSource = GetComponent<AudioSource>();
        audioSource.clip = clip;
        if (clip != null) {
            audioSource.Play();
        }
        return StartCoroutine(destroyWhenDone());
    }

    private IEnumerator destroyWhenDone() {
        AudioSource audioSource = GetComponent<AudioSource>();
        yield return new WaitForSeconds(audioSource.clip.length);
        while (audioSource.isPlaying) {
            yield return new WaitForSeconds(1f);
        }
        Destroy(this.gameObject);
    }
}
