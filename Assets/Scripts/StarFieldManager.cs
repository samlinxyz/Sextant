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

    private Star[] stars = null;
    public Star[] Stars
    {
        get
        {
            if (stars == null || stars.Length == 0)
            {
                stars = GetComponentsInChildren<Star>();
            }
            return stars;
        }
    }

    void Awake()
    {
        instance = this;
    }

    //  Creates the original positions of the stars
        public void ConfigureStarsTransform()
    {
        foreach (Star star in GetComponentsInChildren<Star>())
        {
            star.ConfigureTransform();
            star.UpdateTransform();
        }
    }

    public void SquishStarsAround(ConstellationLines level)
    {
        float angle = Mathf.Asin(level.LevelRadius / level.Distance);
        foreach (Star star in Stars)
        {
            if (Vector3.Angle(star.TruePosition, level.transform.localPosition) <= Mathf.Rad2Deg * angle)
            {
                star.ConfigureSquishedTransform(level.SquishParameters);
                star.UpdateTransformExaggerated();
            }
        }
    }

    public void DimStarsOutside(ConstellationLines level, bool dim)
    {
        float angle = Mathf.Asin(level.GetComponent<ConstellationLines>().LevelRadius / level.Distance);
        foreach (Star star in Stars)
        {
            if (Vector3.Angle(star.TruePosition, level.transform.localPosition) > Mathf.Rad2Deg * angle)
            {
                star.DimStarColor(dim);
            }
            star.UpdateTransform();
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

    [System.Serializable]
    public class SquishParameters
    {
        [SerializeField]
        private float radialCompression = 1f;
        public float RadialCompression
        {
            get { return radialCompression; }
            private set { radialCompression = value; }
        }

        [SerializeField]
        private float minRadius = 0f;
        public float MinRadius
        {
            get { return minRadius; }
            private set { minRadius = value; }
        }

        public SquishParameters(float radialCompression, float minRadius)
        {
            RadialCompression = radialCompression;
            MinRadius = minRadius;
        }

        public SquishParameters() { }
    }

    public static Vector3 SquishPositionLinear(SquishParameters parameters, Vector3 initialPosition)
    {
        if (parameters == null)
        {
            Debug.LogError("Parameters have not been initialized for this constellation, and the identity transformation has been applied.");
            parameters = new SquishParameters();
        }

        return initialPosition / parameters.RadialCompression + parameters.MinRadius * initialPosition.normalized;
    }
}
