using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using System;
using System.IO;
using System.IO.Ports;
using System.Threading;

public class RR_YMazeTrain : MonoBehaviour
{
    public string port = "COM6";
    private int pulses;
    private SerialPort _serialPort;
    private int delay;
    private SP_YMazeTrain sp;
    private PC_YMazeTrain pc;
    public float delta_z;
    public float true_delta_z;
    private float realSpeed = 0.0447f;
    public float t2dist;
    public float speedBool = 0;
    public float startBool = 0;
    public float toutBool = 1;
    public float blockBool = 1;
    private bool firstFlag = true;
    public float startTime = -1f;



    private Vector3[] controlPoints;
    //private Vector3[] RControlPoints;
    private float alpha = .5f;

    public float t;
    public float t_old;
    private float dt = 1;
    private float t0 = 0f;
    public float[] tvec;
    
    

    private static bool created = false;
    public void Awake()
    {
        
    }

    void Start()
    {
        // connect to Arduino uno serial port
        connect(port, 57600, true, 4);
        Debug.Log("Connected to rotary encoder serial port");

        // set speed
        speedBool = 0;

        // connect to playerController script
        GameObject player = GameObject.Find("Player");
        pc = player.GetComponent<PC_YMazeTrain>();
        sp = player.GetComponent<SP_YMazeTrain>();

        // get list of control points
        controlPoints = new Vector3[]
        {
            new Vector3(0f, .5f, -70f),
            new Vector3(0f, .5f, -25f),
            new Vector3(0f, .5f, -5f),
            new Vector3(0f, .5f, 0f),
            new Vector3(0f, .5f, 110f),
            new Vector3(24.14f, .5f, 174.14f),
            new Vector3(110.6f, .5f, 259.6f),
            new Vector3(194.14f, .5f, 344.14f)
        };

        tvec = new float[8];
        tvec[0] = 0.0f;
        tvec[1] = GetT(0, controlPoints[0], controlPoints[1]);
        tvec[2] = GetT(tvec[1], controlPoints[1], controlPoints[2]);
        tvec[3] = GetT(tvec[2], controlPoints[2], controlPoints[3]);
        tvec[4] = GetT(tvec[3], controlPoints[3], controlPoints[4]);
        tvec[5] = GetT(tvec[4], controlPoints[4], controlPoints[5]);
        tvec[6] = GetT(tvec[5], controlPoints[5], controlPoints[6]);
        Debug.Log(tvec[6]);
        tvec[7] = GetT(tvec[6], controlPoints[6], controlPoints[7]);

        t2dist = (tvec[6] - tvec[3]) / 300f ;
        t = tvec[1];
        t_old = tvec[1];
    }
        
        
    

    void Update()
    {

        if (firstFlag) { speedBool = 1; firstFlag = false; }
        if (Input.GetKeyDown(KeyCode.G)) { startBool = 1; startTime = Time.time; };
        // read quadrature encoder
        _serialPort.Write("\n");
        try
        {
            pulses = int.Parse(_serialPort.ReadLine());
            //Debug.Log (pulses);

            //transform.eulerAngles = new Vector3(0.0f, theta_angle * -90.0f + (1.0f - theta_angle) * (-90 + (45.0f * sp.LR)), 0.0f); // -135.0f, 0.0f);
            true_delta_z = -1f * pulses * realSpeed * t2dist;
            delta_z = -1f * startBool * speedBool * toutBool * blockBool * pulses * realSpeed * t2dist;
            t_old = t;
            t = t + delta_z;

            Vector3 target = CatmulRom(t);
            Vector3 view = CatmulRom(t + 1);
            Vector3 dir = target - view;
            transform.eulerAngles = new Vector3(0.0f,  90.0f + Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg, 0f);
            //transform.LookAt(target);
            transform.position= target;
            //transform.position = CatmulRom();
            
            //Vector3 movement = new Vector3(0.0f, 0.0f, delta_z);
            //transform.position = transform.position + movement;

        }
        catch (TimeoutException)
        {
            Debug.Log("rotary timeout");
        }


    }

    float GetT(float t, Vector3 p0, Vector3 p1)
    {
        float a = Mathf.Pow((p1.x - p0.x), 2.0f) + Mathf.Pow((p1.z - p0.z), 2.0f) + Mathf.Pow((p1.y-p0.y),2.0f);
        float b = Mathf.Pow(a, alpha * 0.5f);

        return (b + t);
        

    }

    Vector3 CatmulRom(float t_)
    {
        int ind;
        if ((t_>=tvec[1])&(t_<tvec[2]))
        {
            ind = 0;
        } else if ((t_>=tvec[2]) &(t_<tvec[3]))
        {
            ind = 1;
        } else if ((t_ >= tvec[3]) & (t_ < tvec[4]))
        {
            ind = 2; // t[6] max
        } else if ((t_ >= tvec[4]) & (t_ < tvec[5]))
        {
            ind = 3;
        } else if ((t_ >= tvec[5]) & (t_ < tvec[6]))
        {
            ind = 4;
        } else
        {
            t_ = tvec[2];
            ind = 1;
        }
        Vector3 p0 = controlPoints[ind];
        Vector3 p1 = controlPoints[ind + 1];
        Vector3 p2 = controlPoints[ind + 2];
        Vector3 p3 = controlPoints[ind + 3];

        p0.x = sp.LR * p0.x;
        p1.x = sp.LR * p1.x;
        p2.x = sp.LR * p2.x;
        p3.x = sp.LR * p3.x;

        float t0 = tvec[ind];
        float t1 = tvec[ind + 1];
        float t2 = tvec[ind + 2];
        float t3 = tvec[ind + 3];



        Vector3 A1 = (t1 - t_) / (t1 - t0) * p0 + (t_ - t0) / (t1 - t0) * p1;
        Vector3 A2 = (t2 - t_) / (t2 - t1) * p1 + (t_ - t1) / (t2 - t1) * p2;
        Vector3 A3 = (t3 - t_) / (t3 - t2) * p2 + (t_ - t2) / (t3 - t2) * p3;

        Vector3 B1 = (t2 - t_) / (t2 - t0) * A1 + (t_ - t0) / (t2 - t0) * A2;
        Vector3 B2 = (t3 - t_) / (t3 - t1) * A2 + (t_ - t1) / (t3 - t1) * A3;

        Vector3 C = (t2 - t_) / (t2 - t1) * B1 + (t_ - t1) / (t2 - t1) * B2;

        return C;

    }

    //Vector3 GetVelocity()
    //{

    //}




    private void connect(string serialPortName, Int32 baudRate, bool autoStart, int delay)
    {
        _serialPort = new SerialPort(serialPortName, baudRate);

        _serialPort.DtrEnable = true; // win32 hack to try to get DataReceived event to fire
        _serialPort.RtsEnable = true;
        _serialPort.PortName = serialPortName;
        _serialPort.BaudRate = baudRate;

        _serialPort.DataBits = 8;
        _serialPort.Parity = Parity.None;
        _serialPort.StopBits = StopBits.One;
        _serialPort.ReadTimeout = 1000; // since on windows we *cannot* have a separate read thread
        _serialPort.WriteTimeout = 1000;


        if (autoStart)
        {
            this.delay = delay;
            this.Open();
        }
    }

    private void Open()
    {
        _serialPort.Open();

        if (_serialPort.IsOpen)
        {
            Thread.Sleep(delay);
        }
    }

    private void Close()
    {
        if (_serialPort != null)
            _serialPort.Close();
    }

    private void Disconnect()
    {
        Close();
    }

    void OnDestroy()
    {
        Disconnect();
    }


    //IEnumerator MoveMouse(int LR)
    //{ // -1 is a left trial , 1 is right trial



    //}

}
