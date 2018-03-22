using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CameraController : MonoBehaviour {


    bool bDragging = false;
    public static bool reCenter = false;
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

    //The selection square we draw when we drag the mouse to select units
    [SerializeField]
    private RectTransform selectionSquareImage;

    private Vector3 startPos, endPos;

    void Start()
    {
        selectionSquareImage.gameObject.SetActive(false);
    }

    void Update()
    {
        SelectUnits();

        inputState = VillageUIManager.Instance.InMenu() ? 0 : (VillageUIManager.Instance.GetBuildingMode() == 0) ? 1 : 2;
        bool inputEnabled = inputState > 0;

        float deltaAngle = 0f;
        if (Input.GetKey(KeyCode.E)) deltaAngle = -1;
        else if (Input.GetKey(KeyCode.Q)) deltaAngle = 1;
        lerpLookAtRotation += deltaAngle * rotateSpeed * Time.deltaTime;

        float scrollAmount = Input.GetAxis("Mouse ScrollWheel") * scrollSensitivity;
        //scrollAmount *= (cameraDistance * 0.3f);
        cameraDistance += scrollAmount * -1f;
        cameraDistance = Mathf.Clamp(cameraDistance, 0.1f, 15f);

        if (Input.GetKey(KeyCode.A)) dx = -1;
        else if (Input.GetKey(KeyCode.D)) dx = 1;
        else if (Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.D)) dx = 0;

        if (Input.GetKey(KeyCode.W)) dy = 1;
        else if (Input.GetKey(KeyCode.S)) dy = -1;
        else if (Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.S)) dy = 0;

        if (dx != 0 || dy != 0)
            reCenter = false;
        if (VillageUIManager.Instance.InMenu() && (dx != 0 || dy != 0))
        {
            VillageUIManager.Instance.ExitMenu();
        }

        Vector3 delta = new Vector3(dx, 0, dy) * keyMoveSpeed * Mathf.Pow(cameraDistance, 0.3f) * Time.deltaTime;

        if (VillageUIManager.Instance.GetSelectedBuilding() != null && VillageUIManager.Instance.InMenu() && reCenter)
        {
            lerplookAtPosition = VillageUIManager.Instance.GetSelectedBuilding().transform.position + Quaternion.AngleAxis(transform.rotation.eulerAngles.y, Vector3.up) * new Vector3(5 + cameraDistance, 0, 0);
        }
        if (inputEnabled)
        {
            if (Input.GetKey(KeyCode.Space))
            {
                if (VillageUIManager.Instance.GetSelectedPerson(0) != null)
                {
                    lerplookAtPosition = VillageUIManager.Instance.GetSelectedPerson(0).transform.position;
                }
            }

            if (Input.GetMouseButtonDown(2))
            {
                bDragging = true;
                panOrigin = Camera.main.ScreenToViewportPoint(Input.mousePosition); //Get the ScreenVector the mouse clicked
            }

            if (Input.GetMouseButton(2))
            {
                Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition) - panOrigin; //Get the difference between where the mouse clicked and where it moved
                Vector3 newPos = -pos * panSpeed; //Move the position of the camera to simulate a drag, speed * 10 for screen to worldspace conversion
                //newPos = Quaternion.Euler(0, 0, Camera.main.transform.localEulerAngles.y) * newPos;
                delta -=  new Vector3(newPos.x, 0, newPos.y);
                //panTarget = oldPos + new Vector3(newPos.x, 0, newPos.y);
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

        /*float scrollAmount = Input.GetAxis("Mouse ScrollWheel") * scrollSensitivity;
        scrollAmount *= (cameraDistance * 0.3f);

        cameraDistance += scrollAmount;

        Vector3 lookAtOffset = new Vector3(60, 40, 0);
        Vector3 v = Quaternion.AngleAxis(Time.time * -10, Vector3.up) * new Vector3(cameraDistance, 0, 0);
        transform.position = lookAt.position + v;

        transform.rotation = Quaternion.AngleAxis(Time.time * -10, Vector3.up);
        transform.position = lookAt.position + lookAtOffset;
        transform.position -= transform.rotation * Vector3.forward * cameraDistance;
        transform.position += transform.right * deltaAngle * Time.deltaTime;*/

        /*Vector3 lookAtOffset = new Vector3(60, 40, 0); 
        
        float keyMoveSpeed = 5f;
        if (Input.GetKey(KeyCode.A)) dx = -1;
        else if (Input.GetKey(KeyCode.D)) dx = 1;
        else if (Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.D)) dx = 0;

        if (Input.GetKey(KeyCode.W)) dy = 1;
        else if (Input.GetKey(KeyCode.S)) dy = -1;
        else if (Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.S)) dy = 0;

        float deltaAngle = 0f;
        if (Input.GetKey(KeyCode.E)) deltaAngle = 1;
        else if (Input.GetKey(KeyCode.Q)) deltaAngle = -1;
        rotationY += deltaAngle;

        Vector3 delta = new Vector3(dx, 0, dy) * keyMoveSpeed;
        delta = Quaternion.AngleAxis(transform.rotation.eulerAngles.y, Vector3.up) * delta;

        float fov = Camera.main.fieldOfView;
        if (inputEnabled) fov -= Input.GetAxis("Mouse ScrollWheel") * scrollSensitivity;
        fov = Mathf.Clamp(fov, minFov, maxFov);
        float tmp = (maxFov - fov) / (maxFov - minFov);

        lookAt.position += delta;

        transform.position = lookAt.position + Quaternion.AngleAxis(transform.rotation.eulerAngles.y, Vector3.up) * lookAtOffset;
        transform.LookAt(lookAt);
        //transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, rotationY, transform.rotation.eulerAngles.z);

        //lookAtOffset = Quaternion.AngleAxis(transform.rotation.eulerAngles.y, Vector3.up) * lookAtOffset;
        //transform.Translate(Vector3.right * Time.deltaTime * deltaAngle);

        /*transform.position = new Vector3(transform.position.x, 20, transform.position.z);

        if (inputEnabled)
        {
            if (Input.GetMouseButtonDown(2))
            {
                bDragging = true;
                oldPos = transform.position;
                panOrigin = Camera.main.ScreenToViewportPoint(Input.mousePosition); //Get the ScreenVector the mouse clicked
            }

            if (Input.GetMouseButton(2))
            {
                Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition) - panOrigin; //Get the difference between where the mouse clicked and where it moved
                Vector3 newPos = -pos * panSpeed; //Move the position of the camera to simulate a drag, speed * 10 for screen to worldspace conversion
                newPos = Quaternion.Euler(0, 0, -Camera.main.transform.localEulerAngles.y) * newPos;
                transform.position = oldPos + new Vector3(newPos.x, 0, newPos.y);
                //panTarget = oldPos + new Vector3(newPos.x, 0, newPos.y);
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
        //transform.position = Vector3.Lerp(transform.position, panTarget, 1f);

        float fov = Camera.main.fieldOfView;
        if (inputEnabled) fov -= Input.GetAxis("Mouse ScrollWheel") * scrollSensitivity; 
        fov = Mathf.Clamp(fov, minFov, maxFov);
        float tmp = (maxFov - fov) / (maxFov - minFov);
        transform.rotation = Quaternion.Euler(new Vector3(50 - tmp * 20, transform.rotation.eulerAngles.y, 0));

        float dx = 0, dy = 0;
        float keyMoveSpeed = 5f;
        if (Input.GetKey(KeyCode.A)) dx = -1;
        else if (Input.GetKey(KeyCode.D)) dx = 1;
        if (Input.GetKey(KeyCode.W)) dy = 1;
        else if (Input.GetKey(KeyCode.S)) dy = -1;
        Vector3 delta = new Vector3(dx, 0, dy) * keyMoveSpeed;
        delta = Quaternion.AngleAxis(transform.rotation.eulerAngles.y, Vector3.up) * delta;
        transform.position = new Vector3(transform.position.x, 100 - tmp * 20, transform.position.z) + delta;
        panSpeed = 100 - tmp*30f;
        Camera.main.fieldOfView = fov;*/
    }

    private void SelectUnits()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;

            startPos = Input.mousePosition;
            /*if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity))
            {
                startPos = hit.point;
            }*/
        }
        if (Input.GetMouseButtonUp(0))
        {
            Rect rect = new Rect(startPos, endPos - startPos);
            if (Mathf.Abs(rect.width * rect.height) > 100)
            {
                if (rect.width < 0) { rect.x = endPos.x; rect.width = -rect.width; }
                if (rect.height < 0) { rect.y = endPos.y; rect.height = -rect.height; }
                VillageUIManager.Instance.SelectPeople(rect);
            }
            selectionSquareImage.gameObject.SetActive(false);
        }
        if (Input.GetMouseButton(0))
        {
            if (!selectionSquareImage.gameObject.activeInHierarchy) selectionSquareImage.gameObject.SetActive(true);
            endPos = Input.mousePosition;

            Vector3 squareStart = startPos;//Camera.main.WorldToScreenPoint(startPos);

            Vector3 center = (squareStart + endPos) / 2f;

            selectionSquareImage.position = center;

            float sizeX = Mathf.Abs(squareStart.x - endPos.x);
            float sizeY = Mathf.Abs(squareStart.y - endPos.y);

            selectionSquareImage.sizeDelta = new Vector2(sizeX, sizeY);
        }
    }
}
