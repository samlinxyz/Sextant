using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum Locus
{
    Planckian,
    Alto
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
    private Locus locus;

    [SerializeField]
    private Gradient planckianLocus;
    [SerializeField]
    private Gradient altoLocus;
    public Color ReadLocus(float temperature)
    {
        float temperatureNormalized = temperature / 30000f;
        if (temperatureNormalized > 1f || temperatureNormalized <= 0f)
        {
            Debug.LogError($"A star has a temperature of {temperature}, which is outside the range (1K, 30000 K]. The error color is applied.");
            return Color.magenta;
        }
        switch (locus)
        {
            case Locus.Planckian:
                return planckianLocus.Evaluate(temperatureNormalized);
            case Locus.Alto:
                return altoLocus.Evaluate(temperatureNormalized);
            default:
                return Color.black;
        }
    }
    
}
