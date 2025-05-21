using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSmoothing : MonoBehaviour
{
    // Start is called before the first frame update

    public Transform target; // The target object the camera should follow
    public float smoothFactor = 0.5f; // The smoothing factor between 0 and 1
    private Vector3 velocity = Vector3.zero; // Variable to store the current velocity

    void LateUpdate()
    {
        // Smoothly move the camera towards the target position
        transform.position = Vector3.Lerp(transform.position, target.position, smoothFactor * Time.deltaTime);

        // Smoothly rotate the camera towards the target rotation
        transform.rotation = Quaternion.Lerp(transform.rotation, target.rotation, smoothFactor * Time.deltaTime);
    }
}
