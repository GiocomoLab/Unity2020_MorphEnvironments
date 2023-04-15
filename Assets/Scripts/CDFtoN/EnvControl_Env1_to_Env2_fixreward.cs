using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvControl_Env1_to_Env2_fixreward : MonoBehaviour
{

    // private GameObject reward_t;
    private GameObject reward;
    private GameObject player;
    private GameObject Env1_maze;
    private GameObject Env2_maze;
    private SP_runtrain_fixreward sp;
    private PC_runtrain_fixreward pc;
    private RR_runtrain_fixreward rr;
    private Vector3 initialPosition;

    private int switchCount = 0;

    private int numTraversalsLocal = -1;

    public int ChangeRewardTrial = 30;


    // Use this for initialization
    void Start()
    {
        player = GameObject.Find("Player");
        sp = player.GetComponent<SP_runtrain_fixreward>();
        pc = player.GetComponent<PC_runtrain_fixreward>();
        rr = player.GetComponent<RR_runtrain_fixreward>();

        sp.morph = 0f;
        sp.TrainingTrack = 1;

        // reward_t = GameObject.Find("Reward_T");
        reward = GameObject.Find("Reward");

        Env1_maze = GameObject.Find("Env1_Maze");
        Env2_maze = GameObject.Find("Env2_Maze");

        Env1_maze.SetActive(true);
        Env2_maze.SetActive(false);
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
                Env1_maze.SetActive(false);
                Env2_maze.SetActive(true);
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

                //reward_t.SetActive(false);
            }
            else
            {
                //reward_t.SetActive(true);
                reward.SetActive(true);
            }

            rr.speedBool = 1;
        }
    }
}

