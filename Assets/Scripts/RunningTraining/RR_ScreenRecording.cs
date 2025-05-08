using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.IO.Ports;
using System.Threading;

// This class handles screen recording functionality in Unity by taking sequential screenshots
// while moving the camera/object along the Z axis
public class RR_ScreenRecording : MonoBehaviour
{
    // References to other components
    private SP_RunTrain sp;
    
    // Controls how far the camera/object moves each frame
    public float stepSize = 1;
    
    // Keeps track of the current frame number for screenshot naming
    public int frameNumber = 0;
    
    // Stores the initial Z position
    public float zPos;

    public float true_delta_z = 0;

    void Start(){
        // Find the Player object in the scene
        GameObject player = GameObject.Find("Player");
        
        // Get required components from the player object
        sp = player.GetComponent<SP_RunTrain>();

        // Store initial Z position
        zPos = transform.position.z;
    }

    void Update(){
        // Stop playing if number of traversals is greater than 0
        if (sp.numTraversals > 0){
             UnityEditor.EditorApplication.isPlaying = false;
        }

        // Take a screenshot each frame
        StartCoroutine(TakeScreenShot());
        Debug.Log(zPos);
        frameNumber = frameNumber + 1;

        // Move the object forward along Z axis by stepSize amount
        Vector3 movement = new Vector3(0.0f, 0.0f, stepSize);
        transform.position = transform.position + movement;
    }

    // Coroutine to capture screenshots
    // Waits for end of frame to ensure UI and everything is rendered
    // Saves screenshot with frame number in the filename
    IEnumerator TakeScreenShot(){
        yield return new WaitForEndOfFrame();
        ScreenCapture.CaptureScreenshot("screenshot_" + frameNumber + ".png");
    }
}