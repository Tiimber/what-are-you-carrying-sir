using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.SerializableAttribute]
public class SlideInOtherObjectInspectAction : MonoBehaviour, ActionOnInspect {

    public SlideInOtherObjectDefinition[] slideInObjectDefinitions;

	public void run(bool reverse) {
        for (int i = 0; i < slideInObjectDefinitions.Length; i++) { 
            SlideInOtherObjectDefinition slideInObjectDefinition = slideInObjectDefinitions[i];
            if (slideInObjectDefinition.instantiatedObject == null) {
                slideInObjectDefinition.instantiatedObject = Instantiate(slideInObjectDefinition.gameObjectToSlideIn, null);
                slideInObjectDefinition.instantiatedObject.transform.localPosition = slideInObjectDefinition.startSlidePosition;
                slideInObjectDefinition.instantiatedObject.GetComponent<BagContentPropertiesReference>().reference = GetComponent<BagContentProperties>();
            }
            slideInObjectDefinition.pills = GetComponent<BagContentProperties>();
            float time = slideInObjectDefinition.animationTime > 0 ? slideInObjectDefinition.animationTime : Misc.DEFAULT_ANIMATION_TIME;
            if (!reverse) {
                Misc.AnimateMovementTo("position_other_inspect_item_"+i, slideInObjectDefinition.instantiatedObject, slideInObjectDefinition.endSlidePosition, time);
            } else if (slideInObjectDefinition.animateReverse) {
                Misc.AnimateMovementTo("position_other_inspect_item_end_"+i, slideInObjectDefinition.instantiatedObject, slideInObjectDefinition.startSlidePosition, time);
            }
        }
	}

}
