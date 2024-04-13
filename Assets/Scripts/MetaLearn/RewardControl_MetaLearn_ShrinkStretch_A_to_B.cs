using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RewardControl_MetaLearn_ShrinkStretch_A_to_B: MonoBehaviour
{

    private GameObject reward;
    private GameObject reward_a;
    private GameObject reward_b;

    private GameObject player;
    private SP_MetaLearn sp;
    private PC_MetaLearn pc;
    private EnvControl_MetaLearn_ShrinkStretch_Blocks env;

    public int ChangeRewardTrial = 30;
    private int changeEnvTrial;



    private int numTraversalsLocal = -1;
    

    // Use this for initialization
    void Start()
    {
        player = GameObject.Find("Player");
        sp = player.GetComponent<SP_MetaLearn>();
        pc = player.GetComponent<PC_MetaLearn>();
        env = player.GetComponent<EnvControl_MetaLearn_ShrinkStretch_Blocks>();
        changeEnvTrial = env.ChangeEnvTrial;


        reward_a = GameObject.Find("Reward_A");
        reward_b = GameObject.Find("Reward_B");

    }

    // Update is called once per frame
    void Update()
    {
        if (numTraversalsLocal != sp.numTraversals )
        {
            numTraversalsLocal = sp.numTraversals;
            
            if (numTraversalsLocal<ChangeRewardTrial)
            {
                reward = reward_a;
                reward_b.SetActive(false);
            }
            else
            {
                reward = reward_b;
                reward_a.SetActive(false);
            }

            //if ((numTraversalsLocal >= ChangeRewardTrial) & (numTraversalsLocal <= ChangeRewardTrial + 9))
            //{
            //    sp.AutoReward = true;
            //}
            //else
            //{
            //    sp.AutoReward = false;
            //}

            // Also turn auto reward on for 5 trials if we just changed environments
            if ((numTraversalsLocal >= ChangeRewardTrial) & (numTraversalsLocal <= ChangeRewardTrial + 9))
            {
                sp.AutoReward = true;
            }
            else if ((numTraversalsLocal >= changeEnvTrial) & (numTraversalsLocal <= changeEnvTrial + 4))
            {
                sp.AutoReward = true;
            }
            else if ((numTraversalsLocal >= changeEnvTrial + changeEnvTrial/2) & (numTraversalsLocal <= changeEnvTrial + changeEnvTrial/2 + 4))
            {
                sp.AutoReward = true;
            }
            else if ((numTraversalsLocal >= changeEnvTrial*2) & (numTraversalsLocal <= changeEnvTrial*2 + 4))
            {
                sp.AutoReward = true;
            }
            else
            {
                sp.AutoReward = false;
            }



            if (UnityEngine.Random.value < sp.SkipTrialPcnt)
            {
                reward.SetActive(false);

            }
            else
            {
                reward.SetActive(true);


            }



        }
    }
}
