using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PersonData : TransformData {

    public int nr;
    public string firstName, lastName;
    public Gender gender;
    public int age, lifeTimeYears, lifeTimeDays, invMatId, invMatAm, invFoodId, invFoodAm;
    public Disease disease;

    public int jobID, workingBuildingId, noTaskBuildingId;

    public float health, hunger;
    public float saturation;

    public List<TaskData> routine = new List<TaskData>();

    public int motherNr;
    public bool pregnant;
    public float pregnancyTime;
}
