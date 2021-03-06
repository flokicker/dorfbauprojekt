﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData : TransformData {
    public string username;

    public int currentDay, coins;

    public int foodFactor, roomspaceFactor, healthFactor, fertilityFactor, luxuryFactor;
    public float totalFactor;

    public bool faithEnabled;
    public float faithPoints, techPoints;

    public bool techTreeEnabled;
    public List<int> unlockedBranches;

    //public bool[] unlockedBuildings, unlockedJobs, unlockedResources;
    public List<int> unlockedBuildings, unlockedJobs, unlockedResources;

    public float cameraDistance, cameraRotation;

    public List<int> featuredResources = new List<int>();
    public List<int>[] peopleGroups;

    public Achievement[] achList;

    public List<GameQuest> openQuests;
    public List<GameAchievement> achievements;
}
