using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyController : MonoBehaviour {

    float shove = 0;
    public float acceleration = 0.1f;

    private void Start()
    {
        
    }

    private void Update()
    {
        /*
        if (shove > 0)
        {
            shove = 0;
        }
        else if (shove < 1)
        {
            shove = 1;
        }
        */

        if (Input.KeyPressed(KeyCode.W))
        {
            shove += acceleration;
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            shove -= acceleration;
        }

        Debug.Log(shove);
    }
}
