using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EnvControl_MetaLearn_12_Blocks_RewardSwap: MonoBehaviour
{

    private GameObject reward0;
    private GameObject reward1;
    private GameObject player;
    private SP_NeuroMods sp;
    private PC_NeuroMods pc;
    private Vector3 initialPosition;



    private int numTraversalsLocal = -1;


    // Use this for initialization
    void Start()
    {
        player = GameObject.Find("Player");
        sp = player.GetComponent<SP_NeuroMods>();
        pc = player.GetComponent<PC_NeuroMods>();


        reward0 = GameObject.Find("Reward_A");
        reward1 = GameObject.Find("Reward_B");
    }

    // Update is called once per frame
    void Update()
    {
        if (numTraversalsLocal != sp.numTraversals)
        {
            numTraversalsLocal = sp.numTraversals;

	        if (numTraversalsLocal==60)
            {
                reward0.transform.position = new Vector3(0f, 0f, 345.0f);
                reward1.transform.position = new Vector3(0f, 0f, 225.0f);
            }
            //{
        	  //  reward1 = GameObject.Find("Reward_A");
        	  //  reward0 = GameObject.Find("Reward_B");
		    //}
	        //else
		   // {
        	 //   reward0 = GameObject.Find("Reward_A");
        	 //   reward1 = GameObject.Find("Reward_B");
		    //}

            if ((numTraversalsLocal%10==0) & (sp.numTraversals > 5))
            {
                Debug.Log("Switch");
                sp.morph = Mathf.Abs(sp.morph - 1.0f);
                //reward.transform.position = new Vector3(0f, 0f, 250.0f + 150.0f * UnityEngine.Random.value); ;
            }

            if (sp.morph==0f)
            {
                if (UnityEngine.Random.value < sp.SkipTrialPcnt)
                {
                    reward0.SetActive(false);

                }
                else
                {
                    reward0.SetActive(true);
                }
                
                reward1.SetActive(false);
            }
            else
            {

                if (UnityEngine.Random.value < sp.SkipTrialPcnt)
                {
                    reward1.SetActive(false);

                }
                else
                {
                    reward1.SetActive(true);
                }
                
                reward0.SetActive(false);
            }
            


        }
    }
}
