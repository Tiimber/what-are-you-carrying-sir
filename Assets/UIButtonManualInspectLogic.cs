using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIButtonManualInspectLogic : MonoBehaviour {

    public GameObject bigTray;

    public GameObject sameTray;

    public GameObject newTray;

    public void showCorrectButtons (bool newTrayAvailable) {
        bigTray.SetActive(!newTrayAvailable);

        sameTray.SetActive(newTrayAvailable);
        newTray.SetActive(newTrayAvailable);
    }

}
