using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClickableObject : MonoBehaviour {

    public bool clickable = true, highlightable, selectedOutline, showSmallInfo;
    public bool outlined = false;

    public bool keepOriginalPos = false;

    private float radius;

    private Vector3 orgPosition;

    //private cakeslice.Outline outline;
    private Projector selectionCircle;

	private Transform scriptedParent;

    private static Color highlighted = new Color(1f,1f,0.9f), selected = new Color(1f,1f,0.4f);

    private PersonScript ps;

	// Use this for initialization
	void Start () {
        /*if(!GetComponentInChildren<cakeslice.Outline>()) 
        {
            outline = gameObject.AddComponent<cakeslice.Outline>();
		    outline.enabled = false;
        }
        else outline = gameObject.GetComponentInChildren<cakeslice.Outline>();*/

        // make sure to copy material, so every clickable object can change its material independantely
        selectionCircle = ScriptedParent().GetComponentInChildren<Projector>();
        if (selectionCircle == null)
        {
            selectionCircle = Instantiate(GameManager.Instance.selectionCirclePrefab, ScriptedParent()).GetComponent<Projector>();
            selectionCircle.material = new Material(selectionCircle.material);
            SetOutline(false);
        }

        selectedOutline = true;
        highlightable = true;
        showSmallInfo = true;

        orgPosition = selectionCircle.transform.position;

        ps = transform.GetComponent<PersonScript>();
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (selectionCircle.orthographicSize != radius)
            SetSelectionCircleRadius(radius);
        //outline.color = 0;
        bool b = outlined;
        if (b && EventSystem.current.IsPointerOverGameObject())
            b = false;
        if (selectedOutline && (UIManager.Instance.IsTransformSelected(ScriptedParent()) || (ps != null && ps.selected)))
        {
            b = true;
            selectionCircle.material.color = selected;
        }
        else selectionCircle.material.color = highlighted;
        if (!highlightable) b = false;

        SetOutline(b);
	}

    private void LateUpdate()
    {
        if(keepOriginalPos) selectionCircle.transform.position = orgPosition;
        selectionCircle.transform.rotation = Quaternion.Euler(90,0,0);
    }

    public void SetOutline(bool en)
    {
        //if(outline) outline.enabled = en;
        selectionCircle.gameObject.SetActive(en);
    }

    public void SetSelectionCircleRadius(float radius)
    {
        this.radius = radius;
        if (selectionCircle)
        {
            selectionCircle.orthographicSize = radius;
            selectionCircle.material.SetFloat("_Radius", 0.25f);
            selectionCircle.material.SetFloat("_Border", 1f / radius * 0.01f);
        }
    }

    public Transform ScriptedParent()
    {
        if(scriptedParent) return scriptedParent;
        return transform;
    }

    public void SetScriptedParent(Transform sp)
    {
        scriptedParent = sp;
    }

    void OnMouseExit()
    {
        outlined = false;

        InputManager.MouseExitClickableObject(ScriptedParent(), this);
    }
    void OnMouseOver()
    {
        if (CameraController.inputState == 2) outlined = true;

        InputManager.MouseOverClickableObject(ScriptedParent(), this);
    }
}
