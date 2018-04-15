using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TaskType
{
    None, Walk, CutTree, CollectMushroom, CullectMushroomStump, Harvest, MineRock, BringToWarehouse, TakeFromWarehouse, 
    Campfire, PickupItem, Build, Fishing, Fisherplace, Craft
}

public class Task
{
    public TaskType taskType;
    public Vector3 target;
    public Transform targetTransform;
    public float taskTime;
    public List<GameResources> taskRes;
    public bool automated;

    public Task(TaskType ty, Vector3 tarPos,  Transform tarTrsf, List<GameResources> tr, bool aut)
    {
        taskType = ty;
        target = tarPos;
        targetTransform = tarTrsf;
        taskTime = 0f;
        taskRes = tr;
        automated = aut;
    }
    public Task(TaskType ty, Vector3 tarPos,  Transform tarTrsf, GameResources tr, bool aut)
    {
        taskType = ty;
        target = tarPos;
        targetTransform = tarTrsf;
        taskTime = 0f;
        taskRes = new List<GameResources>();
        taskRes.Add(tr);
        automated = aut;
    }

    public Task(TaskType ty, Vector3 tarPos,  Transform tarTrsf, List<GameResources> tr)
    : this(ty,tarPos,tarTrsf,tr,false)
    {
    }

    public Task(TaskType ty, Vector3 tar)
    : this(ty,tar,null)
    {
    }

    public Task(TaskType ty, Transform tar)
    : this(ty,Vector2.zero,tar)
    {
    }

    public Task(TaskType ty, Vector3 tarPos, Transform tarTrsf)
    : this(ty,tarPos,tarTrsf,new List<GameResources>())
    {
    }
}
