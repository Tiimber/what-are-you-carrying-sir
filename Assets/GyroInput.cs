using UnityEngine;
using System.Collections;

public class GyroInput : MonoBehaviour
{
//    Quaternion offset;

    void OnEnable () {
//        offset = transform.rotation * Quaternion.Inverse(GyroToUnity(Input.gyro.attitude));
    }

    void OnDisable () {

    }

    void Update () {
//        this.transform.rotation = offset * GyroToUnity(Input.gyro.attitude);
        this.transform.rotation *= Quaternion.Euler(Input.gyro.rotationRate * 30f * Time.deltaTime);
    }

    private static Quaternion GyroToUnity(Quaternion q) {
        return new Quaternion(q.x, q.y, -q.z, -q.w);
    }
}