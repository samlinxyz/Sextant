using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Player : MonoBehaviour
{

    public GameManager game;
    public Camera mainCamera;

    public Transform constellation;

    public Minimap minimap;

    public Vector3 upAxis;

    public Transform testCube;

    public Slider fuelSlider;
    public Transform spacecraft;
    public float spacecraftDistance;
    public Ground ground;
    public Transform glow;

    public CanvasGroup joysticks;
    public Joystick movementControl;
    public Joystick parallaxControl;

    public bool aimCamera = false;

    private bool onDesktop;

    // Start is called before the first frame update
    void Start()
    {
        game = GameManager.instance;
        mainCamera = Camera.main;

        onDesktop = SystemInfo.deviceType == DeviceType.Desktop;
    }

    void Update()
    {

        Move();
        Parallax();
        if (aimCamera) mainCamera.transform.LookAt(transform.position, transform.up);
        if (Input.GetKeyDown("-") && game.state == GameState.Play)
        {
            FailLevel();
        }
    }

    public static bool allowMovement = false;
    public float speed;
    public void Move()
    {
        if (!allowMovement) return;

        Vector3 move = movementControl.Direction;

        float angle = move.sqrMagnitude * speed * Time.deltaTime;
        Vector3 rotationAxis = transform.TransformDirection(Vector3.Cross(move, Vector3.forward)).normalized;
        transform.rotation = Quaternion.AngleAxis(angle, rotationAxis) * transform.rotation;

        fuelSlider.value -= Mathf.Deg2Rad * angle;
        if (fuelSlider.value == 0f) FailLevel();
    }

    public float parallaxRange;
    public float parallaxSpeed;
    public void Parallax()
    {
        Vector2 parallax;
        if (onDesktop)
        {
            float right = Input.GetAxisRaw("Horizontal");
            float up = Input.GetAxisRaw("Vertical");
            parallax = right * Vector2.right + up * Vector2.up;
            if (right != 0f && up != 0)
            {
                parallax /= 1.41421356237f;
            }
        }
        else
        {
            parallax = parallaxControl.Direction;
        }
        Vector2 target = Vector3.Lerp((Vector2)mainCamera.transform.localPosition, allowMovement ? parallaxRange * parallax : Vector2.zero, Time.deltaTime * parallaxSpeed);
        mainCamera.transform.localPosition = new Vector3(target.x, target.y, mainCamera.transform.localPosition.z);
    }

    public void CompleteLevel()
    {
        Sequence completeLevelAnimation = DOTween.Sequence();
        completeLevelAnimation
            .AppendCallback(() =>
            {
                allowMovement = false;
            })
            .Append(transform.DORotateQuaternion(Quaternion.LookRotation(constellation.position, transform.up), 2f).SetEase(Ease.OutCubic))
            .Join(joysticks.DOFade(0f, 1f).SetEase(Ease.InQuart).OnComplete(() => joysticks.gameObject.SetActive(false)))
            .Join(glow.DOScale(0.05f, 2f).SetEase(Ease.InOutQuad))
            .Append(DOTween.To(x => RenderSettings.skybox.SetFloat("_AtmosphereThickness", x), 0f, 1f, 5f).SetEase(Ease.OutQuad))
            .Join(spacecraft.DOMove(3f * transform.forward + 5f * Vector3.down, 2f).SetEase(Ease.InCirc))
            .Join(DOTween.To(x => { ground.height = x; }, 100f, 1f, 4f).SetEase(Ease.OutCirc).OnPlay(() => { ground.gameObject.SetActive(true); }))
            .AppendCallback(() =>
            {
                aimCamera = false;
                //  The level is finished.
                game.LevelComplete();
                Debug.Log("The level is finished");
            })
            ;
    }

    public void FailLevel()
    {

        allowMovement = false;
        joysticks.DOFade(0f, 1f).SetEase(Ease.InQuart).OnComplete(() => joysticks.gameObject.SetActive(false));

        game.LevelFailed();

        StopAllCoroutines();
    }


    #region Z-Axis Rotation
    public float zRotationSpeed;
    public GameObject rotationButtons;
    public void RotateZ(bool clockwise) { transform.Rotate(0f, 0f, (clockwise ? 1f : -1f) * Time.deltaTime * zRotationSpeed); }

    #endregion


    public void ConfigureLevelCameraAround(Transform selectedConstellation)
    {
        constellation = selectedConstellation;

        Vector3 cameraPosition = mainCamera.transform.position;
        Quaternion cameraRotation = mainCamera.transform.rotation;
        transform.position = constellation.position;
        transform.rotation = constellation.rotation;
        mainCamera.transform.position = cameraPosition;
        mainCamera.transform.rotation = cameraRotation;

        upAxis = Quaternion.AngleAxis(Random.Range(-75f, 75f), constellation.right) * constellation.up;

        //  The direction of view that we start with is more than 30 degrees from the axis of the level and also more than 30 degrees from the initial camera view.
        Vector3 view = upAxis;
        while (Mathf.Abs(Vector3.Angle(view, upAxis) - 90f) > 60f || Vector3.Angle(view, constellation.position) < 30f)
        {
            view = Random.onUnitSphere;
            view = Vector3.ProjectOnPlane(view, upAxis).normalized;
        }

        //  Go to the randomly generated place where you start your level.
        transform.position = constellation.position - constellation.position.magnitude * view;
        transform.LookAt(constellation, upAxis);



        testCube.rotation = Quaternion.LookRotation(upAxis);
    }

    //  This one is for if you're starting with a star
    public void ConfigureLevelCameraAround(Transform selectedConstellation, StarSublevel stage)
    {




        constellation = selectedConstellation;

        //  Fuel alloted is the difficulty plus the minimum amount of fuel required to get home.
        float degreesAway = Vector3.Angle(-selectedConstellation.position, stage.AssociatedStar.transform.position - selectedConstellation.position);
        if (degreesAway < 30f) { degreesAway = 180f - degreesAway; }
        fuelSlider.maxValue += Mathf.Deg2Rad * degreesAway + stage.Difficulty;
        fuelSlider.value = fuelSlider.maxValue;

        upAxis = Quaternion.AngleAxis(Random.Range(-75f, 75f), constellation.right) * constellation.up;

        //  The direction of view that we start with is more than 30 degrees from the axis of the level and also more than 30 degrees from the initial camera view.
        Vector3 view = constellation.position - stage.AssociatedStar.transform.position;

        // logs a warning if the starting position is aligned to the constellation up axis within 30 degrees
        if (Mathf.Abs(Vector3.Angle(view, upAxis) - 90f) > 60f) Debug.Log("Warning: the starlevel's direction is only " + (90f - Mathf.Abs(Vector3.Angle(view, upAxis) - 90f)));

        //  The spacecraft rises into the upper atmosphere and zooms off at the star. The player is then transported to the starting location and zooms out.
        spacecraft.rotation = mainCamera.transform.rotation;
        spacecraft.position = 1f * spacecraft.forward + 5f * Vector3.down;
        glow.localScale = 0.5f * Vector3.one;

        CircularFade circularFade = mainCamera.GetComponent<CircularFade>();

        Sequence transition = DOTween.Sequence();
        transition
            .Append(DOTween.To(x => circularFade.fadeRadius = x, 1f, 0f, 1f).OnStart(() =>
            {
                circularFade.MaskCenterWorld = stage.AssociatedStar.transform.position;
                circularFade.enabled = true;
            }))
            .AppendCallback(() =>
             {
                 //  Go to the star where you start your level, then animate outward.
                 transform.position = constellation.position;
                 transform.rotation = Quaternion.LookRotation(view, upAxis);
                 mainCamera.transform.localPosition = (-2f - view.magnitude) * Vector3.forward;
                 mainCamera.transform.localRotation = Quaternion.identity;
                 spacecraft.position = mainCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.25f, spacecraftDistance));
                 spacecraft.localRotation = Quaternion.identity;
                 spacecraft.localScale = 0.05f * Vector3.one;
                 glow.localScale = 100f * Vector3.one;

                 //  this is why this should be in gamemanager
                 game.level.GetComponent<ConstellationLines>().SetAlpha(0f);
             })
            .Append(DOTween.To(x => circularFade.fadeRadius = x, 0f, 1f, 1f)
                .OnStart(() => circularFade.ResetMaskCenter())
                .OnComplete(() => circularFade.enabled = false));

        Sequence spacecraftAnimation = DOTween.Sequence();
        spacecraftAnimation
            .AppendCallback(() => {game.state = GameState.Play; }) // to disable clicking to star a star sublevel.
            .Append(DOTween.To(x => RenderSettings.skybox.SetFloat("_AtmosphereThickness", x), 1f, 0f, 5f).SetEase(Ease.OutQuad))
            .Join(DOTween.To(x => { ground.height = x; }, 1f, 100f, 4f).SetEase(Ease.InCirc).OnComplete(() => { ground.gameObject.SetActive(false); }))
            .Join(spacecraft.DOMove(mainCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.25f, 1f)), 2.5f).SetEase(Ease.OutQuad))
            .Insert(2.5f, spacecraft.DOMove(stage.AssociatedStar.transform.position, 2f).SetEase(Ease.InQuint))
            .Append(transition)
            .Append(mainCamera.transform.DOLocalMoveZ(-(constellation.position.magnitude), 5f).SetEase(Ease.InOutQuart))
            .Join(spacecraft.transform.DOLocalMoveZ(spacecraftDistance - constellation.position.magnitude, 5f).SetEase(Ease.InOutQuart))
            .AppendCallback(() =>
            {
                //  Things to take care of as animation ends.
                Debug.Log("start Parallax");

                //testCube.rotation = Quaternion.LookRotation(upAxis);
                allowMovement = true;
                aimCamera = true;

                ConfigureControls(SystemInfo.deviceType == DeviceType.Handheld);
                joysticks.gameObject.SetActive(true);
            })
            .Append(joysticks.DOFade(1f, 1f).SetEase(Ease.OutQuart))
            ;
          
    }

    public void ConfigureControls(bool forMobile)
    {

        if (forMobile)
        {
            movementControl.GetComponent<RectTransform>().anchoredPosition = -150f * Vector2.right;
            parallaxControl.GetComponent<RectTransform>().anchoredPosition = new Vector2(175f, -50f);
        }
        else
        {
            movementControl.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            parallaxControl.GetComponent<RectTransform>().anchoredPosition = Vector2.positiveInfinity;
        }
    }
}
