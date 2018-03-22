﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStar : MonoBehaviour {

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
    void Update()
    {
	}

    /// <summary>
    /// Returns path in start-end order if found, otherwise an empty list
    /// </summary>
    public static List<Node> FindPath(int startX, int startY, int endX, int endY)
    {
        // Variable definition
        Node startNode = Grid.GetNode(startX, startY);
        Node endNode = Grid.GetNode(endX, endY);

        Transform startObject = startNode.nodeObject;
        if (startObject != null && startObject.tag == "NatureElement") startObject = null;
        Transform endObject = endNode.nodeObject;
        if (endObject != null && endObject.tag == "NatureElement") endObject = null;

        Node currentNode = startNode;

        List<Node> path = new List<Node>();
        List<Node> openlist = new List<Node>();

        // Reset every Node to default G and appropriate H Value
        for (int x = 0; x < Grid.WIDTH; x++)
        {
            for (int y = 0; y < Grid.HEIGHT; y++)
            {
                Node cn = Grid.GetNode(x, y);
                cn.ResetHeuristics();
                cn.SetH(Dist(x, y, endX, endY));
                cn.onClosedList = false;
                cn.onOpenList = false;
            }
        }

        // Begin with currentNode = startNode
        openlist.Add(currentNode);
        currentNode.onOpenList = true;

        // Count to prevent stalls
        int loopCount = 0;
        while (openlist.Count != 0)
        {
            loopCount++;
            if (loopCount > 10000) break;

            // Get node with minimum F-Value
            currentNode = GetMin(openlist);
            openlist.RemoveAt(0);
            currentNode.onOpenList = false;
            //openlist.Remove(currentNode);

            // If we have reached the endNode, we have found a path
            if (currentNode == endNode)
            {
                // Bcktrack path to start node
                path.Add(currentNode);
                while (currentNode != startNode)
                {
                    currentNode = currentNode.GetParent();
                    path.Add(currentNode);
                }
                // Return path in start-end order
                path.Reverse();
                break;
            }

            // add current node to closed list
            currentNode.onClosedList = true;

            int cx = currentNode.GetX();
            int cy = currentNode.GetY();

            // Find neighbours of currentNode
            int[] dx = { 1, -1, 0, 0, 1, -1, 1, -1 };
            int[] dy = { 0, 0, 1, -1, 1, -1, -1, 1 };
            // Weight of diagonal moves are sqrt(2)
            float[] weight = { 1, 1.4f };
            for (int i = 0; i < 8; i++)
            {
                // Check if neghbour exists in grid
                if (!Grid.ValidNode(cx + dx[i], cy + dy[i])) continue;

                Node neighbour = Grid.GetNode(cx + dx[i], cy + dy[i]);

                // Only update neighbour, if not already closed and if walkable
                if (neighbour.onClosedList || (neighbour.IsOccupied() && (startObject == null || neighbour.nodeObject != startObject) && 
                    (neighbour.nodeObject != endObject || endObject == null) && neighbour != endNode))
                    continue;

                // Set tentative G-Value
                float tg = currentNode.GetG() + weight[i / 4];

                bool contains = neighbour.onOpenList;

                // Only update G-Value of neighbour, if lower
                if (contains && tg >= neighbour.GetG())
                    continue;

                // Update neighbour
                neighbour.SetParent(currentNode);
                neighbour.SetG(tg);

                // Add neighbour to openList if not already contained
                if (!contains)
                {
                    InsertNode(openlist, neighbour);
                    neighbour.onOpenList = true;
                }
            }
        }

        // Code for staright path
        /*while (true)
        {
            path.Add(currentNode);

            int dx = endX - currentNode.gridX;
            int dy = endY - currentNode.gridY;

            int dxx = 0; int dyy = 0;

            if (dx > 0) dxx = 1;
            else if (dx < 0) dxx = -1;
            if (dy > 0) dyy = 1;
            else if (dy < 0) dyy = -1;

            if (dx == 0 && dy == 0) break;

            currentNode = Grid.GetNode(currentNode.gridX + dxx, currentNode.gridY + dyy);
        }*/

        path.Remove(startNode);
        /*if (endNode.nodeObject != null && endNode.nodeObject.tag == "Tree")
        {
            path.Remove(endNode);
        }*/

        return path;
    }

    private static Node GetMin(List<Node> openlist)
    {
        return openlist[0];
        Node min_node = null;
        foreach (Node n in openlist)
        {
            if (min_node == null || n.GetF() < min_node.GetF())
            {
                min_node = n;
            }
        }
        return min_node;
    }
    private static void InsertNode(List<Node> openlist, Node n)
    {
        for (int i = 0; i < openlist.Count; i++)
        {
            if (openlist[i].GetF() > n.GetF())
            {
                openlist.Insert(i, n);
                return;
            }
        }
        openlist.Add(n);
    }

    private static float Dist(int x0, int y0, int x1, int y1)
    {
        int dx = x1 - x0;
        int dy = y1 - y0;
        return Mathf.Sqrt(dx * dx + dy * dy);
    }
}