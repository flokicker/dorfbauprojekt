using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour {

    private GameResources resource;

	// Use this for initialization
    void Start()
    {
        /* TODO: init method with specific resources */
        resource = new GameResources(0, 1);

        // handles all outline/interaction stuff
        gameObject.AddComponent<ClickableObject>();
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
