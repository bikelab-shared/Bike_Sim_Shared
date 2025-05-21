using System.Collections;
using UnityEditor;
using UnityEngine;

public class SnapToGround : MonoBehaviour
{
    [MenuItem("Custom/Snap To Ground %g")]
    public static void Ground()
    {
        int i = 0;
        var transforms = Selection.transforms;

        foreach(var transform in transforms)
        {
            var hits = Physics.RaycastAll(transform.position + Vector3.up, Vector3.down, 100f);
            foreach(var hit in hits)
            {
                if (hit.collider.gameObject == transform.gameObject)
                    continue;

                transform.position = hit.point;
                ++i;
                break;
            }
        }

        Debug.Log("Selected '"+transforms.Length+"' Objects, Raycast hit + repositionings '"+i+"'");
    }

}