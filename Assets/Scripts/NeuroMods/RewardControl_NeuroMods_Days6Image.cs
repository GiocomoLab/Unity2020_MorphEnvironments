using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RewardControl_NeuroMods_Day6Image: MonoBehaviour
{

    private GameObject reward;
    private GameObject reward_a;
    private GameObject reward_b;

    private GameObject player;
    private SP_NeuroMods sp;
    private PC_NeuroMods pc;

    public int ChangeRewardTrial = 20;



    private int numTraversalsLocal = -1;
    private float morph = -1;

    // Use this for initialization
    void Start()
    {
        player = GameObject.Find("Player");
        sp = player.GetComponent<SP_NeuroMods>();
        pc = player.GetComponent<PC_NeuroMods>();


        reward_a = GameObject.Find("Reward_A");
        reward_b = GameObject.Find("Reward_B");

    }

    // Update is called once per frame
    void Update()
    {
        if (numTraversalsLocal != sp.numTraversals | morph != sp.morph)
        {
            numTraversalsLocal = sp.numTraversals;
            
            if (numTraversalsLocal>=ChangeRewardTrial)
            {
                reward = reward_a;
            }
            else
            {
                reward = reward_b;
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
