using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Uduino;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System;
using TMPro;

public enum Condition
{
    VR,
    Screen
}

public enum Bike
{
    Mountain,
    Racing,
    Cargo
}

public class HandleBarCollider : MonoBehaviour
{
    // Start is called before the first frame update
    public String participantID;
    [SerializeField] public Condition currentCondition;
    public Bike bikemodel;

    [SerializeField] public bool lanekeeping;
    [SerializeField] public bool lanecentering;

    public GameObject BikeSimulator;
    public int ballCounter = 0;
    public int ballLapCounter = 0;
    bool startCount = false;
    bool writeDataFile = false;
    public float pastTime = 0.00f;
    public float lapTime = 0.00f;
    public float ballDistance = 0.00f;
    public float ballDistanceMin;
    public float ballDistanceMinVelocity;
    public float ballDistanceMinSteeringAngle;
    public int ballIndex;
    public String timeStamp;
    private float roundTime = 0.00f;
    private bool timerStarted = false;
    private bool passedFinishLine = false;

    private bool camIsSet = false;

    public GameObject finishCube;
    public TextMeshPro startText;
    public TextMeshPro endText;

    [SerializeField] public GameObject vrCamera, screenCamera;


    public GameObject startline;
    public GameObject barrier;

    public GameObject sphere;

    private GameObject CurrentBall = null;
    public GameControllerScript gameControllerScript;
    //public DetectMinMaxLineCollision_version2 detectMinMaxLine;

    public DetectMinMaxLineCollision_version2 detectMinMaxLine;
    public LaneCenteringAlwaysActive laneCenteringAlwaysActive;

    private float targetDistanceThreshold = 4.51f;  //davor war es ca. 8

    public DistractionTask distractionTask;
    public GameObject lanecenteringSphere;

    public bool keypress = false;

    void Start()
    {

        //JUST FOR PAPER TEST DISTANCE
        float distance = Vector3.Distance(this.transform.position, sphere.transform.position);
        Debug.Log("DISTANCE SPHERE AND BIKE: " + distance);

        finishCube.SetActive(false);

        UduinoManager.Instance.OnDataReceived += DetectedBoard;

        if (lanekeeping == true)
        {
            lanecentering = false;
            // lanecenteringSphere.GetComponent<SphereCollider>().enabled = false;

        }
        else if (lanecentering == true)
        {
            lanekeeping = false;
            // lanecenteringSphere.GetComponent<SphereCollider>().enabled = true;
        }


    }

    void DetectedBoard(string data, UduinoDevice device)
    {
        if (data.StartsWith("0.00"))
        {
            startText.SetText("START");
            UduinoManager.Instance.OnDataReceived -= DetectedBoard;
        }

    }


    void OnTriggerEnter(Collider collider)
    {

        var potentialBall = collider.transform;
        var startLine = collider.transform;

        if (collider.tag.Equals("Start"))
        {
            timerStarted = true;
            startline.SetActive(false);
            barrier.SetActive(false);
            finishCube.SetActive(true);

            sphere.GetComponent<WaypointsFree.WaypointsTraveler>().Move(true);


            laneCenteringAlwaysActive.dontStartEarly = false;
            //Added on 16.11 without testing
            //if (lanecentering == true || lanekeeping == true) {
            //  laneCenteringAlwaysActive.dontStartEarly = false;
            //detectMinMaxLine.dontStartEarly = true;
            //}

        }
        if (collider.tag.Equals("Finish"))
        {
            timerStarted = false;
            passedFinishLine = true;
            endText.SetText("FINISHED \n\n Balls collected: " + ballCounter + "\n Time: " + roundTime);
        }

        if (collider.tag.Equals("finished"))
        {
            QuitApplication();
        }


        if (startLine.GetComponent<startLineScript>() != null)
        {
            if (startCount == false)
            {
                timeStamp = GetTimestamp(DateTime.Now);
                startCount = true;
                writeDataFile = true;
                ballCounter = 0;
                Debug.Log("Start BallCounter: " + startCount);
            }

            else if (startCount == true)
            {
                //startCount = false;
                writeDataFile = false;
                ballLapCounter = ballCounter;
                ballCounter = 0;
                lapTime = pastTime;
                pastTime = 0;
                Debug.Log("Stopp Lap BallCounter: " + ballLapCounter + " Startcount " + startCount);
                Debug.Log("Lap Time: " + lapTime);
            }

        }


        if (potentialBall.GetComponent<BallScript>() != null && startCount == true)
        {
            //increment counter;
            CurrentBall = potentialBall.gameObject;
            ballIndex = potentialBall.GetComponent<BallScript>().BallIndex;
            ballDistanceMin = Vector3.Distance(CurrentBall.transform.position, transform.position);
            ballCounter++;
            //var offset = this.transform.position - potentialBall.position;
            //Debug.Log(offset);

            Debug.Log("Distance to other: " + ballDistance);
            Debug.Log("Ballcounter: " + ballCounter);
        }

    }

    void OnTriggerExit(Collider collider)
    {

        if (CurrentBall != null || collider.tag.Equals("Finish"))
        {

            // Min Wert der Liste hinzufügen
            String filepath = "C:/Users/Bikelab/Desktop/Study_RealismVsSim/Logfiles/";
            filepath = filepath + participantID + "_" + timeStamp + ".csv";

            if (writeDataFile == true)
            {

                if (timerStarted || passedFinishLine)
                {

                    addData(currentCondition, timerStarted, roundTime, participantID, ballCounter, ballIndex, ballDistanceMin, ballDistanceMinVelocity, ballDistanceMinSteeringAngle, bikemodel, distractionTask, lanekeeping, lanecentering, keypress, @filepath);
                    passedFinishLine = false;
                }
            }
            CurrentBall.SetActive(false);
            CurrentBall = null;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //        if (detectMinMaxLine.onTrack) { 

        //sphere.GetComponent<WaypointsFree.WaypointsTraveler>().MoveSpeed = gameControllerScript.BikeSpeed / 3.6f;
        //Debug.Log("bikespeed / 3.6: " + gameControllerScript.BikeSpeed / 3.6f);

        float currentSpeed = gameControllerScript.BikeSpeed / 3.6f;

        //USE CODE LINES 351 - 362 FOR LANE KEEPING 

        /*
              if (detectMinMaxLine.distanceBetweenObjects < targetDistanceThreshold)
              {
                  // If the distance is greater than the threshold, increase the speed
                  float speedMultiplier = 1.2f; // Adjust this multiplier as needed
                  sphere.GetComponent<WaypointsFree.WaypointsTraveler>().MoveSpeed = currentSpeed * speedMultiplier;
              }
              else
              {
                  // If the distance is less than the threshold, decrease the speed
                  float speedMultiplier = 0.8f; // Adjust this multiplier as needed
                  sphere.GetComponent<WaypointsFree.WaypointsTraveler>().MoveSpeed = currentSpeed * speedMultiplier;
              }
     */

        // USE CODE LINES 366 - 377 FOR LANECENTERING


        if (laneCenteringAlwaysActive.distanceBetweenObjects < targetDistanceThreshold)
        {
            // If the distance is greater than the threshold, increase the speed
            float speedMultiplier = 1.2f; // Adjust this multiplier as needed
            sphere.GetComponent<WaypointsFree.WaypointsTraveler>().MoveSpeed = currentSpeed * speedMultiplier;
        }
        else
        {
            // If the distance is less than the threshold, decrease the speed
            float speedMultiplier = 0.8f; // Adjust this multiplier as needed
            sphere.GetComponent<WaypointsFree.WaypointsTraveler>().MoveSpeed = currentSpeed * speedMultiplier;
        }


        //}

        if (!camIsSet)
        {
            if (currentCondition == Condition.VR)
            {
                vrCamera.SetActive(true);
                screenCamera.SetActive(false);
            }
            else if (currentCondition == Condition.Screen)
            {
                vrCamera.SetActive(false);
                screenCamera.SetActive(true);
            }
            camIsSet = true;
        }
        if (timerStarted == true)
        {
            roundTime += Time.deltaTime;
        }

        if (startCount == true)
        {
            pastTime += Time.deltaTime;
        }
        if (CurrentBall != null)
        {
            ballDistance = Vector3.Distance(CurrentBall.transform.position, transform.position);
            if (ballDistance < ballDistanceMin)
            {
                ballDistanceMin = ballDistance;

                GameControllerScript p = BikeSimulator.GetComponent<GameControllerScript>();
                // supportLevel = p.supportLevel.ToString();
                ballDistanceMinVelocity = p.BikeSpeed;
                ballDistanceMinSteeringAngle = p.ISteeringAngle;
            }

        }

        if (Input.GetKeyDown("a"))
        {

            String filepath1 = "C:/Users/Bikelab/Desktop/Logfiles/";
            filepath1 = filepath1 + participantID + "_" + timeStamp + ".csv";

            keypress = true;

            Debug.Log("A press");

            addData(currentCondition, timerStarted, roundTime, participantID, ballCounter, ballIndex, ballDistanceMin, ballDistanceMinVelocity, ballDistanceMinSteeringAngle, bikemodel, distractionTask, lanekeeping, lanecentering, keypress, @filepath1);

            keypress = false;
        }

    }
    public void QuitApplication()
    {
        Debug.Log("QUIT");
        UnityEditor.EditorApplication.isPlaying = false;
        //Application.Quit();
    }

    public static void addData(Condition currentCondition, bool isStarted, float roundTime, string participantid, int nr, int ballIndex, float ObjectOffset, float Velocity, float StearingAngle, Bike bikemodel, DistractionTask distraction, bool lanekeeping, bool lanecentering, bool keypress, string Filepath)
    {
        try
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(Filepath, true))
            {

                if (!isStarted && roundTime > 0)
                {
                    //file.WriteLine("BallCounter: " + nr + ", RoundTime: " + roundTime + ", Participantid: " + participantid + ", Condition: " + currentCondition.ToString() + ", Traffic: " + busMode + ", TrafficPrediction: " + busPrediction + ", Bikemodel: " + bikemodel);
                    file.WriteLine("BallCounter: " + nr + ", RoundTime: " + roundTime + ", TotalNumber7Count: " + distraction.number7Count + ", NumberInViewTime: " + distraction.numberInViewTime + ", NumberOutOfViewTime: " + distraction.numberOutOfViewTime + ", Participantid: " + participantid + ", LaneKeeping: " + lanekeeping + ",LaneCentering: " + lanecentering);
                }
                else
                {
                    file.WriteLine("BallIndex: " + ballIndex + ", ObjectOffset: " + ObjectOffset + ", Velocity: " + Velocity + ", StearingAngle: " + StearingAngle);
                }

                if (keypress == true)
                {

                    Debug.Log("A press write");
                    file.WriteLine("BallCounter: " + nr + ", RoundTime: " + roundTime + ", TotalNumber7Count: " + distraction.number7Count + ", NumberInViewTime: " + distraction.numberInViewTime + ", NumberOutOfViewTime: " + distraction.numberOutOfViewTime + ", Participantid: " + participantid + ", LaneKeeping: " + lanekeeping + ",LaneCentering: " + lanecentering);
                }

            }
        }
        catch (Exception ex)
        {
            throw new ApplicationException("failed to write data: ", ex);
        }
    }

    public static String GetTimestamp(DateTime value)
    {
        return value.ToString("yyyyMMddHHmmssffff");
    }

}
