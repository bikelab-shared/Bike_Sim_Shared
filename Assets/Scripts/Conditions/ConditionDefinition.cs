using UnityEngine;
using static GameControllerScript;

public class ConditionDefinition : MonoBehaviour
{
    [Header("Bike Positioning")]
    public Vector3 startPosition = Vector3.zero;
    public Vector3 startRotationEuler = Vector3.zero;

    [Header("Bike Parameters")]
    public float startSpeed = 0f;
    public VisualTiltingMode visualTiltingMode = VisualTiltingMode.Disabled;


    public void ApplyToBike(GameObject bike, GameControllerScript controller)
    {
        // Set position and rotation
        bike.transform.position = startPosition;
        bike.transform.rotation = Quaternion.Euler(startRotationEuler);

        // Apply config to controller
        controller.BikeSpeed = startSpeed;
        controller.currentVisualTiltingMode = visualTiltingMode;
        controller.currentRealismSupportLevel = GameControllerScript.RealismSupportLevel.OptimizedSupport;
    }
}
