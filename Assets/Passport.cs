using System.Globalization;
using TMPro;
using UnityEngine;

public class Passport : MonoBehaviour, IPubSub {

    private static int PASSPORT_LAYER_MASK = -1;
    private static int PASSPORT_ID = 0;

    public int id;

    private bool isZoomedIn = false;

    public Vector3 originalPosition;
    public Quaternion originalRotation;
    public bool startInactive;

    private static Passport ZoomedInPassport;
    private static CultureInfo cultureInfoEnUS = new CultureInfo("en-US");

    public GameObject photo;
    public TextMeshPro passportName;
    public TextMeshPro nationality;
    public TextMeshPro dateOfBirth;
    public TextMeshPro quote;
    public GameObject favoriteColorObject;

    public Person person;

    void Awake() {
        id = ++PASSPORT_ID;
    }

    void Start() {
        PASSPORT_LAYER_MASK = LayerMask.GetMask(new string[]{"Passport"});

        if (person != null) {
            PubSub.subscribe("Click", this);
            PubSub.subscribe("CameraMovementStarted", this);
            PubSub.subscribe("CameraMovementFinished", this);

            passportName.text = person.personName;
            nationality.text = person.nationality;
            dateOfBirth.text = person.dateOfBirth.ToString("MMMM yyyy", cultureInfoEnUS);
            quote.text = person.idPhrase;
            photo.GetComponent<PerRendererShaderTexture>().texture = person.photo;
        }

        if (startInactive) {
            this.gameObject.SetActive(false);
        }
    }

    void OnDestroy() {
        PubSub.unsubscribeAllForSubscriber(this);
    }

    public PROPAGATION onMessage(string message, object data) {
        if (message == "Click") {
            Vector3 position = Vector3.zero;
            if (data.GetType() == typeof(Vector2)) {
                Vector2 posV2 = (Vector2)data;
                position = new Vector3(posV2.x, posV2.y);
            } else {
                position = (Vector3)data;
            }

            // Get camera
            Camera camera = Game.instance.gameCamera;
            RaycastHit hit;
            Ray ray = camera.ScreenPointToRay(position);

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, PASSPORT_LAYER_MASK)) {
                Passport passport = hit.transform.parent.GetComponent<Passport>();
                if (passport != null) {
                    passport.togglePassport();
                }
            }
            return PROPAGATION.STOP_IMMEDIATELY;
        } else if (message == "CameraMovementStarted") {
            if (isZoomedIn) {
                togglePassport();
            }
            this.gameObject.SetActive(false);
        } else if (message == "CameraMovementFinished") {
            if (Game.instance.cameraXPos == 1 && !Game.instance.zoomedOutState) {
                this.gameObject.SetActive(true);
            }
        }
        return default(PROPAGATION);
    }

    public void setFavoriteColor(Color color) {
        favoriteColorObject.GetComponent<Renderer>().material.SetColor("_Color", color);
    }

    private void togglePassport() {
        // Only toggle if no animation is active
        string animationKey = "passport_animation_zoom_" + id;
        if (isZoomedIn) {
            Misc.AnimateMovementTo(animationKey, this.gameObject, originalPosition, Misc.DEFAULT_ANIMATION_TIME, true);
            Misc.AnimateRotationTo(animationKey, this.gameObject, originalRotation);

            ZoomedInPassport = null;
        } else {
            Vector3 cameraPosition = Game.instance.gameCamera.transform.position;
            Vector3 targetPosition = new Vector3(0, 0, 14.812f) + cameraPosition;
            Misc.AnimateMovementTo(animationKey, this.gameObject, targetPosition, Misc.DEFAULT_ANIMATION_TIME, true);
            Misc.AnimateRotationTo(animationKey, this.gameObject, Quaternion.identity);

            if (ZoomedInPassport != null) {
                ZoomedInPassport.togglePassport();
            }
            ZoomedInPassport = this;
        }
        isZoomedIn = !isZoomedIn;
    }

    void OnGUI() {
        if (isZoomedIn) {
            if (GUI.Button(new Rect(Screen.width / 2f - 50f, Screen.height - 100f, 100f, 50), "Report person!")) {
                person.reportToAuthorities();
            }
        }
    }

    public void animateAndDestroy() {
//        Misc.AnimateMovementTo("person_passport_hide_" + id, gameObject, passportTargetPosition);
        Misc.AnimateScaleTo("passport_scale_destroy_" + id, gameObject, new Vector3(0.001f, 0.001f, 0.001f));
        Misc.SetActiveAfterDelay("passport_active_destroy_" + id, gameObject, false, true);
    }

}
