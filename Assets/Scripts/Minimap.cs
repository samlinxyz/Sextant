using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minimap : MonoBehaviour
{
    public MinimapMesh minimapMesh;
    public Transform minimapCamera;

    bool drawing;

    public Vector3 relativeDirection;
    Transform mainCameraTransform;
    public Transform level;
    GameManager game;

    public float minimapSize;
    public float maxPositionDelta;
    public float minimapCameraDistance;

    public List<Vector3> path;
    public List<bool> turns;


    // Start is called before the first frame update
    void Start()
    {
        drawing = false;

        mainCameraTransform = Camera.main.transform;
        game = GameManager.instance;

        relativeDirection = Vector3.up;
    }

    public void SetMinimap(bool on)
    {
        if (on)
        {
            //  Animates in the minimap.

            //  References the center of the constellation.
            level = game.level;

            //  Reset the minimap.
            ResetMap();

            //  Tells the minimap to start drawing its mesh
            drawing = true;
        }
        else
        {
            //  Animates out the minimap.
            Debug.Log("Start erasing");
            StartCoroutine(Erase());
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (drawing)
        {
            minimapCamera.rotation = mainCameraTransform.rotation;

            //  Let the head of the snake be exactly on a point on the close side of the map.
            Vector3 nextPoint = minimapSize * RelativeDirection();
            minimapCamera.position = minimapCameraDistance * nextPoint;

            path[path.Count - 1] = nextPoint;
            if ((path[path.Count - 2] - nextPoint).sqrMagnitude > maxPositionDelta * maxPositionDelta)
            {
                //  Add a point if we rotate far enough from the second last point.
                path.Add(nextPoint);
                turns.Add(false);
            }

            //  Draw the mesh based on the current path.
            minimapMesh.DrawPath(path.ToArray(), turns.ToArray());
        }
    }

    public void TurnPath()
    {
        Vector3 nextPoint = minimapSize * RelativeDirection();

        if (path.Count > 2)
        {
            path.Add(nextPoint);
            turns.Add(false);

            path.Add(nextPoint);
            turns.Add(path.Count > 2);

            path.Add(nextPoint);
            turns.Add(false);
        }
    }

    public void ResetMap()
    {
        path = new List<Vector3>();

        Vector3 nextPoint = minimapSize * RelativeDirection();

        path.Add(nextPoint);
        path.Add(nextPoint);
        turns.Add(false);
        turns.Add(false);
    }

    Vector3 RelativeDirection()
    {
        //  Calculate the relative position of the main camera with respect to the constellation center.
        return (mainCameraTransform.position - level.position).normalized;
    }

    IEnumerator Erase()
    {
        while (path.Count > 21) //  it has to be 2 more than the amount we're allowed to remove because of the update loop
        {
            path.RemoveRange(0, 20);
            yield return null;
        }
        ResetMap();
        drawing = false;
    }
}
