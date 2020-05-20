using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using Supyrb;

[CustomEditor(typeof(StarFieldManager))]
public class StarFieldEditor : Editor
{
    public override void OnInspectorGUI()
    {
        StarFieldManager field = target as StarFieldManager;

        base.OnInspectorGUI();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Load stars"))
        {
            //  Record following changes to the undo stack.
            Undo.RegisterFullObjectHierarchyUndo(field.gameObject, "Load Stars");

            LoadStars(field);

            SetStarReferences();
        }

        if (GUILayout.Button("Delete all stars"))
        {
            //  Record changes to the undo stack.
            Undo.RegisterFullObjectHierarchyUndo(field.gameObject, "Delete All Stars");

            DeleteAllStars();
        }

        GUILayout.EndHorizontal();

        if (GUILayout.Button("Set star references"))
        {
            SetStarReferences();
        }
        if (GUILayout.Button("Update stage difficulties"))
        {
            UpdateStageDifficulties();
        }
    }

    private void LoadStars(StarFieldManager field)
    {
        //  Loads data about position, color, and absolute magnitude of all stars from a csv.
        string starInfoArray = File.ReadAllText(Application.dataPath + "/Resources/Stars - GameData.csv");
        starInfoArray.Replace("\r", null);

        foreach (string starInfo in starInfoArray.Split('\n'))
        {
            string[] dataEntry = starInfo.Split(',');

            //  Instantiate star
            GameObject starObject = Instantiate(field.starPrefab, field.transform);
            starObject.tag = "Star";
            Star star = starObject.GetComponent<Star>();

            //  Set references
            star.game = field.game;
            star.cam = field.mainCamera.transform;
            star.field = field;

            //  Set position and parameters
            Vector3 positionData = new Vector3(float.Parse(dataEntry[0]), float.Parse(dataEntry[1]), float.Parse(dataEntry[2]));
            star.TruePosition = positionData;
            star.Temperature = float.Parse(dataEntry[3]);
            star.absoluteMagnitude = float.Parse(dataEntry[4]);

            //  Properly size and rotate star sprite.
            star.ConfigureTransform();
            star.UpdateTransform();

            //  Update Transform sets rotation equal to camera rotation, so rotation must be performed here.
            star.transform.rotation = Quaternion.LookRotation(positionData);
        }
    }

    //  Reset the star references for all stages and lines.
    private void SetStarReferences()
    {
        StarSublevel[] allStages = GameObject.FindGameObjectsWithTag("Stage").Select(go => go.GetComponent<StarSublevel>()).ToArray();
        foreach (StarSublevel stage in allStages)
        {
            SerializedObject so = new SerializedObject(stage);
            so.FindProperty("associatedStar").objectReferenceValue = stage.FindAssociatedStar();
            so.ApplyModifiedProperties();
        }

        Line[] allLines = GameObject.FindGameObjectsWithTag("Line").Select(go => go.GetComponent<Line>()).ToArray();
        foreach (Line line in allLines)
        {
            SerializedObject so = new SerializedObject(line);
            Transform[] starsFound = line.FindAssociatedStars();
            so.FindProperty("starTransforms").arraySize = 2;
            so.FindProperty("starTransforms").GetArrayElementAtIndex(0).objectReferenceValue = starsFound[0];
            so.FindProperty("starTransforms").GetArrayElementAtIndex(1).objectReferenceValue = starsFound[1];
            so.ApplyModifiedProperties();
        }
    }

    private void UpdateStageDifficulties()
    {
        StarSublevel[] allStages = GameObject.FindGameObjectsWithTag("Stage").Select(go => go.GetComponent<StarSublevel>()).ToArray();
        foreach (StarSublevel stage in allStages)
        {
            SerializedObject so = new SerializedObject(stage);
            so.FindProperty("difficulty").floatValue = Settings.I.TemperatureToDifficulty(stage.AssociatedStar.Temperature);
            so.ApplyModifiedProperties();
        }
    }

    private void DeleteAllStars()
    {
        GameObject[] allStars = GameObject.FindGameObjectsWithTag("Star");
        int starCount = allStars.Length;
        for (int i = 0; i < starCount; i++)
        {
            DestroyImmediate(allStars[starCount - 1 - i]);
        }
    }
}