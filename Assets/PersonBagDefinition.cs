using System;
using System.Collections.Generic;

public class PersonBagDefinition {

    public Person person;
    public String bagRandomSeed;

    // All bags (and trays) belonging to person
    public List<BagProperties> bags = null;

    // All items not left in any bag (police/throw away)
    public List<BagContentProperties> removedItems = new List<BagContentProperties>();

    public bool hasInitiated () {
        return bags != null;
    }
    public void addBag (BagProperties bag) {
        if (bags == null) {
            bags = new List<BagProperties>();
        }

        bags.Add(bag);
    }

    public BagProperties getLeftmostBag() {
        BagProperties leftmostBag = null;
        foreach(BagProperties bag in bags) {
            if (leftmostBag == null || leftmostBag.gameObject.transform.position.x > bag.gameObject.transform.position.x) {
                leftmostBag = bag;
            }
        }
        return leftmostBag;
    }

    public BagProperties getRightmostBag() {
        return bags[0];
    }

}
