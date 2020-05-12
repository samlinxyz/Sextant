using UnityEngine;
using UnityEditor;
using System.IO;

[CustomEditor(typeof(StarFieldManager))]
public class StarFieldEditor : Editor
{
    public override void OnInspectorGUI()
    {
        StarFieldManager field = target as StarFieldManager;

        base.OnInspectorGUI();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Configure Stars Transform")) field.ConfigureStarsTransform();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Load stars"))
        {
            //  Record following changes to the undo stack.
            Undo.RegisterFullObjectHierarchyUndo(field.transform.parent.gameObject, "Load Stars");

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
                star.transform.position = star.truePosition = positionData;
                star.Temperature = float.Parse(dataEntry[3]);
                star.absoluteMagnitude = float.Parse(dataEntry[4]);

                //  Properly size and rotate star sprite.
                star.ConfigureTransform();
                star.UpdateTransform();

                //  Update Transform sets rotation equal to camera rotation, so rotation must be performed here.
                star.transform.rotation = Quaternion.LookRotation(positionData);
            }

            //  Reset the star references for all stages and lines.
            foreach (StarSublevel stage in field.transform.parent.GetComponentsInChildren<StarSublevel>())
            {
                stage.FindAssociatedStar();
            }
            foreach (Line line in field.transform.parent.GetComponentsInChildren<Line>())
            {
                line.FindAssociatedStars();
            }
        }

        if (GUILayout.Button("Delete stars (will take a while)"))
        {
            //  Record changes to the undo stack.
            Undo.RegisterFullObjectHierarchyUndo(field.gameObject, "Delete Stars");

            //  Delete the children.
            int childCount = field.transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                DestroyImmediate(field.transform.GetChild(childCount - i - 1).gameObject);
            }
        }

        GUILayout.EndHorizontal();
    }
}
