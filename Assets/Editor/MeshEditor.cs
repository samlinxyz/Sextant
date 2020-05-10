using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MeshHelper))]
public class MeshEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        MeshHelper helper = (MeshHelper)target;

        if (GUILayout.Button("Redraw Mesh"))
        {
            helper.GenerateMesh();
        }
    }
}
