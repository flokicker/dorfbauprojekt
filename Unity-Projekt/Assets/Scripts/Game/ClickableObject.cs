using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClickableObject : MonoBehaviour {

    public bool clickable = true;
    public bool outlined = false;

	private cakeslice.Outline outline;

	private Transform scriptedParent;

	// Use this for initialization
	void Start () {
        if(!GetComponentInChildren<cakeslice.Outline>()) 
        {
            outline = gameObject.AddComponent<cakeslice.Outline>();
		    outline.enabled = false;
        }
        else outline = gameObject.GetComponentInChildren<cakeslice.Outline>();
	}
	
	// Update is called once per frame
	void Update () {
        bool b = outlined;
        if(b && EventSystem.current.IsPointerOverGameObject())
            b = false;
        
        SetOutline(b);
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
        outlined = false;

        InputManager.MouseExitClickableObject(ScriptedParent(), this);
    }
    void OnMouseOver()
    {
        if (CameraController.inputState == 2) outlined = true;

        InputManager.MouseOverClickableObject(ScriptedParent(), this);
    }
}
