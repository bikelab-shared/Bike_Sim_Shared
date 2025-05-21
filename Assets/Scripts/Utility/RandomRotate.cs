using static UnityEngine.Random;
using UnityEditor;
using UnityEngine;

public class RandomRotate : MonoBehaviour
{
    [MenuItem("Custom/Rotate %h")]
    public static void Rotate()
    {
        var transforms = Selection.transforms;

        foreach(var transform in transforms)
        {
            var x = Random.Range(-90.0f,90.0f);
            transform.Rotate(0,x,0);

        }

        Debug.Log("Transforms rotated " + transforms.Length);
    }

}