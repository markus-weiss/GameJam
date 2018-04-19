using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public class callJs : MonoBehaviour {

    [DllImport("__Internal")]
    private static extern void CallGyroSensor();
    [DllImport("__Internal")]
    private static extern void GetSensorInput(float a);
    [DllImport("__Internal")]
    private static extern void PrintFloatArray(float[] array, int size);

    float speed = 10f;

    void Start()
    {
        CallGyroSensor();

        GetSensorInput(1);

        float[] myArray = new float[10];
        PrintFloatArray(myArray, myArray.Length);

        //CallGyroSensor();

        //GetSensorInput(a,b);
        /*
        Vector3 dir = Vector3.zero;
        dir.x = -Input.acceleration.y;
        dir.z = Input.acceleration.x;
        if (dir.sqrMagnitude > 1)
            dir.Normalize();

        dir *= Time.deltaTime;
        transform.Translate(dir * speed);
        */
        //CallGyroSensor(dir.x, dir.y);
    }

    private void Update()
    {
        
    }
}
