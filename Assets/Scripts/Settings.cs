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
