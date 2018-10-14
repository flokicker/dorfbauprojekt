using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameManager : Singleton<GameManager>
{
    public static string Username
    {
        get { return gameData.username; }
    }
    public static int CurrentDay
    {
        get { return gameData.currentDay; }
    }
    public static int DayOfYear
    {
        get { return CurrentDay % 365; }
    }
    public static int Year
    {
        get { return CurrentDay / 365; }
    }
    public static List<int> FeaturedResources
    {
        get { return gameData.featuredResources; }
    }

    public static GameData gameData;

    // Our current village
    public static Village village;

    // The transform of the village
    [SerializeField]
    private Transform villageTrsf;
    // reference to the fade manager ingame
    [SerializeField]
    private FadeManager gameFadeManager;

    private static bool debugging;
    public static bool gameOver, noCost;
    
    // Time settings
    private float dayChangeTimeElapsed;
    public static float secondsPerDay = 9.86f, speedFactor = 1f;

    // SaveLoad time settings
    private float saveTime;

    private static bool setupStart, setupFinished;

    public GameObject selectionCirclePrefab;

    private void Awake()
    {
        // if no gamedata present yet, its a new game
        if (gameData == null)
        {
            // start in summer
            gameData = new GameData();
            gameData.username = MainMenuManager.username;
            gameData.currentDay = 65;
            gameData.techTreeEnabled = true;
            gameData.peopleGroups = new List<int>[10];
            for (int i = 0; i < 10; i++)
            {
                gameData.peopleGroups[i] = new List<int>() { i - 1 };
            }
            gameData.featuredResources = new List<int>();

            gameData.openQuests = new List<GameQuest>();
            gameData.achievements = new List<GameAchievement>();
        }
    }

    void Start()
    {
        if (gameData.featuredResources.Count == 0)
        {
            gameData.featuredResources.Add(ResourceData.Id("Holz"));
            gameData.featuredResources.Add(ResourceData.Id("Stein"));
        }

        if (gameData.openQuests.Count == 0)
        {
            foreach(Quest q in Quest.allQuests)
                if(q.starterQuest)
                    gameData.openQuests.Add(new GameQuest(q));
        }
        if(gameData.achievements.Count == 0)
        {
            foreach (Achievement ach in Achievement.allAchievements)
                gameData.achievements.Add(new GameAchievement(ach));
        }

        foreach (Building b in Building.allBuildings)
        {
            if (b.unlockedFromStart)
                Building.Unlock(b.id);
        }

        // unlock fisher and researcher from beginning
        Job.Unlock(Job.Id("Fischer"));
        Job.Unlock(Job.Id("Tüftler"));

        dayChangeTimeElapsed = 0;

        // debugging is turned off by default
        debugging = Application.isEditor;
        noCost = debugging;
        gameOver = false;

        setupStart = false;

        // get reference to village script
        village = villageTrsf.gameObject.AddComponent<Village>();
            
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
            }
            else 
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
        dayChangeTimeElapsed += Time.deltaTime * speedFactor;
        if (dayChangeTimeElapsed >= secondsPerDay)
        {
            NextDay();
            dayChangeTimeElapsed -= secondsPerDay;
        }
    }
    
    public static void UpdateQuestAchievementCollectingResources(GameResources collectedRes)
    {
        foreach(GameAchievement ga in gameData.achievements)
        {
            if (ga.achievement.type == AchievementType.Resource && ga.achievement.resBuildJobId == collectedRes.Id)
                ga.currentAmount += collectedRes.Amount;
        }

        foreach (GameQuest gq in gameData.openQuests)
        {
            //bool finishedBefore = gq.Finished();

            bool exists = false;
            foreach (GameResources questRes in gq.collectedResources)
            {
                if (questRes.Id == collectedRes.Id)
                {
                    exists = true;
                    questRes.Add(collectedRes.Amount);
                    break;
                }
            }

            // if res not yet on collection list, add it by cloning
            if (!exists) gq.collectedResources.Add(new GameResources(collectedRes));
        }
    }
    public static void UpdateAchievementBuilding(Building b)
    {
        foreach (GameAchievement ga in GameManager.gameData.achievements)
            if (ga.achievement.type == AchievementType.Building && ga.achievement.resBuildJobId == b.id)
                ga.currentAmount++;
    }
    public static void UpdateAchievementPerson()
    {
        foreach (GameAchievement ga in GameManager.gameData.achievements)
            if (ga.achievement.type == AchievementType.Population)
                ga.currentAmount++;
    }

    private void NextDay()
    {
        gameData.currentDay++;
        if (CurrentDay % 365 == 0)
        {
            NextYear();
        }

        if (CurrentDay % 8 == 0)
        {
            foreach (PersonScript p in PersonScript.allPeople)
            {
                if(!p.Controllable())
                    p.AgeOneYear();
            }
        }
        if(CurrentDay % 52 == 0)
        {
            foreach (PersonScript p in PersonScript.allPeople)
            {
                if (p.Controllable())
                    p.AgeOneYear();
            }
        }
    }
    private void NextYear()
    {
        ChatManager.Msg("Happy new year! " + (CurrentDay / 365));
        foreach (PersonScript p in PersonScript.allPeople)
        {
            p.AgeOneYear();
        }
    }
    public static void PassYears(int yr)
    {
        for (int i = 0; i < yr; i++)
        {
            gameData.currentDay += 365;
            Instance.NextYear();
        }
    }
    public static void SetDay(int day)
    {
        gameData.currentDay = day;
    }
    public static float GetDayPercentage()
    {
        return Instance.dayChangeTimeElapsed / secondsPerDay;
    }
    private static int[] daysPerMonth = {31,28,31,30,31,30,31,31,30,31,30,31};
    private static string[] months = { "Januar", "Februar", "März", "April", "Mai", "Juni", "Juli", "August", "September", "Oktober", "November", "Dezember"};
    public static int GetMonth()
    {
        int month = 0;
        int days = CurrentDay % 365;
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
        int days = CurrentDay % 365;
        while(days >= daysPerMonth[month])
        {
            days -= daysPerMonth[month];
            month++;
        }
        return days;
    }
    public static string GetDateStr()
    {
        return (GetDayOfMonth()+1) +"."+(GetMonth()+1)+"."+ Year;
    }
    public static void ToggleDebugging()
    {
        debugging = !debugging;
    }
    public static bool IsDebugging()
    {
        return debugging;
    }

    public static List<int> GetPeopleGroup(int num)
    {
        return gameData.peopleGroups[num];
    }
    public static void SetPeopleGroup(int num, List<int> peoples)
    {
        gameData.peopleGroups[num] = peoples;
    }

    // 0=winter, 1=spring, 2=summer, 3=fall
    public static int GetFourSeason()
    {
        int month = GetMonth();
        if(month == 11 || month < 2) return 0;
        if(month < 3) return 1;
        if(month < 10) return 2;
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
            case 1: months = new int[] {2}; break;
            case 2: months = new int[] {3,4,5,6,7,8,9}; break;
            case 3: months = new int[] {10}; break;
        }
        for(int i = 0; i < months.Length; i++)
        {
            totDays += daysPerMonth[months[i]];
            if(month >= months[i])
            {
                if(month == months[i]) currDays += day;
                else currDays += daysPerMonth[months[i]];
            }
        }

        if(month == 0 || month == 1)
        {
            currDays += daysPerMonth[11];
        }
        else if(month == 11)
        {
            currDays = day;
        }

        return (currDays + Instance.dayChangeTimeElapsed/secondsPerDay) / totDays;
    }

    public static void UnlockResource(string resNm)
    {
        UnlockResource(ResourceData.Id(resNm));
    }
    public static void UnlockResource(int resId)
    {
        if (resId != -1 && !ResourceData.IsUnlocked(resId))
        {
            GameResources res = new GameResources(resId);
            ResourceData.Unlock(resId);
            
            if(!gameData.featuredResources.Contains(resId) && res.Type == ResourceType.Building && FeaturedResources.Count < UIManager.FeaturedResCount)
                gameData.featuredResources.Add(resId);
            
            if (IsSetup()) ChatManager.Msg("Neue Ressource entdeckt: "+res.Name, MessageType.News);
            UIManager.Instance.Blink("PanelTopResources", true);
        }
    }
    public static void UnlockBuilding(Building b)
    {
        if (b.id != -1 && !Building.IsUnlocked(b.id))
        {
            Building.Unlock(b.id);
            ChatManager.Msg("Neues Gebäude freigeschalten: " + b.name);
            UIManager.Instance.Blink("ButtonBuild", true);
        }
    }
    public static void UnlockJob(Job j)
    {
        if (j.id != -1 && !Job.IsUnlocked(j.id))
        {
            Job.Unlock(j.id);
            ChatManager.Msg("Neuen Beruf freigeschalten: " + j.name);
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

        loadedObjects += BuildingScript.allBuildingScripts.Count;
        loadedObjects += Nature.nature.Count;
        loadedObjects += PersonScript.allPeople.Count;
        loadedObjects += ItemScript.allItemScripts.Count;
        loadedObjects += AnimalScript.allAnimals.Count;

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
