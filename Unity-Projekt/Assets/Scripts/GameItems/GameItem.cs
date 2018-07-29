using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameItem : TransformData {

    public int Amount
    {
        get { return amount; }
        private set { amount = value; }
    }
    [SerializeField]
    private int amount;

    public int variation;
    
    public int resourceId;
    public ResourceData resource
    {
        get { return ResourceData.Get(resourceId); }
    }

    public GameItem(int res) : this(res, 0) { }
    public GameItem(int res, int am)
    {
        resourceId = res;
        variation = Random.Range(0, resource.models.Count);
        amount = am;
    }

    public void Take(int am)
    {
        amount -= am;
    }
}
