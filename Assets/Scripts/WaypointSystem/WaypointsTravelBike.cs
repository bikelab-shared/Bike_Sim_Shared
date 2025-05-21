using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//this script controll the movement of proceeding bike, involed pathway, start and end point, moving speed and deceleration etc.

namespace WaypointsFree
{

    /// <summary>
    /// Moves the gameobject through a series of waypoints
    /// 
    /// </summary>
    public class WaypointsTravelBike : MonoBehaviour
    {
        [Tooltip("WaypointsGroup gameobject containing the waypoints to travel.")]
        public WaypointsGroup Waypoints = null;

        [Tooltip("Movement and look-at constraints.")]
        public PositionConstraint XYZConstraint = PositionConstraint.XYZ;


        [Tooltip("Auto-start movement if true.")]
        public bool AutoStart = true;    //proceeding bike starts to move when the game run

        //[Range(0,float.MaxValue)]
        //public float MoveSpeedKmH = 15f; // initialize the max speed of proceeding bike is 15km/h
        //public float MoveSpeedMS; //the speed of proceeding bike convert to m/s


        public float MoveSpeedMS = 3.0f;  //initialize the cycling speed of proceedingbike as 3 m/s
        public float MoveSpeedKmH;

       


        //[Range(0, float.MaxValue)]
        public float LookAtSpeed = 4.5f;

        [Tooltip("Starts movement from the position vector at this index. Dependent upon StartTravelDirection!")]
        public int StartIndex = 0;


        [Tooltip("Immediately move starting position to postion at StartIndex.")]
        public bool AutoPositionAtStart = true;

        [Tooltip("Initial direction of travel through the positions list.")]
        public TravelDirection StartTravelDirection = TravelDirection.FORWARD;

        [Tooltip("Movement behavior to apply when last postion reached.")]
        public EndpointBehavior EndReachedBehavior = EndpointBehavior.STOP;

        [Tooltip("Movement function type")]
        public MoveType StartingMovementType = MoveType.LERP;


        public bool IsMoving
        {
            get { return isMoving; }
        }

        delegate bool MovementFunction();
        MovementFunction moveFunc = null;



        int positionIndex = -1; // Index of the next waypoint to move toward
        List<Waypoint> waypointsList; //Reference to the list of waypoints located in Waypoints 


        Vector3 nextPosition; // The next position to travel to.
        Vector3 startPosition;
        Vector3 destinationPosition;

        float distanceToNextWaypoint;
        float distanceTraveled = 0;
        float timeTraveled = 0;

        int travelIndexCounter = 1;

        bool isMoving = false; // Movement on/off



        Vector3 positionOriginal;
        Quaternion rotationOriginal;
        float moveSpeedOriginal = 0;
        float lookAtSpeedOriginal = 0;

 
        
        public GameObject BusTriggerPBike;  //access to BusTrigger script for collision detection between proceeding bike and buses
        public bool isBusesTriggered; // isBussesTriggered lead to a deceleration of proceddingbike's speed
        public float BusTriggeredDeceleration = 2.0f;  //initilize the deceleration triggered by the collison with buses
        public float minMoveSpeedMS = 1.0f;  // initialize the min value of proceedingbike's speed is 1 m/s


        public GameObject CornerTrigger;
        public bool isCornerDeceleration;
        public float CornerDecelerationRate = 1.0f;
        public float minCornerSpeed = 2.0f;


        public void ResetTraveler()
        {
            transform.position = positionOriginal;
            transform.rotation = rotationOriginal;

            MoveSpeedMS = moveSpeedOriginal;  //Use MoveSpeedMS before
            LookAtSpeed = lookAtSpeedOriginal;


            StartAtIndex(StartIndex, AutoPositionAtStart);
            SetNextPosition();
            travelIndexCounter = StartTravelDirection == TravelDirection.REVERSE ? -1 : 1;

            if (StartingMovementType == MoveType.LERP)
                moveFunc = MoveLerpSimple;
            else if (StartingMovementType == MoveType.FORWARD_TRANSLATE)
                moveFunc = MoveForwardToNext;

        }

        //public float ProBikeSpeed;  // in order to get proceeding bike speed to speed mapper

        // Start is called before the first frame update
        void Start()
        {
            MoveSpeedKmH = MoveSpeedMS * 3.6f;


            moveSpeedOriginal = MoveSpeedMS;  //Use MoveSpeedMS before
            lookAtSpeedOriginal = LookAtSpeed;

            positionOriginal = transform.position;
            rotationOriginal = transform.rotation;

            ResetTraveler();

            Move(AutoStart);
           
            

        }

        public void Move(bool tf)
        {
            isMoving = tf;
        }

        private void Awake()
        {
            if (Waypoints != null)
            {
                waypointsList = Waypoints.waypoints;
            }
        }

        // Update is called once per frame
        void Update()
        {

            if (isMoving == true && moveFunc != null)
            {
                bool arrivedAtDestination = false;

                // Call the delegate Movement Function...
                arrivedAtDestination = moveFunc();

                if (arrivedAtDestination == true)
                {
                    SetNextPosition();
                }
            }

            SpeedVariation();

        }


    void SpeedVariation()
        {

           
            //ProceedingBike decelerate when collided with buses or corner
            /* Bus was removed while purging Assets by RZ
            if (BusTriggerPBike.GetComponent<BusMovingTrigger>().BusTriggeredtoMove == true)
            {
                MoveSpeedMS = Mathf.Max(MoveSpeedMS - BusTriggeredDeceleration * Time.deltaTime, minMoveSpeedMS);
                Debug.Log("The move speed bus is " + MoveSpeedMS);
                Debug.Log("The delta time bus is " + Time.deltaTime);
                

            }
            */

            if (CornerTrigger.GetComponent<CornerDeceleration>().isReachCorner == true)
            {
                MoveSpeedMS = Mathf.Max(MoveSpeedMS - CornerDecelerationRate * Time.deltaTime, minCornerSpeed);
                Debug.Log("The move speed corner is " + MoveSpeedMS);
                Debug.Log("The delta time corner is " + Time.deltaTime);
            }
            else
            {
                MoveSpeedMS = moveSpeedOriginal;
            }
            transform.Translate(Vector3.forward * MoveSpeedMS * Time.deltaTime);

      
        }

        //detect the collision with buses 
        /*private void OnTriggerEnter(Collider other)  // check wheter the proceeding bike collided with buses
        {

            if (BusTriggerPBike.GetComponent<BusMovingTrigger>().BusTriggeredtoMove == true)
            {
                isBusesTriggered = true;            
            }
           
            if(CornerTrigger.GetComponent<CornerDeceleration>().isReachCorner == true)
            {
                isCornerDeceleration = true;
            }
        }

        private void OnTriggerExit(Collider other)  // check wheter the proceeding bike stop colliding with buses
        {
            if (BusTriggerPBike.GetComponent<BusMovingTrigger>().BusTriggeredtoMove == true)
            {
                isBusesTriggered = false;
            }

            if (CornerTrigger.GetComponent<CornerDeceleration>().isReachCorner == true)
            {
                isCornerDeceleration = false;
            }
        }*/
        
    


        /// <summary>
        /// Setup the list of positions to follow
        /// </summary>
        /// <param name="positionsList">List of Vector3s indicating move-to locations</param>
        public void SetWaypointsGroup(WaypointsGroup newGroup)
        {
            Waypoints = newGroup;
            waypointsList = null;
            if (newGroup != null)
            {
                waypointsList = newGroup.waypoints;
            }

        }


        /// <summary>
        /// Sets the position to beging moving toward; if autpAupdatePostion is true, then start
        /// at that index-related position immediately.
        /// </summary>
        /// <param name="ndx"></param>
        /// <param name="autoUpdatePosition"></param>
        void StartAtIndex(int ndx, bool autoUpdatePosition = true)
        {
            if (StartTravelDirection == TravelDirection.REVERSE)
                ndx = waypointsList.Count - ndx - 1;

            ndx = Mathf.Clamp(ndx, 0, waypointsList.Count - 1);
            positionIndex = ndx - 1;
            if (autoUpdatePosition)
            {
                transform.position = waypointsList[ndx].GetPosition();
                if (LookAtSpeed > 0)
                {
                    if (StartTravelDirection == TravelDirection.REVERSE)
                    {
                        ndx -= 1;
                        if (ndx < 0) ndx = waypointsList.Count - 1;
                    }
                    else
                    {
                        ndx += 1;
                        if (ndx >= waypointsList.Count)
                            ndx = 0;
                    }

                    /*
                    Waypoint wp = waypointsList[ndx];
                    Vector3 wpPos = wp.GetPosition();
                    Vector3 worldUp = Vector3.forward;

                    transform.LookAt(wpPos, worldUp);
                    */
                }
            }
        }



        /// <summary>
        /// Fetch the next waypoint position in the waypoints list
        /// Depending on Endpoint behavior, ping pong, loop, or stop.
        ///  - Stop : Stops movement at the endpoint
        ///  - Ping Pong: reverses traveseral of the Waypoint Positions list
        ///  - Loop : Resets the index to 0; restarting at the first waypoint
        /// </summary>
        void SetNextPosition()
        {
            int posCount = waypointsList.Count;
            if (posCount > 0)
            {

                // Reached the endpoint; determing what do do next
                if ((positionIndex == 0 && travelIndexCounter < 0) || (positionIndex == posCount - 1 && travelIndexCounter > 0))
                {
                    // Stop moving when an endpoint is reached
                    if (EndReachedBehavior == EndpointBehavior.STOP)
                    {
                        isMoving = true;
                        return;
                    }

                    // Continue movement, but reverse direction
                    else if (EndReachedBehavior == EndpointBehavior.PINGPONG)
                    {
                        travelIndexCounter = -travelIndexCounter;
                    }

                    // General Loop (default)
                    else if (EndReachedBehavior == EndpointBehavior.LOOP)
                    {

                    }

                }


                positionIndex += travelIndexCounter;

                if (positionIndex >= posCount)
                    positionIndex = 0;
                else if (positionIndex < 0)
                    positionIndex = posCount - 1;


                nextPosition = waypointsList[positionIndex].GetPosition();
                if (XYZConstraint == PositionConstraint.XY)
                {
                    nextPosition.z = transform.position.z;
                }
                else if (XYZConstraint == PositionConstraint.XZ)
                {
                    nextPosition.y = transform.position.y;
                }

                ResetMovementValues();
            }
        }

        /// <summary>
        /// Reset movement metrics based on the current transform postion and the next waypoint.
        /// </summary>
        void ResetMovementValues()
        {
            // Update current position, distance, and start time -- used in Update to move the transform
            startPosition = transform.position;
            destinationPosition = nextPosition;
            distanceToNextWaypoint = Vector3.Distance(startPosition, destinationPosition);
            distanceTraveled = 0;
            timeTraveled = 0;

        }

        /// <summary>
        /// Uses a Vector3 Lerp function to update the gameobject's transform position.
        /// MoveSpeed needs to be > 0
        /// </summary>
        /// <returns></returns>
        bool MoveLerpSimple()
        {
            if (MoveSpeedMS < 0) //Use MoveSpeedMS before
                MoveSpeedMS = 0; //Use MoveSpeedMS before

            timeTraveled += Time.deltaTime;
            distanceTraveled += Time.deltaTime * MoveSpeedMS; //Use MoveSpeedMS before
            float fracAmount = distanceTraveled / distanceToNextWaypoint;
            transform.position = Vector3.Lerp(startPosition, destinationPosition, fracAmount);
            // set LookAt speed to 0 if no rotation toward the destination is desired.
            UpdateLookAtRotation();
            return fracAmount >= 1;
        }

        /// <summary>
        /// Translates the waypoint traveler using the forward direction. Note that "forward" is dependent
        /// upon the directional/position constraint.
        ///  - XYZ: Vector3.forward
        ///  - XY : Vector3.up
        ///  - XZ : Vector3.right
        ///  
        ///  -- When using this method of translation, LOOKATSPEED must be > 0.
        /// </summary>
        /// <returns></returns>
        /// 

        //AT the trigger enter event, we gradually reduce the speed 

        //At the trigger exit, we again increase the speed until it matches MoveSpeedMS

        bool MoveForwardToNext()
        {
            if (MoveSpeedMS < 0) //Use MoveSpeedMS before
                MoveSpeedMS = 0; //Use MoveSpeedMS before

            float rate = Time.deltaTime * MoveSpeedMS;   //Use MoveSpeedMS before
            float distance = Vector3.Distance(transform.position, destinationPosition);
            if (distance < rate)
            {
                transform.position = destinationPosition;
                return true;
            }

            // If lookatspeed is 0, then set it to max; this method of movement requires
            // the gameobject transform to be looking toward it's destination
            if (LookAtSpeed <= 0) LookAtSpeed = float.MaxValue;

            UpdateLookAtRotation();

            Vector3 moveDir = Vector3.forward;
            if (XYZConstraint == PositionConstraint.XY)
            {
                moveDir = Vector3.up;
            }

            transform.Translate(moveDir * rate);

            return false;

        }

        /// <summary>
        /// Rotate the traveler to "look" at the next waypoint.
        /// Note: When using the FORWARD_TRANSLATE movement mode, LookAtSpeed is always > 0
        /// </summary>
        void UpdateLookAtRotation()
        {
            // LookatSpeed is 0; no rotating...
            if (LookAtSpeed <= 0) return;

            float step = LookAtSpeed * Time.deltaTime;
            Vector3 targetDir = nextPosition - transform.position;


            if (XYZConstraint == PositionConstraint.XY)
            {
                float angle = Mathf.Atan2(targetDir.y, targetDir.x) * Mathf.Rad2Deg - 90;
                Quaternion qt = Quaternion.AngleAxis(angle, Vector3.forward);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, qt, step);
            }

            else if (XYZConstraint == PositionConstraint.XZ)
            {
                float angle = Mathf.Atan2(targetDir.x, targetDir.z) * Mathf.Rad2Deg;
                Quaternion qt = Quaternion.AngleAxis(angle, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, qt, step);
            }
            else
            {
                Vector3 newDir = Vector3.RotateTowards(transform.forward, targetDir, step, 0.0f);
                // Move our position a step closer to the target.
                transform.rotation = Quaternion.LookRotation(newDir);

            }

        }

    }
}
