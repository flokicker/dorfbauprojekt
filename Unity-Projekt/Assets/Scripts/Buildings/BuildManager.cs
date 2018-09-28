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
    // ID of the currently being placed building
    public static Building placingBuilding;

    // Placing building hover-transform properties
    private Transform hoverBuilding;
    private int hoverGridX, hoverGridY;
    public int rotation = 0;

    [SerializeField]
    private Transform buildingParentTransform;
    // All buildable prefabs
    [SerializeField]
    private GameObject rangeCanvas, placeParticles;

    // Ground plane where buildings are placed on
    private Plane groundPlane;

    // blueprints
    [SerializeField]
    private GameObject blueprintCanvas, blueprintMaterialPanel;
    public Material blueprintMaterial;

    public BuildingScript cave, movingBuilding;

    void Start()
    {
        myVillage = GameManager.village;

        // initially not in placing mode
        placing = false;
        // first placable building is ID=1
        placingBuilding = Building.Get(1);

        // Setup ground plane with reference point of activeTerain
        groundPlane = new Plane(Vector3.up, Vector3.zero);

        // init movingBuilding to null
        movingBuilding = null;
    }

    void Update()
    {
        // Update all placing building functions
        if(placing)
        {
            int oldRot = rotation;
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

                float buildDistX = Grid.WIDTH / 2;
                float buildDistY = Grid.HEIGHT / 2;

                buildDistX = cave.BuildRange;
                buildDistY = cave.BuildRange;

                if(true) // || GameManager.InRange(Grid.ToWorld(gridX + Grid.WIDTH/2, gridY + Grid.HEIGHT/2), cave.transform.position, cave.buildRange))
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

                    if(hoverGridX != oldX || hoverGridY != oldY || rotation != oldRot)
                    {
                        bool placable = true;

                        int oldGx = oldRot % 2 == 0 ? placingBuilding.gridWidth : placingBuilding.gridHeight;
                        int oldGy = oldRot % 2 == 1 ? placingBuilding.gridWidth : placingBuilding.gridHeight;

                        // Disable old occupation temporary
                        for (int dx = 0; dx < oldGx; dx++)
                        {
                            for (int dy = 0; dy < oldGy; dy++)
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
                                if(!GameManager.InRange(Grid.ToWorld(hoverGridX + dx, hoverGridY + dy), cave.transform.position, cave.BuildRange)) placable = false;
                                else checkNode.SetTempOccupied(true, placingBuilding.showGrid);
                            }
                        }
                        //hoverBuilding.GetComponent<cakeslice.Outline>().color = placable ? 0 : 2;
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

        Instance.hoverBuilding = (Instantiate(placingBuilding.models, Vector3.zero, Quaternion.identity)).transform;
        //Instance.hoverBuilding.gameObject.AddComponent<cakeslice.Outline>();
    }

    // start moving building
    public static void StartMoving(BuildingScript b)
    {
        // only move building if movable
        if (!b.Movable) return;

        Instance.movingBuilding = b;
        placingBuilding = b.Building;
        StartPlacing();
    }

    // Exiting placing mode
    public static void EndPlacing()
    {
        placing = false;
        
        Instance.movingBuilding = null;

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
                if(Instance.cave && !GameManager.InRange(Grid.ToWorld(Instance.hoverGridX + dx, Instance.hoverGridY + dy), Instance.cave.transform.position, Instance.cave.BuildRange)) 
                    canBuild = false;
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

            BuildingScript mb = Instance.movingBuilding;
            if(mb) // moving an existing building
            {
                int oldGx = mb.Orientation % 2 == 0 ? mb.GridWidth : mb.GridHeight;
                int oldGy = mb.Orientation % 2 == 1 ? mb.GridWidth : mb.GridHeight;
                for (int dx = 0; dx < oldGx; dx++)
                {
                    for (int dy = 0; dy < oldGy; dy++)
                    {
                        if(!Grid.ValidNode(mb.GridX + dx, mb.GridY + dy)) continue;
                        Node n = Grid.GetNode(mb.GridX + dx, mb.GridY + dy);
                        n.SetNodeObject(null);
                        if(!mb.Walkable)  n.objectWalkable = true;
                    }
                }
                
                mb.transform.position = Instance.hoverBuilding.position;
                mb.transform.rotation = Instance.hoverBuilding.rotation;
                mb.SetPosRot(Instance.hoverGridX, Instance.hoverGridY, Instance.rotation);
                
                for (int dx = 0; dx < gx; dx++)
                {
                    for (int dy = 0; dy < gy; dy++)
                    {
                        if(!Grid.ValidNode(Instance.hoverGridX + dx, Instance.hoverGridY + dy)) continue;
                        Node n = Grid.GetNode(Instance.hoverGridX + dx, Instance.hoverGridY + dy);
                        n.SetNodeObject(mb.transform);
                        if(!mb.Walkable)  n.objectWalkable = false;
                        n.gameObject.SetActive(mb.Building.showGrid);
                    }
                }
            }
            else
            {
                GameBuilding toSpawn = new GameBuilding(placingBuilding, Instance.hoverGridX, Instance.hoverGridY, Instance.rotation);
                toSpawn.SetPosition(Instance.hoverBuilding.position);
                toSpawn.SetRotation(Instance.hoverBuilding.rotation);
                toSpawn.blueprint = true;
                SpawnBuilding(toSpawn);
            }

            // Take cost for coins 
            //myVillage.Purchase(b);
            
            // If shift is pressed, don't exit the placing mode
            if (mb || !Input.GetKey(KeyCode.LeftShift) || !placingBuilding.multipleBuildings) EndPlacing();

            InputManager.LeftClickHandled = true;
        }
    }
    
    public static bool IsPlacingBuilding(Building b)
    {
        return placingBuilding.id == b.id;
    }

    public static BuildingScript SpawnBuilding(GameBuilding gameBuilding)
    {

        // Spawn prefab
        GameObject newBuilding = Instantiate(gameBuilding.building.models, 
            gameBuilding.GetPosition(), gameBuilding.GetRotation(), Instance.buildingParentTransform);

        // Add BuildingScript
        BuildingScript bs = newBuilding.AddComponent<BuildingScript>();
        bs.SetBuilding(gameBuilding);

        // Add fog of war influencer if building is starter building
        if(bs.Name == "Höhle")
        {
            SimpleFogOfWar.FogOfWarInfluence fowi = newBuilding.AddComponent<SimpleFogOfWar.FogOfWarInfluence>();
            fowi.ViewDistance = bs.ViewRange;
        }

        GameObject pc = Instantiate(Instance.placeParticles, bs.transform);
        
        // Blueprint UI
        GameObject canvRange = Instantiate(Instance.rangeCanvas);
        canvRange.transform.SetParent(newBuilding.transform, false);
        canvRange.name = "CanvasRange";
        canvRange.SetActive(false);
        GameObject canvBlueprint = Instantiate(Instance.blueprintCanvas);
        canvBlueprint.transform.SetParent(newBuilding.transform, false);
        canvBlueprint.name = "CanvasBlueprint";
        if (bs.Blueprint)
        {
            foreach (GameResources res in bs.CostResource)
            {
                if (res.Amount == 0) continue;
                GameObject materialPanel = (GameObject)Instantiate(Instance.blueprintMaterialPanel, canvBlueprint.transform.Find("Cost"));
                materialPanel.transform.Find("TextMat").GetComponent<Text>().text = "0/" + res.Amount;
                materialPanel.transform.Find("ImageMat").GetComponent<Image>().sprite = res.Icon;
            }
        }
        canvBlueprint.transform.Find("ButtonCancel").GetComponent<Button>().onClick.AddListener(() => bs.DestroyBuilding());

        // Set Grid
        int gx = gameBuilding.orientation % 2 == 0 ? bs.GridWidth : bs.GridHeight;
        int gy = gameBuilding.orientation % 2 == 1 ? bs.GridWidth : bs.GridHeight;
        for (int dx = 0; dx < gx; dx++)
        {
            for (int dy = 0; dy < gy; dy++)
            {
                if(!Grid.ValidNode(gameBuilding.gridX + dx, gameBuilding.gridY + dy)) continue;

                Node n = Grid.GetNode(gameBuilding.gridX + dx, gameBuilding.gridY + dy);
                n.SetNodeObject(newBuilding.transform);
                if(!bs.Walkable) n.objectWalkable = false;
                n.gameObject.SetActive(gameBuilding.building.showGrid);
            }
        }
        
        if(bs.Name == "Höhle") Instance.cave = bs;

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
