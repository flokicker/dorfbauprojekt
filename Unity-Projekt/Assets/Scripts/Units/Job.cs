using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum JobType
{
    Unemployed, BuildingMaterial, Clothes, Luxury, Food, Ships, Religion, Tools
}
public class Job 
{
    public int id;
    public JobType type;
    public string jobName;
    public bool limited;

    public Job(int id)
    {
        FromID(id);
    }

    private void FromID(int id)
    {
        this.id = id;
        switch (id)
        {
            case 0: Set(JobType.Unemployed, "-", false); break;
            case 1: Set(JobType.BuildingMaterial, "Holzfäller", false); break;
            case 2: Set(JobType.Food, "Sammler", false); break;
            case 3: Set(JobType.Food, "Jäger", true); break;
            case 4: Set(JobType.Food, "Fischer", true); break;
            case 5: Set(JobType.Tools, "Schmied", true); break;
            case 6: Set(JobType.Religion, "Priester", true); break;
            case 7: Set(JobType.Ships, "Schiffbauer", true); break;
        }
    }
    private void Set(JobType type, string jobName, bool limited)
    {
        this.type = type;
        this.jobName = jobName;
        this.limited = limited;
    }
    /*public static int MaxEmployees(int id)
    {
        switch(id)
        {
            
        }
        return 0;
    }*/

    public static int COUNT = 8;
    public static int UNEMPLOYED = 0;
    public static int LUMBERJACK = 1;
    public static int GATHERER = 2;
    public static int HUNTER = 3;
    public static int FISHER = 4;
    public static int BLACKSMITH = 5;
    public static int PRIEST = 6;
    public static int SHIP_BUILDER = 7;

    /*public static int JobCount()
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
    }*/


    private static bool[] unlocked = new bool[10];
    public static bool IsUnlocked(int id)
    {
        return unlocked[id];
    }
    public static void Unlock(int id)
    {
        unlocked[id] = true;
    }
    public static void ResetAllUnlocked()
    {
        unlocked = new bool[10];
    }

    /*public bool IsUnlocked()
    {
        return unlocked[id];
    }
    public void Unlock()
    {
        unlocked[id] = true;
    }*/
}
