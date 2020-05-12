using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;
using DG.Tweening;


public enum LevelProgress
{
    Zero,
    Partial,
    Complete
}

public enum GameState
{
    Sky,    //  When the player is looking around at all the constellations
    Level,  //  When the player has tapped on one specific constellation and has zoomed in
    Play    //  When the player is navigating the stars
}

public class GameManager : MonoBehaviour
{
    public GameState state;
    public Levels levels;
    public Transform level;
    public StarSublevel stage;
    
    public CanvasGroup skyViewMenu;

    public TextMeshProUGUI selectedLevelName;
    public CanvasGroup levelSelectionCanvas;
    public CanvasGroup stageSelectionCanvas;
    public TextMeshProUGUI stageNameText;
    public TextMeshProUGUI stageCompleteText;
    public TextMeshProUGUI startStageButton;
    public GameObject levelCompleteMenu;
    public GameObject levelFailedMenu;

    public GameObject star;
    public float referenceStarSize; //  The size that a magnitude 0 star appears in the sky view in radians
    public float starSize;  //  The size of a magnitude 0 star modified to let star size appear constant of camera zoom. Stars reference this variable to calculate local scale.
    public Camera cam;

    public Transform player;

    public Minimap minimap;

    public StarFieldManager field;


    public CircularFade circularFade;

    public Slider remainingFuel;

    public MouseSkyNavigation mouseSky;

    #region Selection Camera Animation Variables
    [SerializeField]
    private float skyViewFOV = 70f;
    public float SkyViewFOV { get { return skyViewFOV; } }

    [SerializeField]
    private float zoomDuration = 1f;

    private Tween cameraRotationAnimation = null;
    private Tween cameraFOVAnimation = null;
    public void EndCameraAnimations()
    {
        cameraRotationAnimation.Kill();
        cameraFOVAnimation.Kill();
    }

    [SerializeField]
    private float cameraAnimationDuration;
    #endregion

    public static GameManager instance = null;  //  Amazing! This allows you to find and reference the Game Manager script from everywhere by typing GameManager.instance!!! 
    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);

        state = GameState.Sky;

        mouseSky.enabled = true;

        InitializeSky();

        RenderSettings.skybox.SetFloat("_AtmosphereThickness", 1f);

        starSize = referenceStarSize;

        field.ConfigureStarsTransform();

        DOTween.defaultEaseType = Ease.Linear;
    }

    void Start()
    {
        levels.LoadProgress();

        foreach (ConstellationLines otherLevel in levels.GetComponentsInChildren<ConstellationLines>())
        {
            otherLevel.FadeLines(fadeIn: true);
        }
    }

    #region Level Navigation

    public void SelectLevel(Transform constellation)
    {
        state = GameState.Level;
        level = constellation;

        mouseSky.enabled = false;
        skyViewMenu.gameObject.SetActive(false);

        ConstellationLines lines = level.GetComponent<ConstellationLines>();
        level.GetComponent<SphereCollider>().enabled = false;

        lines.ShowStages(true);

        //  Animate the camera view the constellation
        EndCameraAnimations();
        Vector3 targetRotation = Quaternion.LookRotation(constellation.position, constellation.up).eulerAngles;
        cameraRotationAnimation = cam.transform.DORotateQuaternion(Quaternion.Euler(targetRotation), zoomDuration);
        //  Zoom in
        cameraFOVAnimation = DOTween.To(x => cam.fieldOfView = x, cam.fieldOfView, lines.GetFOV(), zoomDuration);
        DOTween.To(x => starSize = x, starSize, referenceStarSize * lines.GetFOV() / skyViewFOV, zoomDuration);

        selectedLevelName.text = constellation.gameObject.name;

        levelSelectionCanvas.alpha = 0f;
        levelSelectionCanvas.gameObject.SetActive(true);
        levelSelectionCanvas.DOFade(1f, 0.5f);
    }
    public void SelectStage(StarSublevel selectedStage, bool alreadyCompleted)
    {
        state = GameState.Level;

        Sequence selectStageSequence = DOTween.Sequence();

        if (stage == null)
        {
            stageNameText.alpha = 0f;
            stageCompleteText.alpha = 0f;
            startStageButton.alpha = 0f;
            stageSelectionCanvas.gameObject.SetActive(true);
        }
        else if (selectedStage != stage) 
        {
            //  Animate out previously selected stage
            selectStageSequence.Append(stageNameText.DOFade(0f, 0.5f));
            selectStageSequence.Join(stageCompleteText.DOFade(0f, 0.5f));
        }

        stage = selectedStage;
        //  Animate in star name and completion status
        selectStageSequence.AppendCallback(() =>
        {
            stageNameText.text = selectedStage.name;
            stageCompleteText.text = alreadyCompleted ? "Cleared" : "";
        });
        selectStageSequence
            .Append(stageNameText.DOFade(1f, 0.5f))
            .Join(stageCompleteText.DOFade(1f, 0.5f))
            .Append(startStageButton.DOFade(1f, 1f));
    }

    public void StartLevel()
    {
        levelSelectionCanvas.DOFade(0f, 0.5f).OnComplete(() => levelSelectionCanvas.gameObject.SetActive(false));
        stageSelectionCanvas.DOFade(0f, 0.5f).OnComplete(() => stageSelectionCanvas.gameObject.SetActive(false));

        field.DimStarsOutside(level, true);

        ConstellationLines lines = level.GetComponent<ConstellationLines>();

        lines.ShowStages(false);

        foreach (ConstellationLines otherLevel in levels.GetComponentsInChildren<ConstellationLines>())
        {
            if (otherLevel != lines)
            {
                otherLevel.FadeLines(fadeIn: false);
            }
        }
        //  Adjust the stars to the squish factor of the selected constellation

        field.SquishStarsAround(level, true);

        player.GetComponent<Player>().ConfigureLevelCameraAround(level, stage);


        //minimap.SetMinimap(true);

    
        //  state = GameState.Play;     //  Delayed to when the camera finishes animating
    }

    public void LevelFailed()
    {
        levelFailedMenu.SetActive(true);

        //minimap.SetMinimap(false);
    }

    //sdffjhfsdsdfsdfjfdsdfsjdfsjkfdjkdsfkdfksjkdfsjkfdskjhfdslkjfhsadlkfjhaslkf hsalf kuhsefliuhvsaneli vae ashfcsam ucscfsfa csfc fsec ifec feilsfeailufseilsefalisflllefslfseclsfelseflililfslisfd
    public Ground ground;

    public void ReturnToLevel()
    {
        Sequence back = DOTween.Sequence()
            .Append(DOTween.To(x => circularFade.fadeRadius = x, 1f, 0f, 1f).OnStart(() =>
            {
                circularFade.ResetMaskCenter();
                circularFade.enabled = true;
            }))
            .AppendCallback(() =>
            {
                player.transform.position = Vector3.zero;
                player.transform.rotation = Quaternion.identity;
                cam.transform.position = Vector3.zero;
                cam.transform.rotation = Quaternion.LookRotation(level.position, level.up);

                ground.height = 1f;
                ground.gameObject.SetActive(true);

                RenderSettings.skybox.SetFloat("_AtmosphereThickness", 1f);
            })
            .Append(DOTween.To(x => circularFade.fadeRadius = x, 0f, 1f, 1f)
                .OnStart(() => circularFade.ResetMaskCenter())
                .OnComplete(() => circularFade.enabled = false))
            .AppendCallback(() =>
            {
                level.GetComponent<ConstellationLines>().ShowStages(true);

                foreach (ConstellationLines lev in levels.GetComponentsInChildren<ConstellationLines>())
                {
                    lev.FadeLines(fadeIn: true);
                }
            })
            .Append(levelSelectionCanvas.DOFade(1f, 0.5f)
                .OnStart(() =>
                {
                    selectedLevelName.text = level.gameObject.name;

                    levelSelectionCanvas.alpha = 0f;
                    levelSelectionCanvas.gameObject.SetActive(true);
                }))
            .Join(stageSelectionCanvas.DOFade(1f, 0.5f)
                .OnStart(() =>
                {
                    stageSelectionCanvas.alpha = 0f;
                    stageSelectionCanvas.gameObject.SetActive(true);

                    //SelectStage(stage, stage.Completed);
                }));

        state = GameState.Level;
    }

    public void LevelComplete()
    {
        state = GameState.Level;


        levelSelectionCanvas.alpha = 0f;
        levelSelectionCanvas.gameObject.SetActive(true);
        levelSelectionCanvas.DOFade(1f, 0.5f);

        stage.Completed = true;

        stageSelectionCanvas.gameObject.SetActive(true);
        stageCompleteText.text = "Cleared";
        Sequence selectStageSequence = DOTween.Sequence()
            .Append(stageNameText.DOFade(1f, 1f))
            .Join(startStageButton.DOFade(1f, 1f))
            .Append(stageCompleteText.DOFade(1f, 1f));

        level.GetComponent<ConstellationLines>().ShowStages(true);
        level.GetComponent<ConstellationLines>().UpdateLinesColors();

        foreach (ConstellationLines otherLevel in levels.GetComponentsInChildren<ConstellationLines>())
        {
            if (otherLevel != level)
            {
                otherLevel.FadeLines(fadeIn: true);
            }
        }

        //  Minimap fades out
        //minimap.SetMinimap(false);

        // autosave
        levels.SaveProgress();
    }

    public void ReturnToMainMenu()
    {
        level.GetComponent<SphereCollider>().enabled = true;

        //  Camera rotates...
        EndCameraAnimations();
        Vector3 targetRotation = cam.transform.rotation.eulerAngles;
        targetRotation.x = Mathf.Min(targetRotation.x - 360f, -mouseSky.MininumDeclination);
        targetRotation.z = 0f;
        cameraRotationAnimation = cam.transform.DORotateQuaternion(Quaternion.Euler(targetRotation), zoomDuration);
        //  ... and zooms out again to level selection
        cameraFOVAnimation = DOTween.To(x => cam.fieldOfView = x, cam.fieldOfView, skyViewFOV, zoomDuration);
        DOTween.To(x => starSize = x, starSize, referenceStarSize, zoomDuration);

        //  The dimmed stars return to regular brightness.
        field.DimStarsOutside(level, false);

        //  The stars' normal positions are loaded
        //  This doesn't have to be done when the function is called when a player is just looking at constellations, but it is currently.
        field.SquishStarsAround(level, false);

        level.GetComponent<ConstellationLines>().ShowStages(false);

        //mouseSky.SetTarget(constellatio)
        mouseSky.enabled = true;

        skyViewMenu.gameObject.SetActive(true);

        level = null;

        levelSelectionCanvas.DOFade(0f, 0.5f).OnComplete(() => levelSelectionCanvas.gameObject.SetActive(false));

        stage = null;


        Sequence selectStageSequence = DOTween.Sequence();
        stage = null;
        //  Animate out previously selected stage
        selectStageSequence
            .Append(stageNameText.DOFade(0f, 0.5f))
            .Join(stageCompleteText.DOFade(0f, 0.5f))
            .Join(startStageButton.DOFade(0f, 0.5f))
            .AppendCallback(() =>
            {
                stageSelectionCanvas.gameObject.SetActive(false);
            });

        state = GameState.Sky;
    }
    #endregion

    #region Sky orientation

    public Transform sky;
    public float latitude;
    public int hoursToRotate;


    public void InitializeSky()
    {
        //  The north pole is rotated along the y-z plane
        sky.rotation = Quaternion.AngleAxis(90f - latitude, Vector3.right);
        sky.Rotate(0f, -110f, 0f);
    }


    public void RotateSky(bool forward)
    {
        float rotationDegrees = (float)hoursToRotate * 360 / 24;
        rotationDegrees *= forward ? 1 : -1;
        Quaternion targetRotation = Quaternion.AngleAxis(rotationDegrees, sky.up) * sky.rotation;
        sky.DORotateQuaternion(targetRotation, 5f).SetEase(Ease.InOutSine);
    }

    #endregion
}
