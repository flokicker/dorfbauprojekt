using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : HideableObject {

    // Collection of all items
    public static HashSet<Item> allItems = new HashSet<Item>();

    public GameResources resource;

	// Use this for initialization
    public override void Start()
    {
        base.Start();

        tag = "Item";

        // handles all outline/interaction stuff
        gameObject.AddComponent<ClickableObject>();

        allItems.Add(this);
	}
	
    public void Set(int id, int amount)
    {
        resource = new GameResources(id, amount);
    }

	// Update is called once per frame
	public override void  Update () {
        base.Update();
	}
    
    void OnDestroy()
    {
        allItems.Remove(this);
    }

    public string GetName()
    {
        if (resource == null) return "undefined";
        return resource.GetName();
    }
    public int ResID()
    {
        return resource.id;
    }
    public int Amount()
    {
        return resource.amount;
    }
    public GameResources GetResource()
    {
        return resource;
    }
}
