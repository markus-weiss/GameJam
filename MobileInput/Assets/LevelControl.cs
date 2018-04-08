using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelControl : MonoBehaviour {

    public Button btn_restart;
    public Button btn_Gyro;
    public Button btn_Acce;

    public Camera cam;
    public GameObject Shp;    

    // Use this for initialization
    void Start () {

        btn_restart.onClick.AddListener(restart);
        btn_Gyro.onClick.AddListener(setGyroActive);
        btn_Acce.onClick.AddListener(setAcceleraActive);

    }

    void restart()
    {
        SceneManager.LoadScene("test_1", LoadSceneMode.Single);
    }

    void setGyroActive()
    {
        cam.GetComponent<GyroControl>().enabled = true;
        Shp.GetComponent<Accelerometer>().enabled = false;
    } 

    void setAcceleraActive()
    {
        cam.GetComponent<GyroControl>().enabled = false;
        Shp.GetComponent<Accelerometer>().enabled = true;
    }
	
	// Update is called once per frame
	void Update () {
		 
	}
}
