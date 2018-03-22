using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour{

    private Node parent;
    private int gridX, gridY;
    private float gValue, hValue;
    private bool walkable, tempOccupied, peopleOccupied;
    public Transform nodeObject;
    public bool onClosedList, onOpenList;

    void Start()
    {
    }

    void Update()
    {
        if (nodeObject != null && !nodeObject.gameObject.activeSelf)
        {
            DestroyImmediate(nodeObject.gameObject);
            nodeObject = null;
        }
    }

    public void Init(int x, int y, bool w)
    {
        walkable = w;
        gridX = x;
        gridY = y;
        ResetHeuristics();
    }

    public int GetX()
    {
        return gridX;
    }
    public int GetY()
    {
        return gridY;
    }
    public void SetG(float g)
    {
        gValue = g;
    }
    public void SetH(float h)
    {
        hValue = h;
    }
    public float GetG()
    {
        return gValue;
    }
    public float GetF()
    {
        return gValue + hValue;
    }
    public void SetParent(Node p)
    {
        parent = p;
    }
    public Node GetParent()
    {
        return parent;
    }

    public void ResetHeuristics()
    {
        gValue = 0;
        hValue = 0;
    }

    public bool Walkable()
    {
        return walkable;
    }
    public bool IsOccupied()
    {
        return nodeObject != null && nodeObject.gameObject.activeSelf || !walkable;
    }
    public bool IsTempOccupied()
    {
        return tempOccupied;
    }
    public bool IsPeopleOccupied()
    {
        return peopleOccupied;
    }


    public void SetTempOccupied(bool tmp)
    {
        tempOccupied = tmp;
    }
    public void SetPeopleOccupied(bool tmp)
    {
        peopleOccupied = tmp;
    }
}
