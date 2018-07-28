using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameNatureObject : TransformData {

    public NatureObject natureObject
    {
        get { return NatureObject.Get(natureObjectId); }
    }
    public int natureObjectId;

    public int gridX, gridY;

    public GameResources resourceCurrent;
    
    public int size, variation, miningTimes;
    public float fallSpeed, breakTime;
    public bool broken;

    // Growth and Despawning
    public float currentGrowth, growthTime, despawnTime;

    public GameNatureObject(NatureObject natureObject, int gridX, int gridY, int size, int variation)
    {
        this.natureObjectId = natureObject.id;

        this.gridX = gridX;
        this.gridY = gridY;

        this.size = size;
        this.variation = variation;

        miningTimes = 0;
        fallSpeed = 0;
        breakTime = -1;
        broken = false;

        currentGrowth = natureObject.growth;
        growthTime = 0;
        despawnTime = 0;

        float amount = natureObject.materialPerSize.Amount * (1+size);
        amount *= Random.Range(1f - natureObject.materialVarFactor, 1f + natureObject.materialVarFactor);
        resourceCurrent = new GameResources(natureObject.materialPerSize.Id, (int)amount);
    }
    public GameNatureObject(NatureObject natureObject, int gridX, int gridY) : this(natureObject,gridX,gridY,Random.Range(0,natureObject.sizes),Random.Range(0,natureObject.variations))
    {
    }
    public GameNatureObject(string name) : this(NatureObject.Get(name))
    {
    }
    public GameNatureObject(int id) : this(NatureObject.Get(id))
    {
    }
    public GameNatureObject(NatureObject natureObject) : this(natureObject, 0, 0) { }

    // Resets current growth factor to zero
    public void StopGrowth()
    {
        currentGrowth = 0;
    }

    public GameObject GetModelToSpawn()
    {
        return natureObject.models[variation];
    }
}
