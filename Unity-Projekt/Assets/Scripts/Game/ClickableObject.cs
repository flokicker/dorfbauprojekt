using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickableObject : MonoBehaviour {

    public bool clickable = true;

	private cakeslice.Outline outline;

	private Transform scriptedParent;

	// Use this for initialization
	void Start () {
        if(!GetComponent<cakeslice.Outline>()) 
        {
            outline = gameObject.AddComponent<cakeslice.Outline>();
		    outline.enabled = false;
        }
        else outline = gameObject.GetComponent<cakeslice.Outline>();
	}
	
	// Update is called once per frame
	void Update () {
	}

    public void SetOutline(bool en)
    {
        outline.enabled = en;
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

        InputManager.MouseExitClickableObject(ScriptedParent(), this);
    }
    void OnMouseOver()
    {
        if (CameraController.inputState == 2) SetOutline(true);

        InputManager.MouseOverClickableObject(ScriptedParent(), this);
    }
}
