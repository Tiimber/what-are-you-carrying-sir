using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConveyorObject : MonoBehaviour, IPubSub {

	public bool isMainMachine;
	public AudioSource conveyorSound;
	public float pitchForward;
	public float pitchBackward;
	private bool conveyorSoundDirectionIsForward = false;

	// Use this for initialization
	void Start () {
		if (isMainMachine) {
			PubSub.subscribe("belt_movement", this);
			PubSub.subscribe("belt_stop", this);
			conveyorSound.clip.LoadAudioData();
		}
	}

	private void stopConveyorSound() {
		conveyorSoundDirectionIsForward = false;
		conveyorSound.Stop();
	}

	private void playConveyorSound(bool forward) {
		if (forward && (!conveyorSound.isPlaying || !conveyorSoundDirectionIsForward)) {
			// Forward sound
			conveyorSoundDirectionIsForward = true;
			conveyorSound.pitch = pitchForward;
			conveyorSound.Play();
		} else if (!forward && (!conveyorSound.isPlaying || conveyorSoundDirectionIsForward)) {
			// Back sound
			conveyorSoundDirectionIsForward = false;
			conveyorSound.pitch = pitchBackward;
			conveyorSound.Play();
		}
	}
	
	public PROPAGATION onMessage(string message, object data) {
		if (message == "belt_movement") {
			float movement = (float) data;
			if (movement != 0f) {
				playConveyorSound(movement > 0f);
			}
		} else if (message == "belt_stop") {
			stopConveyorSound();
		}

		return PROPAGATION.DEFAULT;
	}
}
