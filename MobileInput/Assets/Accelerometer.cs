using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Accelerometer : MonoBehaviour {

    private Rigidbody rigid;
    public bool isFlat = true;

    private void Start()
    {
        rigid = GetComponent<Rigidbody>();

        if (SystemInfo.supportsAccelerometer)
        {
            Debug.Log("Supported");
        }
    }

    private void Update()
    {
        

        unityAccelerate();
        //tutAccelerate();

    }

    void tutAccelerate()
    {
        // Beschleunigt den Ridgidbody anhand der letzten gemessenen bescheunigungrichtung
        // hier muss man die beschleunigung druch eine entsprechende Gegenbewegung ausgleichen 
        Vector3 tilt = Input.acceleration;
        if (isFlat)
        {
            tilt = Quaternion.Euler(90, 0, 0) * tilt;
        }
        rigid.AddForce(tilt);
        Debug.DrawRay(transform.position + Vector3.up, tilt, Color.cyan);
    }

    void unityAccelerate()
    {
        // Eher wie Gyro die letzte gemessene beschleunigung wird zur Bewegungsrichung und beschleunigung
        // Kann aprupt abgebrochen werden
        Vector3 dir = Vector3.zero;
        dir.x = Input.acceleration.y;
        dir.z = -Input.acceleration.x;
        if (dir.sqrMagnitude > 1)
            dir.Normalize();

        dir *= Time.deltaTime;
        transform.Translate(dir * 10f);

        print("dir" + dir);
    }
}

//rigid.AddForce(Input.acceleration);

