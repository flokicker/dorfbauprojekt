using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour{

    // Grid properties
    public int gridX, gridY;
    public Transform nodeObject;

    // A* algorithm parameters
    private Node parent;
    private float gValue, hValue;
    private bool walkable, tempOccupied, peopleOccupied;
    public bool onClosedList, onOpenList;

    public bool objectWalkable;
    public int occ = 0;

    public int id;

    void Start()
    {
    }

    void Update()
    {
        if(!tempOccupied && !IsOccupied()) gameObject.SetActive(false);

        if(nodeObject)
        {
            HideableObject ho = nodeObject.gameObject.GetComponent<HideableObject>();
            if(ho && ho.isHidden) gameObject.SetActive(false);
        }
        if (CheckNodeObject())//nodeObject != null && !nodeObject.gameObject.activeSelf && )
        {
            DestroyImmediate(nodeObject.gameObject);
            nodeObject = null;
        }
    }

    public bool CheckNodeObject()
    {
        if(nodeObject == null) return false;

        HideableObject ho = nodeObject.gameObject.GetComponent<HideableObject>();
        if(!nodeObject.gameObject.activeSelf) 
        {
            if(ho == null) {
                return true;
            }
            else
            {
                return !ho.isHidden;
            }
        }   
        return false;
    }

    public void Init(int x, int y, bool w)
    {
        objectWalkable = true;
        walkable = w;
        gridX = x;
        gridY = y;
        ResetHeuristics();
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

    public void Reset(int id, int endX, int endY)
    {
        this.id = id;
        ResetHeuristics();
        SetH(Dist(gridX,gridY,endX,endY));
        onClosedList = false;
        onOpenList = false;
    }

    public bool Walkable()
    {
        return walkable;
    }
    public bool IsOccupied()
    {
        return nodeObject != null && (nodeObject.gameObject.activeSelf ||nodeObject.GetComponent<HideableObject>() != null) || !walkable;
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

        if(tmp && !gameObject.activeSelf) gameObject.SetActive(true);
    }
    public void SetPeopleOccupied(bool tmp)
    {
        peopleOccupied = tmp;
    }

    public void SetNodeObject(Transform transform)
    {
        nodeObject = transform;

        if(nodeObject && !gameObject.activeSelf) gameObject.SetActive(true);
    }
    
    private static float Dist(int x0, int y0, int x1, int y1)
    {
        int dx = x1 - x0;
        int dy = y1 - y0;
        return Mathf.Sqrt(dx * dx + dy * dy);
    }
}
