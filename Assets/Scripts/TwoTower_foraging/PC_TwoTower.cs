﻿using UnityEngine;
using System;
using System.Collections;
using System.IO;
using System.IO.Ports;
using System.Threading;





public class PC_TwoTower : MonoBehaviour
{


    private GameObject player;
    
    private GameObject blackCam;
    private GameObject panoCam;
    private GameObject reward0;
    private GameObject reward1;
    private GameObject reward;
    
    


    private Rigidbody rb;

    private SP_TwoTower sp;
    private DL_TwoTower dl;
    private RR_TwoTower rotary;

    private bool reward_dir;


    private Vector3 initialPosition;
    private Vector3 movement;

    private static bool created = false;
    private int r;
    private int r_last = 0;
    //private bool rflag = true;

    public int cmd = 2;
    private bool flashFlag = false;
    private float LastRewardTime;
    private int prevReward = 0;

    public ArrayList LickHistory;
    public bool bckgndOn = true;

    public float towerJitter;
    public float wallJitter;
    public float bckgndJitter;

    public int mRewardFlag = 0;
    public int rzoneFlag = 0;
    public int toutzoneFlag = 0;
    public int tendFlag = 0;
    public int tstartFlag = 0;
    private bool NoSkipTO = true;




    public void Awake()
    {


        GameObject player = GameObject.Find("Player");
        sp = player.GetComponent<SP_TwoTower>();
        rotary = player.GetComponent<RR_TwoTower>();
        dl = player.GetComponent<DL_TwoTower>();
        Debug.Log(sp.sceneName);
        if (sp.sceneName == "TwoTower_foraging")
        {
           
            reward = GameObject.Find("Reward");
        }
        else
        {
            
            reward0 = GameObject.Find("Reward0");
            reward1 = GameObject.Find("Reward1");
        }
       // Debug.Log(sp.sceneName);



        panoCam = GameObject.Find("panoCamera");
        panoCam.transform.eulerAngles = new Vector3(0.0f, -90.0f, 0.0f);
        initialPosition = new Vector3(0f, 6f, -50.0f);

        
        //towerJitter = .2f * (UnityEngine.Random.value - .5f);
        //wallJitter = .2f * (UnityEngine.Random.value - .5f);
        //bckgndJitter = .2f * (UnityEngine.Random.value - .5f);

        LickHistory = new ArrayList();
    }

    void Start()
    {
        StartCoroutine(FlagCheck());
        if ((sp.sceneName != "SingleMorph") & (sp.sceneName != "SingleMorph_Reversal"))
        {
            Debug.Log("failed");
            towerJitter = .2f * (UnityEngine.Random.value - .5f);
            wallJitter = .2f * (UnityEngine.Random.value - .5f);
            bckgndJitter = .2f * (UnityEngine.Random.value - .5f);
        }
        else
        {
            towerJitter = 0f;
            wallJitter = 0f;
            bckgndJitter = 0f;
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



    }

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

            if ((sp.sceneName != "SingleMorph") & (sp.sceneName != "SingleMorph_Reversal"))
            {
                towerJitter = .2f * (UnityEngine.Random.value - .5f);
                wallJitter = .2f * (UnityEngine.Random.value - .5f);
                bckgndJitter = .2f * (UnityEngine.Random.value - .5f);
            }
            //towerJitter = .2f * (UnityEngine.Random.value - .5f);
            //wallJitter = .2f * (UnityEngine.Random.value - .5f);
            //bckgndJitter = .2f * (UnityEngine.Random.value - .5f);


            sp.numTraversals += 1;
            tendFlag = 1;
            transform.position = initialPosition;
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


    IEnumerator FlagCheck() {
      while (true) {
        yield return new WaitForEndOfFrame();
        tendFlag = 0;
        tstartFlag = 0;
      }
      yield return null;

    }
    IEnumerator InterTrialTimeout()
    {

        rotary.toutBool = 0f;
        if (prevReward == 0) // omission
        {
            yield return new WaitForSeconds(5f + UnityEngine.Random.value * 5f); // 10f); // 7f);

        }
        else
        {
            yield return new WaitForSeconds(1f + UnityEngine.Random.value*4f);
        }

        rotary.toutBool = 1f;
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

        //bool counted = true;
        while ((transform.position.z <= pos + 75) & (transform.position.z > 100)) // & (rflag))  //& (counted)
        {
            
            
            if ((sp.AutoReward) & (transform.position.z > pos + 50)) // & (counted) )
            {
     
               // if (transform.position.z > pos + 50)
                //{
                cmd = 4;
                //counted = false;
                //rflag = false;
                    
                // yield return new WaitForSeconds(.01f);
                //cmd = 0;
                LickHistory.Add(.0f);
                StartCoroutine(DeliverReward(1));
                sp.numRewards += 1;
                yield return new WaitForEndOfFrame();
                break;
                   
                //}
               // else
                //{
                //    cmd = 12;
                //}
           // } else
            //{
             //   cmd = 12;
            }

            if ((dl.c_1 > 0))// & (counted))
            {
                    //counted = false;
                    //rflag = false;
                cmd = 4;
                sp.numRewards += 1;
                if (sp.morph + wallJitter + bckgndJitter<=.5) //(sp.morph == 0)
                {

                    LickHistory.Add(-1f); //.66f);
                }
                else if (sp.morph + wallJitter + bckgndJitter>.5) //(sp.morph == 1)
                {

                    LickHistory.Add(1f); //.33f);
                }
                else // morph trials
                {

                    LickHistory.Add(.0f);
                }
                //LickHistory.Add(0f);
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
        if (sp.sceneName == "TwoTower_foraging")
        {
            
            reward.SetActive(false);
        }
        else
        {
           
            reward0.SetActive(false); reward1.SetActive(false);
        }
        
        
        
        
        //cmd = 2;
        rzoneFlag = 0;
        yield return new WaitForEndOfFrame();
        //yield return new WaitForSeconds(.1f);
        cmd = 2;

    }

    IEnumerator TimeoutSequence(float pos)
    {
        toutzoneFlag = 1;
        if (sp.morph == .5f)
        {
            NoSkipTO = false;
        }
        else
        {
            NoSkipTO = true;
        }

        while ((transform.position.z<=pos + 65) )
        {
            cmd = 0;
            if (dl.c_1 > 0)
            {
                if (sp.morph == 0)
                {

                    LickHistory.Add(1f); //.66f);
                }
                else if (sp.morph == 1)
                {

                    LickHistory.Add(-1f); //.33f);
                }
                else // morph trials
                {

                    LickHistory.Add(.0f);
                }

                toutzoneFlag = 0;
                tendFlag = 1;
                transform.position = initialPosition;
                sp.numTraversals += 1;
                cmd = 2;
                bckgndOn = true;


                if ((sp.sceneName != "SingleMorph") & (sp.sceneName != "SingleMorph_Reversal"))
                {
                    towerJitter = .2f * (UnityEngine.Random.value - .5f);
                    wallJitter = .2f * (UnityEngine.Random.value - .5f);
                    bckgndJitter = .2f * (UnityEngine.Random.value - .5f);
                }
                
                
                // rflag = true;
                // avoid .5 timeout just end trial
                if (NoSkipTO)
                {
                    rotary.toutBool = 0f;
                    yield return new WaitForSeconds(5f);
                  //  if (sp.morph == 0f)
                  //  {
                  //      yield return new WaitForSeconds(5f);
                   // }
                    // sp.morph = nextMorph;
                    yield return new WaitForSeconds(5f);
                    rotary.toutBool = 1f;
                }
                else
                {
                    rotary.toutBool = 0f;
                    yield return new WaitForSeconds(1f);
                    rotary.toutBool = 1f;
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


    IEnumerator DeliverReward(int r)
    { // deliver

        if (r == 1) // reward left
        {
            prevReward = 1;
            //sp.numRewards += 1;
            yield return null;
        }
        else if (r ==4)
        {
            cmd = 4;
            //sp.numRewards += 1;
            yield return new WaitForSeconds(0.01f);
            cmd = 0;
        }
        else if (r==5)
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
