using System.Collections;
using System.Collections.Generic;
using Obi;
using UnityEngine;

public class TmpRunRandom : MonoBehaviour {
    public bool destroy;
    public bool enable;
    private bool firstTime = true;
    public ObiSolver solver;
    
    // Start is called before the first frame update
    void Start()
    {
        ItsRandom.setRandomSeed(9);
        
        // Trigger "random"-functions on it
        RandomInterface[] randomInterfaces = GetComponents<RandomInterface>();
        foreach (RandomInterface randomInterface in randomInterfaces) {
            randomInterface.run();
        }

        // ActionOnInspect[] actionInterfaces = GetComponents<ActionOnInspect>();
        // foreach (ActionOnInspect actionInterface in actionInterfaces) {
        //     actionInterface.run();
        // }
    }

    // Update is called once per frame
    void Update()
    {
        if (destroy) {
            GameObject.Destroy(this.gameObject);
        }
        if (enable && firstTime) {
            firstTime = false;
            // solver.transform.SetParent(null);
            // solver.gameObject.SetActive(true);
            // solver.simulateWhenInvisible = true;
            // solver.parameters.gravity = Vector3.zero;
            GetComponent<PillBottle>().fillLiquid();
        }
    }
}
