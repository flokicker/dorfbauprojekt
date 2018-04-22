using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PersonData {

    public int nr;
    public string firstName, lastName;
    public Gender gender;
    public int age, invMatId, invMatAm, invFoodId, invFoodAm;

    public int jobID, workingBuildingId;

    public float health, hunger;
    public float saturation;

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

    //public Task task;
}
