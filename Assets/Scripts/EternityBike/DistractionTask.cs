using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class DistractionTask : MonoBehaviour
{
    public TextMeshPro textMeshPro; 
    public float minDisplayTime = 1.0f; // Minimum time to display a number (in seconds)
    public float maxDisplayTime = 3.0f; // Maximum time to display a number (in seconds)

    private int[] numbersToDisplay = { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
    private int currentIndex = 0;

    private float displayTimer;
    private int currentNumber;

    public int number7Count = 0; 

    private Camera oculusCamera; 
    public float numberInViewTime = 0f; // Time the current number is in view
    public float numberOutOfViewTime = 0f; // Time the current number is out of view
    private bool isNumberInView = false;

    private void Start()
    {
        ShuffleNumbers();
        ShowRandomNumber();
        oculusCamera = Camera.main; 
    }

    private void Update()
    {
        displayTimer -= Time.deltaTime;

        if (displayTimer <= 0)
        {
            ShowRandomNumber();
        }

        // Check if the current number is in the FOV of the camera
        Vector3 viewportPoint = oculusCamera.WorldToViewportPoint(transform.position);

        if (viewportPoint.x >= 0 && viewportPoint.x <= 1 && viewportPoint.y >= 0 && viewportPoint.y <= 1)
        {
            // Number is in view
            if (!isNumberInView)
            {
                isNumberInView = true;
                //numberInViewTime = 0f;
            }
            else
            {
                numberInViewTime += Time.deltaTime;
            }
        }
        else
        {
            // Number is not in view
            isNumberInView = false;
            numberOutOfViewTime += Time.deltaTime;
        }

        //Debug.Log("Number in View: " + numberInViewTime);
        //Debug.Log("Number not in View " + numberOutOfViewTime);
    }

    private void ShuffleNumbers()
    {
        for (int i = 0; i < numbersToDisplay.Length; i++)
        {
            int temp = numbersToDisplay[i];
            int randomIndex = Random.Range(i, numbersToDisplay.Length);
            numbersToDisplay[i] = numbersToDisplay[randomIndex];
            numbersToDisplay[randomIndex] = temp;
        }
    }

    private void ShowRandomNumber()
    {
        currentNumber = numbersToDisplay[currentIndex];
        textMeshPro.text = currentNumber.ToString();

        // Check if the displayed number is 7
        if (currentNumber == 7)
        {
            number7Count++;
            Debug.Log("Number 7 Count: " + number7Count);
        }

        // Update the timer for the next display
        displayTimer = Random.Range(minDisplayTime, maxDisplayTime);

        // Move to the next number (looping)
        currentIndex = (currentIndex + 1) % numbersToDisplay.Length;
    }
}