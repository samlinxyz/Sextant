using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshHelper : MonoBehaviour
{
    public int numberOfSpokes;

    public Color centerColor;

    public void GenerateMesh()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;

        mesh.Clear();

        Vector3[] vertices = new Vector3[numberOfSpokes + 1];

        vertices[0] = Vector3.zero;

        for (int i = 1; i <= numberOfSpokes; i++)
        {
            vertices[i] = Quaternion.Euler(0f, 0f, 360f * i / numberOfSpokes) * Vector3.up;
        }

        Color[] colors = new Color[numberOfSpokes + 1];

        colors[0] = centerColor;

        for (int i = 1; i <= numberOfSpokes; i++)
        {
            colors[i]= new Color(0f, 0f, 0f, 0f);
        }

        int[] triangles = new int[3 * numberOfSpokes];

        for (int i = 0; i < numberOfSpokes - 1; i++)
        {
            triangles[3 * i] = 0;
            triangles[3 * i + 1] = 2 + i;
            triangles[3 * i + 2] = 1 + i;
        }

        triangles[3 * numberOfSpokes - 3] = 0;
        triangles[3 * numberOfSpokes - 2] = 1;
        triangles[3 * numberOfSpokes - 1] = numberOfSpokes;

        mesh.vertices = vertices;
        mesh.colors = colors;
        mesh.triangles = triangles;

    }
}
