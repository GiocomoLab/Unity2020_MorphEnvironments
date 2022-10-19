using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EnvControl_MetaLearn_123_Blocks: MonoBehaviour
{

    private GameObject reward1;
    private GameObject reward2;
    private GameObject reward3;
    private GameObject player;
    private SP_NeuroMods sp;
    private PC_NeuroMods pc;
    private Vector3 initialPosition;

    private int switchCount = 0;

    private int numTraversalsLocal = -1;


    // Use this for initialization
    void Start()
    {
        player = GameObject.Find("Player");
        sp = player.GetComponent<SP_NeuroMods>();
        pc = player.GetComponent<PC_NeuroMods>();
        
        sp.morph = 0f;

        reward3 = GameObject.Find("Reward_A");
        reward1 = GameObject.Find("Reward_B");
        reward2 = GameObject.Find("Reward_C");
    }

    // Update is called once per frame
    void Update()
    {
        if (numTraversalsLocal != sp.numTraversals)
        {
            numTraversalsLocal = sp.numTraversals;

            if (numTraversalsLocal > 5)
            {
            
                if (numTraversalsLocal%20==0)
                {   
                    Debug.Log("Switch");
                    sp.morph = 0.5f; // Mathf.Abs(sp.morph - 1.0f);
                    //reward.transform.position = new Vector3(0f, 0f, 250.0f + 150.0f * UnityEngine.Random.value); ;
                }
                else if (numTraversalsLocal%30==0)
                {
                    Debug.Log("Switch");
                    sp.morph = 0f; // Mathf.Abs(sp.morph - 1.0f);
                }
                else if (numTraversalsLocal%10==0)
                {
                    Debug.Log("Switch");
                    sp.morph = 1.0f; // Mathf.Abs(sp.morph - 1.0f);
		    switchCount = switchCount + 1;
                }
            }

            if (sp.morph==0f)
            {
                if (UnityEngine.Random.value < sp.SkipTrialPcnt)
                {
                    reward1.SetActive(false);

                }
                else
                {
                    reward1.SetActive(true);
                }
                
                reward2.SetActive(false);
                reward3.SetActive(false);
            }

            else if (sp.morph==1.0f)
            {
                if (UnityEngine.Random.value < sp.SkipTrialPcnt)
                {
                    reward2.SetActive(false);

                }
                else
                {
                    reward2.SetActive(true);
                }
                
                reward1.SetActive(false);
                reward3.SetActive(false);

            }
            else
            {

                if (UnityEngine.Random.value < sp.SkipTrialPcnt)
                {
                    reward3.SetActive(false);

                }
                else
                {
                    reward3.SetActive(true);
                }
                
                reward1.SetActive(false);
                reward2.SetActive(false);
            }
            


        }
    }
}
