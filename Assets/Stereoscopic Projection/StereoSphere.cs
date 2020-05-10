using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class StereoSphere : MonoBehaviour
{
    public float c;

    Mesh mesh;
    List<Vector3> vertices;
    List<int> triangles;
    List<Vector2> uvs;

    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;


        DrawSphere();
    }
    void DrawSphere()
    {
        vertices = new List<Vector3>();
        triangles = new List<int>();
        uvs = new List<Vector2>();

        for (int theta = 0; theta < 60; theta++)
        {
            for (int phi = 0; phi < 360; phi++)
            {
                int n = vertices.Count;
                triangles.Add(n);
                triangles.Add(n+1);
                triangles.Add(n+3);
                triangles.Add(n);
                triangles.Add(n+3);
                triangles.Add(n+2);

                float t = Mathf.Deg2Rad * (float)theta;
                float p = Mathf.Deg2Rad * (float)phi;
                float o = Mathf.Deg2Rad;

                // (theta, phi)
                Vector3 a00 = new Vector3(Mathf.Sin(p) * Mathf.Sin(t), Mathf.Cos(p) * Mathf.Sin(t), Mathf.Cos(t) - 1);
                Vector3 a10 = new Vector3(Mathf.Sin(p) * Mathf.Sin(t+o), Mathf.Cos(p) * Mathf.Sin(t+o), Mathf.Cos(t+o) - 1);
                Vector3 a01 = new Vector3(Mathf.Sin(p+o) * Mathf.Sin(t), Mathf.Cos(p+o) * Mathf.Sin(t), Mathf.Cos(t) - 1);
                Vector3 a11 = new Vector3(Mathf.Sin(p+o) * Mathf.Sin(t+o), Mathf.Cos(p+o) * Mathf.Sin(t+o), Mathf.Cos(t+o) - 1);

                vertices.Add(a00);
                vertices.Add(a10);
                vertices.Add(a01);
                vertices.Add(a11);


                uvs.Add(uv(t,p));
                uvs.Add(uv(t+o,p));
                uvs.Add(uv(t,p+o));
                uvs.Add(uv(t+o,p+o));
            }
        }


        if (6 * vertices.Count != triangles.Count * 4)
        {
            Debug.Log("vertex and triangle count mismatch" + vertices.Count + " " + triangles.Count);
            return;
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        
    }

    Vector2 uv(float t, float p)
    {
        float y = c * Mathf.Tan(t) / Mathf.Sqrt(1f + Mathf.Tan(p) * Mathf.Tan(p));
        if (p >= 0.5f * Mathf.PI && p < Mathf.PI * 1.5f)
        {
            y = -y;
        }
        float x = y * Mathf.Tan(p);
        y = Mathf.Clamp01(y + 0.5f);
        x = Mathf.Clamp01(x + 0.5f);
        return new Vector2(x, y);
    }


    // Update is called once per frame
    void Update()
    {
        DrawUV();
    }

    void DrawUV()
    {
        uvs = new List<Vector2>();

        for (int theta = 0; theta < 60; theta++)
        {
            for (int phi = 0; phi < 360; phi++)
            {
                float t = Mathf.Deg2Rad * (float)theta;
                float p = Mathf.Deg2Rad * (float)phi;
                float o = Mathf.Deg2Rad;

                uvs.Add(uv(t, p));
                uvs.Add(uv(t + o, p));
                uvs.Add(uv(t, p + o));
                uvs.Add(uv(t + o, p + o));
            }
        }
        mesh.uv = uvs.ToArray();
    }
}
