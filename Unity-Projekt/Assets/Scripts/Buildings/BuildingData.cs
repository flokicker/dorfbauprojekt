using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BuildingData {

    public int id, nr;
    public int[] resourceCurrent, bluePrintBuildCost;
    public int populationCurrent;
    
    public int gridX, gridY;
    public int gridWidth, gridHeight;
    public int orientation;

    public bool blueprint;

    public List<int> workingPeople;

    public float posX, posY, posZ;
    public float rotX, rotY, rotZ;

    public void SetPosition(Vector3 pos)
    {
        posX = pos.x;
        posY = pos.y;
        posZ = pos.z;
    }
    public void SetRotation(Quaternion rot)
    {
        rotX = rot.eulerAngles.x;
        rotY = rot.eulerAngles.y;
        rotZ = rot.eulerAngles.z;
    }
    public Vector3 GetPosition()
    {
        return new Vector3(posX, posY, posZ);
    }
    public Quaternion GetRotation()
    {
        return Quaternion.Euler(rotX, rotY, rotZ);
    }
}
