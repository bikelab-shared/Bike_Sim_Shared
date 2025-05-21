using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using WaypointsFree;
using UnityEngine.Diagnostics;




    public class SpeedMapper : MonoBehaviour
    {

        public GameObject UserBike;   // user bike reference
        public bool IsSpeedMapped;

        private BoxCollider ProRB;   // refer to the rigidbody of proceeding bike
        private BoxCollider UserRB;   // refer to the rigidbody of the user's bike
        



        public BikeControllerScript bikeControllerScript; // get access to BikeCtrollerScript
        public WaypointsTravelBike waypointsTravelBike;  // get access to WaypointsTravelBike script
        public GameControllerScript gameControllerScript; //get access yo GameControllerScript
        

        public bool ReleaseOccur;
        public void ReleaseDetection()
        {
        UnityEngine.Debug.Log("gameControllerScript.appliedBrakeForce:  " + gameControllerScript.appliedBrakeForce);
        UnityEngine.Debug.Log(" gameControllerScript.BikeSpeed:  " + gameControllerScript.BikeSpeed);

            if ( gameControllerScript.BikeSpeed == 0) //gameControllerScript.appliedBrakeForce != 0 ||
        {
                ReleaseOccur = true;
            }
            else
            {
                ReleaseOccur = false;
            }
            UnityEngine.Debug.Log("The ReleaseOccur is "+ ReleaseOccur);
        }


        // Start is called before the first frame update
        void Start()
        {
            //get the rigidbody component of the two objetcs
            ProRB = GetComponent<BoxCollider>();
            UserRB = UserBike.GetComponent<BoxCollider>();
            
        }

        void OnTriggerEnter(Collider otherOne)
        {
                ReleaseDetection();

        UnityEngine.Debug.Log("OnTrigger:  " + otherOne.name);

        if (otherOne == UserRB && ReleaseOccur == false)   // when the collider is of user's bike and no brake or stop padel the map the speed of proceeding bike 
                {
                    //gameControllerScript.BikeSpeed = waypointsTravelBike.MoveSpeedKmH;  //get the speed of the proceeding bike magnitude
                    IsSpeedMapped = true;

                    UnityEngine.Debug.Log("Collision detected and mapped.");
                    UnityEngine.Debug.Log("Speed Mapped is " + IsSpeedMapped);
                }

        }

    // define the conditions of  the releasement of UBike 
        void OnTriggerExit(Collider otherOne)
        {
            ReleaseDetection();

            if (otherOne == UserRB || ReleaseOccur == true)
            {
                IsSpeedMapped = false;
            }
        }





    // Update is called once per frame
        void Update()
        {

        }


        
    }
