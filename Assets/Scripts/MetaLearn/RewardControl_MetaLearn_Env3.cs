using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RewardControl_MetaLearn_Env3 : MonoBehaviour {

    private GameObject reward;
    
    private GameObject player;
    private SP_NeuroMods sp;
    private PC_NeuroMods pc;
    private RR_NeuroMods rr;

    

    private int numTraversalsLocal = -1;
    private float morph= -1;

    // Use this for initialization
    void Start () {
        player = GameObject.Find("Player");
        sp = player.GetComponent<SP_NeuroMods>();
        pc = player.GetComponent<PC_NeuroMods>();
        rr = player.GetComponent<RR_NeuroMods>();

        reward = GameObject.Find("Reward");
        
    }
	
	// Update is called once per frame
	void Update () {
        if (numTraversalsLocal != sp.numTraversals )
        {
            rr.speedBool=0;

		numTraversalsLocal = sp.numTraversals;            
            if (UnityEngine.Random.value < sp.SkipTrialPcnt)
            {
                reward.SetActive(false);
                
            }
            else
            {
                reward.SetActive(true);

                
            }
            
            rr.speedBool=1;

        }
    }
}
