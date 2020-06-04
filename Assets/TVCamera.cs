using System.Collections;
using UnityEngine;

public class TVCamera : MonoBehaviour {

    public GameObject blackBarLeft;
    public GameObject blackBarRight;
    public GameObject blackBarTop;
    public GameObject blackBarBottom;

    public float topBottomTurnOnScaleZ;
    public float topBottomTurnOffScaleZ;
    public float leftRightTurnOnScaleX;
    public float leftRightTurnOffScaleX;

    private bool on = true;

    public bool isOn () {
        return on;
    }

    public void ToggleTV () {
        if (!Misc.IsScaleAnimationActive("tvPowerToggleTopBar") && !Misc.IsScaleAnimationActive("tvPowerToggleLeftBar")) {
            bool toggledState = !on;
            StartCoroutine(delayToggleOnState());
            StartCoroutine(toggleTVState(toggledState));
        }
    }

    private IEnumerator delayToggleOnState () {
        if (on) {
            on = false;
        } else {
            yield return null;
            on = true;
        }
    }

    private IEnumerator toggleTVState(bool toggledState) {
        float topBottomTargetZ = toggledState ? topBottomTurnOnScaleZ : topBottomTurnOffScaleZ;
        float leftRightTargetX = toggledState ? leftRightTurnOnScaleX : leftRightTurnOffScaleX;

        Vector3 topBarTargetScale = new Vector3(blackBarTop.transform.localScale.x, blackBarTop.transform.localScale.y, topBottomTargetZ);
        Vector3 bottomBarTargetScale = new Vector3(blackBarBottom.transform.localScale.x, blackBarBottom.transform.localScale.y, topBottomTargetZ);
        Misc.AnimateScaleTo("tvPowerToggleTopBar", blackBarTop, topBarTargetScale, Misc.DEFAULT_ANIMATION_TIME, Misc.ANIMATION_SQRT);
        Misc.AnimateScaleTo("tvPowerToggleBottomBar", blackBarBottom, bottomBarTargetScale, Misc.DEFAULT_ANIMATION_TIME, Misc.ANIMATION_SQRT);

        yield return new WaitForSeconds(0.2f);

        Vector3 leftBarTargetScale = new Vector3(leftRightTargetX, blackBarLeft.transform.localScale.y, blackBarLeft.transform.localScale.z);
        Vector3 rightBarTargetScale = new Vector3(leftRightTargetX, blackBarRight.transform.localScale.y, blackBarRight.transform.localScale.z);
        Misc.AnimateScaleTo("tvPowerToggleLeftBar", blackBarLeft, leftBarTargetScale, Misc.DEFAULT_ANIMATION_TIME, Misc.ANIMATION_SQRT);
        Misc.AnimateScaleTo("tvPowerToggleRightBar", blackBarRight, rightBarTargetScale, Misc.DEFAULT_ANIMATION_TIME, Misc.ANIMATION_SQRT);

    }

}
