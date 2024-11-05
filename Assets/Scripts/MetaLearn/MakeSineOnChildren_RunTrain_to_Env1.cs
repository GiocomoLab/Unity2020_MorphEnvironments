using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MakeSineOnChildren_RunTrain_to_Env1 : MonoBehaviour
{

    private int dim1 = 120; 
    private int dim2 = 1500;
    private float f1 = 2.5f; 
    private float f2 = 3.5f; 
    private float theta1 = 60f;
    private float theta2 = 10f;

    private SP_RunTrain_to_Env1 sp;
    private PC_RunTrain_to_Env1 pc;
    private float morph;

    private Color color;
    private Renderer eastRenderer;
    private Renderer westRenderer;
    

    private GameObject player;
    private GameObject blackCam;
    private GameObject eWall;
    private GameObject wWall;

    private RR_RunTrain_to_Env1 rr;


    private int numTraversalsLocal = -1;


    // Use this for initialization
    void Start()
    {
        player = GameObject.Find("Player");
        sp = player.GetComponent<SP_RunTrain_to_Env1>();
        rr = player.GetComponent<RR_RunTrain_to_Env1>();


        eWall = GameObject.Find("East Wall");
        eastRenderer = eWall.GetComponent<Renderer>();
        wWall = GameObject.Find("West Wall");
        westRenderer = wWall.GetComponent<Renderer>();

        

        morph = sp.morph;

    }

    // Update is called once per frame
    void Update()
    {
        if (numTraversalsLocal != sp.numTraversals | morph != sp.morph)
        {
            numTraversalsLocal = sp.numTraversals;

            morph = sp.morph;

            rr.speedBool = 0;
            //jitter = .2f * (UnityEngine.Random.value - .5f);
            
            StartCoroutine(drawSineWall());
            
           
        }
    }


    IEnumerator drawSineWall()
    {

        Texture2D texture = new Texture2D(dim1, dim2);
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (var r in renderers)
        {
            // Do something with the renderer here...
            r.material.mainTexture = texture; // like disable it for example. 
        }

        float xs = 0f;
        float ys = 0f;
        float tmp_morph = morph;
        float theta = tmp_morph * theta1 + (1.0f - tmp_morph) * theta2 ;
        float f = tmp_morph * f1 + (1.0f - tmp_morph) * f2 ;
        float thetar = theta * Mathf.PI / 180.0f;
        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                
                xs = (float)x / (float)dim2;
                ys = (float)y / (float)dim1;

                float intensity = Mathf.Cos(2.0f * Mathf.PI * f * (xs * (Mathf.Cos(thetar + Mathf.PI / 4.0f) + Mathf.Sin(thetar + Mathf.PI / 4.0f)) + ys * (Mathf.Cos(thetar + Mathf.PI / 4.0f) - Mathf.Sin(thetar + Mathf.PI / 4.0f))));


                color.r = intensity;
                color.g = intensity;
                color.b = intensity;


                color.a = 0.0f; // set alpha to 0 (transparency... reduces glare)
                texture.SetPixel(x, y, color);
                
            }
           
        }
       
        texture.filterMode = FilterMode.Point;
        texture.Apply();
        rr.speedBool = 1;
        yield return null;
    }
}