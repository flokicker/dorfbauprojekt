using System.Collections.Generic;
using UnityEngine;

public class DatabaseManager : Singleton<DatabaseManager> {

    private void Awake()
    {
        ResourceData.allResources = new List<ResourceData>(Resources.LoadAll<ResourceData>("ResMats"));
        ResourceData.allResources.Sort(DatabaseData.SortById);
        Building.allBuildings = new List<Building>(Resources.LoadAll<Building>("Buildings"));
        Building.allBuildings.Sort(DatabaseData.SortById);
        NatureObject.allNatureObject = new List<NatureObject>(Resources.LoadAll<NatureObject>("NatureObjects"));
        NatureObject.allNatureObject.Sort(DatabaseData.SortById);
        Animal.allAnimals = new List<Animal>(Resources.LoadAll<Animal>("Animals"));
        Animal.allAnimals.Sort(DatabaseData.SortById);
        Quest.allQuests = new List<Quest>(Resources.LoadAll<Quest>("Quests"));
        Quest.allQuests.Sort(DatabaseData.SortById);
        Achievement.allAchievements = new List<Achievement>(Resources.LoadAll<Achievement>("Achievements"));
        Achievement.allAchievements.Sort(DatabaseData.SortById);
    }

    private void Update()
    {

    }
}
