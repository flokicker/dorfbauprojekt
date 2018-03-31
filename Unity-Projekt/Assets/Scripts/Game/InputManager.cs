using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : Singleton<InputManager> {
	
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

        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if(BuildManager.placing)
                BuildManager.EndPlacing();
        }

		HandleTerrainClick();
        HandleBuildClick();
	}

    public static bool InputUI()
    {
        return !BuildManager.placing && !uiManager.InMenu();
    }

	// Returns if a raycast sent from the camera to the mouse position hits an object
	private bool MouseRaycastHit(out RaycastHit hit)
	{
		Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
		return Physics.Raycast(mouseRay, out hit, 1000);
	}

	private void WalkSelectedPeopleTo(Node targetNode, Vector3 clickPos)
	{
		if (PersonScript.selectedPeople.Count == 0) return;

		List<Vector2> delta = new List<Vector2>();
		for (float r = 0; r < 10; r+=0.5f)
		{
			for (int x = (int)-r; x <= r; x++)
			{
				for (int y = (int)-r; y <= r; y++)
				{
					if(x*x + y*y <= r*r && !delta.Contains(new Vector2(x,y)))
						delta.Add(new Vector2(x, y));
				}
			}
		}
		int ind = 0;
		int newX, newY;
		Vector3 targetPos;
		foreach (PersonScript ps in PersonScript.selectedPeople)
		{
			// dont move inactive people
			if (!ps || !ps.gameObject.activeSelf) continue;

			// Find a node to put this person on
			do {
				newX = targetNode.gridX + (int)delta[ind].x;
				newY = targetNode.gridY + (int)delta[ind].y;
			} while((++ind) < delta.Count && (!Grid.ValidNode(newX, newY) ||Grid.Occupied(newX, newY)));
			
			// no more available space
			if(ind == delta.Count) break;

			targetPos =	clickPos + new Vector3(newX-targetNode.gridX, 0, newY-targetNode.gridY) * Grid.SCALE;

			if(targetNode.nodeObject) ps.SetTargetTransform(targetNode.nodeObject, targetPos);
			else {
				ps.SetTargetPosition(targetPos);
			}

			//if (target == 1 && Grid.ToGrid(ps.transform.position) != Grid.ToGrid(targetPos) + new Vector3(delta[ind].x, 0, delta[ind].y)) ps.SetTargetPosition(targetPos + new Vector3(delta[ind].x, 0, delta[ind].y) * Grid.SCALE);
			//else if (target == 2/* && Grid.ToGrid(ps.transform.position) != Grid.ToGrid(targetTr.position) + new Vector3(delta[ind].x, 0, delta[ind].y)*/) ps.SetTargetTransform(targetTr, targetTr.position + new Vector3(delta[ind].x, 0, delta[ind].y) * Grid.SCALE);
			ind++;
		}
	}

    private void TargetSelectedPeopleTo(Transform target)
    {
		foreach (PersonScript ps in PersonScript.selectedPeople)
		{
			// dont move inactive people
			if (!ps || !ps.gameObject.activeSelf) continue;

            ps.SetTargetTransform(target);
        }
    }

	// Handles all clicks on the terrain, e.g. movement and deselection
	private void HandleTerrainClick()
	{
        if(EventSystem.current.IsPointerOverGameObject() || BuildManager.placing) return;

        if (Input.GetMouseButtonDown(1))
        {
            RaycastHit hit;
            if(MouseRaycastHit(out hit))
            {
                string tag = hit.transform.gameObject.tag;
                if (tag == "Terrain")
                {
                    Vector3 hitGrid = Grid.ToGrid(hit.point);
                    Node hitNode = Grid.GetNode((int)hitGrid.x, (int)hitGrid.z);
                    WalkSelectedPeopleTo(hitNode, hit.point);
                    RightClickHandled = true;
                }
            }
        }
	}

    // Handles click of placing a building
    private void HandleBuildClick()
    {
        if(!BuildManager.placing) return;
        if(EventSystem.current.IsPointerOverGameObject()) return;

        if (Input.GetMouseButtonDown(0))
        {
            LeftClickHandled = true;
            BuildManager.PlaceBuilding();
        }
    }

	// if click is not already handled, select units
	void LateUpdate()
	{
		SelectUnits();

		LeftClickHandled = false;
		RightClickHandled = false;
	}

	public static void MouseOverClickableObject(Transform script, ClickableObject co)
	{
		bool leftClick = Input.GetMouseButton(0);
		bool rightClick = Input.GetMouseButton(1);

		if(rightClick && !RightClickHandled)
		{
			RightClickHandled = true;

			/* TODO: handle building */

			/* TODO: handle  */

            Instance.TargetSelectedPeopleTo(script);
		}

		// Handle UI Hover and Click stuff
        if (leftClick && !LeftClickHandled && co.clickable) {
            uiManager.OnShowObjectInfo(script);
			LeftClickHandled = true;
		}
        else {
            uiManager.OnShowSmallObjectInfo(script);
		}
	}

	public static void MouseExitClickableObject(Transform script, ClickableObject co)
	{
		// Hide UI info on object
		uiManager.OnHideSmallObjectInfo();
	}

    // update selection
    private void SelectUnits()
    {
        // only update if not building and pointer is not over a UI Element
        if(VillageUIManager.Instance.GetBuildingMode() != -1 ||
			EventSystem.current.IsPointerOverGameObject() ||
            BuildManager.placing)
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
