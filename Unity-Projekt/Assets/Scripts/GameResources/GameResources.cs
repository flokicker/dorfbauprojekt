using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ResourceType
{
    BuildingMaterial, Clothes, Food, RawFood, Luxury
}
public class GameResources 
{
    public int id;
    private ResourceType type;
    private string name;

    public int amount;
    public float nutrition;

    private bool unlocked;

    public GameResources(int id, int amount)
    {
        this.id = id;
        this.amount = amount;
        nutrition = 1;
        switch (id)
        {
            case 5: nutrition = 14; break;
            case 6: nutrition = 0; break;
            case 7: nutrition = 25; break;
            case 8: nutrition = 20; break;
            case 9: nutrition = 10; break;
        }
        if (id < bmNames.Length)
        {
            name = bmNames[id];
            type = ResourceType.BuildingMaterial;
        }
        else if ((id -= bmNames.Length) < fdNames.Length)
        {
            name = fdNames[id];
            type = ResourceType.Food;
            if (id == 1)
                type = ResourceType.RawFood;
        }

    }
    public GameResources(int id) : this(id, 0)
    {
    }

    public int GetID()
    {
        return id;
    }
    public string GetName()
    {
        return name;
    }
    public int GetAmount()
    {
        return amount;
    }
    public void SetAmount(int setAmount)
    {
        amount = setAmount;
    }
    public void Add(int addAmount)
    {
        amount += addAmount;
    }
    public void Take(int takeAmount)
    {
        amount -= takeAmount;
    }
    public ResourceType GetResourceType()
    {
        return type;
    }
    public float GetNutrition()
    {
        return nutrition;
    }

    private static List<GameResources> allResources = null;
    public static int ResourceCount()
    {
        if (allResources == null) SetupResources();
        return allResources.Count;
    }
    public static int AvailableResourceCount()
    {
        if (allResources == null) SetupResources();
        return GetAvailableResources().Count;
    }
    public static List<GameResources> GetAllResources()
    {
        if (allResources == null) SetupResources();
        return allResources;
    }
    public static List<GameResources> GetAvailableResources()
    {
        if (allResources == null) SetupResources();
        List<GameResources> availableResources = new List<GameResources>();
        foreach (GameResources res in allResources)
            if (res.IsUnlocked()) availableResources.Add(res);
        return availableResources;
    }

    /*public static GameResources FromID(int id)
    {
        if(id < bmNames.Length)
            return new GameResources(id, ResourceType.BuildingMaterial, bmNames[id]);
        return null;
    }*/

    public static void SetupResources()
    {
        allResources = new List<GameResources>();

        for (int i = 0; i < bmNames.Length; i++)
            allResources.Add(new GameResources(i));
        for (int i = 0; i < fdNames.Length; i++)
            allResources.Add(new GameResources(bmNames.Length + i));
    }
    private static string[] bmNames = { "Holz", "Stein", "Eisen", "Bronze", "Silber" };
    private static string[] fdNames = { "Pilz", "Roher Fisch", "Fisch", "Fleisch", "Korn" };

    public static int GetBuildingResourcesCount()
    {
        return bmNames.Length;
    }

    public bool IsUnlocked()
    {
        return unlocked;
    }
    public static void Unlock(int id)
    {
        allResources[id].unlocked = true;
    }

    public static int COUNT = 10;
    public static int WOOD = 0;
    public static int STONE = 1;
    public static int MUSHROOM = 5;
    public static int RAWFISH = 6;
    public static int FISH = 7;
    public static int MEAT = 8;
    public static int CORN = 9;
}
