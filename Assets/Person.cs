using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Person : MonoBehaviour {

    private static int PERSON_ID = 0;

    private int id = ++PERSON_ID;
    private PersonBagDefinition bagDefinition = new PersonBagDefinition();
    public List<BagContentProperties> toBePlacedInTrays = new List<BagContentProperties>();

	// Use this for initialization
	void Start () {
		bagDefinition.person = this;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void startPlaceBags(BagHandler bagHandler, Vector3 bagDropPosition) {
        // TODO - each instantiated item should have a ruleset on if it prefers tray or not, and if max one can be instantiated per person...
        StartCoroutine(startPlaceBagsCoroutine(bagHandler, bagDropPosition));

        if (toBePlacedInTrays.Count > 0) {
            // TODO - Create tray with items (after delay)
//            BagHandler.instance.createTrayWithContents(Game.instance.getTrayDropPosition(), manualInspectItems);
        }
    }

    public IEnumerator startPlaceBagsCoroutine(BagHandler bagHandler, Vector3 bagDropPosition) {
        yield return bagHandler.packBagAndDropIt(bagDropPosition, bagDefinition, toBePlacedInTrays, id);
    }
}
