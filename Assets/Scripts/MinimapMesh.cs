using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(MeshFilter))]
public class MinimapMesh : MonoBehaviour
{
    Mesh mesh;

    public float lineWidth;

    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
    }

    public void DrawPath(Vector3[] path, bool[] turns)
    {
        Vector3[] verts = new Vector3[path.Length * 4];
        //  Vector2[] uvs = new Vector2[verts.Length];
        Vector3[] normals = new Vector3[verts.Length];

        int numTris = 2 * (path.Length - 1);
        int[] topTriangles = new int[numTris * 3];
        int[] bottomTriangles = new int[numTris * 3];

        int vertIndex = 0;
        int triIndex = 0;

        // Vertices for the path are layed out:
        // 4  5
        // 0  1
        // and so on... So the triangle map 0,4,1 for example, defines a triangle from bottom left to top left to bottom right.
        int[] triangleMap = { 0, 4, 1, 1, 4, 5 };

        Vector3 lastLocalForward = Vector3.up;

        for (int i = 0; i < path.Length; i++)
        {
            //  Calculate the tangent of the line. 
            //  1. In general, the tangent is constructed in the direction of the next point.
            //  2. However, if the current point is the end of the path, and there is no next point, then the tangent is set equal to the tangent of the previous point.
            //  3. If the next point is equal to the current point (such as when the path first starts or turns) then the tangent is set equal to the tangent of the previous point, and... 
            //  3.5 ... in the case that there's no previous point (solely the case of when the path first starts) then the tangent is set to an arbitrary value.

            Vector3 localForward;
            if (i == path.Length - 1)
            {
                //  2
                localForward = lastLocalForward;
            }
            else if (path[i] == path[i + 1])
            {
                //  3
                if (lastLocalForward == null) localForward = Vector3.up;    //  3.5 This is an arbitrary vector.
                localForward = lastLocalForward;
            }
            else
            {
                //  1
                localForward = path[i + 1] - path[i];
            }

            lastLocalForward = localForward;    //  For the next iteration's calculations.

            Vector3 localUp = path[i].normalized;
            Vector3 localRight = Vector3.Cross(path[i], localForward);
            localRight.Normalize();

            // Find position to left and right of current path vertex
            Vector3 vertSideA = path[i] - localRight * Mathf.Abs(lineWidth);
            Vector3 vertSideB = path[i] + localRight * Mathf.Abs(lineWidth);

            // Add top of line vertices
            verts[vertIndex + 0] = vertSideA;
            verts[vertIndex + 1] = vertSideB;
            // Add bottom of line vertices
            verts[vertIndex + 2] = vertSideA;
            verts[vertIndex + 3] = vertSideB;

            // Set uv on y axis to path time (0 at start of path, up to 1 at end of path)
            //uvs[vertIndex + 0] = new Vector2(0, path.times[i]);
            //uvs[vertIndex + 1] = new Vector2(1, path.times[i]);


            // Top of path normals
            normals[vertIndex + 0] = localUp;
            normals[vertIndex + 1] = localUp;
            // Bottom of path normals
            normals[vertIndex + 2] = -localUp;
            normals[vertIndex + 3] = -localUp;

            // Set triangle indices
            if (i < path.Length - 1)
            {
                for (int j = 0; j < triangleMap.Length; j++)
                {
                    topTriangles[triIndex + j] = (vertIndex + triangleMap[j]) % verts.Length;
                    // reverse triangle map for under path so that triangles wind the other way and are visible from underneath
                    bottomTriangles[triIndex + j] = (vertIndex + triangleMap[triangleMap.Length - 1 - j] + 2);
                }
            }

            vertIndex += 4;
            triIndex += 6;
        }

        mesh.Clear();
        mesh.vertices = verts;
        //mesh.uv = uvs;
        mesh.normals = normals;
        mesh.subMeshCount = 2;
        mesh.SetTriangles(topTriangles, 0);
        mesh.SetTriangles(bottomTriangles, 1);
        mesh.RecalculateBounds();
    }
}
