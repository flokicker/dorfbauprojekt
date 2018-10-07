
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameBuilding : TransformData
{
    public Building building
    {
        get { return Building.Get(buildingId); }
    }
    [SerializeField]
    private int buildingId;

    public int nr = -1;
    public List<GameResources> resourceCurrent, blueprintBuildCost;
    public int populationCurrent, noTaskCurrent;

    public int gridX, gridY;
    public int orientation;

    public bool blueprint;
    public int stage;

    public int /*familyJobId, */parentBuildingNr = -1;
    public List<int> childBuildingField, childBuildingStorage;

    public float fieldTime;
    public int fieldResource;

    public List<int> livingPeople, workingPeople;

    public GameBuilding(Building building, int gridX, int gridY, int orientation)
    {
        this.buildingId = building.id;

        this.gridX = gridX;
        this.gridY = gridY;
        this.orientation = orientation;

        workingPeople = new List<int>();
        childBuildingField = new List<int>();
        childBuildingStorage = new List<int>();

        resourceCurrent = new List<GameResources>();

        InitBluePrintBuildCost(building);
    }
    public GameBuilding(string name) : this(Building.Get(name))
    {
    }
    public GameBuilding(int id) : this(Building.Get(id))
    {
    }
    public GameBuilding(Building building) : this(building, 0, 0, 0) { }

    private void InitBluePrintBuildCost(Building building)
    {
        blueprintBuildCost = new List<GameResources>();
        if (GameManager.noCost) return;
        foreach (GameResources res in building.costResource[stage].list)
            blueprintBuildCost.Add(new GameResources(res));
    }
}