using UnityEngine;

[System.SerializableAttribute]
public class SlideInOtherObjectDefinition {
    public GameObject gameObjectToSlideIn;
    public GameObject instantiatedObject;
    public BagContentProperties pills;
    public Vector3 startSlidePosition;
    public Vector3 endSlidePosition;
    public bool animateReverse = true;
    public bool hideAfterReverse = true;
    public float animationTime;
    public bool enabled = true;
}