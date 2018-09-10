using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BuildingType
{
    BuildingMaterialProduction, Clothes, Food, Population, Storage, Luxury, Crafting, Religion, Recruiting, Research, Path, Campfire, Other
}

[CreateAssetMenu(fileName = "New Building", menuName = "Building")]
[System.Serializable]
public class Building : DatabaseData
{
    public const string Tag = "Building";

    // General
    public BuildingType type;
    public string description;

    // Resources
    public int cost;
    public List<GameResources> costResource, storage;
    
    // Population
    public int jobId, workspace, noTaskCapacity;
    public int[] populationRoom;

    // Grid properties
    public int gridWidth, gridHeight;

    // Range
    public int viewRange, foodRange, buildRange;

    // Collision and Editing
    public bool walkable, movable, destroyable;
    public float collisionRadius, selectionCircleRadius;

    // Show grid-node under building
    public bool showGrid;

    // Multiple buildings
    public int peoplePerBuilding;

    // Stages
    public int maxStages;

    // Other
    public bool multipleBuildings, hasFire;
    public int unlockBuildingID;
    /* TODO: public int stage; */

    // UI
    public Sprite icon;

    // Model
    public GameObject model;

    // Get reference to resource data by id or name
    public static Building Get(int id)
    {
        foreach (Building b in allBuildings)
            if (b.id == id)
                return b;
        return null;
    }
    public static Building Get(string name)
    {
        foreach (Building b in allBuildings)
            if (b.name == name)
                return b;
        return null;
    }

    // Get other property directly
    public static int Id(string name)
    {
        return Get(name).id;
    }
    public static string Name(int id)
    {
        Building res = Get(id);
        if (res == null) return "undefined building id=" + id;
        return res.name;
    }

    // List of all available buildings
    public static List<Building> allBuildings = new List<Building>();
    public static int Count
    {
        get { return allBuildings.Count; }
    }
    // (Un)locking
    public static HashSet<int> unlockedBuilding = new HashSet<int>();
    public static void Unlock(int id)
    {
        unlockedBuilding.Add(id);
    }
    public static bool IsUnlocked(int id)
    {
        return unlockedBuilding.Contains(id);
    }
    public static bool IsUnlocked(string name)
    {
        return unlockedBuilding.Contains(Id(name));
    }
    
    // make sure that if requirement for people per building to be built are fullfilled
    public static bool PeoplePerBuildingFullfilled(Building b)
    {
        if (b.peoplePerBuilding > 0)
        {
            // e.g. 100 people per building -> 1-100 = 1 Building, 101-200 = 2 Buildings, etc
            int totB = GameManager.village.BuildingIdCount(b.id);
            if (PersonScript.allPeople.Count <=  b.peoplePerBuilding * totB)
            {
                return false;
            }
        }
        return true;

    }
}
