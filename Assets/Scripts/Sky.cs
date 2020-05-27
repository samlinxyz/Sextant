using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Sky : MonoBehaviour
{
    [SerializeField]
    private float latitude = 20f;
    [SerializeField]
    private int hoursToRotate = 6;

    private void Awake()
    {
        InitializeSky();
    }

    public void InitializeSky()
    {
        //  The north pole is rotated along the y-z plane
        transform.rotation = Quaternion.AngleAxis(90f - latitude, Vector3.right);
        transform.Rotate(0f, -110f, 0f);
    }

    public void RotateSky(bool forward)
    {
        float rotationDegrees = (float)hoursToRotate * 360 / 24;
        rotationDegrees *= forward ? 1 : -1;
        Quaternion targetRotation = Quaternion.AngleAxis(rotationDegrees, transform.up) * transform.rotation;
        transform.DORotateQuaternion(targetRotation, 5f).SetEase(Ease.InOutSine);
    }

    public void RotateSky(float degrees)
    {
        Quaternion targetRotation = Quaternion.AngleAxis(degrees, transform.up) * transform.rotation;
        transform.DORotateQuaternion(targetRotation, 1.2f).SetEase(Ease.OutSine);
    }
}
