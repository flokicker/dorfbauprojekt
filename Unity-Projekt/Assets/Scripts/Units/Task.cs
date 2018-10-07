using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum TaskType
{
    None, Walk, MineNatureObject, CollectMushroom, Harvest, BringToWarehouse, TakeFromWarehouse, 
    Campfire, PickupItem, Build, Fishing, Craft, HuntAnimal, FollowPerson, SacrificeResources,
    TakeIntoVillage, WorkOnField, GoWork
    // Fisherplace, TakeEnergySpot ProcessAnimal
}
/*[System.Serializable]
public class TaskData
{
    public TaskType taskType;
    public float taskTime;
    public bool automated;
    public int[] taskRes;
}*/
[System.Serializable]
public class Task
{
    public TaskType taskType;
    public float targetX, targetZ;
    public float targetTransformX, targetTransformZ;

    [System.NonSerialized]
    public Vector3 target;
    [System.NonSerialized]
    public Transform targetTransform;

    public float taskTime;
    public List<GameResources> taskRes;
    public bool automated, checkFromFar, setup = true;

    public Task(TaskType ty, Vector3 tarPos,  Transform tarTrsf, List<GameResources> tr, bool aut, bool cff)
    {
        taskType = ty;
        SetTarget(tarPos);
        targetTransform = tarTrsf;
        taskTime = 0f;
        taskRes = tr;
        automated = aut;
        checkFromFar = cff;

        Vector3 trsfPos = target;
        if(targetTransform)
        {
            trsfPos = targetTransform.position;
        }
        targetTransformX = trsfPos.x;
        targetTransformZ = trsfPos.z;
    }
    public Task(TaskType ty, Vector3 tarPos,  Transform tarTrsf, GameResources tr, bool aut, bool cff)
        : this(ty,tarPos,tarTrsf,new List<GameResources>(), aut, cff)
    {
        taskRes = new List<GameResources>();
        taskRes.Add(tr);
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

    public void SetTarget(Vector3 newTarget)
    {
        target = newTarget;
        targetX = target.x;
        targetZ = target.z;
    }
    public void SetupTarget()
    {
        setup = true;
        target = new Vector3(targetX, 0, targetZ);
        Node targetTransformNode = Grid.GetNodeFromWorld(new Vector3(targetTransformX, 0, targetTransformZ));
        if (targetTransformNode)
        {
            targetTransform = targetTransformNode.nodeObject;
        }
    }

    //    Task(TaskType.Walk, targetPosition, target)

    /*public Task(TaskData td)
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
    }*/
}
