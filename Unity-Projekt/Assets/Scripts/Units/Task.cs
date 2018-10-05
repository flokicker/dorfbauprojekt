using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum TaskType
{
    None, Walk, CutTree, CollectMushroom, CullectMushroomStump, Harvest, MineRock, BringToWarehouse, TakeFromWarehouse, 
    Campfire, PickupItem, Build, Fishing, Craft, HuntAnimal, FollowPerson, SacrificeResources,
    TakeIntoVillage, WorkOnField, GoWork
    // Fisherplace, TakeEnergySpot ProcessAnimal
}
[System.Serializable]
public class TaskData
{
    public TaskType taskType;
    public int targetX, targetY;
    public float taskTime;
    public bool automated;
    public int[] taskRes;
}
public class Task
{
    public TaskType taskType;
    public Vector3 target;
    public Transform targetTransform;
    public float taskTime;
    public List<GameResources> taskRes;
    public bool automated, checkFromFar, setup = false;
    public TaskData taskData = null;

    public Task(TaskType ty, Vector3 tarPos,  Transform tarTrsf, List<GameResources> tr, bool aut, bool cff)
    {
        taskType = ty;
        target = tarPos;
        targetTransform = tarTrsf;
        taskTime = 0f;
        taskRes = tr;
        automated = aut;
        checkFromFar = cff;
    }
    public Task(TaskType ty, Vector3 tarPos,  Transform tarTrsf, GameResources tr, bool aut, bool cff)
    {
        taskType = ty;
        target = tarPos;
        targetTransform = tarTrsf;
        taskTime = 0f;
        taskRes = new List<GameResources>();
        taskRes.Add(tr);
        automated = aut;
        checkFromFar = cff;
    }

    /*public Task(TaskType ty, Vector3 tarPos, Transform tarTrsf, GameResources tr, bool aut)
    : this(ty, tarPos, tarTrsf, tr, false, false)
    { 
    }

    public Task(TaskType ty, Vector3 tarPos,  Transform tarTrsf, List<GameResources> tr)
    : this(ty,tarPos,tarTrsf,tr,false)
    {
    }

    public Task(TaskType ty, Vector3 tarPos, Transform tarTrsf, bool cff)
    : this(ty, tarPos, tarTrsf, new List<GameResources>(), cff)
    {
    }
    public Task(TaskType ty, Vector3 tarPos, Transform tarTrsf)
    : this(ty, tarPos, tarTrsf, new List<GameResources>())
    {
    }

    public Task(TaskType ty, Vector3 tar, bool cff)
    : this(ty, tar, null,cff)
    {
    }
    public Task(TaskType ty, Vector3 tar)
    : this(ty,tar,null)
    {
    }

    public Task(TaskType ty, Transform tar)
    : this(ty,Vector2.zero,tar)
    {
    }*/
    public Task(TaskType ty, Vector3 tarPos, Transform tarTrsf, List<GameResources> tr)
    : this(ty, tarPos, tarTrsf, tr, false, false)
    {
    }

    public Task(TaskType ty, Vector3 tarPos, Transform tarTrsf, bool aut, bool cff)
    : this(ty, tarPos, tarTrsf, new List<GameResources>(), aut, cff)
    {
    }
    public Task(TaskType ty, Vector3 tarPos, Transform tarTrsf)
    : this(ty, tarPos, tarTrsf, false, false)
    {
    }
    public Task(TaskType ty, Vector3 tar, bool aut, bool cff)
    : this(ty, tar, null, aut, cff)
    {
    }
    public Task(TaskType ty, Transform tar, bool aut, bool cff)
    : this(ty, tar.position, tar, aut, cff)
    {
    }
    public Task(TaskType ty, Vector3 tar)
    : this(ty, tar, null)
    {
    }
    public Task(TaskType ty, Transform tar)
    : this(ty, tar.position, tar)
    {
    }

    //    Task(TaskType.Walk, targetPosition, target)

    public Task(TaskData td)
    {
        taskData = td;
        taskType = td.taskType;
        target = Grid.ToWorld(td.targetX, td.targetY);
        Node targetNode = Grid.GetNode(td.targetX, td.targetY);
        if(targetNode)
        {
            targetTransform = targetNode.nodeObject;
        }
        taskTime = td.taskTime;
        automated = td.automated;
        taskRes = new List<GameResources>();
        for(int i = 0; i < td.taskRes.Length; i++)
            if(td.taskRes[i] > 0)
                taskRes.Add(new GameResources(i,td.taskRes[i]));
    }

    public void SetupTarget()
    {
        setup = true;
        if(taskData == null) return;
        Node targetNode = Grid.GetNode(taskData.targetX, taskData.targetY);
        if(targetNode)
        {
            targetTransform = targetNode.nodeObject;
        }
    }

    public TaskData GetTaskData()
    {
        TaskData td = new TaskData();
        td.taskType = taskType;
        if(targetTransform != null) target = targetTransform.position;
        Node targetNode = Grid.GetNodeFromWorld(target);
        if(targetNode)
        {
            td.targetX = targetNode.gridX;
            td.targetY = targetNode.gridY;
        }
        else 
        {
            td.targetX = 0;
            td.targetY = 0;
        }
        td.taskTime = taskTime;
        td.automated = automated;
        td.taskRes = new int[100];
        foreach(GameResources res in taskRes)
        {
            td.taskRes[res.Id] += res.Amount;
        }
        return td;
    }
}
