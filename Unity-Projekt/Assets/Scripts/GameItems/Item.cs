using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemData : TransformData
{
    public int resId, resAm;
}
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
	
    public void Set(int id, int Amount)
    {
        resource = new GameResources(id, Amount);
    }

	// Update is called once per frame
	public override void  Update () {
        base.Update();

        if(resource.Amount == 0) Destroy(gameObject);
	}
    
    public override void OnDestroy()
    {
        base.OnDestroy();
        allItems.Remove(this);
    }

    public string GetName()
    {
        if (resource == null) return "undefined";
        return resource.Name;
    }
    public int ResID()
    {
        return resource.Id;
    }
    public int Amount()
    {
        return resource.Amount;
    }
    public GameResources GetResource()
    {
        return resource;
    }

    public ItemData GetItemData()
    {
        ItemData itd = new ItemData();
        itd.SetPosition(transform.position);
        itd.SetRotation(transform.rotation);
        itd.resId = resource.Id;
        itd.resAm = resource.Amount;
        return itd;
    }
    public void SetItemData(ItemData itd)
    {
        transform.position = itd.GetPosition();
        transform.rotation = itd.GetRotation();

        resource = new GameResources(itd.resId, itd.resAm);
    }
}
