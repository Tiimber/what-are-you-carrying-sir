using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public abstract class Clock : MonoBehaviour {
    public bool setRandomPosition;
    public int clockPosition;
    
    public String time;
    public String notifyTime;
    public int speed;
    
    private Coroutine timePassing;

    public abstract void setClockPosition(ClockPosition position);
    
    public abstract void doTime();
    
    public virtual void setTime(String newTime) {
        time = newTime;
        doTime();
        notifyIfEndTimePassed();
    }

    public void setTime(String startTime, float addMinutes) {
        int hours = Convert.ToInt32(startTime.Substring(0, 2), 10);
        int minutes = Convert.ToInt32(startTime.Substring(2, 2), 10);

        int totalMinutes = minutes + (int) addMinutes;
        int totalHours = hours + (totalMinutes - (totalMinutes % 60)) / 60;
        totalMinutes %= 60;
        
        string newTime = totalHours.ToString("00") + ":" + totalMinutes.ToString("00");
        setTime(newTime);
    }
    
    public void notifyIfEndTimePassed() {
        if (notifyTime != null) {
            bool endTimePassed = String.Compare(notifyTime, time, StringComparison.Ordinal) <= 0;
            if (endTimePassed) {
                PubSub.publish("MISSION_TIME_END");
                notifyTime = null;
            }
        }
    }

    public void notifyOnTime(String notifyTime) {
        this.notifyTime = notifyTime;
    }
    
    public void Start() {
        ClockPositions clockPositions = GetComponent<ClockPositions>();
        if (setRandomPosition) {
            clockPosition = ItsRandom.randomRange(0, clockPositions.positions.Length);
        }

        ClockPosition clockPositionObj = clockPositions.positions[clockPosition];
        setClockPosition(clockPositionObj);
        StartCoroutine(calculateTime());
    }

    private IEnumerator calculateTime() {
        float timeAtClockStart = Time.time;
        string startTime = time.Replace(":", "");
        while (true) {
            if (Game.paused) {
                timeAtClockStart += Time.deltaTime;
            } else {
                float currentTimeElapsedInSeconds = Time.time - timeAtClockStart;
                float currentTimeElapsedInClock = (currentTimeElapsedInSeconds / 60F) * speed;
                setTime(startTime, currentTimeElapsedInClock);
            }
            yield return new WaitForSeconds(0.5F);
        }
    }

    public void OnDestroy() {
        StopAllCoroutines();
    }
}
