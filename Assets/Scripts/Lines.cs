using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Lines : MonoBehaviour
{
    void Start()
    {
        foreach (Transform child in transform)
        {
            child.position = Vector3.zero;
            child.rotation = Quaternion.identity;
            child.localScale = Vector3.one;
        }
    }
}
