using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MotionSystems;
using WaypointsFree;

public class BikeControllerScript : MonoBehaviour
{
    private Bike bikeModel;
    private string handlebar_parent;

    public GameObject BikeBase;
    public GameObject HandleBar;
    public GameObject ReferenceCube;
    public GameObject Camera;
    public GameObject leftController;

    private Transform handlebar;
    private Vector3 handlebarInit;
    private Quaternion initalHandlebarRotation, initialControllerRotation;
    public static float steeringAngle = 0.0f;

    GameControllerScript gameControllerScript;
    HandleBarCollider handleBarColliderScript;

    public Quaternion initialBikeRotation;


    //public GameObject ProceedingBike;    //get component from SpeedMapper script


    // Start is called before the first frame update
    void Start()
    {

        gameControllerScript = BikeBase.GetComponent<GameControllerScript>();

        //handleBarColliderScript = HandleBar.GetComponent<HandleBarCollider>();
        //bikeModel = handleBarColliderScript.bikemodel;

        //handlebar = this.transform.Find("WheelHandleBar");
        //handlebar = GameObject.FindGameObjectWithTag("HandleBar").transform;
        //handlebarInit = new Vector3(handlebar.rotation.x, handlebar.rotation.y, handlebar.rotation.z);
        //Debug.Log("inithandle: " + handlebarInit);

        handlebar = GameObject.FindGameObjectWithTag("HandleBar").transform;
        handlebarInit = new Vector3(handlebar.rotation.x, handlebar.rotation.y, handlebar.rotation.z);
        handlebar_parent = handlebar.parent.name.ToString();
        Debug.Log("parent: " + handlebar.parent);

        // TEST SONJA 30.09.2024 BUG: wenn man eternity bike dreht / rotiert, stimmt lenkung nichtmehr
        //initalHandlebarRotation = handlebar.transform.rotation;
        //Debug.Log("inithandlebarrotation: " + initalHandlebarRotation); normal 0,0,0,1 
        //                                                                racing 0.8,0,0,0.6
        //initialBikeRotation = BikeBase.transform.localRotation;

        // Get the bike's initial rotation
        // initialBikeRotation = transform.rotation;

        // Set the initial handlebar rotation to counteract the bike's rotation so it looks straight
        // initalHandlebarRotation = Quaternion.Inverse(transform.rotation) * handlebar.transform.rotation;


        initalHandlebarRotation = Quaternion.Euler(new Vector3(0, 0, 0));  //Sonst wieder rein
        initialControllerRotation = leftController.transform.rotation; 

    }

    // Update is called once per frame
    void Update()
    {
        
        if (BikeBase != null)
        {
            GameControllerScript p = BikeBase.GetComponent<GameControllerScript>();

            var rgb = this.GetComponent<Rigidbody>();

            /*if (ProceedingBike.GetComponent<SpeedMapper>().IsSpeedMapped == true)
            {
                rgb.velocity = (transform.forward * ProceedingBike.GetComponent<WaypointsTravelBike>().MoveSpeedKmH) * (Time.deltaTime * 2f);

                Debug.Log("Bikes are chained.");
            }

            else if(ProceedingBike.GetComponent<SpeedMapper>().IsSpeedMapped != true)
            {
                rgb.velocity = (transform.forward * p.BikeSpeed) * (Time.deltaTime * 2f);
            }*/

            rgb.velocity = (transform.forward * p.BikeSpeed) * (Time.deltaTime * 2f);

            if (handlebar != null)
            {

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

                // Todo fix this!
                /*if (Input.GetKeyDown("h"))
                { //If you press c
                    Debug.Log("Reset handlebarrotation");
                    handlebar.transform.rotation = new Quaternion(0.0f, 0.0f, 0.0f, 0.0f);
                    leftController.transform.rotation = new Quaternion(0.0f, 0.0f, 0.0f, 0.0f);
                    set = false;
                }*/

                //TEST 

                //---------------


                //handlebar.transform.rotation = initialControllerRotation * rightController.transform.rotation * initalHandlebarRotation;

                //TEST 235  wieder rein
                steeringAngle = (Quaternion.Inverse(transform.rotation) * initialControllerRotation * leftController.transform.rotation * initalHandlebarRotation).eulerAngles.y; // 0 to 360 degrees
                                                                                                                                                                                  //handlebar.transform.rotation = Quaternion.Euler(0.0f, steeringAngle, 0.0f);
                Debug.LogWarning("checking formula: steeringangle before maping: " + steeringAngle);
                Debug.LogWarning("checking formula: leftcontrollerrotation: " + leftController.transform.rotation);
                Debug.LogWarning("checking formula: initialControllerRotation: " + initialControllerRotation);
                Debug.LogWarning("checking formula: initalHandlebarRotation: " + initalHandlebarRotation);
                // fixedUpdate


                //steeringAngle = steeringAngle.Remap(0, 360, 120.89f,-120.89f);
                //steeringAngle = steeringAngle.Remap(0, 360, 90, -90);

                if (steeringAngle > 90 && steeringAngle <= 180)
                {  //beschränkung rechts
                    steeringAngle = 90;
                }
                else if (steeringAngle > 180 && steeringAngle < 270)
                { //beschränkung links
                    steeringAngle = -90;
                }
                else if (steeringAngle >= 270 && steeringAngle <= 360)
                {  //eigenes Remap links
                    steeringAngle = steeringAngle - 360;
                }

                Debug.Log("parent: " + handlebar_parent);
                if (handlebar_parent.Equals("helper_LOD_Rbike_body"))
                {
                    var steeringVec = new Vector3(-106.271f, steeringAngle, 0.0f);
                    Debug.Log("steeringvec: " + steeringVec);
                    //handlebar.transform.rotation = Quaternion.Euler(steeringVec);
                    handlebar.transform.localEulerAngles = steeringVec;
                }
                else if (handlebar_parent.Equals("EternityBike_Cargo"))
                {
                    var steeringVec = new Vector3(0.0f, steeringAngle, 0.0f);
                    Debug.Log("Cargo steeringvec: " + steeringVec);
                }
                else
                {
                    var steeringVec = new Vector3(0.0f, steeringAngle, 0.0f);
                    //var steeringVec = new Vector3(handlebar.rotation.x, steeringAngle, handlebar.rotation.z);
                    handlebar.transform.localEulerAngles = steeringVec;
                }



                Debug.LogWarning("!!!!! steeringangle new: " + steeringAngle);

                float wheelbase = 1.5f;
                float turnRadius = wheelbase / (Mathf.Sin(Mathf.Abs(steeringAngle) * Mathf.Deg2Rad));

                //-------------
                p.ICurveRadius = turnRadius;
                //----------------

                if (turnRadius > 85)
                {
                    turnRadius = Mathf.Infinity;
                }



                Debug.LogWarning("turning: steeringangle after radius: " + steeringAngle);
                Debug.LogWarning("turning: TurnRadius: " + turnRadius);
                Debug.LogWarning("turning: Transform position: " + transform.position);
                Debug.LogWarning("turning: Transform right: " + transform.right.normalized);

                Vector3 turningCenterCurve = (transform.position + (transform.right.normalized * turnRadius));

                int sign = 0; // curve direction 

                Debug.LogWarning("before turningCenterCurve: " + turningCenterCurve);

                if (steeringAngle < 0) // left
                {
                    Vector3 curDirection = turningCenterCurve - transform.position;
                    turningCenterCurve = transform.position - curDirection;
                    sign = -1;
                }
                else if (steeringAngle > 0) // right
                {
                    sign = 1;
                }

                float speedInMS = p.BikeSpeed / 3.6f;


                Debug.LogWarning("after turningCenterCurve: " + turningCenterCurve);

                Debug.LogWarning("speedInMS: " + speedInMS);

                if (steeringAngle != 0 && turnRadius != Mathf.Infinity) // curve
                {
                    //this.transform.RotateAround(turningCenterCurve, Vector3.up, sign * ((speedInMS * 1f) / (2f * Mathf.PI * turnRadius) * 360f) * Time.deltaTime);
                    this.transform.RotateAround(turningCenterCurve, Vector3.up, sign * ((speedInMS * 1f) / (2f * Mathf.PI * turnRadius) * 360f) * Time.deltaTime);

                }
                else // straight ahead
                {
                    this.transform.position = transform.position + transform.forward * Time.deltaTime * speedInMS;

                }
            }
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

