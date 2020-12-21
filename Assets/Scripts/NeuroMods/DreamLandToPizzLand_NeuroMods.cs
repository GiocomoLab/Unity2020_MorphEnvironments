using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DreamLandToPizzLand_NeuroMods: MonoBehaviour
{

    private GameObject reward0;
    private GameObject reward1;
    private GameObject reward2;
    private GameObject player;

    private GameObject subcam1; 
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
        

        sp = player.GetComponent<SP_NeuroMods>();
        pc = player.GetComponent<PC_NeuroMods>();
        rr = player.GetComponent<RR_NeuroMods>();



        reward0 = GameObject.Find("Reward_A");
        reward1 = GameObject.Find("Reward_B");
        reward2 = GameObject.Find("Reward_C");
        sp.morph=1;

        

        sp.DreamLand = 1;


    }

    // Update is called once per frame
    void Update()
    {
        if (numTraversalsLocal != sp.numTraversals)
        {
            rr.speedBool = 0;
            numTraversalsLocal = sp.numTraversals;
            

            if (UnityEngine.Random.value >= sp.SkipTrialPcnt)
            {
                if (sp.numTraversals < 30)
                {
                    reward1.SetActive(false);
                    reward2.SetActive(false);
                    reward0.SetActive(true);
                }
                else
                {
                    reward0.SetActive(false);
                    float val = UnityEngine.Random.value;
                    if (val<=.25)
                    {
                        reward1.SetActive(true);
                        reward1.transform.position = new Vector3(0f, 0f, 50f + 125.0f * UnityEngine.Random.value);

                        reward2.SetActive(true);
                        reward2.transform.position = new Vector3(0f, 0f, 200f + 150.0f * UnityEngine.Random.value);
                    }
                    else
                    {
                        reward1.SetActive(true);
                        reward1.transform.position = new Vector3(0f, 0f, 50f + 300.0f * UnityEngine.Random.value);
                        reward2.SetActive(false);
                    }
                        


                }
            }

            else
            {
                reward0.SetActive(false);
                reward1.SetActive(false);
                reward2.SetActive(false);

            }

            rr.speedBool = 1;

        }
    }
}
