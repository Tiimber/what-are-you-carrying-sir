using UnityEngine;
using UnityEngine.Events;

    // This class works together with a GenericTrigger

public class PhysicalTVButton : MonoBehaviour, IPubSub {

    private static int TV_CONTENTS_BUTTONS_LAYER_MASK = -1;

    // Start is called before the first frame update
    void Start() {
        TV_CONTENTS_BUTTONS_LAYER_MASK = LayerMask.GetMask(new string[]{"TVContentsButton"});

        PubSub.subscribe("ClickTV", this, 100);
    }

    public PROPAGATION onMessage(string message, object data) {
        if (message == "ClickTV") {
            if (!(Game.instance.cameraXPos == 2 && !Game.instance.zoomedOutState)) {
                Vector3 position = Vector3.zero;
                if (data.GetType() == typeof(Vector2)) {
                    Vector2 posV2 = (Vector2)data;
                    position = new Vector3(posV2.x, posV2.y);
                } else {
                    position = (Vector3)data;
                }

                // Get camera
                Camera camera = Game.instance.gameCamera;
                RaycastHit hit;
                Ray ray = camera.ScreenPointToRay(position);

                if (Physics.Raycast(ray, out hit, Mathf.Infinity, TV_CONTENTS_BUTTONS_LAYER_MASK)) {
                    //                    Debug.Log(hit.transform.gameObject.name);
                    PhysicalTVButton toggleButton = hit.transform.GetComponent<PhysicalTVButton>();
                    if (toggleButton == this) {
                        GetComponent<GenericTrigger>().toggle();
                    }
                }
            }
        }
        return default(PROPAGATION);
    }

}
