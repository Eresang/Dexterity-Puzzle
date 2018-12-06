using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayfieldControl : MonoBehaviour {
    // Default floor
    public GameObject FloorObject;

    // Floor with dimple
    public GameObject HoleObject;

    // Ball object
    public GameObject BallObject;

    // Reset object
    public TriggerCounter ResetObject;

    // Light object
    public GameObject LightObject;

    // Indicator object
    public IndicatorControl Indicator;

    // Determines the size of the playfield - made private as change requires new assets
    private int Size = 3;
    private float Spacing = 2f;

    // Definitions of playfields using bitmasks
    private static readonly int[] PlayfieldLayoutValues = { 16, 68, 84, 325, 341, 365, 381, 495, 511 };

    // Record of balls, floor and hole objects
    private GameObject[] pPlayfieldObjects = new GameObject[0];
    private TriggerCounter[] pHoleTriggers = new TriggerCounter[0];
    private GameObject[] pBalls = new GameObject[0];

    // Tracking
    private int pLastPlayfield = -1;
    private int pHoles = -1;
    private bool pWaitingForReset;

    // Create a random playfield
    private void RandomPlayfield() {
        int lLayout = 0;

        // Prevent playfield repetition if there is more than one
        if (PlayfieldLayoutValues.Length > 1) {
            do {
                lLayout = Mathf.RoundToInt(Random.Range(0f, (float)PlayfieldLayoutValues.Length - 1f));
            } while (lLayout == pLastPlayfield);
        }

        SetPlayfield(lLayout);
    }

    // Create a playfield using a PlayfieldLayoutValues value
    private void SetPlayfield(int vLayout) {
        if ((vLayout < 0) || (vLayout >= PlayfieldLayoutValues.Length)) {
            Debug.Log("Invalid playfield layout identifier specified");
            return;
        }

        if (!(FloorObject)) {
            Debug.Log("FloorObject has not been assigned");
            return;
        }

        if (!(HoleObject)) {
            Debug.Log("HoleObject has not been assigned");
            return;
        } else if (!(HoleObject.GetComponentInChildren<TriggerCounter>())) {
            Debug.Log("HoleObject requires a child with a TriggerCounter component");
            return;
        }

        if (!(BallObject))
        {
            Debug.Log("BallObject has not been assigned");
            return;
        }

        if (!(ResetObject))
        {
            Debug.Log("ResetObject has not been assigned");
            return;
        }

        ResetObject.ForceReset();
        ResetObject.gameObject.SetActive(false);

        // Destroy previous tiles
        for (int i = 0; i < pPlayfieldObjects.Length; i++) {
            Destroy(pPlayfieldObjects[i]);
        }

        // Destroy previous balls
        for (int i = 0; i < pBalls.Length; i++)
        {
            Destroy(pBalls[i]);
        }

        // Create new tiles
        pPlayfieldObjects = new GameObject[Size * Size];

        float lHalfSize = 0.5f * (Size - 1);
        int lValue = PlayfieldLayoutValues[vLayout];

        pHoles = 0;

        for (int i = 0; i < Size * Size; i++) {
            Vector3 lPos = new Vector3(Mathf.Floor(i / Size) - lHalfSize, 0f, (float)(i % Size) - lHalfSize) * Spacing;

            // Check if bit at index i of lValue is set to determine if a HoleObject should be spawned
            if (((1 << i) & lValue) != 0) {
                pPlayfieldObjects[i] = Instantiate(HoleObject, lPos, Quaternion.identity);

                pHoles++;
            }
            else {
                pPlayfieldObjects[i] = Instantiate(FloorObject, lPos, Quaternion.identity);
            }

            // Make it look cleaner in the editor
            pPlayfieldObjects[i].gameObject.transform.SetParent(this.transform);
        }

        // Get all trigger counters
        pHoleTriggers = new TriggerCounter[pHoles];
        lValue = 0;

        for (int i = 0; i < pPlayfieldObjects.Length; i++) {
            TriggerCounter lTrigger = pPlayfieldObjects[i].GetComponentInChildren<TriggerCounter>();

            if (lTrigger) {
                pHoleTriggers[lValue] = lTrigger;
                lValue++;
            }
        }

        pBalls = new GameObject[pHoles];
        lHalfSize = -0.5f * (pHoles - 1);

        // Spawn a ball for each hole
        for (int i = 0; i < pHoles; i++) {
            Vector3 lPos = new Vector3(-0.5f * (Size - 1) - 3f, 0.5f, lHalfSize + i);

            pBalls[i] = Instantiate(BallObject, lPos, Quaternion.identity);
        }

        if (Indicator) {
            Indicator.SetWaiting();
        }

        pLastPlayfield = vLayout;
        pWaitingForReset = false;
    }

    // Initiate a playfield on start
    void Start()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        RandomPlayfield();
    }

    // Handle the game states
    // 1. Game state
    // 2. Victory/reset state
    void Update()
    {
        // Update gravity
        Physics.gravity = new Vector3(Input.acceleration.x, Input.acceleration.z, Input.acceleration.y) * 98.1f;

        // Update light
        if (LightObject) {
            Vector3 lPos = Vector3.zero;

            for (int i = 0; i < pBalls.Length; i++) {
                lPos += pBalls[i].transform.position * (1f / pBalls.Length);
            }

            lPos.y = LightObject.transform.position.y;
            LightObject.transform.position = lPos;
        }

        if (pWaitingForReset) {
            // Check if reset conditions are met, if so, create new play field
            if ((ResetObject) && (ResetObject.GetCountLast() == pHoles)) {
                RandomPlayfield();
            }
        } else {
            // Check if victory conditions are met
            int lCount = 0;

            for (int i = 0; i < pHoleTriggers.Length; i++) {
                lCount += Mathf.Min(1, pHoleTriggers[i].GetCountFirst());
            }

            // Mission accomplished
            if (lCount == pHoles) {
                pWaitingForReset = true;

                if (ResetObject) {
                    ResetObject.gameObject.SetActive(true);
                }

                if (Indicator)
                {
                    Indicator.SetSuccess();
                }
            }
        }
    }
}
