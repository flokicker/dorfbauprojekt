using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData : TransformData {
    public string username;

    public int currentDay, coins;

    public int foodFactor, roomspaceFactor, healthFactor, fertilityFactor, luxuryFactor;
    public float totalFactor;

    public bool faithEnabled;
    public float faithPoints;

    public bool techTreeEnabled;
    public TechTree techTree;

    //public bool[] unlockedBuildings, unlockedJobs, unlockedResources;
    public List<int> unlockedBuildings, unlockedJobs, unlockedResources;

    public float cameraDistance, cameraRotation;

    private List<int> featuredResources = new List<int>();
    public List<int>[] peopleGroups;

    public Achievement[] achList;
}
