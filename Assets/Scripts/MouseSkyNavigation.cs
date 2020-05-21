using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseSkyNavigation : MonoBehaviour
{
    GameManager game;
    Camera mainCamera;

    Vector3 initialCameraEulers;

    [SerializeField]
    private Vector2 targetRotation = Vector2.zero;

    [SerializeField]
    private float mininumDeclination = 12.5f;
    public float MininumDeclination { get { return mininumDeclination; } }

    [SerializeField]
    private float maximumDeclination = 85f;

    Vector3 initialMousePosition;
    public float sensitivity;
    [SerializeField]
    private float dragSpeed = 10f;
    [SerializeField]
    private float fOVRelaxSpeed = 2f;
    public bool dragged = false;
    [SerializeField]
    private float dragThreshold;

    private bool clickedSinceEnabled = false;

    [SerializeField]
    private float degreesToRotate = 0f;
    [SerializeField]
    private Transform skyTransform = null;
    private Quaternion initialSkyRotation;

    [SerializeField]
    private Quaternion targetSkyRotation;

    [SerializeField]
    private float skyRotationDirection = 0f;

    // Start is called before the first frame update
    void Start()
    {
        game = GameManager.instance;
        mainCamera = Camera.main;

        initialMousePosition = Input.mousePosition;
        dragThreshold = UnityEngine.EventSystems.EventSystem.current.pixelDragThreshold;
    }

    // Update is called once per frame
    void Update()
    {
        //  The following two if statements is the implementation of panning through mouse drag. 
        //  When the user clicks the screen, the camera orientation is captured along with the local space direction of the click (the direction of a hypothetical raycast).
        //  As the user moves the mouse, the pitch (x Euler angle) of the camera is made equal to the pitch required to bring the current local space direction to the initial mouse direction's declination. 
        //  The same is done for the camera's yaw (y Euler angle).

        if (Input.GetMouseButtonDown(0))
        {
            //  Stop animating the level transition?
            if (!clickedSinceEnabled)
            {
                game.EndCameraAnimations();
                clickedSinceEnabled = true;
            }

            initialCameraEulers = mainCamera.transform.rotation.eulerAngles;
            initialCameraEulers.x -= initialCameraEulers.x > 180f ? 360f : 0f;
            initialMousePosition = Input.mousePosition;
            skyRotationDirection = Mathf.Sign(mainCamera.ScreenToWorldPoint(initialMousePosition + Vector3.forward).x);

            initialSkyRotation = skyTransform.rotation;
            degreesToRotate = 0f;
        }
        if (Input.GetMouseButton(0))
        {
            Vector2 deltaPosition = Input.mousePosition - initialMousePosition;
            if (deltaPosition.sqrMagnitude > dragThreshold * dragThreshold)
            {
                dragged = true;
            }
            Vector2 deltaRotation = deltaPosition * sensitivity;

            float finalPitchVirtual = initialCameraEulers.x + deltaRotation.y;

            //  The camera's declination is constrained.
            targetRotation.x = Mathf.Clamp(finalPitchVirtual, -maximumDeclination, -mininumDeclination);

            //  If the user intends to drag the camera below the minimum declination, the sky will rotate to reveal more sky in that direction.
            float pitchOverflow = finalPitchVirtual - targetRotation.x;
            degreesToRotate += skyRotationDirection * pitchOverflow;
            targetSkyRotation = initialSkyRotation * Quaternion.Euler(0f, degreesToRotate, 0f);

            //  If the user drags downward after rotating the sky, this will allow the user to immediately pan.
            initialCameraEulers.x -= pitchOverflow;

            targetRotation.y = initialCameraEulers.y - deltaRotation.x;
        }
        if (Input.GetMouseButtonUp(0))
        {
            dragged = false;
        }

        if (clickedSinceEnabled)
        {
            mainCamera.transform.rotation = Quaternion.Slerp(mainCamera.transform.rotation, Quaternion.Euler(targetRotation), dragSpeed * Time.deltaTime);
            mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, game.SkyViewFOV, fOVRelaxSpeed * Time.deltaTime);

            skyTransform.rotation = Quaternion.Slerp(skyTransform.rotation, targetSkyRotation, dragSpeed * Time.deltaTime);
        }
    }

    void OnEnable()
    {
        clickedSinceEnabled = false;
    }

}
