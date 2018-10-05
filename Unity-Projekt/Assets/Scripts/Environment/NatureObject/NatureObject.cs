using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NatureObjectType
{
    Tree, Mushroom, MushroomStump, Reed, Crop, Rock, EnergySpot, Water
}
[CreateAssetMenu(fileName = "New Nature Object", menuName = "NatureObject")]
public class NatureObject : DatabaseData
{
    public const string Tag = "NatureObject";

    // General
    public NatureObjectType type;
    public string description;

    // Grid properties
    public int gridWidth, gridHeight;

    // Size, vartiation, radius
    public int sizes, variations;
    public int meterPerSize, meterOffsetSize;
    public float radiusPerSize, radiusOffsetSize;

    // Individual NatureObjectScript spawning (tree,mushroom,mushroomStump,reed,corn,rock)
    public float spawningFactor, spawningLimit;

    // Resources
    public float materialVarFactor;
    public GameResources materialPerSize;
    public int materialAmPerChop;

    // Collision
    public bool walkable;

    // Breaking and mining by chopping
    public int chopTimesPerSize, chopTimesOffsetSize;
    public bool chopShake, tilting;

    // Growth
    public float growth, resourceGrowth;
    public List<IntegerInterval> growingMonths;

    // UI
    public Sprite icon;

    // Prefab with multiple sizes models [variation]
    public GameObject[] models;

    // Get reference to resource data by id or name
    public static NatureObject Get(int id)
    {
        foreach (NatureObject no in allNatureObject)
            if (no.id == id)
                return no;
        return null;
    }
    public static NatureObject Get(string name)
    {
        foreach (NatureObject no in allNatureObject)
            if (no.name == name)
                return no;
        return null;
    }

    // Get other property directly
    public static int Id(string name)
    {
        return Get(name).id;
    }
    public static string Name(int id)
    {
        NatureObject no = Get(id);
        if (no == null) return "undefined nature object id=" + id;
        return no.name;
    }

    // List of all available buildings
    public static List<NatureObject> allNatureObject = new List<NatureObject>();
    public static int Count
    {
        get { return allNatureObject.Count; }
    }

    /* TODO: discovery system for new plants */
}
