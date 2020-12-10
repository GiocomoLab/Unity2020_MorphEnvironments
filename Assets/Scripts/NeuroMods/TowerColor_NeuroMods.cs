using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerColor_NeuroMods : MonoBehaviour
{

    private Color color;
  
    private SP_NeuroMods sp;
    
    private Renderer[] renderers;
    private Texture2D texture;
    
    private int numTraversalsLocal = -1;
    private float morph = -1;
    private bool bw = false;
    private float jit = 0f;


    // Use this for initialization
    void Start()
    {
        GameObject player = GameObject.Find("Player");
        sp = player.GetComponent<SP_NeuroMods>();
        
        

        texture = new Texture2D(1, 1);
        renderers = GetComponentsInChildren<Renderer>();

        
        foreach (var r in renderers)
        {
            // Do something with the renderer here...
            r.material.mainTexture = texture; // like disable it for example. 
        }

       

        
        color = Color.Lerp(Color.green, Color.blue, .5f);
            
        
        texture.SetPixel(1, 1, color);
        texture.filterMode = FilterMode.Point;
        texture.Apply();



    }

    // Update is called once per frame
    void Update()
    {

        if (numTraversalsLocal != sp.numTraversals | morph != sp.morph )
        {
            numTraversalsLocal = sp.numTraversals;

            morph = sp.morph;
          
            
            float tmp_morph =  sp.morph;
            color = Color.Lerp(Color.green, Color.blue, tmp_morph);
            StartCoroutine(DrawTowers());

        }
    }



    IEnumerator DrawTowers()
    {
        texture = new Texture2D(1, 1);
        renderers = GetComponentsInChildren<Renderer>();

        foreach (var r in renderers)
        {
            // Do something with the renderer here...
            r.material.mainTexture = texture; // like disable it for example. 
        }




        texture.SetPixel(1, 1, color);
        texture.filterMode = FilterMode.Point;
        texture.Apply();
        yield return null;
    }
       
        
     

    
}
