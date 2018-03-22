﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameManager : Singleton<GameManager>
{
    private GameSetting mySettings;

    [SerializeField]
    private Village myVillage;

    void Start()
    {
        List<GameResources> myList = new List<GameResources>();
        myList.AddRange(GameResources.GetAvailableResources());
        mySettings = new GameSetting(myList);
    }

    void Update()
    {
    }

    public static GameSetting GetGameSettings()
    {
        return Instance.mySettings;
    }
    public static void AddFeaturedResourceID(int id)
    {
        Instance.mySettings.GetFeaturedResources().Add(GameResources.GetAllResources()[id]);
    }
    public static void RemoveFeaturedResourceID(int id)
    {
        List<GameResources> featured = Instance.mySettings.GetFeaturedResources();
        for (int i = 0; i < featured.Count; i++)
        {
            if (featured[i].GetID() == id)
            {
                featured.RemoveAt(i); 
                break;
            }
        }
    }

    public static Village GetVillage()
    {
        return Instance.myVillage;
    }
}

    //public static GameManager singleton;

    //public const float GRID_SCALE = 1;
    //public const int GRID_WIDTH = 20;
    //public const int GRID_HEIGHT = 20;
    //private Vector3 grid_start;

    //private List<Building> buildings = new List<Building>();

    //[SerializeField]
    //private List<GameObject> buildingPrefabList;

    //[SerializeField]
    //private Transform panelBuilding;
    //private Transform selectedBuilding = null;
    //[SerializeField]
    //private Transform panelBuildMenu;

    //private Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
    //[SerializeField]
    //private Transform groundPlaneTransform;
    //[SerializeField]
    //private Transform hoverBuilding;
    //private int buildingType = -1;
    //private int buildingMode = -1;
    //private bool placing = false;
    //private Vector3 worldPosition;

    //[SerializeField]
    //private Village myVillage;

    //void Start () {
    //    singleton = this;

    //    panelBuilding.gameObject.SetActive(false);
    //    panelBuildMenu.gameObject.SetActive(false);

    //    Color c = hoverBuilding.GetComponent<MeshRenderer>().material.color;
    //    c.a = 0.5f;
    //    hoverBuilding.GetComponent<MeshRenderer>().material.color = c;

    //    groundPlaneTransform.localScale = new Vector3(GRID_WIDTH, 1, GRID_HEIGHT)/10*2;

    //    grid_start = new Vector3((1f -GRID_WIDTH) * GRID_SCALE / 2, GRID_SCALE / 2, (1f -GRID_HEIGHT) * GRID_SCALE / 2);
    //    SetupVillage();
    //}
    //void Update()
    //{
    //    singleton = this;

    //    Ray mouseRay;
    //    RaycastHit[] collision;
    //    float distance;

    //    hoverBuilding.gameObject.SetActive(placing);
    //    if (Input.GetKeyDown(KeyCode.Escape))
    //    {
    //        if (buildingMode == 0 )
    //        {
    //            buildingMode = -1;
    //            placing = false;
    //            panelBuildMenu.gameObject.SetActive(false);
    //        }
    //        else if (selectedBuilding != null)
    //        {
    //            SetSelectedBuilding(null);
    //        }
    //    }

    //    if (selectedBuilding != null)
    //    {
    //        BuildingScript bs = selectedBuilding.GetComponent<BuildingScript>();
    //        Transform bp = panelBuilding.Find(selectedBuilding.GetComponent<BuildingScript>().b.name);
    //        switch (bs.b.type)
    //        {
    //            case 0:
    //                bp.Find("TextEarning").GetComponent<Text>().text = "<color=green>" + ((int)myVillage.taxIncomeTot).ToString()+"</color>";
    //                bp.Find("TextTaxes").GetComponent<Text>().text = "<color=green>" + ((int)myVillage.taxIncomeTot).ToString()+"</color>";
    //                bp.Find("TextNeeds").GetComponent<Text>().text = "<color=green>" + 0 +"</color>";
    //                bp.Find("TextInvestment").GetComponent<Text>().text = "<color=red>" + 0 + "</color>";
    //                bp.Find("TextInvestmentMilitary").GetComponent<Text>().text = "<color=red>" + 0 + "</color>";
    //                bp.Find("TextInvestmentBuilding").GetComponent<Text>().text = "<color=red>" + 0 + "</color>";
    //                bp.Find("TextCurrencyTot").GetComponent<Text>().text = ((int)myVillage.currentCurrency).ToString();
    //                break;
    //            case 1:
    //                bp.Find("TextPopulation").GetComponent<Text>().text = bs.populationCurrent.ToString();
    //                bp.Find("TextPopulationRoom").GetComponent<Text>().text = bs.b.populationRoom.ToString();
    //                break;
    //            case 2:
    //                bp.Find("TextFood").GetComponent<Text>().text = bs.b.foodRationsProduce.ToString();
    //                break;
    //            case 3:
    //                bp.Find("TextWood").GetComponent<Text>().text = bs.b.materialProduce[0].ToString();
    //                break;
    //            case 4:
    //                bp.Find("TextClay").GetComponent<Text>().text = bs.b.materialProduce[1].ToString();
    //                break;
    //        }
    //    }

    //    if (buildingMode == 0)
    //    {
    //        if (placing)
    //        {
    //            mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
    //            if (groundPlane.Raycast(mouseRay, out distance))
    //            {
    //                worldPosition = mouseRay.GetPoint(distance);
    //                /*worldPosition.x = Mathf.RoundToInt(worldPosition.x / GRID_SCALE + 0.5f) - 0.5f;
    //                worldPosition.z = Mathf.RoundToInt(worldPosition.z / GRID_SCALE + 0.5f) - 0.5f;
    //                /*Vector3 pos = collision[0].point;
    //                pos.x -= (pos.x + GRID_SCALE/2f) % GRID_SCALE;
    //                pos.y = 0f;
    //                pos.z -= (pos.z + GRID_SCALE/2f) % GRID_SCALE;

    //                worldPosition.x = Mathf.Clamp(worldPosition.x, -GRID_WIDTH / 2 + 0.5f, GRID_WIDTH / 2 - 0.5f);
    //                worldPosition.z = Mathf.Clamp(worldPosition.z, -GRID_HEIGHT / 2 + 0.5f, GRID_HEIGHT / 2 - 0.5f);*/
    //                Building b = new Building();
    //                b.FromType(buildingType);
    //                worldPosition -= grid_start;
    //                int gridX = Mathf.RoundToInt(worldPosition.x / GRID_SCALE);
    //                int gridY = Mathf.RoundToInt(worldPosition.z / GRID_SCALE);
    //                hoverBuilding.transform.position = grid_start + (new Vector3(gridX, 0f, gridY) + new Vector3((float)b.gridSizeX / 2f, 0, (float)b.gridSizeY / 2f)) * GRID_SCALE + new Vector3(0, hoverBuilding.transform.localScale.y / 2 - 0.5f, 0);//(worldPosition + (new Vector3(0, 0.5f, 0) - new Vector3((float)b.gridSizeX / 2f, 0, (float)b.gridSizeY / 2f))) * GRID_SCALE + new Vector3(0, hoverBuilding.transform.localScale.y / 2 - 0.5f, 0);
    //            }
    //        }
    //        else
    //        {

    //        }
    //    }

    //    if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
    //    {
    //        if (buildingMode == -1)
    //        {
    //            mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
    //            collision = Physics.RaycastAll(mouseRay);
    //            Transform bld = null;
    //            for (int i = 0; i < collision.Length; i++)
    //                if (collision[i].transform.tag == "Building")
    //                    bld = collision[i].transform;

    //            if (bld != null && selectedBuilding == null)
    //            {
    //                SetSelectedBuilding(bld);
    //            }
    //            else
    //            {
    //                //SetSelectedBuilding(null);
    //            }
    //        }
    //        else if (buildingMode == 0 && placing) // BUILDING
    //        {

    //            GameObject newBuilding = (GameObject)Instantiate(buildingPrefabList[buildingType], hoverBuilding.transform.position, Quaternion.identity, myVillage.transform);
    //            Transform t = newBuilding.transform;
    //            BuildingScript bs = newBuilding.GetComponent<BuildingScript>();
    //            bs.b.FromType(buildingType);


    //            bool canBuy = true;

    //            if (myVillage.currentCurrency < bs.b.cost) canBuy = false;
    //            if (myVillage.currentFreePopulation < bs.b.populationUse) canBuy = false;
    //            for (int j = 0; j < myVillage.currentMaterials.Length; j++)
    //                if (myVillage.currentMaterials[j] < bs.b.materialUse[j]) canBuy = false;

    //            if (canBuy)
    //            {
    //                int gridX = Mathf.RoundToInt(worldPosition.x / GRID_SCALE);
    //                int gridY = Mathf.RoundToInt(worldPosition.z / GRID_SCALE);
    //                bs.gridX = gridX;//(int)((t.position.x - grid_start.x) / GRID_SCALE);
    //                bs.gridY = gridY;//(int)((t.position.z - grid_start.z) / GRID_SCALE);
    //                if(myVillage.AddBuilding(bs))
    //                {
    //                    myVillage.currentCurrency -= bs.b.cost;
    //                    for (int j = 0; j < myVillage.currentMaterials.Length; j++)
    //                        myVillage.currentMaterials[j] -= bs.b.materialUse[j];

    //                    int toEmploy = bs.b.populationUse;
    //                    myVillage.EmployPeople(toEmploy);


    //                    if (!Input.GetKey(KeyCode.LeftShift))
    //                    {
    //                        buildingMode = -1;
    //                        placing = false;
    //                    }
    //                }
    //                else
    //                {
    //                    Destroy(newBuilding);
    //                }
    //            }
    //            else
    //            {
    //                Destroy(newBuilding);
    //                buildingMode = -1;
    //                placing = false;
    //            }
    //        }
    //    }
    //}

    //private void SetupVillage()
    //{
    //    myVillage.currentCurrency = 200;
    //    myVillage.currentMaterials = new float[] { 600, 200, 30 };

    //    // Format: 'type*gridX*gridY;' 
    //    string buildingListStr = "0*5*5";// "0*-1*-1;0*" + GRID_WIDTH + "*" + GRID_HEIGHT; /* TODO: Get Info from Database*/
    //    for(int i = 0; i < 2; i++)
    //        buildingListStr += ";" + 1 + "*" + Random.Range(0, (int)GRID_WIDTH) + "*" + Random.Range(0, (int)GRID_HEIGHT);
    //    for (int i = 0; i < 2; i++)
    //        buildingListStr += ";" + 2 + "*" + Random.Range(0, (int)GRID_WIDTH) + "*" + Random.Range(0, (int)GRID_HEIGHT);
    //    for (int i = 0; i < 2; i++)
    //        buildingListStr += ";" + 3 + "*" + Random.Range(0, (int)GRID_WIDTH) + "*" + Random.Range(0, (int)GRID_HEIGHT);
    //    /*for (int i = 0; i < 3; i++)
    //    {
    //        buildingListStr += (i!=0?";":"")+Random.Range(1, 3)+"*" + Random.Range(0, (int)GRID_WIDTH) + "*" + Random.Range(0, (int)GRID_HEIGHT);
    //    }*/
    //    buildings = new List<Building>();
    //    string[] buildingStr = buildingListStr.Split(';');
    //    for (int i = 0; i < buildingStr.Length; i++)
    //    {
    //        GameObject building = (GameObject)Instantiate(buildingPrefabList[int.Parse(buildingStr[i].Split('*')[0])], myVillage.transform);
    //        building.GetComponent<BuildingScript>().Initialize(buildingStr[i]);
    //        BuildingScript b = building.GetComponent<BuildingScript>();
    //        b.populationCurrent = 10;
    //        building.transform.position = grid_start + (new Vector3(b.gridX, 0f, b.gridY) + new Vector3((float)b.b.gridSizeX / 2f, 0, (float)b.b.gridSizeY / 2f)) * GRID_SCALE + new Vector3(0, building.transform.localScale.y / 2 - 0.5f, 0);
    //        //myVillage.AddBuilding(b);
    //        if (!myVillage.AddBuilding(b))
    //        {
    //            Destroy(building);
    //        }
    //        else
    //        {
    //        }
    //    }
    //}
    //public Village GetVillage()
    //{
    //    return myVillage;
    //}

    //public void OnButtonBuild()
    //{
    //    SetSelectedBuilding(null);
    //    panelBuildMenu.gameObject.SetActive(true);
    //    buildingMode = 0;
    //    placing = false;
    //}
    //public void OnButtonMove()
    //{
    //    /* TODO */
    //}
    //public void OnButtonRemove()
    //{
    //    /* TODO */
    //}

    //public GameObject GetBuildingPrefab(int type)
    //{
    //    return buildingPrefabList[type];
    //}

    //public void SetHoverBuildingType(int t)
    //{
    //    panelBuildMenu.gameObject.SetActive(false);
    //    buildingType = t;
    //    Destroy(hoverBuilding.gameObject);
    //    hoverBuilding = ((GameObject)Instantiate(buildingPrefabList[t])).transform;
    //    placing = true;
    //}
    //private void SetSelectedBuilding(Transform newBuilding)
    //{
    //    if (selectedBuilding != null) panelBuilding.Find(selectedBuilding.GetComponent<BuildingScript>().b.name).gameObject.SetActive(false);
    //    selectedBuilding = newBuilding;
    //    panelBuilding.gameObject.SetActive(selectedBuilding != null);
    //    if (selectedBuilding != null)
    //    {
    //        panelBuilding.Find("TextBuildingName").GetComponent<Text>().text =
    //            selectedBuilding.GetComponent<BuildingScript>().b.name;
    //        panelBuilding.Find(selectedBuilding.GetComponent<BuildingScript>().b.name).gameObject.SetActive(true);
    //    }
    //}
//}