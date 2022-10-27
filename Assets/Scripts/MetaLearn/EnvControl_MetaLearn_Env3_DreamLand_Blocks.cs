using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EnvControl_MetaLearn_Env3_DreamLand_Blocks: MonoBehaviour
{

    private GameObject reward0;
    private GameObject reward1;
    private GameObject player;
    private Camera middleCamera;
    private GameObject subcam1; 
    private GameObject dreamland;
    private GameObject morphmaze;
    private SP_NeuroMods sp;
    private PC_NeuroMods pc;
    private RR_NeuroMods rr;
    private Vector3 initialPosition;



    private int numTraversalsLocal = -1;
    


    // Use this for initialization
    void Start()
    {
        player = GameObject.Find("Player");
        subcam1 = GameObject.Find("subCam1");
        middleCamera = subcam1.GetComponent<Camera>();

        sp = player.GetComponent<SP_NeuroMods>();
        pc = player.GetComponent<PC_NeuroMods>();
	rr = player.GetComponent<RR_NeuroMods>();



        reward0 = GameObject.Find("Reward_A");
        reward1 = GameObject.Find("Reward_B");
        sp.morph = 0.5f;

        dreamland = GameObject.Find("DreamLand");
        morphmaze = GameObject.Find("MorphMaze");

        sp.DreamLand = 0;

	morphmaze.SetActive(true);
	dreamland.SetActive(false);


    }

    // Update is called once per frame
    void Update()
    {
        if (numTraversalsLocal != sp.numTraversals)
        {
            rr.speedBool=0;
	    numTraversalsLocal = sp.numTraversals;
            if ((numTraversalsLocal%10==0) & (sp.numTraversals > 5))
            {
                Debug.Log("Switch");
                sp.DreamLand = Mathf.Abs(sp.DreamLand - 1);
                
            }

            if (sp.DreamLand == 0)
            {
                dreamland.SetActive(false);
                morphmaze.SetActive(true);
                middleCamera.farClipPlane = 350f;
            }
            else
            {
                dreamland.SetActive(true);
                morphmaze.SetActive(false);
                middleCamera.farClipPlane = 550f;
            }

            if (UnityEngine.Random.value >= sp.SkipTrialPcnt)
            {
                if (sp.DreamLand == 0)
                {
                    reward1.SetActive(false);
                    reward0.SetActive(true);
                }
                else
                {
                    reward0.SetActive(false);
                    reward1.SetActive(true);
                }
            }

            else
            {
                reward0.SetActive(false);
                reward1.SetActive(false);

            }
		
	    rr.speedBool=1;

        }
    }
}
