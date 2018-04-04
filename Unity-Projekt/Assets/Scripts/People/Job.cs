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

    private static bool[] unlocked = new bool[10];

    public Job(int id)
    {
        FromID(id);
    }

    private void FromID(int id)
    {
        this.id = id;
        switch (id)
        {
            case 0: Set(JobType.Unemployed, "-"); break;
            case 1: Set(JobType.BuildingMaterial, "Holzfäller"); break;
            case 2: Set(JobType.Food, "Sammler"); break;
            case 3: Set(JobType.Food, "Jäger"); break;
            case 4: Set(JobType.Food, "Fischer"); break;
            case 5: Set(JobType.Tools, "Schmied"); break;
        }
    }
    private void Set(JobType type, string jobName)
    {
        this.type = type;
        this.jobName = jobName;
    }
    /*public static int MaxEmployees(int id)
    {
        switch(id)
        {
            
        }
        return 0;
    }*/

    public static int COUNT = 6;
    public static int UNEMPLOYED = 0;
    public static int LUMBERJACK = 1;
    public static int GATHERER = 2;
    public static int HUNTER = 3;
    public static int FISHER = 4;
    public static int BLACKSMITH = 5;

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

    public static bool IsUnlocked(int id)
    {
        return unlocked[id];
    }
    public static void Unlock(int id)
    {
        unlocked[id] = true;
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
