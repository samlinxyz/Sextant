﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;
using DG.Tweening;
using UnityEngine.Windows.WebCam;
using UnityEngine.Rendering.PostProcessing;

public enum GameState
{
    Menu,   //  When the player is still at the main menu
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

    [SerializeField]
    private MainMenu mainMenu = null;

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
    [SerializeField]
    private float maxVisibleMagnitude = 6.5f;
    public float MaxVisibleMagnitude
    {
        get { return maxVisibleMagnitude; }
    }
    public Camera cam;

    public Transform player;

    public Minimap minimap;

    public StarFieldManager field;
    [SerializeField]
    private LineManager lineManager = null;

    public CircularFade circularFade;

    public Slider remainingFuel;

    public MouseSkyNavigation mouseSky;

    #region Selection Camera Animation Variables

    [SerializeField]
    private float zoomDuration = 1f;

    private Tween cameraRotationAndFOV = null;
    public void EndCameraAnimations()
    {
        cameraRotationAndFOV.Kill();
    }

    [SerializeField]
    private float cameraAnimationDuration;
    #endregion

    [SerializeField]
    private Transform skyTransform = null;

    public static GameManager instance = null;  //  Amazing! This allows you to find and reference the Game Manager script from everywhere by typing GameManager.instance!!! 
    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);

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

    public void GoToSky()
    {
        state = GameState.Sky;

        mouseSky.enabled = true;
    }

    public void StartTutorial()
    {
        Debug.LogError("StartTutorial() currently does nothing.");
    }

    public void SelectLevel(Transform constellation)
    {
        state = GameState.Level;
        level = constellation;

        mouseSky.enabled = false;

        ConstellationLines lines = level.GetComponent<ConstellationLines>();
        level.GetComponent<SphereCollider>().enabled = false;

        lines.ShowStages(true);

        //  Animate the camera view the constellation
        EndCameraAnimations();
        CalculateTargetEulersAndFOV(constellation.localPosition, lines.getFrame, out Vector3 targetEuler, out float fieldOfView);
        cameraRotationAndFOV = DOCameraRotationAndFOV(targetEuler, fieldOfView, zoomingIn: true);
        DOTween.To(x => starSize = x, starSize, (2f - fieldOfView / Settings.I.SkyViewFOV) * referenceStarSize * fieldOfView / Settings.I.SkyViewFOV, zoomDuration);
        
        selectedLevelName.text = constellation.gameObject.name;

        levelSelectionCanvas.alpha = 0f;
        levelSelectionCanvas.gameObject.SetActive(true);

        DOTween.Sequence()
            .Append(mainMenu.AnimateMenuButtonAlpha(fadeIn: false))
            .Append(levelSelectionCanvas.DOFade(1f, 0.5f));

    }

    private Tween DOCameraRotationAndFOV(Vector3 targetEuler, float fieldOfView, bool zoomingIn)
    {
        Vector3 initialEuler = cam.transform.rotation.eulerAngles;
        RegularizeTargetEuler(ref targetEuler, initialEuler);

        float pushback = 0.4f * Mathf.Clamp01(Vector3.Angle(Quaternion.Euler(targetEuler) * Vector3.forward, Quaternion.Euler(initialEuler) * Vector3.forward) / 90f);

        Sequence animation = DOTween.Sequence()
            .Append(RotateCameraXY(initialEuler, targetEuler, 0.7f * zoomDuration).SetEase(Ease.OutCubic))
            .Insert(pushback * zoomDuration, cam.DOFieldOfView(fieldOfView, (1f - pushback) * zoomDuration).SetEase(Ease.InOutCubic))
            .Insert(pushback * zoomDuration, RotateCameraZ(initialEuler.z, targetEuler.z, (1f-pushback) * zoomDuration).SetEase(Ease.InOutCubic));

        return animation;
    }

    private void RegularizeTargetEuler(ref Vector3 targetEuler, Vector3 initialEuler)
    {
        targetEuler.x -= (targetEuler.x - initialEuler.x > 180f) ? 360f : 0f;
        targetEuler.x += (targetEuler.x - initialEuler.x < -180f) ? 360f : 0f;
        targetEuler.y -= (targetEuler.y - initialEuler.y > 180f) ? 360f : 0f;
        targetEuler.y += (targetEuler.y - initialEuler.y < -180f) ? 360f : 0f;
        targetEuler.z -= (targetEuler.z - initialEuler.z > 180f) ? 360f : 0f;
        targetEuler.z += (targetEuler.z - initialEuler.z < -180f) ? 360f : 0f;
    }

    private void CalculateTargetEulersAndFOV(Vector3 constellationLocalPosition, ConstellationLines.Frame targetFrame, out Vector3 targetEuler, out float fieldOfView)
    {
        Quaternion localRotation = Quaternion.LookRotation(constellationLocalPosition);
        Quaternion targetRotation = skyTransform.localRotation * localRotation * Quaternion.Euler(0f, 0f, targetFrame.ZRotation);

        targetEuler = targetRotation.eulerAngles;
        //targetEuler.x -= targetEuler.x > 180 ? 360f : 0f; // Do not uncomment this line. It fucks with the manual euler angle rotation.

        // If the x euler angle is less than the angular radius of the constellation, rotate the sky and change the target rotation according to the rotated sky.
        if (targetEuler.x > 360f - targetFrame.FieldOfView / 2f || targetEuler.x < 180f || targetEuler.x > 360f - mouseSky.MininumDeclination)
        {
            Vector3 n = skyTransform.InverseTransformDirection(Vector3.up);
            float h = Mathf.Sin(Mathf.Deg2Rad * Mathf.Max(targetFrame.FieldOfView / 2f, mouseSky.MininumDeclination));
            float alpha = Mathf.Atan2(n.z, n.x);
            float localRotationEulerXRad = Mathf.Deg2Rad * localRotation.eulerAngles.x;
            float arcsinArgument = (h + Mathf.Sin(localRotationEulerXRad) * n.y) / Mathf.Cos(localRotationEulerXRad) / Mathf.Sqrt(n.x * n.x + n.z * n.z);
            if (arcsinArgument * arcsinArgument > 1f)
            {
                Debug.LogError("You just clicked on a constellation who will never get to right above the horizon. Prepare to die.");
            }
            
            float eulerY1 = Mathf.Rad2Deg * (Mathf.Asin(arcsinArgument) - alpha);
            eulerY1 %= 360f;
            eulerY1 += eulerY1 < 0f ? 360f : 0f;

            float eulerY2 = Mathf.Rad2Deg * (Mathf.PI - Mathf.Asin(arcsinArgument) - alpha);
            eulerY2 %= 360f;
            eulerY2 += eulerY2 < 0f ? 360f : 0f;

            float localY = localRotation.eulerAngles.y;

            float toY1 = Mathf.Abs(eulerY1 - localY);
            toY1 = toY1 > 180f ? 360f - toY1 : toY1;
            float toY2 = Mathf.Abs(eulerY2 - localY);
            toY2 = toY2 > 180f ? 360f - toY2 : toY2;

            float oneTrueEulerY = toY1 < toY2 ? eulerY1 : eulerY2;

            skyTransform.GetComponent<Sky>().RotateSky(oneTrueEulerY - localY);

            targetRotation = skyTransform.localRotation * Quaternion.Euler(localRotation.eulerAngles.x, oneTrueEulerY, 0f) * Quaternion.Euler(0f, 0f, targetFrame.ZRotation);
            targetEuler = targetRotation.eulerAngles;
        }

        if (targetFrame.IsotropicShape)
        {
            // If the constellation makes sense from any angle, don't rotate the camera.
            targetEuler.z = 0f;
        }
        else
        {
            targetEuler.z -= targetEuler.z > 180f ? 360f : 0f;
            if (Mathf.Abs(targetEuler.z) > 60f)
            {
                // If the constellation is more than 60 degrees from the angle with which it makes the most sense, rotate 75% of the way there.
                targetEuler.z *= 0.75f;
            }
            else
            {
                targetEuler.z = 0f;
            }
        }

        fieldOfView = targetFrame.FieldOfView;
    }

    private void CalculateTargetEulersAndFOV(Vector3 constellationLocalPosition, out Vector3 targetEuler, out float fieldOfView)
    {
        targetEuler = Quaternion.LookRotation(skyTransform.localRotation * constellationLocalPosition).eulerAngles;
        fieldOfView = Settings.I.SkyViewFOV; // This is bad code. you should not use an overload to have a different function.
    }

    private Tween RotateCameraXY(Vector2 initial, Vector2 target, float duration)
    {
        return DOTween.To(t =>
        {
            Vector2 currentXY = Vector2.Lerp(initial, target, t);
            Vector3 z = cam.transform.rotation.eulerAngles.z * Vector3.forward;
            cam.transform.rotation = Quaternion.Euler((Vector3)currentXY + z);
        },
        0f, 1f, duration);
    }
    private Tween RotateCameraZ(float initial, float target, float duration)
    {
        return DOTween.To(t =>
        {
            float currentZ = Mathf.Lerp(initial, target, t);
            Vector3 currentEuler = cam.transform.rotation.eulerAngles;
            currentEuler.z = currentZ;
            cam.transform.rotation = Quaternion.Euler(currentEuler);
        }, 
        0f, 1f, duration);
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
            stageCompleteText.text = alreadyCompleted ? "Cleared" : string.Empty;
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

        ConstellationLines lines = level.GetComponent<ConstellationLines>();

        field.DimStarsOutside(lines, true);

        lines.ShowStages(false);

        foreach (ConstellationLines otherLevel in levels.GetComponentsInChildren<ConstellationLines>())
        {
            if (otherLevel != lines)
            {
                otherLevel.FadeLines(fadeIn: false);
            }
        }
        //  Adjust the stars to the squish factor of the selected constellation

        field.SquishStarsAround(lines);

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
        lineManager.enabled = false;

        Sequence back = DOTween.Sequence()
            .Append(DOTween.To(x => circularFade.fadeRadius = x, 1f, 0f, 1f).OnStart(() =>
            {
                circularFade.ResetMaskCenter();
                circularFade.enabled = true;
            }))
            .AppendCallback(() =>
            {
                player.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                cam.transform.SetPositionAndRotation(Vector3.zero, Quaternion.LookRotation(level.position, level.up));

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


                    selectedLevelName.text = level.gameObject.name;

                    //levelSelectionCanvas.alpha = 0f;
                    levelSelectionCanvas.gameObject.SetActive(true);

                    stageSelectionCanvas.gameObject.SetActive(true);
            })
            .Append(levelSelectionCanvas.DOFade(1f, 0.5f))
            .Join(stageSelectionCanvas.DOFade(1f, 0.5f)
                .OnComplete(() =>
                {
                    SelectStage(stage, stage.Completed);
                }));

        state = GameState.Level;
    }

    public void LevelComplete()
    {
        state = GameState.Level;

        lineManager.enabled = false;

        levelSelectionCanvas.alpha = 0f;
        levelSelectionCanvas.gameObject.SetActive(true);
        levelSelectionCanvas.DOFade(1f, 0.5f);

        stage.Completed = true;

        stageSelectionCanvas.alpha = 1f;
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

        //  Camera rotates and zooms out again to level selection
        EndCameraAnimations();
        CalculateTargetEulersAndFOV(level.localPosition, out Vector3 targetEuler, out float fieldOfView);
        cameraRotationAndFOV = DOCameraRotationAndFOV(targetEuler, fieldOfView, zoomingIn: false);

        DOTween.To(x => starSize = x, starSize, referenceStarSize, zoomDuration);

        ConstellationLines lines = level.GetComponent<ConstellationLines>();

        //  The dimmed stars return to regular brightness.
        field.DimStarsOutside(lines, false);

        //  The stars' normal positions are loaded
        //  This doesn't have to be done when the function is called when a player is just looking at constellations, but it is currently.
        field.ConfigureStarsTransform();

        lines.ShowStages(false);

        //mouseSky.SetTarget(constellatio)
        mouseSky.enabled = true;


        level = null;

        levelSelectionCanvas.interactable = false;
        levelSelectionCanvas.blocksRaycasts = false;
        mainMenu.SetMenuButtonActiveAndInteractable(true);

        Sequence anim = DOTween.Sequence();
        anim.Append(levelSelectionCanvas.DOFade(0f, 0.5f).OnComplete(() =>
            {
                levelSelectionCanvas.interactable = true;
                levelSelectionCanvas.blocksRaycasts = true;
                levelSelectionCanvas.gameObject.SetActive(false);
            }))
            .AppendCallback(() =>
            {
                // If the user double clicked and is now in the menu, 
                // the menu button should not fade in.
                if (state != GameState.Menu) 
                {
                    mainMenu.AnimateMenuButtonAlpha(fadeIn: true);
                }
            });

        stage = null;

        Sequence selectStageSequence = DOTween.Sequence();
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

}
