using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum AchievementType
{
    Resource, Building, Job, Population, DeadPopulation, DeadWarrior
}
[System.Serializable]
[CreateAssetMenu(fileName = "New Achievement", menuName = "Achievement")]
public class Achievement : DatabaseData
{
    // Type
    public AchievementType type;

    // amount and id of achievemnt resource/building/job
    public int[] amountLvl;
    public int resBuildJobId;

    public string TaskDescription(string amtxt)
    {
        string text = "";
        switch (type)
        {
            case AchievementType.Resource:
                text += amtxt + " " + ResourceData.Name(resBuildJobId) + " gesammelt";
                break;
            case AchievementType.Building:
                text += "Baue " + amtxt + " " + Building.Get(resBuildJobId);
                break;
            case AchievementType.Job:
                text += "Beschäftige " + amtxt + " " + Job.Get(resBuildJobId).name;
                break;// + (ach.id == 7 ? " Tote Krieger" :  ((i == 4 ? " Gestorbene" : "")+ " Bewohner"));
            case AchievementType.Population:
                text += amtxt + " Bewohner";
                break;
            case AchievementType.DeadPopulation:
                text += amtxt + " Gestorbene Bewohner";
                break;
            case AchievementType.DeadWarrior:
                text += amtxt + " Tote Krieger";
                break;
        }
        text += "\n";
        return text;
    }

    // Get reference to resource data by id or name
    public static Achievement Get(int id)
    {
        foreach (Achievement ach in allAchievements)
            if (ach.id == id)
                return ach;
        return null;
    }
    public static Achievement Get(string name)
    {
        foreach (Achievement ach in allAchievements)
            if (ach.name == name)
                return ach;
        return null;
    }

    // Get other property directly
    public static int Id(string name)
    {
        return Get(name).id;
    }
    public static string Name(int id)
    {
        Achievement ach = Get(id);
        if (ach == null) return "undefined achievement id=" + id;
        return ach.name;
    }

    // List of all available quests
    public static List<Achievement> allAchievements = new List<Achievement>();
    public static int Count
    {
        get { return allAchievements.Count; }
    }
}
