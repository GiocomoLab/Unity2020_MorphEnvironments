using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EnvControl_MetaLearn_ShrinkStretch_Blocks: MonoBehaviour
{

    private GameObject reward1;
    private GameObject reward2;
    private GameObject player;
    private GameObject basicmaze;
    private GameObject shrinkmaze;
    private GameObject stretchmaze;

    private GameObject towers1;
    private GameObject towers2;
    private GameObject towers3;
    private GameObject towers4;
    private GameObject endWall;

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

                    //towers1.transform.position = towers1.transform.position - new Vector3(0f, 0f, 15f);
                    //towers2.transform.position = towers2.transform.position - new Vector3(0f, 0f, 55f);
                    //towers3.transform.position = towers3.transform.position - new Vector3(0f, 0f, 95f);
                    //towers4.transform.position = towers4.transform.position - new Vector3(0f, 0f, 135f);
                    //endWall.transform.position = endWall.transform.position - new Vector3(0f, 0f, 150f);

                }
                //else if ( ((numTraversalsLocal-10)==0) | ((numTraversalsLocal-10)%30==0) )
                //{
                //    sp.morph = 1.0f; // Mathf.Abs(sp.morph - 1.0f);
                //    morphmaze.SetActive(true);
                
                //}
                //else if ( ((numTraversalsLocal-20)==0) | ((numTraversalsLocal-20)%30==0) )
                //{
                //    sp.morph = 0.5f;
                //    morphmaze.SetActive(false);
            
                //}
                    
                //switchCount = switchCount + 1;
                //Debug.Log(switchCount);
            }


            //if (sp.morph==0f)
            //{
            //    if (UnityEngine.Random.value < sp.SkipTrialPcnt)
            //    {
            //        reward1.SetActive(false);

            //    }
            //    else
            //    {
            //        reward1.SetActive(true);
            //    }
                
            //    reward2.SetActive(false);
            //}

            //else if (sp.morph==1.0f)
            //{
            //    if (UnityEngine.Random.value < sp.SkipTrialPcnt)
            //    {
            //        reward2.SetActive(false);

            //    }
            //    else
            //    {
            //        reward2.SetActive(true);
            //    }
                
            //    reward1.SetActive(false);

            //}            
            

        }
    }
}
