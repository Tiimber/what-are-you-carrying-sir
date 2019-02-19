using UnityEngine;

[System.SerializableAttribute]
public class MoveDefinition {
    public GameObject gameObject;
    public Vector3 startInspectPosition;
    public bool animateReverse = true;
    public Vector3 endInspectPosition;
    public float animationTime;
}