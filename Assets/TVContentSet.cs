using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TVContentSet : MonoBehaviour {

    public GameObject top;
    public GameObject bottom;

    public float getTop() {
        return top.transform.localPosition.y;
    }

    public float getBottom() {
        return bottom.transform.localPosition.y;
    }

}
