using UnityEngine;
using System;
using System.Collections;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Text;
using System.Net;
using System.Net.Sockets;




public class PC_RunTrain_to_Env1 : MonoBehaviour
{


    private GameObject player;
    
    private GameObject blackCam;
    private GameObject panoCam;
    private GameObject reward_t;
 //   private GameObject reward_b;
 //   private GameObject reward_c;
    private GameObject reward;

    private Rigidbody rb;
    private SP_RunTrain_to_Env1 sp;
    private DL_RunTrain_to_Env1 dl;
    private RR_RunTrain_to_Env1 rotary;
    private SbxTTLs_RunTrain_to_Env1 sbxttls;

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

    // "connection" things
    IPEndPoint remoteEndPoint;
    UdpClient client;

    public int frameRate = 60;

   // public void Awake()
   // {
        
   // }


    public void Start()
    {
        GameObject player = GameObject.Find("Player");
        sp = player.GetComponent<SP_RunTrain_to_Env1>();
        rotary = player.GetComponent<RR_RunTrain_to_Env1>();
        dl = player.GetComponent<DL_RunTrain_to_Env1>();
        sbxttls = player.GetComponent<SbxTTLs_RunTrain_to_Env1>();
        panoCam = GameObject.Find("panoCamera");
        panoCam.transform.eulerAngles = new Vector3(0.0f, -90.0f, 0.0f);
        initialPosition = new Vector3(0f, 6f, -5.0f);
        reward_t = GameObject.Find("Reward_T");
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
        reward_t.SetActive(true);
        reward.SetActive(false);
        reward_t.transform.position = reward_t.transform.position + new Vector3(0.0f, 0.0f, sp.mrd + UnityEngine.Random.value * sp.ard);
        LastRewardTime = Time.realtimeSinceStartup;




        //   panoCam = GameObject.Find("panoCamera");
        //   panoCam.transform.eulerAngles = new Vector3(0.0f, -90.0f, 0.0f);
        //   initialPosition = new Vector3(0f, 6f, -50.0f);

        LickHistory = new ArrayList();

        remoteEndPoint = new IPEndPoint(IPAddress.Parse(IP), port);
        client = new UdpClient();

        Application.targetFrameRate = frameRate;

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
        transform.eulerAngles = new Vector3(0.0f, 90.0f, 0.0f);

        // end game after appropriate number of trials
        if ((sp.numTraversals >= sp.numTrialsTotal) | (sp.numRewards >= sp.maxRewards & transform.position.z < 0f))
        {
            UnityEditor.EditorApplication.isPlaying = false;

        }

        if (dl.r > 0 & dl.rflag < 1) { StartCoroutine(DeliverReward(dl.r)); dl.rflag = 1; }; // deliver appropriate reward

        // manual rewards and punishments
        mRewardFlag = 0;
        if (Input.GetKeyDown(KeyCode.Q) | Input.GetMouseButtonDown(0)) // reward left - sweetened condensed milk
        {
            mRewardFlag = 1;
            StartCoroutine(DeliverReward(4));
            sp.numRewards += 1;

        }

        // set frame Rate
        if (Application.targetFrameRate != frameRate)
        {
            Application.targetFrameRate = frameRate;
        }

        Debug.Log(Application.targetFrameRate);


    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.tag);


        if (other.tag == "Reward")
        {
           StartCoroutine(RewardSequence(transform.position.z,other.gameObject)); 
        }
        else if (other.tag == "Teleport")
        {
            
            

            sp.numTraversals += 1;
            tendFlag = 1;
            transform.position = initialPosition;
            bckgndOn = true;

            StartCoroutine(InterTrialTimeout());

            if (sp.TrainingTrack == 1)
            {
                reward.SetActive(true);
            }
            
           
            //reward.transform.position = new Vector3(0.0f, 0f, sp.mrd + UnityEngine.Random.value * sp.ard);
            LastRewardTime = Time.realtimeSinceStartup; // to avoid issues with teleports
        }
        else if (other.tag == "Start")
        {
            cmd = 0;
            tstartFlag = 1;
            
            //rflag = true;
        }
      

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
        if ((sbxttls.scanning>0) & sp.BlankLaser)
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
        } else
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
            if (sp.TrainingTrack == 1)
            {
                Debug.Log(cmd);
                cmd = 12;
                if ((sp.AutoReward) & (transform.position.z > pos + 30))
                {
                    cmd = 4; 
                    sp.numRewards += 1;
                    prevReward = 1;
                    StartCoroutine(DeliverReward(1));
                    if (sp.MultiReward)
                    {
                        StartCoroutine(MoveReward());
                    }
                    //counted = false;
                    yield return new WaitForEndOfFrame();
                    break; // if (sp.MultiReward) { break;  };
                            //break;
                }
                else
                {
                    cmd = 12;
                }

                if (dl.c_1 > 0)
                {
                    cmd = 4;
                    sp.numRewards += 1;
                    prevReward = 1;
                    if (sp.MultiReward)
                    {

                        StartCoroutine(MoveReward());
                    }
                    else
                    {
                        reward_t.SetActive(false);
                    }
                    //counted = false;
                    //if (sp.MultiReward) { break; };
                    yield return new WaitForEndOfFrame();
                    break;
                }
                yield return null;
            }

            if (sp.TrainingTrack == 0)
            {
                cmd = 12;
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


        }
        _reward.SetActive(false);


        rzoneFlag = 0;
        yield return new WaitForEndOfFrame();
        cmd = 2;
        yield return new WaitForEndOfFrame();
        cmd = 0;

    }

    IEnumerator MoveReward()
    {
        float CurrRewardTime = Time.realtimeSinceStartup;
        yield return new WaitForSeconds(.5f);
        if (!sp.fixedRewardSchedule)
        {

            if (CurrRewardTime - LastRewardTime > 20.0f)
            {
                sp.mrd = Mathf.Max(sp.MinTrainingDist, sp.mrd + UnityEngine.Random.value * sp.ard - 10f);

            }
            else
            {
                sp.mrd = Mathf.Min(sp.MaxTrainingDist, sp.mrd + UnityEngine.Random.value * sp.ard + 10f);
            }

            if ((sp.mrd > 170f) & (sp.mrd < 310))
            {
                sp.mrd = 300;
            }
        }
        //float zpos = (reward.transform.position.z + sp.mrd +sp.ard) % 330f;
        reward_t.transform.position = reward_t.transform.position + new Vector3(0f, 0f, sp.mrd + UnityEngine.Random.value * sp.ard);
        LastRewardTime = CurrRewardTime;
        yield return null;
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


        if (r ==4)
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
