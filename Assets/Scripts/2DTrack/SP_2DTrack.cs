using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System.IO;
using System;
using System.Data;
using Mono.Data.Sqlite;


public class SP_2DTrack : MonoBehaviour
{


    private string mouse;

    public float SkipTrialPcnt = 0.0f;

    public bool AutoReward = false;
    private int _autoReward = 0;
    public bool BlankLaser = false;

    public float autoRewardPercent = 0.0f;

    public int DreamLand = 0;


    public int numRewards = 0;
    public int numRewards_manual = 0;
    public int rewardFlag = 0;

    public int numTraversals = 0;
    public int numTrialsTotal;
    public int maxRewards = 200;


    public float morph = 0.0f;
    public float rDur = 2;

    // for saving data
    public string localDirectory_pre = "C:/Users/markp/VR_Data/Michelle/";
    public string serverDirectory_pre = "I:/My Drive/VR_Data/";
    public string localDirectory;
    public string serverDirectory;
    public string localPrefix;
    public string serverPrefix;
    public string sceneName;

    public int session = 1;


    private DateTime today;
    private GameObject player;

    //private TrialOrdering_NeuroMods tott;
    private RR_2DTrack rr;
    private DL_2DTrack dl;
    private PC_2DTrack pc;
    private SbxTTLs_2DTrack ttls;
    private Notes notes;
    private TrialBlocks_2DTrack tb;


    private IDbConnection _connection;
    private IDbCommand _command;



    public int scanning = 0;

    public void Awake()
    {

        player = GameObject.Find("Player");
        sceneName = SceneManager.GetActiveScene().name;
        Debug.Log(sceneName);


        rr = player.GetComponent<RR_2DTrack>();
        dl = player.GetComponent<DL_2DTrack>();
        pc = player.GetComponent<PC_2DTrack>();
        ttls = player.GetComponent<SbxTTLs_2DTrack>();
        notes = player.GetComponent<Notes>();
        mouse = notes.mouse;
        tb = player.GetComponent<TrialBlocks_2DTrack>();

        today = DateTime.Today;
        Debug.Log(today.ToString("yyyy_MM_dd"));



        localDirectory = localDirectory_pre + mouse + '/' + today.ToString("yyyy_MM_dd") + '/';
        serverDirectory = serverDirectory_pre + mouse + '/' + today.ToString("yyyy_MM_dd") + '/';

        try
        {
            if (!Directory.Exists(localDirectory))
            {
                Directory.CreateDirectory(localDirectory);
            }
        }
        catch (DirectoryNotFoundException ex)
        {
            Debug.LogError("CRITICAL ERROR: Local directory path not found: " + localDirectory);
            Debug.LogError("Exception: " + ex.Message);
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
            return;
        }

        try
        {
            if (!Directory.Exists(serverDirectory))
            {
                Directory.CreateDirectory(serverDirectory);
            }
        }
        catch (DirectoryNotFoundException ex)
        {
            Debug.LogError("CRITICAL ERROR: Server directory path not found: " + serverDirectory);
            Debug.LogError("Exception: " + ex.Message);
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
            return;
        }





        bool nameFlag = true;
        while (nameFlag)
        {
            localPrefix = localDirectory + "/" + sceneName + "_" + session.ToString();
            serverPrefix = serverDirectory + "/" + sceneName + "_" + session.ToString();
            if (File.Exists(localPrefix + ".sqlite"))
            {
                session++;
            } else
            {
                nameFlag = false;
                SqliteConnection.CreateFile(localPrefix + ".sqlite");
            }
        }

        string connectionString = "Data Source=" + localPrefix + ".sqlite;Version=3;";
        _connection = (IDbConnection) new SqliteConnection(connectionString);
        _connection.Open();
        _command = _connection.CreateCommand();
        _command.CommandText = "create table data (time REAL, trialnum INT, startangle REAL, posx REAL, posz REAL, dz REAL, lick INT, reward INT, tstart INT, teleport INT, rzone INT, scanning NUMERIC, manrewards INT, autoreward INT, cmd INT)";
        
        _command.ExecuteNonQuery();

        // make table for session information

        // trial type numbers
        _command.CommandText = "create table trialInfo (baseline INT, training INT, test INT)";
        _command.ExecuteNonQuery();

       
    }

    void LateUpdate() {

        if (pc.isQuitting) return;

        if (AutoReward)
        {
            _autoReward = 1;
        }
        else
        {
            _autoReward = 0;
        }
        


        _command.CommandText = "insert into data (time, trialnum, startangle, posx, posz, dz, lick, reward, tstart, teleport, rzone, scanning, manrewards, autoreward, cmd) values (" + Time.realtimeSinceStartup + "," + numTraversals + "," + tb.trialAnglesList[numTraversals] + "," + transform.position.x + "," + transform.position.z + "," + rr.true_delta_z + "," + dl.c_1 + "," + dl.r + "," + pc.tstartFlag + "," + pc.tendFlag + "," + pc.rzoneFlag + ","  + ttls.scanning + "," + pc.mRewardFlag + "," + _autoReward + "," + pc.cmd + ")";
        _command.ExecuteNonQuery();

    }

    void OnApplicationQuit()
    {
        _command.Dispose();
        _command = null;

        _connection.Close();
        _connection = null;

        File.Copy(localPrefix + ".sqlite", serverPrefix + ".sqlite",true);

        string sess_connectionString = "Data Source=" + serverDirectory_pre + "behavior_sessions.db;Version=3;";
        IDbConnection db_connection;
        db_connection = (IDbConnection) new SqliteConnection(sess_connectionString);
        db_connection.Open();
        IDbCommand db_command = db_connection.CreateCommand();

        string tmp_date = today.ToString("yyyy_MM_dd");
        db_command.CommandText = "insert into sessions (Mouse, Date, Scene, Session, Rewards, Trials, Imaging) values ('" + mouse + "','" + tmp_date + "','" + sceneName + "'," + session + "," + numRewards + "," + numTraversals + "," + scanning + ")";
        db_command.ExecuteNonQuery();

        Debug.Log(db_command.CommandText);

        db_command.Dispose();
        db_command = null;

        db_connection.Close();
        db_connection = null;


    }

}
