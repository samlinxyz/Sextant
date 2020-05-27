using UnityEngine;
using UnityEditor;
using Supyrb;
using System.Linq;

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

        SerializedObject so = new SerializedObject(lines);
        SerializedProperty sp = so.FindProperty("squishParameters");
        
        if (GUILayout.Button("click me to display what properties there are within the custom class frame!"))
        {
            if (sp.isArray)
            {
                Debug.Log("squishparams is an array");
            }
            if (sp.isExpanded)
            {
                Debug.Log("squishparams is expanded");
            }
            Debug.Log($"this property's full path is: {sp.propertyPath}");
            Debug.Log("These are the names of the property's children:");
            foreach (SerializedProperty spchild in sp)
            {
                Debug.Log(spchild.displayName + $" - full property path: {spchild.propertyPath}");
            }
        }

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Transform starfield")) TransformStarsAround(lines);
        if (GUILayout.Button("Reset starfield")) ResetStarPositions();
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Normalize stage positions")) NormalizeStagePositions(lines);

        if (GUILayout.Button("Initialize star positions of lines and stages")) InitializeStarPositions(lines);
        if (GUILayout.Button("Set Stage Diffraction Size and Color")) SetStageDiffractionSizeAndColor(lines);

        EditorGUILayout.BeginHorizontal();
        float radius = GetStageClickingRadius(lines);
        radius = EditorGUILayout.Slider("Stage Click Radius", radius, 1f, 100f);
        if (GetStageClickingRadius(lines) != radius)
        {
            SetStageClickingRadius(lines, radius);
        } 
        EditorGUILayout.EndHorizontal();
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

    private void SetFrameFor(ConstellationLines lines)
    {
        SceneView view = SceneView.lastActiveSceneView;
        if (view != null)
        {
            float fieldOfView = view.camera.fieldOfView;
            float zRotation = view.camera.transform.rotation.eulerAngles.z;

            SerializedObject so = new SerializedObject(lines);
            so.FindProperty("frame").FindPropertyRelative("fieldOfView").floatValue = fieldOfView;
            so.FindProperty("frame").FindPropertyRelative("zRotation").floatValue = zRotation;
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
            stage.transform.position = 100f * stage.AssociatedStar.TruePosition.normalized;
        }
    }
    public void InitializeStarPositions(ConstellationLines level)
    {
        Undo.RegisterFullObjectHierarchyUndo(level.gameObject, "Initialize Star Positions");

        foreach (Line line in level.Lines)
        {
            SerializedObject so = new SerializedObject(line);
            Transform[] starTransforms = line.StarTransforms;
            so.FindProperty("starPositions").GetArrayElementAtIndex(0).vector3Value = starTransforms[0].position;
            so.FindProperty("starPositions").GetArrayElementAtIndex(1).vector3Value = starTransforms[1].position;
            so.ApplyModifiedProperties();
        }

        foreach (StarSublevel stage in level.Stages)
        {
            SerializedObject so = new SerializedObject(stage);
            Star star = stage.AssociatedStar;
            so.FindProperty("starPosition").vector3Value = star.transform.position;
            so.ApplyModifiedProperties();
        }
    }
    public void SetStageDiffractionSizeAndColor(ConstellationLines level)
    {
        foreach (StarSublevel stage in level.Stages)
        {
            Undo.RegisterFullObjectHierarchyUndo(stage.gameObject, "Set Stage Diffraction Size and Color");
            stage.EditorSetDiffractionColor();
        }
    }

    public void SetStageClickingRadius(ConstellationLines level, float radius)
    {
        Undo.RegisterFullObjectHierarchyUndo(level.gameObject, "Set Stage Clicking Radius");
        foreach (SphereCollider sphereCollider in level.Stages.Select(stage => stage.GetComponent<SphereCollider>()).ToArray())
        {
            sphereCollider.radius = radius;
        }
    }

    private float GetStageClickingRadius(ConstellationLines level)
    {
        if (level.Stages.Length > 0)
        {
            return level.Stages[0].GetComponentInChildren<SphereCollider>().radius;
        }
        else
        {
            return 5f;  // Arbitrary number.
        }
    }
}
