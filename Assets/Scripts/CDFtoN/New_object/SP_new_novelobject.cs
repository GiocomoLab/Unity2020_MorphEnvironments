using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System;
using System.Data;
using Mono.Data.Sqlite;

public class SP_new_novelobject : MonoBehaviour
{
    // Start is called before the first frame update
    private string mouse;

    public float SkipTrialPcnt = 0.0f;

    public bool AutoReward = false;
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
    public int numTrialsTotal = 40;
    public int maxRewards = 100;

    public bool BlankLaser = false;

    public float rDur = 2f; // timeout duration between available rewards

    public float morph = 0.0f;

    public int TrainingTrack = 1;

    public bool MultiReward = false;

    // Track ball
    public GameObject blackOval;
    private Transform ballTransform;
    private OvalTraverser_with_conditions ovalTraverser;

    // for saving data
    public string localDirectory_pre = "C:/Users/markp/VR_Data/CanD/behavior/";
    public string serverDirectory_pre = "G:\\My Drive\\CA123\\behavior\\";
    public string localDirectory;
    public string serverDirectory;
    public string localPrefix;
    public string serverPrefix;
    public string sceneName;

    private GameObject player;
    private RR_new_novelobject rr;
    private DL_new_novelobject dl;
    private PC_new_novelobject pc;
    private SbxTTLs_new_novelobject ttls;
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
        rr = player.GetComponent<RR_new_novelobject>();
        dl = player.GetComponent<DL_new_novelobject>();
        pc = player.GetComponent<PC_new_novelobject>();
        ttls = player.GetComponent<SbxTTLs_new_novelobject>();
        notes = player.GetComponent<Notes>();
        mouse = notes.mouse;

        // find ball
        if (blackOval == null)
        {
            blackOval = GameObject.Find("BlackOval");
            if (blackOval == null)
            {
                Debug.LogWarning("BlackOval not found! Ball position not being tracked");
            }
        }

        if (blackOval != null)
        {
            ballTransform = blackOval.transform;
            ovalTraverser = blackOval.GetComponent<OvalTraverser_with_conditions>();

            if (ovalTraverser == null)
            {
                Debug.LogWarning("OvalTraverser component not found on BlackOval. Traversal parameters will not be saved.");
            }
        }

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

        // Main high-frequency behavioral data table
        _command = _connection.CreateCommand();
        _command.CommandText =
            "create table data (time REAL, morph REAL, trialnum INT, pos REAL, dz REAL, posx REAL, lick INT, reward INT," +
            "tstart INT, teleport INT, rzone INT, scanning NUMERIC, manrewards INT, autoreward INT, cmd INT, trainingtrack INT, norewardSess INT," +
            "ball_x REAL, ball_y REAL, ball_z REAL)";
        _command.ExecuteNonQuery();

        // Save the experimental traversal condition once per session.
        _command.CommandText =
            "create table parameters (" +
            "conditionID INT, " +
            "traverseSpeed REAL, " +
            "pauseProbability REAL, " +
            "centerRange REAL, " +
            "minPauseDuration REAL, " +
            "maxPauseDuration REAL, " +
            "centerGammaShape REAL, " +
            "centerGammaScale REAL, " +
            "randomFreezeProbability REAL, " +
            "minFreezeDuration REAL, " +
            "maxFreezeDuration REAL, " +
            "outsideGammaShape REAL, " +
            "outsideGammaScale REAL, " +
            "freezeCheckInterval REAL, " +
            "centerLambda REAL, " +
            "outsideLambda REAL, " +
            "centerTurnBackProbability REAL, " +
            "nonCenterTurnBackProbability REAL)";
        _command.ExecuteNonQuery();

        if (ovalTraverser != null)
        {
            ovalTraverser.ApplyCondition();
        }

        SaveTraversalParameters();
    }

    private void SaveTraversalParameters()
    {
        if (ovalTraverser == null)
            return;

        _command.CommandText =
            "insert into parameters (" +
            "conditionID, traverseSpeed, pauseProbability, centerRange, minPauseDuration, maxPauseDuration, centerGammaShape, centerGammaScale, " +
            "randomFreezeProbability, minFreezeDuration, maxFreezeDuration, outsideGammaShape, outsideGammaScale, freezeCheckInterval, " +
            "centerLambda, outsideLambda, centerTurnBackProbability, nonCenterTurnBackProbability) values (" +
            ovalTraverser.conditionID + "," +
            ovalTraverser.traverseSpeed + "," +
            ovalTraverser.pauseProbability + "," +
            ovalTraverser.centerRange + "," +
            ovalTraverser.minPauseDuration + "," +
            ovalTraverser.maxPauseDuration + "," +
            ovalTraverser.centerGammaShape + "," +
            ovalTraverser.centerGammaScale + "," +
            ovalTraverser.randomFreezeProbability + "," +
            ovalTraverser.minFreezeDuration + "," +
            ovalTraverser.maxFreezeDuration + "," +
            ovalTraverser.outsideGammaShape + "," +
            ovalTraverser.outsideGammaScale + "," +
            ovalTraverser.freezeCheckInterval + "," +
            ovalTraverser.centerLambda + "," +
            ovalTraverser.outsideLambda + "," +
            ovalTraverser.centerTurnBackProbability + "," +
            ovalTraverser.nonCenterTurnBackProbability + ")";

        _command.ExecuteNonQuery();

        Debug.Log(
            "Saved OvalTraverser condition " + ovalTraverser.conditionID +
            " | speed=" + ovalTraverser.traverseSpeed +
            " | center probability/check=" + ovalTraverser.pauseProbability +
            " | center range=+/-" + ovalTraverser.centerRange + " deg" +
            " | non-center probability/check=" + ovalTraverser.randomFreezeProbability +
            " | center lambda=" + ovalTraverser.centerLambda +
            " | outside lambda=" + ovalTraverser.outsideLambda +
            " | center gamma=(shape " + ovalTraverser.centerGammaShape + ", scale " + ovalTraverser.centerGammaScale + ", max " + ovalTraverser.maxPauseDuration + ")" +
            " | outside gamma=(shape " + ovalTraverser.outsideGammaShape + ", scale " + ovalTraverser.outsideGammaScale + ", max " + ovalTraverser.maxFreezeDuration + ")" +
            " | center turn-back probability=" + ovalTraverser.centerTurnBackProbability +
            " | non-center turn-back probability=" + ovalTraverser.nonCenterTurnBackProbability
        );
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

        // get ball position
        float ballX = 0f;
        float ballY = 0f;
        float ballZ = 0f;

        if (ballTransform != null)
        {
            ballX = ballTransform.position.x;
            ballY = ballTransform.position.y;
            ballZ = ballTransform.position.z;
        }

        _command.CommandText =
            "insert into data (time , morph , trainingtrack, trialnum, pos, dz, posx, lick, reward," +
            "tstart, teleport, rzone , scanning, manrewards, autoreward, cmd, norewardSess, ball_x, ball_y, ball_z) values (" +
            Time.realtimeSinceStartup + "," + morph + "," + TrainingTrack + "," + numTraversals +
            "," + transform.position.z + "," + rr.true_delta_z + "," + transform.position.x + "," + dl.c_1 + "," + dl.r + "," +
            pc.tstartFlag + "," + pc.tendFlag + "," + pc.rzoneFlag + "," + ttls.scanning + "," + pc.mRewardFlag + "," +
            _autoReward + "," + pc.cmd + ", " + Convert.ToByte(pc.norewardSession) + "," + ballX + "," + ballY + "," + ballZ + ")";

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

        db_command.CommandText =
            "insert into sessions (MouseName, DateFolder, SessionNumber, Track, RewardCount, Imaging) values ('" +
            mouse + "', '" + tmp_date + "', " + session + ",'" + sceneName + "', " + numRewards + ", " + scanning + ")";

        Debug.Log(db_command.CommandText);

        db_command.ExecuteNonQuery();

        db_command.Dispose();
        db_command = null;

        db_connection.Close();
        db_connection = null;
    }
}
