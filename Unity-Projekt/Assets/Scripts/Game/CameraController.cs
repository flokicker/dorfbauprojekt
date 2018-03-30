using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CameraController : MonoBehaviour {


    bool bDragging = false;
    Vector3 oldPos, panOrigin, panTarget;
    float panSpeed = 2.5f;

    [SerializeField]
    private Transform lookAt;
    private Vector3 currentLookAtOffset;
    private Vector3 lerplookAtPosition;
    private float lookAtRotation, lerpLookAtRotation;

    private float cameraDistance = 1f;
    private float scrollSensitivity = 10f;
    private float keyMoveSpeed = 5.0f, rotateSpeed = 80.0f;

    private float dx = 0, dy = 0;

    public static int inputState;

    // invert mousewheel direction
    private bool invertedMousehweel;

    void Start()
    {
        invertedMousehweel = PlayerPrefs.GetInt("InvertedMousehweel") == 1;
    }

    void Update()
    {
        inputState = VillageUIManager.Instance.InMenu() ? 0 : (VillageUIManager.Instance.GetBuildingMode() == 0) ? 1 : 2;
        bool inputEnabled = inputState > 0;

        float deltaAngle = 0f;
        if (Input.GetKey(KeyCode.E)) deltaAngle = -1;
        else if (Input.GetKey(KeyCode.Q)) deltaAngle = 1;
        lerpLookAtRotation += deltaAngle * rotateSpeed * Time.deltaTime;

        float scrollAmount = Input.GetAxis("Mouse ScrollWheel") * scrollSensitivity;
        if(invertedMousehweel) scrollAmount *= -1f;
        cameraDistance += scrollAmount * -1f;
        cameraDistance = Mathf.Clamp(cameraDistance, 0.1f, 15f);

        if (Input.GetKey(KeyCode.A)) dx = -1;
        else if (Input.GetKey(KeyCode.D)) dx = 1;
        else if (Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.D)) dx = 0;

        if (Input.GetKey(KeyCode.W)) dy = 1;
        else if (Input.GetKey(KeyCode.S)) dy = -1;
        else if (Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.S)) dy = 0;


        Vector3 delta = new Vector3(dx, 0, dy) * keyMoveSpeed * Mathf.Pow(cameraDistance, 0.3f) * Time.deltaTime;

        if (inputEnabled)
        {
            if (Input.GetKey(KeyCode.Space))
            {
                /* TODO: implement average camera pos + zoom*/
                
                /*if (VillageUIManager.Instance.GetSelectedPerson(0) != null)
                {
                    lerplookAtPosition = VillageUIManager.Instance.GetSelectedPerson(0).transform.position;
                }*/
            }

            if (Input.GetMouseButtonDown(2))
            {
                bDragging = true;
                panOrigin = Camera.main.ScreenToViewportPoint(Input.mousePosition); //Get the ScreenVector the mouse clicked
            }

            if (Input.GetMouseButton(2))
            {
                // Get the difference between where the mouse clicked and where it moved
                Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition) - panOrigin; 
                // Move the position of the camera to simulate a drag, speed * 10 for screen to worldspace conversion
                Vector3 newPos = -pos * panSpeed;
                delta -=  new Vector3(newPos.x, 0, newPos.y);
            }

            if (Input.GetMouseButtonUp(2))
            {
                bDragging = false;
            }
        }
        else
        {
            bDragging = false;
        }

        delta = Quaternion.AngleAxis(transform.rotation.eulerAngles.y, Vector3.up) * delta;

        lookAtRotation = Mathf.Lerp(lookAtRotation, lerpLookAtRotation, Time.deltaTime * 5f);
        lerplookAtPosition += delta;
        lookAt.position = Vector3.Lerp(lookAt.position, lerplookAtPosition, Time.deltaTime * 10f);

        Vector3 lookAtOffset = new Vector3(2 + 0.5f * cameraDistance, 0.5f + 1f * cameraDistance, 0);
        currentLookAtOffset = Vector3.Lerp(currentLookAtOffset, lookAtOffset, Time.deltaTime * 5f);
        transform.position = lookAt.position + Quaternion.AngleAxis(lookAtRotation, Vector3.up) * currentLookAtOffset;
        transform.LookAt(lookAt);
    }

    // set inverted mousewheel setting
    public void SetInvertedMousewheel(bool inverted)
    {
        invertedMousehweel = inverted;
    }
}
