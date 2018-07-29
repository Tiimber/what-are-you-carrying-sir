using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Generic : MonoBehaviour {

    public enum CONSEQUENCE {
        NOTHING,

        WEAPON,
        DRUGS,
        EXPLOSIVES,
        BIOLOGICAL,
        ANIMAL,

        SMUGGLE
    }

    public static Dictionary<CONSEQUENCE, int> consequenceCounts = new Dictionary<CONSEQUENCE, int>();

    public static Dictionary<CONSEQUENCE, Dictionary<int, string>> consequenceLimits = new Dictionary<CONSEQUENCE, Dictionary<int, string>>(){
		{
			CONSEQUENCE.WEAPON, new Dictionary<int, string>(){
				{1, "Police caught a man with a weapon. \"Shame on inspector, big security fail on behalf of airport staff\", police says."},
				{2, "Man shot in leg at airport."},
				{3, "Woman dead after shooting at airport."},
				{4, "Family of five shot at point blank at airport."},
				{5, "Pilot shot, horrible plane crash with 330 people dead. Amongst the dead was a pregnant woman expecting triplets."}
			}
		}
	};

    public static string increaseConsequenceCount (CONSEQUENCE consequence) {
        if (!consequenceCounts.ContainsKey(consequence)) {
            consequenceCounts.Add(consequence, 1);
        } else {
            consequenceCounts[consequence] = consequenceCounts[consequence] + 1;
        }

        if (consequenceLimits.ContainsKey(consequence)) {
			Dictionary<int, string> consequenceLimit = consequenceLimits[consequence];
			int amountOfConsequence = consequenceCounts[consequence];
			if (consequenceLimit.ContainsKey(amountOfConsequence)) {
                return consequenceLimit[amountOfConsequence];
            } else if (consequenceLimit.Keys.Max() < amountOfConsequence) {
				return consequenceLimit[consequenceLimit.Keys.Max()];
            }
		}

        return null;
    }

    // Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
