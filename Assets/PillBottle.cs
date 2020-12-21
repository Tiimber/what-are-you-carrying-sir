using System;
using System.Collections;
using System.Collections.Generic;
using Obi;
using UnityEngine;

public class PillBottle : MonoBehaviour {
    public Color colorHalf1;
    public Color colorHalf2;
    
    public ObiSolver solver;

    public bool liquidPrepared = false;
    private bool needCalculateGravity = false;

    public bool fillLiquid() {
        if (liquidPrepared) {
            solver.transform.SetParent(null);
            solver.gameObject.SetActive(true);
            solver.simulateWhenInvisible = true;
            // solver.parameters.gravity = Vector3.zero;
            needCalculateGravity = true;
            return true;
        }

        return false;
    }

    public void emptyLiquid() {
        solver.transform.SetParent(this.transform);
        solver.gameObject.SetActive(false);
        solver.simulateWhenInvisible = false;
        needCalculateGravity = false;
    }

    public Vector3 localBottleGravityVector = new Vector3(3.5f, 0f, 10f);
    void Update() {
        if (needCalculateGravity) {
            // If outside of pillbottle
            solver.parameters.gravity = localBottleGravityVector;
            
            // Quaternion bottleWorldRotation = Misc.getWorldRotation(transform);
            // solver.parameters.gravity = bottleWorldRotation * localBottleGravityVector;
            // if (System.DateTime.Now.Second % 10 == 5) {
                // solver.parameters.gravity = -(bottleWorldRotation * localBottleGravityVector);
            // }
            solver.UpdateParameters();
            
            // solver.parameters.gravity = new Vector3(0f, 0f, -1f);
            // solver.parameters.gravity = new Vector3(0f, 0f, -1f) - bottleWorldRotation.eulerAngles;
            // Debug.Log(bottleWorldRotation.eulerAngles.ToString());
            // bottleWorldRotation.eulerAngles = 0, 0, 0
            // gravity 0, 0, 1f
        }
    }

    // private void OnDestroy() {
    //     GameObject.Destroy(solver.gameObject);
    // }
}
