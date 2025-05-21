using System.Runtime.InteropServices;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Uduino;
using System;
using UnityEngine.SceneManagement;
using System.IO;
using MotionSystems;
using WaypointsFree;
//using static Unity.Mathematics;

/* 
 * Legacy_lab is the original one with platform and physical sensors
 * EternityBike (aka forschungsfest) is the current working version with the controller on the SteeringWheel
 */
public enum BikeMode 
{
    Legacy_Lab,
    EternityBike //aka Forschungsfest
};

/* 
 * Connection-String for TrainingMachine Tacx Flux-2 which connects through BLE supported by the Arduino 
 */
public static class BikeModeExtensions
{
    public static string GetTacxString(this BikeMode mode)
    {
        switch (mode)
        {
            case BikeMode.Legacy_Lab:
                return "Tacx Flux-2 36688";
            case BikeMode.EternityBike:
                return "Tacx Flux-2 18201";
            default:
                return "Bikmode not set";
        }
    }
}

public class GameControllerScript : MonoBehaviour
{
    #region EternityBike-Modes
    public BikeMode bikemode;

    public enum RealismSupportLevel { OptimizedSupport, NoSupport, FullSupport };
    public RealismSupportLevel currentRealismSupportLevel;

    public enum VisualTiltingMode { SynchronisedTilting, NoTilting}
    public VisualTiltingMode currentVisualTiltingMode;

    public enum UserBikeStopMode { AutoStop, ManuStop };   // different stop modes for eternity bike, AutoStop for cruise control and ManuStop for baseline mode
    public UserBikeStopMode currentStopMode;

    public AbstractPlatformCalculationModel[] calculationModelRegistry = new AbstractPlatformCalculationModel[] {
        new RealismPlatformCalculationModel(
            PLATFORM_POSITION_LOGIC_MIN, PLATFORM_POSITION_LOGIC_MAX,
            PLATFORM_POSITION_LOGIC_MIN, PLATFORM_POSITION_LOGIC_MAX,
            0, 30
        ),
        new ApproximatedPlatformCalculationModel(
            PLATFORM_POSITION_LOGIC_MIN, PLATFORM_POSITION_LOGIC_MAX,
            PLATFORM_POSITION_LOGIC_MIN, PLATFORM_POSITION_LOGIC_MAX,
            0, 30
        ),
        new NoTiltPlatformCalculationModel(
            PLATFORM_POSITION_LOGIC_MIN, PLATFORM_POSITION_LOGIC_MAX,
            PLATFORM_POSITION_LOGIC_MIN, PLATFORM_POSITION_LOGIC_MAX,
            0, 30
        ),
        new NoTiltAndNoPitchPlatformCalculationModel(
            PLATFORM_POSITION_LOGIC_MIN, PLATFORM_POSITION_LOGIC_MAX,
            PLATFORM_POSITION_LOGIC_MIN, PLATFORM_POSITION_LOGIC_MAX,
            0, 30)
    };
    public int activeCalculationModelIndex = 0;

    public bool controller_mode = true;

    public DetectMinMaxLineCollision_version2 detectMinMaxLine;
    #endregion

    public bool loadArduinoWithNewCode = false;

    #region Physical-Parameters
    public float SteeringAngle = 0.0f;

    public float BikeSpeed = 0f;
    public float ISteeringAngle = 0f;

    public const float GRAVITATIONAL_ACCELERATION = 9.81f;
    public float ICurveRadius = 0.0f;
    public float supportedAngle = 40.0f;
    public float Sign = 0.0f;
    public float supportFactor = 0.0f;
    public float speedCalculationMultiplier = 0.6f;
    public float speedCalculationExponent = 1.7f;
    #endregion

    #region GameObjects
    private GameObject Bicycle = null;

    Transform[] routes = null;
    Transform[] spawnPoints = null;

    int currentRoute = 0;
    int currentSpawnPoint = 0;

    public int level = 0;
    const int max_levels = 6;

    #endregion

    #region Logger
    float elapsed = 0f;
    bool recording = false;
    public Logger logger = new Logger();
    #endregion

    #region FSMI-Parameters

    public float appliedBrakeForce = 0;
    public float ITiltAngle = 0f;
    public float ITiltAngleMax = 0f;
    public float optimizedITiltAngleFactor = 100.00f;
    public float calculatedTiltAngle = 0.0f;

    public float RollMultiplier = 1f;
    public float[] custom_rollMultipliers = { 0f, 0.25f, 0.5f, 0.75f, 1f };
    public int custom_rollMultiplierInd = 4;

    // Platform logical min/max coordinates
    public const int PLATFORM_POSITION_LOGIC_MIN = -32767;
    public const int PLATFORM_POSITION_LOGIC_MAX = 32767;
    //public const int PLATFORM_POSITION_LOGIC_MIN = -16384;
    //public const int PLATFORM_POSITION_LOGIC_MAX = 16384;

    // Heave maximum value that is available in the game
    private const float DRAWING_HEAVE_MAX = 1.0f;

    // Heave change step
    private const float DRAWING_HEAVE_STEP = 0.05f;

    // Maximum value of pitch angle that is available in the game
    //private const float DRAWING_PITCH_MAX = 16;
    private const float DRAWING_PITCH_MAX = 2;

    // Pitch change step
    //private const float DRAWING_PITCH_STEP = 1;
    private const float DRAWING_PITCH_STEP = 0.1f;

    // Maximum value of roll angle that is available in the game
    //private const float DRAWING_ROLL_MAX = 16;
    private const float DRAWING_ROLL_MAX = 2;

    // Pitch change step
    //private const float DRAWING_ROLL_STEP = 1;
    private const float DRAWING_ROLL_STEP = 0.1f;

    // Shaft object
    private GameObject m_shaft = null;

    // Board object
    private GameObject m_board = null;

    // Origin position of the shaft
    private Vector3 m_originPosition;

    // Origin rotation of the board
    private Vector3 m_originRotation;

    // Current platform's heave in game
    private float m_heave = 0;

    // Current platform's pitch in game
    private float m_pitch = 0;

    // Current platform's roll in game
    private float m_roll = 0;

    // FSMI api
    private ForceSeatMI m_fsmi;

    // Position in logical coordinates that will be send to the platform
    private FSMI_TopTablePositionLogical m_platformPosition = new FSMI_TopTablePositionLogical();

    /** 
     * Rolling Position (left/right lean), (positive -> right lean, negative -> left lean)
     * <see cref="PLATFORM_POSITION_LOGIC_MIN"/>
     * <see cref="PLATFORM_POSITION_LOGIC_MAX"/>
     */
    public float RollPosition = 0;

    /**
     * Forward Pitch (positive -> bike leans forward, negative -> bike leans back)
     * max val: <see cref="DRAWING_PITCH_MAX"/>
     * with incremental steps of <see cref="DRAWING_PITCH_STEP"/>
     */
    public float PitchPosition = 0;

    #endregion

    void Start()
    {
        setCalculationModel();
        setUduinoEvent();
        setRealismSupportLevel();
        setFSMI();
        setGameObjects();
    }

    private void setCalculationModel()
    {
        //@levent here, it's ugly i know... i will think of something better...
        activeCalculationModelIndex = 0;
        ((RealismPlatformCalculationModel)calculationModelRegistry[activeCalculationModelIndex]).setPlatform(this);
    }
    private void setRealismSupportLevel()
    {
        if (currentRealismSupportLevel == RealismSupportLevel.OptimizedSupport)
        {
            speedCalculationMultiplier = 0.6f;
            speedCalculationExponent = 1.7f;
            RollMultiplier = custom_rollMultipliers[custom_rollMultiplierInd];
        }
        else if (currentRealismSupportLevel == RealismSupportLevel.FullSupport)
        {
            speedCalculationMultiplier = 1.0f;
            speedCalculationExponent = 2.0f;
            RollMultiplier = custom_rollMultipliers[custom_rollMultiplierInd];
        }
        else if (currentRealismSupportLevel == RealismSupportLevel.NoSupport)
        {
            RollMultiplier = 0;
        }
    }
    private void setUduinoEvent()
    {
        /*
       Testing if i can send the bikename at the start

       String[] words2 = bikemode.GetTacxString().Split(' ');
       UduinoManager.Instance.sendCommand("star", words2[2]);
        */

        //Uduino
        UduinoManager.Instance.OnDataReceived += UpdateEternityBikeData;
    }

    /*Load ForceSeatMI library from ForceSeatPM installation directory */
    private void setFSMI()
    {
        m_fsmi = new ForceSeatMI();

        if (m_fsmi.IsLoaded())
        {
            // Find platform's components
            m_shaft = GameObject.Find("Shaft");
            m_board = GameObject.Find("Board");

            SaveOriginPosition();
            SaveOriginRotation();

            // Prepare data structure by clearing it and setting correct size
            m_platformPosition.mask = 0;
            m_platformPosition.structSize = (byte)Marshal.SizeOf(m_platformPosition);

            m_platformPosition.state = FSMI_State.NO_PAUSE;

            // Set fields that can be changed by demo application
            m_platformPosition.mask = FSMI_POS_BIT.STATE | FSMI_POS_BIT.POSITION;

            m_fsmi.SetAppID(""); // If you have dedicated app id, remove ActivateProfile calls from your code
            m_fsmi.ActivateProfile("SDK - Positioning");
            m_fsmi.BeginMotionControl();

            SendDataToFSMIPlatform();
        }
        else
        {
            Debug.LogError("ForceSeatMI library has not been found! Please install ForceSeatPM.");
        }
    }
    private void setGameObjects()
    {
        {
            var routesRoot = GameObject.Find("Routes");

            var tmp = routesRoot.GetComponentsInChildren<Transform>(true);
            var tmpRoutes = new List<Transform>();
            for (int i = 0; i < tmp.Length; ++i)
            {
                var cur = tmp[i];
                if (cur.parent.gameObject == routesRoot)
                {
                    tmpRoutes.Add(cur);
                    cur.gameObject.SetActiveRecursively(false);
                }
            }
            this.routes = tmpRoutes.ToArray();

            Debug.Log("ROUTES LEN " + this.routes.Length);
        }
        {
            var spawns = GameObject.Find("Spawns");
            var tmp = spawns.GetComponentsInChildren<Transform>(true);
            spawnPoints = new Transform[tmp.Length - 1];
            for (int i = 1; i < tmp.Length; ++i)
            {
                spawnPoints[i - 1] = tmp[i];
            }
        }
        var bike = GameObject.Find("EternityBike");
    }
    void loadLevel(int ind)
    {
        currentRoute = ind;
        currentSpawnPoint = ind;

        for (int i = 0; i < routes.Length; ++i) routes[i].gameObject.SetActiveRecursively(false);
        routes[currentRoute].gameObject.SetActiveRecursively(true);


        var spawnpoint = spawnPoints[currentSpawnPoint];

        Bicycle.transform.position = spawnpoint.position;
        Bicycle.transform.rotation = spawnpoint.rotation;
        Debug.Log("Level Loaded " + ind);
    }

    void Update()
    {
        if (loadArduinoWithNewCode)
        {
            testArduino();
        }

        if (logger.isActive())
        {
            elapsed += Time.deltaTime;
        }

        updateFSMI();
        handleInput();
    }
    void FixedUpdate()
    {
        // Update values in order to received user's input
        UpdateValue(ref m_pitch, Input.GetAxis("Vertical"), DRAWING_PITCH_STEP, -DRAWING_PITCH_MAX, DRAWING_PITCH_MAX);
        UpdateValue(ref m_roll, SteeringAngle, DRAWING_ROLL_STEP, -DRAWING_ROLL_MAX, DRAWING_ROLL_MAX);
        UpdateValue(ref m_heave, Input.GetKey(KeyCode.Space) ? 1 : 0, DRAWING_HEAVE_STEP, 0, DRAWING_HEAVE_MAX);

        //TODO CHECK FUNCTION
        // if (bikemode == BikeMode.Lab){
        //UpdateValue(ref m_pitch, -0.5f, DRAWING_PITCH_STEP, -DRAWING_PITCH_MAX, DRAWING_PITCH_MAX); //Veränderung der Steigung
        //UpdateValue(ref m_roll, Input.GetAxis("Horizontal"), DRAWING_ROLL_STEP, -DRAWING_ROLL_MAX, DRAWING_ROLL_MAX);
        //UpdateValue(ref m_roll, 0.8f, DRAWING_ROLL_STEP, -DRAWING_ROLL_MAX, DRAWING_ROLL_MAX);
        //UpdateValue(ref m_heave, Input.GetKey(KeyCode.Space) ? 1 : 0, DRAWING_HEAVE_STEP, 0, DRAWING_HEAVE_MAX);
        // }
    }

    private void handleInput()
    {
        if (Input.GetKeyDown("r"))
        {
            SceneManager.LoadScene("BikeSimulator"); //Load Main Scene
            Debug.Log("Reset Scene");
        }
        if (Input.GetKeyDown("p"))
        {
            Bicycle = GameObject.Find("EternityBike");
            var spawnpoint = spawnPoints[currentSpawnPoint];
            currentSpawnPoint = (currentSpawnPoint + 1) % spawnPoints.Length;

            Bicycle.transform.position = spawnpoint.position;
            Bicycle.transform.rotation = spawnpoint.rotation;

            //Bicycle.transform.position = new Vector3(-360, 0.09f, 430);
            Debug.Log("Load Parking lot"); //TODO Check if still acurate
        }
        if (Input.GetKeyDown("l"))
        {
            for (int i = 0; i < routes.Length; ++i) routes[i].gameObject.SetActiveRecursively(false);
            routes[currentRoute].gameObject.SetActiveRecursively(true);
            currentRoute = (currentRoute + 1) % routes.Length;
        }
        if (Input.GetKeyDown("n"))
        {
            currentRealismSupportLevel = RealismSupportLevel.NoSupport;
            setRealismSupportLevel();
            Debug.Log("Changed Support Level to: No Support");
        }
        if (Input.GetKeyDown("f"))
        {
            currentRealismSupportLevel = RealismSupportLevel.FullSupport;
            setRealismSupportLevel();
            Debug.Log("Changed Support Level to: Full Support");
        }
        if (Input.GetKeyDown("o"))
        {
            currentRealismSupportLevel = RealismSupportLevel.OptimizedSupport;
            setRealismSupportLevel();
            Debug.Log("Changed Support Level to: Optimized Support");
        }
        if (Input.GetKeyDown("t"))
        {
            activeCalculationModelIndex = (activeCalculationModelIndex + 1) % calculationModelRegistry.Length;
            Debug.Log("New Active Calculation-Model: " + calculationModelRegistry[activeCalculationModelIndex].getLabel());
        }
        if (Input.GetKeyDown("x"))
        {
            Bicycle = GameObject.Find("EternityBike");
            Bicycle.transform.position = new Vector3(0f, 0.1f, 0f);
            Debug.Log("Load New Level"); //TODO Check Function
        }
        if (Input.GetKeyDown("c"))
        {
            controller_mode = !controller_mode;
            Debug.Log("Controller Mode " + controller_mode);
        }
        if (Input.GetKeyDown("i"))
        {
            loadLevel(level);
            level = (level + 1) % max_levels;
        }
        if (Input.GetKeyDown("z"))
        {
            logger.setActive(!logger.isActive(), this);
            elapsed = 0;
        }
        if (OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger) > 0)
        {
           //TODO Quick Fix for brakes
            Debug.Log("Secondary Trigger: " + OVRInput.Axis1D.SecondaryIndexTrigger.ToString());
        }
    }

    private void updateFSMI()
    {
        if (m_fsmi != null && m_fsmi.IsLoaded())
        {
            // Set back origin position and then modify it
            m_shaft.transform.position = m_originPosition;
            m_shaft.transform.Translate(0, m_heave, 0);

            // Set back origin rotation and then modify it
            m_board.transform.eulerAngles = m_originRotation;
            m_board.transform.Rotate(m_pitch, 0, -m_roll);

            SendDataToFSMIPlatform();
        }
    }

    private void testArduino()
    {
        // for testing issues

        /*String[] words = bikemode.GetTacxString().Split(' ');
         UduinoManager.Instance.sendCommand("setTacx0", words[0]);
         UduinoManager.Instance.sendCommand("setTacx1", words[1]);
         UduinoManager.Instance.sendCommand("setTacx2", words[2]);*/

        String[] words = bikemode.GetTacxString().Split(' ');
        // Debug.Log("TACX: " + words[2]);
        UduinoManager.Instance.sendCommand("tacx", words[2]);


        //UduinoManager.Instance.CloseAllDevices();
        //UduinoManager.Instance.DiscoverPorts();
        // TODO deactivate bike until everything is set up!
    }

    private void UpdateValue(ref float value, float input, float step, float min, float max)
    {
        if (0 < input)
        {
            value = Mathf.Clamp(value + step, min, max);
        }
        else if (0 > input)
        {
            value = Mathf.Clamp(value - step, min, max);
        }
        else if (value > 0)
        {
            value = Mathf.Clamp(value - step, 0, max);
        }
        else if (value < 0)
        {
            value = Mathf.Clamp(value + step, min, 0);
        }
    }

    /*here control the baseline stop and cruise control stop of user bike*/
    void OnTriggerEnter(Collider other)
    {
        if (currentStopMode == UserBikeStopMode.AutoStop && other.tag.Equals("pedestrain"))
        {
            UBikeStop();
        }

        else if (currentStopMode == UserBikeStopMode.ManuStop && other.tag.Equals("pedestrain") && appliedBrakeForce != 0)
        {
            UBikeStop();
        }
    }

    void UBikeStop()
    {
        BikeSpeed = 0;

        Debug.Log("The EternityBike is stopped!");
    }

    void OnDestroy()
    {
        if (m_fsmi.IsLoaded())
        {
            m_fsmi.EndMotionControl();
            m_fsmi.Dispose();
        }
    }

    private void SaveOriginPosition()
    {
        // Save origin position of the platform's shaft
        var x = m_shaft.transform.position.x;
        var y = m_shaft.transform.position.y;
        var z = m_shaft.transform.position.z;

        m_originPosition = new Vector3(x, y, z);
    }

    private void SaveOriginRotation()
    {
        // Save origin rotation of the platform's board
        var x = m_board.transform.eulerAngles.x;
        var y = m_board.transform.eulerAngles.y;
        var z = m_board.transform.eulerAngles.z;

        m_originRotation = new Vector3(x, y, z);
    }

    private void SendDataToFSMIPlatform()
    {
        // Convert parameters to logical units
        m_platformPosition.state = FSMI_State.NO_PAUSE;
        
        m_platformPosition.pitch = (short)Mathf.Clamp(PitchPosition, PLATFORM_POSITION_LOGIC_MIN, PLATFORM_POSITION_LOGIC_MAX);
        m_platformPosition.roll = (short)Mathf.Clamp(RollPosition * RollMultiplier, PLATFORM_POSITION_LOGIC_MIN, PLATFORM_POSITION_LOGIC_MAX);
        m_platformPosition.heave = (short)Mathf.Clamp(m_heave / DRAWING_HEAVE_MAX * PLATFORM_POSITION_LOGIC_MAX, PLATFORM_POSITION_LOGIC_MIN, PLATFORM_POSITION_LOGIC_MAX);

        // Send data to platform
        m_fsmi.SendTopTablePosLog(ref m_platformPosition);
    }
   
    void UpdateEternityBikeData(string data, UduinoDevice device)
    {
        if (device.name.Equals("IndoorBikeData")) {
            Bicycle = GameObject.Find("EternityBike");
            string[] values = data.Split(','); // [Speed0, processedSteeringAngle, ForntBreakForche, RearBrakeForce, CombineBreakForce, Resistance]

            //READ AND VALIDATE INPUT
            float Velocity = 0;
            if (controller_mode)
            {
                float vertical = Input.GetAxis("Vertical");
                Velocity = vertical * 20;
            }
            else
            {
                //Test from sonja 09.01.22
                int i;
                if (int.TryParse(values[0].Substring(0, 1), out i)) {
                    Velocity = float.Parse(values[0]);      // Velocity refers to the speed of the tacx
                }

            }

            BikeSpeed = Velocity;

            float iSteeringAngle = 0;
            if (controller_mode)
            {
                float vertical = Input.GetAxis("Horizontal");
                if (bikemode == BikeMode.EternityBike)
                {
                    iSteeringAngle = Math.Max(Math.Min(vertical, 1.0f), -1.0f) * 180;
                }
                else if (bikemode == BikeMode.Legacy_Lab) {
                    iSteeringAngle = Math.Max(Math.Min(vertical, 1.0f), -1.0f) * 90;
                }
            }
            else
            {
                if (bikemode == BikeMode.EternityBike)
                {
                    iSteeringAngle = BikeControllerScript.steeringAngle; // -90, 0, +90
                }
                else if (bikemode == BikeMode.Legacy_Lab)
                {
                    iSteeringAngle = float.Parse(values[1]);
                }
            }

            if (bikemode == BikeMode.EternityBike & values.Length > 1) {

                if (iSteeringAngle < 0)
                {
 
                    Sign = -1;

                }
                else
                {
                    Sign = 1;
                }

                // -90 to 90
                ISteeringAngle = iSteeringAngle;

                // -1 to 1
                SteeringAngle = iSteeringAngle.Remap(90, -90, 1.0f, -1.0f);
                SteeringAngle = (float)Math.Round(SteeringAngle * 100f) / 100f;

                if (logger.isActive() && elapsed > logger.frequency)
                {
                    logger.log(this, elapsed, Velocity);
                    elapsed = 0;
                }

                //Debug.Log("Steering ANGLE == " + SteeringAngle + " IST " + ISteeringAngle);
                //Debug.Log("Velocity: " + Velocity + " SteeringAngle: " + iSteeringAngle + " SteeringAngle: " + SteeringAngle);

                //TODO FIX BRAKES
                //float FrontBrakeForce = 0;//float.Parse(values[2]);
                //float RearBrakeForce = 0;//float.Parse(values[3]);
;
                float CombinedBrakeForce;
                 if (controller_mode)
                 {
                     var hor2 = Input.GetAxis("Horizontal2");
                     CombinedBrakeForce = hor2 * 200;
                     if (CombinedBrakeForce < 0) CombinedBrakeForce *= -1;
                 }
                 else
                 {
                     CombinedBrakeForce = float.Parse(values[4]);
                 }

                float Resistance = float.Parse(values[5]);

                //APPLY PARSED DATA

                var activeCalculationModel = calculationModelRegistry[activeCalculationModelIndex];
                appliedBrakeForce = activeCalculationModel.calculateBreakForce(BikeSpeed, CombinedBrakeForce);
                PitchPosition = activeCalculationModel.calculatePitch(Bicycle.transform.forward, appliedBrakeForce);
                RollPosition = activeCalculationModel.calculateTilt(Velocity, SteeringAngle);

                //Debug.Log("Velocity: " + Velocity + " SteeringAngle: " + iSteeringAngle + " SteeringAngle: " + SteeringAngle + " FrontBrakeForce: " + FrontBrakeForce + " RearBrakeForce: " + RearBrakeForce + " CombinedBrakeForce: " + CombinedBrakeForce + " Resistance: " + Resistance);

                if (logger.isActive() && elapsed > logger.frequency)
                {
                    logger.log(this, elapsed, Velocity, PitchPosition, RollPosition, appliedBrakeForce, Resistance);
                    elapsed = 0;
                }

            }
            else if (bikemode == BikeMode.Legacy_Lab)
            {
                // Dectect negativ Steering Angle
                if (iSteeringAngle > 450)
                {
                    String RemoveAscii = iSteeringAngle.ToString();
                    RemoveAscii = RemoveAscii.Substring(2);
                    iSteeringAngle = float.Parse(RemoveAscii);
                    Sign = -1;
                    iSteeringAngle = iSteeringAngle * Sign;
                    // Debug.Log("iSteeringAngle in 450: " + iSteeringAngle);
                }
                else
                {
                    Sign = 1;
                }

                if (iSteeringAngle <= -90)
                {
                    iSteeringAngle = -90;
                }
                else if (iSteeringAngle >= 90)
                {
                    iSteeringAngle = 90;
                }

                ISteeringAngle = iSteeringAngle;

                SteeringAngle = iSteeringAngle.Remap(-90, 90, -1.0f, 1.0f);
                SteeringAngle = (float)Math.Round(SteeringAngle * 100f) / 100f;

                float FrontBrakeForce = float.Parse(values[2]);
                float RearBrakeForce = float.Parse(values[3]);

                float CombinedBrakeForce;
                if (controller_mode)
                {
                    var hor2 = Input.GetAxis("Horizontal2");
                    // Debug.Log("combinedbf before: " + hor2);
                    CombinedBrakeForce = hor2 * 200;
                    if (CombinedBrakeForce < 0)
                    {
                        CombinedBrakeForce *= -1;
                    }
                }
                else
                {
                    CombinedBrakeForce = float.Parse(values[4]);
                }
                // Debug.Log("combinedbf: " + CombinedBrakeForce);


                float Resistance = float.Parse(values[5]);

                //APPLY PARSED DATA

                var activeCalculationModel = calculationModelRegistry[activeCalculationModelIndex];

                appliedBrakeForce = activeCalculationModel.calculateBreakForce(BikeSpeed, CombinedBrakeForce);
                PitchPosition = activeCalculationModel.calculatePitch(Bicycle.transform.forward, appliedBrakeForce);
                RollPosition = activeCalculationModel.calculateTilt(Velocity, SteeringAngle);

                Debug.Log("Velocity: " + Velocity + " SteeringAngle: " + iSteeringAngle + " SteeringAngle: " + SteeringAngle + " FrontBrakeForce: " + FrontBrakeForce + " RearBrakeForce: " + RearBrakeForce + " CombinedBrakeForce: " + CombinedBrakeForce + " Resistance: " + Resistance);

                if (logger.isActive() && elapsed > logger.frequency)
                {
                    logger.log(this, elapsed, Velocity, PitchPosition, RollPosition, appliedBrakeForce, Resistance);
                    elapsed = 0;
                }
            }
        }
        else if (device.name.Equals("MotorBoard")) {
            Debug.Log("phys Switch general read: " + data);
            if (data.Equals("0") || data.Equals("0"))
                Debug.Log("phys Switch read: " + data);
                detectMinMaxLine.physicalSwitch = Int32.Parse(data);
        }
    }

}

public static class ExtensionMethods
{
    public static float Remap(this float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }
}

public class Logger
{
    const string filename = "C:\\Users\\Bikelab\\Desktop\\MYLOG.txt";

    private bool active = false;
    public float frequency = 0.1f;

    public Logger() { }

    public void setActive(bool active, GameControllerScript platform)
    {
        this.active = active;
        List<string> lines = new List<string> { };
        if (this.active)
        {
            lines.Add("");
            lines.Add("Recording Started;" + DateTime.Now);
            lines.Add("Configurations");
            lines.Add("Controller Mode;" + platform.controller_mode);
            lines.Add("Selected Level;" + platform.level);

            Debug.Log("LOGGER ACTIVATED");
        }
        else
        {
            lines.Add("Recording Ended;" + DateTime.Now);
            Debug.Log("LOGGER DEACTIVATED");
        }
        File.AppendAllLines(filename, lines);
    }

    public void log(GameControllerScript platform, float elapsedTime, float velocity)
    {
        List<string> lines = new List<string> { };
        lines.Add("ElapsedTime;" + elapsedTime + ";Velocity;" + velocity);
        File.AppendAllLines(filename, lines);
    }

    public void log(GameControllerScript platform, float elapsedTime, float velocity, float rollposition, float pitchposition, float combinedBrakeforce, float Resistance)
    {
        List<string> lines = new List<string> { };
        lines.Add("ElapsedTime;" + elapsedTime + ";Velocity;" + velocity + ";Rollposition;" + rollposition + ";Pitchposition;" + pitchposition + ";Combined Brakeforce;" + combinedBrakeforce + ";Resistance;" + Resistance);
        File.AppendAllLines(filename, lines);
    }

    public bool isActive()
    {
        return this.active;
    }
}

public abstract class AbstractPlatformCalculationModel
{
    private readonly float minTilt;
    private readonly float maxTilt;

    public readonly float minPitch;
    public readonly float maxPitch;

    private readonly float minBrakeForce;
    private readonly float maxBrakeForce;


    public AbstractPlatformCalculationModel(
        float minTilt,
        float maxTilt,
        float minPitch,
        float maxPitch,
        float minBrakeForce,
        float maxBrakeForce
        )
    {
        this.minTilt = minTilt;
        this.maxTilt = maxTilt;
        this.minPitch = minPitch;
        this.maxPitch = maxPitch;
        this.minBrakeForce = minBrakeForce;
        this.maxBrakeForce = maxBrakeForce;
    }

    public abstract String getLabel();

    public delegate float Calculation();

    public static float getResultWithinRange(float min, float max, Calculation calculation)
    {
        float ret = calculation.Invoke();

        ret = Math.Min(ret, max);
        ret = Math.Max(ret, min);

        return ret;
    }

    public float calculateTilt(float velocity, float steeringAngle)
    {
        float ret = getResultWithinRange(minTilt, maxTilt, () => calculateTilt2(velocity, steeringAngle));
            Debug.Log("Calculated Tilt: " + ret);
        return ret;
    }

    protected abstract float calculateTilt2(float velocity, float steeringAngle);

    public float calculatePitch(Vector3 bikeForward, float brakeForce)
    {
        var ret = getResultWithinRange(minPitch, maxPitch, () => calculatePitch2(bikeForward, brakeForce));
        Debug.Log("Calculated Pitch: " + ret);
        return ret;
    }

    protected abstract float calculatePitch2(Vector3 bikeForward, float brakeForce);

    public float calculateBreakForce(float bikeSpeed, float combinedBrakeForce)
    {
        var ret = getResultWithinRange(minBrakeForce, maxBrakeForce, () => calculateBreakForce2(bikeSpeed, combinedBrakeForce));
        Debug.Log("Calculated Brakeforce: " + ret);
        return ret;
    }
    protected abstract float calculateBreakForce2(float bikeSpeed, float combinedBrakeForce);
}

public class RealismPlatformCalculationModel : ApproximatedPlatformCalculationModel
{
   GameControllerScript platform;
    

    public RealismPlatformCalculationModel(float minTilt,
        float maxTilt,
        float minPitch,
        float maxPitch,
        float minBrakeForce,
        float maxBrakeForce
        ) : base(minTilt, maxTilt, minPitch, maxPitch, minBrakeForce, maxBrakeForce)
    {
        this.platform = null;
    }

    public void setPlatform(GameControllerScript platform)
    {
        this.platform = platform;
    }

    public override string getLabel()
    {
        return "Realism";
    }

    /**
     * TODO: @levent fix ugly references to platform...
     */
    protected override float calculateTilt2(float velocity, float steeringAngle)
    {
        float Range = GameControllerScript.PLATFORM_POSITION_LOGIC_MAX / (this.platform.supportedAngle * 1000);

        float speedInMS = this.platform.BikeSpeed / 3.6f;
        double iCalc = Mathf.Atan((float)Math.Pow(this.platform.speedCalculationMultiplier * speedInMS, this.platform.speedCalculationExponent) / (GameControllerScript.GRAVITATIONAL_ACCELERATION * this.platform.ICurveRadius)) * (180 / Math.PI);
        this.platform.calculatedTiltAngle = (float)iCalc;

        this.platform.supportFactor = 90 / this.platform.supportedAngle;

        if (this.platform.calculatedTiltAngle >= this.platform.supportedAngle)
        {
            this.platform.calculatedTiltAngle = this.platform.supportedAngle;
        }

        float MultipliedTiltAngle = this.platform.calculatedTiltAngle * 1000;


        float RollPosition = Range * MultipliedTiltAngle * this.platform.Sign;
        //Debug.Log("Calculate 2 RollPosition (Range, MultTiltAnge, Sign: " + RollPosition + " ("+ Range+ ", "+ MultipliedTiltAngle+ "," + platform.Sign + ")");

        //ITiltAngle = realisticITiltAngleFactor * RollPosition / PLATFORM_POSITION_LOGIC_MAX;
        //ITiltAngle = calculatedTiltAngle;
        //Debug.LogWarning("ITiltAngle: " + ITiltAngle);

        this.platform.ITiltAngleMax = this.platform.ITiltAngle;

        if (this.platform.currentRealismSupportLevel == GameControllerScript.RealismSupportLevel.OptimizedSupport)
        {
            this.platform.ITiltAngle = this.platform.calculatedTiltAngle * this.platform.Sign / this.platform.optimizedITiltAngleFactor;
        }
        else if (this.platform.currentRealismSupportLevel == GameControllerScript.RealismSupportLevel.FullSupport)
        {
            this.platform.ITiltAngle = (float)Math.Truncate(this.platform.calculatedTiltAngle * this.platform.Sign);
        }
        else if (this.platform.currentRealismSupportLevel == GameControllerScript.RealismSupportLevel.NoSupport)
        {
            this.platform.ITiltAngle = 0;
        }

        return RollPosition * this.platform.custom_rollMultipliers[this.platform.custom_rollMultiplierInd];
    }
}

public class ApproximatedPlatformCalculationModel : AbstractPlatformCalculationModel
{
    public ApproximatedPlatformCalculationModel(float minTilt,
        float maxTilt,
        float minPitch,
        float maxPitch,
        float minBrakeForce,
        float maxBrakeForce
        ) : base(minTilt, maxTilt, minPitch, maxPitch, minBrakeForce, maxBrakeForce)
    {
    }

    public override string getLabel()
    {
        return "Approximated";
    }

    private const float tiltMultiplicator = 4000;

    protected override float calculateTilt2(float velocity, float steeringAngle)
    {
        float toApply = velocity * steeringAngle * tiltMultiplicator;

        if (velocity > 10)
        {
            //alles gut
        }
        else if (velocity > 6)
        {
            toApply *= 0.6f;
        }
        else if (velocity > 4)
        {
            toApply *= 0.4f;
        }
        else if (velocity > 2)
        {
            toApply *= 0.2f;
        }
        else
        {
            toApply = 0f;
        }
        return toApply;
    }

    protected override float calculateBreakForce2(float bikeSpeed, float combinedBrakeForce)
    {
        float applyBrakeForce = 0;
        if (combinedBrakeForce < 20)
        {
            Debug.Log("Ignore BrakeForce");
        }
        else
        {
            applyBrakeForce = combinedBrakeForce / 15;
            //sollte nie vorkommen aber sicher ist sicher
            if (applyBrakeForce < 0)
            {
                applyBrakeForce = 0;
            }
        }

        return applyBrakeForce;
    }

    private const float pitchDeadzone = 1.0f;
    private const float pitchMultiplier = 600.0f;

    protected override float calculatePitch2(Vector3 bikeForward, float brakeForce)
    {
        float angle = Vector3.Angle(bikeForward, Vector3.up);

        //Debug.Log("pitch angle: " + angle);
        /*
        if (angle >= 90 - pitchDeadzone && angle <= 90 + pitchDeadzone)
        {
            return 0;
        }
        */
        //TODO PITCH ÄNDERUNGEN


        float pitchToApply = (angle - 90) * pitchMultiplier;
        //    Debug.Log("Pitch To Apply Before " + pitchToApply);
        pitchToApply += brakeForce * 500;
        //    Debug.Log("Pitch To Apply After " + pitchToApply);

        return pitchToApply;
    }
}

public class NoTiltPlatformCalculationModel : ApproximatedPlatformCalculationModel
{
    public NoTiltPlatformCalculationModel(float minTilt,
        float maxTilt,
        float minPitch,
        float maxPitch,
        float minBrakeForce,
        float maxBrakeForce
        ) : base(minTilt, maxTilt, minPitch, maxPitch, minBrakeForce, maxBrakeForce)
    {
    }

    public override string getLabel()
    {
        return "No Tilt";
    }

    protected override float calculateTilt2(float velocity, float steeringAngle)
    {
        return 0;
    }
}

public class NoTiltAndNoPitchPlatformCalculationModel : NoTiltPlatformCalculationModel
{
    public NoTiltAndNoPitchPlatformCalculationModel(float minTilt,
        float maxTilt,
        float minPitch,
        float maxPitch,
        float minBrakeForce,
        float maxBrakeForce
        ) : base(minTilt, maxTilt, minPitch, maxPitch, minBrakeForce, maxBrakeForce)
    {
    }

    public override string getLabel()
    {
        return "No Tilt & No Pitch";
    }

    protected override float calculatePitch2(Vector3 bikeForward, float brakeForce)
    {
        return 0;
    }
}
