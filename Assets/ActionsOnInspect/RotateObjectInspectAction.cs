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
            if (rotationDefinition.keepEndRotationFromOriginal && !rotationDefinition.haveSetEndInspectRotation) {
                Vector3 rotationDefinitionEndInspectRotation = rotationDefinition.gameObject.transform.localRotation.eulerAngles;
                /*
                if (rotationDefinitionEndInspectRotation.x > 180f) {
                    rotationDefinitionEndInspectRotation.x -= 360f;
                }
                if (rotationDefinitionEndInspectRotation.y > 180f) {
                    rotationDefinitionEndInspectRotation.y -= 360f;
                }
                if (rotationDefinitionEndInspectRotation.z > 180f) {
                    rotationDefinitionEndInspectRotation.z -= 360f;
                }
                */
                rotationDefinition.endInspectRotation = rotationDefinitionEndInspectRotation;
                rotationDefinition.haveSetEndInspectRotation = true;
            }
            float time = rotationDefinition.animationTime > 0 ? rotationDefinition.animationTime : Misc.DEFAULT_ANIMATION_TIME;
            if (!reverse) {
                if (rotationDefinition.animationDelay > 0f) {
                    Misc.AnimateRotationFromToWithDelay("rotate_inspect_item_"+i, rotationDefinition.gameObject, rotationDefinition.endInspectRotation, rotationDefinition.startInspectRotation, rotationDefinition.animationDelay, time);
                } else {
                    Misc.AnimateRotationFromTo("rotate_inspect_item_"+i, rotationDefinition.gameObject, rotationDefinition.endInspectRotation, rotationDefinition.startInspectRotation, time);
                }
            } else if (rotationDefinition.animateReverse) {
                Misc.AnimateRotationFromTo("rotate_inspect_item_end_"+i, rotationDefinition.gameObject, rotationDefinition.startInspectRotation, rotationDefinition.endInspectRotation, time);
            }
        }
	}

}
