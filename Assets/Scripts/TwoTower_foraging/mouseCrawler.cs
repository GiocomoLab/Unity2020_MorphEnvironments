using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mouseCrawler : MonoBehaviour {

    
    private SP_TwoTower sp;
    private PC_TwoTower pc;
    public float delta_z=10f;
    
    public void Awake()
    {
        

        // connect to playerController script
        GameObject player = GameObject.Find("Player");
        pc = player.GetComponent<PC_TwoTower>();
        sp = player.GetComponent<SP_TwoTower>();
    }

    void Start()
    {
        


    }

    void Update()
    {
       
            
       Vector3 movement = new Vector3(0.0f, 0.0f, delta_z);
       transform.position = transform.position + movement;


    }

   

   

}
