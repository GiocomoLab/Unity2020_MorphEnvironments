using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System.IO;
using System;
using System.Data;
using Mono.Data.Sqlite;

public class SP_RunTrain_to_Env1_can : MonoBehaviour
{


    private string mouse;

    public float SkipTrialPcnt = 0.0f;

    public bool AutoReward = true;
    private int _autoReward = 0;
    public float mrd = 30.0f; // minimum reward distance
    public float ard = 10.0f; // additional reward distance
    public bool fixedRewardSchedule = false; // proportion of trials with towers on both sides
    public float MinTrainingDist = 10f;
    public float MaxTrainingDist = 300f;

    public int numRewards = 0;
    public int numRewards_manual = 0;
    public int rewardFlag = 0;

    public int numTraversals = 0;
    public int numTrialsTotal;
    public int maxRewards = 100;

    public bool BlankLaser = false;

    public float rDur = 2f; // timeout duration between available rewards

    public float morph = 0.0f;

    public int TrainingTrack = 1;

    public bool MultiReward = false;
    // for saving data
    public string localDirectory_pre = "C:/Users/CanD/behavior/";
    public string serverDirectory_pre = "G:\\My Drive\\CA123\\behavior\\";
    public string localDirectory;
    public string serverDirectory;
    public string localPrefix;
    public string serverPrefix;
    public string sceneName;

    private GameObject player;
    private RR_RunTrain_to_Env1 rr;
    private DL_RunTrain_to_Env1 dl;
    private PC_RunTrain_to_Env1 pc;
    private SbxTTLs_RunTrain_to_Env1 ttls;
    private Notes notes;


    public int session = 1;
    private DateTime today;
    private IDbConnection _connection;
    private IDbCommand _command;

    public int scanning = 0;


    public int dirCheck = 0;

    public void Awake()
    {

        player = GameObject.Find("Player");
        rr = player.GetComponent<RR_RunTrain_to_Env1>();
        dl = player.GetComponent<DL_RunTrain_to_Env1>();
        pc = player.GetComponent<PC_RunTrain_to_Env1>();
        ttls = player.GetComponent<SbxTTLs_RunTrain_to_Env1>();
        notes = player.GetComponent<Notes>();
        mouse = notes.mouse;

        today = DateTime.Today;
        Debug.Log(today.ToString("dd_MM_yyyy"));
        sceneName = SceneManager.GetActiveScene().name;
        localDirectory = localDirectory_pre + mouse + '/' + today.ToString("dd_MM_yyy") + '/';
        serverDirectory = serverDirectory_pre + mouse + '/' + today.ToString("dd_MM_yyy") + '/';
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
        _command.CommandText = "create table data (time REAL, morph REAL, trialnum INT, pos REAL, dz REAL, posx REAL, lick INT, reward INT," +
        "tstart INT, teleport INT, rzone INT, scanning NUMERIC, manrewards INT, autoreward INT, cmd INT, trainingtrack INT)";
        _command.ExecuteNonQuery();
    }

    void LateUpdate()
    {

        if (AutoReward)
        {
            _autoReward = 1;
        }
        else
        {
            _autoReward = 0;
        }

        _command.CommandText = "insert into data (time , morph , trainingtrack, trialnum, pos, dz, posx, lick, reward," +
        "tstart, teleport, rzone , scanning, manrewards, autoreward, cmd) values (" + Time.realtimeSinceStartup + "," + morph + "," + TrainingTrack + "," + numTraversals +
        "," + transform.position.z + "," + rr.true_delta_z + "," + transform.position.x + "," + dl.c_1 + "," + dl.r + "," + pc.tstartFlag + "," + pc.tendFlag + "," +
        pc.rzoneFlag + "," + ttls.scanning + "," + pc.mRewardFlag + "," + _autoReward + "," + pc.cmd + ")";


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

        string sess_connectionString = "Data Source=H:\\My Drive\\VR_Data\\behavior_sessions.db;Version=3;";
        IDbConnection db_connection;
        db_connection = (IDbConnection)new SqliteConnection(sess_connectionString);
        db_connection.Open();
        IDbCommand db_command = db_connection.CreateCommand();
        string tmp_date = today.ToString("dd_MM_yyyy");
        db_command.CommandText = "insert into sessions (MouseName, DateFolder, SessionNumber, Track, RewardCount, Imaging) values ('" + mouse + "', '" + tmp_date + "', "
            + session + ",'" + sceneName + "', " + numRewards + ", " + scanning + ")";

        Debug.Log(db_command.CommandText);

        db_command.ExecuteNonQuery();


        db_command.Dispose();
        db_command = null;

        db_connection.Close();
        db_connection = null;


    }
}
