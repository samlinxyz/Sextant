﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Star : MonoBehaviour
{
    public GameManager game;
    public Transform cam;
    public StarFieldManager field;

    public Vector3 truePosition;

    public float absoluteMagnitude;
    public float correctedAbsoluteMagnitude;

    [SerializeField]
    private SpriteRenderer sprite;

    public Color trueColor; //  The color that all stars can revert to.

    [SerializeField]
    private float temperature = 0f;
    public float Temperature
    {
        get
        {
            return temperature;
        }
        //  This should be used only once, at initialization
        set
        {
            if (temperature == 0f)
            {
                Color starColor = Settings.I.ReadLocus(value);
                sprite.color = trueColor = starColor;
                temperature = value;
            }
            else
            {
                Debug.LogError("Temperature should only be set once, immediately after instantiating the star.");
            }
        }
    }

    bool squished = false;

    bool dimmed;

    public void DimStarColor(bool dim)
    {
        dimmed = dim;
    }

    // Start is called before the first frame update
    void Start()
    {
        game = GameManager.instance;
        cam = Camera.main.transform;
        field = StarFieldManager.instance;

        sprite = GetComponent<SpriteRenderer>();

        squished = false;
        ConfigureTransform();
    }

    // Update is called once per frame
    void Update()
    {
        if (sprite.isVisible)
        {
            if (!squished)
                UpdateTransform();
            else
                UpdateTransformExaggerated();
        }
    }

    public void ConfigureTransform()
    {
        transform.localPosition = truePosition;
        correctedAbsoluteMagnitude = absoluteMagnitude;

        squished = false;
    }

    public void UpdateTransform()
    {
        float distance = Vector3.Distance(cam.position, transform.position);    //  Distance is not optimal. Use sqrMagnitude.
        float vmag = correctedAbsoluteMagnitude - 7.5f + 5 * Mathf.Log10(distance);
        vmag = 1f - vmag / 6.5f;
        vmag = Mathf.Clamp(vmag, 0f, 5f); // 5 is way more than the max

        //  Transformations
        transform.localScale = game.starSize * distance * Vector3.one * (0.75f * vmag + 0.25f);

        Color color = trueColor;
        if (vmag < 0.5f)
        {
            vmag *= 2f;
        } else if (vmag < 1f)
        {
            vmag = 1f;
        }
        color.a = dimmed ? field.DimAlpha * vmag : vmag;
        sprite.color = color;

        transform.rotation = cam.rotation;
    }

    public void UpdateTransformExaggerated()
    {
        float distance = Vector3.Distance(cam.position, transform.position);    //  Distance is not optimal. Use sqrMagnitude.
//float vmag = correctedAbsoluteMagnitude - 7.5f + 5 * Mathf.Log10(field.UnsquishDistance(distance, 4f));
        float vmag = correctedAbsoluteMagnitude - 7.5f + 5 * Mathf.Log10(distance);
        vmag = 1f - vmag / 6.5f;
        vmag = Mathf.Clamp(vmag, 0f, 5f); // 5 is way more than the max

        //  Transformations
        transform.localScale = game.starSize * distance * Vector3.one * (0.75f * vmag + 0.25f);

        Color color = trueColor;
        if (vmag < 0.5f)
        {
            vmag *= 2f;
        }
        else if (vmag < 1f)
        {
            vmag = 1f;
        }
        color.a = dimmed ? field.DimAlpha * vmag : vmag;
        sprite.color = color;

        transform.rotation = cam.rotation;
    } 

    public void ConfigureSquishedTransform(float squishFactor)
    {
        //  Use absolute magnitude to calculate vmag, and then perform a spatial transformation. Calculate the strange absolute magnitude.

        float distance = truePosition.magnitude;    //  magnitude is not optimal. Consider using sqrMagnitude.

        //  This is the formula when distance is measured in lightyears.
        float vmag = absoluteMagnitude - 7.5f + 5 * Mathf.Log10(distance);

        float strangeDistance = field.SquishDistance(distance, squishFactor);
        

        transform.localPosition = strangeDistance * truePosition.normalized;


        //  Calculate the star's absolute brightness given relative brightness if the star is strangeDistance away from the origin.
        //correctedAbsoluteMagnitude = vmag + 7.5f - 5 * Mathf.Log10(strangeDistance);

        //  Calculate the star's absolute brightness given relative brightness if the star is strangeDistance away from the origin. The last bit compensates for the fact that we want to exaggerate star brightness.
        //correctedAbsoluteMagnitude = vmag + 7.5f - 5 * Mathf.Log10(field.UnsquishDistance(strangeDistance, 4f));
        //  Ok we don't actually want to exaggerate star brightness.
        correctedAbsoluteMagnitude = vmag + 7.5f - 5 * Mathf.Log10(strangeDistance);

        squished = true;
    }

}
