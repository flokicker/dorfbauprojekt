
using System.Collections.Generic;
using UnityEngine;

public enum QuestType
{
    Collect, Find, Build
}
[CreateAssetMenu(fileName = "New Quest", menuName = "Quest")]
[System.Serializable]
public class Quest : DatabaseData
{
    [TextArea(15,20)]
    public string description;

    // Unlocking Quests
    public bool starterQuest;
    public List<Quest> unlockQuest;

    // Resource quests
    public List<GameResources> collectResources;
    // Build quests
    public List<BuildingQuestInfo> buildings;

    // Get reference to resource data by id or name
    public static Quest Get(int id)
    {
        foreach (Quest qu in allQuests)
            if (qu.id == id)
                return qu;
        return null;
    }
    public static Quest Get(string name)
    {
        foreach (Quest qu in allQuests)
            if (qu.name == name)
                return qu;
        return null;
    }

    // Get other property directly
    public static int Id(string name)
    {
        return Get(name).id;
    }
    public static string Name(int id)
    {
        Quest qu = Get(id);
        if (qu == null) return "undefined quest id=" + id;
        return qu.name;
    }

    // List of all available quests
    public static List<Quest> allQuests = new List<Quest>();
    public static int Count
    {
        get { return allQuests.Count; }
    }
}

[System.Serializable]
public class BuildingQuestInfo
{
    public int buildingId;
    public int count;

    public BuildingQuestInfo(int bid): this(bid,0) { }
    public BuildingQuestInfo(int bid, int am)
    {
        buildingId = bid;
        count = am;
    }
}