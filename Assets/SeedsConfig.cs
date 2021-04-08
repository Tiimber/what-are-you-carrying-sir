using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEngine;

public class SeedsConfig {
    public List<int> people;
    public List<int> bags;

    public SeedsConfig(List<int> people, List<int> bags) {
        this.people = people;
        this.bags = bags;
    }

    public static IEnumerator LoadConfig(XmlNode seedsXml, MissionConfig missionConfig) {
        XmlNode peopleNode = seedsXml.SelectSingleNode("people");
        List<int> people = Misc.splitInts(peopleNode.InnerText);

        XmlNode bagsNode = seedsXml.SelectSingleNode("bags");
        List<int> bags = Misc.splitInts(bagsNode.InnerText);

        SeedsConfig seedsConfig = new SeedsConfig(people, bags);
        
        missionConfig.seedsConfig = seedsConfig;

        yield return null;
    }
}