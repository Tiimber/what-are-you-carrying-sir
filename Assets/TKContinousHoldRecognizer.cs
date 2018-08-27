using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class TKContinousHoldRecognizer : TKAbstractGestureRecognizer {

	public event Action<TKContinousHoldRecognizer> gestureHoldingEvent;

    // touches that last shorter than this duration will be ignored
    float _maxDurationForTapConsideration = 0.5f;

    float startTime;
	Coroutine continousHoldCoroutine;

    public int touchCount;

    internal override void fireRecognizedEvent() {}

    void fireHoldingEvent() {
        if(gestureHoldingEvent != null) {
            gestureHoldingEvent(this);
        }
    }

    internal override bool touchesBegan(List<TKTouch> touches) {
        touchCount = touches.Count;
        if (touches.Count == 1 || touches.Count == 2) {
            startTime = Time.time;
            continousHoldCoroutine = Singleton<SingletonInstance>.Instance.StartCoroutine (CheckContinousHold(this));
            _trackingTouches.AddRange(touches);
            state = TKGestureRecognizerState.Began;
        }
        return false;
    }

    internal override void touchesMoved(List<TKTouch> touches) {
    }

    internal override void touchesEnded(List<TKTouch> touches) {
        startTime = float.MaxValue;
        Singleton<SingletonInstance>.Instance.StopCoroutine(continousHoldCoroutine);
        state = TKGestureRecognizerState.FailedOrEnded;
    }

    private static IEnumerator CheckContinousHold (TKContinousHoldRecognizer holdRecognizer) {
        while (holdRecognizer.startTime <= Time.time) {
            yield return null;
//            Debug.Log("running");
            if (Time.time > holdRecognizer.startTime + holdRecognizer._maxDurationForTapConsideration) {
                holdRecognizer.fireHoldingEvent();
            }
        }
    }

}
