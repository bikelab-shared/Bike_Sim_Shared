using System.Collections;
using System.Collections.Generic;
using Uduino;
using UnityEngine;

public class DetectMinMaxLineCollision : MonoBehaviour
{

    //DOES NOT WORK IDK WHY -> use version 2

/*    private int minLineCounter = 0;
    private int maxLineCounter = 0;

    UduinoDevice motorDevice = null;

    public int physicalSwitch;

    int enablePin = 12;
    int direction_CW_CCW = 14;
    int cmdPin = 26;
    int physicalSwitchPin = 18;
    int actual_speed_motorPin = 25;

    public bool digitalSwitch;

    private bool isCollidingMin = false;
    private bool isCollidingMax = false;

   
    const int minCmd = 26, maxCmd = 229;   // 10-90% of 255 (8-bit int)
    public int cmd = minCmd;

    public GameObject sphere_path;
    public GameObject sphere_bike;
    public GameObject handlebar;
    private float distance;

    private List<float> MinAngle = new List<float>();
    private List<float> MaxAngle = new List<float>();

    private float Min_MinAngle = float.MaxValue;
    private float Max_MinAngle = float.MinValue;
    private float Min_MaxAngle = float.MaxValue;
    private float Max_MaxAngle = float.MinValue;

    float angle_min;
    float angle_max;

    public float distanceBetweenObjects;

    public bool onTrack = true;

    void Start()
    {
        UduinoManager.Instance.OnBoardConnected += OnBoardConnected;
        //UduinoManager.Instance.OnDataReceived += ValueReceived;
       
    }

    private void Update()
    {
        //physicalSwitch = UduinoManager.Instance.digitalRead(motorDevice, physicalSwitchPin);
 
        if (digitalSwitch == true)
        {
            if (isCollidingMin)
            {
                Debug.Log("testLineMin collision");
                SetMotorSpeed(1);
                
            }
            else if (isCollidingMax)
            {
                Debug.Log("testLineMax collision");
                SetMotorSpeed(-1);
            }
            else {
                StopMotor();
            }

        }
        else
        {
            StopMotor();
        }

        if (Input.GetKeyDown(KeyCode.C)) 
        {
            // Call the method to print the stored angles when the game is stopped.
            PrintStoredAngles();
        }
        distanceBetweenObjects = Vector3.Distance(handlebar.transform.position, sphere_path.transform.position);


    }

    private void PrintStoredAngles()
    {
        Debug.Log("Minimum Angles:");
        Debug.Log("Minimum Min: " + Min_MinAngle);
        Debug.Log("Minimum Max: " + Max_MinAngle);

        Debug.Log("Maximum Angles:");
        Debug.Log("Maximum Min: " + Min_MaxAngle);
        Debug.Log("Maximum Max: " + Max_MaxAngle);
    }


    private void SetMotorSpeed(int direction) {

        cmd = 75;

        UduinoManager.Instance.digitalWrite(motorDevice, enablePin, State.HIGH);

        //clockwise (?)
        if (direction == 1) {

            //min
            // Calculate the t value based on the current distance and the range [0, maxDistance]
            float t = Mathf.InverseLerp(Min_MinAngle, Max_MinAngle, angle_min);

            // Interpolate the value within the range [min, max] based on the t value
            float interpolatedValue = Mathf.Lerp(75, 90, t);

            Debug.Log("MotorSpeed Min_MinAngle: " + Min_MinAngle + "Max_MinAnlge: " + Max_MinAngle + "with the angle_min: " + angle_min + "calc interpolate " + interpolatedValue);

            UduinoManager.Instance.digitalWrite(motorDevice, direction_CW_CCW, State.HIGH);
            UduinoManager.Instance.analogWrite(motorDevice, cmdPin, (int)interpolatedValue);
            Debug.Log("SetMotorSpeed Min: ");
        }
        //counter clockwise(?)
        else if (direction == -1) {

            //max
            // Calculate the t value based on the current distance and the range [0, maxDistance]
            float t = Mathf.InverseLerp(Min_MaxAngle, Max_MaxAngle, angle_max);

            // Interpolate the value within the range [min, max] based on the t value
            float interpolatedValueM = Mathf.Lerp(75, 90, t);

            Debug.Log("MotorSpeed Max_MaxAngle: " + Max_MaxAngle + "Min_MaxAnlge: " + Min_MaxAngle + "with the angle_max: " + angle_max + "calc interpolate " + interpolatedValueM);

            UduinoManager.Instance.digitalWrite(motorDevice, direction_CW_CCW, State.LOW);
            UduinoManager.Instance.analogWrite(motorDevice, cmdPin, (int)interpolatedValueM);
            Debug.Log("SetMotorSpeed Max: ");
        }
    }

    private void StopMotor() {
        //UduinoManager.Instance.digitalWrite(motorDevice, enablePin, State.LOW);
        UduinoManager.Instance.analogWrite(motorDevice, cmdPin, 0);   //TODO check ob 0 ihn wirklich auch stoppt

    }

    private void OnTriggerEnter(Collider other)
    {
        
        if (other.CompareTag("LineMin"))
        {
            distance = Vector3.Distance(sphere_path.transform.position, sphere_bike.transform.position);
            Debug.Log("Vector LineMin Distance: " + distance);

            //Vector3 targetDir = sphere.transform.position - handlebar.transform.position;
            //float angle = Vector3.Angle(targetDir, handlebar.transform.forward);
            angle_min = Vector3.Angle(handlebar.transform.forward, sphere_path.transform.position - sphere_bike.transform.position);
            Debug.Log("Vector LineMin Angle: " + angle_min);

            if (angle_min < Min_MinAngle)
            {
                Min_MinAngle = angle_min;
            }
            if (angle_min > Max_MinAngle) {
                Max_MinAngle = angle_min;
            }

            MinAngle.Add(angle_min);

            minLineCounter++;
   
            isCollidingMin = true;
            isCollidingMax = false;
        }
        else if (other.CompareTag("LineMax"))
        {

            distance = Vector3.Distance(sphere_path.transform.position, sphere_bike.transform.position);
            Debug.Log("Vector LineMax Distance: " + distance);

            //  Vector3 targetDir1 = sphere.transform.position - handlebar.transform.position;
            // float angle1 = Vector3.Angle(targetDir1, handlebar.transform.forward);
            angle_max = Vector3.Angle(handlebar.transform.forward, sphere_path.transform.position - sphere_bike.transform.position);
            Debug.Log("Vector LineMax Angle: " + angle_max);

            MaxAngle.Add(angle_max);

            if (angle_max < Min_MaxAngle)
            {
                Min_MaxAngle = angle_max;
            }
            if (angle_max > Max_MaxAngle)
            {
                Max_MaxAngle = angle_max;
            }

            maxLineCounter++;

            isCollidingMin = false;
            isCollidingMax = true;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("LineMin"))
        {
            Debug.Log("LineMin Stay");
            minLineCounter++;

            isCollidingMin = true;
            isCollidingMax = false;
        }
        else if (other.CompareTag("LineMax"))
        {
            Debug.Log("LineMax Stay");
            maxLineCounter++;

            isCollidingMin = false;
            isCollidingMax = true;
        }
    } 

    private void OnTriggerExit(Collider other)
    {
        //TODO motor off -> send signal to disable

        Debug.Log("ontriggerExit " + other.tag);

        if (other.CompareTag("LineMin"))
        {
            isCollidingMin = false;
            isCollidingMax = false;
        }
        else if (other.CompareTag("LineMax"))
        {
            isCollidingMin = false;
            isCollidingMax = false;
        }
    }

    void OnBoardConnected(UduinoDevice connectedDevice) //Generic ESP32
    {
        Debug.Log("name: " + connectedDevice.name);

        if (connectedDevice.name == "MotorBoard")
        {
            UduinoManager.Instance.SetBoardType(connectedDevice, "Sonja ESP32");
            motorDevice = connectedDevice;
            //Debug.Log("BOARD STATUS: ");
            int customPin = UduinoManager.Instance.GetPinNumberFromBoardType("Sonja ESP32", "12");
            //Debug.Log("custom Pin: " + customPin);
            UduinoManager.Instance.pinMode(motorDevice, cmdPin, PinMode.PWM); //cmd
            UduinoManager.Instance.pinMode(motorDevice, actual_speed_motorPin, PinMode.Input); //actual_speed_motor
            UduinoManager.Instance.pinMode(motorDevice, enablePin, PinMode.Output); //enable pin
            UduinoManager.Instance.pinMode(motorDevice, direction_CW_CCW, PinMode.Output); //direction pin
            //UduinoManager.Instance.pinMode(motorDevice, physicalSwitchPin, PinMode.Input_pullup);
        }

    } */
}
