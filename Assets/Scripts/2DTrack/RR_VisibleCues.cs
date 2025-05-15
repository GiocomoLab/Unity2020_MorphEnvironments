using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Data;
using Mono.Data.Sqlite;

public class RR_VisibleCues : MonoBehaviour
{
    // References to other components
    private SP_2DTrack sp;
    private TrialBlocks_2DTrack tb;

    // Controls how far the camera/object moves each frame
    public float stepSize = 1;

    private IDbConnection _connection;
    private IDbCommand _command;

    private int starVisible_l = 0;
    private int circleVisible_l = 0;
    private int diamondVisible_l = 0;
    private int triangleVisible_l = 0;

    private int starVisible_c = 0;
    private int circleVisible_c = 0;
    private int diamondVisible_c = 0;
    private int triangleVisible_c = 0;

    private int starVisible_r = 0;
    private int circleVisible_r = 0;
    private int diamondVisible_r = 0;
    private int triangleVisible_r = 0;

    private GameObject player;
    private GameObject star;
    private GameObject circle;
    private GameObject diamond;
    private GameObject triangle;
    private Camera rightcam;
    private Camera centercam;
    private Camera leftcam;
    private GameObject subCam0;
    private GameObject subCam1;
    private GameObject subCam2;

    private float true_delta_z = 0;

    void Start(){

        // Find objects in the scene
        player = GameObject.Find("Player");
        star = GameObject.Find("Star");
        circle = GameObject.Find("Circle");
        diamond = GameObject.Find("Diamond");
        triangle = GameObject.Find("Triangle");

        subCam0 = GameObject.Find("subCam0");
        subCam1 = GameObject.Find("subCam1");
        subCam2 = GameObject.Find("subCam2");

        rightcam = subCam0.GetComponent<Camera>();
        centercam = subCam1.GetComponent<Camera>();
        leftcam = subCam2.GetComponent<Camera>();

        // Get required components from the player object
        sp = player.GetComponent<SP_2DTrack>();
        tb = player.GetComponent<TrialBlocks_2DTrack>();

        // Open sql connection
        SqliteConnection.CreateFile(sp.localDirectory_pre + sp.sceneName + "_cues.db");
        _connection = new SqliteConnection("Data Source=" + sp.localDirectory_pre + sp.sceneName + "_cues.db;Version=3;");
        _connection.Open();
        _command = _connection.CreateCommand();
        _command.CommandText = "create table cues (angle INT, posx REAL, posz REAL, star_left INT, circle_left INT, diamond_left INT, triangle_left INT, star_center INT, circle_center INT, diamond_center INT, triangle_center INT, star_right INT, circle_right INT, diamond_right INT, triangle_right INT)";
        _command.ExecuteNonQuery();

    }

    void LateUpdate(){
        // Stop playing if number of traversals is greater than 0
        if (sp.numTraversals > 360){
             UnityEditor.EditorApplication.isPlaying = false;
        }

        starVisible_l = CheckVisibility(star, leftcam);
        circleVisible_l = CheckVisibility(circle, leftcam);
        diamondVisible_l = CheckVisibility(diamond, leftcam);
        triangleVisible_l = CheckVisibility(triangle, leftcam);

        starVisible_c = CheckVisibility2(star, centercam);
        circleVisible_c = CheckVisibility2(circle, centercam);
        diamondVisible_c = CheckVisibility2(diamond, centercam);
        triangleVisible_c = CheckVisibility2(triangle, centercam);

        starVisible_r = CheckVisibility(star, rightcam);
        circleVisible_r = CheckVisibility(circle, rightcam);
        diamondVisible_r = CheckVisibility(diamond, rightcam);
        triangleVisible_r = CheckVisibility(triangle, rightcam);

        _command.CommandText = "insert into cues (angle, posx, posz, star_left, circle_left, diamond_left, triangle_left, star_center, circle_center, diamond_center, triangle_center, star_right, circle_right, diamond_right, triangle_right) values (" + tb.trialAnglesList[sp.numTraversals] + "," + transform.position.x + "," + transform.position.z + ", " + starVisible_l + ", " + circleVisible_l + ", " + diamondVisible_l + ", " + triangleVisible_l + ", " + starVisible_c + ", " + circleVisible_c + ", " + diamondVisible_c + ", " + triangleVisible_c + ", " + starVisible_r + ", " + circleVisible_r + ", " + diamondVisible_r + ", " + triangleVisible_r + ")";
        _command.ExecuteNonQuery();

        // Move the object forward by stepSize amount
        transform.position += transform.forward * stepSize;
    }

    int CheckVisibility(GameObject obj, Camera camz)
    {
        int onScreen;

        Vector3 viewPos = camz.WorldToViewportPoint(obj.transform.position);
        Debug.Log(viewPos);

        if (viewPos.x > 0f && viewPos.x <= 1 && viewPos.y > 0f && viewPos.y <= 1 && viewPos.z < 0)
        {
            onScreen = 1;
        }
        else
        {
            onScreen = 0;
        }

        if (onScreen == 1)
        {
            return 1;
        }
        else
        {
            return 0;
        }

        /*
        Vector3 screenPos = panoCamera.GetComponent<Camera>().WorldToScreenPoint(obj.transform.position);
        Debug.Log(screenPos);

        if (screenPos.x > 0f && screenPos.x < Screen.width && screenPos.y > 0f && screenPos.y < Screen.height)
        {
            onScreen = 1;
        }
        else
        {
            onScreen = 0;
        }

        if (onScreen == 1 && obj.GetComponent<Renderer>().isVisible)
        {
            return 1;
        }
        else
        {
            return 0;
        }
        */
    }


    int CheckVisibility2(GameObject obj, Camera camz)
    {
        int onScreen;

        Vector3 viewPos = camz.WorldToViewportPoint(obj.transform.position);
        Debug.Log(viewPos);

        if (viewPos.x > 0f && viewPos.x <= 1 && viewPos.y > 0f && viewPos.y <= 1 && viewPos.z > 0)
        {
            onScreen = 1;
        }
        else
        {
            onScreen = 0;
        }

        if (onScreen == 1)
        {
            return 1;
        }
        else
        {
            return 0;
        }

        /*
        Vector3 screenPos = panoCamera.GetComponent<Camera>().WorldToScreenPoint(obj.transform.position);
        Debug.Log(screenPos);

        if (screenPos.x > 0f && screenPos.x < Screen.width && screenPos.y > 0f && screenPos.y < Screen.height)
        {
            onScreen = 1;
        }
        else
        {
            onScreen = 0;
        }

        if (onScreen == 1 && obj.GetComponent<Renderer>().isVisible)
        {
            return 1;
        }
        else
        {
            return 0;
        }
        */
    }

    void OnApplicationQuit()
    {
        _command.Dispose();
        _command = null;

        _connection.Close();
        _connection = null;

    }
}