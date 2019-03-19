using System.Collections.Generic;

public class PersonBagDefinition {

    public Person person;

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

}
