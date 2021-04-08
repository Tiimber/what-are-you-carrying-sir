using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClockPositions : MonoBehaviour {
    public ClockPosition[] positions;
    
}

[System.SerializableAttribute]
public class ClockPosition {
    public string description; 
    public Vector3 position;
    public Vector3 rotation;
    public Vector3 scale;
}