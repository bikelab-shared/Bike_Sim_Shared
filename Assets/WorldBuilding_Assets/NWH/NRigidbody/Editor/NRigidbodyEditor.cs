#if UNITY_EDITOR
using UnityEditor;

namespace NWH.NPhysics
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(NRigidbody))]
    public class NRigidbodyEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("Sub-stepping settings and physical properties of the vehicle can be " +
                                    "changed under VehicleController > Settings.", MessageType.Info);
        }
    }
}
#endif