using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BuildManager : Singleton<BuildManager>
{
    [SerializeField]
    private Village myVillage;

    private int buildingMode = -1;
    private bool placing = false;

    private Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
    [SerializeField]
    private Transform hoverBuilding;
    private int hoverGridX, hoverGridY;
    public int rotation = 0;

    [SerializeField]
    private List<GameObject> buildingPrefabList;

    [SerializeField]
    private GameObject bluePrintCanvas;

    public Material bluePrintMaterial;

    void Start()
    {

    }

    void Update()
    {
        int buildingType = VillageUIManager.Instance.GetPlacingBuilding();
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
            hoverBuilding.GetComponent<MeshRenderer>().materials = mat;*/
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

        if (!EventSystem.current.IsPointerOverGameObject())
        {
            if (Input.GetMouseButtonDown(1) && buildingMode == -1)
            {
                mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(mouseRay, out hit, 1000))
                {
                    string tag = hit.transform.gameObject.tag;
                    Vector3 targetPos = Vector3.zero;
                    Transform targetTr = null;
                    int target = 0;
                    List<string> handledTags = new List<string>() { "Plant", "Special", "Building", "Item" };
                    if (tag == "Terrain")
                    {
                        targetPos = hit.point;
                        target = 1;
                        Vector3 hitGrid = Grid.ToGrid(targetPos);
                        Node hitNode = Grid.GetNode((int)hitGrid.x, (int)hitGrid.z);
                        if (hitNode.nodeObject != null)
                        {
                            tag = hitNode.nodeObject.tag;
                        }
                    }
                    if (handledTags.Contains(tag))
                    {
                        targetTr = hit.transform;
                        target = 2;
                    }
                    if (PersonScript.selectedPeople.Count > 0)
                    {
                        /*Vector2[] delta = { new Vector2(0,0),
                            new Vector2(-1, 0), new Vector2(0, -1), new Vector2(1, 0), new Vector2(0, 1),
                            new Vector2(-1,-1), new Vector2(1,-1), new Vector2(1,1), new Vector2(-1, 1) };*/
                        List<Vector2> delta = new List<Vector2>();
                        for (float r = 0; r < 10; r+=0.5f)
                        {
                            for (int x = (int)-r; x <= r; x++)
                            {
                                for (int y = (int)-r; y <= r; y++)
                                {
                                    if(x*x + y*y <= r*r && !delta.Contains(new Vector2(x,y)))
                                        delta.Add(new Vector2(x, y));
                                }
                            }
                        }
                        int ind = 0;
                        Vector3 nodePos = Grid.ToGrid(new Vector3(targetPos.x, 0, targetPos.z));
                        foreach (PersonScript ps in PersonScript.selectedPeople)
                        {
                            if (ps == null || !ps.gameObject.activeSelf) continue;
                            while (Grid.Occupied((int)(nodePos.x + delta[ind].x), (int)(nodePos.z + delta[ind].y))) ind++;
                            if (target == 1 && Grid.ToGrid(ps.transform.position) != Grid.ToGrid(targetPos) + new Vector3(delta[ind].x, 0, delta[ind].y)) ps.SetTargetPosition(targetPos + new Vector3(delta[ind].x, 0, delta[ind].y) * Grid.SCALE);
                            else if (target == 2/* && Grid.ToGrid(ps.transform.position) != Grid.ToGrid(targetTr.position) + new Vector3(delta[ind].x, 0, delta[ind].y)*/) ps.SetTargetTransform(targetTr, targetTr.position + new Vector3(delta[ind].x, 0, delta[ind].y) * Grid.SCALE);
                            ind++;
                        }
                    }
                }
            }
            /*if (buildingMode == -1)
            {
                mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                collision = Physics.RaycastAll(mouseRay);
                Transform bld = null;
                for (int i = 0; i < collision.Length; i++)
                    if (collision[i].transform.tag == "Building")
                        bld = collision[i].transform;

                if (bld != null && selectedBuilding == null)
                {
                    SetSelectedBuilding(bld);
                }
                else
                {
                    //SetSelectedBuilding(null);
                }
            }*/
            if (Input.GetMouseButtonDown(0) && buildingMode == 0) // BUILDING
            {
                bool canBuild = true;
                int gx = rotation % 2 == 0 ? b.GetGridWidth() : b.GetGridHeight();
                int gy = rotation % 2 == 1 ? b.GetGridWidth() : b.GetGridHeight();
                for (int dx = 0; dx < gx; dx++)
                {
                    for (int dy = 0; dy < gy; dy++)
                    {
                        if (Grid.Occupied(hoverGridX + dx, hoverGridY + dy)) canBuild = false;
                    }
                }

                if (myVillage.GetCoins() < b.GetCost()) canBuild = false;
                //if (myVillage.Get < bs.b.populationUse) canBuild = false;
                /*for (int j = 0; j < GameResources.GetBuildingResourcesCount(); j++)
                    if (myVillage.GetResources(j).GetAmount() < b.GetMaterialCost(j)) canBuild = false;*/

                if (canBuild)
                {
                    GameObject newBuilding = (GameObject)Instantiate(buildingPrefabList[buildingType], hoverBuilding.transform.position, hoverBuilding.transform.rotation, myVillage.transform);
                    GameObject canv = (GameObject)Instantiate(bluePrintCanvas, newBuilding.transform);
                    canv.name = "CanvasBluePrint";
                    BuildingScript bs = (BuildingScript)newBuilding.AddComponent(typeof(BuildingScript));
                    newBuilding.AddComponent(typeof(cakeslice.Outline));
                    Transform t = newBuilding.transform;
                    t.tag = "Building";
                    b.SetPosition(hoverGridX, hoverGridY);
                    bs.SetBuilding(b);
                    for (int dx = 0; dx < gx; dx++)
                    {
                        for (int dy = 0; dy < gy; dy++)
                        {
                            Grid.GetNode(hoverGridX + dx, hoverGridY + dy).nodeObject = t;
                        }
                    }

                    /* Take cost for coins */
                    //myVillage.Purchase(b);
                    myVillage.AddBuilding(bs);


                    if (!Input.GetKey(KeyCode.LeftShift)) VillageUIManager.Instance.ExitBuildingMode();
                }
                
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
}
