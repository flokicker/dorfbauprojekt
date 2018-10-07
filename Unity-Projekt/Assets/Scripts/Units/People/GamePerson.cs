using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GamePerson : TransformData {

    public int nr = -1;
    public string firstName, lastName;
    public Gender gender;
    public int age, lifeTimeYears, lifeTimeDays, invMatId, invMatAm, invFoodId, invFoodAm;
    public Disease disease;
    public bool wild;

    public int jobID, workingBuildingId, noTaskBuildingId;

    public int lastNodeX, lastNodeY;

    public float health, hunger;
    public float saturation, saturationTimer;

    public List<Task> routine = new List<Task>();

    public int motherNr;
    public bool pregnant;
    public float pregnancyTime;

    // Inventory
    public GameResources inventoryMaterial, inventoryFood;

    public void SetLastNode(Node n)
    {
        lastNodeX = n.gridX;
        lastNodeY = n.gridY;
    }
}
