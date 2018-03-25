﻿using UnityEngine;
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

    // the selection square we draw when we drag the mouse to select units
    [SerializeField]
    private RectTransform selectionSquareImage;
    private Rect selection = new Rect();
    private Vector3 startPos, endPos;

    // invert mousewheel direction
    private bool invertedMousehweel;

    void Start()
    {
        selectionSquareImage.gameObject.SetActive(false);
        invertedMousehweel = PlayerPrefs.GetInt("InvertedMousehweel") == 1;
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
        if(invertedMousehweel) scrollAmount *= -1f;
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
                Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition) - panOrigin; //Get the difference between where the mouse clicked and where it moved
                Vector3 newPos = -pos * panSpeed; //Move the position of the camera to simulate a drag, speed * 10 for screen to worldspace conversion
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

    // update selection
    private void SelectUnits()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // set start position of drag
            startPos = Input.mousePosition;
        }
        if (Input.GetMouseButtonUp(0))
        {
            // if selection box is too small, just select highlighted person
            if(selection.width * selection.height >= 100)
            {
                SelectPeople(selection);
            }
            else
            {
                if(!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))
                    PersonScript.DeselectAll();
                foreach(PersonScript ps in PersonScript.allPeople)
                {
                    if(ps.highlighted) ps.OnClick();
                }
            }
            

            // make the selectio nsquare invisible
            selectionSquareImage.gameObject.SetActive(false);
        }
        if (Input.GetMouseButton(0))
        {
            // make the selectio nsquare visible
            if (!selectionSquareImage.gameObject.activeInHierarchy) selectionSquareImage.gameObject.SetActive(true);
            endPos = Input.mousePosition;
            
            // make sure the rect has non-negative size
            selection.xMin = startPos.x < endPos.x ? startPos.x : endPos.x;
            selection.xMax = startPos.x < endPos.x ? endPos.x : startPos.x;
            selection.yMin = startPos.y < endPos.y ? startPos.y : endPos.y;
            selection.yMax = startPos.y < endPos.y ? endPos.y : startPos.y;

            // set the selection square rectTransform
            selectionSquareImage.offsetMin = selection.min;
            selectionSquareImage.offsetMax = selection.max;
        }
    }
    
    public void SelectPeople(Rect selection)
    {
        if(!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))
            PersonScript.DeselectAll();
        foreach (PersonScript ps in PersonScript.allPeople)
        {
            Vector3 pos = Camera.main.WorldToScreenPoint(ps.transform.position);
            if (selection.Contains(new Vector2(pos.x, pos.y)))
            {
                ps.OnSelect();
            }
        }
    }
}
