using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MouseManager : Singleton<MouseManager> {
	
	// Wether anything has already been done with the current mouse click
	public static bool LeftClickHandled, RightClickHandled;

	// A reference to the UIManager
	private static VillageUIManager uiManager;

    // the selection square we draw when we drag the mouse to select units
    [SerializeField]
    private RectTransform selectionBoxImage;
    private Rect selection = new Rect();
    private Vector3 startPos, endPos;
	private bool dragging;

	void Start () 
	{
		LeftClickHandled = false;
		RightClickHandled = false;
		
		uiManager = VillageUIManager.Instance;

        selectionBoxImage.gameObject.SetActive(false);
	}
	
	void Update () 
	{
		uiManager = VillageUIManager.Instance;
	}

	// if click is not already handled, select units
	void LateUpdate()
	{
		SelectUnits();

		LeftClickHandled = false;
		RightClickHandled = false;
	}

	public static void MouseOverClickableObject(Transform script)
	{
		bool leftClick = Input.GetMouseButton(0);
		bool rightClick = Input.GetMouseButton(1);

		if(rightClick && !RightClickHandled)
		{
			RightClickHandled = true;

			/* TODO: handle building */

			/* TODO: handle  */
		}

		// Handle UI Hover and Click stuff
        if (leftClick && !LeftClickHandled) {
            uiManager.OnShowObjectInfo(script);
			LeftClickHandled = true;
		}
        else {
            uiManager.OnShowSmallObjectInfo(script);
		}
	}

	public static void MouseExitClickableObject(Transform script)
	{
		// Hide UI info on object
		uiManager.OnHideSmallObjectInfo();
	}
	

    // update selection
    private void SelectUnits()
    {
        // only update if not building and pointer is not over a UI Element
        if(VillageUIManager.Instance.GetBuildingMode() != -1 ||
			EventSystem.current.IsPointerOverGameObject())
		{
			LeftClickHandled = true;
		}

        if (Input.GetMouseButtonDown(0) && !LeftClickHandled)
        {
			// mouse click is handled
			LeftClickHandled = true;

			dragging = true;

            // set start position of drag
            startPos = Input.mousePosition;
        }
        if (Input.GetMouseButtonUp(0) && dragging)
        {
			dragging = false;

            // if selection box is too small, just select highlighted person
            if(selection.width * selection.height >= 100)
            {
                SelectPeople(selection);
            }
            else
            {
                // only deselect units if shift-key is not hold
                if(!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))
                    PersonScript.DeselectAll();
                foreach(PersonScript ps in PersonScript.allPeople)
                {
                    if(ps.highlighted) ps.OnClick();
                }
            }
            

            // make the selectio nsquare invisible
            selectionBoxImage.gameObject.SetActive(false);
        }
        if (Input.GetMouseButton(0) && dragging)
        {
            // make the selection square visible
            if (!selectionBoxImage.gameObject.activeInHierarchy) selectionBoxImage.gameObject.SetActive(true);
            endPos = Input.mousePosition;
            
            // make sure the rect has non-negative size
            selection.xMin = startPos.x < endPos.x ? startPos.x : endPos.x;
            selection.xMax = startPos.x < endPos.x ? endPos.x : startPos.x;
            selection.yMin = startPos.y < endPos.y ? startPos.y : endPos.y;
            selection.yMax = startPos.y < endPos.y ? endPos.y : startPos.y;

            // set the selection square rectTransform
            selectionBoxImage.offsetMin = selection.min;
            selectionBoxImage.offsetMax = selection.max;
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
