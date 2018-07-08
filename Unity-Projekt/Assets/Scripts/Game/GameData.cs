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

    public bool[] unlockedBuildings, unlockedResources;

    public float cameraDistance, cameraRotation;

    public List<int>[] peopleGroups;
}
