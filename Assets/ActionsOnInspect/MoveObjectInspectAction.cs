using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.SerializableAttribute]
public class MoveObjectInspectAction : MonoBehaviour, ActionOnInspect {

    public MoveDefinition[] moveDefinitions;

	public void run(bool reverse) {
        for (int i = 0; i < moveDefinitions.Length; i++) {
            MoveDefinition moveDefinition = moveDefinitions[i];
            float time = moveDefinition.animationTime > 0 ? moveDefinition.animationTime : Misc.DEFAULT_ANIMATION_TIME;
            if (!reverse) {
                Misc.AnimateMovementTo("move_inspect_item_"+i, moveDefinition.gameObject, moveDefinition.startInspectPosition, time, true);
            } else if (moveDefinition.animateReverse) {
                Misc.AnimateMovementTo("move_inspect_item_end_"+i, moveDefinition.gameObject, moveDefinition.endInspectPosition, time, true);
            }
        }
	}

}
