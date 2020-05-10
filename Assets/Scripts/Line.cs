using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[ExecuteInEditMode]
public class Line : MonoBehaviour
{
    [SerializeField]
    private Camera cam;
    [SerializeField]
    private LineRenderer line;
    void Start()
    {
        cam = Camera.main;
        EditorUpdateColor();
    }
    void Update()
    {
        //UpdatePosition();
        
    }

    public void UpdatePosition()
    {
        Vector3 position1 = 100f * (star1.position - cam.transform.position).normalized; 
        Vector3 position2 = 100f * (star2.position - cam.transform.position).normalized;
        line.startWidth = line.endWidth = 0.2f;

        line.SetPositions(new Vector3[] 
        {
            transform.InverseTransformPoint(cam.transform.position + position1),
            transform.InverseTransformPoint(cam.transform.position + position2)
        } );
    }

    private Color incompleteColor = Color.Lerp(Color.black, Color.white, 0.2f);
    private float alpha = 0.3f;
    private float incompleteAlpha = 0.7f;

    public void UpdateColor()
    {
        if (starStage1 == null && starStage2 == null)
        {
            Debug.LogError("The constellation line does not connect to at least one starStage. Make sure you have manually set references to the levels.");
            return;
        }

        Color startColor1 = line.colorGradient.colorKeys[0].color;
        startColor1.a = line.colorGradient.alphaKeys[0].alpha;
        Color startColor2 = line.colorGradient.colorKeys[1].color;
        startColor2.a = line.colorGradient.alphaKeys[1].alpha;

        Color endColor1 = Color.magenta;
        if (starStage1 == null)
        {
            endColor1 = starStage2.Completed ? star1.GetComponent<SpriteRenderer>().color : incompleteColor;
        }
        else
        {
            endColor1 = starStage1.Completed ? star1.GetComponent<SpriteRenderer>().color : incompleteColor;
        }
        endColor1.a = (endColor1 == incompleteColor)? incompleteAlpha : alpha;
        Color endColor2 = Color.magenta;
        if (starStage2 == null)
        {
            endColor2 = starStage1.Completed ? star2.GetComponent<SpriteRenderer>().color : incompleteColor;
        }
        else
        {
            endColor2 = starStage2.Completed ? star2.GetComponent<SpriteRenderer>().color : incompleteColor;
        }
        endColor2.a = (endColor2 == incompleteColor) ? incompleteAlpha : alpha;

        line.DOColor(new Color2(startColor1, startColor2), new Color2(endColor1, endColor2), 1f);
    }

    public void Fade()
    {
        Color startColor1 = line.colorGradient.colorKeys[0].color;
        startColor1.a = line.colorGradient.alphaKeys[0].alpha;
        Color startColor2 = line.colorGradient.colorKeys[1].color;
        startColor2.a = line.colorGradient.alphaKeys[1].alpha;

        line.DOColor(new Color2(startColor1, startColor2), new Color2(Color.clear, Color.clear), 1f);
    }

    public void SetAlpha(float alpha)
    {
        Color startColor1 = line.colorGradient.colorKeys[0].color;
        Color startColor2 = line.colorGradient.colorKeys[1].color;

        Gradient gradient = new Gradient();

        GradientColorKey[] colorKeys = new GradientColorKey[]
        {
            new GradientColorKey(startColor1, 0f),
            new GradientColorKey(startColor2, 1f)
        };
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[]
        {
            new GradientAlphaKey(alpha, 0f),
            new GradientAlphaKey(alpha, 1f)
        };
        gradient.alphaKeys = alphaKeys;
        gradient.colorKeys = colorKeys;

        line.colorGradient = gradient;
    }

    public void EditorUpdateColor()
    {
        if (starStage1 == null && starStage2 == null)
        {
            Debug.LogError("The constellation line does not connect to at least one starStage. Make sure you have manually set references to the levels.");
            return;
        }

        Color startColor1 = line.colorGradient.colorKeys[0].color;
        startColor1.a = line.colorGradient.alphaKeys[0].alpha;
        Color startColor2 = line.colorGradient.colorKeys[1].color;
        startColor2.a = line.colorGradient.alphaKeys[1].alpha;

        Color endColor1 = Color.magenta;
        if (starStage1 == null)
        {
            endColor1 = starStage2.Completed ? star1.GetComponent<SpriteRenderer>().color : incompleteColor;
        }
        else
        {
            endColor1 = starStage1.Completed ? star1.GetComponent<SpriteRenderer>().color : incompleteColor;
        }
        Color endColor2 = Color.magenta;
        if (starStage2 == null)
        {
            endColor2 = starStage1.Completed ? star2.GetComponent<SpriteRenderer>().color : incompleteColor;
        }
        else
        {
            endColor2 = starStage2.Completed ? star2.GetComponent<SpriteRenderer>().color : incompleteColor;
        }

        Gradient gradient = new Gradient();

        GradientColorKey[] colorKeys = new GradientColorKey[]
        {
            new GradientColorKey(endColor1, 0f),
            new GradientColorKey(endColor2, 1f)
        };
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[]
        {
            new GradientAlphaKey(alpha, 0f),
            new GradientAlphaKey(alpha, 1f)
        };
        gradient.alphaKeys = alphaKeys;
        gradient.colorKeys = colorKeys;

        line.colorGradient = gradient;
    }

    [SerializeField]
    private Transform star1;
    [SerializeField]
    private Transform star2;

    [SerializeField]
    private StarSublevel starStage1;
    [SerializeField]
    private StarSublevel starStage2;

    public bool SetStarReferences(GameObject[] stars, StarSublevel[] stages)
    {
        if (stars.Length != 2)
        {
            Debug.LogError("Star references not set in Line. You are not assigning 2 references, which is the only sensible number of references for a line! The references were not assigned.");
            return false;
        }
        if (stages.Length != 2)
        {
            Debug.LogError("what the hell did you do?");
        }

        star1 = stars[0].transform;
        star2 = stars[1].transform;

        starStage1 = stages[0];
        starStage2 = stages[1];

        return true;
    }


    //  This sets the reference to the Star associated with this Stage
    public void FindAssociatedStars()
    {
        Transform candidateStar = null;
        float closestAngle = 180f;
        foreach (Transform star in StarField.StarTransformArray())
        {
            float angle = Vector3.Angle(line.GetPosition(0), star.position);
            if (closestAngle > angle)
            {
                candidateStar = star;
                closestAngle = angle;
            }
        }

        if (closestAngle > 0.01f)
            Debug.LogWarning($"The star set to be the associated star is more than 0.01 degrees away from end 1 of {this.name}. Check that associated star for {this.name} is correct.");
        star1 = candidateStar;

        candidateStar = null;
        closestAngle = 180f;
        foreach (Transform star in StarField.StarTransformArray())
        {
            float angle = Vector3.Angle(line.GetPosition(1), star.position);
            if (closestAngle > angle)
            {
                candidateStar = star;
                closestAngle = angle;
            }
        }

        if (closestAngle > 0.01f)
            Debug.LogWarning($"The star set to be the associated star is more than 0.01 degrees away from end 2 of {this.name}. Check that associated star for {this.name} is correct.");
        star2 = candidateStar;

        UpdatePosition();
    }
}
