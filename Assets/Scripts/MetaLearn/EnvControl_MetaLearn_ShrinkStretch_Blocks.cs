using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EnvControl_MetaLearn_ShrinkStretch_Blocks: MonoBehaviour
{

    private GameObject reward1;
    private GameObject reward2;
    private GameObject player;
    private GameObject basic_maze;
    private GameObject shrink_maze;
    private GameObject stretch_maze;

    private GameObject towers1;
    private GameObject towers2;
    private GameObject towers3;
    private GameObject towers4;
    private GameObject endWall;
    private GameObject sineGroup;

    private SP_MetaLearn sp;
    private PC_MetaLearn pc;
    //private MakeSineOnChildren_MetaLearn_StretchShrink msw;
    private Vector3 initialPosition;
    private Vector3 reward1_initialPosition;
    private Vector3 reward2_initialPosition;
    public float wallScale=1;


    private int switchCount = 0;
    public int ChangeEnvTrial = 60;

    private int numTraversalsLocal = -1;


    // Use this for initialization
    void Start()
    {
        player = GameObject.Find("Player");
        sp = player.GetComponent<SP_MetaLearn>();
        pc = player.GetComponent<PC_MetaLearn>();
        
        //sp.morph = 0f;

        reward1 = GameObject.Find("Reward_A");
        reward2 = GameObject.Find("Reward_B");
        reward1_initialPosition = reward1.transform.position;
        reward2_initialPosition = reward2.transform.position;
        basic_maze = GameObject.Find("basic_maze");
        shrink_maze = GameObject.Find("shrink_maze");
        stretch_maze = GameObject.Find("stretch_maze");
        sineGroup = GameObject.Find("SineWalls1");

        basic_maze.SetActive(true);
        stretch_maze.SetActive(false);
        shrink_maze.SetActive(false);

    }

    // Update is called once per frame
    void Update()
    {
        if (numTraversalsLocal != sp.numTraversals)
        {
            numTraversalsLocal = sp.numTraversals;

            if ((numTraversalsLocal%10==0) & (numTraversalsLocal > 5))
            {

                if ( (numTraversalsLocal%ChangeEnvTrial==0) & (switchCount%2==0) )
                {

                    basic_maze.SetActive(false);
                    stretch_maze.SetActive(false);
                    shrink_maze.SetActive(true);
                    //sineGroup.transform.localScale = new Vector3(1f, 1f, 0.6666f);
                    wallScale = 0.6666f;
                    reward2.transform.position = new Vector3(0f, 0f, Mathf.RoundToInt(reward2_initialPosition.z*0.6666f));
                    switchCount += 1;

                    
                }
                else if ( ((numTraversalsLocal-ChangeEnvTrial/2)%ChangeEnvTrial==0) )
                {
                    
                    basic_maze.SetActive(true);
                    //sineGroup.transform.localScale = new Vector3(1f, 1f, 1f);
                    wallScale = 1f;
                    shrink_maze.SetActive(false);
                    stretch_maze.SetActive(false);
                    reward2.transform.position = reward2_initialPosition;

                }
                else if ( (numTraversalsLocal % ChangeEnvTrial ==0) & (switchCount%2 != 0))
                {

                    stretch_maze.SetActive(true);
                    //sineGroup.transform.localScale = new Vector3(1f, 1f, 1.3333f);
                    wallScale =1.3333f;
                    basic_maze.SetActive(false);
                    shrink_maze.SetActive(false);
                    reward2.transform.position = new Vector3(0f, 0f, Mathf.RoundToInt(reward2_initialPosition.z * 1.3333f));
                    switchCount += 1;


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
