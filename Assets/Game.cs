using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour {

    const float CONVEYOR_SPEED = 0.005f;

    public static Game instance;

    void Awake () {
        Game.instance = this;
    }

    // Use this for initialization
	void Start () {
		
	}

	// Update is called once per frame
	void Update () {
		if (Input.GetKey(KeyCode.Space)) {
            List<GameObject> bags = Misc.FindShallowStartsWith("Bag_");
            foreach(GameObject bag in bags) {
                bag.transform.position = new Vector3(bag.transform.position.x + CONVEYOR_SPEED, bag.transform.position.y, bag.transform.position.z);
            }
        }
	}
}
