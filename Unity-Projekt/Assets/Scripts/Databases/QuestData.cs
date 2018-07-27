
public enum QuestType
{
    Collect, Find, Build
}

[System.Serializable]
public class QuestData : DatabaseData
{
    public string description;

    // Resource quests
    public int resId, resAm;
    // Build quests
    public int buildingId, buildingAm;
}