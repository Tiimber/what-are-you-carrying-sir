using UnityEngine;

[System.SerializableAttribute]
public class RotationDefinition {
    public GameObject gameObject;
    public Vector3 startInspectRotation;
    public bool animateReverse = true;
    public bool keepEndRotationFromOriginal = false;
    [HideInInspector]
    public bool haveSetEndInspectRotation = false;
    public Vector3 endInspectRotation;
    public float animationTime;
    public float animationDelay;
}