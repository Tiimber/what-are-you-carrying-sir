using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DigitalAlarmClock : Clock {

    public DigitalNumber tenHour;
    public DigitalNumber hour;
    public DigitalColon colon;
    public DigitalNumber tenMinute;
    public DigitalNumber minute;
    
    // [ContextMenu("Update Time (Alarm)")]
    public override void doTime() {
        char[] chars = time.Replace(":", "").ToCharArray();
        tenHour.setNumber(Convert.ToInt32(chars[0].ToString()));
        hour.setNumber(Convert.ToInt32(chars[1].ToString()));
        tenMinute.setNumber(Convert.ToInt32(chars[2].ToString()));
        minute.setNumber(Convert.ToInt32(chars[3].ToString()));
    }
    
    public override void setClockPosition(ClockPosition position) {
        transform.position = position.position;
        transform.rotation = Quaternion.Euler(position.rotation);
        transform.localScale = position.scale;
    }
    
    // Start is called before the first frame update
    void Start() {
        base.Start();
    }

    // Update is called once per frame
    void Update() {
        
    }
}
