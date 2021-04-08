using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEngine;

public class EncountersConfig {
    public bool untilEndTime = false;
    public bool untilQueueEmpty = true;
    public List<PersonInMissionConfig> people;

    public EncountersConfig(bool untilEndTime, bool untilQueueEmpty, List<PersonInMissionConfig> people) {
        this.untilEndTime = untilEndTime;
        this.untilQueueEmpty = untilQueueEmpty;
        this.people = people;
    }

    public static IEnumerator LoadConfig(XmlNode encountersXml, MissionConfig missionConfig) {
        bool untilEndTime = Misc.xmlBool(encountersXml.Attributes.GetNamedItem("untilEndTime"), false);
        bool untilQueueEmpty = Misc.xmlBool(encountersXml.Attributes.GetNamedItem("untilQueueEmpty"), true);

        XmlNodeList peopleNodes = encountersXml.SelectNodes("person");
        List<PersonInMissionConfig> people = new List<PersonInMissionConfig>();
        foreach (XmlNode personNode in peopleNodes) {
            yield return PersonInMissionConfig.LoadConfig(personNode, people);
        }

        EncountersConfig encountersConfig = new EncountersConfig(untilEndTime, untilQueueEmpty, people);
        
        missionConfig.encountersConfig = encountersConfig;

        yield return null;
    }
}