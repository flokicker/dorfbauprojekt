using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BuildingType
{
    BuildingMaterialProduction, Clothes, Food, Population, Storage, Luxury,
    Crafting, Religion, Recruiting, Research, Path, Campfire, Field, HutStorage, Other
}
[System.Serializable]
public class StageCosts
{
    public List<GameResources> list;
}
[System.Serializable]
public class VariationResList
{
    public List<StageCosts> list;
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
    public bool canBuild, unlockedFromStart;
    public int cost;
    public List<StageCosts> costResource, storage;
    
    // Population
    public int jobId, workspace, noTaskCapacity;
    public int[] populationRoom;

    // Grid properties
    public int gridWidth, gridHeight;

    // Entry points
    public bool hasEntry;
    public int entryDx, entryDy;

    // Range
    public int viewRange, foodRange, buildRange;

    // Collision and Editing
    public bool walkable, movable, destroyable, inWater;
    public float collisionRadius, selectionCircleRadius;

    // Show grid-node under building
    public bool showGrid;

    // Multiple buildings
    public int peoplePerBuilding;

    // Stages
    public int maxStages;

    // Fields/Storages
    public int fieldBuildingId, storageBuildingId;
    public int maxFields, maxStorages;

    // Other
    public bool multipleBuildings, hasFire;
    public int unlockBuildingID;
    public bool stayInRangeOfParent;
    public int techPoints;

    // UI
    public Sprite icon;

    // Model
    public GameObject models;

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
        Building b = Get(name);
        if (b) return b.id;

        Debug.Log("undefined building: " + name);
        return 0;
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
