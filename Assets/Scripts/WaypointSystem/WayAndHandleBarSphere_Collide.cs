using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WayAndHandleBarSphere_Collide : MonoBehaviour
{
    // Start is called before the first frame update

    public bool bothSphere_Collide = false;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("BothSpheres OnTriggerEnter: " +other.name);

        if (other.name.Equals("PathSphere")) {
            Debug.Log("BothSpheres TRUE");
            bothSphere_Collide = true;
        }    
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.name.Equals("PathSphere"))
        {
            Debug.Log("BothSpheres FALSE");
            bothSphere_Collide = false;
        }
    }
}
