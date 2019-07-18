using System;
using System.Collections.Generic;
using UnityEngine;

public class ToggleButtons : MonoBehaviour, IPubSub {

    private static int TOGGLE_BUTTONS_LAYER_MASK = -1;
    public SpawningSoundObject soundObject;
    public AudioClip buttonPressClip;
    public AudioClip buttonDepressClip;

    public enum ToggleMaterial {
        Metal,
        Organic,
        Liquid,
        Other,
    }

    public static List<ToggleMaterial> XRAY_MATERIALS_VISIBLE = new List<ToggleMaterial>(){ToggleMaterial.Metal, ToggleMaterial.Organic, ToggleMaterial.Liquid, ToggleMaterial.Other};

    public static ToggleMaterial GetToggleMaterialForString(string name) {
        if (name.Contains("liquid")) {
            return ToggleMaterial.Liquid;
        } else if (name.Contains("metal")) {
            return ToggleMaterial.Metal;
        } else if (name.Contains("organic")) {
            return ToggleMaterial.Organic;
        }
        return ToggleMaterial.Other;
    }

    // Start is called before the first frame update
    void Start() {
        TOGGLE_BUTTONS_LAYER_MASK = LayerMask.GetMask(new string[]{"XRayButton"});

        PubSub.subscribe("Click", this);
    }

    public PROPAGATION onMessage(string message, object data) {
        if (message == "Click") {
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

                if (Physics.Raycast(ray, out hit, Mathf.Infinity, TOGGLE_BUTTONS_LAYER_MASK)) {
//                    Debug.Log(hit.transform.gameObject.name);
                    ToggleButton toggleButton = hit.transform.GetComponent<ToggleButton>();
                    if (toggleButton != null) {
                        toggleButton.toggle();
                    }
                }
            }
        }
        return default(PROPAGATION);
    }

    public void buttonToggled(string name) {
        ToggleMaterial material;
        Enum.TryParse(name, out material);
        bool pressed;

        if (XRAY_MATERIALS_VISIBLE.Contains(material)) {
            pressed = false;
            XRAY_MATERIALS_VISIBLE.Remove(material);
        } else {
            pressed = true;
            XRAY_MATERIALS_VISIBLE.Add(material);
        }

        PubSub.publish("xray_button_toggle");

        soundObject.spawn(pressed ? buttonPressClip : buttonDepressClip);
    }
}
