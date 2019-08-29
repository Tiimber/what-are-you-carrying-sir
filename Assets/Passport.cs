using System.Collections;
using System.Collections.Generic;
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

    void Awake() {
        id = ++PASSPORT_ID;
    }

    void Start() {
        PASSPORT_LAYER_MASK = LayerMask.GetMask(new string[]{"Passport"});

        PubSub.subscribe("Click", this);
        PubSub.subscribe("CameraMovementStarted", this);
        PubSub.subscribe("CameraMovementFinished", this);

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

}
