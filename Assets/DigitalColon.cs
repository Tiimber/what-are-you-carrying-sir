using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DigitalColon : MonoBehaviour {

    public bool shouldBlink;
    
    // Update is called once per frame
    void Update() {
        if (shouldBlink) {
            int second = (int) Time.time;
            transform.GetChild(0).gameObject.SetActive(second % 2 == 1);
            transform.GetChild(1).gameObject.SetActive(second % 2 == 1);
        }
    }
}
