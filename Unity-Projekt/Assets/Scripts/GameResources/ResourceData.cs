
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum ResourceType
{
    Building, Clothes, Food, RawFood, Luxury, Crafting, Tool, DeadAnimal, AnimalParts
}

[CreateAssetMenu(fileName = "New Material Resource", menuName = "MatResource")]
[System.Serializable]
public class ResourceData : DatabaseData
{
    // General
    public ResourceType type;

    // Food
    public bool edible;
    public int nutrition;
    public int health;

    // Processing and crafting
    public float processTime;

    // Religion
    public float faithPoints;

    // Results for crafting
    public List<GameResources> results;

    // UI
    public Sprite icon;

    // Models for items
    public List<GameObject> models;

    // Get reference to resource data by id or name
    public static ResourceData Get(int id)
    {
        foreach (ResourceData rd in allResources)
            if (rd.id == id)
                return rd;
        Debug.Log("undefined res id=" + id);
        return null;
    }
    public static ResourceData Get(string name)
    {
        foreach (ResourceData rd in allResources)
            if (rd.name == name)
                return rd;
        Debug.Log("undefined res name=" + name);
        return null;
    }

    // Get other property directly
    public static int Id(string name)
    {
        return Get(name).id;
    }
    public static string Name(int id)
    {
        ResourceData res = Get(id);
        if (res == null) return "undefined res id=" + id;
        return res.name;
    }

    // List of all available resources
    public static List<ResourceData> allResources = new List<ResourceData>();
    public static int Count
    {
        get { return allResources.Count; }
    }
    // (Un)locking
    public static HashSet<int> unlockedResources = new HashSet<int>();
    public static void Unlock(int id)
    {
        unlockedResources.Add(id);
    }
    public static bool IsUnlocked(int id)
    {
        return unlockedResources.Contains(id);
    }
}
