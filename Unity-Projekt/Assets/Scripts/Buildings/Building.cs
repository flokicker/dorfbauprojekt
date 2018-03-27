using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BuildingType
{
    BuildingMaterialProduction, Clothes, Food, Population, StorageMaterial, StorageFood, Other
}
public enum BuildingID
{
    Main, Tent, Warehouse
}
public class Building {

    private int id;
    private BuildingType type;
    private string name, description;
    private int stage;

    private int cost;
    private int[] materialCost;
    private int workspace, populationRoom, populationCurrent;

    private int gridX, gridY;
    private int gridWidth, gridHeight;

    private int orientation;

    private bool unlocked;

    /* TODO: implement entry points for buildings like tent */
    //private List<Vector2> entryPoints = new List<Vector2>();

    public Building(int id)
    {
        FromID(id);
    }

    private void FromID(int id)
    {
        this.id = id;
        switch (id)
        {
            case 0: Set(BuildingType.Other, "Haupthaus", "", 0, new int[5], 0, 25, 4, 4); break;
            case 1: Set(BuildingType.Population, "Unterschlupf", "Erhöht den Wohnraum", 0, new int[] { 0, 0, 0, 0, 0 }, 0, 4, 4, 4); //cost: wood30
                break;
            case 2: Set(BuildingType.StorageMaterial, "Lagerhaus", "Lagert Holz und Steine", 0, new int[] { 45, 0, 0, 0, 0 }, 0, 0, 4, 4); break;
            case 3: Set(BuildingType.StorageFood, "Kornspeicher", "Lagert Getreide und Pilze", 0, new int[] { 50, 0, 0, 0, 0 }, 0, 0, 2, 2); break;
            case 4: Set(BuildingType.Food, "Fischerplatz", "Gefangene Fische werden hier verarbeitet", 0, new int[] { 60, 0, 0, 0, 0 }, 0, 0, 4, 4); break;
            case 5: Set(BuildingType.Other, "Holzfäller", "Zur Holzverarbeitung", 0, new int[] { 50, 0, 0, 0, 0 }, 0, 0, 4, 4); break;
            case 6: Set(BuildingType.Other, "Jagdhütte", "Zum Jagen", 0, new int[] { 75, 0, 0, 0, 0 }, 0, 0, 4, 4); break;
            case 7: Set(BuildingType.Other, "Versammlungsplatz", "Erhöht den Luxus", 0, new int[] { 270, 0, 0, 0, 0 }, 0, 0, 4, 4); break;

            /*case 0: Set(BuildingType.Other, "Haupthaus", 0, new int[5], 0, 25, 4, 4); break;
            case 1: Set(BuildingType.BuildingMaterialProduction, "Holzfäller", 10, new int[] { 0, 15, 2, 0, 0 }, 2, 0, 1, 1); break;
            case 2: Set(BuildingType.BuildingMaterialProduction, "Lehmgrube", 10, new int[] { 4, 0, 20, 0, 0 }, 3, 0, 1, 1); break;
            case 3: Set(BuildingType.BuildingMaterialProduction, "Steinmetz", 10, new int[] { 5, 13, 10}, 4, 0, 1, 1); break;
            case 4: Set(BuildingType.BuildingMaterialProduction, "Bronzemiene", 10, new int[] { 10, 8, 2, 0, 0},5, 0, 1, 1); break;
            case 5: Set(BuildingType.BuildingMaterialProduction, "Silbermiene", 10, new int[] { 15, 32, 20, 0, 0 }, 5, 0, 1, 1); break;
            case 6: Set(BuildingType.Population, "Hütte", 10, new int[] { 25, 20, 0, 0, 0 }, 0, 4, 1, 1); break;*/
        }
    }
    private void Set(BuildingType type, string name, string description, int cost, int[] materialCost, int workspace, int populationRoom,
        int gridWidth, int gridHeight)
    {
        this.type = type;
        this.name = name;
        this.description = description;
        this.cost = cost;
        this.materialCost = materialCost;
        this.workspace = workspace;
        this.populationRoom = populationRoom;

        this.gridWidth = gridWidth;
        this.gridHeight = gridHeight;

        this.stage = 0;
        this.populationCurrent = 0;
        this.gridX = 0;
        this.gridY = 0;
    }
    public void SetPosition(int gx, int gy)
    {
        gridX = gx;
        gridY = gy;
    }
    public void SetOrientation(int o)
    {
        orientation = o;
    }
    
    /*private void AddEntry(int dx, int dy)
    {
        entryPoints.Add(new Vector2(dx, dy));
    }*/

    public int GetID()
    {
        return id;
    }
    public BuildingType GetBuildingType()
    {
        return type;
    }
    public string GetName()
    {
        return name;
    }
    public string GetDescription()
    {
        return description;
    }
    public int GetStage()
    {
        return stage;
    }
    public int GetCost()
    {
        return cost;
    }
    public int GetMaterialCost(int i)
    {
        return materialCost[i];
    }
    public int[] GetAllMaterialCost()
    {
        return materialCost;
    }
    public int GetWorkspace()
    {
        return workspace;
    }
    public int GetPopulationRoom()
    {
        return populationRoom;
    }
    public int GetCurrentPopulation()
    {
        return populationCurrent;
    }
    public int GetGridX()
    {
        return gridX;
    }
    public int GetGridY()
    {
        return gridY;
    }
    public int GetGridWidth()
    {
        return gridWidth;
    }
    public int GetGridHeight()
    {
        return gridHeight;
    }
    /*public Vector2 GetEntryPoint(int id)
    {
        return entryPoints[id];
    }*/

    private static List<Building> allBuildings;
    public static void SetupBuildings()
    {
        allBuildings = new List<Building>();

        for (int i = 0; i < 7; i++)
            allBuildings.Add(new Building(i));
        for (int i = 0; i < 2; i++) allBuildings[i].Unlock();
    }
    public static int BuildingCount()
    {
        if (allBuildings == null) SetupBuildings();
        return allBuildings.Count;
    }
    public static Building GetBuilding(int id)
    {
        return allBuildings[id];
    }

    public bool IsUnlocked()
    {
        return unlocked;
    }
    public void Unlock()
    {
        unlocked = true;
    }

    /*private static string[] building_names = { 
        "Haupthaus",
        "Holzfäller", "Lehmgrube", "Steinmetz", "Bronzemiene", "Silbermiene", "Goldmiene",
        "Hütte", "Kleines Langhaus", "Grosses Langhaus",
        "Fischer", "Schweinestall", "Metzger", "Bäcker"
    };*/
}
