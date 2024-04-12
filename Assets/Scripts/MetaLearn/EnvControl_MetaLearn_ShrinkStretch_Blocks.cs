using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EnvControl_MetaLearn_ShrinkStretch_Blocks: MonoBehaviour
{

    private GameObject reward1;
    private GameObject reward2;
    // private GameObject reward3;
    private GameObject player;
    private GameObject basicmaze;
    private GameObject shrinkmaze;
    private GameObject stretchmaze;
    private SP_MetaLearn sp;
    private PC_MetaLearn pc;
    private Vector3 initialPosition;

    private int switchCount = 0;
    public int ChangeEnvTrial = 60;

    private int numTraversalsLocal = -1;


    // Use this for initialization
    void Start()
    {
        player = GameObject.Find("Player");
        sp = player.GetComponent<SP_MetaLearn>();
        pc = player.GetComponent<PC_MetaLearn>();
        
        sp.morph = 0f;

        reward1 = GameObject.Find("Reward_A");
        reward2 = GameObject.Find("Reward_B");
        // reward2 = GameObject.Find("Reward_C");

        basicmaze = GameObject.Find("basicmaze");
        shrinkmaze = GameObject.Find("shrinkmaze");
        stretchmaze = GameObject.Find("stretchmaze");
     

        // towers1 = GameObject.Find("Tower 1");
        // towers2 = GameObject.Find("Tower 2");
        // towers3 = GameObject.Find("RewardTower1");
        // towers4 = GameObject.Find("RewardTower2");

        //reward.transform.position = reward.transform.position + new Vector3(0f, 0f, sp.mrd
        // initial tower positions: 45, 165, 285, 405
        // towers1.transform.position = new Vector3(0f, 0f, 45f)
        // towers2.transform.position = new Vector3(0f, 0f, 165f)
        // towers3.transform.position = new Vector3(0f, 0f, 285f)
        // towers4.transform.position = new Vector3(0f, 0f, 405f)

        basicmaze.SetActive(true);
      
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

                if (numTraversalsLocal%ChangeEnvTrial==0)
                {
                   basicmaze.SetActive(false);
                   shrinkmaze.SetActive(true);

                    
                }
                else if ( ((numTraversalsLocal-ChangeEnvTrial/2)==0) | ((numTraversalsLocal-ChangeEnvTrial/2)%ChangeEnvTrial==0) )
                {
                    
                    basicmaze.SetActive(true);
                    shrinkmaze.SetActive(false);
                
                }
                else if ( ((numTraversalsLocal-ChangeEnvTrial/2)==0) | ((numTraversalsLocal-ChangeEnvTrial/2)%ChangeEnvTrial==0) )
                {

                    stretchmaze.SetActive(true);
                    basicmaze.SetActive(false);
            
                }
                    
                //switchCount = switchCount + 1;
                //Debug.Log(switchCount);
            }

            
            


        }
    }
}
