using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class CameraController : Singleton<CameraController> {

    bool bDragging = false;
    Vector3 oldPos, panOrigin, panTarget;
    float panSpeed = 2.5f;

    [SerializeField]
    private Transform lookAt;
    private Vector3 currentLookAtOffset;
    public Vector3 lerplookAtPosition;
    private float lookAtRotation, lerpLookAtRotation;

    public float cameraDistance = 1f;
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
        inputState = VillageUIManager.Instance.InMenu() ? 0 : (BuildManager.placing) ? 1 : 2;
        bool inputEnabled = inputState > 0;

        float deltaAngle = 0f;
        if (Input.GetKey(KeyCode.E)) deltaAngle = -1;
        else if (Input.GetKey(KeyCode.Q)) deltaAngle = 1;
        lerpLookAtRotation += deltaAngle * rotateSpeed * Time.deltaTime;

        float scrollAmount = Input.GetAxis("Mouse ScrollWheel") * scrollSensitivity;
        if(invertedMousehweel) scrollAmount *= -1f;
        if(EventSystem.current.IsPointerOverGameObject()) scrollAmount = 0f;
        
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
            // center camera around average position of selected people to make them all visible
            if (Input.GetKey(KeyCode.Space) && PersonScript.selectedPeople.Count > 0)
            {
                ZoomSelectedPeople();
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
        lerplookAtPosition = Vector3.ClampMagnitude(lerplookAtPosition, Grid.WIDTH/2);
        lookAt.position = Vector3.Lerp(lookAt.position, lerplookAtPosition, Time.deltaTime * 10f);

        Vector3 lookAtOffset = new Vector3(2 + 0.5f * cameraDistance, 0.5f + 1f * cameraDistance, 0);
        currentLookAtOffset = Vector3.Lerp(currentLookAtOffset, lookAtOffset, Time.deltaTime * 5f);
        transform.position = lookAt.position + Quaternion.AngleAxis(lookAtRotation, Vector3.up) * currentLookAtOffset;
        transform.LookAt(lookAt);
    }

    public static void ZoomSelectedPeople()
    {
        Vector3 averagePos = Vector3.zero;
            foreach(PersonScript ps in PersonScript.selectedPeople)
            {
                averagePos += ps.transform.position;
            }
            averagePos /= PersonScript.selectedPeople.Count;
            Instance.lerplookAtPosition = averagePos;

            // get maximum distance from averageposition
            float maxDist = 0f;
            foreach(PersonScript ps in PersonScript.selectedPeople)
            {
                float d = Vector3.Distance(ps.transform.position, averagePos);
                if(d > maxDist)
                    maxDist = d;
            }

            // make sure they are both visible on the screen by zooming appropriately
            float minScreenSize = Screen.height;
            Instance.cameraDistance = Mathf.Max(maxDist / minScreenSize * 1000f, 1);
    }

    // set inverted mousewheel setting
    public void SetInvertedMousewheel(bool inverted)
    {
        invertedMousehweel = inverted;
    }
}
