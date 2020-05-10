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
    private float dimAlpha;
    public float DimAlpha { get { return dimAlpha; } }

    void Awake()
    {
        instance = this;
    }

    //  Loads data about position, color, and absolute magnitude of all stars from a csv.
    public void LoadStars()
    {
        if (File.Exists(Application.dataPath + "/starsData.csv"))
        {
            string dataString = File.ReadAllText(Application.dataPath + "/Resources/Stars - GameData.csv");
            foreach (string line in dataString.Split('\n'))
            {
                string[] entry = line.Split(',');

                GameObject starObject = Instantiate(starPrefab, transform);
                starObject.tag = "Star";

                starObject.transform.position = new Vector3(float.Parse(entry[0]), float.Parse(entry[1]), float.Parse(entry[2]));

                Star star = starObject.GetComponent<Star>();

                star.game = game;
                star.cam = mainCamera.transform;
                star.field = this;

                star.Temperature = float.Parse(entry[3]);

                star.absoluteMagnitude = float.Parse(entry[4]);
                star.truePosition = starObject.transform.position;

                
            }
        }

        ConfigureStarsTransform();
        StarsFaceOrigin();
    }

    public void DeleteAllStars()
    {
        int childCount = transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            DestroyImmediate(transform.GetChild(childCount - i - 1).gameObject);
        }
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
                    star.GetComponent<Star>().ConfigureSquishedTransform(level.GetComponent<ConstellationLines>().squishFactor);
                    star.GetComponent<Star>().UpdateTransformExaggerated();

                }
                else
                    star.GetComponent<Star>().ConfigureTransform();
                    star.GetComponent<Star>().UpdateTransform();
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

    //  A function for the connector window which makes the stars look at the origin 
    public void StarsFaceOrigin()
    {
        foreach (Transform star in transform)
        {
            star.rotation = Quaternion.LookRotation(star.position);
        }
    }

    #region Math functions for squishing

    public float median;

    public float SquishDistance(float trueDistance, float squishFactor)
    {
        trueDistance /= median;
        return median * Mathf.Pow(trueDistance, 1f / squishFactor);
    }

    public Vector3 SquishPosition(Vector3 truePosition, float squishFactor)
    {
        return SquishDistance(truePosition.magnitude, squishFactor) * truePosition.normalized;
    }

    //  The inverse of SquishDistance
    public float UnsquishDistance(float squishedDistance, float squishFactor)
    {
        squishedDistance /= median;
        return median * Mathf.Pow(squishedDistance, squishFactor);
    }
    #endregion
}
