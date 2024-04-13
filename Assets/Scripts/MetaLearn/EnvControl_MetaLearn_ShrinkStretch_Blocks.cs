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
    private Vector3 initialPosition;
    private Vector3 reward1_initialPosition;
    private Vector3 reward2_initialPosition;
    public float scale = 1;


    private int switchCount = 0;
    public int ChangeEnvTrial = 60;
    public bool StretchFirst = false;

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
                    if (StretchFirst)
                    {
                        stretch_maze.SetActive(true);
                        shrink_maze.SetActive(false);
                        scale = 1.3333f;
                    }
                    else
                    {
                        stretch_maze.SetActive(false);
                        shrink_maze.SetActive(true);
                        scale = 0.6666f;

                    }

                    //sineGroup.transform.localScale = new Vector3(1f, 1f, 0.6666f);
                    reward2.transform.position = new Vector3(0f, 0f, Mathf.RoundToInt(reward2_initialPosition.z*scale));
                    switchCount += 1;

                    
                }
                else if ( ((numTraversalsLocal-ChangeEnvTrial/2)%ChangeEnvTrial==0) )
                {
                    
                    basic_maze.SetActive(true);
                    //sineGroup.transform.localScale = new Vector3(1f, 1f, 1f);
                    scale = 1f;
                    shrink_maze.SetActive(false);
                    stretch_maze.SetActive(false);
                    reward2.transform.position = reward2_initialPosition;

                }
                else if ( (numTraversalsLocal % ChangeEnvTrial ==0) & (switchCount%2 != 0))
                {
                    basic_maze.SetActive(false);
                    if (StretchFirst)
                    {
                        stretch_maze.SetActive(false);
                        shrink_maze.SetActive(true);
                        scale = 0.6666f;
                    }
                    else
                    {
                        stretch_maze.SetActive(true);
                        shrink_maze.SetActive(false);
                        scale = 1.3333f;

                    }
                    //sineGroup.transform.localScale = new Vector3(1f, 1f, 1.3333f);
                    reward2.transform.position = new Vector3(0f, 0f, Mathf.RoundToInt(reward2_initialPosition.z * scale));
                    switchCount += 1;


                }

            }      
            

        }
    }
}
