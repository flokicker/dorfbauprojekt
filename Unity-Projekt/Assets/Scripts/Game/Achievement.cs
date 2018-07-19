﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum AchievementType
{
    Resource, Building, Job, Population
}
[System.Serializable]
public class Achievement {

    public string name;

    public int currentAmount;
    public int[] amountLvl;
    public int resId;

    public AchievementType type;

    public static Achievement achLumberjack, achFisherman, achMiner, achCivilisation, achReaper, achFaith, achBuilder, achValhalla;
    public static Achievement[] achList;

    public Achievement(string name, int[] amountLvl, int resId, AchievementType type)
    {
        this.name = name;
        this.currentAmount = 0;
        this.amountLvl = amountLvl;
        this.resId = resId;
        this.type = type;
    }

    public int GetLvl()
    {
        for (int i = 0; i < amountLvl.Length; i++)
            if (currentAmount < amountLvl[i]) return i;
        return amountLvl.Length;
    }

    public void UpdateAmount(int add)
    {
        int oldLvl = GetLvl();
        currentAmount += add;
        if (GetLvl() > oldLvl) ChatManager.Msg("Neuen Erfolg freigeschalten: " + name + " Stufe " + (oldLvl + 1), Color.cyan);
    }

    public static void SetupAchievements()
    {
        achLumberjack = new Achievement("Holzfäller", new int[] { 50, 250, 1000, 2500, 5000, 10000, 25000, 50000, 100000, 1000000 }, GameResources.WOOD, AchievementType.Resource);
        achFisherman = new Achievement("Fischer", new int[] { 50, 250, 1000, 2500, 5000, 10000, 25000, 50000, 100000, 1000000 }, GameResources.FISH, AchievementType.Resource);
        achMiner = new Achievement("Bergarbeiter", new int[] { 500, 2500, 5000, 25000, 50000, 100000, 250000, 1000000, 10000000, 25000000 }, GameResources.STONE, AchievementType.Resource);
        achCivilisation = new Achievement("Zivilisation", new int[] { 10, 25, 50, 100, 200, 500, 750, 1000, 1500, 2500 }, -1, AchievementType.Population);
        achReaper = new Achievement("Sensenmann", new int[] { 1, 5, 25, 50, 100, 250, 500, 1000, 1500, 2500 }, -1, AchievementType.Population);
        achFaith = new Achievement("Glauben", new int[] { 1, 3, 10, 25, 50 }, Job.PRIEST, AchievementType.Job);
        achBuilder = new Achievement("Bauherr", new int[] { 1, 3, 5, 25, 50 }, Building.SHELTER, AchievementType.Building);
        achValhalla = new Achievement("Valhalla", new int[] { 1, 10, 25, 50, 100 }, -1, AchievementType.Population);
        achList = new Achievement[] { achLumberjack, achFisherman, achMiner, achCivilisation, achReaper, achFaith, achBuilder, achValhalla };
    }
}
