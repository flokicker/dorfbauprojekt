using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BuildManager : Singleton<BuildManager>
{
    // Reference to our village
    [SerializeField]
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

    // All buildable prefabs
    [SerializeField]
    private List<GameObject> buildingPrefabList;

    // Ground plane where buildings are placed on
    private Plane groundPlane;

    // Blueprints
    [SerializeField]
    private GameObject bluePrintCanvas;
    public Material bluePrintMaterial;

    void Start()
    {
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
                int gx = rotation % 2 == 0 ? placingBuilding.GetGridWidth() : placingBuilding.GetGridHeight();
                int gy = rotation % 2 == 1 ? placingBuilding.GetGridWidth() : placingBuilding.GetGridHeight();

                Vector3 worldPosition = mouseRay.GetPoint(distance);
                int gridX = Mathf.RoundToInt(worldPosition.x / Grid.SCALE - gx/2f + 0.5f);
                int gridY = Mathf.RoundToInt(worldPosition.z / Grid.SCALE - gy/2f + 0.5f);
                gridX = (int)Mathf.Clamp(gridX, (-Grid.WIDTH / 2), (Grid.WIDTH / 2) - gx);
                gridY = (int)Mathf.Clamp(gridY, (-Grid.HEIGHT / 2), (Grid.HEIGHT / 2) - gy);
                
                hoverGridX = gridX + Grid.WIDTH/2;
                hoverGridY = gridY + Grid.HEIGHT/2;

                Vector3 hoverPos = Grid.ToWorld(hoverGridX, hoverGridY) - Grid.SCALE * new Vector3(0.5f, 0, 0.5f);
                hoverBuilding.transform.position = hoverPos + (new Vector3((float)gx / 2f, 0, (float)gy / 2f)) * Grid.SCALE;
                hoverBuilding.transform.eulerAngles = new Vector3(0, rotation * 90, 0);

                // Set node occupation temporary
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
        placingBuilding = new Building(placingBuildingID);
    }

    public static void EndPlacing()
    {
        placing = false;
        Grid.SetGridActive(false);
        DestroyImmediate(Instance.hoverBuilding.gameObject);
    }

    // Instantiate a new building at the same place as the hover building and disable it
    public static void PlaceBuilding()
    {
        Village myVillage = GameManager.GetVillage();
        bool canBuild = true;

        // Check if nodes are occupied
        int gx = Instance.rotation % 2 == 0 ? placingBuilding.GetGridWidth() : placingBuilding.GetGridHeight();
        int gy = Instance.rotation % 2 == 1 ? placingBuilding.GetGridWidth() : placingBuilding.GetGridHeight();
        for (int dx = 0; dx < gx; dx++)
        {
            for (int dy = 0; dy < gy; dy++)
            {
                if (Grid.Occupied(Instance.hoverGridX + dx, Instance.hoverGridY + dy)) canBuild = false;
            }
        
        }

        // Check if we have enough coins
        //if (myVillage.GetCoins() < placingBuilding.GetCost()) canBuild = false;

        /*for (int j = 0; j < GameResources.GetBuildingResourcesCount(); j++)
            if (myVillage.GetResources(j).GetAmount() < b.GetMaterialCost(j)) canBuild = false;*/

        if (canBuild)
        {
            GameObject newBuilding = (GameObject)Instantiate(Instance.buildingPrefabList[placingBuildingID], 
                Instance.hoverBuilding.transform.position, Instance.hoverBuilding.transform.rotation, myVillage.transform);
            GameObject canv = (GameObject)Instantiate(Instance.bluePrintCanvas, newBuilding.transform);
            canv.name = "CanvasBluePrint";
            BuildingScript bs = (BuildingScript)newBuilding.AddComponent<BuildingScript>();
            Transform t = newBuilding.transform;
            t.tag = "Building";
            placingBuilding.SetPosition(Instance.hoverGridX, Instance.hoverGridY);
            bs.SetBuilding(placingBuilding);
            for (int dx = 0; dx < gx; dx++)
            {
                for (int dy = 0; dy < gy; dy++)
                {
                    Grid.GetNode(Instance.hoverGridX + dx, Instance.hoverGridY + dy).nodeObject = t;
                }
            }

            // Take cost for coins 
            //myVillage.Purchase(b);

            myVillage.AddBuilding(bs);

            // If shift is pressed, don't exit the placing mode
            if (!Input.GetKey(KeyCode.LeftShift)) EndPlacing();

            InputManager.LeftClickHandled = true;
        }
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
                mat[i] = bluePrintMaterial;
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
                int gx = rotation % 2 == 0 ? b.GetGridWidth() : b.GetGridHeight();
                int gy = rotation % 2 == 1 ? b.GetGridWidth() : b.GetGridHeight();

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

            Village v = GameManager.singleton.GetVillage();
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
