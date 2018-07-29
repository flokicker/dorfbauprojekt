using System.Collections;
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

    // Spawning of new animals
    public float spawningFactor, spawningLimit;
    public int maxWaterDistance; // set to 0 for not spawning near water

    // Collision
    public float stopRadius;

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

    // List of all available buildings
    public static List<Animal> allAnimals = new List<Animal>();
    public static int Count
    {
        get { return allAnimals.Count; }
    }
}

