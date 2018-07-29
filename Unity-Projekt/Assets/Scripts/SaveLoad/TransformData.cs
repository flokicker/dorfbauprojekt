using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TransformData {

    public float posX, posY, posZ;
    public float rotX, rotY, rotZ;

    public void SetTransform(Transform trsf)
    {
        SetPosition(trsf.position);
        SetRotation(trsf.rotation);
    }

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
