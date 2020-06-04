using UnityEngine;
using UnityEngine.Events;

// This class works together with a GenericTrigger

public class TVContentsButton : MonoBehaviour, IPubSub {

    private static int TV_CONTENTS_BUTTONS_LAYER_MASK = -1;
    private static int TV_SCREEN_LAYER_MASK = -1;

    // Start is called before the first frame update
    void Start() {
        TV_CONTENTS_BUTTONS_LAYER_MASK = LayerMask.GetMask(new string[]{"TVContentsButton"});
        TV_SCREEN_LAYER_MASK = LayerMask.GetMask(new string[]{"TVScreen"});

        PubSub.subscribe("ClickTV", this, 99);
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

                RaycastHit hit;
                Ray ray = Game.instance.gameCamera.ScreenPointToRay(position);

                // do we hit our portal plane?
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, TV_SCREEN_LAYER_MASK)) {
                    Debug.Log(hit.collider.gameObject);


                    Vector2 localPoint = hit.textureCoord;
                    // convert the hit texture coordinates into camera coordinates
                    Camera tvCamera = Game.instance.tvCamera;
                    Ray tvRay = tvCamera.ScreenPointToRay(new Vector2(localPoint.x * tvCamera.pixelWidth, localPoint.y * tvCamera.pixelHeight));
                    RaycastHit tvCameraHit;
                    // test these camera coordinates in another raycast test
                    if(Physics.Raycast(tvRay, out tvCameraHit, Mathf.Infinity, TV_CONTENTS_BUTTONS_LAYER_MASK)) {
                        Debug.Log(tvCameraHit.collider.gameObject);
                        TVContentsButton genericButton = tvCameraHit.transform.GetComponent<TVContentsButton>();
                        if (genericButton == this) {
                            GetComponent<GenericTrigger>().toggle();
                        }
                    }
                }

            }
        }
        return default(PROPAGATION);
    }

}
