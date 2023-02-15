using UnityEngine;
using System;
using System.Collections;
using System.IO;
using System.IO.Ports;
using System.Threading;




public class PC_RunTrain_to_Env1_old : MonoBehaviour
{


    private GameObject player;
    private GameObject training_reward;
    private GameObject panoCam;

    private Rigidbody rb;

    private SP_RunTrain_to_Env1 sp;
    private DL_RunTrain_to_Env1 dl;
    private RR_RunTrain_to_Env1 rotary;

    private bool reward_dir;


    private Vector3 initialPosition;
    private Vector3 movement;

    private int r;
    public int cmd = 0;
    private float LastRewardTime;

    public int rzoneFlag = 0;
    public int mRewardFlag = 0;
    public int tendFlag = 0;
    public int tstartFlag = 0;

    public void Awake()
    {
        GameObject player = GameObject.Find("Player");
        sp = player.GetComponent<SP_RunTrain_to_Env1>();
        rotary = player.GetComponent<RR_RunTrain_to_Env1>();
        dl = player.GetComponent<DL_RunTrain_to_Env1>();
        panoCam = GameObject.Find("panoCamera");
        panoCam.transform.eulerAngles = new Vector3(0.0f, -90.0f, 0.0f);
        training_reward = GameObject.Find("Training_Reward");
        initialPosition = new Vector3(0f, 6f, -5.0f);
    }

    void Start()
    {

        StartCoroutine(FlagCheck());
        // put the mouse in the dark tunnel
        training_reward.transform.position = training_reward.transform.position + new Vector3(0.0f, 0.0f, sp.mrd + UnityEngine.Random.value * sp.ard); ;
        LastRewardTime = Time.realtimeSinceStartup;
    }


    void Update()
    {
        // make sure rotation angle is 0
        transform.eulerAngles = new Vector3(0.0f, -90.0f, 0.0f);

        // end game after appropriate number of trials
        if (sp.numTraversals >= sp.numTrialsTotal | sp.numRewards >= sp.maxRewards)
        {
            UnityEditor.EditorApplication.isPlaying = false;


        }

        if (dl.r > 0) { StartCoroutine(DeliverReward(dl.r)); sp.numRewards++; dl.r = 0; }; // deliver appropriate reward

        // manual rewards and punishments
        mRewardFlag = 0;
        if (Input.GetKeyDown(KeyCode.Q) | Input.GetMouseButtonDown(0)) // reward left
        {
            mRewardFlag = 1;
            StartCoroutine(DeliverReward(4));
        }


    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.tag);
        if (other.tag == "Start")
        {
            tstartFlag = 1;
        }

        else if (other.tag == "Reward")
        {

            StartCoroutine(RewardSequence(transform.position.z));
            // StartCoroutine(MoveReward());

        }
        else if (other.tag == "Teleport")
        {
            training_reward.SetActive(true);
            sp.numTraversals += 1;
            tendFlag = 1;
            transform.position = initialPosition;
            training_reward.transform.position = new Vector3(0.0f, 0f, sp.mrd + UnityEngine.Random.value * sp.ard);
            LastRewardTime = Time.realtimeSinceStartup; // to avoid issues with teleports
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
        training_reward.transform.position = training_reward.transform.position + new Vector3(0f, 0f, sp.mrd + UnityEngine.Random.value * sp.ard);
        LastRewardTime = CurrRewardTime;
        yield return null;
    }

    void OnApplicationQuit()
    {

    }


    IEnumerator RewardSequence(float pos)
    {   // water reward

        // Debug.Log("Reward");
        // bool counted = true;
        rzoneFlag = 1;

        while ((transform.position.z <= pos + 75) & (transform.position.z > 0))
        {
            Debug.Log(cmd);
            cmd = 12;
            if ((sp.AutoReward))// & (counted))
            {
                if (transform.position.z > pos + 30)
                {
                    cmd = 4;
                    if (sp.MultiReward)
                    {
                        StartCoroutine(MoveReward());
                    }
                    sp.numRewards++;
                    //counted = false;
                    break; // if (sp.MultiReward) { break;  };
                           //break;

                }
            }
            else
            {
                cmd = 12;
            }

            if ((dl.c_1 > 0))// & (counted))
            {
                if (sp.MultiReward)
                {

                    StartCoroutine(MoveReward());
                }
                else
                {
                    training_reward.SetActive(false);
                }
                //counted = false;
                //if (sp.MultiReward) { break; };
                break;
            }
            yield return null;
        }
        rzoneFlag = 0;
        yield return new WaitForSeconds(.1f);
        cmd = 2;
        yield return new WaitForSeconds(.1f);
        cmd = 0;

    }



    IEnumerator DeliverReward(int r)
    { // deliver
        if (r == 4)
        {
            cmd = 4;
            yield return new WaitForSeconds(0.01f);
            cmd = 0;
        }
    }

}
