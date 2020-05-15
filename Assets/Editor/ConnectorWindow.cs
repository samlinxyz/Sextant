using UnityEngine;
using UnityEditor;

public class ConnectorWindow : EditorWindow
{

    [MenuItem("Window/Connector")]
    public static void ShowWindow()
    {
        GetWindow<ConnectorWindow>("Connector");
    }

    public Transform constellation;
    public Transform starSublevelPrefab;
    public Transform linePrefab;
    
    public float zRotation;
    bool updating;

    void OnGUI()
    {
        SerializedObject obj = new SerializedObject(this);

        EditorGUILayout.PropertyField(obj.FindProperty("constellation"));
        EditorGUILayout.PropertyField(obj.FindProperty("starSublevelPrefab"));
        EditorGUILayout.PropertyField(obj.FindProperty("linePrefab"));

        EditorGUILayout.Space();

        if (constellation != null)
        {
            EditorGUILayout.BeginVertical("Box");
            EditConstellationLines();
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical("Box");
            EditConstellationSublevels();
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical("Box");
            EditConstellationSkyLines();
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            var view = SceneView.lastActiveSceneView;
            if (view != null)
            {
                EditorGUILayout.BeginVertical("Box");
                ConfigureView(view);
                EditorGUILayout.EndVertical();
            }
            else EditorGUILayout.HelpBox("click on the scene window first!", MessageType.Warning);
        }
        else EditorGUILayout.HelpBox("Select a constellation to edit.", MessageType.Warning);

        obj.ApplyModifiedProperties();
    }

    void EditConstellationLines()
    {
        GUILayout.Label("Edit constellation lines", EditorStyles.boldLabel);

        ConstellationLines lines = constellation.GetComponent<ConstellationLines>();

        GUILayout.Label("There are currently " + lines.GetLineCount() + " constellation lines");



        if (GUILayout.Button("Add Line"))
        {
            if (Selection.gameObjects.Length == 2)
            {
                Vector3 start = Selection.gameObjects[0].transform.position;
                Vector3 end = Selection.gameObjects[1].transform.position;

                lines.AddLine(start, end);
            }
            else Debug.Log("Connections are made only between two selected objects. There are currently " + Selection.gameObjects.Length + " objects selected.");

            Selection.activeObject = constellation;
        }
        if (Selection.gameObjects.Length != 2)
            EditorGUILayout.HelpBox("Connections are made only between two selected objects. There are currently " + Selection.gameObjects.Length + " objects selected.", MessageType.Warning);
    }

    void EditConstellationSublevels()
    {
        GUILayout.Label("Add star sublevels", EditorStyles.boldLabel);

        ConstellationLines lines = constellation.GetComponent<ConstellationLines>();

        if (GUILayout.Button("Add sublevel"))
        {
            if (Selection.gameObjects.Length == 1)
            {
                Debug.Log("adding sublevel");
                GameObject lol = PrefabUtility.InstantiatePrefab(starSublevelPrefab.gameObject as GameObject) as GameObject;
                Transform newSublevel = lol.transform;

                // set the parent as the "stages" child of the constellation!
                newSublevel.SetParent(constellation.GetChild(1));

                StarSublevel stage = newSublevel.GetComponent<StarSublevel>();

                // set star to the one currently selected
                SerializedObject so = new SerializedObject(stage);
                so.FindProperty("associatedStar").objectReferenceValue = Selection.gameObjects[0].GetComponent<Star>();
                so.ApplyModifiedProperties();
                // normalize stage position
                Undo.RecordObject(stage.transform, "normalize");
                stage.transform.position = 200f * stage.AssociatedStar.transform.position.normalized;

                Selection.activeTransform = newSublevel;
            }
            else Debug.Log("Select one star.");
        }
        if (Selection.gameObjects.Length != 1)
            EditorGUILayout.HelpBox("Select one star. There are currently " + Selection.gameObjects.Length + " objects selected.", MessageType.Warning);
    }
    void EditConstellationSkyLines()
    {
        GUILayout.Label("Add sky lines", EditorStyles.boldLabel);

        ConstellationLines lines = constellation.GetComponent<ConstellationLines>();

        if (GUILayout.Button("Add line"))
        {
            if (Selection.gameObjects.Length == 2)
            {
                Debug.Log("adding sublevel");
                GameObject lol = PrefabUtility.InstantiatePrefab(linePrefab.gameObject as GameObject) as GameObject;
                Transform newLine = lol.transform;
                newLine.SetParent(constellation.GetChild(0));

                Line line = newLine.GetComponent<Line>();


                //  This next block of code automatically finds if each star has an associated stage. 
                StarSublevel stageAssociatedWithStar1 = null;
                float closestAngle = 180f;
                foreach (StarSublevel stage in lines.GetComponentsInChildren<StarSublevel>())
                {
                    float angle = Vector3.Angle(Selection.gameObjects[0].transform.position, stage.transform.position);
                    if (closestAngle > angle)
                    {
                        stageAssociatedWithStar1 = stage;
                        closestAngle = angle;
                    }
                }
                if (closestAngle > 0.1f)
                {
                    Debug.LogWarning("associated star stage is far from the star. it is assumed that there is no stage associated with the first star. angle: " + closestAngle);
                    stageAssociatedWithStar1 = null;
                }
                StarSublevel stageAssociatedWithStar2 = null;
                closestAngle = 180f;
                foreach (StarSublevel stage in lines.GetComponentsInChildren<StarSublevel>())
                {
                    float angle = Vector3.Angle(Selection.gameObjects[1].transform.position, stage.transform.position);
                    if (closestAngle > angle)
                    {
                        stageAssociatedWithStar2 = stage;
                        closestAngle = angle;
                    }
                }
                if (closestAngle > 0.1f)
                {
                    Debug.LogWarning("associated star stage is far from the star. it is assumed that there is no stage associated with the second star. angle: " + closestAngle);
                    stageAssociatedWithStar2 = null;
                }



                bool success = line.SetStarReferences(Selection.gameObjects, new StarSublevel[] { stageAssociatedWithStar1, stageAssociatedWithStar2 });
                if (!success)
                {
                    DestroyImmediate(lol);
                    Debug.LogError("Line not created");
                    return;
                }

                line.UpdatePosition();
                line.EditorUpdateColor();










                // whatever this is here, you need to do the serialized object thing.











                //  Project the stars positions onto a sphere of radius 100000f, behind any (reasonable) star.
                Vector3 startPosition = Selection.gameObjects[0].transform.position;
                Vector3 endPosition = Selection.gameObjects[1].transform.position;
                //startPosition = 100f * startPosition.normalized;
                //endPosition = 100f * endPosition.normalized;

                newLine.position = Vector3.Slerp(startPosition, endPosition, 0.5f);

                //  Set the start/end of the line in local coordinates, as the LineRenderer uses local positions.
                startPosition = newLine.InverseTransformPoint(startPosition);
                endPosition = newLine.InverseTransformPoint(endPosition);
                //newLine.GetComponent<LineRenderer>().SetPositions(new Vector3[] { startPosition, endPosition });
                
                Selection.activeTransform = newLine;
            }
            else Debug.Log("Connections are made only between two selected objects.");
        }
        if (Selection.gameObjects.Length != 2)
            EditorGUILayout.HelpBox("Connections are made only between two selected objects. There are currently " + Selection.gameObjects.Length + " objects selected.", MessageType.Warning);
    }

    void ConfigureView(SceneView view)
    {
        GUILayout.Label("Edit scene view", EditorStyles.boldLabel);

        //  Look at the selected transform and set the pivot to the origin. The user will have to use the scroll wheel to adjust their distance from the pivot.
        if (GUILayout.Button("Frame constellation"))
        {
            view.pivot = Vector3.zero;

            ConstellationLines.Frame frame = constellation.GetComponent<ConstellationLines>().getFrame;
            Vector3 rotationEuler = Quaternion.LookRotation(constellation.position).eulerAngles;
            rotationEuler.z = frame.ZRotation;

            view.rotation = Quaternion.Euler(rotationEuler);
            view.cameraSettings.fieldOfView = frame.FieldOfView;
        }

        //  Look at the selected transform and set the pivot to the origin. The user will have to use the scroll wheel to adjust their distance from the pivot.
        if (GUILayout.Button("Make constellation center equal to 200 in the direction of scene camera"))
        {
            constellation.position = 200f * (view.camera.transform.rotation * Vector3.forward);
            constellation.rotation = Quaternion.LookRotation(constellation.position, view.camera.transform.up);
        }

        //  Allows you to use the float field as a scroll button to adjust the orientation of the constellation of interest.
        float newZRotation = EditorGUILayout.FloatField("Rotate", zRotation);
        if (zRotation != newZRotation)
            view.rotation = Quaternion.Euler(
                view.rotation.eulerAngles.x, 
                view.rotation.eulerAngles.y, 
                view.rotation.eulerAngles.z + newZRotation - zRotation
                );

        if (GUILayout.Button("Reset scene view camera"))
        {
            view.pivot = Vector3.zero;
            view.rotation = Quaternion.identity;
            view.cameraSettings.fieldOfView = 60f;
        }
    }
}