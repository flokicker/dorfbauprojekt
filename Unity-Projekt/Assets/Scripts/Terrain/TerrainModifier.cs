﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainModifier : MonoBehaviour {

    private Terrain terrain;
    private TerrainData terrainData;
    private Vector3 terrainPos;

    // Use this for initialization
    void Start () {
        terrain = GetComponent<Terrain>();
        terrain.terrainData = TerrainDataCloner.Clone(terrain.terrainData);
        terrain.GetComponent<TerrainCollider>().terrainData = terrain.terrainData; // Don't forget to update the TerrainCollider as well

        terrainPos = terrain.transform.position;
        terrainData = terrain.terrainData;

        AddPath(Grid.WIDTH/2, Grid.HEIGHT/2, 12, 8);
    }

    public void AddPath(int startX, int startY, int sizeX, int sizeY)
    {
        Vector3 worldPos = Grid.ToWorld(startX, startY);

        // calculate which splat map cell the worldPos falls within (ignoring y)
        int mapX = (int)(((worldPos.x - terrainPos.x) / terrainData.size.x) * terrainData.alphamapWidth);
        int mapZ = (int)(((worldPos.z - terrainPos.z) / terrainData.size.z) * terrainData.alphamapHeight);

        // get the splat data for this cell as a 1x1xN 3d array (where N = number of textures)
        float[,,] splatmapData = terrainData.GetAlphamaps(mapX, mapZ, sizeX, sizeY);

        for (int x = 0; x < splatmapData.GetLength(0); x++)
            for (int y = 0; y < splatmapData.GetLength(1); y++)
                for (int i = 0; i < splatmapData.GetLength(2); i++)
                    splatmapData[x, y, i] = (i == 7 ? 1 : 0);

        terrainData.SetAlphamaps(mapX, mapZ, splatmapData);
        terrain.Flush();
    }
	
	// Update is called once per frame
	void Update () {
	}
}
