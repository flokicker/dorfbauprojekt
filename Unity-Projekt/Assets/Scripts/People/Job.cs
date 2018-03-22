using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum JobType
{
    Unemployed, BuildingMaterial, Clothes, Luxury, Food, Ships, Religion, Weapon
}
public class Job 
{
    private int id;
    private JobType type;
    private string name;
    private bool unlocked;

    public Job(int id, JobType type, string name)
    {
        this.id = id;
        this.type = type;
        this.name = name;
    }

    public int GetID()
    {
        return id;
    }
    public string GetName()
    {
        return name;
    }

    private static List<Job> allJobs = null;
    public static Job unemployed = new Job(0, JobType.Unemployed, "-");

    public static int JobCount()
    {
        if (allJobs == null) SetupJobs();
        return allJobs.Count;
    }
    public static Job GetJob(int id)
    {
        if (allJobs == null) SetupJobs();
        return allJobs[id];
    }
    public static Job RandomJob()
    {
        if (allJobs == null) SetupJobs();
        int id = Random.Range(0,allJobs.Count);
        return allJobs[id];
    }
    public static void SetupJobs()
    {
        allJobs = new List<Job>();
        allJobs.Add(unemployed);

        int index = 1;

        string[] bmJobNames = { "Baumeister", "Holzfäller", "Handwerker - Lehmgrube", "Handwerker - Eisenmiene" };
        for (int i = 0; i < bmJobNames.Length; i++)
        {
            allJobs.Add(new Job(i + index, JobType.BuildingMaterial, bmJobNames[i]));
        }
        index += bmJobNames.Length;
        string[] fJobNames = { "Sammler", "Jäger", "Fischer" };
        for (int i = 0; i < fJobNames.Length; i++)
        {
            allJobs.Add(new Job(i + index, JobType.Food, fJobNames[i]));
            allJobs[i + index - 1].Unlock();
        }
        index += fJobNames.Length;
    }

    public bool IsUnlocked()
    {
        return unlocked;
    }
    public void Unlock()
    {
        unlocked = true;
    }
}
