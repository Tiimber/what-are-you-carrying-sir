using UnityEngine;

[System.SerializableAttribute]
public class RotationDefinition {
    public GameObject gameObject;
    public Vector3 startInspectRotation;
    public bool animateReverse = true;
    public Vector3 endInspectRotation;
    public float animationTime;
}