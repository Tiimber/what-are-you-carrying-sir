using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToParentNull : MonoBehaviour {
    void Awake() {
        this.transform.parent = null;
    }
}
