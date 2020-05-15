using System.Linq;
using UnityEngine;

public class StarField
{
    public static Transform[] StarTransformArray()
    {
        return GameObject.FindGameObjectsWithTag("Star").Select(gameObject => gameObject.transform).ToArray();
    }
    public static Star[] StarArray()
    {
        return GameObject.FindGameObjectsWithTag("Star").Select(gameObject => gameObject.GetComponent<Star>()).ToArray();
    }
}
