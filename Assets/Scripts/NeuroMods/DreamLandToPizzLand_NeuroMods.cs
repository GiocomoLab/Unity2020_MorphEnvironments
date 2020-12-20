using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DreamLandToPizzLand_NeuroMods: MonoBehaviour
{

    private GameObject reward0;
    private GameObject reward1;
    private GameObject player;

    private GameObject subcam1; 
    private SP_NeuroMods sp;
    private PC_NeuroMods pc;
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



        reward0 = GameObject.Find("Reward_A");
        reward1 = GameObject.Find("Reward_B");
        sp.morph=1;

        dreamland = GameObject.Find("DreamLand");

        sp.DreamLand = 1;


    }

    // Update is called once per frame
    void Update()
    {
        if (numTraversalsLocal != sp.numTraversals)
        {
            numTraversalsLocal = sp.numTraversals;
            

            if (UnityEngine.Random.value >= sp.SkipTrialPcnt)
            {
                if (sp.numTraversals < 30)
                {
                    reward1.SetActive(false);
                    reward0.SetActive(true);
                }
                else
                {
                    reward0.SetActive(false);
                    reward1.transform.position= new Vector3(0f,0f, 50f + 300.0f * UnityEngine.Random.value); 
                    reward1.SetActive(true);
                }
            }

            else
            {
                reward0.SetActive(false);
                reward1.SetActive(false);

            }


        }
    }
}
