using System.Collections;
using System.Collections.Generic;
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
    public float speedBool = 0;
    public float startBool = 0;
    private bool firstFlag = true;
    private float theta_angle = 1f;
    private float theta_pos = 1f;
    private float h_max = 154.1421f;
    private float radius = 20f;
    private float h_thresh;
    

    private static bool created = false;
    public void Awake()
    {
        h_thresh = h_max - (float) Math.Sqrt(radius * radius / 2.0);
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
    }

    void Update()
    {

        if (firstFlag) { speedBool = 1; firstFlag = false; }
        if (Input.GetKeyDown(KeyCode.G)) { startBool = 1; };
        // read quadrature encoder
        _serialPort.Write("\n");
        try
        {
            pulses = int.Parse(_serialPort.ReadLine());
            //Debug.Log (pulses);
            if (transform.position.z <= h_thresh) //140f)
            {
                theta_angle = 1f;
                theta_pos = 1f;
            }
            else if ((transform.position.z>h_thresh) & (transform.position.z<h_max)) //((transform.position.z >140f) & (transform.position.z<154.1421) )
            {
                theta_angle = 1 - (transform.position.z - h_thresh) / (h_max - h_thresh); // 140f) / (14.1421f);
                theta_pos =  1- (transform.position.z - h_thresh) / (h_max - h_thresh)/2f; //(transform.position.z - 140f) / (14.1421f)/2f;
                Debug.Log("theta angle"); // theta_angle;
            }
            else
            {
                theta_angle = 0f;
                theta_pos = .5f;
            }

            transform.eulerAngles = new Vector3(0.0f, theta_angle * -90.0f + (1.0f - theta_angle) * (-90 + (45.0f * sp.LR)), 0.0f); // -135.0f, 0.0f);
            true_delta_z = -1f * pulses * realSpeed;
            delta_z = -1f * startBool * speedBool * pulses * realSpeed;
            Vector3 movement = new Vector3(sp.LR*(1.0f-theta_pos)*delta_z, 0.0f, theta_pos*delta_z);
            //Vector3 movement = new Vector3(0.0f, 0.0f, theta_pos * delta_z);
            transform.position = transform.position + movement;

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


    //IEnumerator MoveMouse(int LR)
    //{ // -1 is a left trial , 1 is right trial



    //}

}
