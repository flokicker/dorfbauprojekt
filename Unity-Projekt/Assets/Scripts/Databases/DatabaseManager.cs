using System.Collections.Generic;
using UnityEngine;

public class DatabaseManager : Singleton<DatabaseManager> {
    
    public static List<QuestData> allQuests = new List<QuestData>();

    private void Awake()
    {
        ResourceData.allResources = new List<ResourceData>(Resources.LoadAll<ResourceData>("ResMats"));
        ResourceData.allResources.Sort(DatabaseData.SortById);
        Building.allBuildings = new List<Building>(Resources.LoadAll<Building>("Buildings"));
        Building.allBuildings.Sort(DatabaseData.SortById);
        NatureObject.allNatureObject = new List<NatureObject>(Resources.LoadAll<NatureObject>("NatureObjects"));
        NatureObject.allNatureObject.Sort(DatabaseData.SortById);
    }

    private void Update()
    {

    }
}
