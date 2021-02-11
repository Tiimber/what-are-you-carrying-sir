using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEngine;

public class Person : MonoBehaviour, IPubSub {

    private static int PERSON_ID = 0;

    public PersonConfig config;

    private int id;
    private PersonBagDefinition bagDefinition = new PersonBagDefinition();
    public List<BagContentProperties> toBePlacedInTrays = new List<BagContentProperties>();
    public float currentX;

    public SpawningSoundObject soundObject;
    private Color bodyColor;
    private Color favouriteColor;
    private Color favouriteColor2;
    private Color chosenFavoriteColor;
    private string voice;
    private bool haveSaidGreeting = false;
    private AudioClip greeting;
    public float greetingPositionX;
    public WalkingMan walkingMan;
    public string personName;
    public string personUniqueId;
    public string nationality;
    public DateTime dateOfBirth;
    public string idPhrase;
    public Texture photo;
    public List<Tuple2<string, string>> books;

    private string worstMistake = "none";
    private static List<string> WORST_MISTAKES = new List<string>(){
        "false arrest", "gun", "knife", "drugs", "warning", "none"
    };

    private List<AudioClip> clips;

    const float PASSPORT_OFFSET_X =  1.5f;
    public bool showingPassport;
    public Passport passportPrefab;

    [HideInInspector]
    public Passport passport;

    public bool isDestroyed = false;

    private static bool HasInstantiatedPerson = false;
    private static Person LastPerson;

    void Awake() {
        id = ++PERSON_ID;
        Debug.Log("PERSON CREATED");
    }

    private void OnDestroy() {
        Debug.Log("Destroy person");
        Destroy(passport);
        isDestroyed = true;
        Game.instance.removePerson(this);
    }
    
    private float colorDistance(Color col1, Color col2) {
        long r1 = (long)(col1.r * 256);
        long r2 = (long)(col2.r * 256);
        long g1 = (long)(col1.g * 256);
        long g2 = (long)(col2.g * 256);
        long b1 = (long)(col1.b * 256);
        long b2 = (long)(col2.b * 256);
        
        long rmean = (r1 + r2) / 2;
        long r = r1 - r2;
        long g = g1 - g2;
        long b = b1 - b2;
        return Mathf.Sqrt(
            (((512 + rmean) * r * r) >> 8)
            + 4 * g * g + 
            (((767 - rmean) * b * b) >> 8)
        );
    }

    private Color pickFavouriteColorWithBiggestDistance(Color col1, Color col2, Color prevCol) {
        float distanceCol1 = colorDistance(col1, prevCol);
        float distanceCol2 = colorDistance(col2, prevCol);
        return distanceCol1 >= distanceCol2 ? col1 : col2;
    }
    
    public void setConfig(Tuple2<XmlDocument, Texture2D> personConfig) {
        config = new PersonConfig(personConfig.First, personConfig.Second);

        // Decide person characteristics
        personUniqueId = config.id;
        personName = config.name;
        nationality = config.nationality;
        dateOfBirth = config.dob;
        idPhrase = config.idPhrase;
        photo = config.photoTexture;
        voice = config.voice;
        bodyColor = config.bodyColor;
        favouriteColor = config.favouriteColor;
        favouriteColor2 = config.favouriteColor2;
        
        if (Person.HasInstantiatedPerson) {
            chosenFavoriteColor = pickFavouriteColorWithBiggestDistance(favouriteColor, favouriteColor2, Person.LastPerson.getFavouriteColor());
        } else {
            chosenFavoriteColor = favouriteColor;
        }
        
        Person.LastPerson = this;
        Person.HasInstantiatedPerson = true;

        books = config.personBooksConfig.books.GetRange(0, config.personBooksConfig.books.Count);

        /*
        Dictionary<string, Dictionary<string, float>> probabilityMap = new Dictionary<string, Dictionary<string, float>>() {
            {"clothing", new Dictionary<string, float>() {
                    {"briefs", 1f},
                    {"panties", 0.1f},
                    {"bras", 0.5f},
                    {"shirts", 0.4f},
                    {"jeans", 0.4f},
                    {"dress", 0.01f},
                    {"skirt", 0.01f},
                    {"socks", 0.9f},
                    {"hat", 0.05f},
                    {"cap", 0.05f},
                    {"slippers", 0.05f},
                    {"shoes", 0.05f},
                    {"gloves", 0.05f},
                }
            },
            {"pills", new Dictionary<string, float>() {
                    {"allergies", 1f},
                    {"moreThanOne", 0f},
                    {"smuggler", 0.01f}
                }
            },
            {"weapons", new Dictionary<string, float>() {
                    {"risk", 1f},
                    {"riskGun", 1f},
                    {"riskKnife", 1f}
                }
            },
            // Computers, mobile phone material
            {"displays", new Dictionary<string, float>() {
                    {"piracy", 0.2f},
                    {"worried", 0.5f},
                    {"noob", 0.3f}
                }
            },
            {"personality", new Dictionary<string, float>() {
                    {"rude", 0.2f},
                    {"polite", 0.4f},
                    {"impatient", 0.3f},
                }
            }
        };
        */

        // Load information on audio clips
        clips = Resources.LoadAll<AudioClip>("voice/" + voice).ToList();
        // Greeting
        // Some people don't say greeting - in those cases, pretend we've already greeted
//        haveSaidGreeting = ItsRandom.randomBool(); // TODO
        haveSaidGreeting = false;
    }

    private Color getFavouriteColor() {
        return chosenFavoriteColor;
    }
    
	// Use this for initialization
	void Start () {
        Debug.Log("CONFIG");
        Debug.Log(config);

        // Choose greeting
        greeting = ItsRandom.pickRandom(clips.FindAll(i => i.name.StartsWith("greetings-")));

        bagDefinition.person = this;
        currentX = this.transform.position.x;
        walkingMan.reportPositionX(currentX);
        walkingMan.setColors(bodyColor, chosenFavoriteColor);

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
        yield return bagHandler.packBagAndDropIt(bagDropPosition, bagDefinition, toBePlacedInTrays, this);
    }

    private List<Color> getTeddyColors(Color baseColor) {
        Color colorDiff = new Color(0.2f, 0.2f, 0.2f);

        float r = baseColor.r;
        float g = baseColor.g;
        float b = baseColor.b;

        int numberTooDark = 0;
        if (r < 0.15f) {
            numberTooDark++;
        }
        if (g < 0.15f) {
            numberTooDark++;
        }
        if (b < 0.15f) {
            numberTooDark++;
        }

        List<Color> colors = new List<Color>();
        if (numberTooDark < 2) {
            colors.Add(baseColor);
            colors.Add(baseColor - colorDiff);
        } else {
            colors.Add(baseColor + colorDiff);
            colors.Add(baseColor);
        }

        return colors;
    } 

    public void setTeddyBearColor(BagProperties bag) {
        List<Color> teddyColors = getTeddyColors(chosenFavoriteColor); 
        GameObject teddybear = bag.teddybearWithMaterials;
        // Add PerRendererShaders on teddybear and assign favorite colors on it
        PerRendererShader teddyMain = teddybear.AddComponent<PerRendererShader>();
        teddyMain.materialIndex = 0;
        teddyMain.color = teddyColors[0];
        
        PerRendererShader teddyTass = teddybear.AddComponent<PerRendererShader>();
        teddyTass.materialIndex = 1;
        teddyTass.color = teddyColors[1];
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

        yield return new WaitForSeconds(ItsRandom.randomRange(4f, 8f));
        while (Misc.HasCoroutine(coroutineKey)) {
            if (ItsRandom.randomRange(0, 100) < 25) {
                yield return playVoice(ItsRandom.pickRandom(itemAudioClips));
            }
            yield return new WaitForSeconds(ItsRandom.randomRange(3f, 5f));
        }
    }

    private IEnumerator reactOnAction(string action) {
        yield return new WaitForSeconds(ItsRandom.randomRange(0.5f, 1.5f));
        List<AudioClip> reactionAudioClips = clips.FindAll(i => i.name.Contains(action + "-"));
        yield return playVoice(ItsRandom.pickRandom(reactionAudioClips));
    }

    private void reactOnInspectAction(InspectUIButton.INSPECT_TYPE action, BagContentProperties item) {
        if (action == InspectUIButton.INSPECT_TYPE.MANUAL_INSPECT || action == InspectUIButton.INSPECT_TYPE.MANUAL_INSPECT_NEW) {
            // TODO - Maybe don't react on 50% (ajdust level)
            if (ItsRandom.randomBool()) {
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
                Debug.Log("Leftmostbag " + leftmostBag);
                if (leftmostBag != null && !leftmostBag.isDestroyed) {
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
                            // showPassport(false);
                        }

                        // Check if we move past a certain point, to say our greeting
                        if (currentX > greetingPositionX && !haveSaidGreeting) {
                            haveSaidGreeting = true;
                            playVoice(greeting);
                        }
                    }
                } else {
                    walkingMan.finishWalkingMan();
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

    public void showPassport(bool show = true) {
        showingPassport = show;
        float passportMovementY = 2.61f;
        if (show && passport == null) {
            passport = Instantiate(passportPrefab);
            passport.person = this;
            passport.setFavoriteColor(chosenFavoriteColor);
            Vector3 passportTargetPosition = new Vector3(passport.transform.localPosition.x - PASSPORT_OFFSET_X * (passport.id % 6), passport.transform.localPosition.y + passportMovementY, passport.transform.localPosition.z);
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

    public void reportToAuthorities() {
        // TODO - was it really a mistake to "report person"?
        worstMistake = "false arrest";
        playVoice(ItsRandom.pickRandom(clips.FindAll(i => i.name.StartsWith("false_arrest-"))));
        bagDefinition.bags.ForEach(bag => bag.bagFinished(false));
        passport.animateAndDestroy();
        Destroy(walkingMan.gameObject);
        finishPerson();
    }

    public Tuple2<string, string> getRandomBook() {
        if (books.Count > 0) {
            Tuple2<string,string> book = ItsRandom.pickRandom(books);
            int bookIndex = books.IndexOf(book);
            books.RemoveAt(bookIndex);
            return book;
        }

        return null;
    }

    public void applyTeddybearForce(float force) {
        foreach (BagProperties bag in bagDefinition.bags) {
            if (!bag.isDestroyed) {
                bag.teddybearWithMaterials.GetComponentInParent<Rigidbody>().AddForce(new Vector3(force, 0, 0), ForceMode.Impulse);
            } 
        }
    }
}
