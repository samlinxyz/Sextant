using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class StarSublevel : MonoBehaviour
{
    [SerializeField, Range(0f, 1f)]
    private float difficulty;
    public float Difficulty
    {
        get 
        {
            float normalizedTemperature = associatedStar.Temperature / 30000f;
            float normalizedDifficulty = (Mathf.Sqrt(normalizedTemperature) - 0.5f) * 3.1f + 0.5f;
            normalizedDifficulty = Mathf.Clamp01(normalizedDifficulty);
            difficulty = normalizedDifficulty;
            return normalizedDifficulty;
        }
    }

    [SerializeField]
    bool completed = false;
    public bool Completed
    {
        get { return completed; }
        set { completed = value; }
    }

    GameManager game;
    Camera cam;
    Levels levels;
    void Start()
    {
        game = GameManager.instance;
        cam = Camera.main;
        levels = Levels.instance;
    }
    void OnMouseUpAsButton()
    {
        if (game.state == GameState.Level) game.SelectStage(this, Completed);
    }

    void Update()
    {
        if (visible)
        {
            transform.rotation = Quaternion.LookRotation(transform.position, cam.transform.up);
            transform.Rotate(0f, 0f, levels.DiffractionAngle);
        }
    }

    public SpriteRenderer diffraction;
    public SpriteRenderer refraction;

    [SerializeField]
    private bool visible;
    public bool Visible
    {
        get
        {
            return visible;
        }
        set
        {
            if (value == true)
            {
                diffraction.DOFade(1f, 1f).SetEase(Ease.OutBack).OnStart(() =>
                {
                    visible = true;
                    diffraction.gameObject.SetActive(true);
                });
            }
            else
            {
                diffraction.DOFade(0f, 1f).SetEase(Ease.InBack).OnComplete(() =>
                {
                    visible = false;
                    diffraction.gameObject.SetActive(false);
                });
            }
        }
    }

    [SerializeField]
    private Star associatedStar;
    public Star AssociatedStar { get { return associatedStar; } }

    //  This sets the reference to the Star associated with this Stage
    public void FindAssociatedStar()
    {
        Transform candidateStar = null;
        float closestAngle = 180f;
        foreach (Transform star in StarField.StarTransformArray())
        {
            float angle = Vector3.Angle(transform.position, star.position);
            if (closestAngle > angle)
            {
                candidateStar = star;
                closestAngle = angle;
            }
        }

        if (closestAngle > 0.01f)
            Debug.LogWarning($"The star set to be the associated star is more than 0.01 degrees away from stage {this.name}. Check that associated star for {this.name} is correct.");
        associatedStar = candidateStar.GetComponent<Star>();

        //  Set up the stage to match the star's color and size
        float distance = associatedStar.transform.position.magnitude;
        float vmag = associatedStar.correctedAbsoluteMagnitude - 7.5f + 5 * Mathf.Log10(distance);
        vmag = 1f - vmag / 6.5f;
        vmag = Mathf.Clamp(vmag, 0f, 5f);
        diffraction.transform.localScale = 1.5f * Vector3.one * (0.75f * vmag + 0.25f);
        diffraction.color = associatedStar.TrueColor;
    }

    //  Sets the position of this stage as 200 m in the direction of the associated star's position.
    public void NormalizePosition()
    {
        transform.position = 200f * associatedStar.transform.position.normalized;
    }
}
