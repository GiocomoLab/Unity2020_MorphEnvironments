using UnityEngine;
using System;
using System.Collections;
using System.IO;
using System.IO.Ports;
using System.Threading;

public class PC_YMazeTrain : MonoBehaviour
{

    private GameObject player;
    private GameObject rewardL;
    private GameObject rewardR;
    

    private Rigidbody rb;

    private SP_YMazeTrain sp;
    private DL_YMazeTrain dl;
    private RR_YMazeTrain rr;


    private Vector3 initialPosition;
    private Vector3 movement;

    private int r;
    public int cmd = 0;
    private float LastRewardTime;


    public int mRewardFlag = 0;
    public int tendFlag = 0;
    public int tstartFlag = 0;

    public void Awake()
    {
        GameObject player = GameObject.Find("Player");
        sp = player.GetComponent<SP_YMazeTrain>();
        rr = player.GetComponent<RR_YMazeTrain>();
        dl = player.GetComponent<DL_YMazeTrain>();
        rewardL = GameObject.Find("RewardL");
        rewardR = GameObject.Find("RewardR");

        
    }

    void Start()
    {

        StartCoroutine(FlagCheck());
        
    }


    void Update()
    {

        // end game after appropriate number of trials
        if (sp.numTraversals >= sp.numTrialsTotal | sp.numRewards >= sp.maxRewards)
        {
            UnityEditor.EditorApplication.isPlaying = false;
        }

        if (dl.r > 0) { StartCoroutine(DeliverReward(dl.r));  dl.r = 0; }; // deliver appropriate reward

        // manual rewards and punishments
        mRewardFlag = 0;
        if (Input.GetKeyDown(KeyCode.Q) | Input.GetMouseButtonDown(0)) 
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
            StartCoroutine(RewardSequence(rr.t/rr.t2dist));
        }
        else if (other.tag == "Teleport")
        {
            if (UnityEngine.Random.value > sp.skipTrialPcnt)
            {
                rewardL.SetActive(true);
                rewardR.SetActive(true);

            }
            else
            {
                rewardL.SetActive(false);
                rewardR.SetActive(false);
            }
            // rewardL.SetActive(true);
            // rewardR.SetActive(true);
            
            tendFlag = 1;
            rr.t = rr.tvec[2];
            sp.numTraversals += 1;
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

    

    void OnApplicationQuit()
    {

    }


    IEnumerator RewardSequence(float pos)
    {   // water reward

        rewardL.SetActive(false);
        rewardR.SetActive(false);
        while (transform.position.z > 0)
        {
            
            Debug.Log(cmd);
            //cmd = 12;
            if ((sp.AutoReward) & (rr.t/rr.t2dist > pos + 25))
            {
                
                cmd = 4;
                sp.numRewards++;
                //rewardL.SetActive(false);
                //rewardR.SetActive(false);
                yield return new WaitForEndOfFrame();
                break; 

                
            }
           

            if ((dl.c_1 > 0) & (rr.t/rr.t2dist < pos + 25))
            {
                cmd = 4;
                sp.numRewards++;
                //rewardL.SetActive(false);
                //rewardR.SetActive(false);
                yield return new WaitForEndOfFrame();
                break;

            }
            yield return null;
        }
        
        yield return new WaitForEndOfFrame();
        cmd = 2;
        

    }



    IEnumerator DeliverReward(int r)
    { // deliver
        if (r == 4)
        {
            cmd = 4;
            yield return new WaitForSeconds(0.01f);
            sp.numRewards++;
            cmd = 0;
        }
    }

}
