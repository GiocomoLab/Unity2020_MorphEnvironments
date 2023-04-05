using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class SbxTTLs_RunTrain_to_Env1 : MonoBehaviour
{

    private static int localPort;

    // prefs 

    private static string IP = "10.124.53.26";  // define in init
    private static int port = 7000;  // define in init

    // "connection" things
    IPEndPoint remoteEndPoint;
    UdpClient client;


    private PC_RunTrain_to_Env1 pc;
    private int numTraversals_local = -1;
    //private int numTraversals;

    private SP_RunTrain_to_Env1 sp;
    private Notes notes;
    public int scanning = 0;

    public void Awake()
    {
        remoteEndPoint = new IPEndPoint(IPAddress.Parse(IP), port);
        client = new UdpClient();

        
    }

    void Start()
    {

        // for saving data
        GameObject player = GameObject.Find("Player");
        sp = player.GetComponent<SP_RunTrain_to_Env1>();
        pc = player.GetComponent<PC_RunTrain_to_Env1>();
        notes = player.GetComponent<Notes>();
        Debug.Log(sp.numTraversals);

    }


    // sendData
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
        if (Input.GetKeyDown(KeyCode.N) & (scanning==0))
        {
            StartCoroutine(set_filenames());
        }

        if (Input.GetKeyDown(KeyCode.S) & (scanning==0))
        {
            
            StartCoroutine(ScannerStart());
            Debug.Log("start");
        };



        if (Input.GetKeyDown(KeyCode.T) & (scanning==0))
        {
            StartCoroutine(ScannerToggle());
            Debug.Log("toggle");

        };

        
    }

    void OnApplicationQuit()
    {

    }

    IEnumerator ScannerToggle()
    {
        pc.cmd = 8;
        yield return new WaitForSeconds(.01f);
        pc.cmd = 0;
    }

    IEnumerator ScannerStart()
    {

        //start first trial ttl1

        scanning = 1; sp.scanning = 1;
        pc.cmd = 8;
        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(.01f);
        //yield return new WaitForSeconds(.01f);
        pc.cmd = 0;
        yield return new WaitForSeconds(10f);
        pc.cmd = 9;
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        //yield return new WaitForSeconds(.01f);
        pc.cmd = 0;
        Debug.Log("Press G to Start!");


    }

    IEnumerator set_filenames()
    {
        DateTime today = DateTime.Today;
        // set base directory
        sendString("D" + "D:/CanDong/" + notes.mouse + "/" + today.ToString("dd_MM_yyyy") + '/');
        yield return new WaitForSeconds(1.5f);
        // set first field/final directory
        sendString("A" + sp.sceneName);
        yield return new WaitForSeconds(1.5f);
        // set second field
        sendString("U" + sp.session.ToString());
        yield return null;


    }

    void move_laser(float dx, float dy, float dz)
    {
        sendString("Px" + dx.ToString());
        sendString("Py" + dy.ToString());
        sendString("Pz" + dz.ToString());

    }

}
