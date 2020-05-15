using UnityEngine;
using UnityEditor;
using Supyrb;

[CustomEditor(typeof(ConstellationLines))]
public class ConstellationLinesEditor : Editor
{

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        ConstellationLines lines = (ConstellationLines)target;

        // The camera FOV and orientation that the camera zooms to when it looks at the constellation.
        // zoom and rotation characteristic view frame 
        if (GUILayout.Button("Set frame to scene"))
        {
            SetFrameFor(lines);
        }

        /*
        // Soon to be deprecated 
        float newSquishFactor = EditorGUILayout.Slider(lines.SquishFactor, 1f, 50f);
        if (newSquishFactor != lines.SquishFactor)
        {
            // this shit change isn't preserved ! use serialization! I hate this!
            //lines.squishFactor = newSquishFactor;
            lines.field.SquishStarsAround(lines.transform, true);
        }
        */

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Transform starfield (wip)")) TransformStarsAround(lines.transform);
        if (GUILayout.Button("Reset starfield (wip)")) ResetStarPositions();
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Normalize Stage Positions")) NormalizeStagePositions(lines);
    }

    private void TransformStarsAround(Transform constellation)
    {
        Undo.RegisterFullObjectHierarchyUndo(GameObject.FindGameObjectWithTag("StarField"), $"Transform around {constellation.name}.");
        
        Transform[] stars = StarField.StarTransformArray();
        
        // modify the positions
    }

    private void ResetStarPositions()
    {
        Undo.RegisterFullObjectHierarchyUndo(GameObject.FindGameObjectWithTag("StarField"), "Reset Star Positions");

        Star[] stars = StarField.StarArray();

        foreach (Star star in stars)
        {
            star.transform.position = star.TruePosition;
        }
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

    private void SetFrameFor(ConstellationLines lines)
    {
        SceneView view = SceneView.lastActiveSceneView;
        if (view != null)
        {
            float fov = view.cameraSettings.fieldOfView;
            float zRotation = view.camera.transform.rotation.eulerAngles.z;
            SerializedObject so = new SerializedObject(lines);
            so.FindProperty("frame").SetValue(new ConstellationLines.Frame(fov, zRotation));
            so.ApplyModifiedProperties();
        }
        else
        {
            Debug.Log("Last active scene view is null.");
            return;
        }
    }

    public void NormalizeStagePositions(ConstellationLines level)
    {
        //  Sets the position of each stage to 200 m in the direction of the associated star's position from the origin.
        foreach (StarSublevel stage in level.GetComponentsInChildren<StarSublevel>())
        {
            SerializedObject so = new SerializedObject(stage.transform);
            so.FindProperty("position").vector3Value = 200f * stage.AssociatedStar.transform.position.normalized;
            so.ApplyModifiedProperties();
        }
    }
}
