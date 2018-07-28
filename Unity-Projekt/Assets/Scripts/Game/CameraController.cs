using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class CameraController : Singleton<CameraController> {

    // 0=birdview 1=shoulder
    public int cameraMode;
    private bool cameraModeChanging;

    private bool bDragging = false;
    private Vector3 oldPos, panOrigin, panTarget;
    private Quaternion oldRot;
    private float panSpeed = 2.5f, camerModeChangingTime = 0;

    [SerializeField]
    private Transform lookAt;
    private Vector3 currentLookAtOffset;
    public Vector3 lerplookAtPosition;
    public float lookAtRotation, lerpLookAtRotation;

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
        inputState = (UIManager.Instance.InMenu() || ChatManager.IsChatActive()) ? 0 : (BuildManager.placing) ? 1 : 2;
        bool inputEnabled = inputState > 0;

        dx = 0;
        dy = 0;

        PersonScript sp = PersonScript.FirstSelectedPerson();
        if(cameraMode == 1 && sp == null)
        {
            cameraModeChanging = true;
            cameraMode = 0;
        }

        if (inputEnabled)
        {
            float deltaAngle = 0f;
            if (Input.GetKey(KeyCode.E)) deltaAngle = -1;
            else if (Input.GetKey(KeyCode.Q)) deltaAngle = 1;
            lerpLookAtRotation += deltaAngle * rotateSpeed * Time.deltaTime;

            float scrollAmount = Input.GetAxis("Mouse ScrollWheel") * scrollSensitivity;
            if(invertedMousehweel) scrollAmount *= -1f;
            if(EventSystem.current.IsPointerOverGameObject()) scrollAmount = 0f;
            
            if (Input.GetKey(KeyCode.A)) dx = -1;
            else if (Input.GetKey(KeyCode.D)) dx = 1;
            else if (Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.D)) dx = 0;

            if (Input.GetKey(KeyCode.W)) dy = 1;
            else if (Input.GetKey(KeyCode.S)) dy = -1;
            else if (Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.S)) dy = 0;

            // additional camera movement when in bird view
            if (cameraMode == 0)
            {
                cameraDistance += scrollAmount * -1f;
                cameraDistance = Mathf.Clamp(cameraDistance, 2f, 12f);

                Vector3 mousePos = Input.mousePosition;
                float mouseMoveScreenPerc = 0.005f;
                if (mousePos.x < Screen.width * mouseMoveScreenPerc && mousePos.x >= 0) dx = -1;
                if (mousePos.x > Screen.width * (1f - mouseMoveScreenPerc) && mousePos.x <= Screen.width) dx = 1;
                if (mousePos.y < Screen.height * mouseMoveScreenPerc && mousePos.y >= 0) dy = -1;
                if (mousePos.y > Screen.height * (1f - mouseMoveScreenPerc) && mousePos.y <= Screen.height) dy = 1;
            }
            else if(cameraMode == 1)
            {
                sp.Move(dx, dy);
                sp.Rotate(deltaAngle);
            }

        }

        // scale movement relatively to camera distance
        Vector3 delta = new Vector3(dx, 0, dy) * keyMoveSpeed * Mathf.Pow(cameraDistance, 0.3f) * Time.deltaTime;

        if (inputEnabled)
        {
            // center camera around average position of selected people to make them all visible
            if (PersonScript.selectedPeople.Count > 0)
            {
                if(Input.GetKey(KeyCode.Space))
                    ZoomSelectedPeople();
                else if(Input.GetKeyDown(KeyCode.Z))
                {
                    cameraMode = 1 - cameraMode;
                    cameraModeChanging = true;
                    ZoomSelectedPeople();
                }
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
                //delta -=  new Vector3(newPos.x, 0, newPos.y);
                lerpLookAtRotation += Mathf.Clamp(newPos.x * 20f, -15f, 15f);
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

        // birds eye view
        if (cameraMode == 0)
        {
            delta = Quaternion.AngleAxis(transform.rotation.eulerAngles.y, Vector3.up) * delta;

            lookAtRotation = Mathf.Lerp(lookAtRotation, lerpLookAtRotation, Time.deltaTime * 5f);
            lerplookAtPosition += delta;
            lerplookAtPosition = Vector3.ClampMagnitude(lerplookAtPosition, Grid.WIDTH / 2);
            lookAt.position = Vector3.Lerp(lookAt.position, lerplookAtPosition, Time.deltaTime * 10f);

            Vector3 lookAtOffset = new Vector3(2 + 0.5f * cameraDistance, 0.5f + 1f * cameraDistance, 0);
            currentLookAtOffset = Vector3.Lerp(currentLookAtOffset, lookAtOffset, Time.deltaTime * 5f);
            Vector3 targetPos = lookAt.position + Quaternion.AngleAxis(lookAtRotation, Vector3.up) * currentLookAtOffset;
            if (cameraModeChanging)
            {
                transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime*5f);
                Quaternion targetRot = Quaternion.LookRotation(lookAt.position - transform.position);
                transform.rotation = Quaternion.Lerp(oldRot, targetRot, camerModeChangingTime);
                camerModeChangingTime += Time.deltaTime*3f;
                if (Vector3.Distance(transform.position, targetPos) <= 0.01f && Quaternion.Angle(transform.rotation,targetRot) <= 2f) cameraModeChanging = false;
            }
            else
            {
                transform.position = targetPos;
                transform.LookAt(lookAt);
            }

        }
        else if(cameraMode == 1)
        {
            if (cameraModeChanging)
            {
                Transform target = sp.GetShoulderCamPos();
                transform.position = Vector3.Lerp(transform.position, target.position, Time.deltaTime*5f);
                transform.rotation = Quaternion.Lerp(transform.rotation, target.rotation, Time.deltaTime*5f);

                oldRot = transform.rotation;
                camerModeChangingTime = 0;

                // zoom out on this person
                lookAt.position = target.position;
                lerpLookAtRotation = target.rotation.eulerAngles.y + 90;
                lookAtRotation = lerpLookAtRotation;
            }
        }
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

    public static void SetCameraData(GameData gd)
    {
        Instance.lerplookAtPosition = gd.GetPosition();
        Instance.lerpLookAtRotation = gd.cameraRotation;
        Instance.cameraDistance = gd.cameraDistance;
    }

    public static Transform LookAtTransform()
    {
        return Instance.lookAt;
    }
}
