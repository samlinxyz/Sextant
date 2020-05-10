using UnityEngine;

[ExecuteInEditMode]
public class CircularFade : MonoBehaviour
{
    [SerializeField]
    private Shader shader;
    
    [SerializeField, Range(0f, 2f)]
    public float fadeRadius;    //  In units of the longer screen dimension.

    [SerializeField]
    private Camera cam;

    [SerializeField]
    private Vector2 maskCenterViewport;
    public Vector2 MaskCenterScreen
    {
        get
        {
            return cam.ViewportToScreenPoint(maskCenterViewport);
        }
        set
        {
            maskCenterViewport = cam.ScreenToViewportPoint(value);
        }
    }
    public Vector3 MaskCenterWorld
    {
        set
        {
            maskCenterViewport = cam.WorldToViewportPoint(value);
        }
    }
    public void ResetMaskCenter() 
    {
        maskCenterViewport = 0.5f * Vector2.one;
    }

    [SerializeField, Range(0f, 1f)]
    private float softness;
    [SerializeField]
    private Color maskColor = Color.black;

    private Material m_Material;
    Material material
    {
        get
        {
            if (m_Material == null)
            {
                m_Material = new Material(shader);
                m_Material.hideFlags = HideFlags.HideAndDontSave;
            }
            return m_Material;
        }
    }

    void Start()
    {
        cam = GetComponent<Camera>();

        // Disable the image effect if the shader can't run on the user's graphics card.
        if (shader == null || !shader.isSupported)
        {
            Debug.LogWarning($"Shader is not supported in {this.name}.");
            enabled = false;
        }
    }


    public void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (!enabled)
        {
            Graphics.Blit(source, destination);
            return;
        }

        material.SetColor("_MaskColor", maskColor);
        material.SetFloat("_FadeRadius", fadeRadius);
        material.SetVector("_MaskCenter", maskCenterViewport);
        material.SetFloat("_Softness", softness);
        material.SetTexture("_MainTex", source);

        Graphics.Blit(source, destination, material);
    }

    void OnDisable()
    {
        if (m_Material)
        {
            DestroyImmediate(m_Material);
        }
    }
}
