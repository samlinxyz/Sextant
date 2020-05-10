using UnityEngine;

[ExecuteInEditMode]
public class LightingManager : MonoBehaviour
{
    void Start()
    {
        RestoreAtmosphere();
    }

    //  Helps restore the atmosphere to normal thickness after leaving play mode during play view.
    void RestoreAtmosphere()
    {
        if (RenderSettings.skybox.GetFloat("_AtmosphereThickness") != 1f)
        {
            RenderSettings.skybox.SetFloat("_AtmosphereThickness", 1f);
        }
    }
}
