using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : Singleton<InputManager> {
	
	// Wether anything has already been done with the current mouse click
	public static bool LeftClickHandled, RightClickHandled;

	// A reference to the UIManager
	private static UIManager uiManager;

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
		
		uiManager = UIManager.Instance;

        selectionBoxImage.gameObject.SetActive(false);
	}
	
	void Update ()
    {
        uiManager = UIManager.Instance;

        HandleTerrainClick();
        HandleBuildClick();
    }

    public static bool InputUI()
    {
        return !BuildManager.placing && !ChatManager.IsChatActive();
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
        bool addTask = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
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

			if(targetNode.nodeObject) {
                ps.AddRoutineTaskTransform(targetNode.nodeObject, targetPos, false, !addTask);
            }
			else {
                ps.AddRoutineTaskPosition(targetPos, false, !addTask);
			}

			//if (target == 1 && Grid.ToGrid(ps.transform.position) != Grid.ToGrid(targetPos) + new Vector3(delta[ind].x, 0, delta[ind].y)) ps.SetTargetPosition(targetPos + new Vector3(delta[ind].x, 0, delta[ind].y) * Grid.SCALE);
			//else if (target == 2/* && Grid.ToGrid(ps.transform.position) != Grid.ToGrid(targetTr.position) + new Vector3(delta[ind].x, 0, delta[ind].y)*/) ps.SetTargetTransform(targetTr, targetTr.position + new Vector3(delta[ind].x, 0, delta[ind].y) * Grid.SCALE);
			ind++;
		}
	}

    private void TargetSelectedPeopleTo(Transform target)
    {
        bool addTask = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
		foreach (PersonScript ps in PersonScript.selectedPeople)
		{
			// dont move inactive people
			if (!ps || !ps.gameObject.activeSelf) continue;

            if(addTask)
                ps.AddTargetTransform(target, false);
            else
                ps.SetTargetTransform(target, false);
        }
    }

	// Handles all clicks on the terrain, e.g. movement and deselection
	private void HandleTerrainClick()
	{
        if(EventSystem.current.IsPointerOverGameObject() || BuildManager.placing) return;

        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
            if (ChatManager.IsChatActive()) ChatManager.ToggleChat();

        if (Input.GetMouseButtonDown(1))
        {
            RaycastHit hit;
            if(MouseRaycastHit(out hit))
            {
                string tag = hit.transform.gameObject.tag;
                if (tag == "Terrain")
                {
                    Vector3 hitGrid = Grid.ToGrid(hit.point);
                    // Check if valid
                    if(Grid.ValidNode((int)hitGrid.x, (int)hitGrid.z))
                    {
                        Node hitNode = Grid.GetNode((int)hitGrid.x, (int)hitGrid.z);
                        WalkSelectedPeopleTo(hitNode, hit.point);
                        RightClickHandled = true;
                    }
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

	void LateUpdate()
	{
        if (!ChatManager.IsChatActive())
        {

            // exit ui and building mode
            if (Input.GetKeyDown(KeyCode.Escape))
            {

                if (BuildManager.placing)
                    BuildManager.EndPlacing();
                else if (ChatManager.IsChatActive())
                    ChatManager.ToggleChat();
                else
                {
                    if (uiManager.InMenu())
                        uiManager.ExitMenu();
                    else
                        uiManager.ShowMenu(6);
                }
            }

            // open build menu if exactly one person is selected
            if (Input.GetKeyDown(KeyCode.B))
            {
                if (PersonScript.selectedPeople.Count == 1)
                {
                    uiManager.ShowMenu(7);
                }
            }

            // open job overview if no person selected
            if (Input.GetKeyDown(KeyCode.J))
            {
                if (PersonScript.selectedPeople.Count == 0)
                {
                    uiManager.ShowMenu(1);
                    uiManager.OnPopulationTab(1);
                }
            }

            // open minimap overview
            if (Input.GetKeyDown(KeyCode.M))
            {
                uiManager.ToggleMiniMap();
            }

            Building selb = uiManager.GetSelectedBuilding();
            // destroy building if selected
            if (Input.GetKeyDown(KeyCode.Period))
            {
                if (selb)
                {
                    /* TODO: show warning when destroying buildings */
                    selb.DestroyBuilding();
                }
            }
            // move building if selected
            if (Input.GetKeyDown(KeyCode.Comma))
            {
                if (selb)
                {
                    BuildManager.StartMoving(selb);
                }
            }

            // Person Groups
            int numberInput = -1;
            if (Input.GetKeyDown(KeyCode.Alpha0)) numberInput = 0;
            if (Input.GetKeyDown(KeyCode.Alpha1)) numberInput = 1;
            if (Input.GetKeyDown(KeyCode.Alpha2)) numberInput = 2;
            if (Input.GetKeyDown(KeyCode.Alpha3)) numberInput = 3;
            if (Input.GetKeyDown(KeyCode.Alpha4)) numberInput = 4;
            if (Input.GetKeyDown(KeyCode.Alpha5)) numberInput = 5;
            if (Input.GetKeyDown(KeyCode.Alpha6)) numberInput = 6;
            if (Input.GetKeyDown(KeyCode.Alpha7)) numberInput = 7;
            if (Input.GetKeyDown(KeyCode.Alpha8)) numberInput = 8;
            if (Input.GetKeyDown(KeyCode.Alpha9)) numberInput = 9;

            if (numberInput >= 0)
            {
                List<int> group = GameManager.GetGameSettings().GetPeopleGroup(numberInput);
                if (Input.GetKey(KeyCode.LeftControl))
                {
                    group = new List<int>();
                    foreach (PersonScript ps in PersonScript.selectedPeople)
                    {
                        group.Add(ps.nr);
                    }
                    GameManager.GetGameSettings().SetPeopleGroup(numberInput, group);
                    ChatManager.Msg("Personengruppe " + numberInput + " erstellt!");
                }
                else
                {
                    PersonScript.DeselectAll();
                    if (group != null)
                        foreach (PersonScript ps in PersonScript.allPeople)
                        {
                            if (group.Contains(ps.nr)) ps.OnSelect();
                        }
                }
            }
        }
        // enter chat
        if (Input.GetKeyDown(KeyCode.Return))
        {
            ChatManager.CommitMsg();
            ChatManager.ToggleChat();
        }

        // if click is not already handled, select units
        SelectUnits();

		LeftClickHandled = false;
		RightClickHandled = false;
	}

    public static void MouseExitClickableUnit(Transform script, ClickableUnit cu)
    {
        uiManager.ShowPersonInfo(false);
    }

	public static void MouseOverClickableUnit(Transform script, ClickableUnit cu)
	{
		bool leftClick = Input.GetMouseButtonDown(0);
		bool rightClick = Input.GetMouseButtonDown(1);

		// Handle UI Hover and Click stuff
        if (leftClick && !LeftClickHandled && cu.clickable) {
            PersonScript ps = script.GetComponent<PersonScript>();
            if(ps)
            {
                ps.OnClick();
            }
			LeftClickHandled = true;
		}
    }

	public static void MouseOverClickableObject(Transform script, ClickableObject co)
	{
		bool leftClick = Input.GetMouseButtonDown(0);
		bool rightClick = Input.GetMouseButtonDown(1);

        // only register clicks if not over ui
        if(!EventSystem.current.IsPointerOverGameObject())
        {
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
        if(EventSystem.current.IsPointerOverGameObject() ||
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
    
    // Select people in rect
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
