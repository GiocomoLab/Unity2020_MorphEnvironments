using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class grabFrames : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        ScreenCapture.CaptureScreenshot("C://Users//markp//Desktop//test.png");

    }

    void OnMouseDown ()
    {
       // ScreenCapture.CaptureScreenshot("C:\\Users\\markp\\Desktop\\test.png");
    }
}
