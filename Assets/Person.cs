using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Person : MonoBehaviour, IPubSub {

    private static int PERSON_ID = 0;

    private int id;
    private PersonBagDefinition bagDefinition = new PersonBagDefinition();
    public List<BagContentProperties> toBePlacedInTrays = new List<BagContentProperties>();
    private float currentX;

    public SpawningSoundObject soundObject;
    private string voice;
    private bool haveSaidGreeting = false;
    private AudioClip greeting;
    public float greetingPositionX;
    public WalkingMan walkingMan;

    private string worstMistake = "none";
    private static List<string> WORST_MISTAKES = new List<string>(){
        "gun", "knife", "drugs", "warning", "none"
    };

    private List<AudioClip> clips;

    const float PASSPORT_OFFSET_X =  1.5f;
    private bool showingPassport;
    public Passport passportPrefab;

    [HideInInspector]
    public Passport passport;

    void Awake() {
        id = ++PERSON_ID;
        Debug.Log("PERSON CREATED");
        // Decide person charachteristics
        voice = "robot1"; // TODO - Decide voice

        // Load information on audio clips
        clips = Resources.LoadAll<AudioClip>("voice/" + voice).ToList();
        // Greeting
        // Some people don't say greeting - in those cases, pretend we've already greeted
//        haveSaidGreeting = Misc.randomBool(); // TODO
        haveSaidGreeting = false;
    }

	// Use this for initialization
	void Start () {
        // Choose greeting
        greeting = Misc.pickRandom(clips.FindAll(i => i.name.StartsWith("greetings-")));

        bagDefinition.person = this;
        currentX = this.transform.position.x;
        walkingMan.reportPositionX(currentX);

        PubSub.subscribe("belt_movement", this);

        // Subscribe to inspection on items in own bags
        PubSub.subscribe("bag_inspect_item", this);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void startPlaceBags(BagHandler bagHandler, Vector3 bagDropPosition) {
        // TODO - each instantiated item should have a ruleset on if it prefers tray or not, and if max one can be instantiated per person...
        StartCoroutine(startPlaceBagsCoroutine(bagHandler, bagDropPosition));

        if (toBePlacedInTrays.Count > 0) {
            // TODO - Create tray with items (after delay)
//            BagHandler.instance.createTrayWithContents(Game.instance.getTrayDropPosition(), manualInspectItems);
        }
    }

    public IEnumerator startPlaceBagsCoroutine(BagHandler bagHandler, Vector3 bagDropPosition) {
        yield return bagHandler.packBagAndDropIt(bagDropPosition, bagDefinition, toBePlacedInTrays, id);
    }

    private Coroutine playVoice(AudioClip clip) {
        return soundObject.spawn(clip);
    }

    private void finishPerson() {
        PubSub.unsubscribe("belt_movement", this);
        PubSub.unsubscribe("bag_inspect_item", this);
        // TODO - Calculate points and consequences
        Destroy(this.gameObject);
    }

    private void setTimerAndReact(BagContentProperties item) {
        Coroutine itemReaction = StartCoroutine(reactOnItemInspection(item));
        Misc.SetCoroutine("inspect-reaction-" + item.id, itemReaction);
    }

    private void stopTimerAndPossiblyReact(InspectUIButton.INSPECT_TYPE action, BagContentProperties item) {
        Misc.StopCoroutine("inspect-reaction-" + item.id);
        reactOnInspectAction(action, item);
    }

    private IEnumerator reactOnItemInspection(BagContentProperties item) {
        // Get applicable audio clips
        string coroutineKey = "inspect-reaction-" + item.id;
        List<AudioClip> itemAudioClips = clips.FindAll(i => i.name.Contains(item.category + "-"));

        yield return new WaitForSeconds(Misc.randomRange(4f, 8f));
        while (Misc.HasCoroutine(coroutineKey)) {
            if (Misc.randomRange(0, 100) < 25) {
                yield return playVoice(Misc.pickRandom(itemAudioClips));
            }
            yield return new WaitForSeconds(Misc.randomRange(3f, 5f));
        }
    }

    private IEnumerator reactOnAction(string action) {
        yield return new WaitForSeconds(Misc.randomRange(0.5f, 1.5f));
        List<AudioClip> reactionAudioClips = clips.FindAll(i => i.name.Contains(action + "-"));
        yield return playVoice(Misc.pickRandom(reactionAudioClips));
    }

    private void reactOnInspectAction(InspectUIButton.INSPECT_TYPE action, BagContentProperties item) {
        if (action == InspectUIButton.INSPECT_TYPE.MANUAL_INSPECT || action == InspectUIButton.INSPECT_TYPE.MANUAL_INSPECT_NEW) {
            // TODO - Maybe don't react on 50% (ajdust level)
            if (Misc.randomBool()) {
                StartCoroutine(reactOnAction("manual"));
            }
        } else if (action == InspectUIButton.INSPECT_TYPE.TRASHCAN) {
            StartCoroutine(reactOnAction("throw"));
        } else if (action == InspectUIButton.INSPECT_TYPE.POLICE) {
            StartCoroutine(reactOnAction("police"));
        }
    }

    public PROPAGATION onMessage(string message, object data) {
        if (message == "belt_movement") {
            if (bagDefinition.hasInitiated()) {
                BagProperties leftmostBag = bagDefinition.getLeftmostBag();
                if (leftmostBag != null) {
                    float leftMostBagPositionX = leftmostBag.gameObject.transform.position.x;
                    if (currentX < leftMostBagPositionX) {
                        currentX = leftMostBagPositionX;
                        walkingMan.reportPositionX(currentX);
                        transform.position = new Vector3(leftMostBagPositionX, transform.position.y, transform.position.z);

                        // Check if person should show passport
                        BagProperties rightmostBag = bagDefinition.getRightmostBag();
                        GameObject triggerCube = rightmostBag.contentsTriggerCube;
                        float centerOfTriggerCube = rightmostBag.transform.position.x + triggerCube.transform.localPosition.x;
                        float bagRightmostPos = centerOfTriggerCube + triggerCube.transform.localScale.x / 2f;
                        float bagLeftmostPos = centerOfTriggerCube - triggerCube.transform.localScale.x / 2f;
                        if (bagRightmostPos >= Game.instance.currentXrayMachine.scanLeft && bagLeftmostPos <= Game.instance.currentXrayMachine.scanRight) {
                            if (!showingPassport) {
                                showPassport();
                            }
                        } else if (showingPassport) {
                            showPassport(false);
                        }

                        // Check if we move past a certain point, to say our greeting
                        if (currentX > greetingPositionX && !haveSaidGreeting) {
                            haveSaidGreeting = true;
                            playVoice(greeting);
                        }
                    }
                } else {
                    finishPerson();
                }
            }
        } else if (message == "bag_inspect_item") {
            InspectActionBag bagInfo = (InspectActionBag) data;
            bool isOurBag = bagDefinition.bags.Find(i => i.id == bagInfo.bagId) != null;
            if (isOurBag) {
                if (bagInfo.action == InspectUIButton.INSPECT_TYPE.UNDEFINED) {
                    setTimerAndReact(bagInfo.item);
                } else {
                    stopTimerAndPossiblyReact(bagInfo.action, bagInfo.item);
                }
            }
            // bagID
            // start/stop
            // throw away/ok/manual_inspect/police
        }
        return default(PROPAGATION);
    }

    public void registerWrongAction(BagContentProperties item) {
        if (Array.IndexOf(item.acceptableActions, item.actionTaken) == -1) {
            string newMistake = "warning";
            if (item.acceptableActions.Contains(InspectUIButton.INSPECT_TYPE.POLICE)) {
                newMistake = item.category;
            }
            if (WORST_MISTAKES.IndexOf(worstMistake) > WORST_MISTAKES.IndexOf(newMistake)) {
                worstMistake = newMistake;
            }
        }
    }

    public string getWorstMistake() {
        return worstMistake;
    }

    private void showPassport(bool show = true) {
        showingPassport = show;
        float passportMovementY = 1.63f;
        if (show && passport == null) {
            passport = Instantiate(passportPrefab);
            Vector3 passportTargetPosition = new Vector3(passport.transform.localPosition.x - PASSPORT_OFFSET_X * (passport.id % 3), passport.transform.localPosition.y + passportMovementY, passport.transform.localPosition.z);
            Quaternion passportTargetRotation = passport.transform.localRotation;
            Misc.AnimateMovementTo("person_passport_show_" + id, passport.gameObject, passportTargetPosition);
            passport.originalPosition = passportTargetPosition;
            passport.originalRotation = passportTargetRotation;
            if (!(Game.instance.cameraXPos == 1 && !Game.instance.zoomedOutState)) {
                passport.startInactive = true;
            }
        } else {
            Vector3 passportTargetPosition = new Vector3(passport.transform.localPosition.x, passport.transform.localPosition.y - passportMovementY, passport.transform.localPosition.z);
            Misc.AnimateMovementTo("person_passport_hide_" + id, passport.gameObject, passportTargetPosition);
            Misc.SetActiveAfterDelay("person_passport_active_" + id, passport.gameObject, false, true);
        }
    }
}
