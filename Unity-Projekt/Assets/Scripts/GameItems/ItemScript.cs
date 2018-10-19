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
        co.SetSelectionCircleRadius(0.3f);

        // disable physics collision, only trigger collision
        GetComponent<Collider>().isTrigger = true;

        SetGroundY();

        // start coroutine
        StartCoroutine(GameItemTransformAndNode());

        base.Start();
    }

    public override void Update()
    {
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

        // destroy game object if amount is 0 after take
        if (gameItem.Amount == 0) Destroy(gameObject);
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
        pos.y = Terrain.activeTerrain.transform.position.y + smph + 0.01f;
        transform.position = pos;
    }

    // Update transform once per second
    private IEnumerator GameItemTransformAndNode()
    {
        while (true)
        {
            // update transform position rotation on save object
            gameItem.SetTransform(transform);

            // check my node
            if (myNode != null)
            {
                if (myNode.IsOccupied()) Destroy(gameObject);
            }
            else
            {
                myNode = Grid.GetNodeFromWorld(transform.position);
            }

            yield return new WaitForSeconds(1);
        }
    }
}
