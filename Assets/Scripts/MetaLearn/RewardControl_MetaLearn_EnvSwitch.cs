using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RewardControl_MetaLearn_EnvSwitch: MonoBehaviour
{

    private GameObject reward0;
    private GameObject reward1;
    private GameObject player;
    private SP_NeuroMods sp;
    private PC_NeuroMods pc;
    private Vector3 initialPosition;

    public int ChangeRewardTrial = 30;

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

            if (numTraversalsLocal==ChangeRewardTrial)
            //if ((numTraversalsLocal%10==0) & (sp.numTraversals > 5))
            {
                Debug.Log("Switch");
                sp.morph = Mathf.Abs(sp.morph - 1.0f);
                //reward.transform.position = new Vector3(0f, 0f, 250.0f + 150.0f * UnityEngine.Random.value); ;
            }

	    if ((numTraversalsLocal >= ChangeRewardTrial) & (numTraversalsLocal <= ChangeRewardTrial+9))
	    {
		    sp.AutoReward = true;
	    }
	    else
	    {
		    sp.AutoReward = false;
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
