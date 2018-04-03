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

    public Task(TaskType ty, Vector3 tar)
    {
        taskType = ty;
        target = tar;
        targetTransform = null;
        taskTime = 0f;
    }

    public Task(TaskType ty, Transform tar)
    {
        taskType = ty;
        target = Vector2.zero;
        targetTransform = tar;
        taskTime = 0f;
    }

    public Task(TaskType ty, Vector3 tarPos, Transform tarTrsf)
    {
        taskType = ty;
        target = tarPos;
        targetTransform = tarTrsf;
        taskTime = 0f;
    }
}
