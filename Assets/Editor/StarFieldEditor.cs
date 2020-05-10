using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(StarFieldManager))]
public class StarFieldEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        StarFieldManager field = (StarFieldManager)target;

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Configure Stars Transform")) field.ConfigureStarsTransform();
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Stars Face Origin")) field.StarsFaceOrigin();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Load stars")) field.LoadStars();
        if (GUILayout.Button("Delete stars")) field.DeleteAllStars();
        GUILayout.EndHorizontal();
    }
}
