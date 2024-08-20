using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.IO.Ports;
using System.Threading;


public class RR_2DTrack : MonoBehaviour
{

    public string port = "COM3";
    private int pulses;
    private SerialPort _serialPort;
    private int delay;
    private SP_2DTrack sp;
    private PC_2DTrack pc;
    public float delta_z;
    public float true_delta_z;
    private float realSpeed = 0.0447f;
    public float speedBool = 1;     //Changed this to 1
    private float startBool = 0;            
    //public float servoBool = 0;
    private bool firstFlag = true;
    public float toutBool = 1;

    private static bool created = false;
    public void Awake()
    {
        // set speed
        speedBool = 0;

        // connect to playerController script
        GameObject player = GameObject.Find("Player");
        pc = player.GetComponent<PC_2DTrack>();
        sp = player.GetComponent<SP_2DTrack>();
    }

    void Start()
    {
        // connect to Arduino uno serial port
        connect(port, 57600, true, 4);
        //connect(port, 115200, true, 4);
        Debug.Log("Connected to rotary encoder serial port");


    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G)) { startBool = 1; };

        // read quadrature encoder
        _serialPort.Write("\n"); // write a blank line to indicate new frame
        try
        {
            pulses = int.Parse(_serialPort.ReadLine()); // read number of clicks from rotary encoder
            //Debug.Log (pulses);
            true_delta_z = -1f * pulses * realSpeed;
            delta_z = -1f * speedBool * startBool * toutBool * pulses * realSpeed;
            transform.position += transform.forward * delta_z;
            Debug.Log("speedBool: " + speedBool + "startBool: " + startBool + "toutBool: " + toutBool + "pulses: " + pulses);

            
            Debug.Log("Current position in world space: " + transform.position);

        }
        catch (TimeoutException)
        {
            Debug.Log("rotary timeout");
        }


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
