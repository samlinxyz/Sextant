using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CircularFade)), CanEditMultipleObjects]
public class CircularFadeEditor : Editor
{
    protected virtual void OnSceneGUI()
    {
        CircularFade example = (CircularFade)target;

        EditorGUI.BeginChangeCheck();
        Vector3 newTargetPosition = Handles.PositionHandle(example.MaskCenterScreen, Quaternion.identity);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(example, "Change Look At Target Position");
            example.MaskCenterScreen = newTargetPosition;
        }
    }
}
