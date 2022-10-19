using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EnvControl_MetaLearn_123_Blocks: MonoBehaviour
{

    private GameObject reward1;
    private GameObject reward2;
    private GameObject reward3;
    private GameObject player;
    private GameObject morphmaze;
    private GameObject xmaze;
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

	morphmaze = GameObject.Find("MorphMaze");
	xmaze = GameObject.Find("XMaze");

	morphmaze.SetActive(true);
	xmaze.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (numTraversalsLocal != sp.numTraversals)
        {
            numTraversalsLocal = sp.numTraversals;

            if ((numTraversalsLocal%10==0) & (numTraversalsLocal > 5))
            {
                Debug.Log("Switch");

                if (numTraversalsLocal%30==0)
                {
                    sp.morph = 0f; // Mathf.Abs(sp.morph - 1.0f);
                
		    morphmaze.SetActive(true);
		    xmaze.SetActive(false);
		}
                else if ( ((numTraversalsLocal-10)==0) | ((numTraversalsLocal-10)%30==0) )
                {
                    sp.morph = 1.0f; // Mathf.Abs(sp.morph - 1.0f);

		    morphmaze.SetActive(true);
		    xmaze.SetActive(false);
		}
		else if ( ((numTraversalsLocal-20)==0) | ((numTraversalsLocal-20)%30==0) )
		
		{
                    sp.morph = 0.5f;

		    morphmaze.SetActive(false);
		    xmaze.SetActive(true);
                }
                    
                //switchCount = switchCount + 1;
		//Debug.Log(switchCount);
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
