using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerCounter : MonoBehaviour {
    // Make TriggerCounter return zero if its count was increased from zero and minimum delay has not passed since
    public float MinimumDelay = 0f;

    private int pCount = 0;
    private float pTime, pLastTime;

    // Return count if MinimumDelay seconds have passed since the first OnTriggerEnter that raised count to one
    public int GetCountFirst() {
        return pTime + MinimumDelay <= Time.time ? pCount: 0;
    }

    // Return count if MinimumDelay seconds have passed since the last OnTriggerEnter
    public int GetCountLast() {
        return pLastTime + MinimumDelay <= Time.time ? pCount : 0;
    }

    // Force counter back to zero
    public void ForceReset() {
        pCount = 0;
    }

    // Add one to count
    void OnTriggerEnter(Collider other) {
        if (pCount < 1) {
            pTime = Time.time;
        }

        pLastTime = Time.time;
        pCount++;
    }

    // Subtract one from count if count is larger than one
    void OnTriggerExit(Collider other) {
        pCount = Mathf.Max(0, pCount - 1);
    }
}
