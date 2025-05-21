using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetPosition : MonoBehaviour
{

    [SerializeField] Transform resetTransform;
    [SerializeField] GameObject player;
    [SerializeField] Camera playerHead;

    public void Update()
    {
        if (Input.GetKeyDown("v"))
        {
            ResetViewPosition();
        }
        if(OVRInput.Get(OVRInput.Button.Two))
        {
            ResetViewPosition();
        }
    }

    [ContextMenu("Reset Camera Pos")]
    public void ResetViewPosition() {

        var rotationAngleY = resetTransform.rotation.eulerAngles.y - playerHead.transform.rotation.eulerAngles.y;
        player.transform.Rotate(0, rotationAngleY, 0);

        var distanceDiff = resetTransform.position - playerHead.transform.position;
        player.transform.position += distanceDiff;

    }
}
