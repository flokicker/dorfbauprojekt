﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ResourceType
{
    BuildingMaterial, Clothes, Food, RawFood, Luxury, Crafting, Tool, DeadAnimal
}
public class GameResources 
{
    public int id;
    private ResourceType type;
    private string name;

    public int amount, processTime;
    public float nutrition, health;

    private bool unlocked;

    public GameResources(int id, int amount)
    {
        this.id = id;
        this.amount = amount;
        name = names[id];

        nutrition = 1;
        health = 0;
        processTime = 0;
        
        if((id-=COUNT_BUILDING_MATERIAL) < 0) type = ResourceType.BuildingMaterial;
        else if((id-=COUNT_FOOD) < 0) 
        {
            type = ResourceType.Food;
            if(this.id == MUSHROOM) { nutrition = 15; health = 12; }
            if(this.id == FISH) { nutrition = 25; health = 5; }
            if(this.id == MEAT) { nutrition = 28; health = 8; }
            if(this.id == CROP) { nutrition = 10; health = 10; }
        }
        else if((id-=COUNT_RAW_FOOD) < 0) type = ResourceType.RawFood;
        else if((id-=COUNT_CRAFTING) < 0)
        { 
            type = ResourceType.Crafting;
        }
        else if((id-=COUNT_TOOLS) < 0) 
        {
            type = ResourceType.Tool;
            if(this.id == TOOL_BONE) { processTime = 2*60; }
            if(this.id == CLUB) { processTime = 10; }
        }
        else if((id-=ANIMAL_DUCK) < 0) 
        {
            type = ResourceType.DeadAnimal;
            if(this.id == ANIMAL_DUCK) { processTime = 5; }
        }

        /*switch (id)
        {
            case 5: nutrition = 15; health = 12; break;
            case 6: nutrition = 0; health = -5; break;
            case 7: nutrition = 25; health = 5; break;
            case 8: nutrition = 20; health = 5; break;
            case 9: nutrition = 10; health = 5; break;
        }*/

        /*if (id < bmNames.Length)
        {
            name = bmNames[id];
            type = ResourceType.BuildingMaterial;
        }
        else if ((id - bmNames.Length) < fdNames.Length)
        {
            type = ResourceType.Food;
            if (id == 1)
                type = ResourceType.RawFood;
        }*/

    }
    public GameResources(int id) : this(id, 0)
    {
    }

    public string GetName()
    {
        return name;
    }
    public int GetAmount()
    {
        return amount;
    }
    public string GetDescription()
    {
        string additional = "";
        if(id == GameResources.WOOD) additional = "";

        string desc = name+"\n"+ResourceTypeToString(type);
        if(additional != "") desc += "\n"+additional;

        return desc;
    }
    public void SetAmount(int setAmount)
    {
        amount = setAmount;
    }
    public void Add(int addAmount)
    {
        amount += addAmount;
    }
    public void Take(int takeAmount)
    {
        amount -= takeAmount;
    }
    public ResourceType GetResourceType()
    {
        return type;
    }
    public float GetNutrition()
    {
        return nutrition;
    }

    private static List<GameResources> allResources = null;
    public static int ResourceCount()
    {
        if (allResources == null) SetupResources();
        return allResources.Count;
    }
    public static int AvailableResourceCount()
    {
        if (allResources == null) SetupResources();
        return GetAvailableResources().Count;
    }
    public static List<GameResources> GetAllResources()
    {
        if (allResources == null) SetupResources();
        return allResources;
    }
    public static List<GameResources> GetAvailableResources()
    {
        if (allResources == null) SetupResources();
        List<GameResources> availableResources = new List<GameResources>();
        foreach (GameResources res in allResources)
            if (IsUnlocked(res.id)) availableResources.Add(res);
        return availableResources;
    }

    public static string ResourceTypeToString(ResourceType resourceType)
    {
        switch(resourceType)
        {
            case ResourceType.BuildingMaterial: return "Baumaterial";
            case ResourceType.Food: return "Nahrung";
            case ResourceType.RawFood: return "Rohe Nahrung";
            case ResourceType.Crafting: return "Verarbeitung";
            case ResourceType.Tool: return "Werkzeug";
            case ResourceType.DeadAnimal: return "Totes Tier";
        }
        return "undefiniert";
    }

    /*public static GameResources FromID(int id)
    {
        if(id < bmNames.Length)
            return new GameResources(id, ResourceType.BuildingMaterial, bmNames[id]);
        return null;
    }*/

    public static void SetupResources()
    {
        allResources = new List<GameResources>();
        for(int i = 0; i < COUNT; i++)
            allResources.Add(new GameResources(i));
        /*for (int i = 0; i < bmNames.Length; i++)
            allResources.Add(new GameResources(i));
        for (int i = 0; i < fdNames.Length; i++)
            allResources.Add(new GameResources(bmNames.Length + i));*/
    }
    //private static string[] bmNames = { "Holz", "Stein", "Eisen", "Bronze", "Silber" };
    //private static string[] fdNames = { "Pilz", "Roher Fisch", "Fisch", "Fleisch", "Korn" };
    private static string[] names = { "Holz", "Stein", "Eisen", "Bronze", "Silber", 
            "Pilz", "Fisch", "Fleisch", "Korn",
            "Roher Fisch", 
            "Knochen", "Fell",
            "Knochen-Werkzeug", "Keule",
            "Tote Ente" };


    public static bool IsUnlocked(int id)
    {
        return allResources[id].unlocked;
    }
    public static void Unlock(int id)
    {
        if(allResources == null) SetupResources();
        allResources[id].unlocked = true;
    }

    public GameResources Clone()
    {
        return new GameResources(id, amount);
    }

    public static int COUNT_BUILDING_MATERIAL = 5;
    public static int COUNT_FOOD = 4;
    public static int COUNT_RAW_FOOD = 1;
    public static int COUNT_CRAFTING = 2;
    public static int COUNT_TOOLS = 2;
    public static int COUNT_ANIMALS = 1;
    public static int COUNT = 15;

    public static int WOOD = 0;
    public static int STONE = 1;

    public static int MUSHROOM = 5;
    public static int FISH = 6;
    public static int MEAT = 7;
    public static int CROP = 8;

    public static int RAWFISH = 9;

    public static int BONES = 10;
    public static int FUR = 11;

    public static int TOOL_BONE = 12;
    public static int CLUB = 13;

    public static int ANIMAL_DUCK = 14;
}
