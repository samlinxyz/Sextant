using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class GroundDarkness : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
        Mesh mesh = new Mesh();
        mesh = GetComponent<MeshFilter>().mesh;

        mesh.Clear();

        Vector3[] vertices = new Vector3[]
        {
            Vector3.left,
            Vector3.right,
            Vector3.left + Vector3.down,
            Vector3.right + Vector3.down,
            Vector3.left + 10f * Vector3.down,
            Vector3.right + 10f * Vector3.down,
        };

        int[] triangles = new int[]
        {
            0, 1, 2,
            2, 1, 3,
            2, 3, 4,
            4, 3, 5,

        };

        Color clearBlack = new Color(0f, 0f, 0f, 0f);

        Color[] colors = new Color[]
        {
            clearBlack,
            clearBlack,
            Color.black,
            Color.black,
            Color.black,
            Color.black,
        };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.colors = colors;
    }
}
