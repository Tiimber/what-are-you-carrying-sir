using System.Collections;
using System.Collections.Generic;
using Obi;
using UnityEngine;

public class FillBottleInspectAction : MonoBehaviour, ActionOnInspect {
    public float delay;

    public void run(bool reverse) {
        if (!reverse) {
            Singleton<SingletonInstance>.Instance.StartCoroutine (FillWithLiquidAfterDelay(gameObject, 1f));
        } else {
            PillBottle pillBottle = gameObject.GetComponent<PillBottle>();
            pillBottle.emptyLiquid();
        }
    }

    private static IEnumerator FillWithLiquidAfterDelay(GameObject gameObject, float delay) {
        PillBottle pillBottle = gameObject.GetComponent<PillBottle>();
        if (pillBottle.liquidPrepared) {
            yield return new WaitForSeconds(delay);
            pillBottle.fillLiquid();
        }
    }
}
