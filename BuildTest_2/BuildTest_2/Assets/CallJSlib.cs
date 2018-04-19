using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using UnityEngine.UI;

public class CallJSlib : MonoBehaviour {

    [DllImport("__Internal")]
    private static extern void Hello();

    [DllImport("__Internal")]
    private static extern void HelloString(string str);

    [DllImport("__Internal")]
    private static extern void PrintFloatArray(float[] array, int size);

    [DllImport("__Internal")]
    private static extern int AddNumbers(int x, int y);

    [DllImport("__Internal")]
    private static extern string StringReturnValueFunction();

    [DllImport("__Internal")]
    private static extern void BindWebGLTexture(int texture);

    void Start()
    {
        HelloString("This is a string." + Input.gyro.rotationRate);

        float[] myArray = new float[10];
        PrintFloatArray(myArray, myArray.Length);

    }
    int a = 1;
    public void MyFunction(int _a)
    {
        a = _a;
        Debug.Log(a);
    }

    void OnGUI()
    {       
        GUI.Label(new Rect(10, 10, 100, 20), "RotationRate: " + Input.gyro.rotationRate);
        GUI.Label(new Rect(150, 10, 100, 20), "Attitude: " + Input.gyro.attitude);
        GUI.Label(new Rect(300, 10, 100, 20), "Enabled: " + Input.gyro.enabled);
        GUI.Label(new Rect(400, 10, 100, 20), "Enabled: " + a);

        //GUI.Label(new Rect(10, 50, 100, 20), "X: " + Input.acceleration.x);
        //GUI.Label(new Rect(10, 70, 100, 20), "Y: " + Input.acceleration.y);
        //GUI.Label(new Rect(10, 90, 100, 20), "Z: " + Input.acceleration.z);
    }

    private void Update()
    {
        Text t = GetComponent<Text>();
        t.text = Input.acceleration.x + " : " + Input.acceleration.z;
        Debug.Log(Input.acceleration.x + " : " + Input.acceleration.z);
        string s = Input.acceleration.x + " : " + Input.acceleration.z;
    }
}
