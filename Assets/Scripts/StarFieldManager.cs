using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.CompilerServices;

[ExecuteInEditMode]
public class StarFieldManager : MonoBehaviour
{
    public GameManager game;
    public Camera mainCamera;

    public static StarFieldManager instance;

    public GameObject starPrefab;

    [SerializeField, Range(0f, 1f)]
    private float dimAlpha = 0.5f;
    public float DimAlpha { get { return dimAlpha; } }

    void Awake()
    {
        instance = this;
    }

    //  Creates the original positions of the stars
    public void ConfigureStarsTransform()
    {
        foreach (Transform star in transform)
        {
            star.GetComponent<Star>().ConfigureTransform();
            star.GetComponent<Star>().UpdateTransform();
        }
    }

    public void SquishStarsAround(Transform level, bool squish)
    {
        float angle = Mathf.Asin(level.GetComponent<ConstellationLines>().LevelRadius / level.position.magnitude);
        foreach (Transform star in transform)
        {
            //  If the star is within the constellation, apply the transform.
            if (Vector3.Angle(star.position, level.position) <= Mathf.Rad2Deg * angle)
            {
                if (squish)
                {
                    star.GetComponent<Star>().ConfigureSquishedTransform(level.GetComponent<ConstellationLines>().SquishFactor);
                    star.GetComponent<Star>().UpdateTransformExaggerated();

                }
                else
                {
                    star.GetComponent<Star>().ConfigureTransform();
                    star.GetComponent<Star>().UpdateTransform();
                }
            }
        }
    }

    public void DimStarsOutside(Transform level, bool dim)
    {
        float angle = Mathf.Asin(level.GetComponent<ConstellationLines>().LevelRadius / level.position.magnitude);
        foreach (Transform star in transform)
        {
            //  If the star is within the size  of the direction that the camera faces, apply the transform.
            if (Vector3.Angle(star.position, level.position) > Mathf.Rad2Deg * angle)
            {
                star.GetComponent<Star>().DimStarColor(dim);
            }
            star.GetComponent<Star>().UpdateTransform();
        }
    }

    #region Math functions for squishing

    public static float median = 200f;

    public static float SquishDistance(float trueDistance, float squishFactor)
    {
        trueDistance /= median;
        return median * Mathf.Pow(trueDistance, 1f / squishFactor);
    }

    public static Vector3 SquishPosition(Vector3 truePosition, float squishFactor)
    {
        return SquishDistance(truePosition.magnitude, squishFactor) * truePosition.normalized;
    }

    //  The inverse of SquishDistance
    public static float UnsquishDistance(float squishedDistance, float squishFactor)
    {
        squishedDistance /= median;
        return median * Mathf.Pow(squishedDistance, squishFactor);
    }
    #endregion
}
