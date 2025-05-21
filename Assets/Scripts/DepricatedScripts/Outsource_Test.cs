using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Uduino;
using System;


//Disclaimer: this is the "old" code which worked for the arduino as second microcontroller
//but since it was to slow its depricated
public class Outsource_Test : MonoBehaviour
{
    // Start is called before the first frame update

    public bool digitalSwitch;
    public float actual_speed_motor;

    UduinoDevice motorDevice = null;

 

    public float bikespeed;
    public float ISteeringAngle;

     //-----Motor variables------------
    // from dynamics model
    const double c1_21 = -0.85035641456978, c1_22 = 1.68540397397560;
    const double k0_21 = -2.59951685249872, k0_22 = -0.80329488458618;
    const double k2_22 = 2.65431523794604;   // k2_21 = 0
    const double g = 9.81;

    const double kT = 78.6 / 1000.0;  // Nm/A motor torque constant
    const double nGears = 18.0, nBelt = 72.0 / 24.0;
    const double etaMaxMotor = 0.83, etaMaxGears = 0.75;
    // A motor current per Nm steering torque on handle bar [A/Nm]:
    const double constT2I = 1.0 / nBelt / nGears / etaMaxGears / etaMaxMotor / kT;
    const double constRPM2dphi = 2 * Math.PI / 60.0 / nGears / nBelt; // motor rotations per minute to steering angular velocity in rad/s

    const float maxCurrent = 4.0f;  // [A] from controller settings
    const int minCmd = 26, maxCmd = 229;   // 10-90% of 255 (8-bit int)
    public int cmd = minCmd;
    public int sign;   // check which sign corresponds to CW/CCW, +/- A

    double delta = 0.0, ddelta = 0.0;
    double v = 0.0, phi = 0.0, dphi = 0.0;
    double steerTorqueDes = 0.0, motorCurrentDes;


    //----------from tilt model----------------------- 
    const double m_11Inv = 1.0 / 80.81722;
    const double c1_12 = 33.86641391492494;   // c1_11 = 0
    const double k0_11 = -80.95, k0_12 = -2.59951685249872;
    const double k2_12 = 76.59734589573222;   // k2_11 = 0

    double tilt_phi = 0.0, tilt_dphi = 0.0, tilt_ddphi = 0.0;
    double tilt_v = 0.0, tilt_delta = 0.0, tilt_ddelta = 0.0;
    double dt;

    DateTime prevTime;

    public GameControllerScript gameController;


    int physicalSwitch = 0;

    int enablePin = 6;
    int direction_CW_CCW = 7;
    int cmdPin = 3;
    int physicalSwitchPin = 8;

    //---------------------------
    /*
     -----------------------------------------------------------
    */


    void Start()
    {
        UduinoManager.Instance.OnBoardConnected += OnBoardConnected;
    }

    void Update()
    {

        CalcSteeringTorqueAndTilt(bikespeed, gameController.ISteeringAngle, actual_speed_motor);

        if (motorDevice != null)
        {
            physicalSwitch = UduinoManager.Instance.digitalRead(motorDevice, physicalSwitchPin);

           


                Debug.Log("write: " + System.DateTime.Now.Millisecond);
                Debug.Log("motorDevice: " + cmd);


                UduinoManager.Instance.analogWrite(motorDevice, cmdPin, cmd);

                actual_speed_motor = UduinoManager.Instance.analogRead(motorDevice, AnalogPin.A0);  //range 0-1024  --> todo test and/or remap
                actual_speed_motor = actual_speed_motor / 60;
                actual_speed_motor = actual_speed_motor / 18;

                Debug.Log("actual_speed_motor: " + actual_speed_motor);
                Debug.Log("switchDi:" + digitalSwitch);


                if (digitalSwitch == true && physicalSwitch != 0)
                {
                    Debug.Log("physicalSwitch ON: " + physicalSwitch);
                    Debug.Log("digitalSwitch ON: " + digitalSwitch);
                    UduinoManager.Instance.digitalWrite(motorDevice, enablePin, State.HIGH);
                }
                else
                {
                    UduinoManager.Instance.digitalWrite(motorDevice, enablePin, State.LOW);
                }


                if (sign == 1)
                {
                    UduinoManager.Instance.digitalWrite(motorDevice, direction_CW_CCW, State.HIGH);
                }
                else
                {
                    UduinoManager.Instance.digitalWrite(motorDevice, direction_CW_CCW, State.LOW);
                }
            
        }

    }

    void OnBoardConnected(UduinoDevice connectedDevice)
    {
        Debug.Log("name: " + connectedDevice.name);

        if (connectedDevice.name == "uduinoBoard")
        {
           
            motorDevice = connectedDevice;
            UduinoManager.Instance.pinMode(motorDevice, cmdPin, PinMode.PWM); //cmd
            UduinoManager.Instance.pinMode(motorDevice, AnalogPin.A0, PinMode.Input); //actual_speed_motor
            UduinoManager.Instance.pinMode(motorDevice, enablePin, PinMode.Output); //enable pin
            UduinoManager.Instance.pinMode(motorDevice, direction_CW_CCW, PinMode.Output); //direction pin
            UduinoManager.Instance.pinMode(motorDevice, physicalSwitchPin, PinMode.Input); 
        }

    }

  
    private void CalcSteeringTorqueAndTilt(float velocity, float steeringAngle, float steeringAngleVelocity) {

        // update state values from sensors
        // We have an offset from the potentiometer of 8 degrees!!
        Debug.Log("angle: " + steeringAngle);
        delta = (steeringAngle - 8.0) / 180.0 * Math.PI;        // in rad
        //DateTime calc = DateTime.Now;
        //TimeSpan ts = dtRead - calc;
       // Debug.Log("steeringAng calc: " + delta + "dt: " + ts.TotalMilliseconds);

        ddelta = steeringAngleVelocity; // *voltToRadperSecond (abhänging von den einstellungen bei controller also die rpm) = max_rpm/max_volt*60*pi/180
        v = velocity;


        //wenn v null ist stollte der lenker in eine richtung einschlagen wie beim echten fahrrad wenn ich nicht fahre und der lenker sich einlenkt; je höher v desto mehr sollte sich der lenker in der mitte einpendeln
        // v > 2 sollte schon leichte stabilisierung bringen
        //steerTorqueDes = v * (c1_21 * tilt_dphi + c1_22 * ddelta) + g * k0_21 * tilt_phi + (g * k0_22 + v * v * k2_22) * delta;
        Debug.Log("steerTorq: " + steerTorqueDes);
        steerTorqueDes = -delta*2;
        Debug.Log("steer: " + steerTorqueDes);
        motorCurrentDes = constT2I * steerTorqueDes;

        // extract sign
        if (motorCurrentDes < 0)
        {
            sign = 1;
            motorCurrentDes = -motorCurrentDes;
        }
        else
        {
            sign = 0;
        }


        // translate current to PWM command signa;
        cmd = Math.Min(Math.Max((int)Math.Round(motorCurrentDes * 255 / maxCurrent), minCmd), maxCmd);

        Debug.Log("CMD: " + cmd);
       // Debug.Log("motorCurrentDes: " + motorCurrentDes);


        //-----------tilt

        // local dynamics equation
        tilt_ddphi = -m_11Inv * (v * c1_12 * ddelta + g * k0_11 * tilt_phi + (g * k0_12 + v * v * k2_12) * delta);
       // Debug.Log("tilt_ddphi: " + tilt_ddphi);

        // get passed time
        var currTime = DateTime.Now;
        dt = (currTime.Millisecond - prevTime.Millisecond);
        prevTime = currTime;


        tilt_dphi += tilt_ddphi * dt * 0.001; // dt*1000 to make it seconds
        Debug.Log("tilt_dphi: " + tilt_dphi);
       // tilt_phi += tilt_dphi * dt * 0.001;

        //-----------end tilt


    }

}
