using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Video;

[System.SerializableAttribute]
public class PlayVideoInspectAction : MonoBehaviour, ActionOnInspect {

    public VideoPlayer videoPlayer;
    public VideoClip[] videoClips;
    private bool chosenVideoClip = false;
    private bool running = false;

    AudioSource audioSource;

	public void run(bool reverse) {
        if (!reverse) {
            videoPlayer.playOnAwake = true;
            if (!chosenVideoClip) {

                // Duplicate the material of the screen
                GameObject videoPlayerGameObject = videoPlayer.gameObject;
                Renderer componentRenderer = videoPlayerGameObject.GetComponent<Renderer>();
                Material copyOfMaterial = new Material(componentRenderer.material);
                RenderTexture copyOfTexture = new RenderTexture((RenderTexture)copyOfMaterial.mainTexture);
                copyOfMaterial.mainTexture = copyOfTexture;
                componentRenderer.material = copyOfMaterial;
                videoPlayer.targetTexture = copyOfTexture;

                // Create audio source to play sound through
                audioSource = videoPlayer.gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = true;

                audioSource.volume = 1f;
                audioSource.spatialBlend = 1f;

                //Set Audio Output to AudioSource
                videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;

                //Assign the Audio from Video to AudioSource to be played
                videoPlayer.EnableAudioTrack(0, true);
                videoPlayer.SetTargetAudioSource(0, audioSource);
                videoPlayer.controlledAudioTrackCount = 1;

                // Pick video clip
                VideoClip videoClip = Misc.pickRandom(videoClips.ToList());
                videoPlayer.clip = videoClip;
                chosenVideoClip = true;
            }

            running = true;
            StartCoroutine(playWhenReady());
        } else {
            running = false;
            videoPlayer.Stop();
            audioSource.Stop();
        }
	}

    public IEnumerator playWhenReady() {
        videoPlayer.Prepare();

        while (!videoPlayer.isPrepared && running)
        {
            yield return null;
        }

        if (running) {
            videoPlayer.Play();
            audioSource.Play();
            running = false;
            videoPlayer.playOnAwake = false;
        }
    }

}
