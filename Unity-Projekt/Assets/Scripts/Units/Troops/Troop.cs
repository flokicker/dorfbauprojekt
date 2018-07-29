
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Troop", menuName = "Troop")]
[System.Serializable]
public class Troop : HealthUnit
{
    public const string Tag = "Troop";

    // Cost
    public List<GameResources> resourceCost;

    // Time
    public float recruitingTime;

    // Movement
    public float moveSpeed;

    // UI
    public Sprite icon;

    // Prefab
    public GameObject model;
    
    /*
    public string Desc()
    {
        string ret = "Baukosten: ";
        for (int i = 0; i < resourceCost.Count; i++)
            ret += resourceCost[i].Amount + " " + resourceCost[i].Name + ", ";
        if (resourceCost.Count > 0) ret = ret.Substring(0,ret.Length - 2);
        ret += "\nBewohner: 1\nAusbildungsdauer: " + recruitingTime + "s";

        return ret;
    }*/
}
