using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemData
{
    public int resId, resAm;

    public float posX, posY, posZ;
    public float rotX, rotY, rotZ;

    public void SetPosition(Vector3 pos)
    {
        posX = pos.x;
        posY = pos.y;
        posZ = pos.z;
    }
    public void SetRotation(Quaternion rot)
    {
        rotX = rot.eulerAngles.x;
        rotY = rot.eulerAngles.y;
        rotZ = rot.eulerAngles.z;
    }
    public Vector3 GetPosition()
    {
        return new Vector3(posX, posY, posZ);
    }
    public Quaternion GetRotation()
    {
        return Quaternion.Euler(rotX, rotY, rotZ);
    }
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

    public ItemData GetItemData()
    {
        ItemData itd = new ItemData();
        itd.SetPosition(transform.position);
        itd.SetRotation(transform.rotation);
        itd.resId = resource.id;
        itd.resAm = resource.amount;
        return itd;
    }
    public void SetItemData(ItemData itd)
    {
        transform.position = itd.GetPosition();
        transform.rotation = itd.GetRotation();

        resource = new GameResources(itd.resId, itd.resAm);
    }
}
