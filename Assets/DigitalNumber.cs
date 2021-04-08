using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DigitalNumber : MonoBehaviour {

    public GameObject b;
    public GameObject bl;
    public GameObject br;
    public GameObject m;
    public GameObject t;
    public GameObject tl;
    public GameObject tr;

    private List<GameObject> leds = new List<GameObject>();

    private Dictionary<int, List<bool>> numbers = new Dictionary<int, List<bool>>() {
        {0, new List<bool>() {true, true, true, false, true, true, true} },
        {1, new List<bool>() {false, false, true, false, false, false, true} },
        {2, new List<bool>() {true, true, false, true, true, false, true} },
        {3, new List<bool>() {true, false, true, true, true, false, true} },
        {4, new List<bool>() {false, false, true, true, false, true, true} },
        {5, new List<bool>() {true, false, true, true, true, true, false} },
        {6, new List<bool>() {true, true, true, true, true, true, false} },
        {7, new List<bool>() {false, false, true, false, true, false, true} },
        {8, new List<bool>() {true, true, true, true, true, true, true} },
        {9, new List<bool>() {true, false, true, true, true, true, true} }
    };

    private void initLeds() {
        leds.Add(b);
        leds.Add(bl);
        leds.Add(br);
        leds.Add(m);
        leds.Add(t);
        leds.Add(tl);
        leds.Add(tr);
    }
    
    void Awake() {
        initLeds();
    }

    public void setNumber(int number) {
        if (leds.Count == 0) {
            initLeds();
        }
        List<bool> ledToggles = numbers[number];
        for (int i = 0; i < ledToggles.Count; i++) {
            GameObject led = leds[i];
            led.SetActive(ledToggles[i]);
        }
    }
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
