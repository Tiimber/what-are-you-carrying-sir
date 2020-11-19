using System;
using System.Collections;
using System.Xml;
using UnityEngine;
using UnityEngine.Networking;

public class PersonConfig {

    public string id;
    public string name;
    public string nationality;
    public DateTime dob;
    public string idPhrase;
    public string photo;
    public Texture photoTexture;
    public string voice;
    public Color bodyColor;
    public Color favouriteColor;
    public Color favouriteColor2;

    public PersonBooksConfig personBooksConfig;

    public PersonConfig(XmlDocument xmlDoc, Texture2D photoTexture) {
        XmlAttributeCollection personAttributes = xmlDoc.SelectSingleNode("/person").Attributes;
        id = Misc.xmlString(personAttributes.GetNamedItem("id"));
        name = Misc.xmlString(personAttributes.GetNamedItem("name"));
        nationality = Misc.xmlString(personAttributes.GetNamedItem("nationality"));
        dob = Misc.parseDate(Misc.xmlString(personAttributes.GetNamedItem("dateOfBirth")));
        idPhrase = Misc.xmlString(personAttributes.GetNamedItem("idPhrase"));
        // photo = Misc.xmlString(personAttributes.GetNamedItem("photo"));
        this.photoTexture = photoTexture;
        voice = Misc.xmlString(personAttributes.GetNamedItem("voice"));
        bodyColor = Misc.parseColor(Misc.xmlString(personAttributes.GetNamedItem("bodyColor")));
        favouriteColor = Misc.parseColor(Misc.xmlString(personAttributes.GetNamedItem("favouriteColor")));
        // favouriteColor2 = Misc.parseColor(Misc.xmlString(personAttributes.GetNamedItem("scondaryFavouriteColor")));

        personBooksConfig = new PersonBooksConfig(xmlDoc.SelectNodes("/person/books/book"));
        
        // // Load photo
        // Singleton<SingletonInstance>.Instance.StartCoroutine(LoadImageTexture(this));
    }

    // private static IEnumerator LoadImageTexture(PersonConfig config) {
    //     string photoUrl = config.photo;
    //     WWW www = CacheWWW.Get(photoUrl);
    //     yield return www;
    //     Texture2D texture2D = new Texture2D (350, 389);
    //     www.LoadImageIntoTexture (texture2D);
    //     config.photoTexture = texture2D;
    // }
}
