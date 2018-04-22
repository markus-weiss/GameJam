using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CalledFromBrowser : MonoBehaviour {

    public float alpha;
    public float beta;
    public float gamma;

    public void SetAlpha(float a)
    {
        alpha = a;
    }

    public void SetBeta(float b)
    {
        beta = b;
    }

    public void SetGamma(float g)
    {
        gamma = g;
    }

    private void Update()
    {
        Debug.Log("Alpha: " + alpha + " Beta: "  + beta + " Gamma: "  + gamma);
    }
}
