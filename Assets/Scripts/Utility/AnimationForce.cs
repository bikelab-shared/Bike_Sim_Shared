using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationForce : MonoBehaviour
{

    public Transform parentAnimation;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = parentAnimation.position;
        transform.rotation = parentAnimation.rotation;
    }
}
