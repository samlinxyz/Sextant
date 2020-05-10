using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstellationLines : MonoBehaviour
{
    public List<Vector3> vertices;

    public GameManager game;
    public StarFieldManager field;
    [Range(0f, 1f)]
    public float gizmosAlpha;
    public bool showDelete;

    
    public bool transformVertices;

    public float squishFactor;
    public float fieldOfView;
    public float zRotation;



    private StarSublevel[] stages;
    private StarSublevel[] Stages
    {
        get
        {
            if (stages == null)
            {
                stages = GetComponentsInChildren<StarSublevel>();
            }
            return stages;
        }
    }

    private Line[] lines;
    private Line[] Lines
    {
        get
        {
            if (lines == null)
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

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.white * gizmosAlpha;
        for (int i = 0; i < vertices.Count; i += 2)
        {
            if (transformVertices)
                Gizmos.DrawLine(field.SquishPosition(vertices[i], squishFactor), field.SquishPosition(vertices[i + 1], squishFactor));
            else
                Gizmos.DrawLine(vertices[i], vertices[i + 1]);
        }
    }


    public void AddLine(Vector3 start, Vector3 end)
    {
        vertices.Add(start);
        vertices.Add(end);
    }

    public int GetLineCount() { return vertices.Count / 2; }
    //public void SetSquishFactor() { squishFactor = game.squishynessFactor; } 
    public float GetSquishFactor() { return squishFactor; }

    public void SetFOV(float degrees) { fieldOfView = degrees; }
    public float GetFOV() { return fieldOfView; }
    public void SetZRotation(float cameraZRotation) { zRotation = cameraZRotation; }
    public float GetZRotation() { return zRotation; }

    [SerializeField]
    private SphereCollider levelSphere;
    public float LevelRadius { get { return levelSphere.radius; } }

    public void EnableColliderButton(bool enable)
    {
        levelSphere.enabled = enable;
    }

    public void SetStarReferences()
    {
        foreach (StarSublevel stage in Stages)
        {
            stage.FindAssociatedStar();
        }
        foreach (Line line in Lines)
        {
            line.FindAssociatedStars();
        }
    }
    public void EditorNormalizeStagePositions()
    {
        foreach (StarSublevel stage in GetComponentsInChildren<StarSublevel>())
            stage.NormalizePosition();
    }

    [SerializeField, Range(1f, 50f)]
    private float stageRadius;
    public void EditorAdjustColliderSize()
    {
        foreach (SphereCollider stageButton in transform.GetComponentsInChildren<SphereCollider>())
        {
            if (stageButton != levelSphere)
                stageButton.radius = stageRadius;
        }
    }
}
