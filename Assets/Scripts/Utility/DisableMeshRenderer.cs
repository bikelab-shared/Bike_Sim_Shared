using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableMeshRenderer : MonoBehaviour
{
  
    void Start()
    {
        // Call the function to disable MeshRenderers for all children
        DisableAllChildMeshRenderers(transform);
    }

    void DisableAllChildMeshRenderers(Transform parent)
    {
        // Get all child objects (including nested children)
        Transform[] children = parent.GetComponentsInChildren<Transform>(true);

        // Loop through each child
        foreach (Transform child in children)
        {
            // Check if the child has a MeshRenderer component
            MeshRenderer renderer = child.GetComponent<MeshRenderer>();

            // If a MeshRenderer component is found, disable it
            if (renderer != null)
            {
                renderer.enabled = false;
            }
        }
    }
}

