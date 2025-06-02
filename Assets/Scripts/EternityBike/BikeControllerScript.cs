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
    public GameObject ReferenceCube;
    public GameObject Camera;
    public GameObject leftController;

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
        //handleBarColliderScript = HandleBar.GetComponent<HandleBarCollider>();
        //bikeModel = handleBarColliderScript.bikemodel;
    }

    private void initalizeHandlebar()
    {
        handlebar = GameObject.FindGameObjectWithTag("HandleBar").transform;
        initialHandlebarPosition = new Vector3(handlebar.rotation.x, handlebar.rotation.y, handlebar.rotation.z);
        initalHandlebarRotation = Quaternion.Euler(new Vector3(0, 0, 0));  //Sonst wieder rein
        initialControllerRotation = leftController.transform.rotation;
        Debug.Log("inithandle: " + initialHandlebarPosition);
        //TODO Check if Rotation is right from start

        handlebar_parent = handlebar.parent.name.ToString();
        Debug.Log("parent: " + handlebar.parent);
    }

    void Update()
    {
        if (BikeBase != null)
        {
            var rgb = this.GetComponent<Rigidbody>();
            rgb.velocity = (transform.forward * gameControllerScript.BikeSpeed) * (Time.deltaTime * 2f);

            if (handlebar != null)
            {
                #region legacy-tilting
                /*
                //  this.transform.localEulerAngles = rotationVec;

                //var tiltVec = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, -p.ITiltAngle*2);
                var tiltVec = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, -p.ITiltAngle);
                if (p.RollMultiplier != 0 || 1 == 1)
                {
                    // maybe we add the slerp here to reduce the statter

                    //tiltVec is our TARGET, IF WE ARE NOT ALREADY AT THE TARGET; LETS MOVE THERE (INTERPOLATED IN 0.2 SECONDS)

                      if (-p.ITiltAngle != Mathf.RoundToInt(this.transform.localEulerAngles.z))
                       {

                           lastTiltVec += Time.deltaTime;
                           Vector3 euler = this.transform.localEulerAngles;
                           // euler.z = Mathf.Lerp(transform.localEulerAngles.z-p.ITiltAngle, -p.ITiltAngle, Time.deltaTime);
                           if(-p.ITiltAngle>=this.transform.localEulerAngles.z)
                               euler.z = Mathf.Lerp(euler.z, -p.ITiltAngle, lastTiltVec);

                           this.transform.localEulerAngles = euler;
                       }
                       else
                       {
                           lastTiltVec = 0;
                       }

                    this.transform.localEulerAngles = tiltVec;

                //float angle = Mathf.LerpAngle(0, -p.ITiltAngle, Time.time);
                //this.transform.localEulerAngles = new Vector3(0, angle, 0);

                //this.transform.localEulerAngles=Vector3.Slerp(this.transform.localEulerAngles, tiltVec, Time.deltaTime);

                //this.transform.localEulerAngles = Vector3.Lerp(this.transform.localEulerAngles,tiltVec, 0.1F);

                // var _targetRotation = Quaternion.Euler(tiltVec);
                // this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation, _targetRotation, 1 * Time.deltaTime);

                // Debug.Log(tiltVec);

                }
                */
                #endregion

                ApplySteering();
                ApplyVisualTilting();
                MoveBikeAlongTurn();
            }
        }
    }

    private void ApplySteering()
    {
        // 0 to 360 degrees
        steeringAngle = (Quaternion.Inverse(transform.rotation) * initialControllerRotation * leftController.transform.rotation * initalHandlebarRotation).eulerAngles.y;
        Debug.LogWarning("checking formula: steeringangle before maping: " + steeringAngle);
        Debug.LogWarning("checking formula: leftcontrollerrotation: " + leftController.transform.rotation);
        Debug.LogWarning("checking formula: initialControllerRotation: " + initialControllerRotation);
        Debug.LogWarning("checking formula: initalHandlebarRotation: " + initalHandlebarRotation);
        
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

    private void ApplyVisualTilting()
    {
        float tiltFactor = gameControllerScript.RollMultiplier;
        float tiltAngle = -gameControllerScript.ITiltAngle;

        if (tiltFactor != 0)
        {
            Vector3 targetTilt = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, tiltAngle);
            transform.localEulerAngles = Vector3.Lerp(transform.localEulerAngles, targetTilt, Time.deltaTime * 5f);
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




