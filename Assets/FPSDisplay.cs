using UnityEngine;
using System.Collections;

public class FPSDisplay : MonoBehaviour
{
	float deltaTime = 0.0f;
    int randomNumber;

    void Awake () {
		randomNumber = Misc.randomRange(1000, 9999);
    }

	void Update() {
		deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
	}

	void OnGUI() {
		int w = Screen.width, h = Screen.height;

		GUIStyle style = new GUIStyle ();

		int labelHeight = h * 2 / 100;
		Rect rect = new Rect (0, h - labelHeight, w, labelHeight);
		style.alignment = TextAnchor.UpperLeft;
		style.fontSize = labelHeight;
		style.normal.textColor = new Color (0.0f, 0.0f, 0.0f, 1.0f);
		float msec = deltaTime * 1000.0f;
		float fps = 1.0f / deltaTime;
		string text = string.Format ("v. {0:0} | {1:0.0} ms ({2:0.} fps)", randomNumber, msec, fps);
		GUI.Label (rect, text, style);
	}
}