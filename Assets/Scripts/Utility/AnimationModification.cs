using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationModification : MonoBehaviour
{

    public float targetXRotationAngle = -90;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 currentRotation = transform.rotation.eulerAngles;
        currentRotation.x = targetXRotationAngle;
        transform.rotation = Quaternion.Euler(currentRotation);
    }
}
