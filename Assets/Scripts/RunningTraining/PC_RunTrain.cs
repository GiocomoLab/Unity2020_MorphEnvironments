using UnityEngine;
using System;
using System.Collections;
using System.IO;
using System.IO.Ports;
using System.Threading;




public class PC_RunTrain : MonoBehaviour
{


    private GameObject player;
    private GameObject reward;
    private GameObject panoCam;

    private Rigidbody rb;

    private SP_RunTrain sp;
    private DL_RunTrain dl;
    private RR_RunTrain rotary;

    private bool reward_dir;


    private Vector3 initialPosition;
    private Vector3 movement;

    private int r;
    public int cmd = 0;
    private float LastRewardTime;


    public int mRewardFlag = 0;
    public int tendFlag = 0;
    public int tstartFlag = 0;
    public int frameRate = 60;

    public void Awake()
    {
        GameObject player = GameObject.Find("Player");
        sp = player.GetComponent<SP_RunTrain>();
        rotary = player.GetComponent<RR_RunTrain>();
        dl = player.GetComponent<DL_RunTrain>();
        panoCam = GameObject.Find("panoCamera");
        panoCam.transform.eulerAngles = new Vector3(0.0f, -90.0f, 0.0f);
        reward = GameObject.Find("Reward");
        initialPosition = new Vector3(0f, 6f, -5.0f);
    }

    void Start()
    {

        StartCoroutine(FlagCheck());
        // put the mouse in the dark tunnel
        reward.transform.position = reward.transform.position + new Vector3(0.0f, 0.0f, sp.mrd + UnityEngine.Random.value * sp.ard); ;
        LastRewardTime = Time.realtimeSinceStartup;
        Application.targetFrameRate = frameRate;
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

        if (dl.r > 0 & dl.rflag < 1) { StartCoroutine(DeliverReward(dl.r)); dl.rflag = 1; }; // deliver appropriate reward

        //if (dl.r > 0) 
	//{
	//	StartCoroutine(DeliverReward(dl.r));
	//	dl.r = 0;
	//}; // deliver appropriate reward

        // manual rewards and punishments
        mRewardFlag = 0;
        if (Input.GetKeyDown(KeyCode.Q) | Input.GetMouseButtonDown(0)) // reward left
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
            reward.SetActive(true);
            sp.numTraversals += 1;
            tendFlag = 1;
            transform.position = initialPosition;
            reward.transform.position = new Vector3(0.0f, 0f, sp.mrd + UnityEngine.Random.value * sp.ard);
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
        reward.transform.position = reward.transform.position + new Vector3(0f, 0f, sp.mrd + UnityEngine.Random.value * sp.ard);
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
        while ((transform.position.z <= pos + 75))
        {


            if ((sp.AutoReward) & (transform.position.z > pos + 50))
            {


                cmd = 4;
                if (sp.MultiReward)
                {
                    StartCoroutine(MoveReward());
                }
                StartCoroutine(DeliverReward(1));
                sp.numRewards += 1;
                yield return new WaitForEndOfFrame();
                break;


            }

            if (dl.c_1 > 0)
            {

                cmd = 4;
                sp.numRewards += 1;
                if (sp.MultiReward)
                {

                    StartCoroutine(MoveReward());
                }
                else
                {
                    reward.SetActive(false);
                }
                yield return new WaitForEndOfFrame();
                break;
            }
            yield return new WaitForEndOfFrame();

        }

       
        yield return new WaitForEndOfFrame();
        cmd = 2;
        yield return new WaitForEndOfFrame();
        cmd = 0;

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
