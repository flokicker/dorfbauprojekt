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
    private bool walkable, isWater, tempOccupied, peopleOccupied;
    public bool onClosedList, onOpenList;

    public bool objectWalkable;
    public int occ = 0;

    public int id;

    void Start()
    {
    }

    void Update()
    {

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

    void LateUpdate()
    {
        if(!tempOccupied && !IsOccupied() || !Walkable() || (nodeObject && nodeObject.tag == Building.Tag && !nodeObject.GetComponent<BuildingScript>().Building.showGrid)) gameObject.SetActive(false);
        if(IsOccupied()) GetComponent<MeshRenderer>().material = Grid.Instance.redGridMaterial;
        else if(tempOccupied) GetComponent<MeshRenderer>().material = Grid.Instance.tempMaterial;
        else GetComponent<MeshRenderer>().material = Grid.Instance.greenGridMaterial;
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

    public void Init(int x, int y, bool wal, bool wat)
    {
        objectWalkable = true;
        walkable = wal;
        isWater = wat;
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

    public bool IsWater()
    {
        return isWater;
    }
    public bool Walkable()
    {
        return walkable;
    }
    public bool IsOccupied()
    {
        //if (!walkable) return true;
        if (nodeObject == null) return false;
        return nodeObject.gameObject.activeSelf || nodeObject.GetComponent<HideableObject>() != null;
    }
    public bool IsPath()
    {
        if (nodeObject == null) return false;
        BuildingScript bs = nodeObject.GetComponent<BuildingScript>();
        if (bs == null) return false;
        return bs.Type == BuildingType.Path;
    }
    public bool IsTempOccupied()
    {
        return tempOccupied;
    }
    public bool IsPeopleOccupied()
    {
        return peopleOccupied;
    }
    public float Weight()
    {
        if (isWater) return 2f;
        if (nodeObject == null) return 1f;
        BuildingScript bs = nodeObject.GetComponent<BuildingScript>();
        if (bs == null) return 1f;
        return bs.Type == BuildingType.Path ? 1f/1.3f : 1f;
    }
    public bool StartFromHere()
    {
        if (nodeObject == null) return objectWalkable;
        BuildingScript bs = nodeObject.GetComponent<BuildingScript>();
        return objectWalkable || (bs && bs.HasEntry);
    }

    public void SetTempOccupied(bool tmp, bool activate)
    {
        tempOccupied = tmp;

        if(tmp && !gameObject.activeSelf) gameObject.SetActive(activate);
        LateUpdate();
    }
    public void SetPeopleOccupied(bool tmp)
    {
        peopleOccupied = tmp;
    }

    public void SetNodeObject(Transform transform)
    {
        nodeObject = transform;
        if(nodeObject && !gameObject.activeSelf) gameObject.SetActive(true);
        LateUpdate();
    }
    
    private static float Dist(int x0, int y0, int x1, int y1)
    {
        int dx = x1 - x0;
        int dy = y1 - y0;
        return Mathf.Sqrt(dx * dx + dy * dy);
    }
}
