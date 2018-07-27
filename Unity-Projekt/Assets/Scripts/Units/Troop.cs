using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Troop {
    public int id;
    public string name;
    public List<GameResources> resourceCost;
    public float recruitingTime;

    public Troop(int id, string name, List<GameResources> resourceCost, int recruitingTime)
    {
        this.id = id;
        this.name = name;
        this.resourceCost = resourceCost;
        this.recruitingTime = recruitingTime;
    }

    public static int COUNT = 1;
    public static Troop FromID(int id)
    {
        Troop troop = null;
        List<GameResources> res = new List<GameResources>();
        switch(id)
        {
            case 0:
                res.Add(new GameResources("Keule", 1));
                troop = new Troop(id, "Keulenkämpfer", res, 20);
                break;
        }

        return troop;
    }

    public string Desc()
    {
        string ret = "Baukosten: ";
        for (int i = 0; i < resourceCost.Count; i++)
            ret += resourceCost[i].Amount + " " + resourceCost[i].Name + ", ";
        if (resourceCost.Count > 0) ret = ret.Substring(0,ret.Length - 2);
        ret += "\nBewohner: 1\nAusbildungsdauer: " + recruitingTime + "s";

        return ret;
    }
}
