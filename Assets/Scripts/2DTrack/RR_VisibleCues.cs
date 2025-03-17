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

    private int starVisible = 0;
    private int circleVisible = 0;
    private int diamondVisible = 0;
    private int triangleVisible = 0;

    private GameObject player;
    private GameObject panoCamera;
    private GameObject star;
    private GameObject circle;
    private GameObject diamond;
    private GameObject triangle;

    private float true_delta_z = 0;

    void Start(){

        // Find objects in the scene
        player = GameObject.Find("Player");
        panoCamera = GameObject.Find("panoCamera");
        star = GameObject.Find("Star");
        circle = GameObject.Find("Circle");
        diamond = GameObject.Find("Diamond");
        triangle = GameObject.Find("Triangle");
        
        // Get required components from the player object
        sp = player.GetComponent<SP_2DTrack>();
        tb = player.GetComponent<TrialBlocks_2DTrack>();

        // Open sql connection
        SqliteConnection.CreateFile(sp.serverDirectory_pre + sp.sceneName + "_cues.db");
        _connection = new SqliteConnection("Data Source=" + sp.serverDirectory_pre + sp.sceneName + "_cues.db;Version=3;");
        _connection.Open();
        _command = _connection.CreateCommand();
        _command.CommandText = "create table cues (angle INT, posx REAL, posz REAL, star INT, circle INT, diamond INT, triangle INT)";
        _command.ExecuteNonQuery();
    }

    void LateUpdate(){
        // Stop playing if number of traversals is greater than 0
        if (sp.numTraversals > 360){
             UnityEditor.EditorApplication.isPlaying = false;
        }

        starVisible = CheckVisibility(star);
        circleVisible = CheckVisibility(circle);
        diamondVisible = CheckVisibility(diamond);
        triangleVisible = CheckVisibility(triangle);

        _command.CommandText = "insert into cues (angle, posx, posz, star, circle, diamond, triangle) values (" + tb.trialAnglesList[sp.numTraversals] + "," + transform.position.x + "," + transform.position.z + ", " + starVisible + ", " + circleVisible + ", " + diamondVisible + ", " + triangleVisible + ")";
        _command.ExecuteNonQuery();

        // Move the object forward by stepSize amount
        transform.position += transform.forward * stepSize;
    }

    int CheckVisibility(GameObject obj)
    {
        int onScreen;
        Vector3 screenPos = panoCamera.GetComponent<Camera>().WorldToScreenPoint(obj.transform.position);

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
    }  

    void OnApplicationQuit()
    {
        _command.Dispose();
        _command = null;

        _connection.Close();
        _connection = null;

    }
}