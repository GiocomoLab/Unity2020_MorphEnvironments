using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ForagingControl_NeuroMods : MonoBehaviour {

    private GameObject reward1;
    private GameObject reward2;
    private GameObject player;
    private SP_NeuroMods sp;
    private PC_NeuroMods pc;
    private Vector3 initialPosition;



    private int numTraversalsLocal = -1;
    

    // Use this for initialization
    void Start () {
        player = GameObject.Find("Player");
        sp = player.GetComponent<SP_NeuroMods>();
        pc = player.GetComponent<PC_NeuroMods>();
        

        reward1 = GameObject.Find("Reward1");
        reward2 = GameObject.Find("Reward2");

    }
	
	// Update is called once per frame
	void Update () {
        if (numTraversalsLocal != sp.numTraversals)
        {
            numTraversalsLocal = sp.numTraversals;
            if (UnityEngine.Random.value>sp.SkipTrialPcnt)
            {
                reward1.SetActive(true);
                reward1.transform.position= new Vector3(0f,0f, 50f + 350.0f * UnityEngine.Random.value); 
                reward2.SetActive(true);
                reward2.transform.position =  new Vector3(0f,0f,reward1.transform.position.z + 50.0f + 500.0f * UnityEngine.Random.value) ;
                
                
            } else
            {
                reward1.SetActive(false);
                reward2.SetActive(false);
            }
            

        }
    }
}
