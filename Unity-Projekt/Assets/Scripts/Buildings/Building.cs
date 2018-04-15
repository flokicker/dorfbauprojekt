using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BuildingType
{
    BuildingMaterialProduction, Clothes, Food, Population, Storage, Luxury, Other, Crafting
}
public class Building {

    public int id;
    private BuildingType type;
    public string name, description;
    private int stage;

    private int cost;
    public int[] materialCost, resourceCurrent, resourceStorage;
    public int jobId, workspace, populationRoom, populationCurrent;

    private int gridX, gridY;
    private int gridWidth, gridHeight;
    private int orientation;

    public int viewRange, foodRange, buildRange;

    private bool unlocked;

    public bool walkable, multipleBuildings;

    // Show grid-node under building
    public bool showGrid;

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
            case 0: Set(BuildingType.Storage, "Höhle", "Kleiner Unterschlupf mit Vorratslager", 0, new int[10], new int[] {25,25,0,0,0,0,0,0,0,0,0,0}, 0, 0, 5, 3, 3, true, false, 20, 0, 20); break;
            case 1: Set(BuildingType.Population, "Unterschlupf", "Erhöht den Wohnraum", 0, new int[] { 40, 10, 0, 0, 0 }, new int[20], 0, 0, 2, 4, 4, true, true, 0, 0, 0); break;
            case 2: Set(BuildingType.Storage, "Lagerplatz", "Lagert Holz und Steine", 0, new int[] { 25, 15, 0, 0,  }, new int[] {200,200,0,0,0,0,0,0,0,0,0,0}, 0, 0, 0, 4, 4, true, true, 0, 0, 0); break;
            case 3: Set(BuildingType.Storage, "Kornspeicher", "Lagert Getreide, Pilze und Fische", 0, new int[] { 20, 0, 0, 0, 0 }, new int[] {0,0,0,0,0,150,150,0,150,0,0,0}, 0, 0, 0, 2, 2, true, false, 0, 35, 0); break;
            case 4: Set(BuildingType.Food, "Fischerplatz", "Gefangene Fische (Wild) werden hier zu Fisch und Knochen verarbeitet", 0, new int[] { 25, 0, 0, 0, 0 }, new int[] { 0,0,0,0,0,0,50,0,0,50,50,0}, Job.FISHER, 2, 0, 4, 4, true, true, 0, 0, 0); break;
            case 5: Set(BuildingType.Other, "Holzlager", "Erlaubt die Holzverarbeitung", 0, new int[] { 35, 10, 0, 0, 0 }, new int[20], Job.LUMBERJACK, 0, 0, 4, 4, true, false, 0, 0, 0); break;
            case 6: Set(BuildingType.Other, "Jagdhütte", "Erlaubt das Jagen", 0, new int[] { 45, 20, 0, 0, 0 }, new int[20], Job.HUNTER, 1, 0, 4, 4, true, true, 0, 0, 0); break;
            case 7: Set(BuildingType.Crafting, "Steinzeit Schmied", "Herstellung von Knochen-Werkzeug", 0, new int[] { 50, 35, 0, 0, 0 }, new int[] {0,0,0,0,0,0,0,0,0,0,40,10}, Job.BLACKSMITH, 1, 0, 8, 4, true, true, 0, 0, 0); break;
            case 8: Set(BuildingType.Luxury, "Lagerfeuer", "Bringe Holz, um das Feuer anzuzünden. Erhöht den Gesundheitsfaktor (+2)", 0, new int[] { 15, 5, 0, 0, 0 }, new int[20], Job.GATHERER, 0, 0, 2, 1, false, true, 0, 0, 0); break;

            /*case 0: Set(BuildingType.Other, "Haupthaus", 0, new int[5], 0, 25, 4, 4); break;
            case 1: Set(BuildingType.BuildingMaterialProduction, "Holzfäller", 10, new int[] { 0, 15, 2, 0, 0 }, 2, 0, 1, 1); break;
            case 2: Set(BuildingType.BuildingMaterialProduction, "Lehmgrube", 10, new int[] { 4, 0, 20, 0, 0 }, 3, 0, 1, 1); break;
            case 3: Set(BuildingType.BuildingMaterialProduction, "Steinmetz", 10, new int[] { 5, 13, 10}, 4, 0, 1, 1); break;
            case 4: Set(BuildingType.BuildingMaterialProduction, "Bronzemiene", 10, new int[] { 10, 8, 2, 0, 0},5, 0, 1, 1); break;
            case 5: Set(BuildingType.BuildingMaterialProduction, "Silbermiene", 10, new int[] { 15, 32, 20, 0, 0 }, 5, 0, 1, 1); break;
            case 6: Set(BuildingType.Population, "Hütte", 10, new int[] { 25, 20, 0, 0, 0 }, 0, 4, 1, 1); break;*/
        }
    }
    private void Set(BuildingType type, string name, string description, int cost, int[] materialCost, int[] resourceStorage, int jobId, int workspace, int populationRoom,
        int gridWidth, int gridHeight, bool walkable, bool multipleBuildings, int viewRange, int foodRange, int buildRange)
    {
        showGrid = id == CAVE || id == CAMPFIRE;
        this.type = type;
        this.name = name;
        this.description = description;
        this.cost = cost;
        this.materialCost = materialCost;
        this.resourceStorage = resourceStorage;
        this.resourceCurrent = new int[20];

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

        this.viewRange = viewRange;
        this.foodRange = foodRange;
        this.buildRange = buildRange;
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
        if(!multipleBuildings && id > 0) ret += "\nKann nur einmal gebaut werden";
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
    
    public int Restock(GameResources res, int amount)
    {
        int freeSpace = resourceStorage[res.id] - resourceCurrent[res.id];
        int restockRes = 0;
        if(freeSpace >= amount)
        {
            restockRes = amount;
        }
        else
        {
            restockRes = freeSpace;
        }
        resourceCurrent[res.id] += restockRes;
        return restockRes;
    }
    public int Restock(GameResources res)
    {
        return Restock(res, res.amount);
    }
    public int Take(GameResources res, int amount)
    {
        int takeRes = amount;
        if(resourceCurrent[res.id] < takeRes)
        {
            takeRes = resourceCurrent[res.id];
        }
        resourceCurrent[res.id] -= takeRes;
        return takeRes;
    }
    public int Take(GameResources res)
    {
        return Take(res, res.amount);
    }
    public int FreeStorage(int id)
    {
        return resourceStorage[id] - resourceCurrent[id];
    }

    public static int CAVE = 0;
    public static int SHELTER = 1;
    public static int WAREHOUSE = 2;
    public static int WAREHOUSEFOOD = 3;
    public static int FISHERMANPLACE = 4;
    public static int LUMBERJACK = 5;
    public static int HUNTINGLODGE = 6;
    public static int BLACKSMITH = 7;
    public static int CAMPFIRE = 8;

    /*public int TakeFromBuilding(GameResources res, BuildingScript bs)
    {
        int[] bsr = bs.GetBuilding().resourceCurrent;
        for (int i = 0; i < bsr.Count; i++)
        {
            if (resources[i].GetID() == res.GetID())
            {
                if (resources[i].GetAmount() >= res.GetAmount())
                {
                    resources[i].Take(res.GetAmount());
                    return res.GetAmount();
                }
                else if (resources[i].GetAmount() < res.GetAmount())
                {
                    int take = resources[i].GetAmount();
                    resources[i].Take(take);
                    return take;
                }
            }
        }
        return 0;
    }*/

    /*private static string[] building_names = { 
        "Haupthaus",
        "Holzfäller", "Lehmgrube", "Steinmetz", "Bronzemiene", "Silbermiene", "Goldmiene",
        "Hütte", "Kleines Langhaus", "Grosses Langhaus",
        "Fischer", "Schweinestall", "Metzger", "Bäcker"
    };*/
}
