using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class TKContinousHoldRecognizer : TKAbstractGestureRecognizer {

	public event Action<TKContinousHoldRecognizer> gestureHoldingEvent;
	public event Action<TKContinousHoldRecognizer> gestureHoldingEventEnded;

    // touches that last shorter than this duration will be ignored
    float _maxDurationForTapConsideration = 0.5f;

    float startTime;
	Coroutine continousHoldCoroutine;

    public int touchCount;
    public bool haveSentTouchEvent = false;

    public bool firstFrame = true;
    public Vector2 startPosition;
    public float maxTouchMovement;

    internal override void fireRecognizedEvent() {}

    void fireHoldingEvent() {
        haveSentTouchEvent = true;
        if(gestureHoldingEvent != null) {
            gestureHoldingEvent(this);
        }
        firstFrame = false;
    }

    void fireHoldingEventEnded() {
        if (haveSentTouchEvent && gestureHoldingEventEnded != null) {
            gestureHoldingEventEnded(this);
        }
    }

    internal override bool touchesBegan(List<TKTouch> touches) {
        firstFrame = true;
        maxTouchMovement = 0f;
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
        maxTouchMovement = Mathf.Max(maxTouchMovement, touches.OrderByDescending(t => t.deltaPosition.magnitude).ToList()[0].deltaPosition.magnitude * Time.unscaledDeltaTime);
//        Debug.Log(maxTouchMovement);
    }

    internal override void touchesEnded(List<TKTouch> touches) {
        startTime = float.MaxValue;
        Singleton<SingletonInstance>.Instance.StopCoroutine(continousHoldCoroutine);
        state = TKGestureRecognizerState.FailedOrEnded;
        fireHoldingEventEnded();
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
