using UnityEngine;
using System;
using System.Collections;
using System.IO;
using System.IO.Ports;
using System.Threading;





public class PC_TwoReward : MonoBehaviour
{


    private GameObject player;

    private GameObject blackCam;
    private GameObject panoCam;
    private GameObject reward0;
    private GameObject reward1;
    private GameObject reward;




    private Rigidbody rb;

    private SP_TwoReward sp;
    private SerialPort_TwoReward serial_port;

    private bool reward_dir;


    private Vector3 initialPosition;
    private Vector3 movement;

    private static bool created = false;
    private int r;
    private int r_last = 0;
    //private bool rflag = true;

    public int cmd = 2;
    //private bool flashFlag = false;
    private float LastRewardTime;
    private int prevReward = 0;

    public ArrayList LickHistory;
    public bool bckgndOn = true;


    public int mRewardFlag = 0;
    public int rzoneFlag = 0;
    public int toutzoneFlag = 0;
    public int tendFlag = 0;
    public int tstartFlag = 0;
    private bool NoSkipTO = true;




    public void Awake()
    {


        GameObject player = GameObject.Find("Player");
        sp = player.GetComponent<SP_TwoReward>();
        serial_port = player.GetComponent<SerialPort_TwoReward>();
        reward = GameObject.Find("Reward");

        // Debug.Log(sp.sceneName);



        panoCam = GameObject.Find("panoCamera");
        panoCam.transform.eulerAngles = new Vector3(0.0f, 90.0f, 0.0f);
        initialPosition = new Vector3(0f, 6f, -50.0f);


        //towerJitter = .2f * (UnityEngine.Random.value - .5f);
        //wallJitter = .2f * (UnityEngine.Random.value - .5f);
        //bckgndJitter = .2f * (UnityEngine.Random.value - .5f);

        LickHistory = new ArrayList();
    }

    void Start()
    {

    }

    void Update()
    {

        // make sure rotation angle is 0
        transform.eulerAngles = new Vector3(0.0f, -90.0f, 0.0f);

        // end game after appropriate number of trials
        if ((sp.numTraversals >= sp.numTrialsTotal) | (sp.numRewards >= sp.maxRewards & transform.position.z < 0f))
        {
            UnityEditor.EditorApplication.isPlaying = false;

        }

        if (serial_port.r > 0 & serial_port.rflag < 1) { StartCoroutine(DeliverReward(serial_port.r)); serial_port.rflag = 1; }; // deliver appropriate reward

        // manual rewards and punishments
        mRewardFlag = 0;
        if (Input.GetKeyDown(KeyCode.Q) | Input.GetMouseButtonDown(0)) // reward left - sweetened condensed milk
        {
            mRewardFlag = 1;
            StartCoroutine(DeliverReward(4));
            sp.numRewards += 1;

        }



    }
    // GUI for collider, turn on subroutine for collider and turn on isTrigger
    void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.tag);


        if (other.tag == "Reward")
        {
            // if (rflag)
            // {
            StartCoroutine(RewardSequence(transform.position.z));
            //}

        }
        else if (other.tag == "Teleport")
        {
            //rflag = true;


            sp.numTraversals += 1;
            tendFlag = 1;
            transform.position = initialPosition; //teleport
            bckgndOn = true;

            StartCoroutine(InterTrialTimeout());

            LastRewardTime = Time.realtimeSinceStartup; // to avoid issues with teleports
        }
        else if (other.tag == "Start")
        {
            cmd = 0;
            tstartFlag = 1;

            //rflag = true;
        }
        else if (other.tag == "Timeout")
        {
            //if (sp.morph != .5f)
            //{
            StartCoroutine(TimeoutSequence(transform.position.z));

            //}
        }
        else if (other.tag == "Delay")
        {
            bckgndOn = false;
        }

    }



    IEnumerator InterTrialTimeout()
    {

        serial_port.toutBool = 0f;
        if (prevReward == 0) // omission
        {
            yield return new WaitForSeconds(5f + UnityEngine.Random.value * 5f); // 10f); // 7f);

        }
        else
        {
            yield return new WaitForSeconds(1f + UnityEngine.Random.value * 4f);
        }

        serial_port.toutBool = 1f;
        prevReward = 0;
        yield return null;
    }

    void OnApplicationQuit()
    {
        panoCam.SetActive(false);
    }

    IEnumerator RewardSequence(float pos)
    {   // water reward
        rzoneFlag = 1;

        //if (sp.morph == .5f) // guarantee no timeout if morph = .5
        //{
        //    StartCoroutine(DeliverReward(5));
        //}

        //bool counted = true; transform is the player's set of var--pos for xyz in diff dim
        while ((transform.position.z <= pos + 75) & (transform.position.z > 100)) // & (rflag))  //& (counted)
        {

            cmd = 12;
            if ((sp.AutoReward) & (transform.position.z > pos + 50)) // & (counted) )
            {

                // if (transform.position.z > pos + 50)
                //{
                if (sp.morph > 0)
                {
                    cmd = 14;
                }
                else
                {
                    cmd = 4;
                    sp.numRewards += 1;
                }
                //cmd = 4; //arduino; if lick, dispense r
                         //counted = false;
                         //rflag = false;

                // yield return new WaitForSeconds(.01f);
                //cmd = 0;
                LickHistory.Add(.0f);
                StartCoroutine(DeliverReward(1));
                sp.numRewards += 1;
                yield return new WaitForEndOfFrame();
                break;

            }

            if ((serial_port.c_1 > 0))// & (counted))
            {
                //counted = false;
                //rflag = false;
                if (sp.morph > 0)
                {
                    cmd = 14;
                }
                else
                {
                    cmd = 4;
                    sp.numRewards += 1;
                }
                sp.numRewards += 1;

                LickHistory.Add(0f);
                yield return new WaitForEndOfFrame();
                break;
            }
            yield return new WaitForEndOfFrame();
            // yield return null;
        }
        //yield return new WaitForEndOfFrame();
        //cmd = 2;
        //yield return new WaitForSeconds(.1f);

        //yield return null;
        reward.SetActive(false);
        



        //cmd = 2;
        rzoneFlag = 0;
        yield return new WaitForEndOfFrame();
        //yield return new WaitForSeconds(.1f);
        cmd = 2;

    }

    IEnumerator TimeoutSequence(float pos)
    {
        toutzoneFlag = 1;
        //if (sp.morph == .5f)
        //{
        //    NoSkipTO = false;
        //}
        //else
        //{
        //    NoSkipTO = true;
        //}
        NoSkipTO = false;

        while ((transform.position.z <= pos + 65))
        {
            cmd = 0;
            if (serial_port.c_1 > 0)
            {


                LickHistory.Add(.0f);

                toutzoneFlag = 0;
                tendFlag = 1;
                transform.position = initialPosition;
                sp.numTraversals += 1;
                cmd = 2;
                bckgndOn = true;



                // rflag = true;
                // avoid .5 timeout just end trial
                if (NoSkipTO)
                {
                    serial_port.toutBool = 0f;
                    yield return new WaitForSeconds(5f);
                    //  if (sp.morph == 0f)
                    //  {
                    //      yield return new WaitForSeconds(5f);
                    // }
                    // sp.morph = nextMorph;
                    yield return new WaitForSeconds(5f);
                    serial_port.toutBool = 1f;
                }
                else
                {
                    serial_port.toutBool = 0f;
                    yield return new WaitForSeconds(1f);
                    serial_port.toutBool = 1f;
                }
                NoSkipTO = true;

                break;
            }
            else
            {
                yield return null;

            }
        }
        toutzoneFlag = 0;


    }



    IEnumerator LightsOn()
    {
        panoCam.SetActive(true);
        yield return null;
    }

    //keeping track of prev beh
    IEnumerator DeliverReward(int r)
    { // deliver

        if (r == 1) // reward left
        {
            prevReward = 1;
            //sp.numRewards += 1;
            yield return null;
        }
        else if (r == 4)
        {
            if (sp.morph > 0)
            {
                cmd = 14;
            }
            else
            {
                cmd = 4;
            }
            yield return new WaitForSeconds(0.01f);
            //sp.numRewards += 1;
            cmd = 0;

        }
        else if (r == 5)
        {
            prevReward = 1;
            yield return null;
        }
        else
        {
            yield return null;
        };



    }

}
