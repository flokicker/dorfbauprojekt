using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour {

    private GameResources resource;

	// Use this for initialization
    void Start()
    {
        resource = new GameResources(0, 1);
        GetComponent<cakeslice.Outline>().enabled = false;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnMouseExit()
    {
        GetComponent<cakeslice.Outline>().enabled = false;
        VillageUIManager.Instance.OnHideObjectInfo();
    }
    void OnMouseOver()
    {
        if (CameraController.inputState == 2) GetComponent<cakeslice.Outline>().enabled = true;
        if (Input.GetMouseButton(0))
            VillageUIManager.Instance.OnShowObjectInfo(transform);
        else
            VillageUIManager.Instance.OnShowSmallObjectInfo(transform);
    }

    public string GetName()
    {
        if (resource == null) return "undefined";
        return resource.GetName();
    }
    public int GetResID()
    {
        return resource.GetID();
    }
    public int GetAmount()
    {
        return resource.GetAmount();
    }
    public GameResources GetResource()
    {
        return resource;
    }
}
