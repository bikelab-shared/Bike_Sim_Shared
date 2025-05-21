using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//this script detect the collision between ProceedingBike and the Corner collider on the road
public class CornerDeceleration : MonoBehaviour
{

    public bool isReachCorner;
   

    void OnTriggerEnter(Collider other)
    {
        if(other.tag.Equals("CornerCollider"))
        {
            isReachCorner = true;
        }

    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag.Equals("CornerCollider"))
        {
            isReachCorner = false;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
