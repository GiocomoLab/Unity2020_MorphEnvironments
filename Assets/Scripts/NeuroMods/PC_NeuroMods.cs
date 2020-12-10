using UnityEngine;
using System;
using System.Collections;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Text;
using System.Net;
using System.Net.Sockets;




public class PC_NeuroMods : MonoBehaviour
{


    private GameObject player;
    
    private GameObject blackCam;
    private GameObject panoCam;
    private GameObject reward0;
    private GameObject reward1;
    private GameObject reward;
    
    


    private Rigidbody rb;

    private SP_NeuroMods sp;
    private DL_NeuroMods dl;
    private RR_NeuroMods rotary;

    private bool reward_dir;


    private Vector3 initialPosition;
    private Vector3 movement;

    private static bool created = false;
    private int r;

    public int cmd = 2;
    private bool flashFlag = false;
    private float LastRewardTime;
    public int prevReward = 0;

    public ArrayList LickHistory;
    public bool bckgndOn = true;

    public float towerJitter=0;
    public float wallJitter=0;
    public float bckgndJitter=0;

    public int mRewardFlag = 0;
    public int rzoneFlag = 0;
    public int toutzoneFlag = 0;
    public int tendFlag = 0;
    public int tstartFlag = 0;

    private static int localPort;
    private static string IP = "10.124.53.26";  // define in init
    private static int port = 7000;  // define in init

    // "connection" things
    IPEndPoint remoteEndPoint;
    UdpClient client;





    public void Awake()
    {


        GameObject player = GameObject.Find("Player");
        sp = player.GetComponent<SP_NeuroMods>();
        rotary = player.GetComponent<RR_NeuroMods>();
        dl = player.GetComponent<DL_NeuroMods>();
        Debug.Log(sp.sceneName);
        if (sp.sceneName == "NeuroMods_Days2to5")
        {
           
            reward = GameObject.Find("Reward");
        }
        else
        {
            
            reward0 = GameObject.Find("Reward0");
            reward1 = GameObject.Find("Reward1");
        }
      



        panoCam = GameObject.Find("panoCamera");
        panoCam.transform.eulerAngles = new Vector3(0.0f, -90.0f, 0.0f);
        initialPosition = new Vector3(0f, 6f, -50.0f);

        LickHistory = new ArrayList();
    }


    private void sendString(string message)
    {
            try
            {
                if (message != "")
                {

                    // get UTF8 encoding of string
                    byte[] data = Encoding.UTF8.GetBytes(message);

                    // send message
                    client.Send(data, data.Length, remoteEndPoint);
                }
            }
            catch (Exception err)
            {
                print(err.ToString());
            }
    }




    void Update()
    {

        // make sure rotation angle is 0
        transform.eulerAngles = new Vector3(0.0f, 90.0f, 0.0f);

        // end game after appropriate number of trials
        if ((sp.numTraversals >= sp.numTrialsTotal) | (sp.numRewards >= sp.maxRewards & transform.position.z < 0f))
        {
            UnityEditor.EditorApplication.isPlaying = false;

        }

        if (dl.r > 0 & dl.rflag < 1) { StartCoroutine(DeliverReward(dl.r)); dl.rflag = 1; }; // deliver appropriate reward

        // manual rewards and punishments
        mRewardFlag = 0;
        if (Input.GetKeyDown(KeyCode.Q) | Input.GetMouseButtonDown(0)) // reward left - sweetened condensed milk
        {
            mRewardFlag = 1;
            StartCoroutine(DeliverReward(4));
            sp.numRewards += 1;

        }



    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.tag);


        if (other.tag == "Reward")
        {
           StartCoroutine(RewardSequence(transform.position.z)); 
        }
        else if (other.tag == "Teleport")
        {
            
            

            sp.numTraversals += 1;
            tendFlag = 1;
            transform.position = initialPosition;
            bckgndOn = true;

            StartCoroutine(InterTrialTimeout());

            LastRewardTime = Time.realtimeSinceStartup; // to avoid issues with teleports
        }
        else if (other.tag == "Start")
        {
            cmd = 0;
            tstartFlag = 1;
            
            //rflag = true;
        }
      

    }


    
    IEnumerator InterTrialTimeout()
    {

        rotary.toutBool = 0f;
        if (prevReward == 0) // omission or probe trial 
        {
            sendString("L0");
            yield return new WaitForSeconds(4f + UnityEngine.Random.value * 5f); 
            sendString("L1");
            yield return new WaitForSeconds(1f);

        }
        else
        {
            yield return new WaitForSeconds(1f + UnityEngine.Random.value*4f);
        }

        rotary.toutBool = 1f;
        prevReward = 0;
        yield return null;
    }

    void OnApplicationQuit()
    {
        panoCam.SetActive(false);
    }

    IEnumerator RewardSequence(float pos)
    {   // water reward
        rzoneFlag = 1;
       
        
        while ((transform.position.z <= pos + 75) & (transform.position.z > 100)) 
            
            
            if ((sp.AutoReward) & (transform.position.z > pos + 50)) 
     
               
                cmd = 4;
                StartCoroutine(DeliverReward(1));
                sp.numRewards += 1;
                yield return new WaitForEndOfFrame();
                break;
                   
               
            }

            if ((dl.c_1 > 0))
                    
                cmd = 4;
                sp.numRewards += 1;
                yield return new WaitForEndOfFrame();
                break;
            }
            yield return new WaitForEndOfFrame();
           
        }
       
        if (sp.sceneName == "NeuroMods_Days2to5")
        {
            
            reward.SetActive(false);
        }
        
        
        
        
        rzoneFlag = 0;
        yield return new WaitForEndOfFrame();
        cmd = 2;

    }

    

    IEnumerator LightsOn()
    {
        panoCam.SetActive(true);
        yield return null;
    }


    IEnumerator DeliverReward(int r)
    { // deliver


        if (r ==4)
        {
            cmd = 4;
            //sp.numRewards += 1;
            yield return new WaitForEndOfFrame(0.01f);
            cmd = 0;
        }
        {
            yield return null;
        };



    }

}
