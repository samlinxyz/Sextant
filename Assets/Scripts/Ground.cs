using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ground : MonoBehaviour
{
    public Camera mainCamera;
    public float height;
    [SerializeField]
    private Transform floor = null;
    void Update()
    {
        transform.position = mainCamera.transform.forward + height * Vector3.down;
        floor.rotation = Quaternion.Euler(0f, mainCamera.transform.rotation.eulerAngles.y, 0f);
    }
}
