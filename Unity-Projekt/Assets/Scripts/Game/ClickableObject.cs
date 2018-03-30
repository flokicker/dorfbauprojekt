using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickableObject : MonoBehaviour {

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

    /* TODO: unify handler for interactable objects */
    void OnMouseExit()
    {
        SetOutline(false);
        VillageUIManager.Instance.OnHideSmallObjectInfo();
    }
    void OnMouseOver()
    {
        if (CameraController.inputState == 2) SetOutline(true);
        if (Input.GetMouseButton(0))
            VillageUIManager.Instance.OnShowObjectInfo(ScriptedParent());
        else
            VillageUIManager.Instance.OnShowSmallObjectInfo(ScriptedParent());
    }
}
