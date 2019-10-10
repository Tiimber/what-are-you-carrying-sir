using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.SerializableAttribute]
public class RandomScale : MonoBehaviour, RandomInterface {

    public Vector3[] possibleScales;

	// Use this for initialization
	void Start () {

	}

	// Update is called once per frame
	void Update () {

	}

	public void run() {
		Vector3 chosenScale = ItsRandom.pickRandom(possibleScales.ToList());
        BagContentProperties bagContentProperties = this.GetComponent<BagContentProperties>();
        bagContentProperties.objectSize = Vector3.Scale(bagContentProperties.objectSize, chosenScale);
        bagContentProperties.transform.localScale = chosenScale;
        bagContentProperties.GetComponent<Rigidbody>().mass = chosenScale.magnitude;
	}

}
