using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : HideableObject {

    private GameResources resource;

	// Use this for initialization
    public override void Start()
    {
        base.Start();

        tag = "Item";

        // handles all outline/interaction stuff
        gameObject.AddComponent<ClickableObject>();
	}
	
    public void SetResource(int id, int amount)
    {
        resource = new GameResources(id, amount);
    }

	// Update is called once per frame
	void Update () {
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
