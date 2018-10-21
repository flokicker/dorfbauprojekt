using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TerrainTextureType
{
    Grass, Path, Field, Building
}
public class TerrainModifier : MonoBehaviour {

    private static Terrain terrain;
    private static TerrainData terrainData;
    private static Vector3 terrainPos;

    // Use this for initialization
    void Start () {
        terrain = GetComponent<Terrain>();
        terrain.terrainData = TerrainDataCloner.Clone(terrain.terrainData);
        terrain.GetComponent<TerrainCollider>().terrainData = terrain.terrainData; // Don't forget to update the TerrainCollider as well

        terrainPos = terrain.transform.position;
        terrainData = terrain.terrainData;
    }

    public static void ChangeTexture(int startX, int startY, int sizeX, int sizeY, TerrainTextureType tp)
    {
        ChangeTexture(startX, startY, sizeX, sizeY, tp, 0, 0);
    }
    public static void ChangeTexture(int startX, int startY, int sizeX, int sizeY, TerrainTextureType tp, float randMin, float randMax)
    {
        Vector3 worldPos = Grid.ToWorld(startX, startY);
        worldPos -= new Vector3(0.5f, 0, 0.5f) * Grid.SCALE;

        sizeX *= 2;
        sizeY *= 2;

        /*if (sizeX == 2)
        {
            sizeX = 3;
            worldPos -= new Vector3(0.25f, 0, 0) * Grid.SCALE;
        }
        if (sizeY == 2)
        {
            sizeY = 3;
            worldPos -= new Vector3(0, 0, 0.25f) * Grid.SCALE;
        }*/

        int tex = 0;
        switch (tp)
        {
            case TerrainTextureType.Grass: tex = 0; break;
            case TerrainTextureType.Path: tex = 1; break;
            case TerrainTextureType.Field: tex = 5; break;
            case TerrainTextureType.Building: tex = 2; break;
        }

        // calculate which splat map cell the worldPos falls within (ignoring y)
        int mapX = (int)(((worldPos.x - terrainPos.x) / terrainData.size.x) * terrainData.alphamapWidth);
        int mapZ = (int)(((worldPos.z - terrainPos.z) / terrainData.size.z) * terrainData.alphamapHeight);

        // get the splat data for this cell as a 1x1xN 3d array (where N = number of textures)
        float[,,] splatmapData = terrainData.GetAlphamaps(mapX, mapZ, sizeX, sizeY);

        float spW = splatmapData.GetLength(0);
        float spH = splatmapData.GetLength(1);

        float midX = spW / 2 - 0.5f;
        float midY = spH / 2 - 0.5f;
        float maxR = midX * midX + midY * midY;

        for (int x = 0; x < spW; x++)
        {
            for (int y = 0; y < spH; y++)
            {
                float randPx = Random.Range(randMin, randMax);
                for (int i = 0; i < splatmapData.GetLength(2); i++)
                {
                    float r = (midX - x) * (midX - x) + (midY - y) * (midY - y);
                    float alpha = 1f - (r / maxR);

                    alpha += randPx;

                    if (spW <= 2) alpha = 0.5f;

                    if (i == 0) alpha = 1f - alpha;
                    else if (i != tex) alpha = 0;
                    splatmapData[x, y, i] = alpha;
                }
            }
        }

        terrainData.SetAlphamaps(mapX, mapZ, splatmapData);
        //terrain.Flush();
    }

    public static void ChangeTrees(int startX, int startY, int sizeX, int sizeY, bool add)
    {
        Vector3 worldPos = Grid.ToWorld(startX, startY) + new Vector3(sizeX, 0, sizeY) * Grid.SCALE * 0.5f - new Vector3(1, 0, 1) * 0.5f * Grid.SCALE;
        float radius = Mathf.Sqrt(sizeX * sizeX + sizeY * sizeY) * 0.45f;
        //Rect area = new Rect(worldPos.x, worldPos.z, sizeX*Grid.SCALE, sizeY*Grid.SCALE);

        ArrayList instances = new ArrayList();

        foreach (TreeInstance tree in terrainData.treeInstances)
        {
            Vector3 treePos = Vector3.Scale(tree.position, terrainData.size) + terrain.transform.position;
            //Debug.Log("instance check: " + (treePos-worldPos).x +";"+(treePos-worldPos).z+" -- d:"+ Vector3.Distance(treePos, worldPos)+";r"+radius);
            if (Vector3.Distance(treePos, worldPos) > radius)
            {
                // tree is out of range - keep it
                instances.Add(tree);
            }
            else
            {
            }
        }
        terrainData.treeInstances = (TreeInstance[])instances.ToArray(typeof(TreeInstance));
    }
    public static void ChangeGrass(int startX, int startY, int sizeX, int sizeY, bool add)
    {
        Vector3 worldPos = Grid.ToWorld(startX, startY);
        worldPos -= new Vector3(0.5f, 0, 0.5f) * Grid.SCALE;
        sizeX *= 2;
        sizeY *= 2;

        // calculate which splat map cell the worldPos falls within (ignoring y)
        int mapX = (int)(((worldPos.x - terrainPos.x) / terrainData.size.x) * terrainData.detailWidth);
        int mapZ = (int)(((worldPos.z - terrainPos.z) / terrainData.size.z) * terrainData.detailHeight);

        // read all detail of layer 4 into a 2D int array:
        int[,] detailMapData = new int[sizeX, sizeY];
        detailMapData = terrainData.GetDetailLayer(mapX, mapZ, sizeX, sizeY, 4);
        for(int x = 0; x < detailMapData.GetLength(0); x++)
        {
            for(int y = 0; y < detailMapData.GetLength(1); y++)
            {
                detailMapData[x, y] = 0;
            }
        }

        terrainData.SetDetailLayer(mapX, mapZ, 4, detailMapData);
        //terrain.Flush();
    }

    // Update is called once per frame
    void Update () {
	}
}
