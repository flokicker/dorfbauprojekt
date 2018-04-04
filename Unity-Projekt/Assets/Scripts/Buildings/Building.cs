using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BuildingType
{
    BuildingMaterialProduction, Clothes, Food, Population, StorageMaterial, StorageFood, Luxury, Other
}
public class Building {

    public int id;
    private BuildingType type;
    public string name, description;
    private int stage;

    private int cost;
    public int[] materialCost;
    public int jobId, workspace, populationRoom, populationCurrent, resourceStorage;

    private int gridX, gridY;
    private int gridWidth, gridHeight;
    private int orientation;

    private bool unlocked;

    public bool walkable, multipleBuildings;

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
            case 0: Set(BuildingType.Other, "Höhle", "Kleiner Unterschlupf mit Vorratslager", 0, new int[10], 25, 0, 0, 5, 4, 4, true, false); break;
            case 1: Set(BuildingType.Population, "Unterschlupf", "Erhöht den Wohnraum", 0, new int[] { 40, 10, 0, 0, 0 }, 0, 0, 0, 2, 4, 4, true, true); break;
            case 2: Set(BuildingType.StorageMaterial, "Lagerplatz", "Lagert Holz und Steine", 0, new int[] { 25, 15, 0, 0, 0 }, 200, 0, 0, 0, 4, 4, true, true); break;
            case 3: Set(BuildingType.StorageFood, "Kornspeicher", "Lagert Getreide, Pilze und Fische", 0, new int[] { 20, 0, 0, 0, 0 }, 150, 0, 0, 0, 2, 2, true, false); break;
            case 4: Set(BuildingType.Food, "Fischerplatz", "Gefangene Fische werden hier verarbeitet", 0, new int[] { 25, 0, 0, 0, 0 }, 0, Job.FISHER, 2, 0, 4, 4, true, true); break;
            case 5: Set(BuildingType.Other, "Holzlager", "Erlaubt die Holzverarbeitung", 0, new int[] { 50, 10, 0, 0, 0 }, 0, Job.LUMBERJACK, 0, 0, 4, 4, true, false); break;
            case 6: Set(BuildingType.Other, "Jagdhütte", "Erlaubt das Jagen", 0, new int[] { 45, 20, 0, 0, 0 }, 0, Job.HUNTER, 1, 0, 4, 4, true, true); break;
            case 7: Set(BuildingType.Other, "Steinzeit Schmied", "Herstellung von Knochen-Werkzeug", 0, new int[] { 50, 35, 0, 0, 0 }, 0, Job.BLACKSMITH, 1, 0, 4, 8, true, true); break;
            case 8: Set(BuildingType.Luxury, "Lagerfeuer", "Erhöht den Luxus", 0, new int[] { 15, 5, 0, 0, 0 }, 0, Job.GATHERER, 0, 0, 2, 1, false, true); break;

            /*case 0: Set(BuildingType.Other, "Haupthaus", 0, new int[5], 0, 25, 4, 4); break;
            case 1: Set(BuildingType.BuildingMaterialProduction, "Holzfäller", 10, new int[] { 0, 15, 2, 0, 0 }, 2, 0, 1, 1); break;
            case 2: Set(BuildingType.BuildingMaterialProduction, "Lehmgrube", 10, new int[] { 4, 0, 20, 0, 0 }, 3, 0, 1, 1); break;
            case 3: Set(BuildingType.BuildingMaterialProduction, "Steinmetz", 10, new int[] { 5, 13, 10}, 4, 0, 1, 1); break;
            case 4: Set(BuildingType.BuildingMaterialProduction, "Bronzemiene", 10, new int[] { 10, 8, 2, 0, 0},5, 0, 1, 1); break;
            case 5: Set(BuildingType.BuildingMaterialProduction, "Silbermiene", 10, new int[] { 15, 32, 20, 0, 0 }, 5, 0, 1, 1); break;
            case 6: Set(BuildingType.Population, "Hütte", 10, new int[] { 25, 20, 0, 0, 0 }, 0, 4, 1, 1); break;*/
        }
    }
    private void Set(BuildingType type, string name, string description, int cost, int[] materialCost, int resourceStorage, int jobId, int workspace, int populationRoom,
        int gridWidth, int gridHeight, bool walkable, bool multipleBuildings)
    {
        this.type = type;
        this.name = name;
        this.description = description;
        this.cost = cost;
        this.materialCost = materialCost;
        this.resourceStorage = resourceStorage;

        this.jobId = jobId;
        this.workspace = workspace;
        this.populationRoom = populationRoom;

        this.gridWidth = gridWidth;
        this.gridHeight = gridHeight;

        this.stage = 0;
        this.populationCurrent = 0;
        this.gridX = 0;
        this.gridY = 0;

        this.walkable = walkable;
        this.multipleBuildings = multipleBuildings;
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
        string ret = description;
        if(!multipleBuildings) ret += "\nKann nur einmal gebaut werden";
        return ret;
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

        for (int i = 0; i < 9; i++)
            allBuildings.Add(new Building(i));
        for (int i = 0; i < 9; i++) allBuildings[i].Unlock();
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
