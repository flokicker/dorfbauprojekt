using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BuildingData : TransformData {

    public int id, nr;
    public int[] resourceCurrent, bluePrintBuildCost;
    public int populationCurrent;
    
    public int gridX, gridY;
    public int gridWidth, gridHeight;
    public int orientation;

    public bool blueprint;

    public List<int> workingPeople;
}
