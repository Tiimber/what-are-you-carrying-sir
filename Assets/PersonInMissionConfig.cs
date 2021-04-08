using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEngine;

public class PersonInMissionConfig {
    public string illegal;
    public bool illegalHidden = false;
    public bool canBeYourself = false; 
    public XmlDocument personConfig;
    public Texture2D personTexture;

    public PersonInMissionConfig(string illegal, bool illegalHidden, bool canBeYourself) {
        this.illegal = illegal;
        this.illegalHidden = illegalHidden;
        this.canBeYourself = canBeYourself;
    }

    public static IEnumerator LoadConfig(XmlNode personXml, List<PersonInMissionConfig> people) {
        string href = Misc.xmlString(personXml.Attributes.GetNamedItem("href"));
        string illegal = Misc.xmlString(personXml.Attributes.GetNamedItem("illegal"));
        bool illegalHidden = Misc.xmlBool(personXml.Attributes.GetNamedItem("illegalHidden"), false);
        bool canBeYourself = Misc.xmlBool(personXml.Attributes.GetNamedItem("canBeYourself"), true);

        PersonInMissionConfig personInMissionConfig = new PersonInMissionConfig(illegal, illegalHidden, canBeYourself);
        people.Add(personInMissionConfig);

        yield return Singleton<SingletonInstance>.Instance.StartCoroutine(Game.instance.loadPersonConfig(href));

        personInMissionConfig.personConfig = Game.instance.peopleConfigs[0];
        personInMissionConfig.personTexture = Game.instance.passportTextures[0];
        Game.instance.clearPersonConfigs();
        
        yield return null;
    }
}