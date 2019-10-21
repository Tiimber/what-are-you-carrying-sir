using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class LoudspeakerLogic : MonoBehaviour {
    private string voice;
    private List<AudioClip> clips;

    private List<Tuple2<float, AudioClip>> queue = new List<Tuple2<float, AudioClip>>();

    private string GENERIC_ANNOUNCEMENT_PREFIX = "generic-announcement-";
    const float MIN_SECONDS_BETWEEN_LOUDSPEAKER_ANNOUNCEMENTS = 40f;
    const float MAX_SECONDS_BETWEEN_LOUDSPEAKER_ANNOUNCEMENTS = 80f;

    public SpawningSoundObject loudspeaker;

    private Dictionary<string, List<AudioClip>> memory = new Dictionary<string, List<AudioClip>>();

    // Start is called before the first frame update
    void Start() {
        // Decide announcer voice
        voice = "voice1"; // TODO - Decide voice

        // Load information on audio clips
        clips = Resources.LoadAll<AudioClip>("voice/announcements/" + voice).ToList();
        addGenericAnnouncementAfterDelay();

        StartCoroutine(processQueue());
    }

    public IEnumerator processQueue () {
        while (true) { // TODO - Check "game running" flag
            if (queue.Count == 0) {
                addGenericAnnouncementAfterDelay();
            }

            Tuple2<float, AudioClip> firstInQueue = queue.First<Tuple2<float, AudioClip>>();
            float waitTimeForFirstInQueue = firstInQueue.First;
            float iterationWaitTime = Mathf.Min(waitTimeForFirstInQueue, 1f);
            if (iterationWaitTime > 0) {
//                Debug.Log(firstInQueue.First + "s : " + firstInQueue.Second.name);
                firstInQueue.First -= iterationWaitTime;
                yield return new WaitForSeconds(iterationWaitTime);
            } else {
                // Play audio
                queue.Remove(firstInQueue);
                List<AudioClip> otherParts = getAdditionalPartsForAudioClip(firstInQueue.Second);
                yield return loudspeaker.spawn(firstInQueue.Second, otherParts);
                clearMemory();
            }
        }
    }

    public void putMessageOnQueue(string type, int severity, float delay = 0f) {
        Debug.Log("Play sound: " + type);
        AudioClip warningMessage = ItsRandom.pickRandom(clips.FindAll(i => i.name.Contains(type + "-") && i.name.Contains("-severity-" + severity)));
        Debug.Log("SOUND: " + warningMessage);
        if (warningMessage != null) {
            putMessageOnQueue(warningMessage, delay);
        } else {
            Debug.LogError("Audio not found for " + type + ", severity " + severity);
        }
    }

    public void putMessageOnQueue (AudioClip audioClip, float delay = 0f) {
        // Is the clip already in queue a "generic announcement"?
        Debug.Log("Queue length: " + queue.Count);
        if (queue.Count > 0) {
            Tuple2<float, AudioClip> firstInQueue = queue.First<Tuple2<float, AudioClip>>();
            Debug.Log("First announcement in queue: " + firstInQueue.Second.name);
            if (firstInQueue.Second.name.StartsWith(GENERIC_ANNOUNCEMENT_PREFIX)) {
                queue.Remove(firstInQueue);
                Debug.Log("Removed generic announcement from queue!");
            }
        }
        Debug.Log("Adding announcement: " + audioClip.name);
        queue.Insert(0, new Tuple2<float, AudioClip>(delay, audioClip));
    }

    private void addGenericAnnouncementAfterDelay() {
        queue.Add(new Tuple2<float, AudioClip>(ItsRandom.randomRange(MIN_SECONDS_BETWEEN_LOUDSPEAKER_ANNOUNCEMENTS, MAX_SECONDS_BETWEEN_LOUDSPEAKER_ANNOUNCEMENTS), ItsRandom.pickRandom(clips.FindAll(i => i.name.StartsWith(GENERIC_ANNOUNCEMENT_PREFIX)))));
    }

    private List<AudioClip> getAdditionalPartsForAudioClip(AudioClip firstClip) {
        return getAdditionalPartsForAudioClip(firstClip.name);
    }
    private List<AudioClip> getAdditionalPartsForAudioClip(string clipName) {
        List<AudioClip> chosenAdditionalParts = new List<AudioClip>();
        string additionalClipNames = "voice/announcements/" + voice + "/" + clipName + "/";
        Debug.Log("Additional clips path: " + additionalClipNames);
        List<AudioClip> additionalParts = Resources.LoadAll<AudioClip>(additionalClipNames).ToList();

        int part = 2;
        bool foundMoreParts = true;
        while (foundMoreParts) {
            string partStr = part < 10 ? "0" + part : "" + part;
            List<AudioClip> matchingParts = additionalParts.FindAll(i => i.name.StartsWith("part-" + partStr));
            foundMoreParts = matchingParts.Count > 0;
            if (foundMoreParts) {
                AudioClip random = ItsRandom.pickRandom(matchingParts);
                if (new Regex(@"^part-" + partStr + @"_\d+").IsMatch(random.name)) {
                    chosenAdditionalParts.Add(random);
                } else {
                    List<AudioClip> subAudioClips = null;

                    Regex re = new Regex(@"^part-" + partStr + @"_(.*?)(:?_(.+))?$");
                    Match reMatch = re.Match(random.name);
                    GroupCollection groups = reMatch.Groups;
                    string subFolder = groups[1].ToString();

                    string memoryKey = "";
                    if (groups.Count > 2) {
                        string memorySlot = groups[2].ToString();
                        if (memorySlot != "") {
                            memoryKey = subFolder + "_" + memorySlot;
                            if (memory.ContainsKey(memoryKey)) {
                                subAudioClips = memory[memoryKey];
                            }
                        }
                    }
                    if (subAudioClips == null) {
                        subAudioClips = getAdditionalPartsForAudioClip(subFolder);
                        if (memoryKey != "") {
                            memory.Add(memoryKey, subAudioClips);
                        }
                    }
                    chosenAdditionalParts.AddRange(subAudioClips);
                }
            }
            part++;
        }

        Debug.Log("Additional parts: " + chosenAdditionalParts.Count);

        return chosenAdditionalParts;
    }

    private void clearMemory() {
        memory.Clear();
    }
}
