using System.Collections.Generic;

public class PersonBagDefinition {

    public Person person;

    // All bags (and trays) belonging to person
    public List<BagProperties> bags = new List<BagProperties>();

    // All items not left in any bag (police/throw away)
    public List<BagContentProperties> removedItems = new List<BagContentProperties>();

}
