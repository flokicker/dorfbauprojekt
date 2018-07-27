using System.Collections.Generic;
using UnityEngine;

public class DatabaseManager : Singleton<DatabaseManager> {
    
    public static List<QuestData> allQuests = new List<QuestData>();

    private void Awake()
    {
        ResourceData.allResources = new List<ResourceData>(Resources.LoadAll<ResourceData>("ResMats"));
        Building.allBuildings = new List<Building>(Resources.LoadAll<Building>("Buildings"));
    }

    private void Update()
    {

    }
}
