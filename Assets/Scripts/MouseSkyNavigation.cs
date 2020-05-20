using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseSkyNavigation : MonoBehaviour
{
    GameManager game;
    Camera mainCamera;

    Vector2 initialMouseDirection;
    Quaternion initialCameraRotation;

    private float xRotation;
    private float yRotation;
    [SerializeField]
    private Vector2 targetRotation = Vector2.zero;

    [SerializeField]
    private float mininumDeclination = 12.5f;
    public float MininumDeclination { get { return mininumDeclination; } }

    [SerializeField]
    private float maximumDeclination = 85f;

    Vector3 initialMousePosition;
    Vector2 deltaMousePosition;
    public float sensitivity;
    [SerializeField]
    private float dragSpeed = 10f;
    [SerializeField]
    private float fOVRelaxSpeed = 2f;
    public bool dragged = false;
    [SerializeField]
    private float squareDragThreshold;

    private bool clickedSinceEnabled = false;

    // Start is called before the first frame update
    void Start()
    {
        game = GameManager.instance;
        mainCamera = Camera.main;

        initialMousePosition = Input.mousePosition;
        squareDragThreshold = UnityEngine.EventSystems.EventSystem.current.pixelDragThreshold;
        squareDragThreshold *= squareDragThreshold;

        xRotation = -mininumDeclination;
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

            initialCameraRotation = mainCamera.transform.rotation;
            initialMousePosition = Input.mousePosition;
        }
        if (Input.GetMouseButton(0))
        {
            Vector3 deltaPosition = Input.mousePosition - initialMousePosition;
            if (deltaPosition.sqrMagnitude > squareDragThreshold)
            {
                dragged = true;
            }
            deltaPosition *= sensitivity;
            //  Euler angles are inconveniently disconinuous. As positive values decrease beyond 0, they jump to 360 to continue decreasing. Here, I'm changing the range from [0, 360] to [-180, 180], which makes the subsequent calculations possible.
            float deltaPitch = -deltaPosition.y;
            float initialPitch = initialCameraRotation.eulerAngles.x;
            initialPitch -= initialPitch > 180f ? 360f : 0f;

            //  The camera should not look further up than the 5 degrees from the zenith and further down than 5 degrees from the nadir.
            targetRotation.x = Mathf.Clamp(initialPitch - deltaPitch, -maximumDeclination, -mininumDeclination);

            targetRotation.y = initialCameraRotation.eulerAngles.y - deltaPosition.x;

        }
        if (Input.GetMouseButtonUp(0))
        {
            dragged = false;
        }

        if (clickedSinceEnabled)
        {
            mainCamera.transform.rotation = Quaternion.Lerp(mainCamera.transform.rotation, Quaternion.Euler(targetRotation), dragSpeed * Time.deltaTime);
            mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, game.SkyViewFOV, fOVRelaxSpeed * Time.deltaTime);
        }
    }

    void OnEnable()
    {
        clickedSinceEnabled = false;
    }

}
