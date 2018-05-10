using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PersonData : TransformData {

    public int nr;
    public string firstName, lastName;
    public Gender gender;
    public int age, invMatId, invMatAm, invFoodId, invFoodAm;
    public Disease disease;

    public int jobID, workingBuildingId;

    public float health, hunger;
    public float saturation;

    public List<TaskData> routine = new List<TaskData>();
}
