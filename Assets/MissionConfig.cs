using System;
using System.Collections;
using System.Xml;
using UnityEngine;

public class MissionConfig {

    public static MissionConfig Instance;
    
    public String id;
    public String name;
    public String description;
    public String location;
    public String icon;

    public String startTime;
    public String endTime;
    public String overtime;
    public int timeSpeed;
    public String clockType;
    public int clockPosition;
    
    public String difficulty;
    public String stars3;
    public String stars2;
    public String stars1;
    
    public String failCondition;

    public EncountersConfig encountersConfig;
    public SeedsConfig seedsConfig;

    public MissionConfig(string id, string name, string description, string location, string icon, string startTime, string endTime, string overtime, int timeSpeed, string clockType, int clockPosition, string difficulty, string stars3, string stars2, string stars1, string failCondition) {
        this.id = id;
        this.name = name;
        this.description = description;
        this.location = location;
        this.icon = icon;
        this.startTime = startTime;
        this.endTime = endTime;
        this.overtime = overtime;
        this.timeSpeed = timeSpeed;
        this.clockType = clockType;
        this.clockPosition = clockPosition;
        this.difficulty = difficulty;
        this.stars3 = stars3;
        this.stars2 = stars2;
        this.stars1 = stars1;
        this.failCondition = failCondition;
    }

    public static IEnumerator LoadConfig(XmlNode missionXml) {
        XmlAttributeCollection missionAttributes = missionXml.Attributes;
        String id = Misc.xmlString(missionAttributes.GetNamedItem("id"));
        String name = Misc.xmlString(missionAttributes.GetNamedItem("name"));
        String description = Misc.xmlString(missionAttributes.GetNamedItem("description"));
        String location = Misc.xmlString(missionAttributes.GetNamedItem("location"));
        String icon = Misc.xmlString(missionAttributes.GetNamedItem("icon"));
        
        String startTime = Misc.xmlString(missionAttributes.GetNamedItem("startTime"));
        String endTime = Misc.xmlString(missionAttributes.GetNamedItem("endTime"));
        String overtime = Misc.xmlString(missionAttributes.GetNamedItem("overtime"));
        int timeSpeed = Misc.xmlInt(missionAttributes.GetNamedItem("timeSpeed"), 1);
        String clockType = Misc.xmlString(missionAttributes.GetNamedItem("clockType"));
        int clockPosition = Misc.xmlInt(missionAttributes.GetNamedItem("clockPosition"), 0);

        String difficulty = Misc.xmlString(missionAttributes.GetNamedItem("difficulty"));
        String stars3 = Misc.xmlString(missionAttributes.GetNamedItem("stars3"));
        String stars2 = Misc.xmlString(missionAttributes.GetNamedItem("stars2"));
        String stars1 = Misc.xmlString(missionAttributes.GetNamedItem("stars1"));
        
        String failCondition = Misc.xmlString(missionAttributes.GetNamedItem("failCondition"));

        MissionConfig missionConfig = new MissionConfig(id, name, description, location,
                                                        icon, startTime, endTime, overtime, timeSpeed,
                                                        clockType, clockPosition, difficulty, stars3,
                                                        stars2, stars1, failCondition);

        XmlNode seeds = missionXml.SelectSingleNode("seeds");
        yield return SeedsConfig.LoadConfig(seeds, missionConfig);

        XmlNode encounters = missionXml.SelectSingleNode("encounters");
        yield return EncountersConfig.LoadConfig(encounters, missionConfig);
        
        Debug.Log("Mission loaded:");
        Debug.Log("Location: " + missionConfig.location + " " + missionConfig.startTime + " (" + missionConfig.clockType + ", " + missionConfig.clockPosition + ")");
        Debug.Log("Seeds: " + missionConfig.seedsConfig.bags.Count + " " + missionConfig.seedsConfig.people.Count);
        Debug.Log("Encounters: " + missionConfig.encountersConfig.people.Count);

        Instance = missionConfig;
    }
}