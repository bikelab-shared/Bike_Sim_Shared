using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Uduino;

public class DT_Tester : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        UduinoManager.Instance.pinMode(12, PinMode.Output);
        UduinoManager.Instance.pinMode(6, PinMode.Output);
       // UduinoManager.Instance.pinMode(11, PinMode.Input);
        StartCoroutine(BlinkLoop());
    }

    // Update is called once per frame
    IEnumerator BlinkLoop()
    {
        while (true)
        {
            UduinoManager.Instance.digitalWrite(12, State.HIGH);
            yield return new WaitForSeconds(0.5f);
            //UduinoManager.Instance.digitalWrite(12, State.LOW);
            //yield return new WaitForSeconds(0.5f);
            UduinoManager.Instance.digitalWrite(6, State.HIGH);
            yield return new WaitForSeconds(0.5f);

          //  int i = UduinoManager.Instance.digitalRead(11);
          //  Debug.Log("i: " + i);

        }
        
    }
}
