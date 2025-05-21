using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteeringControllerScript : MonoBehaviour
{
    public GameObject leftController;

    private Transform handlebar;
    private Quaternion initalHandlebarRotation, initialControllerRotation;
    private bool set = false;
    private float steeringAngle = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        handlebar = this.transform.Find("WheelHandleBar");
    }

    // Update is called once per frame
    void Update()
    {
        if (!set)
        {
            initalHandlebarRotation = handlebar.transform.rotation;
            initialControllerRotation = leftController.transform.rotation;
            set = true;
        }
        //handlebar.transform.rotation = initialControllerRotation * rightController.transform.rotation * initalHandlebarRotation;

        steeringAngle = (initialControllerRotation * leftController.transform.rotation * initalHandlebarRotation).eulerAngles.y;
        handlebar.transform.rotation = Quaternion.Euler(0.0f, steeringAngle, 0.0f);
        
    }
}
