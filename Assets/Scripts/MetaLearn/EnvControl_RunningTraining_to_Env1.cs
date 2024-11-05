using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EnvControl_RunningTraining_to_Env1: MonoBehaviour
{

    private GameObject reward_t;
    private GameObject reward;
    private GameObject player;
    private GameObject morphmaze;
    private GameObject trainingmaze;
    private SP_RunTrain_to_Env1 sp;
    private PC_RunTrain_to_Env1 pc;
    private RR_RunTrain_to_Env1 rr;
    private Vector3 initialPosition;

    private int switchCount = 0;

    private int numTraversalsLocal = -1;

    public int ChangeRewardTrial = 30;


    // Use this for initialization
    void Start()
    {
        player = GameObject.Find("Player");
        sp = player.GetComponent<SP_RunTrain_to_Env1>();
        pc = player.GetComponent<PC_RunTrain_to_Env1>();
        rr = player.GetComponent<RR_RunTrain_to_Env1>();

        sp.morph = 0f;
        sp.TrainingTrack = 1;

        reward_t = GameObject.Find("Reward_T");
        reward = GameObject.Find("Reward");

        morphmaze = GameObject.Find("Env1_Maze");
        trainingmaze = GameObject.Find("basic_maze");

        morphmaze.SetActive(false);
        trainingmaze.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        if (numTraversalsLocal != sp.numTraversals)
        {
            rr.speedBool = 0;
            numTraversalsLocal = sp.numTraversals;

            if (numTraversalsLocal == ChangeRewardTrial)
            {
                Debug.Log("Switch");

                sp.morph = 0f; // Mathf.Abs(sp.morph - 1.0f);
                morphmaze.SetActive(true);
                trainingmaze.SetActive(false);
                sp.TrainingTrack = 0;
                pc.bckgndOn = true;
            }

            if (sp.TrainingTrack == 0)
            {
                if (UnityEngine.Random.value < sp.SkipTrialPcnt)
                {
                    reward.SetActive(false);

                }
                else
                {
                    reward.SetActive(true);
                }
                
                reward_t.SetActive(false);
            }
            else
            {
                reward_t.SetActive(true);
                reward.SetActive(false);
            }

            rr.speedBool = 1;
        }
    }
}
