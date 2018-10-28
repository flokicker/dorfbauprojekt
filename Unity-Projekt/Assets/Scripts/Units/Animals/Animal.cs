
using System.Collections.Generic;
using UnityEngine;

/* TODO: implement */
public enum AnimalType
{
    Wild
}
[CreateAssetMenu(fileName = "New Animal", menuName = "Animal")]
[System.Serializable]
public class Animal : HealthUnit
{
    public const string Tag = "Animal";

    // Resources dropped when killed
    public List<GameResources> dropResources = new List<GameResources>();

    // Movement
    public float moveSpeed;

    // Maximum distance from water
    public int maxWaterDistance; // set to 0 for not spawning near water

    // Herd
    public int maxDistFromHerdCenter, maxCountHerd;
    public float reproductionRate;

    // Pack
    public bool inPack;

    // Timers
    public int pregnantTime, growUpTime, liveTime;

    // Collision
    public float stopRadius, selectionCircleRadius;

    // Animal jumps around when moving
    public bool jumping;

    // UI
    public Sprite icon;

    // Prefab
    public GameObject model;

    // Get reference to resource data by id or name
    public static Animal Get(int id)
    {
        foreach (Animal no in allAnimals)
            if (no.id == id)
                return no;
        return null;
    }
    public static Animal Get(string name)
    {
        foreach (Animal no in allAnimals)
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
        Animal no = Get(id);
        if (no == null) return "undefined animal id=" + id;
        return no.name;
    }

    // List of all available animals
    public static List<Animal> allAnimals = new List<Animal>();
    public static int Count
    {
        get { return allAnimals.Count; }
    }
}

