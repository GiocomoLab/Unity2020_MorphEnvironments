using UnityEngine;
using System;
using System.Collections;
using System.IO;
using System.IO.Ports;
using System.Threading;

using System.Text;
using System.Net;
using System.Net;
using System.Net.Sockets;



public class PC_ShortTrack : MonoBehaviour
{

    private static int localPort;

    // prefs 

    private static string IP = "171.65.17.36";  // define in init
    private static int port = 7000;  // define in init

    // "connection" things
    IPEndPoint remoteEndPoint;
    UdpClient client;


    private GameObject player;
    private GameObject panoCam;
    private GameObject reward0;
    private GameObject reward1;
    private GameObject reward;




    private Rigidbody rb;
    private SP_ShortTrack sp;
    private DL_ShortTrack dl;
    private RR_ShortTrack rotary;

    
    private Vector3 initialPosition;
    private Vector3 movement;
    private static bool created = false;
    private int r;
    public int cmd = 2;

    public int mRewardFlag = 0;
    public int rzoneFlag = 0;
    
    public int tendFlag = 0;
    public int tstartFlag = 0;



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
    

    public void Awake()
    {
        remoteEndPoint = new IPEndPoint(IPAddress.Parse(IP), port);
        client = new UdpClient();

        GameObject player = GameObject.Find("Player");
        sp = player.GetComponent<SP_ShortTrack>();
        rotary = player.GetComponent<RR_ShortTrack>();
        dl = player.GetComponent<DL_ShortTrack>();
        Debug.Log(sp.sceneName);
        

        reward = GameObject.Find("Reward");
       
        panoCam = GameObject.Find("panoCamera");
        panoCam.transform.eulerAngles = new Vector3(0.0f, -90.0f, 0.0f);
        initialPosition = new Vector3(0f, .5f, -50.0f);


       
    }

    void Start()
    {
        StartCoroutine(FlagCheck());

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
            

            StartCoroutine(InterTrialTimeout());

         
        }
        
        
       

    }


    IEnumerator FlagCheck()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();
            tendFlag = 0;
            tstartFlag = 0;
        }
        yield return null;

    }
    IEnumerator InterTrialTimeout()
    {

        rotary.toutBool = 0f;
        if (UnityEngine.Random.value > sp.skipTrialPcnt)
        {
            reward.SetActive(true);
        }
        else
        {
            reward.SetActive(false);
        }

        sendString("L0");
        yield return new WaitForSeconds(5f + UnityEngine.Random.value * 2f);
        sendString("L1");
        yield return new WaitForSeconds(1f);
       

        rotary.toutBool = 1f;
        
        yield return null;
    }

    void OnApplicationQuit()
    {
        panoCam.SetActive(false);
    }

    IEnumerator RewardSequence(float pos)
    {   // water reward
        rzoneFlag = 1;

        
        while ((transform.position.z <= pos + 50))
        {


            if ((sp.AutoReward) & (transform.position.z > pos + 30)) 
            {

                
                cmd = 4;
                
                StartCoroutine(DeliverReward(1));
                sp.numRewards += 1;
                yield return new WaitForEndOfFrame();
                break;

             
            }

            if ((dl.c_1 > 0))
            {
                
                cmd = 4;
                sp.numRewards += 1;
                
                yield return new WaitForEndOfFrame();
                break;
            }
            yield return new WaitForEndOfFrame();
            
        }
        
        




        
        rzoneFlag = 0;
        reward.SetActive(false);
        yield return new WaitForEndOfFrame();
        
        cmd = 2;

    }

    


    IEnumerator DeliverReward(int r)
    { // deliver

        if (r == 1) // reward left
        {
            
            //sp.numRewards += 1;
            yield return null;
        }
        else if (r == 4)
        {
            cmd = 4;
            //sp.numRewards += 1;
            yield return new WaitForSeconds(0.01f);
            cmd = 0;
        }
        else
        {
            yield return null;
        };



    }

}
