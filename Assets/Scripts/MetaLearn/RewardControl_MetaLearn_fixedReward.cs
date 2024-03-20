using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RewardControl_MetaLearn_fixedReward : MonoBehaviour {

    private GameObject reward;
    
    private GameObject player;
    private SP_MetaLearn sp;
    private PC_MetaLearn pc;
    

    

    private int numTraversalsLocal = -1;
    private float morph= -1;

    // Use this for initialization
    void Start () {
        player = GameObject.Find("Player");
        sp = player.GetComponent<SP_MetaLearn>();
        pc = player.GetComponent<PC_MetaLearn>();
        

        reward = GameObject.Find("Reward");
        
    }
	
	// Update is called once per frame
	void Update () {
        if (numTraversalsLocal != sp.numTraversals )
        {
            numTraversalsLocal = sp.numTraversals;            
            if (UnityEngine.Random.value < sp.SkipTrialPcnt)
            {
                reward.SetActive(false);
                
            }
            else
            {
                reward.SetActive(true);

                
            }
            
            

        }
    }
}
