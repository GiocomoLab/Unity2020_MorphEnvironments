﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BkgndColor_NeuroMods : MonoBehaviour
{

    private SP_NeuroMods sp;
    private PC_NeuroMods pc;
    //private float morph;
    private GameObject player;

    private Color bkgnd;
    private Camera[] cameras;
    private float morph = -1;
    
    private int numTraversalsLocal = -1;
    private float tmp_morph;
    

    // Use this for initialization
    void Start()
    {
        player = GameObject.Find("Player");
        sp = player.GetComponent<SP_NeuroMods>();
        pc = player.GetComponent<PC_NeuroMods>();



        cameras = GetComponentsInChildren<Camera>();
        foreach (var cam in cameras)
        {
            
            cam.clearFlags = CameraClearFlags.SolidColor;
            
        }
       

    }

    // Update is called once per frame
    void Update()
    {
        if ((morph != sp.morph) | (numTraversalsLocal != sp.numTraversals) )
        {
            morph = sp.morph;
            numTraversalsLocal = sp.numTraversals;
            // jitter = .2f * (UnityEngine.Random.value - .5f);
            
            tmp_morph = morph;
            float theta = tmp_morph * .8f + (1 - tmp_morph) * .2f;
            bkgnd.r = theta; bkgnd.b = theta; bkgnd.g = theta; bkgnd.a = 0.0f;

        }

        if (pc.bckgndOn )
        {

            foreach (var cam in cameras)
            {
                cam.backgroundColor = bkgnd; // Color.Lerp(Color.black, Color.white, tmp_morph);

            }
        }
        else
        {
            foreach (var cam in cameras)
            {
                cam.backgroundColor = Color.Lerp(Color.black, Color.white, .5f);

            }
        }
        
    }


 
}
