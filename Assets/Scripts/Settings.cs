using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum Locus
{
    Planckian,
    Alto,
    DramaticPlanckian
}

[CreateAssetMenu]
public class Settings : ScriptableObject
{
    private static Settings instance;
    public static Settings I
    {
        get
        {
            if (!instance)
            {
                instance = Resources.Load("MySettings") as Settings;
            }
            return instance;
        }
    }

    [SerializeField]
    private float starReferenceMaxErrorDegrees = 0.01f;

    public float StarReferenceMaxErrorDegrees
    {
        get { return starReferenceMaxErrorDegrees; }
    }

    [SerializeField]
    private float maxTemp = 0;
    [SerializeField]
    private float maxRadians = 0;
    public float TemperatureToDifficulty(float temperature)
    {
        float normalizedTemperature = temperature / maxTemp;
        float normalizedDifficulty = (Mathf.Sqrt(normalizedTemperature) - 0.5f) * 3.1f + 0.5f;
        return maxRadians * Mathf.Clamp01(normalizedDifficulty);
    }



    [SerializeField]
    private float skyViewFOV = 70f;
    public float SkyViewFOV { get { return skyViewFOV; } }
    [SerializeField]
    private float bloomIntensity = 12f;
    public float BloomIntensity { get { return bloomIntensity; } }

    [SerializeField]
    private Locus locus = Locus.Planckian;

    [SerializeField]
    private Gradient planckianLocus = null;
    [SerializeField]
    private Gradient altoLocus = null;
    [SerializeField]
    private Gradient dramaticPlanckianLocus = null;
    public Color ReadLocus(float temperature)
    {
        float temperatureNormalized = temperature / 30000f;
        if (temperatureNormalized > 1f || temperatureNormalized <= 0f)
        {
            Debug.LogError($"A star has a temperature of {temperature}, which is outside the range (1K, 30000 K]. The error color is applied.");
            return Color.magenta;
        }
        Gradient heatingGradient;
        switch (locus)
        {
            case Locus.Planckian:
                heatingGradient = planckianLocus;
                break;
            case Locus.Alto:
                heatingGradient = altoLocus;
                break;
            case Locus.DramaticPlanckian:
                heatingGradient = dramaticPlanckianLocus;
                break;
            default:
                return Color.black;
        }
        return heatingGradient.Evaluate(temperatureNormalized);
    }
    
}
