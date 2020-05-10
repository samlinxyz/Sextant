using UnityEngine;

public class StarField
{
    public static Transform[] StarTransformArray()
    {
        var starObjects = GameObject.FindGameObjectsWithTag("Star");
        Transform[] starTransforms = new Transform[starObjects.Length];
        for (int i = 0; i < starObjects.Length; i++)
        {
            starTransforms[i] = starObjects[i].transform;
        }
        return starTransforms;
    }

    //  A function for the connector window which makes the stars look at the origin 
    public static void StarsFaceOrigin()
    {
        foreach (Transform star in StarTransformArray())
        {
            star.rotation = Quaternion.LookRotation(star.position);
        }
    }
}
