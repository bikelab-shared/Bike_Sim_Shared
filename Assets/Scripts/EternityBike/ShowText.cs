using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowText : MonoBehaviour
{
    public GameObject BikeSimulator;
    public GameObject WheelHandleBar;

    public string textValue;
    public Text textElement;


    // Start is called before the first frame update
    void Start()
    {
        
    }
    
    // Update is called once per frame
    void Update()
    {
        if (BikeSimulator != null)
        {
            GameControllerScript p = BikeSimulator.GetComponent<GameControllerScript>();
            HandleBarCollider whb = WheelHandleBar.GetComponent<HandleBarCollider>();

            // Debug.Log("Jup " + p.BikeSpeed);

            textValue = p.BikeSpeed.ToString() + "km/h " + whb.ballCounter.ToString() + " Objects \n" + whb.pastTime.ToString("F2") + " Sec " + whb.lapTime.ToString("F2") + " Laptime\n" + "Distance: " + whb.ballDistance.ToString("F3") + "\nDistance Min: " + whb.ballDistanceMin.ToString("F3");
            textElement.text = textValue;

        }


        }
    

}
