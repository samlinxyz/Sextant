using UnityEngine;

[ExecuteInEditMode]
public class StarFieldManager : MonoBehaviour
{
    public GameManager game;
    public Camera mainCamera;

    public static StarFieldManager instance;

    public GameObject starPrefab;

    [SerializeField, Range(0f, 1f)]
    private float dimAlpha = 0.5f;
    public float DimAlpha { get { return dimAlpha; } }

    private Star[] stars = null;
    public Star[] Stars
    {
        get
        {
            if (stars == null || stars.Length == 0)
            {
                stars = GetComponentsInChildren<Star>();
            }
            return stars;
        }
    }

    void Awake()
    {
        instance = this;
    }

    //  Creates the original positions of the stars
        public void ConfigureStarsTransform()
    {
        foreach (Star star in GetComponentsInChildren<Star>())
        {
            star.ConfigureTransform();
            star.UpdateTransform();
        }
    }

    public void SquishStarsAround(ConstellationLines level)
    {
        float angle = Mathf.Asin(level.LevelRadius / level.Distance);
        foreach (Star star in Stars)
        {
            if (Vector3.Angle(star.TruePosition, level.transform.localPosition) <= Mathf.Rad2Deg * angle)
            {
                star.ConfigureSquishedTransform(level.SquishParameters);
                star.UpdateTransformExaggerated();
            }
        }
    }

    public void DimStarsOutside(ConstellationLines level, bool dim)
    {
        float angle = Mathf.Asin(level.GetComponent<ConstellationLines>().LevelRadius / level.Distance);
        foreach (Star star in Stars)
        {
            if (Vector3.Angle(star.TruePosition, level.transform.localPosition) > Mathf.Rad2Deg * angle)
            {
                star.DimStarColor(dim);
            }
            star.UpdateTransform();
        }
    }

    [System.Serializable]
    public class SquishParameters
    {
        [System.Serializable]
        public enum SquishMode
        {
            Linear,
            Power,
        }

        [SerializeField]
        private SquishMode mode = SquishMode.Linear;
        public SquishMode Mode
        {
            get { return mode; }
        }

        [SerializeField]
        private float radialCompression = 1f;
        public float RadialCompression
        {
            get { return radialCompression; }
            private set { radialCompression = value; }
        }

        [SerializeField]
        private float minRadius = 0f;
        public float MinRadius
        {
            get { return minRadius; }
            private set { minRadius = value; }
        }

        /*
        [SerializeField]
        private float farRadialCompression = 1f;
        public float FarRadialCompression
        {
            get { return farRadialCompression; }
            private set { farRadialCompression = value; }
        }
        [SerializeField]
        private float farRadius = 0f;
        public float FarRadius
        {
            get { return farRadius; }
            private set { farRadius = value; }
        }
        */

        [SerializeField]
        private float power = 1f;
        public float Power
        {
            get { return power; }
        }

        [SerializeField]
        private float median = 200f;
        public float Median
        {
            get { return median; }
        }


        public SquishParameters(float radialCompression, float minRadius)
        {
            RadialCompression = radialCompression;
            MinRadius = minRadius;
        }

        public SquishParameters() { }
    }

    public static Vector3 SquishPosition(SquishParameters parameters, Vector3 initialPosition)
    {
        if (parameters == null)
        {
            Debug.LogError("Parameters have not been initialized for this constellation, and the identity transformation has been applied.");
            parameters = new SquishParameters();
        }

        switch (parameters.Mode)
        {
            case SquishParameters.SquishMode.Linear:
                return initialPosition / parameters.RadialCompression + parameters.MinRadius * initialPosition.normalized;
            case SquishParameters.SquishMode.Power:
                return Mathf.Pow(initialPosition.magnitude / parameters.Median, 1f / parameters.Power) * parameters.Median * initialPosition.normalized;
            default:
                Debug.LogError("Calculations in SquishPosition do not include the current squishmode. The identity transformation has been applied.");
                return initialPosition;
        }
    }
}
