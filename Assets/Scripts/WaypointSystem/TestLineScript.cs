using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Uduino;

public class TestLineScript : MonoBehaviour
{
    // Start is called before the first frame update

    public int CollisionHappend;

    void Start()
    {
        CollisionHappend = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Min Collider");
        CollisionHappend = 1;
       // SendCommand();
    }

    private void SendCommand() {
       UduinoManager.Instance.sendCommand("min");

    }
}
