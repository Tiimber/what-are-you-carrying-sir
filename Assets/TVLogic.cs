using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TVLogic : MonoBehaviour, IPubSub {

    const float CAMERA_VISION_VERTICAL = 34.4f;

    public GameObject TVBoundaryTop;
    public GameObject TVBoundaryBottom;

    public Camera camera;
    public TVContentSet currentContent; // TODO - Not public later, should only be set in code

    void Start() {
        PubSub.subscribe("scrollvertical", this);
    }

    public void setCurrentContent(TVContentSet content) {
        this.currentContent = content;
        resetCamera();
    }

    private void resetCamera() {
        scrollCamera(currentContent.getTop() - currentContent.getBottom());
    }

    private void scrollCamera(float amount) {
        float targetPosY = camera.transform.localPosition.y + amount;
        float actualAmount = amount;
        if (amount > 0f) {
            float targetTop = targetPosY + CAMERA_VISION_VERTICAL / 2f;
            float contentTop = currentContent.getTop();
            if (targetTop > contentTop) {
                actualAmount -= targetTop - contentTop;
            }
        } else if (amount < 0f) {
            float targetBottom = targetPosY - CAMERA_VISION_VERTICAL / 2f;
            float contentBottom = currentContent.getBottom();
            if (targetBottom < contentBottom) {
                actualAmount += contentBottom - targetBottom;
            }
        }

        Vector3 cameraTargetPosition = camera.gameObject.transform.localPosition + new Vector3(0f, actualAmount, 0f);
        Misc.AnimateMovementTo("tv-content-camera", camera.gameObject, cameraTargetPosition);
    }

    public PROPAGATION onMessage(string message, object data) {
        if (message == "scrollvertical") {
            float amount = (float) data;
            scrollCamera(amount);
        }
        return default(PROPAGATION);
    }

}
