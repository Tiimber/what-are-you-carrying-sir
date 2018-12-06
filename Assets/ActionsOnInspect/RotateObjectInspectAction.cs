using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.SerializableAttribute]
public class RotateObjectInspectAction : MonoBehaviour, ActionOnInspect {

    public RotationDefinition[] rotationDefinitions;

	public void run(bool reverse) {
        for (int i = 0; i < rotationDefinitions.Length; i++) {
            RotationDefinition rotationDefinition = rotationDefinitions[i];
            float time = rotationDefinition.animationTime > 0 ? rotationDefinition.animationTime : Misc.DEFAULT_ANIMATION_TIME;
            if (!reverse) {
                Misc.AnimateRotationTo("rotate_inspect_item_"+i, rotationDefinition.gameObject, Quaternion.Euler(rotationDefinition.startInspectRotation), time);
            } else if (rotationDefinition.animateReverse) {
                Misc.AnimateRotationTo("rotate_inspect_item_end_"+i, rotationDefinition.gameObject, Quaternion.Euler(rotationDefinition.endInspectRotation), time);
            }
        }
	}

}
