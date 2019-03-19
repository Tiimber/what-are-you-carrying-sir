public class InspectActionBag {
    public int bagId;
    public BagContentProperties item;
    public InspectUIButton.INSPECT_TYPE action;

    public InspectActionBag(int bagId, BagContentProperties item, InspectUIButton.INSPECT_TYPE action) {
        this.bagId = bagId;
        this.item = item;
        this.action = action;
    }
}
