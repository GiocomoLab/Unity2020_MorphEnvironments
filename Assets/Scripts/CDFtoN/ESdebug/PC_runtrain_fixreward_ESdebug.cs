using UnityEngine;
using System;
using System.Collections;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Text;
using System.Net;
using System.Net.Sockets;
using UnityEngine.UI;


public class PC_runtrain_fixreward_ESdebug : MonoBehaviour
{

    private GameObject player;

    private GameObject blackCam;
    private GameObject panoCam;
    private Camera cam;
    // private GameObject reward_t;
    //   private GameObject reward_b;
    //   private GameObject reward_c;
    private GameObject reward;
    //private GameObject Env3_Maze;

    private Rigidbody rb;
    private SP_runtrain_fixreward_ESdebug sp;
    private DL_runtrain_fixreward_ESdebug dl;
    private RR_runtrain_fixreward_ESdebug rotary;
    private SbxTTLs_runtrain_fixreward_ESdebug sbxttls;

    private bool reward_dir;


    private Vector3 initialPosition;
    private Vector3 movement;

    private static bool created = false;
    private int r;

    public int cmd = 2; //0 in runtrain, 2 in neuromods
    private float LastRewardTime;
    public int prevReward = 0;

    public ArrayList LickHistory;
    public bool bckgndOn = false; //false?

    public int mRewardFlag = 0;
    public int rzoneFlag = 0;
    public int toutzoneFlag = 0;
    public int tendFlag = 0;
    public int tstartFlag = 0;

    private static int localPort;
    private static string IP = "10.124.53.26";  // define in init
    private static int port = 7000;  // define in init

    public float deltaTime;
    public int target = 60;

    // UI for punishment in wrong trials
    public GameObject blackScreen;
    public bool blackoutActive = false;
    public float punishLength = 10f; // Can modify this number based on the prefered punishment strength
    //public bool norewardSession = false;
    private bool playerIntrigger = false;
    private float blackoutStartTime = 0f;
    private float TriggerStartTime = 0f;
    private float punishTime = 0f;


    // "connection" things
    IPEndPoint remoteEndPoint;
    UdpClient client;

    // public void Awake()
    // {

    // }


    public void Start()
    {
        GameObject player = GameObject.Find("Player");
        sp = player.GetComponent<SP_runtrain_fixreward_ESdebug>();
        rotary = player.GetComponent<RR_runtrain_fixreward_ESdebug>();
        dl = player.GetComponent<DL_runtrain_fixreward_ESdebug>();
        sbxttls = player.GetComponent<SbxTTLs_runtrain_fixreward_ESdebug>();
        panoCam = GameObject.Find("panoCamera");
        panoCam.transform.eulerAngles = new Vector3(0.0f, -90.0f, 0.0f);
        cam = Camera.main;
        initialPosition = new Vector3(0f, 6f, -5.0f);
        // reward_t = GameObject.Find("Reward_T");
        reward = GameObject.Find("Reward");

        StartCoroutine(FlagCheck());


        //   GameObject player = GameObject.Find("Player");
        //   sp = player.GetComponent<SP_RunTrain_to_Env1>();
        //   sbxttls = player.GetComponent<SbxTTLs_RunTrain_to_Env1>();
        //   rotary = player.GetComponent<RR_RunTrain_to_Env1>();
        //   dl = player.GetComponent<DL_RunTrain_to_Env1>();

        Debug.Log(sp.sceneName);

        sp.TrainingTrack = 1;

        // put the mouse in the dark tunnel
        //reward_t.SetActive(true);
        reward.SetActive(true);
        // reward_t.transform.position = reward_t.transform.position + new Vector3(0.0f, 0.0f, sp.mrd + UnityEngine.Random.value * sp.ard);
        LastRewardTime = Time.realtimeSinceStartup;




        //   panoCam = GameObject.Find("panoCamera");
        //   panoCam.transform.eulerAngles = new Vector3(0.0f, -90.0f, 0.0f);
        //   initialPosition = new Vector3(0f, 6f, -50.0f);

        LickHistory = new ArrayList();

        remoteEndPoint = new IPEndPoint(IPAddress.Parse(IP), port);
        client = new UdpClient();

        // set the recording rate
        Application.targetFrameRate = target;

        // UI for punishment in wrong trials
        blackScreen.SetActive(false);

    }


    private void sendString(string message)
    {
        try
        {
            if (message != "")
            {

                // get UTF8 encoding of string
                byte[] data = Encoding.UTF8.GetBytes(message);

                // send message
                client.Send(data, data.Length, remoteEndPoint);
            }
        }
        catch (Exception err)
        {
            print(err.ToString());
        }
    }




    void Update()
    {

        // make sure rotation angle is 0
        transform.eulerAngles = new Vector3(0.0f, -90.0f, 0.0f);
        //Debug.Log(panoCam.transform.eulerAngles);
        // end game after appropriate number of trials
        deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
        float fps = 1.0f / deltaTime;
        if ((sp.numTraversals >= sp.numTrialsTotal) | (sp.numRewards >= sp.maxRewards & transform.position.z < 0f))
        {
            //Debug.Log(sp.numTrialsTotal);
            UnityEditor.EditorApplication.isPlaying = false;

        }

        if (dl.r > 0 & dl.rflag < 1) { StartCoroutine(DeliverReward(dl.r)); dl.rflag = 1; }; // Debug.Log(fps); }; // deliver appropriate reward

        // manual rewards and punishments
        mRewardFlag = 0;
        if (Input.GetKeyDown(KeyCode.Q) | Input.GetMouseButtonDown(0)) // reward left - sweetened condensed milk
        {
            mRewardFlag = 1;
            StartCoroutine(DeliverReward(4));
            sp.numRewards += 1;

        }


        //Debug.Log(fps);
        //Debug.Log(deltaTime * 1000);
        if (Application.targetFrameRate != target)
        { Application.targetFrameRate = target; }
        // show frame rate

        //Debug.Log(Application.targetFrameRate);



    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.tag);

        if (other.tag == "Reward")
        {
            StartCoroutine(RewardSequence(transform.position.z, other.gameObject));
        }
        else if (other.tag == "Teleport")
        {
            sp.numTraversals += 1;
            tendFlag = 1;
            transform.position = initialPosition;
            bckgndOn = true;
            Debug.Log("BLACK OUT IS:" + blackoutActive);
            //blackScreen.SetActive(false);
            //blackoutActive = false;
            //Env3_Maze.gameObject.SetActive(true);
            StartCoroutine(InterTrialTimeout());

            if (sp.TrainingTrack == 1)
            {
                reward.SetActive(true);
            }
            LastRewardTime = Time.realtimeSinceStartup;
        }
        else if (other.tag == "Start")
        {
            cmd = 0;
            tstartFlag = 1;
        }
        else if (other.tag == "No Reward") // punish the animal if it licks at the wrong reward zone
        {
            Debug.Log("enter punish zone");
            StartCoroutine(NoRewardSequence(transform.position.z, sp.numTraversals));
        }
    }

    IEnumerator NoRewardSequence(float pos, float traversal)
    {
        Debug.Log("enter lick punishment zone");
        //Debug.Log(transform.position.z);
        //Debug.Log(pos);
        yield return null;
        if (dl.c_1 > 0 && !blackoutActive) // If animal lick at the wrong reward location, black out the screen, during this time, animal can still run and finish the trial
        {
            Debug.Log("triggered punishment");
            blackScreen.SetActive(true);
            blackoutActive = true;
            blackoutStartTime = Time.time;
            Debug.Log("start timing");
            while (transform.position.z < 450 && sp.numTraversals <= traversal)
            {
                yield return new WaitForSeconds(punishLength); // screen keep black out if animal not run and not finish the currtent trial
            }
            //yield return new WaitForSeconds(punishLength);
            Debug.Log("punishment end");
            blackScreen.SetActive(false);
            blackoutActive = false;
            Debug.Log("in if loop BLACK OUT IS:" + blackoutActive);


            Color originalColor = cam.backgroundColor;
            cam.backgroundColor = Color.black;


            cam.backgroundColor = originalColor;
        }
        else { Debug.Log("didn't enter the if loop"); }
        //}
    }
            





    IEnumerator FlagCheck()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();
            tendFlag = 0;
            tstartFlag = 0;
        }
        yield return null;

    }


    IEnumerator InterTrialTimeout()
    {

        rotary.toutBool = 0f;
        // finish the rest of the punishment of lick at the wrong loction ( if the animal keep running, the punish ment will be punishLength in total, if it sit in the current trail not running, the punishment will be longer
        TriggerStartTime = Time.time;
        punishTime = TriggerStartTime - blackoutStartTime;
        Debug.Log("punish time before teleport is "+ punishTime);
        if (blackoutActive == true) 
        {
            Debug.Log(" continue punish for " + (punishLength - punishTime));    
            yield return new WaitForSeconds(punishLength - punishTime);
        }
        

        if ((sbxttls.scanning > 0) & sp.BlankLaser)
        {
            if (prevReward == 0) // omission or probe trial 
            {
                sendString("L0");
                yield return new WaitForSeconds(5f + UnityEngine.Random.value * 4f);
                sendString("L1");
                yield return new WaitForSeconds(1f);

            }
            else
            {
                sendString("L0");
                yield return new WaitForSeconds(UnityEngine.Random.value * 4f);
                sendString("L1");
                yield return new WaitForSeconds(1f);
            }
        }
        else
        {
            if (prevReward == 0) // omission or probe trial 
            {
                yield return new WaitForSeconds(5f + UnityEngine.Random.value * 4f);
                yield return new WaitForSeconds(1f);

            }
            else
            {
                yield return new WaitForSeconds(UnityEngine.Random.value * 4f);
                yield return new WaitForSeconds(1f);
            }

        }

        rotary.toutBool = 1f;
        prevReward = 0;
        yield return null;
    }


    IEnumerator RewardSequence(float pos, GameObject _reward)
    {   // water reward
        rzoneFlag = 1;


        while ((transform.position.z <= pos + 75))
        {
            if (blackoutActive == true) // Skipp reward if the wrong location is before the reward location -> this will also cause a longer timeout after teleport, decide to leave it as it is
                                        // but could consider using the lick value to make sure the two type on reward and wrog location (Env_locA and Env_locB) has the same length at the beginning after teleport
            {
                break;
            }


            if ((sp.AutoReward) & (transform.position.z > pos + 50))
            {


                cmd = 4;
                StartCoroutine(DeliverReward(1));
                sp.numRewards += 1;
                prevReward = 1;
                yield return new WaitForEndOfFrame();
                break;


            }

            if (dl.c_1 > 0)
            {

                cmd = 4;
                sp.numRewards += 1;
                prevReward = 1;
                yield return new WaitForEndOfFrame();
                break;
            }
            yield return new WaitForEndOfFrame();

        }
        _reward.SetActive(false);

        //if ((sp.sceneName == "NeuroMods_LocationA"))
        //{
        //    
        //    reward.SetActive(false);
        //
        //      } else if ((sp.sceneName == "NeuroMods_LocationB"))
        //    {
        //      reward.SetActive(false);
        //} else
        //{
        //   reward_a.SetActive(false);
        //  reward_b.SetActive(false);
        // reward_c.SetActive(false);
        //}




        rzoneFlag = 0;
        yield return new WaitForEndOfFrame();
        cmd = 2;
        yield return new WaitForEndOfFrame();
        cmd = 0;

    }

  


    void OnApplicationQuit()
    {
        panoCam.SetActive(false);
    }


    IEnumerator LightsOn()
    {
        panoCam.SetActive(true);
        yield return null;
    }


    IEnumerator DeliverReward(int r)
    { // deliver


        if (r == 4)
        {
            cmd = 4;
            //sp.numRewards += 1;
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            cmd = 0;
            yield return new WaitForEndOfFrame();
        }
        {
            yield return null;
        };



    }



}