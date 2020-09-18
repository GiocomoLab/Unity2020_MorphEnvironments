using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrailOrdering_FrameGrabber : MonoBehaviour {
    

    private SP_TwoTower sp;
    private PC_TwoTower pc;
    private GameObject player;
    private int switchFlag = 0;
    private float rand_val;
    private int numTraversalsLocal = -1;

    private int nMorphs = 5;
    private float dJitter = .025f;
    private float widthJitter = .2f;

    public float[] trialOrder;
    public float[] wallOrder;
    public float[] bckgndOrder;
    public float[] towerOrder;
    private float[] jitList;
    private float[] morphList;
   
    private void Awake()
    {
        player = GameObject.Find("Player");
        sp = player.GetComponent<SP_TwoTower>();
        pc = player.GetComponent<PC_TwoTower>();



    }
    // Use this for initialization
    void Start()
    {
        int nJitSteps = (int) (widthJitter / dJitter +1);
        int totalTrials = (int)nMorphs * nJitSteps * nJitSteps * nJitSteps +1;
        trialOrder = new float[totalTrials];
        wallOrder = new float[totalTrials];
        bckgndOrder = new float[totalTrials];
        towerOrder = new float[totalTrials];
        morphList = new float[5];

        morphList[0] = 0f; morphList[1] = .25f; morphList[2] = .5f; morphList[3] = .75f; morphList[4] = 1f;
        Debug.Log("test morph length"); Debug.Log(morphList[4]);
        trialOrder[totalTrials - 1] = 1;
        Debug.Log("test trialOrder"); Debug.Log(trialOrder[totalTrials - 1]);


        jitList = new float[nJitSteps];
        for (int i = 0; i < nJitSteps; i++)
        {
            jitList[i] = dJitter * (float)i - .1f;
        }

        int trial_counter = 0;
        for (int m = 0; m<5; m++)
        {
            for (int w = 0; w<nJitSteps; w++)
            {
                for (int b=0; b<nJitSteps; b++)
                {
                    for (int t=0; t<nJitSteps; t++)
                    {
                        if (m >= 5)
                        {
                            Debug.Log("M>5");
                        }
                        if (trial_counter > totalTrials-1)
                        {
                            Debug.Log("trial_counter");
                            Debug.Log(trial_counter);
                        }

                        trialOrder[trial_counter] = morphList[m];
                        wallOrder[trial_counter] = jitList[w];
                        bckgndOrder[trial_counter] = jitList[b];
                        towerOrder[trial_counter] = jitList[t];
                        trial_counter++;
                    }
                }
            }
        }


        // for every 8 trialsS
        sp.numTrialsTotal = totalTrials;


    }

    // Update is called once per frame
    void Update()
    {

        if (player.transform.position.z < 0 & numTraversalsLocal != sp.numTraversals)
        {
            numTraversalsLocal++;
            //servoport.servoFlag = true;
            //Debug.Log(pc.LickHistory[0]);
            sp.morph = trialOrder[numTraversalsLocal];
            pc.wallJitter = wallOrder[numTraversalsLocal];
            pc.bckgndJitter = bckgndOrder[numTraversalsLocal];
            pc.towerJitter = towerOrder[numTraversalsLocal];
           // Debug.Log(sp.morph);
            //Debug.Log(pc.wallJitter);
            //Debug.Log(pc.bckgndJitter);
            //Debug.Log(pc.towerJitter);
        }

    }

  
}
