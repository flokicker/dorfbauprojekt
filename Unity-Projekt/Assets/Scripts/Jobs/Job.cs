using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum JobType
{
    Unemployed, BuildingMaterial, Clothes, Luxury, Food, Ships, Religion, Tools
}
[System.Serializable]
[CreateAssetMenu(fileName = "New Job", menuName = "Job")]
public class Job : DatabaseData
{
    // Type
    public JobType type;

    // Limited amount by buildings
    public bool limited;

    // UI
    public Sprite icon;

    public bool Is(int id)
    {
        return this.id == id;
    }
    public bool Is(string name)
    {
        return this.name == name;
    }
    public bool IsUnemployed()
    {
        return this.type == JobType.Unemployed;
    }

    // Get reference to job data by id or name
    public static Job Get(int id)
    {
        foreach (Job jb in allJobs)
            if (jb.id == id)
                return jb;
        Debug.Log("undefined job id=" + id);
        return null;
    }
    public static Job Get(string name)
    {
        foreach (Job jb in allJobs)
            if (jb.name == name)
                return jb;
        Debug.Log("undefined job name=" + name);
        return null;
    }

    // Get other property directly
    public static int Id(string name)
    {
        return Get(name).id;
    }
    public static string Name(int id)
    {
        Job res = Get(id);
        if (res == null) return "undefined job id=" + id;
        return res.name;
    }

    // List of all available jobs
    public static List<Job> allJobs = new List<Job>();
    public static int Count
    {
        get { return allJobs.Count; }
    }
    // (Un)locking
    public static HashSet<int> unlockedJobs = new HashSet<int>();
    public static void Unlock(int id)
    {
        unlockedJobs.Add(id);
    }
    public static bool IsUnlocked(int id)
    {
        return unlockedJobs.Contains(id);
    }
}
