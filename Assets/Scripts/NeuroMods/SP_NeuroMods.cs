using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System.IO;
using System;
using System.Data;
using Mono.Data.Sqlite;


public class SP_NeuroMods : MonoBehaviour
{


    private string mouse;

    public float SkipTrialPcnt = 0.0f;

    public bool AutoReward = false;


    public int numRewards = 0;
    public int numRewards_manual = 0;
    public int rewardFlag = 0;

    public int numTraversals = 0;
    public int numTrialsTotal;
    public int maxRewards = 200;


    public float morph = 0;
    public float rDur = 2;

    // for saving data
    public string localDirectory_pre = "C:/Users/markp/VR_Data/NeuroMods/";
    public string serverDirectory_pre = "G:\\My Drive\\InVivoDA\\VR_Data\\";
    public string localDirectory;
    public string serverDirectory;
    public string localPrefix;
    public string serverPrefix;
    public string sceneName;

    public int session = 1;


    private DateTime today;
    private GameObject player;

    private TrialOrdering_NeuroMods tott;
    private RR_NeuroMods rr;
    private DL_NeuroMods dl;
    private PC_NeuroMods pc;
    private SbxTTLs_NeuroMods ttls;
    private Notes notes;



    private IDbConnection _connection;
    private IDbCommand _command;



    public int scanning = 0;

    public void Awake()
    {
       
        player = GameObject.Find("Player");
        sceneName = SceneManager.GetActiveScene().name;


        
        rr = player.GetComponent<RR_NeuroMods>();
        dl = player.GetComponent<DL_NeuroMods>();
        pc = player.GetComponent<PC_NeuroMods>();
        ttls = player.GetComponent<SbxTTLs_NeuroMods>();
        notes = player.GetComponent<Notes>();
        mouse = notes.mouse;


        today = DateTime.Today;
        Debug.Log(today.ToString("dd_MM_yyyy"));


        
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
        _command.CommandText = "create table data (time REAL, morph REAL, trialnum INT, pos REAL, dz REAL, lick INT, reward INT," +
        "tstart INT, teleport INT, rzone INT, toutzone INT, towerJitter REAL," +
        " wallJitter REAL, bckgndJitter REAL, scanning NUMERIC, manrewards INT, cmd INT)";
        
        _command.ExecuteNonQuery();

        // make table for session information

        // trial type numbers
        _command.CommandText = "create table trialInfo (baseline INT, training INT, test INT)";
        _command.ExecuteNonQuery();

       
    }

    void LateUpdate() {

      _command.CommandText = "insert into data (time , morph , trialnum, pos, dz, lick, reward," +
      "tstart, teleport, rzone , toutzone, towerJitter," +
      " wallJitter , bckgndJitter , scanning, manrewards, autoreward, cmd) values (" + Time.realtimeSinceStartup + "," + morph + "," + numTraversals +
      "," + transform.position.z + "," + rr.true_delta_z + "," + dl.c_1 + "," + dl.r + "," + pc.tstartFlag + "," + pc.tendFlag + "," +
      pc.rzoneFlag + "," + pc.toutzoneFlag + ","  + pc.towerJitter + "," + pc.wallJitter + "," +
      pc.bckgndJitter + "," + ttls.scanning + "," + pc.mRewardFlag + "," + AutoReward + "," + pc.cmd + ")";
      _command.ExecuteNonQuery();

    }

    void OnApplicationQuit()
    {
        _command.Dispose();
        _command = null;

        _connection.Close();
        _connection = null;

        File.Copy(localPrefix + ".sqlite", serverPrefix + ".sqlite",true);

        string sess_connectionString = "Data Source=G:\\My Drive\\InVivoDA\\TVR_Data\\behavior_sessions.sqlite;Version=3;";
        IDbConnection db_connection;
        db_connection = (IDbConnection) new SqliteConnection(sess_connectionString);
        db_connection.Open();
        IDbCommand db_command = db_connection.CreateCommand();
        string tmp_date = today.ToString("dd_MM_yyyy");
        db_command.CommandText = "insert into sessions (MouseName, DateFolder, SessionNumber, Track, RewardCount, Imaging, ImagingRegion, Notes) values ('" + mouse +  "', '" + tmp_date + "', "
            + session + ",'"+ sceneName + "', " + numRewards + ", " + scanning + ",'" + notes.imaging_region + "','" + notes.notes + "')";

        Debug.Log(db_command.CommandText);

        db_command.ExecuteNonQuery();


        db_command.Dispose();
        db_command = null;

        db_connection.Close();
        db_connection = null;


    }

}
