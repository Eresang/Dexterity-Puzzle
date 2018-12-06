using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IndicatorControl : MonoBehaviour {
    public Material WaitingMaterial;
    public Material SuccessMaterial;

	public void SetWaiting () {
        Renderer pRenderer = GetComponent<Renderer>();

        if (pRenderer) {
            pRenderer.material = WaitingMaterial;
        }
	}

    public void SetSuccess()
    {
        Renderer pRenderer = GetComponent<Renderer>();

        if (pRenderer) {
            pRenderer.material = SuccessMaterial;
        }
    }
}
