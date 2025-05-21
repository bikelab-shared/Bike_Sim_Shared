using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CornerTurningDeceleration : MonoBehaviour
{

    public GameObject CornerTurningPoints;
    public bool CornertoTurn;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Equals("corner1"))
        {
            CornertoTurn = true;
        }
        else
        {
            CornertoTurn = false;
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
