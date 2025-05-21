using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MeassureDistance : MonoBehaviour
{
    [MenuItem("Custom/Meassure %m")]
    public static void meassure()
    {
        var transforms = Selection.transforms;
        if(transforms.Length != 2)
        {
            Debug.Log("Please select exactly 2 objects to meassure distance between.");
            return;
        }
        var first = transforms[0];
        var second = transforms[1];

        var dist = second.position - first.position;
        Debug.Log("FIRST: "+ toString(first.position));
        Debug.Log("SECOND: " + toString(second.position));
        Debug.Log("Distance: "+toString(dist)+" total: " + dist.magnitude);
    }

    public static string toString(Vector3 v)
    {
        return "(" + v.x + " | " + v.y + " | " + v.z +")";
    }
}


public class Face : MonoBehaviour
{
    [MenuItem("Custom/Do Stuff %e")]
    public static void face()
    {
        var transforms = Selection.transforms;
        if (transforms.Length != 1)
        {
            Debug.Log("Please select exactly 1 object.");
            return;
        }
        var first = transforms[0];
        var go = first.gameObject;
        var pc = go.GetComponent<PathCreation.PathCreator>();
        var bp = pc.bezierPath;
        var points = bp.GetPointsInSegment(0);

        Debug.Log(points.Length);
        for(int i = 0; i < points.Length; ++i)
        {
            var p = points[i];
            var str = MeassureDistance.toString(p);
        }
        Debug.Log("DONE");
    }
}

