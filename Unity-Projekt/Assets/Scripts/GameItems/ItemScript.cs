using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemScript : HideableObject
{
    public const string Tag = "Item";

    // Collection of all items
    public static HashSet<ItemScript> allItemScripts = new HashSet<ItemScript>();

    public int ResId
    {
        get { return gameItem.resourceId; }
    }
    public string ResName
    {
        get { return gameItem.resource.name; }
    }
    public ResourceData Resource
    {
        get { return gameItem.resource; }
    }
    public int Amount
    {
        get { return gameItem.Amount; }
    }
    private GameItem gameItem;

    public override void Start()
    {
        allItemScripts.Add(this);
        tag = Tag;

        // handles all outline/interaction stuff
        gameObject.AddComponent<ClickableObject>();

        base.Start();
    }

    public override void Update()
    {
        // update transform position rotation on save object
        gameItem.SetTransform(transform);

        if (gameItem.Amount == 0) Destroy(gameObject);

        base.Update();
    }

    public override void OnDestroy()
    {
        allItemScripts.Remove(this);
        base.OnDestroy();
    }

    public void Take(int am)
    {
        gameItem.Take(am);
    }

    public void SetItem(GameItem gameItem)
    {
        this.gameItem = gameItem;
    }

    public static List<GameItem> AllGameItems()
    {
        List<GameItem> ret = new List<GameItem>();
        foreach (ItemScript its in allItemScripts)
            ret.Add(its.gameItem);
        return ret;
    }
    public static void DestroyAllItems()
    {
        foreach (ItemScript its in allItemScripts)
            Destroy(its.gameObject);
        allItemScripts.Clear();
    }
}
