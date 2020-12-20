using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RewardControl_Morph1 : MonoBehaviour
{

    private GameObject reward;
    private GameObject reward_a;
    private GameObject reward_b;

    private GameObject player;
    private SP_NeuroMods sp;
    private PC_NeuroMods pc;

    



    private int numTraversalsLocal = -1;


    // Use this for initialization
    void Start()
    {
        player = GameObject.Find("Player");
        sp = player.GetComponent<SP_NeuroMods>();
        pc = player.GetComponent<PC_NeuroMods>();


        reward_a = GameObject.Find("Reward_A");
        reward_b = GameObject.Find("Reward_B");
        //reward_b.SetActive(false);
        sp.morph = 1f;
        
    }

    // Update is called once per frame
    void Update()
    {
        if (numTraversalsLocal != sp.numTraversals)
        {
            reward_a.SetActive(false);
            numTraversalsLocal = sp.numTraversals;


            if (UnityEngine.Random.value < sp.SkipTrialPcnt)
            {
                reward_b.SetActive(false);

            }
            else
            {
                reward_b.SetActive(true);


            }



        }
    }
}
