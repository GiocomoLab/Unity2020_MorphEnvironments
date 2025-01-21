using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RewardControl_MetaLearn_B_to_C: MonoBehaviour
{

    private GameObject reward;
    private GameObject reward_b;
    private GameObject reward_c;

    private GameObject player;
    private SP_MetaLearn sp;
    private PC_MetaLearn pc;

    public int ChangeRewardTrial = 30;



    private int numTraversalsLocal = -1;
    

    // Use this for initialization
    void Start()
    {
        player = GameObject.Find("Player");
        sp = player.GetComponent<SP_MetaLearn>();
        pc = player.GetComponent<PC_MetaLearn>();


        reward_b = GameObject.Find("Reward_B");
        reward_c = GameObject.Find("Reward_C");

    }

    // Update is called once per frame
    void Update()
    {
        if (numTraversalsLocal != sp.numTraversals )
        {
            numTraversalsLocal = sp.numTraversals;
            
            if (numTraversalsLocal<ChangeRewardTrial)
            {
                reward = reward_b;
                reward_c.SetActive(false);
            }
            else
            {
                reward = reward_c;
                reward_b.SetActive(false);
            }

            if ((numTraversalsLocal >= ChangeRewardTrial) & (numTraversalsLocal <= ChangeRewardTrial + 9))
            {
                sp.AutoReward = true;
            }
            else
            {
                if (numTraversalsLocal == ChangeRewardTrial + 10)
                {
                    // turn it off at the end of the 10-trial period, otherwise leave it alone
                    sp.AutoReward = false;
                }

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
