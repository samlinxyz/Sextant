using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using UnityEngine;

public class ConstellationLines : MonoBehaviour
{
    public List<Vector3> vertices;

    [field: SerializeField]
    public float helloWorld { get; set; }

    public GameManager game;
    public StarFieldManager field;
    [Range(0f, 1f)]
    public float gizmosAlpha;
    public bool showDelete;

    public bool transformVertices;

    [SerializeField, Range(1f, 50f)]
    private float squishFactor = 1f;
    public float SquishFactor
    {
        get { return squishFactor; }
    }
    public float fieldOfView;
    public float zRotation;

    public float Distance 
    { 
        get { return transform.localPosition.magnitude; } 
    }

    [SerializeField]
    private StarSublevel[] stages = null;
    public StarSublevel[] Stages
    {
        get
        {
            if (stages == null || stages.Length == 0)
            {
                stages = GetComponentsInChildren<StarSublevel>();
            }
            return stages;
        }
    }

    [SerializeField]
    private Line[] lines = null;
    public Line[] Lines
    {
        get
        {
            if (lines == null || lines.Length == 0)
            {
                lines = GetComponentsInChildren<Line>();
            }
            return lines;
        }
    }

    public bool[] GetProgress()
    {
        int stageCount = stages.Length;
        bool[] progress = new bool[stageCount];
        for (int i = 0; i < stageCount; i++) progress[i] = stages[i].Completed;
        return progress;
    }

    public void SetProgress(bool[] progress)
    {
        for (int i = 0; i < stages.Length; i++) stages[i].Completed = progress[i];
    }

    // this animates in the lines. if you want to set instantly, use setalpha
    public void UpdateLinesColors()
    {
        foreach (Line line in lines)
        {
            line.UpdateColor();
        }
    }

    public void FadeLines(bool fadeIn)
    {
        foreach (Line line in lines)
        {
            if (fadeIn)
            {
                line.UpdateColor();
            }
            else
            {
                line.Fade();
            }
        }
    }
    public void SetAlpha(float alpha)
    {
        foreach (Line line in lines)
        {
            line.SetAlpha(alpha);
        }
    }

    void Awake()
    {
        stages = GetComponentsInChildren<StarSublevel>();
        lines = GetComponentsInChildren<Line>();
    }
    // Start is called before the first frame update
    void Start()
    {
        game = GameManager.instance;
        field = StarFieldManager.instance;
        vertices = new List<Vector3>();
    }

    public void ShowStages(bool show)
    {
        foreach (StarSublevel stage in stages)
        {
            stage.Visible = show;
        }
    }

    void OnMouseUpAsButton()
    {
        if (game.state == GameState.Sky && game.mouseSky.dragged == false && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) game.SelectLevel(transform);
    }

    #region Gizmos
    public void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white * gizmosAlpha;
        DrawLines();

        Gizmos.color = Color.green * gizmosAlpha;
        DrawStartingPoints();

        // Draw player movement range
        Gizmos.color = Color.gray * gizmosAlpha;
        Gizmos.DrawWireSphere(transform.position, transform.position.magnitude);

        Gizmos.color = Color.blue * gizmosAlpha;
        Gizmos.DrawWireSphere(transform.position, transform.position.magnitude * Mathf.Sin(Mathf.Deg2Rad * frame.FieldOfView / 2f));
    }

    private void DrawLines()
    {
        for (int i = 0; i < vertices.Count; i += 2)
        {
            if (transformVertices)
                Gizmos.DrawLine(StarFieldManager.SquishPosition(squishParameters, vertices[i]), StarFieldManager.SquishPosition(squishParameters, vertices[i+1]));
            else
                Gizmos.DrawLine(vertices[i], vertices[i + 1]);
        }
    }

    private void DrawStartingPoints()
    {
        foreach (StarSublevel stage in Stages)
        {
            // Projects starting point of stars onto the player movement sphere.
            Vector3 projectedLocalLocation = (StarFieldManager.SquishPosition(squishParameters, stage.AssociatedStar.TruePosition) - transform.localPosition).normalized * transform.localPosition.magnitude;
            Gizmos.DrawSphere(transform.root.TransformPoint(projectedLocalLocation) + transform.position, 10f);
        }
    }
    #endregion

    public void AddLine(Vector3 start, Vector3 end)
    {
        vertices.Add(start);
        vertices.Add(end);
    }

    public int GetLineCount() { return vertices.Count / 2; }
    //public void SetSquishFactor() { squishFactor = game.squishynessFactor; } 
    public float GetSquishFactor() { return squishFactor; }


    [System.Serializable]
    public class Frame
    {
        [SerializeField]
        private float fieldOfView = 120f;
        public float FieldOfView
        {
            get { return fieldOfView; }
            private set { fieldOfView = value; }
        }

        [SerializeField]
        private float zRotation = 90f;
        public float ZRotation
        {
            get { return zRotation; }
            private set { zRotation = value; }
        }
        [SerializeField]
        private bool isotropicShape = true;
        public bool IsotropicShape
        {
            get { return isotropicShape; }
            private set { isotropicShape = value; }
        }

        public Frame(float fieldOfView, float zRotation)
        {
            FieldOfView = fieldOfView;
            ZRotation = zRotation;
        }
    }

    [SerializeField]
    private Frame frame = null;

    public Frame getFrame
    {
        get { return frame; }
    }

    [SerializeField]
    private SphereCollider levelSphere = null;
    public float LevelRadius { get { return levelSphere.radius; } }

    public void EnableColliderButton(bool enable)
    {
        levelSphere.enabled = enable;
    }

    [SerializeField]
    private StarFieldManager.SquishParameters squishParameters = null;
    public StarFieldManager.SquishParameters SquishParameters
    {
        get { return squishParameters; }
    }
}
