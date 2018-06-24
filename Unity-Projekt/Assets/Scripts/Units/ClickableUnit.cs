using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickableUnit : MonoBehaviour {

    public bool clickable = true, highlighted, selected, tempOutline;

	private cakeslice.Outline outline;

	private Transform scriptedParent;

	// Use this for initialization
	void Start () {
        selected = false;
        highlighted = false;
        if(!GetComponent<cakeslice.Outline>()) 
        {
            outline = gameObject.AddComponent<cakeslice.Outline>();
        }
        else outline = gameObject.GetComponent<cakeslice.Outline>();
        outline.enabled = false;
	}
	
	// Update is called once per frame
	void Update () {
        outline.enabled = selected || highlighted || tempOutline;
        outline.color = selected ? 1 : 0;
        tempOutline = false;

    }

    public void SetOutline(bool en)
    {
        if(!selected) outline.enabled = en;
        highlighted = en;
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
        SetOutline(false);

        InputManager.MouseExitClickableUnit(ScriptedParent(), this);
    }
    void OnMouseOver()
    {
        if (CameraController.inputState == 2) SetOutline(true);

        InputManager.MouseOverClickableUnit(ScriptedParent(), this);
    }
}
