using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BuildManager : Singleton<BuildManager>
{
    // Reference to our village
    private Village myVillage;

    // Is player currently placing an object in build mode
    public static bool placing;
    // ID of the currently being palced building and a default class
    public static int placingBuildingID;
    private static Building placingBuilding;
    // Placing building hover-transform properties
    [SerializeField]
    private Transform hoverBuilding;
    private int hoverGridX, hoverGridY;
    public int rotation = 0;

    [SerializeField]
    private Transform buildingParentTransform;
    // All buildable prefabs
    [SerializeField]
    public List<GameObject> buildingPrefabList;
    [SerializeField]
    private GameObject rangeCanvas;

    // Ground plane where buildings are placed on
    private Plane groundPlane;

    // blueprints
    [SerializeField]
    private GameObject blueprintCanvas, blueprintMaterialPanel;
    public Material blueprintMaterial;

    public Building cave;

    void Start()
    {
        myVillage = GameManager.village;

        // initially not in placing mode
        placing = false;
        // first placable building is ID=1
        placingBuildingID = 1;

        // Setup ground plane with reference point of activeTerain
        groundPlane = new Plane(Vector3.up, Vector3.zero);
    }

    void Update()
    {
        // Update all placing building functions
        if(placing)
        {
            // Rotate hover building
            if (Input.GetKeyDown(KeyCode.Comma)) rotation--;
            if (Input.GetKeyDown(KeyCode.Period)) rotation++;
            if (rotation > 3) rotation = 0;
            if (rotation < 0) rotation = 3;

            // Cast a ray onto the ground plane to get position 
            Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            float distance;
            if (groundPlane.Raycast(mouseRay, out distance))
            {
                // Center hover building around mousePosition
                int gx = rotation % 2 == 0 ? placingBuilding.gridWidth : placingBuilding.gridHeight;
                int gy = rotation % 2 == 1 ? placingBuilding.gridWidth : placingBuilding.gridHeight;

                Vector3 worldPosition = mouseRay.GetPoint(distance);
                int gridX = Mathf.RoundToInt(worldPosition.x / Grid.SCALE - gx/2f + 0.5f);
                int gridY = Mathf.RoundToInt(worldPosition.z / Grid.SCALE - gy/2f + 0.5f);

                float buildDistX =Grid.WIDTH / 2;
                float buildDistY =Grid.HEIGHT / 2;

                buildDistX = cave.buildRange;
                buildDistY = cave.buildRange;

                if(cave == null || true)// || GameManager.InRange(Grid.ToWorld(gridX + Grid.WIDTH/2, gridY + Grid.HEIGHT/2), cave.transform.position, cave.GetBuilding().buildRange))
                {
                    gridX = (int)Mathf.Clamp(gridX, (-buildDistX), (buildDistX) - gx + 1);
                    gridY = (int)Mathf.Clamp(gridY, (-buildDistY), (buildDistY) - gy + 1);

                    int oldX = hoverGridX;
                    int oldY = hoverGridY;

                    hoverGridX = gridX + Grid.WIDTH/2;
                    hoverGridY = gridY + Grid.HEIGHT/2;

                    Vector3 hoverPos = Grid.ToWorld(hoverGridX, hoverGridY) - Grid.SCALE * new Vector3(0.5f, 0, 0.5f);
                    Vector3 oldPos = new Vector3(hoverBuilding.transform.position.x, hoverBuilding.transform.position.y, hoverBuilding.transform.position.z);
                    hoverBuilding.transform.position = hoverPos + (new Vector3((float)gx / 2f, 0, (float)gy / 2f)) * Grid.SCALE;
                    hoverBuilding.transform.eulerAngles = new Vector3(0, rotation * 90, 0);

                    if(hoverGridX != oldX || hoverGridY != oldY)
                    {
                        bool placable = true;

                        // Disable old occupation temporary
                        for (int dx = 0; dx < gx; dx++)
                        {
                            for (int dy = 0; dy < gy; dy++)
                            {
                                if(!Grid.ValidNode(oldX + dx, oldY + dy)) continue;
                                Node checkNode = Grid.GetNode(oldX + dx, oldY + dy);
                                checkNode.SetTempOccupied(false, false);
                            }
                        }

                        // Set node occupation temporary
                        for (int dx = 0; dx < gx; dx++)
                        {
                            for (int dy = 0; dy < gy; dy++)
                            {
                                if(!Grid.ValidNode(hoverGridX + dx, hoverGridY + dy)) continue;
                                Node checkNode = Grid.GetNode(hoverGridX + dx, hoverGridY + dy);
                                if (checkNode.IsOccupied() || checkNode.IsPeopleOccupied()) placable = false;
                                else checkNode.SetTempOccupied(true, placingBuilding.showGrid);
                            }
                        }
                        hoverBuilding.GetComponent<cakeslice.Outline>().color = placable ? 0 : 2;
                    }
                    /*int newChunk = Grid.Chunk(hoverBuilding.transform.position);
                    Grid.Instance.UpdateNodesNeighbourChunks(newChunk);*/

                }
            }
        }
    }

    // setup hover building
    public static void StartPlacing()
    {
        if(placing) return;

        placing = true;
        Grid.SetGridActive(true);
        if(Instance.hoverBuilding)
            DestroyImmediate(Instance.hoverBuilding.gameObject);
        Instance.hoverBuilding = ((GameObject)Instantiate(Instance.buildingPrefabList[placingBuildingID], Vector3.zero, Quaternion.identity)).transform;
        Instance.hoverBuilding.gameObject.AddComponent<cakeslice.Outline>();
        placingBuilding = Instance.hoverBuilding.gameObject.AddComponent<Building>();
        placingBuilding.FromID(placingBuildingID);
        placingBuilding.prototype = true;
        placingBuilding.enabled = false;
    }

    // Exiting placing mode
    public static void EndPlacing()
    {
        placing = false;

        // Center hover building around mousePosition
        int gx = Instance.rotation % 2 == 0 ? placingBuilding.gridWidth : placingBuilding.gridHeight;
        int gy = Instance.rotation % 2 == 1 ? placingBuilding.gridWidth : placingBuilding.gridHeight;
        // Disable old occupation temporary
        for (int dx = 0; dx < gx; dx++)
        {
            for (int dy = 0; dy < gy; dy++)
            {
                if(!Grid.ValidNode(Instance.hoverGridX + dx, Instance.hoverGridY + dy)) continue;
                Node checkNode = Grid.GetNode(Instance.hoverGridX + dx, Instance.hoverGridY + dy);
                checkNode.SetTempOccupied(false, false);
            }
        }

        Grid.SetGridActive(false);
        DestroyImmediate(Instance.hoverBuilding.gameObject);
    }

    // Instantiate a new building at the same place as the hover building and disable it
    public static void PlaceBuilding()
    {
        Village myVillage = GameManager.village;
        bool canBuild = true;

        // Check if nodes are occupied
        int gx = Instance.rotation % 2 == 0 ? placingBuilding.gridWidth : placingBuilding.gridHeight;
        int gy = Instance.rotation % 2 == 1 ? placingBuilding.gridWidth : placingBuilding.gridHeight;
        for (int dx = 0; dx < gx; dx++)
        {
            for (int dy = 0; dy < gy; dy++)
            {
                if (Grid.Occupied(Instance.hoverGridX + dx, Instance.hoverGridY + dy)) canBuild = false;
            }
        }

        // Check if we have enough coins
        //if (myVillage.GetCoins() < placingBuilding.GetCost()) canBuild = false;

        if (canBuild)
        {
            // Disable old occupation temporary
            for (int dx = 0; dx < gx; dx++)
            {
                for (int dy = 0; dy < gy; dy++)
                {
                    if(!Grid.ValidNode(Instance.hoverGridX + dx, Instance.hoverGridY + dy)) continue;
                    Node checkNode = Grid.GetNode(Instance.hoverGridX + dx, Instance.hoverGridY + dy);
                    checkNode.SetTempOccupied(false, false);
                }
            }

            SpawnBuilding(placingBuildingID, Instance.hoverBuilding.position, Instance.hoverBuilding.rotation, 
            Instance.rotation, Instance.hoverGridX, Instance.hoverGridY, true);

            // Take cost for coins 
            //myVillage.Purchase(b);
            
            // If shift is pressed, don't exit the placing mode
            if (!Input.GetKey(KeyCode.LeftShift) || !placingBuilding.multipleBuildings) EndPlacing();

            InputManager.LeftClickHandled = true;
        }
    }

    public static Building SpawnBuilding(BuildingData bd)
    {
        Building b = SpawnBuilding(bd.id, bd.GetPosition(), bd.GetRotation(), bd.orientation, bd.gridX, bd.gridY, bd.blueprint);
        b.SetBuildingData(bd);
        return b;
    }
    public static Building SpawnBuilding(int buildingId, Vector3 pos, Quaternion rot, float rotInt, int gridX, int gridY, bool blueprint)
    {
        GameObject newBuilding = (GameObject)Instantiate(Instance.buildingPrefabList[buildingId], 
            pos, rot, Instance.buildingParentTransform);
        GameObject canvRange = (GameObject)Instantiate(Instance.rangeCanvas, newBuilding.transform);
        canvRange.name = "CanvasRange";
        canvRange.SetActive(false);
        GameObject canvBlueprint = (GameObject)Instantiate(Instance.blueprintCanvas, newBuilding.transform);
        canvBlueprint.name = "CanvasBlueprint";
        Building bs = (Building)newBuilding.AddComponent<Building>();
        bs.FromID(buildingId);
        bs.blueprint = blueprint;
        Transform t = newBuilding.transform;
        t.tag = "Building";
        bs.SetPosition(gridX, gridY);
        int gx = rotInt % 2 == 0 ? bs.gridWidth : bs.gridHeight;
        int gy = rotInt % 2 == 1 ? bs.gridWidth : bs.gridHeight;
        if(blueprint)
        {
            for(int i = 0; i < bs.materialCost.Length; i++)
            {
                int cost = bs.materialCost[i];
                if(cost == 0) continue;
                GameObject materialPanel = (GameObject)Instantiate(Instance.blueprintMaterialPanel, canvBlueprint.transform);
                materialPanel.transform.Find("TextMat").GetComponent<Text>().text = "0/"+cost;
                materialPanel.transform.Find("ImageMat").GetComponent<Image>().sprite = UIManager.Instance.resourceSprites[i];
            }
        }

        for (int dx = 0; dx < gx; dx++)
        {
            for (int dy = 0; dy < gy; dy++)
            {
                if(!Grid.ValidNode(gridX + dx, gridY + dy)) continue;
                Grid.GetNode(gridX + dx, gridY + dy).SetNodeObject(t);
                if(!bs.walkable)  Grid.GetNode(gridX + dx, gridY + dy).objectWalkable = false;
            }
        }

        if(bs.id == 0) Instance.cave = bs;

        return bs;
    }
}
       /* int buildingType = VillageUIManager.Instance.GetPlacingBuilding();
        Building b = new Building(buildingType);
        if (VillageUIManager.Instance.GetBuildingMode() != buildingMode)
        {
            buildingMode = VillageUIManager.Instance.GetBuildingMode();
            Grid.SetGridActive(buildingMode == 0);
            DestroyImmediate(hoverBuilding.gameObject);
            hoverBuilding = ((GameObject)Instantiate(buildingPrefabList[buildingType], Vector3.zero, Quaternion.identity)).transform;
            /*Material[] mat = hoverBuilding.GetComponent<MeshRenderer>().materials;
            for (int i = 0; i < mat.Length; i++)
                mat[i] = blueprintMaterial;
            hoverBuilding.GetComponent<MeshRenderer>().materials = mat;
            hoverBuilding.gameObject.AddComponent(typeof(cakeslice.Outline));
            hoverBuilding.gameObject.SetActive(buildingMode == 0);
        }
        Ray mouseRay;
        float distance;
        if (buildingMode == 0)
        {
            if (Input.GetKeyDown(KeyCode.Comma)) rotation--;
            if (Input.GetKeyDown(KeyCode.Period)) rotation++;
            if (rotation > 3) rotation = 0;
            if (rotation < 0) rotation = 3;


            mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (groundPlane.Raycast(mouseRay, out distance))
            {
                int gx = rotation % 2 == 0 ? bgridWidth : bgridHeight;
                int gy = rotation % 2 == 1 ? bgridWidth : bgridHeight;

                Vector3 worldPosition = mouseRay.GetPoint(distance);
                int gridX = Mathf.RoundToInt(worldPosition.x / Grid.SCALE);
                int gridY = Mathf.RoundToInt(worldPosition.z / Grid.SCALE);
                gridX = (int)Mathf.Clamp(gridX, (-Grid.WIDTH / 2), (Grid.WIDTH / 2) - gx);
                gridY = (int)Mathf.Clamp(gridY, (-Grid.HEIGHT / 2), (Grid.HEIGHT / 2) - gy);
                
                hoverGridX = gridX + Grid.WIDTH/2;
                hoverGridY = gridY + Grid.HEIGHT/2;

                Vector3 hoverPos = Grid.ToWorld(hoverGridX, hoverGridY) - Grid.SCALE * new Vector3(0.5f, 0, 0.5f);
                hoverBuilding.transform.position = hoverPos + (new Vector3((float)gx / 2f, 0, (float)gy / 2f)) * Grid.SCALE;
                hoverBuilding.transform.eulerAngles = new Vector3(0, rotation * 90, 0);

                bool placable = true;
                for (int dx = 0; dx < gx; dx++)
                {
                    for (int dy = 0; dy < gy; dy++)
                    {
                        Node checkNode = Grid.GetNode(hoverGridX + dx, hoverGridY + dy);
                        if (checkNode.IsOccupied() || checkNode.IsPeopleOccupied()) placable = false;
                        else checkNode.SetTempOccupied(true);
                    }
                }

                hoverBuilding.GetComponent<cakeslice.Outline>().color = placable ? 0 : 2;
            }
            else
            {

            }
        }
    }
    /*bs.b.FromType(buildingType);


                bool canBuy = true;

                if (myVillage.currentCurrency < bs.b.cost) canBuy = false;
                if (myVillage.currentFreePopulation < bs.b.populationUse) canBuy = false;
                for (int j = 0; j < myVillage.currentMaterials.Length; j++)
                    if (myVillage.currentMaterials[j] < bs.b.materialUse[j]) canBuy = false;

                if (canBuy)
                {
                    int gridX = Mathf.RoundToInt(worldPosition.x / GRID_SCALE);
                    int gridY = Mathf.RoundToInt(worldPosition.z / GRID_SCALE);
                    bs.gridX = gridX;//(int)((t.position.x - grid_start.x) / GRID_SCALE);
                    bs.gridY = gridY;//(int)((t.position.z - grid_start.z) / GRID_SCALE);
                    if (myVillage.AddBuilding(bs))
                    {
                        myVillage.currentCurrency -= bs.b.cost;
                        for (int j = 0; j < myVillage.currentMaterials.Length; j++)
                            myVillage.currentMaterials[j] -= bs.b.materialUse[j];

                        int toEmploy = bs.b.populationUse;
                        myVillage.EmployPeople(toEmploy);


                        if (!Input.GetKey(KeyCode.LeftShift))
                        {
                            buildingMode = -1;
                            placing = false;
                        }
                    }
                    else
                    {
                        Destroy(newBuilding);
                    }
                }
                else
                {
                    Destroy(newBuilding);
                    buildingMode = -1;
                    placing = false;
                }*/

    //Debug.Log(worldPosition);
    //worldPosition.x = Mathf.RoundToInt(worldPosition.x / GRID_SCALE + 0.5f);
    //worldPosition.z = Mathf.RoundToInt(worldPosition.z / GRID_SCALE + 0.5f);
    /*Vector3 pos = collision[0].point;
    pos.x -= (pos.x + GRID_SCALE/2f) % GRID_SCALE;
    pos.y = 0f;
    pos.z -= (pos.z + GRID_SCALE/2f) % GRID_SCALE;
    //(worldPosition + (new Vector3(0, 0.5f, 0) - new Vector3((float)b.gridSizeX / 2f, 0, (float)b.gridSizeY / 2f))) * GRID_SCALE + new Vector3(0, hoverBuilding.transform.localScale.y / 2 - 0.5f, 0);
    worldPosition.x = Mathf.Clamp(worldPosition.x, -GRID_WIDTH / 2 + 0.5f, GRID_WIDTH / 2 - 0.5f);
    worldPosition.z = Mathf.Clamp(worldPosition.z, -GRID_HEIGHT / 2 + 0.5f, GRID_HEIGHT / 2 - 0.5f);*/

    /*[SerializeField]
    private GameObject buildListItemPrefab;
    private List<Building> buildingList = new List<Building>();
    private Transform parentPanel;

	// Use this for initialization
	void Start () {
        buildingList = Building.GetAllBuildings();

        parentPanel = transform.Find("PanelBuildingList");
        for (int i = 0; i < buildingList.Count; i++)
        {
            Building b = buildingList[i];
            GameObject g = (GameObject)Instantiate(buildListItemPrefab, parentPanel);
            g.name = i.ToString();
            g.transform.Find("Text").GetComponent<Text>().text = b.name + " | " + b.cost + " "+Village.currencyName[UserManager.nationID]+" | " + b.populationUse + " Bewohner | " + b.materialUse[0] + " Holz";
            int j = i;
            g.GetComponent<Button>().onClick.AddListener(() => OnClickButton(j));
        }
	}

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < parentPanel.childCount; i++)
        {
            Transform t = parentPanel.GetChild(i);
            bool canBuy = true;
            Building b = buildingList[i];

            Village v = GameManager.singleton.village;
            if (v.currentCurrency < b.cost) canBuy = false;
            if (v.currentFreePopulation < b.populationUse) canBuy = false;
            for(int j = 0; j < v.currentMaterials.Length; j++)
                if (v.currentMaterials[j] < b.materialUse[j]) canBuy = false;

            t.GetComponent<Button>().interactable = canBuy;
            //t.Find("Text").GetComponent<Text>().text = buildingList[i].n
        }
	}

    void OnClickButton(int index)
    {
        GameManager.singleton.SetHoverBuildingType(buildingList[index].type);
    }*/
