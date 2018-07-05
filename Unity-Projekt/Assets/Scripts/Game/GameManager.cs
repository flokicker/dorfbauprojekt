using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameManager : Singleton<GameManager>
{
    public static string username;

    private GameSetting mySettings;

    // Our current village
    public static Village village;

    // The transform of the village
    [SerializeField]
    private Transform villageTrsf;
    // reference to the fade manager ingame
    [SerializeField]
    private FadeManager gameFadeManager;

    public static bool debugging;
    public static bool gameOver;
    
    // Time settings
    private int currentDay;
    private float dayChangeTimeElapsed;
    private float secondsPerDay = 2f;

    // SaveLoad time settings
    private float saveTime;

    private static bool setupStart, setupFinished;

    void Start()
    {
        // start in summer
        currentDay = 130;
        dayChangeTimeElapsed = 0;

        // debugging is turned off by default
        debugging = false;
        gameOver = false;

        setupStart = false;

        // get reference to village script
        village = villageTrsf.gameObject.AddComponent<Village>();

        // get all resources
        List<GameResources> myList = new List<GameResources>();
        myList.AddRange(GameResources.GetAvailableResources());
        mySettings = new GameSetting(myList);

        for (int i = 0; i < Building.COUNT; i++)
            //if(i != Building.SACRIFICIALALTAR)
                Building.Unlock(i);
            
        gameFadeManager.Fade(false, 1f, 0.5f);
    }

    void Update()
    {
        if(!setupStart)
        {
            setupStart = true;
            if(SaveLoadManager.SavedGame(SaveLoadManager.saveState))
            {
                SaveLoadManager.LoadGame();
                //ChatManager.Msg("Spielstand geladen");
            } else 
            {
                village.SetupNewVillage();
                SaveLoadManager.SaveGame();
                //ChatManager.Msg("Neuer Spielstand erstellt");
            }

        }
        else
        {
            if (PersonScript.allPeople.Count == 0 && !GameManager.gameOver) 
            {
                ChatManager.Msg("Game Over!");
                GameManager.gameOver = true;
            }
        }

        if(Input.GetKeyDown(KeyCode.O)) {
            debugging = !debugging;
            ChatManager.Msg("debuggin "+ (debugging ? "enabled" : "disabled"));
        }

        // SaveLoad Timer update, auto save game all 20sec
        saveTime += Time.deltaTime;
        if(saveTime >= 20)
        {
            saveTime = 0;
            SaveLoadManager.SaveGame();
		    //ChatManager.Msg("Spielstand gespeichert");
        }
        
        UpdateTime();
    }

    void OnApplicationQuit()
    {
        SaveLoadManager.SaveGame();
    }

    // update daytime
    private void UpdateTime()
    {
        dayChangeTimeElapsed += Time.deltaTime;
        if (dayChangeTimeElapsed >= secondsPerDay)
        {
            NextDay();
            dayChangeTimeElapsed -= secondsPerDay;
        }
    }

    private void NextDay()
    {
        currentDay++;
        if (currentDay % 365 == 0)
        {
            NextYear();
        }
    }
    private void NextYear()
    {
        ChatManager.Msg("Happy new year! " + (currentDay / 365));
        foreach (PersonScript p in PersonScript.allPeople)
        {
            p.AgeOneYear();
        }
    }
    public static void SetDay(int day)
    {
        Instance.currentDay = day;
    }
    public static int GetTotDay()
    {
        return Instance.currentDay;
    }
    public static int GetDay()
    {
        return Instance.currentDay % 365;
    }
    public static int GetYear()
    {
        return Instance.currentDay / 365;
    }
    private static int[] daysPerMonth = {31,28,31,30,31,30,31,31,30,31,30,31};
    private static string[] months = { "Januar", "Februar", "März", "April", "Mai", "Juni", "Juli", "August", "September", "Oktober", "November", "Dezember"};
    public static int GetMonth()
    {
        int month = 0;
        int days = Instance.currentDay % 365;
        while(days >= daysPerMonth[month])
        {
            days -= daysPerMonth[month];
            month++;
        }
        return month;
    }
    public static string GetMonthStr()
    {
        return months[GetMonth()];
    }
    public static int GetDayOfMonth()
    {
        int month = 0;
        int days = Instance.currentDay % 365;
        while(days >= daysPerMonth[month])
        {
            days -= daysPerMonth[month];
            month++;
        }
        return days;
    }
    public static string GetDateStr()
    {
        return (GetDayOfMonth()+1) +"."+(GetMonth()+1)+"."+GetYear();
    }

    // 0=winter, 1=spring, 2=summer, 3=fall
    public static int GetFourSeason()
    {
        int month = GetMonth();
        int dayOfMonth = GetDayOfMonth();
        if(month == 11 || month < 2) return 0;
        if(month < 5) return 1;
        if(month < 8) return 2;
        if(month < 11) return 3;

        return -1;
    }
    // 0=Winter 1=Sommer
    public static int GetTwoSeason()
    {
        int month = GetMonth();
        if(month < 2 || month >= 10) return 0;
        return 1;
    }
    public static string GetTwoSeasonStr()
    {
        switch(GetTwoSeason())
        {
            case 0: return "Winterzeit";
            case 1: return "Sommerzeit";
        }
        return "undefined season";
    }
    public static string GetFourSeasonStr()
    {
        switch(GetFourSeason())
        {
            case 0: return "Winter";
            case 1: return "Frühling";
            case 2: return "Sommer";
            case 3: return "Herbst";
        }
        return "undefined season";
    }
    public static float GetFourSeasonPercentage()
    {
        int season = GetFourSeason();
        int month = GetMonth();
        int day = GetDayOfMonth();
        int totDays = 0, currDays = 0;
        int[] months = new int[0];
        switch(season)
        {
            case 0: months = new int[] {11,0,1}; break;
            case 1: months = new int[] {2,3,4}; break;
            case 2: months = new int[] {5,6,7}; break;
            case 3: months = new int[] {8,9,10}; break;
        }
        for(int i = 0; i < months.Length; i++)
        {
            totDays += daysPerMonth[months[i]];
            if(month >= months[i] && (month - months[i] < 3)|| months[i] == 11)
            {
                if(month == months[i]) currDays += day;
                else currDays += daysPerMonth[months[i]];
            }
        }
        return (float)currDays / totDays;
    }

    // Game settings with featured resources
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
            if (featured[i].id == id)
            {
                featured.RemoveAt(i); 
                break;
            }
        }
    }
    public static void Error(string error)
    {
        ChatManager.Msg("ERROR: "+error);
    }

    public static void UnlockResource(int resId)
    {
        if (resId != -1 && !GameResources.IsUnlocked(resId))
        {
            GameResources res = new GameResources(resId);
            GameResources.Unlock(resId);
            if(resId < GameResources.COUNT_FOOD+GameResources.COUNT_BUILDING_MATERIAL) GameManager.GetGameSettings().AddFeaturedResource(GameResources.GetAllResources()[resId]);
            if(IsSetup()) ChatManager.Msg("Neue Ressource entdeckt: "+res.GetName());
            UIManager.Instance.Blink("PanelTopResources", true);
        }
    }

    public static bool InRange(Vector3 pos1, Vector3 pos2, float range)
    {
        return Vector3.Distance(pos1,pos2) <= range*Grid.SCALE;
        //return (int)(Mathf.Abs(pos1.x-pos2.x)+0.5f) <= (int)(range*Grid.SCALE) && (int)(Mathf.Abs(pos1.z-pos2.z)+0.5f) <= (int)(range*Grid.SCALE);
    }

    public static float LoadPercentage()
    {
        int totLoadObjects = 0;
        int loadedObjects = 0;

        totLoadObjects = SaveLoadManager.myGameState.CountTotalGameObjects();

        loadedObjects += Building.allBuildings.Count;
        loadedObjects += Nature.flora.Count;
        loadedObjects += PersonScript.allPeople.Count;
        loadedObjects += Item.allItems.Count;
        loadedObjects += Animal.allAnimals.Count;

        //Debug.Log(loadedObjects);

        // Debug.Log(loadedObjects + "/"+totLoadObjects);

        if(totLoadObjects == 0) return 1f;

        if(loadedObjects >= totLoadObjects) setupFinished = true;
        if(setupFinished) return 1f;
        return (float)loadedObjects / (float)totLoadObjects;
    }
    public static bool IsSetup()
    {
        return LoadPercentage() >= 1f-float.Epsilon;
    }
    public static void FadeOut()
    {
        Instance.gameFadeManager.Fade(true, 0.5f, 1f);
    }
    public static bool HasFaded()
    {
        return !Instance.gameFadeManager.isInTransition;
    }
    public static void CancelFade()
    {
        Instance.gameFadeManager.Cancel();
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
    //public Village village
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
