using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RandomBook : MonoBehaviour, RandomInterface {

	public TextMeshPro[] authors;
	public TextMeshPro[] titles;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void run() {
	    // First, try to lookup book author + title in person
	    Person person = GetComponent<BagContentProperties>().person;
	    Tuple2<string, string> randomBook = person.getRandomBook();
	    if (randomBook == null) {
		    randomBook = ItsRandom.pickRandom(GenericBookText.books);
	    }
	    
	    foreach (TextMeshPro author in authors) {
		    author.text = randomBook.First;
	    }
	    foreach (TextMeshPro title in titles) {
		    title.text = randomBook.Second;
	    }
    }
}
