﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    [SerializeField]
    private Village myVillage;

    public static float SCALE = 0.5f;
    public static int WIDTH = 80;
    public static int HEIGHT = 80;

    [SerializeField]
    private GameObject gridPlane;
    [SerializeField]
    private Material redGridMaterial, greenGridMaterial, tempMaterial;
    //private GameObject[,] gridPlaneObjects;

    private static bool gridShown;
    private Transform gridParent;
    private static Node[,] nodes;

    private Transform gridOverlay;
    private bool showGrid;

	void Start () 
    {
        // initialized grid
        nodes = new Node[WIDTH, HEIGHT];
        gridParent = myVillage.transform.Find("Grid");
        int groundLevelHeight = 1;
        for (int x = 0; x < WIDTH; x++)
        {
            for (int y = 0; y < HEIGHT; y++)
            {
                GameObject newNode = (GameObject)Instantiate(gridPlane, new Vector3((-WIDTH / 2 + x) * SCALE, 0.001f, (-HEIGHT / 2 + y) * SCALE), Quaternion.identity,gridParent);
                nodes[x, y] = newNode.GetComponent<Node>();
                float smph = Terrain.activeTerrain.SampleHeight(ToWorld(x, y));
                bool walkable = smph-groundLevelHeight > -0.2f;
                nodes[x, y].Init(x, y, walkable);
                SetGridOccupied(x, y, 0);
            }
        }

        // get reference to grid overlay
        gridOverlay = myVillage.transform.Find("GridOverlay");
	}
	
	void LateUpdate () 
    {
        // toggle gridoverlay with y
        if(Input.GetKeyDown(KeyCode.Y)) 
            showGrid = !showGrid;
            
        gridOverlay.gameObject.SetActive(showGrid || BuildManager.placing);

        // Enable/dsiable meshrenderer of nodes
        for (int x = 0; x < WIDTH; x++)
        {
            for (int y = 0; y < HEIGHT; y++)
            {
                Node cn = nodes[x, y];
                if(gridShown)
                {
                    int occ = Occupied(x, y) ? 0 : 1;
                    if (!cn.Walkable()) occ = 1;
                    if (cn.IsTempOccupied())
                    {
                        occ = 2;
                        cn.SetTempOccupied(false);
                    }
                    if (cn.IsPeopleOccupied())
                        occ = 3;
                    bool shown = occ != 1 && occ != 3;

                    int dx = Mathf.Abs(WIDTH/2-x);
                    int dy = Mathf.Abs(HEIGHT/2-y);

                    if(cn.nodeObject && !cn.nodeObject.gameObject.activeSelf) shown = false;

                    cn.GetComponent<MeshRenderer>().enabled = shown;
                    SetGridOccupied(x, y, occ);
                }
                else
                {
                    cn.GetComponent<MeshRenderer>().enabled = false;
                }
            }
        }
	}

    private void SetGridOccupied(int x, int y, int occ)
    {
        nodes[x, y].GetComponent<MeshRenderer>().material = occ == 2 ? tempMaterial : (occ== 0 ? redGridMaterial : greenGridMaterial);
    }
    public static Node GetNode(int x, int y)
    {
        return nodes[x, y];
    }
    public static bool Occupied(int x, int y)
    {
        return nodes[x, y].IsOccupied();
    }
    public static bool ValidNode(int x, int y)
    {
        return x >= 0 && y >= 0 && x < Grid.WIDTH && y < Grid.HEIGHT;
    }

    public static Vector3 ToWorld(int x, int y)
    {
        return new Vector3((-WIDTH / 2 + x) * SCALE, 0, (-HEIGHT / 2 + y) * SCALE);
    }

    public static Vector3 ToGrid(Vector3 worldPos)
    {
        return new Vector3(Mathf.RoundToInt(worldPos.x / SCALE + WIDTH / 2f), 0, Mathf.RoundToInt(worldPos.z / SCALE + HEIGHT / 2f));
    }
    public static Node GetNodeFromWorld(Vector3 worldPos)
    {
        Vector3 gp = ToGrid(worldPos);
        if(!ValidNode((int)gp.x, (int)gp.z)) return null;
        return GetNode((int)gp.x, (int)gp.z);
    }

    public static void SetGridActive(bool gridActive)
    {
        gridShown = gridActive;
    }
}
