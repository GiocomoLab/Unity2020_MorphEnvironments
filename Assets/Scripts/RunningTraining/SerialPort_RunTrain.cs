using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.IO.Ports;
using System.Threading;

public class SerialPort_RunTrain : MonoBehaviour
{


    public string port = "COM4";
    private SerialPort _serialPort;
    private int delay;
    private string lick_raw;

   
    //public int pinValue;
    
    public int c_1;
    public int pulses;

    // for saving data
    public int r;
    public int rflag = 1;
    private PC_RunTrain pc;
    private SP_RunTrain sp;

    public float delta_z;
    public float true_delta_z;
    private float realSpeed = 0.0447f;
    public float speedBool = 0;
    public float startBool = 0;
    private bool firstFlag = true;

    private static bool created = false;

    public void Awake()
    {
        // for saving data
        GameObject player = GameObject.Find("Player");
        pc = player.GetComponent<PC_RunTrain>();
        sp = player.GetComponent<SP_RunTrain>();
    }

    void Start()
    {
        // connect to Arduino uno serial port
        connect(port, 115200, true, 4);
        Debug.Log("Connected to lick detector serial port");

    }


    void Update()
    {
        rflag = 0;
        if (firstFlag) { speedBool = 1; firstFlag = false; }
        if (Input.GetKeyDown(KeyCode.G)) { startBool = 1; };

        _serialPort.Write(pc.cmd.ToString() + ',');
        try
        {
            lick_raw = _serialPort.ReadLine();
            string[] lick_list = lick_raw.Split('\t');
            c_1 = int.Parse(lick_list[0]);
            r = int.Parse(lick_list[1]);
            pulses = int.Parse(lick_list[2]);
            true_delta_z =  pulses * realSpeed;
            delta_z =  startBool * speedBool * pulses * realSpeed;
            Vector3 movement = new Vector3(0.0f, 0.0f, delta_z);
            transform.position = transform.position + movement;


        }
        catch (TimeoutException)
        {
            Debug.Log("lickport timeout");
        }


    }

    void OnApplicationQuit()
    {
        _serialPort.Write("8,");
    }

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

}
