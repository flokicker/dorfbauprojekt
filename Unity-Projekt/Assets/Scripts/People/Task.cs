using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TaskType
{
    None, Walk, CutTree, CollectMushroom, CullectMushroomStump, Harvest, MineRock, BringToWarehouse, TakeFromWarehouse, 
    Campfire, PickupItem, Build, Fishing, Fisherplace
}

public class Task
{
    public TaskType taskType;
    public Vector3 target;
    public Transform targetTransform;
    public float taskTime;
    public List<GameResources> taskRes;

    public Task(TaskType ty, Vector3 tarPos,  Transform tarTrsf, List<GameResources> tr)
    {
        taskType = ty;
        target = tarPos;
        targetTransform = tarTrsf;
        taskTime = 0f;
        taskRes = tr;
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
