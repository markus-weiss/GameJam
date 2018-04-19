using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public class CallJSlib : MonoBehaviour {

    [DllImport("__Internal")]
    private static extern void CallGyroSensor();

    // Use this for initialization
    void Start () {
        //CallGyroSensor();
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
