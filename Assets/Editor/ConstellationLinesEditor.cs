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
        if (GUILayout.Button("Set frame to scene (does not work because SetValue)"))
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
        if (GUILayout.Button("Transform starfield")) TransformStarsAround(lines);
        if (GUILayout.Button("Reset starfield")) ResetStarPositions();
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Normalize stage positions")) NormalizeStagePositions(lines);
    }

    private void TransformStarsAround(ConstellationLines constellation)
    {
        Undo.RegisterFullObjectHierarchyUndo(GameObject.FindGameObjectWithTag("StarField"), $"Transform Around {constellation.name}");
        
        Star[] stars = StarField.StarArray();

        float angle = Mathf.Asin(constellation.LevelRadius / constellation.Distance);
        foreach (Star star in stars)
        {
            //  If the star is within the constellation, apply the transform.
            if (Vector3.Angle(star.TruePosition, constellation.transform.localPosition) <= Mathf.Rad2Deg * angle)
            {
                star.ConfigureSquishedTransform(constellation.SquishParameters);
                star.UpdateTransformExaggerated();
                star.transform.rotation = Quaternion.LookRotation(star.transform.position);
            }
        }
    }

    private void ResetStarPositions()
    {
        Undo.RegisterFullObjectHierarchyUndo(GameObject.FindGameObjectWithTag("StarField"), "Reset Star Positions");

        Star[] stars = StarField.StarArray();

        foreach (Star star in stars)
        {
            star.transform.localPosition = star.TruePosition;
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
        Undo.RegisterFullObjectHierarchyUndo(level.gameObject, "Normalize Stage Positions");
        foreach (StarSublevel stage in level.Stages)
        {
            stage.transform.position = 200f * stage.AssociatedStar.TruePosition.normalized;
        }
    }
}
