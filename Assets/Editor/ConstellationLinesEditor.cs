using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ConstellationLines))]
public class ConstellationLinesEditor : Editor
{

    public override void OnInspectorGUI()
    {
        ConstellationLines lines = (ConstellationLines)target;
        base.OnInspectorGUI();

        if (GUILayout.Button("Set Collider Size")) lines.EditorAdjustColliderSize();

        if (GUILayout.Button("Set FOV to scene"))
        {
            SceneView view = SceneView.lastActiveSceneView;
            if (view != null)
                lines.SetFOV(SceneView.lastActiveSceneView.cameraSettings.fieldOfView);
            else
                Debug.Log("Last active scene view is null.");
        }
        if (GUILayout.Button("Set camera z rotation to scene"))
        {
            SceneView view = SceneView.lastActiveSceneView;
            if (view != null)
                lines.SetZRotation(view.camera.transform.rotation.eulerAngles.z);
            else
                Debug.Log("Last active scene view is null.");
        }

        float newSquishFactor = EditorGUILayout.Slider(lines.squishFactor, 1f, 50f);
        if (newSquishFactor != lines.squishFactor)
        {
            lines.squishFactor = newSquishFactor;
            lines.field.SquishStarsAround(lines.transform, true);
        }
        if (GUILayout.Button("Test squish factor")) lines.field.SquishStarsAround(lines.transform, true);
        //if (GUILayout.Button("Save squish factor")) lines.SetSquishFactor(newSquishFactor);



        if (GUILayout.Button("Normalize Stage Positions")) lines.EditorNormalizeStagePositions();
        if (GUILayout.Button("Set Star References")) lines.SetStarReferences();

        
    }

    protected virtual void OnSceneGUI()
    {
        ConstellationLines lines = (ConstellationLines)target;

        for (int i = 0; i < lines.vertices.Count; i += 2)
        {
            Vector3 midpoint = Vector3.Slerp(lines.vertices[i], lines.vertices[i + 1], 0.5f);

            Handles.color = Color.red;
            if (lines.showDelete)
                if (Handles.Button(midpoint, Quaternion.identity, HandleUtility.GetHandleSize(midpoint) / 10f, HandleUtility.GetHandleSize(midpoint) / 10f, Handles.CircleHandleCap))
                    lines.vertices.RemoveRange(i, 2);
        }
    }

}
