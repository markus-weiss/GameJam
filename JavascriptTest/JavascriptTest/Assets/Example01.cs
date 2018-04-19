using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;


using System.Runtime.InteropServices;

public class Example01 : MonoBehaviour {

    [DllImport("__Internal")]
    private static extern void CallToConsole(float x, float y, float z);
    //[DllImport("__Internal")]
    // private static extern void TestFkt(float x, float y, float z);

    public Text t;

    private void Start()
    {
        //UnityEvent.AddListener();
    }

    public void SetAlpha(float x)
    {
        Debug.Log("This is the testfkt");

        Debug.Log("alpha " + x);

        t.text = "alpha" + x;
    }

    public void SetAlpha(string x)
    {
        Debug.Log("This is the testfkt");

        Debug.Log("alpha " + x);

        t.text = "alpha" + x;
    }
    
    /*
    public void TestFkt()
    {
        float x = Input.gyro.rotationRate.x;
        float y = Input.gyro.rotationRate.y;
        float z = Input.gyro.rotationRate.z;

        Debug.Log("This is the testfkt");

        Debug.Log(" " + x + " " + y + " " + z);

        t.text = " " + x + " " + y + " " + z;
    }
    */

    private void Update()
    {
        float x = Input.gyro.rotationRate.x;
        float y = Input.gyro.rotationRate.y;
        float z = Input.gyro.rotationRate.z;

        //t.text = " " + x + " " + y + " " + z;

        //TestFkt(x, y, z);
        CallToConsole(x , y , z);

    }
}
