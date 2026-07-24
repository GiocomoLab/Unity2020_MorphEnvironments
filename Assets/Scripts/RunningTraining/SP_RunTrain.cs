using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System.IO;
using System;
using System.Data;
using Mono.Data.Sqlite;

public class SP_RunTrain : MonoBehaviour
{


    private string mouse;

    public bool AutoReward = true;
    public float mrd = 30.0f; // minimum reward distance
    public float ard = 10.0f; // additional reward distance
    public bool fixedRewardSchedule = false; // proportion of trials with towers on both sides
    public float MinTrainingDist = 10f;
    public float MaxTrainingDist = 300f;

    public int numRewards = 0;
    public int numRewards_manual = 0;
    public int rewardFlag = 0;
    private int _autoReward = 0;

    public int numTraversals = 0;
    public int numTrialsTotal;
    public int maxRewards = 100;

    public float rDur = 2f; // timeout duration between available rewards

    public bool MultiReward = true;
    // for saving data
    public string localDirectory_pre = "C:/Users/markp/VR_Data/Michelle/";
    public string serverDirectory_pre = "I:/My Drive/VR_Data/";
    public string localDirectory;
    public string serverDirectory;
    public string localPrefix;
    public string serverPrefix;
    public string sceneName;

    private GameObject player;
    private RR_RunTrain rr;
    private DL_RunTrain dl;
    private PC_RunTrain pc;
    private SbxTTLs_RunTrain ttls;
    private Notes notes;


    public int session;
    private DateTime today;
    private IDbConnection _connection;
    private IDbCommand _command;

    public int scanning = 0;


    public int dirCheck = 0;

    public void Awake()
    {

        player = GameObject.Find("Player");
        rr = player.GetComponent<RR_RunTrain>();
        dl = player.GetComponent<DL_RunTrain>();
        pc = player.GetComponent<PC_RunTrain>();
        ttls = player.GetComponent<SbxTTLs_RunTrain>();
        notes = player.GetComponent<Notes>();
        mouse = notes.mouse;

        today = DateTime.Today;
        Debug.Log(today.ToString("yyyy_MM_dd"));
        sceneName = SceneManager.GetActiveScene().name;
        localDirectory = localDirectory_pre + mouse + '/' + today.ToString("yyyy_MM_dd") + '/';
        serverDirectory = serverDirectory_pre + mouse + '/' + today.ToString("yyyy_MM_dd") + '/';
        if (!Directory.Exists(localDirectory))
        {
            Directory.CreateDirectory(localDirectory);
        }
        if (!Directory.Exists(serverDirectory))
        {
            Directory.CreateDirectory(serverDirectory);
        }





        bool nameFlag = true;
        session = 1;
        while (nameFlag)
        {
            localPrefix = localDirectory + "/" + sceneName + "_" + session.ToString();
            serverPrefix = serverDirectory + "/" + sceneName + "_" + session.ToString();
            if (File.Exists(localPrefix + ".sqlite"))
            {
                session++;
            }
            else
            {
                nameFlag = false;
                SqliteConnection.CreateFile(localPrefix + ".sqlite");
            }
        }

        string connectionString = "Data Source=" + localPrefix + ".sqlite;Version=3;";
        _connection = (IDbConnection)new SqliteConnection(connectionString);
        _connection.Open();
        _command = _connection.CreateCommand();
        _command.CommandText = "create table data (time REAL, trialnum INT, pos REAL, dz REAL, lick INT, reward INT," +
        "tstart INT, teleport INT, scanning INT, manrewards INT, autoreward INT, cmd INT)";
        _command.ExecuteNonQuery();
    }

    void LateUpdate()
    {
        if (pc.isQuitting) return;
        
        if (AutoReward){
            _autoReward = 1;
        }
        else{
            _autoReward = 0;
        }

        _command.CommandText = "insert into data (time, trialnum, pos, dz, lick, reward," +
        "tstart, teleport, scanning, manrewards, autoreward, cmd) values (" + Time.realtimeSinceStartup + "," + numTraversals + "," + transform.position.z + "," + rr.true_delta_z + "," + dl.c_1 + "," + dl.r + "," + pc.tstartFlag + "," + pc.tendFlag + "," + ttls.scanning + "," + pc.mRewardFlag + "," + _autoReward + "," + pc.cmd + ")";

        //Debug.Log(_command.CommandText);
        _command.ExecuteNonQuery();



    }

    void OnApplicationQuit()
    {
        _command.Dispose();
        _command = null;

        _connection.Close();
        _connection = null;

        File.Copy(localPrefix + ".sqlite", serverPrefix + ".sqlite", true);

        string sess_connectionString = "Data Source=" + serverDirectory_pre + "behavior_sessions.db;Version=3;";
        IDbConnection db_connection;
        db_connection = (IDbConnection)new SqliteConnection(sess_connectionString);
        db_connection.Open();
        IDbCommand db_command = db_connection.CreateCommand();

        db_command.CommandText = "create table IF NOT exists sessions (Mouse VARCHAR(100), Date VARCHAR(100), Scene VARCHAR(100), Session INT, Rewards INT, Trials INT, Imaging INT)";
        db_command.ExecuteNonQuery();

        string tmp_date = today.ToString("yyyy_MM_dd");
        db_command.CommandText = "insert into sessions (Mouse, Date, Scene, Session, Rewards, Trials, Imaging) values ('" + mouse + "','" + tmp_date + "','" + sceneName + "'," + session + "," + numRewards + "," + numTraversals + "," + scanning + ")";

        Debug.Log(db_command.CommandText);
        Debug.Log(Time.realtimeSinceStartup);

        db_command.ExecuteNonQuery();


        db_command.Dispose();
        db_command = null;

        db_connection.Close();
        db_connection = null;


    }
}
