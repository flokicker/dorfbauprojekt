using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : Singleton<Grid>
{
    public static float SCALE = 256f / 1024f * 2f;
    public static int WIDTH = 200;
    public static int HEIGHT = 200;

    [SerializeField]
    private GameObject gridPlane;
    [SerializeField]
    public Material redGridMaterial, greenGridMaterial, tempMaterial;
    //private GameObject[,] gridPlaneObjects;

    private static bool gridShown;
    private Transform gridParent;
    private static Node[,] nodes;

    private Transform gridOverlay;
    private bool showGrid;

	void Start () 
    {
        if(Application.isEditor) 
        {
            WIDTH = 50;
            HEIGHT = 50;
        }

        // initialized grid
        nodes = new Node[WIDTH, HEIGHT];
        gridParent = transform.Find("Grid");
        int groundLevelHeight = 1;
        for (int x = 0; x < WIDTH; x++)
        {
            for (int y = 0; y < HEIGHT; y++)
            {
                GameObject newNode = (GameObject)Instantiate(gridPlane, new Vector3((-WIDTH / 2 + x) * SCALE, 0.001f, (-HEIGHT / 2 + y) * SCALE), Quaternion.identity,gridParent);
                nodes[x, y] = newNode.GetComponent<Node>();
                float smph = Terrain.activeTerrain.SampleHeight(ToWorld(x, y));
                bool walkable = smph-groundLevelHeight > -0.1f;
                nodes[x, y].Init(x, y, walkable);
                nodes[x, y].gameObject.SetActive(false);
                //SetGridOccupied(x, y, 0);
            }
        }

        // get reference to grid overlay
        gridOverlay = transform.Find("GridOverlay");
        int col = 39;
        int row = 39;
        gridOverlay.GetComponent<GridOverlay>()._columns = col;
        gridOverlay.GetComponent<GridOverlay>()._rows = row;
        gridOverlay.GetComponent<GridOverlay>()._gridSize = new Vector2(col * SCALE, row * SCALE);
        gridOverlay.GetComponent<GridOverlay>().UpdateGrid();

    }
	
	void LateUpdate () 
    {
        // toggle gridoverlay with y
        if(Input.GetKeyDown(KeyCode.Y) && InputManager.InputUI()) 
            showGrid = !showGrid;

        //if(gridOverlay.gameObject.activeSelf != (showGrid || BuildManager.placing))
        //  UpdateNodes(Chunk(Camera.main.transform.position));

        gridOverlay.gameObject.SetActive(false);//showGrid || BuildManager.placing);
        gridParent.gameObject.SetActive(showGrid || BuildManager.placing);
	}
    
    /*private static int chunkSizeX = 20;
    private static int chunkSizeY = 20;
    public static int Chunk(Vector3 position)
    {
        Vector3 gridPos = Grid.ToGrid(position);
        return (int)((int)gridPos.x / (chunkSizeX))*HEIGHT/(chunkSizeY) + (int)(gridPos.z / (chunkSizeY));
    }*/

    // Enable/disable all nodes
    /*public void UpdateNodes()
    {
        for(int x = 0; x < WIDTH/chunkSizeX; x++)
        {
            for(int y = 0; y < HEIGHT/chunkSizeY; y++)
            {
                UpdateNodes(x*HEIGHT/chunkSizeY+y);
            }
        }
    }

    public void UpdateNodesNeighbourChunks(int midChunk)
    {
        UpdateNodes(midChunk);

        UpdateNodes(midChunk-1);
        UpdateNodes(midChunk+1);
        UpdateNodes(midChunk+HEIGHT/chunkSizeY);
        UpdateNodes(midChunk-HEIGHT/chunkSizeY);
        
        UpdateNodes(midChunk+HEIGHT/chunkSizeY+1);
        UpdateNodes(midChunk+HEIGHT/chunkSizeY-1);
        UpdateNodes(midChunk-HEIGHT/chunkSizeY+1);
        UpdateNodes(midChunk-HEIGHT/chunkSizeY-1);
    }

    // Enable/dsiable nodes
    public void UpdateNodes(int chunk)
    {
        int chunkCountY = HEIGHT/chunkSizeY;
        for (int x = (chunk/chunkCountY)*chunkSizeX; x < (chunk/chunkCountY+1)*chunkSizeX; x++)
        {
            for (int y = (chunk%chunkCountY)*chunkSizeY; y < (chunk%chunkCountY + 1)*chunkSizeY; y++)
            {
                if(!ValidNode(x,y)) continue;
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

                    if(cn.nodeObject && !cn.nodeObject.gameObject.activeSelf) shown = false;

                    cn.gameObject.SetActive(shown);
                    SetGridOccupied(x, y, occ);
                }
                else
                {
                    cn.gameObject.SetActive(false);
                }
            }
        }
    }

    /*private void SetGridOccupied(int x, int y, int occ)
    {
        nodes[x, y].GetComponent<MeshRenderer>().material = occ == 2 ? tempMaterial : (occ== 0 ? redGridMaterial : greenGridMaterial);
    }*/
    public static Node GetNode(int x, int y)
    {
        if(!ValidNode(x,y)) return null;
        return nodes[x, y];
    }
    public static bool Occupied(int x, int y)
    {
        if(!ValidNode(x,y)) return false;
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
