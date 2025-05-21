using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Uduino;

public class LaneCenteringAlwaysActive : MonoBehaviour
{

	//public Transform target;  // sphere am weg desired
	public GameObject way_target_sphere;
	public float dirNum;

	UduinoDevice motorDevice = null;


	int enablePin = 12;
	int direction_CW_CCW = 14;
	int cmdPin = 26;
	int physicalSwitchPin = 18;
	int actual_speed_motorPin = 25;

	public bool digitalSwitch;
	public float distanceBetweenObjects;

	public HandleBarCollider handleBarCollider;
	public GameObject sphere_bike;
	public GameObject handlebar;

	private List<float> MinAngle = new List<float>();
	private List<float> MaxAngle = new List<float>();

	private float Min_MinAngle = float.MaxValue;
	private float Max_MinAngle = float.MinValue;
	private float Min_MaxAngle = float.MaxValue;
	private float Max_MaxAngle = float.MinValue;

	float angle_min;
	float angle_max;

	public bool dontStartEarly = true;

	public WayAndHandleBarSphere_Collide WayAndHandleBarSphere_Collide;

	// Start is called before the first frame update
	void Start()
    {
		UduinoManager.Instance.OnBoardConnected += OnBoardConnected;
	}


	void Update()
	{
		Vector3 heading = way_target_sphere.transform.position - sphere_bike.transform.position;
		dirNum = AngleDir(sphere_bike.transform.forward, heading, sphere_bike.transform.up);  //handlebar forward

		if (WayAndHandleBarSphere_Collide.bothSphere_Collide == false && dontStartEarly == false) {

			if (dirNum == -1) //sphere is right so we need to turn the handlebar left
			{
				angle_min = Vector3.Angle(handlebar.transform.forward, way_target_sphere.transform.position - sphere_bike.transform.position);
				Debug.Log("Vector LineMin Angle: " + angle_min);

				if (angle_min < Min_MinAngle)
				{
					Min_MinAngle = angle_min;
				}
				if (angle_min > Max_MinAngle)
				{
					Max_MinAngle = angle_min;
				}

				MinAngle.Add(angle_min);

				SetMotorSpeed(1);


			}
			else if (dirNum == 1) //sphere is left we need to turn right
			{

				angle_max = Vector3.Angle(handlebar.transform.forward, way_target_sphere.transform.position - sphere_bike.transform.position);
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

				SetMotorSpeed(-1);



			}
			else
			{
				StopMotor();
			}

		}

		

		Debug.Log("dirNum: " + dirNum);

		if (WayAndHandleBarSphere_Collide.bothSphere_Collide == true)// && physicalSwitch == 0)
		{
			StopMotor();
		}

		distanceBetweenObjects = Vector3.Distance(handlebar.transform.position, way_target_sphere.transform.position);
	}


	private void SetMotorSpeed(int direction)
	{

		

		UduinoManager.Instance.digitalWrite(motorDevice, enablePin, State.HIGH);

		//In the case for lane centering make it way more less strong


		//clockwise 
		if (direction == 1)
		{

			//min
			// Calculate the t value based on the current distance and the range [0, maxDistance]
			float t = Mathf.InverseLerp(Min_MinAngle, Max_MinAngle, angle_min);

			// Interpolate the value within the range [min, max] based on the t value
			float interpolatedValue = Mathf.Lerp(40, 50, t);  //40 50
	


			Debug.Log("MotorSpeed Min_MinAngle: " + Min_MinAngle + "Max_MinAnlge: " + Max_MinAngle + "with the angle_min: " + angle_min + "t: " + t + "calc interpolate " + interpolatedValue);

			UduinoManager.Instance.digitalWrite(motorDevice, direction_CW_CCW, State.HIGH);
			UduinoManager.Instance.analogWrite(motorDevice, cmdPin, (int)interpolatedValue);

			//Debug.Log("SetMotorSpeed Min: ");

		}
		//counter clockwise
		else if (direction == -1)
		{

			float tt = Mathf.InverseLerp(Min_MaxAngle, Max_MaxAngle, angle_max);

			// Interpolate the value within the range [min, max] based on the t value
			float interpolatedValueM = Mathf.Lerp(40, 50, tt); //40 50
			

			Debug.Log("MotorSpeed Max_MaxAngle: " + Max_MaxAngle + "Min_MaxAnlge: " + Min_MaxAngle + "with the angle_max: " + angle_max + "t: " + tt + "calc interpolate " + interpolatedValueM);

			UduinoManager.Instance.digitalWrite(motorDevice, direction_CW_CCW, State.LOW);
			UduinoManager.Instance.analogWrite(motorDevice, cmdPin, (int)interpolatedValueM);
			//Debug.Log("SetMotorSpeed Max: ");
		}
	}

	float AngleDir(Vector3 fwd, Vector3 targetDir, Vector3 up)
	{
		Vector3 perp = Vector3.Cross(fwd, targetDir);
		float dir = Vector3.Dot(perp, up);

		if (dir > 0f)
		{
			return 1f;  //sphere left
		}
		else if (dir < 0f)
		{
			return -1f;   //sphere right
		}
		else
		{
			return 0f;
		}
	}

	private void StopMotor()
	{
		UduinoManager.Instance.digitalWrite(motorDevice, enablePin, State.LOW);
		UduinoManager.Instance.analogWrite(motorDevice, cmdPin, 0);   //TODO check ob 0 ihn wirklich auch stoppt

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

	}

}
