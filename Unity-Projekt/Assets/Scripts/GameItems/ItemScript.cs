using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemScript : HideableObject
{
    public const string Tag = "Item";

    // Collection of all items
    public static HashSet<ItemScript> allItemScripts = new HashSet<ItemScript>();

    private ClickableObject co;

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

    private Node myNode;

    public override void Start()
    {
        allItemScripts.Add(this);
        tag = Tag;

        // handles all outline/interaction stuff
        co = gameObject.AddComponent<ClickableObject>();

        // disable physics collision, only trigger collision
        GetComponent<Collider>().isTrigger = true;

        SetGroundY();

        base.Start();
    }

    public override void Update()
    {
        co.SetSelectionCircleRadius(0.3f);

        // update transform position rotation on save object
        gameItem.SetTransform(transform);

        if (myNode)
        {
            if (myNode.IsOccupied()) Destroy(gameObject);
        }
        else
        {
            myNode = Grid.GetNodeFromWorld(transform.position);
        }

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

    public void SetGroundY()
    {
        float smph = Terrain.activeTerrain.SampleHeight(transform.position);
        Vector3 pos = transform.position;
        pos.y = Terrain.activeTerrain.transform.position.y + smph;
        transform.position = pos;
    }
}
