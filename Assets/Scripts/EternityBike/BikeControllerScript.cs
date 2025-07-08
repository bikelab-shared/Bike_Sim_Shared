using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MotionSystems;
using WaypointsFree;
using System;

public class BikeControllerScript : MonoBehaviour
{
    private string handlebar_parent;

    public GameObject BikeBase;
    public GameObject HandleBar;
    public GameObject leftController;
    public GameObject Camera;
    [SerializeField] private GameObject visualTiltTarget;

    private Transform handlebar;
    private Vector3 initialHandlebarPosition;
    private Quaternion initalHandlebarRotation, initialControllerRotation;
    public static float steeringAngle = 0.0f;

    private GameControllerScript gameControllerScript;
    private HandleBarCollider handleBarColliderScript;
    private Bike bikeModel;

    public Quaternion initialBikeRotation;


    // Start is called before the first frame update
    void Start()
    {
        setupScripts();
        initalizeHandlebar();
    }

    private void setupScripts()
    {
        gameControllerScript = BikeBase.GetComponent<GameControllerScript>();
        handleBarColliderScript = HandleBar.GetComponent<HandleBarCollider>();
        bikeModel = handleBarColliderScript.bikemodel;
    }

    private void initalizeHandlebar()
    {
        handlebar = GameObject.FindGameObjectWithTag("HandleBar").transform;
        initialHandlebarPosition = new Vector3(handlebar.rotation.x, handlebar.rotation.y, handlebar.rotation.z);
        initalHandlebarRotation = Quaternion.Euler(new Vector3(0, 0, 0));  //Sonst wieder rein
        initialControllerRotation = leftController.transform.rotation;
        //TODO Check if Rotation is right from start
        //Debug.Log("inithandle: " + initialHandlebarPosition);
        
        handlebar_parent = handlebar.parent.name.ToString();
        //Debug.Log("parent: " + handlebar.parent);
    }

    void Update()
    {
        if (BikeBase != null)
        {
            var rgb = this.GetComponent<Rigidbody>();
            rgb.velocity = (transform.forward * gameControllerScript.BikeSpeed) * (Time.deltaTime * 2f);
            ApplyBreakForce(rgb);

            if (handlebar != null)
            {
                ApplySteering();
                ApplyVisualTiltingCamera();
                MoveBikeAlongTurn();
            }
        }
    }

    private void ApplyBreakForce(Rigidbody rgb)
    {
        float brakeForce = gameControllerScript.appliedBrakeForce;

        if (brakeForce > 0f)
        {
            // Reduce current velocity in forward direction
            Vector3 currentVel = rgb.velocity;
            Vector3 brakeDir = -currentVel.normalized * brakeForce * Time.deltaTime;

            rgb.velocity += brakeDir;

            // Optional clamp: don't reverse
            if (Vector3.Dot(rgb.velocity, transform.forward) < 0)
                rgb.velocity = Vector3.zero;
        }
    }

    private void ApplySteering()
    {
        // 0 to 360 degrees
        steeringAngle = (Quaternion.Inverse(transform.rotation) * initialControllerRotation * leftController.transform.rotation * initalHandlebarRotation).eulerAngles.y;
        /*
        Debug.LogWarning("checking formula: steeringangle before maping: " + steeringAngle);
        Debug.LogWarning("checking formula: leftcontrollerrotation: " + leftController.transform.rotation);
        Debug.LogWarning("checking formula: initialControllerRotation: " + initialControllerRotation);
        Debug.LogWarning("checking formula: initalHandlebarRotation: " + initalHandlebarRotation);
        */
        
        if (steeringAngle > 90 && steeringAngle <= 180)
        {
            steeringAngle = 90;
        }
        else if (steeringAngle > 180 && steeringAngle < 270)
        {
            steeringAngle = -90;
        }
        else if (steeringAngle >= 270 && steeringAngle <= 360)
        {
            steeringAngle -= 360;
        }     

        var steeringVec = new Vector3(0.0f, steeringAngle, 0.0f);
        handlebar.transform.localEulerAngles = steeringVec;
    }

    private void ApplyVisualTiltingCamera()
    {
        float visualTiltAngle = -(gameControllerScript.ITiltAngle) * gameControllerScript.visualTiltMultiplier * 1000;

        // --- TEST MODE: Force 45° left/right tilt based on sign ---
        if (gameControllerScript.currentVisualTiltingMode == GameControllerScript.VisualTiltingMode.TestMode)
        {
            if (gameControllerScript.ITiltAngle > 0)
            {
                visualTiltAngle = -(gameControllerScript.supportedAngle);
            }
                
            else if (gameControllerScript.ITiltAngle < 0)
            {
                visualTiltAngle = gameControllerScript.supportedAngle;
                        
            }
            else
            {
                visualTiltAngle = 0f;
            }          
        }

        // --- Apply visual tilt to assigned target ---
        if (gameControllerScript.currentVisualTiltingMode != GameControllerScript.VisualTiltingMode.Disabled && visualTiltTarget != null)
        {
            Quaternion currentRot = visualTiltTarget.transform.localRotation;
            Quaternion targetRot = Quaternion.Euler(0f, 0f, visualTiltAngle);
            visualTiltTarget.transform.localRotation = Quaternion.Lerp(currentRot, targetRot, Time.deltaTime * gameControllerScript.visualTiltSpeed);
        }

        if (gameControllerScript.activateCalculationLogging)
        {
            Debug.Log("[I] visualTiltAngle: " + visualTiltAngle);
        }
    }

    private void MoveBikeAlongTurn()
    {
        float wheelbase = 1.5f;
        float turnRadius = wheelbase / (Mathf.Sin(Mathf.Abs(steeringAngle) * Mathf.Deg2Rad));

        //Update Radius in other Script
        gameControllerScript.ICurveRadius = turnRadius;

        if (turnRadius > 85)
            turnRadius = Mathf.Infinity;

        Vector3 turningCenterCurve = (transform.position + (transform.right.normalized * turnRadius));
        int sign = 0;

        if (steeringAngle < 0)
        {
            Vector3 curDirection = turningCenterCurve - transform.position;
            turningCenterCurve = transform.position - curDirection;
            sign = -1;
        }
        else if (steeringAngle > 0)
        {
            sign = 1;
        }

        float speedInMS = gameControllerScript.BikeSpeed / 3.6f;

        if (steeringAngle != 0 && turnRadius != Mathf.Infinity) // curve
        {
            transform.RotateAround(turningCenterCurve, Vector3.up, sign * ((speedInMS * 1f) / (2f * Mathf.PI * turnRadius) * 360f) * Time.deltaTime);
        }
        else
        {
            transform.position = transform.position + transform.forward * Time.deltaTime * speedInMS;
        }
    }
    /*void OnCollisionEnter(Collision collision)
    {
        Debug.Log("IN COLLISION ENTER");
        set = false;
    }

    private void OnCollisionExit(Collision collision)
    {
        Debug.Log("IN COLLISION EXIT");
        set = false;
    }*/
}




