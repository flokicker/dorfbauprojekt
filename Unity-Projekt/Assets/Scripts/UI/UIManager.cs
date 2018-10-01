using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Net.Mail;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using TMPro;

public class UIManager : Singleton<UIManager>
{
    private Village myVillage;

    [SerializeField]
    private Transform canvas;

    private Text topPopulationTot, topCoinsText;
    private Image topCoinsImage;
    [SerializeField]
    private Sprite[] coinSprites = new Sprite[3];
    private Transform topResourcesParent;
    [SerializeField]
    private GameObject topResourcePrefab;
    [SerializeField]
    private GameObject buildingBuildImagePrefab, buildResourceImagePrefab, buildResourceTextPrefab;

    // General panels
    private Transform topBar, topFaith, topTechTree, populationTabs, panelCoins, panelResources, panelGrowth, panelBuild, panelBuildingInfo, panelTaskResource,
        panelObjectInfo, panelPeopleInfo, panelSinglePersonInfo, panelPeopleInfo6, panelPeopleInfo7, panelObjectInfoSmall, panelTutorial, 
        panelSettings, panelDebug, panelFeedback, panelMap, panelFaith, panelRecruiting, panelTechTree, panelAchievements, panelQuests;

    private Text jobOverviewTotalText, jobOverviewBusyText, jobOverviewFreeText;
    private Transform jobOverviewContent, populationListContent;
    [SerializeField]
    private GameObject jobItemPrefab, populationListItemPrefab, resourcePrefab;
    [SerializeField]
    private List<Sprite> jobIcons;

    /*private Text buildingInfoName, buildingInfoDesc, buildingInfoStage;
    private Transform buildingInfoContent;*/

    private RectTransform gfactorSlider;
    private Text gfactorTot, gfactorRoomText, gfactorFoodText, gfactorHealthText, gfactorFertilityText, gfactorLuxuryText;

    private Text yearText;

    private Text buildName, buildDescription, buildSize, buildCost, buildWorkspace, buildPopulationRoom;
    private Button buildButton;
    private Transform buildImageListParent, buildResourceParent;

    // Feedback
    private Transform feedBackContent, feedBackList, feedBackNew;
    [SerializeField]
    private GameObject feedBackEntryPrefab;
    private InputField feedBackInputTitle, feedBackInputText;
    public Feedback myFeedback = new Feedback();

    // Task res
    [SerializeField]
    private GameObject taskResPrefab;
    private Transform taskResInventory, taskResStorage, taskResInvAm, taskResInvBut, taskResStorAm, taskResStorBut;
    private Image taskResInvImage, taskResStorImage;
    private Slider taskResInvSlider, taskResStorSlider;
    private InputField taskResInvInput, taskResStorInput;
    private Button taskResInvButton, taskResInvMaxButton, taskResStorButton, taskResStorMaxButton;
    public Queue<PersonScript> taskResRequest = new Queue<PersonScript>();

    private int taskResInvSelected, taskResStorSelected;
    private int taskResInvMax, taskResStorMax;

    // Object+Building info
    [SerializeField]
    private List<Sprite> treeIcons, rockIcons;
    private Text objectInfoTitle, objectInfoText, objectInfoSmallTitle, buildingInfoTitle, buildingInfoText;
    private Transform buildingInfoStorage, buildingInfoUpgradeCost, buildingInfoLifebar, buildingButtonJobs, buildingButtonFields;
    private Image objectInfoImage, buildingInfoLifebarImage;
    private Button buildingMoveBut, buildingRemoveBut, buildingUpgradeShelter;
    private TextMeshProUGUI buildingStorageText, buildingUpgradeCostText;

    // Person info
    [SerializeField]
    private Sprite[] personIcons;
    private Text personInfoName, personInfo, personInventoryMatText, personInventoryFoodText, peopleInfo7;
    private Image personImage, personInfoHealthbar, personInfoFoodbar, personInventoryMatImage, personInventoryFoodImage;
    private Button personBuildBut, personJobBut;

    [SerializeField]
    private GameObject personInfoPrefab;

    // Faith
    private Image topFaithImage;

    // TechTree
    private Transform techTreeAge1;

    // Recruiting
    private Text recruitingTimeText, recruitingUnitText;
    private Image recruitingTimeImage, recruitingCurrent;
    private Transform recruitingUnitParent;
    [SerializeField]
    private GameObject recruitingUnitPrefab;

    // Achievements
    private Transform achievementContent;

    // Quests
    private Transform questsContent;
    [SerializeField]
    private GameObject questTextPrefab;

    private Toggle settingsInvertMousewheel;

    private Text debugText;

    private int inMenu = 0;
    public bool objectInfoShown, objectInfoShownSmall, personInfoShown;
    private Transform selectedObject;

	void Start () 
    {
        SetupReferences();

        //inMenu = 8;
        //panelTutorial.gameObject.SetActive(true);

        OnPopulationTab(1);
	}

    private void SetupReferences()
    {
        // setup vilalge reference
        myVillage = GameManager.village;

        // Top bar
        topBar = canvas.Find("TopBar");
        topFaith = topBar.Find("PanelTopFaith");
        topTechTree = topBar.Find("PanelTopTechTree");
        topPopulationTot = topBar.Find("PanelTopPopulation").Find("Text").GetComponent<Text>();
        topCoinsText = topBar.Find("PanelTopCoins").Find("Coins").Find("Text").GetComponent<Text>();
        topCoinsImage = topBar.Find("PanelTopCoins").Find("Coins").Find("Image").GetComponent<Image>();
        topResourcesParent = topBar.Find("PanelTopResources");
        topResourcesParent.gameObject.SetActive(false);
        yearText = topBar.Find("PanelTopYear").Find("Text").GetComponent<Text>();

        populationTabs = canvas.Find("PopulationTabs");
        Transform populationOverview = populationTabs.Find("JobOverviewTab").Find("PanelTab").Find("Content").Find("PopulationOverview");
        jobOverviewTotalText = populationOverview.Find("Total").GetComponent<Text>();
        jobOverviewBusyText = populationOverview.Find("Busy").GetComponent<Text>();
        jobOverviewFreeText = populationOverview.Find("Free").GetComponent<Text>();
        jobOverviewContent = populationTabs.Find("JobOverviewTab").Find("PanelTab").Find("Content").Find("Scroll View").Find("Viewport").Find("JobOverviewContent");
        populationListContent = populationTabs.Find("ListOverviewTab").Find("PanelTab").Find("Content").Find("Scroll View").Find("Viewport").Find("PopulationListContent");
        OnPopulationTab(0);

        /*for (int i = 0; i < Job.COUNT; i++)
        {
            Job.Unlock(i);
        }*/

        foreach(int jid in Job.unlockedJobs)
        {
            AddJob(jid);
        }

        panelCoins = canvas.Find("PanelCoins");

        panelResources = canvas.Find("PanelResources");

        panelGrowth = canvas.Find("PanelGrowth");
        Transform growthTextParent = panelGrowth.Find("Content").Find("Image");
        gfactorSlider = panelGrowth.Find("Content").Find("ImageSlider").Find("Slider").GetComponent<RectTransform>();
        gfactorTot = growthTextParent.Find("Text").GetComponent<Text>();
        gfactorRoomText = growthTextParent.Find("TextRoom").GetComponent<Text>();
        gfactorFoodText = growthTextParent.Find("TextFood").GetComponent<Text>();
        gfactorHealthText = growthTextParent.Find("TextHealth").GetComponent<Text>();
        gfactorFertilityText = growthTextParent.Find("TextFertility").GetComponent<Text>();
        gfactorLuxuryText = growthTextParent.Find("TextLuxury").GetComponent<Text>();

        panelBuild = canvas.Find("PanelBuild");
        buildImageListParent = panelBuild.Find("Content").Find("PreviewScroll").Find("Viewport").Find("BuildingPreview");
        Transform buildInfo = panelBuild.Find("Content").Find("BuildingInfo");
        buildName = buildInfo.Find("TextName").Find("Name").GetComponent<Text>();
        buildDescription = buildInfo.Find("TextDescription").Find("Description").GetComponent<Text>();
        buildSize = buildInfo.Find("TextSize").Find("Size").GetComponent<Text>();
        buildCost = buildInfo.Find("TextCost").Find("Coins").Find("Text").GetComponent<Text>();
        buildWorkspace = buildInfo.Find("TextWorkspace").Find("Workspace").GetComponent<Text>();
        buildPopulationRoom = buildInfo.Find("TextPopulationRoom").Find("PopulationRoom").GetComponent<Text>();
        buildResourceParent = buildInfo.Find("TextMaterial").Find("Parent");

        // Setup build menu with all buildings
        foreach(Building b in Building.allBuildings)
        {
            if (b.name == "Höhle") continue;
            if (!Building.IsUnlocked(b.id)) continue;

            GameObject obj = (GameObject)Instantiate(buildingBuildImagePrefab, buildImageListParent);
            obj.GetComponent<Image>().sprite = b.icon;

            int bid = b.id;
            obj.GetComponent<Button>().onClick.AddListener(() => OnSelectBuilding(bid));
        }
        OnSelectBuilding(1);

        // Taskres
        panelTaskResource = canvas.Find("PanelTaskResource");
        Transform resourcesParent = panelTaskResource.Find("Content").Find("Resources");
        taskResInventory = resourcesParent.Find("Inventory").Find("InventoryRes");
        taskResStorage = resourcesParent.Find("Storage").Find("StorageRes");
        
        for(int i = 0; i < taskResInventory.childCount; i++)
        {
            int j = i;
            Transform resImg = taskResInventory.GetChild(i).Find("Image");
            resImg.GetComponent<Button>().onClick.AddListener(() => OnTaskResInvSelect(j));
        }
        taskResInvAm = resourcesParent.Find("Inventory").Find("Amount");
        taskResInvImage = taskResInvAm.Find("Image").GetComponent<Image>();
        taskResInvSlider = taskResInvAm.Find("Slider").GetComponent<Slider>();
        taskResInvInput = taskResInvAm.Find("InputField").GetComponent<InputField>();
        taskResInvBut = resourcesParent.Find("Inventory").Find("Buttons");
        taskResInvMaxButton = taskResInvBut.Find("ButtonInvMax").GetComponent<Button>();
        taskResInvButton = taskResInvBut.Find("ButtonInv").GetComponent<Button>();
        
        taskResStorAm = resourcesParent.Find("Storage").Find("Amount");
        taskResStorImage = taskResStorAm.Find("Image").GetComponent<Image>();
        taskResStorSlider = taskResStorAm.Find("Slider").GetComponent<Slider>();
        taskResStorInput = taskResStorAm.Find("InputField").GetComponent<InputField>();
        taskResStorBut = resourcesParent.Find("Storage").Find("Buttons");
        taskResStorMaxButton = taskResStorBut.Find("ButtonStorMax").GetComponent<Button>();
        taskResStorButton = taskResStorBut.Find("ButtonStor").GetComponent<Button>();

        buildButton = panelBuild.Find("Content").Find("ButtonBuild").GetComponent<Button>();
        buildButton.onClick.AddListener(OnPlaceBuildingButtonClick);

        // Object Info
        panelObjectInfo = canvas.Find("PanelObjectInfo");
        panelObjectInfoSmall = canvas.Find("PanelObjectInfoSmall");
        objectInfoSmallTitle = panelObjectInfoSmall.Find("Title").GetComponent<Text>();
        objectInfoSmallTitle.text = "Objekt";
        objectInfoTitle = panelObjectInfo.Find("Title").GetComponent<Text>();
        objectInfoText = panelObjectInfo.Find("Text").GetComponent<Text>();
        objectInfoImage = panelObjectInfo.Find("Image").GetComponent<Image>();

        // Building Info
        panelBuildingInfo = canvas.Find("PanelBuildingInfo");
        buildingInfoTitle = panelBuildingInfo.Find("Title").GetComponent<Text>();
        buildingInfoText = panelBuildingInfo.Find("Text").GetComponent<Text>();
        buildingInfoStorage = panelBuildingInfo.Find("StorageRes");
        buildingInfoUpgradeCost = panelBuildingInfo.Find("UpgradeCostRes");
        buildingInfoLifebar = panelBuildingInfo.Find("Lifebar");
        buildingInfoLifebarImage = buildingInfoLifebar.Find("Front").GetComponent<Image>();
        buildingRemoveBut = panelBuildingInfo.Find("Buttons").Find("ButtonRemove").GetComponent<Button>();
        buildingRemoveBut.onClick.AddListener(() => OnBuildingRemove());
        buildingMoveBut = panelBuildingInfo.Find("Buttons").Find("ButtonMove").GetComponent<Button>();
        buildingMoveBut.onClick.AddListener(() => OnBuildingMove());
        buildingUpgradeShelter = panelBuildingInfo.Find("UpgradeButtonShelter").GetComponent<Button>();
        buildingButtonJobs = panelBuildingInfo.Find("ButtonJobs");
        buildingButtonFields = panelBuildingInfo.Find("ButtonFields");
        buildingButtonJobs.Find("Farmer").GetComponent<Button>().onClick.AddListener(() => OnSelectBuildingJob(Job.Get("Bauer")));
        buildingButtonJobs.Find("Hunter").GetComponent<Button>().onClick.AddListener(() => OnSelectBuildingJob(Job.Get("Jäger")));
        buildingButtonJobs.Find("Fisher").GetComponent<Button>().onClick.AddListener(() => OnSelectBuildingJob(Job.Get("Fischer")));

        buildingButtonFields.Find("Farmer").GetComponent<Button>().onClick.AddListener(() => OnPlaceField(Building.Get("Kornfeld")));
        buildingButtonFields.Find("Fisher").GetComponent<Button>().onClick.AddListener(() => OnPlaceField(Building.Get("Fischerbereich")));
        buildingStorageText = panelBuildingInfo.Find("StorageText").GetComponent<TextMeshProUGUI>();
        buildingUpgradeCostText = panelBuildingInfo.Find("UpgradeCostText").GetComponent<TextMeshProUGUI>();

        /*panelBuildingInfo = canvas.Find("PanelBuilding");
        buildingInfoName = panelBuildingInfo.Find("Title").GetComponent<Text>();
        buildingInfoDesc = panelBuildingInfo.Find("Current").Find("TextDesc").GetComponent<Text>();
        buildingInfoStage = panelBuildingInfo.Find("Current").Find("TextStage").GetComponent<Text>();
        buildingInfoContent = panelBuildingInfo.Find("Content");*/

        // People Info
        panelPeopleInfo = canvas.Find("PanelPeopleInfo");
        panelSinglePersonInfo = panelPeopleInfo.Find("PanelSinglePerson");
        Transform left = panelSinglePersonInfo.Find("Left");
        Transform right = panelSinglePersonInfo.Find("Right");
        personInfoName = right.Find("TextName").GetComponent<Text>();
        personImage = right.Find("Image").GetComponent<Image>();
        personInfoHealthbar = right.Find("Lifebar").Find("Front").GetComponent<Image>();
        personInfoFoodbar = right.Find("Foodbar").Find("Front").GetComponent<Image>();

        personInfo = left.Find("TextInfo").GetComponent<Text>();
        personInventoryMatText = left.Find("Inventory").Find("InvMat").Find("Text").GetComponent<Text>();
        personInventoryMatImage = left.Find("Inventory").Find("InvMat").Find("Image").GetComponent<Image>();
        personInventoryFoodText = left.Find("Inventory").Find("InvFood").Find("Text").GetComponent<Text>();
        personInventoryFoodImage = left.Find("Inventory").Find("InvFood").Find("Image").GetComponent<Image>();
        left.Find("Inventory").Find("InvMat").GetComponent<Button>().onClick.AddListener(() => OnDropItem(0));
        left.Find("Inventory").Find("InvFood").GetComponent<Button>().onClick.AddListener(() => OnDropItem(1));
        personBuildBut = left.Find("ButtonBuild").GetComponent<Button>();
        personBuildBut.onClick.AddListener(() => ShowMenu(7));
        personJobBut = left.Find("ButtonJob").GetComponent<Button>();
        personJobBut.onClick.AddListener(() => OnPersonJob());

        panelPeopleInfo6 = panelPeopleInfo.Find("PanelPeople6");
        panelPeopleInfo7 = panelPeopleInfo.Find("PanelPeople7");
        peopleInfo7 = panelPeopleInfo7.Find("TextName").GetComponent<Text>();

        // Feedback
        panelFeedback = canvas.Find("PanelFeedback");
        feedBackList = panelFeedback.Find("ContentList");
        feedBackNew = panelFeedback.Find("ContentNew");
        feedBackNew.gameObject.SetActive(false);
        feedBackList.gameObject.SetActive(true);
        feedBackContent = feedBackList.Find("ScrollView").Find("Viewport").Find("Content");
        feedBackInputTitle = feedBackNew.Find("Title").GetComponentInChildren<InputField>();
        feedBackInputText = feedBackNew.Find("Text").GetComponentInChildren<InputField>();

        // Faith
        panelFaith = canvas.Find("PanelFaith");
        topFaithImage = topFaith.Find("Image/Image").GetComponent<Image>();

        // Recruiting
        panelRecruiting = canvas.Find("PanelRecruiting");
        recruitingTimeText = panelRecruiting.Find("Recruiting/Time/TimeText").GetComponent<Text>();
        recruitingTimeImage = panelRecruiting.Find("Recruiting/Time/TimeImageParent/TimeImage").GetComponent<Image>();
        recruitingUnitParent = panelRecruiting.Find("ScrollView/Viewport/Content");

        // TechTree
        panelTechTree = canvas.Find("PanelTechTree");
        techTreeAge1 = panelTechTree.Find("Content/Scroll View/Viewport/Content/Age1/Content/Panel");
        for(int i = 0; i < techTreeAge1.childCount; i++)
        {
            Button b = techTreeAge1.GetChild(i).GetComponent<Button>();
            if (b == null) continue;
            int c = i;
            b.onClick.AddListener(() =>
            {
                /* TODO research cost */
                myVillage.techTree.Research(c);
            });
        }

        // Achievements
        panelAchievements = canvas.Find("PanelAchievements");
        achievementContent = panelAchievements.Find("Content");

        // Quests
        panelQuests = canvas.Find("PanelQuests");
        questsContent = panelQuests.Find("Content");

        // Minimap
        panelMap = canvas.Find("PanelMap");

        // Tutorial
        panelTutorial = canvas.Find("PanelHelp");
        
        // Debug
        panelDebug = canvas.Find("PanelDebug");
        debugText = panelDebug.Find("Content").Find("Text").GetComponent<Text>();

        // Settings
        panelSettings = canvas.Find("PanelSettings");
        settingsInvertMousewheel = panelSettings.Find("Content").Find("ToggleMousewheel").GetComponent<Toggle>();
        settingsInvertMousewheel.isOn = PlayerPrefs.GetInt("InvertedMousewheel") == 1;
        settingsInvertMousewheel.onValueChanged.AddListener(OnToggleInvertedMousewheel);
    }

	void Update ()
    {
        // setup vilalge reference
        myVillage = GameManager.village;

        if (PersonScript.selectedPeople.Count > 0 && !personInfoShown)
        {
            ShowPersonInfo(true);
        }
        else if(personInfoShown && PersonScript.selectedPeople.Count == 0)
        {
            ShowPersonInfo(false);
        }
        /*if (!InputManager.InputUI())
        {
            objectInfoShown = false;
            //panelBuildingInfo.gameObject.SetActive(false);
            panelBuildingInfo.GetComponent<Animator>().SetBool("show",false);
            panelObjectInfo.GetComponent<Animator>().SetBool("show",false);
            panelObjectInfoSmall.GetComponent<Animator>().SetBool("show",false);
            //panelObjectInfo.gameObject.SetActive(false);
            //panelObjectInfoSmall.gameObject.SetActive(false);
            personInfoShown = false;
            panelPeopleInfo.gameObject.SetActive(false);
        }*/
        
        // Close any panel by clicking outside of UI
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject() && !BuildManager.placing)
        {
            Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(mouseRay, out hit, 1000))
            {
                string tag = hit.transform.gameObject.tag;
                if(tag != "Building")
                    ExitMenu();
                if (tag == "Terrain")
                {
                    if(objectInfoShown)
                    {
                        //panelObjectInfo.gameObject.SetActive(false);
                        //panelBuildingInfo.gameObject.SetActive(false);
                        panelBuildingInfo.GetComponent<Animator>().SetBool("show",false);
                        panelObjectInfo.GetComponent<Animator>().SetBool("show",false);
                        objectInfoShown = false;
                    }
                    if(objectInfoShownSmall)
                    {
                        //panelObjectInfoSmall.gameObject.SetActive(false);
                        panelObjectInfoSmall.GetComponent<Animator>().SetBool("show",false);
                        objectInfoShownSmall = false;
                    }
                }
            }
        }
        
        if(inMenu != 11) taskResRequest.Clear();

        populationTabs.gameObject.SetActive(inMenu == 1);
        panelCoins.gameObject.SetActive(inMenu == 2);
        panelResources.gameObject.SetActive(inMenu == 3);
        panelGrowth.gameObject.SetActive(inMenu == 4);

        //panelTime.gameObject.SetActive(inMenu == 5);
        panelSettings.gameObject.SetActive(inMenu == 6);

        panelBuild.gameObject.SetActive(inMenu == 7);
        //panelBuildingInfo.gameObject.SetActive(inMenu == 8);

        panelTutorial.gameObject.SetActive(inMenu == 9);
        panelDebug.gameObject.SetActive(inMenu == 10);

        panelTaskResource.gameObject.SetActive(inMenu == 11);

        panelFeedback.gameObject.SetActive(inMenu == 12);

        panelMap.gameObject.SetActive(inMenu == 13);

        panelFaith.gameObject.SetActive(inMenu == 14);

        panelRecruiting.gameObject.SetActive(inMenu == 15);

        panelTechTree.gameObject.SetActive(inMenu == 16);

        panelAchievements.gameObject.SetActive(inMenu == 17);
        panelQuests.gameObject.SetActive(inMenu == 18);

        UpdateTopPanels();
        UpdateJobOverview();
        UpdatePopulationList();
        UpdateResourcesPanel();
        UpdateGrowthPanel();
        UpdateBuildPanel();
        UpdateTaskResPanel();
        UpdateObjectInfoPanel();
        UpdateBuildingInfoPanel();
        UpdatePersonPanel();
        UpdateSettingsPanel();
        UpdateFeedbackPanel();
        UpdateDebugPanel();
        UpdateFaithPanel();
        UpdateRecruitingPanel();
        UpdateTechTree();
        UpdateAchievements();
        UpdateQuests();
    }

    public void ExitMenu()
    {
        inMenu = 0;
    }

    public bool HideObjectInfo()
    {
        if (!objectInfoShown) return false;

        //panelObjectInfo.gameObject.SetActive(false);
        //panelBuildingInfo.gameObject.SetActive(false);
        panelBuildingInfo.GetComponent<Animator>().SetBool("show", false);
        panelObjectInfo.GetComponent<Animator>().SetBool("show", false);
        objectInfoShown = false;

        return true;

    }
    public bool HidePersonInfo()
    {
        if (PersonScript.selectedPeople.Count == 0) return false;

        PersonScript.DeselectAll();

        return true;
    }

    private void UpdateTopPanels()
    {
        topFaithImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, topFaithImage.transform.parent.GetComponent<RectTransform>().rect.width * Mathf.Abs(myVillage.GetFaithPoints()/100f));
        topFaithImage.color = (myVillage.GetFaithPoints() >= 0 ? Color.green : Color.red) * 0.8f;
        topFaithImage.transform.parent.Find("Back/Text").GetComponent<Text>().text = ((int)myVillage.GetFaithPoints()).ToString();

        topPopulationTot.text = "Bewohner: " + PersonScript.allPeople.Count.ToString();
        topCoinsText.text = myVillage.GetCoinString();
        topCoinsImage.sprite = coinSprites[myVillage.GetCoinUnit()];

        List<int> list = GameManager.FeaturedResources;
        if (topResourcesParent.childCount != list.Count)
        {
            int chc = topResourcesParent.childCount;
            for (int i = 0; i < chc; i++)
                Destroy(topResourcesParent.GetChild(i).gameObject);
            for (int i = 0; i < list.Count; i++)
            {
                Instantiate(topResourcePrefab, topResourcesParent);
            }
        }

        int childId = topResourcesParent.childCount - list.Count;
        List<GameResources> totResources = myVillage.GetTotalResources(list);
        for(int i = 0; i < list.Count; i++)
        {
            ResourceData res = ResourceData.Get(list[i]);
            topResourcesParent.GetChild(childId).Find("Image").GetComponent<Image>().sprite = res.icon;
            topResourcesParent.GetChild(childId).Find("Text").GetComponent<Text>().text = totResources[i].Amount.ToString();
            childId++;
        }

        topResourcesParent.gameObject.SetActive(Building.IsUnlocked(1));

        yearText.text = GameManager.GetTwoSeasonStr() +"\nJahr "+GameManager.Year;
    }
    private void UpdateFaithPanel()
    {
        /* TODO */
    }
    private void UpdateJobOverview()
    {
        int totalPeople = PersonScript.allPeople.Count;
        int employedPeople = myVillage.EmployedPeopleCount();
        int[] jobemployedPeople = myVillage.JobEmployedCount();

        jobOverviewTotalText.text = "Bewohner insgesamt: " + totalPeople + " (100%)";
        int percBusy = 0;
        if (totalPeople > 0) percBusy = (100 * employedPeople / totalPeople);
        jobOverviewBusyText.text = "Berufstätige Bewohner: " + employedPeople + " (" + percBusy + "%)";
        jobOverviewFreeText.text = "Freie Bewohner: " + (totalPeople - employedPeople) + " (" + (100 - percBusy) + "%)";
        
        int[] maxPeople = myVillage.MaxPeopleJob();
        if (Job.unlockedJobs.Count != jobOverviewContent.childCount)
        {
            for (int i = 0; i < jobOverviewContent.childCount; i++)
                Destroy(jobOverviewContent.GetChild(i).gameObject);

            foreach(int jid in Job.unlockedJobs)
            { 
                AddJob(jid);
            }
        }
        else
        {
            int childIndex = 0;
            foreach (int jid in Job.unlockedJobs)
            {
                int mp = maxPeople[jid];
                string txt = jobemployedPeople[jid].ToString();
                if(mp != -1) txt +=  "/"+ mp;

                jobOverviewContent.GetChild(childIndex).Find("TextCount").GetComponent<Text>().text = txt;
                childIndex++;
            }
        }
    }
    private void UpdatePopulationList()
    {
        int i = 0;
        if (populationListContent.childCount - 1 != PersonScript.allPeople.Count)
        {
            for (i = 1; i < populationListContent.childCount; i++)
            {
                Destroy(populationListContent.GetChild(i).gameObject);
            }
            for(i = 0; i < PersonScript.allPeople.Count; i++)
            {
                int j = i;
                GameObject obj = (GameObject)Instantiate(populationListItemPrefab, populationListContent);
                obj.GetComponent<Button>().onClick.AddListener(() => OnPersonSelect(j));
            }
        }

        i = 0;
        foreach (PersonScript ps in PersonScript.allPeople)
        {
            Transform listItem = populationListContent.GetChild(i+1);
            listItem.GetChild(0).GetComponent<Text>().text = ps.nr.ToString();
            listItem.GetChild(1).GetComponent<Text>().text = ps.firstName;
            listItem.GetChild(2).GetComponent<Text>().text = ps.lastName;
            listItem.GetChild(3).GetComponent<Text>().text = (ps.gender == Gender.Male ? "M" : "W");
            listItem.GetChild(4).GetComponent<Text>().text = ps.age.ToString();
            listItem.GetChild(5).GetComponent<Text>().text = ps.job.name;
            i++;
        }
    }
    private void UpdateResourcesPanel()
    {
        Transform content = panelResources.Find("Content");
        if (content.childCount != ResourceData.unlockedResources.Count)
        {
            for (int i = 0; i < content.childCount; i++)
                Destroy(content.GetChild(i).gameObject);

            foreach(ResourceData rd in ResourceData.allResources)
            {
                if (!ResourceData.IsUnlocked(rd.id)) continue;
                int i = rd.id;
                GameObject obj = Instantiate(resourcePrefab, content);
                obj.transform.Find("Toggle").GetComponent<Toggle>().onValueChanged.AddListener((b) => OnResourceToggle(b, i));
            }
        }

        int childId = content.childCount - ResourceData.unlockedResources.Count;
        int index = 0;
        List<GameResources> totResources = myVillage.GetTotalResources(new List<int>(ResourceData.unlockedResources));
        foreach(int rid in ResourceData.unlockedResources)
        {
            ResourceData rd = ResourceData.Get(rid);
            content.GetChild(childId).Find("Image").GetComponent<Image>().sprite = rd.icon;
            content.GetChild(childId).Find("Text").GetComponent<Text>().text = totResources[index].Amount.ToString();
            childId++;
            index++;
        }
    }
    private void UpdateGrowthPanel()
    {
        int totFact = myVillage.GetTotalFactor();
        string color = "007F3D";
        if (totFact < 20) color = "E30913";
        else if (totFact < 40) color = "F49B03";
        else if (totFact < 60) color = "97C21E";
        else if (totFact < 80) color = "3BA136";
        
        gfactorTot.text = "<color=\"#"+color+"\">"+totFact+"</color>";
        gfactorRoomText.text = "Wohnraum:\n" + myVillage.GetRoomspaceFactor();
        gfactorFoodText.text = "Nahrung:\n" + myVillage.GetFoodFactor();
        gfactorHealthText.text = "Gesundheit:\n" + myVillage.GetHealthFactor(); ;
        gfactorFertilityText.text = "Fruchtbarkeit:\n" + myVillage.GetFertilityFactor();
        gfactorLuxuryText.text = "Luxus:\n" + myVillage.GetLuxuryFactor();
        gfactorSlider.anchoredPosition = new Vector3(420 * totFact / 100, 0, 0);
    }
    private void UpdateBuildPanel()
    {
        int unlockedBC = 0;
        bool placingBuildingStillAvailable = false;
        foreach(Building building in Building.allBuildings)
        {
            if (Building.IsUnlocked(building.id) && Building.PeoplePerBuildingFullfilled(building) && building.name != "Höhle")
            {
                unlockedBC++;
                if (building == BuildManager.placingBuilding) placingBuildingStillAvailable = true;
            }
        }
        if (!placingBuildingStillAvailable && inMenu > 0) BuildManager.placingBuilding = Building.Get(1);

        if (unlockedBC != buildImageListParent.childCount)
        {
            for (int i = 0; i < buildImageListParent.childCount; i++)
                Destroy(buildImageListParent.GetChild(i).gameObject);

            foreach(Building unlb in Building.allBuildings)
            {
                if (!Building.IsUnlocked(unlb.id)) continue;
                if (!Building.PeoplePerBuildingFullfilled(unlb)) continue;
                if (unlb.name == "Höhle") continue;

                GameObject obj = (GameObject)Instantiate(buildingBuildImagePrefab, buildImageListParent);
                obj.GetComponent<Image>().sprite = unlb.icon;
                int c = unlb.id;
                obj.GetComponent<Button>().onClick.AddListener(() => OnSelectBuilding(c));
            }
        }
        
        bool canPurchase = CanBuildBuilding();
        bool b;
        Color tcol;
        /*for (int i = 0; i < matC.Length; i++)
        {
            if (matC[i] == 0) continue;
            b = myVillage.GetResources(i).GetAmount() >= matC[i];
            tcol = b ? Color.black : Color.red;
            if (!b) canPurchase = false;
            buildResourceParent.GetChild(j).GetComponent<Text>().color = tcol;
            j+=2;
        }*/

        Building placing = BuildManager.placingBuilding;
        buildName.text = placing.name;
        buildDescription.text = placing.description;
        buildSize.text = placing.gridWidth + ":" + placing.gridHeight;

        b = myVillage.GetCoins() >= placing.cost;
        tcol = b ? Color.black : Color.red;
        if (!b) canPurchase = false;

        buildCost.text = placing.cost.ToString();
        buildCost.color = tcol;
        buildWorkspace.text = placing.workspace.ToString();
        buildPopulationRoom.text = placing.populationRoom[0].ToString();

        buildButton.enabled = canPurchase;
    } 
    private void UpdateTaskResPanel()
    {
        // only update if if a building is selected and person-queue is not empty
        if(taskResRequest.Count == 0) return;
        if(!selectedObject || !selectedObject.GetComponent<BuildingScript>()) return;

        // get all references to building/people scripts
        PersonScript ps = taskResRequest.Peek();
        BuildingScript bs = selectedObject.GetComponent<BuildingScript>();

        // variables used multiple times
        int invAmount = 0;
        GameResources inv = null;
        bool showAmBut = true;

        // update inventory (0=material,1=food)
        for(int i = 0; i < taskResInventory.childCount; i++)
        {
            invAmount = 0;

            if(i == 0) inv = ps.inventoryMaterial;
            else inv = ps.inventoryFood;

            Image resImg = taskResInventory.GetChild(i).Find("Image").GetComponent<Image>();
            Text resTxt = taskResInventory.GetChild(i).Find("Text").GetComponent<Text>();
            Tooltip tt = taskResInventory.GetChild(i).GetComponent<Tooltip>();
            if (inv != null)
            {
                if(i == taskResInvSelected)
                {
                    taskResInvMax = Mathf.Min(inv.Amount, bs.GetStorageTotal(inv)-bs.GetStorageCurrent(inv));
                    taskResInvSlider.maxValue = taskResInvMax;
                    int input = int.Parse(taskResInvInput.text);
                    input = Mathf.Clamp(input, 1, taskResInvMax);
                    
                    taskResInvImage.sprite = inv.Icon;
                    if (inv.Amount == 0) showAmBut = false;         
                }

                tt.text = inv.Description;

                invAmount = inv.Amount;
                resImg.sprite = inv.Icon;
                resImg.color = Color.white;
            }
            else if(i == taskResInvSelected)
            {
                showAmBut = false;
            }
            resTxt.text = invAmount + "/" + (i == 0 ? ps.GetMaterialInventorySize() : ps.GetFoodInventorySize());
            if(invAmount == 0) resImg.color = new Color(1,1,1,0.1f);

            resImg.GetComponent<Button>().enabled = invAmount > 0;

            tt.enabled = invAmount > 0;
        }

        taskResInvAm.gameObject.SetActive(showAmBut);
        taskResInvBut.gameObject.SetActive(showAmBut);

        List<GameResources> storedRes = GetStoredRes(bs);

        showAmBut = true;
        if(taskResStorage.childCount != storedRes.Count)
        {
            for(int i = 0; i < taskResStorage.childCount; i++)
                Destroy(taskResStorage.GetChild(i).gameObject);
            
            for(int i = 0; i < storedRes.Count; i++)
            {
                int j = i;
                Transform t = Instantiate(taskResPrefab, taskResStorage).transform;
                t.Find("Image").GetComponent<Button>().onClick.AddListener(() => OnTaskResStorSelect(j));
                Tooltip tt = t.gameObject.AddComponent<Tooltip>();
                tt.text = storedRes[i].Description;
            }
        }

        if(taskResStorSelected >= taskResStorage.childCount) taskResStorSelected = 0;

        for(int i = taskResStorage.childCount-storedRes.Count; i < taskResStorage.childCount; i++)
        {
            Image resImg = taskResStorage.GetChild(i).Find("Image").GetComponent<Image>();
            Text resTxt = taskResStorage.GetChild(i).Find("Text").GetComponent<Text>();
            Tooltip tt = taskResStorage.GetChild(i).GetComponent<Tooltip>();
            inv = storedRes[i - (taskResStorage.childCount-storedRes.Count)];
            if (inv != null)
            {
                if(i == taskResStorSelected)
                {
                    taskResStorMax = Mathf.Min(inv.Amount, ps.GetFreeInventorySpace(inv));
                    taskResStorSlider.maxValue = taskResStorMax;
                    int input = int.Parse(taskResStorInput.text);
                    input = Mathf.Clamp(input, 1, taskResStorMax);

                    //if(taskResStorMax == 0) taskResInvAm.gameObject.SetActive(taskResStorA)

                    taskResStorImage.sprite = inv.Icon;
                    if(inv.Amount == 0) showAmBut = false;    
                    if(taskResStorMax == 0) showAmBut = false;     
                }

                tt.text = inv.Description;

                invAmount = inv.Amount;
                resImg.sprite = inv.Icon;
                resImg.color = Color.white;
            }
            else if(i == taskResInvSelected)
            {
                showAmBut = false;
            }
            resTxt.text = invAmount + "/" + bs.GetStorageTotal(inv);
            if(invAmount == 0) resImg.color = new Color(1,1,1,0.1f);

            resImg.GetComponent<Button>().enabled = invAmount > 0;
            
            tt.enabled = invAmount > 0;
        }

        if(storedRes.Count == 0) showAmBut = false;

        taskResStorAm.gameObject.SetActive(showAmBut);
        taskResStorBut.gameObject.SetActive(showAmBut);
    }
    private int ppbw = 1;
    private void UpdatePersonPanel()
    {
        if (PersonScript.selectedPeople.Count > 0)
        {
            int spc = -1;
            if(PersonScript.selectedPeople.Count == 1) spc = 0;
            else if(PersonScript.selectedPeople.Count <= 6) spc = 1;
            else spc = 2;

            if(panelPeopleInfo6.childCount != PersonScript.selectedPeople.Count)
            {
                for(int i = 0; i < panelPeopleInfo6.childCount; i++)
                    Destroy(panelPeopleInfo6.GetChild(i).gameObject);

                foreach(PersonScript selectedPerson in PersonScript.selectedPeople)
                {
                    int k = selectedPerson.nr;
                    GameObject obj = (GameObject)Instantiate(personInfoPrefab, Vector3.zero, Quaternion.identity, panelPeopleInfo6);
                    obj.GetComponent<Button>().onClick.AddListener(() => OnPersonSelect(k));
                }
            }
            float maxWidth;
            int index = 0;
            foreach(PersonScript personScript in PersonScript.selectedPeople)
            {
                Transform panel = panelPeopleInfo6.GetChild(index);
                panel.Find("TextName").GetComponent<Text>().text = personScript.firstName;
                maxWidth = panel.Find("Health").Find("ImageHPBack").GetComponent<RectTransform>().rect.width - ppbw*2;
                panel.Find("Health").Find("ImageHP").GetComponent<RectTransform>().offsetMax = new Vector2(-(ppbw + maxWidth*(ppbw - personScript.GetHealthFactor())),-ppbw);
                panel.Find("Health").Find("ImageHP").GetComponent<Image>().color = personScript.GetConditionCol();
                index++;
            }

            panelSinglePersonInfo.gameObject.SetActive(spc == 0);
            panelPeopleInfo6.gameObject.SetActive(spc == 1);
            panelPeopleInfo7.gameObject.SetActive(spc == 2);

            PersonScript ps = new List<PersonScript>(PersonScript.selectedPeople)[0];

            personInfoName.text = ps.firstName + "\n"+ps.lastName;
            //personInfoGender.text = "Geschlecht: " + (ps.GetGender() == Gender.Male ? "M" : "W");
            //personInfoAge.text = "Alter: " + ps.GetAge().ToString();
            string infoText = "";
            if(ps.IsEmployed())
                infoText += "Beruf: " + ps.job.name + "\n";
            string task = "-";
            bool cont = ps.Controllable();
            if (ps.routine.Count > 0)
            {
                Task ct = ps.routine[0];
                if(ct.taskType == TaskType.Walk && ps.routine.Count > 1) ct = ps.routine[1];
                if(ct.taskType == TaskType.CutTree) task = "Holz hacken";
                if(ct.taskType == TaskType.Fishing) task = "Fischen";
                if(ct.taskType == TaskType.Fisherplace) task = "Fisch Verarbeiten";
                if(ct.taskType == TaskType.Harvest) task = "Ernten";
                if(ct.taskType == TaskType.CollectMushroom) task = "Sammeln";
                if(ct.taskType == TaskType.MineRock) task = "Fels abbauen";
                if(ct.taskType == TaskType.Build) task = "Bauen";
                if(ct.taskType == TaskType.Craft) task = "Verarbeiten";
                if(ct.taskType == TaskType.HuntAnimal) task = "Jagen";
                if(ct.taskType == TaskType.ProcessAnimal) task = "Tier verarbeiten";
            }
            if (cont)
                infoText += "Aufgabe: " + task + "\n";
            else if (ps.AgeState() == 1)
                infoText += "Folgt Mutter\n";
            else infoText += "Will nur spielen\n";
            infoText += "Zustand: " + ps.GetConditionStr() + "";
            personInfo.text = infoText;
            maxWidth = personInfoHealthbar.transform.parent.Find("Back").GetComponent<RectTransform>().rect.width - ppbw*2;
            personInfoHealthbar.rectTransform.offsetMax = new Vector2(-(ppbw + maxWidth * (1f-ps.GetHealthFactor())),-ppbw);
            personInfoHealthbar.color = ps.GetConditionCol();
            maxWidth = personInfoFoodbar.transform.parent.Find("Back").GetComponent<RectTransform>().rect.width - ppbw*2;
            personInfoFoodbar.rectTransform.offsetMax = new Vector2(-(ppbw + maxWidth * (1f-ps.GetFoodFactor())),-ppbw);
            personInfoFoodbar.color = ps.GetFoodCol();
            personJobBut.GetComponentInChildren<Text>().text = (ps.job.type == JobType.Unemployed ? "Einstellen":"Entlassen");

            personImage.sprite = personIcons[ps.gender == Gender.Male ? 0 : 1];

            personJobBut.gameObject.SetActive(cont);
            personBuildBut.gameObject.SetActive(cont);

            peopleInfo7.text = PersonScript.selectedPeople.Count+" Bewohner ausgewählt";

            int invAmount = 0;
            GameResources invMat = ps.inventoryMaterial;
            GameResources invFood = ps.inventoryFood;

            // update material inventory slots
            Tooltip tp = personInventoryMatImage.GetComponentInParent<Tooltip>();
            if (invMat != null)
            {
                invAmount = invMat.Amount;
                personInventoryMatImage.sprite = invMat.Icon;
                personInventoryMatImage.color = Color.white;
                if (tp) tp.text = invMat.Description;
            }
            personInventoryMatText.text = invAmount + "/" + ps.GetMaterialInventorySize();
            if(invAmount == 0) 
            {
                personInventoryMatImage.color = new Color(1,1,1,0.1f);
            }
            if(tp) tp.enabled = invAmount > 0;

            // same for food
            invAmount = 0;
            tp = personInventoryFoodImage.GetComponentInParent<Tooltip>();
            if (invFood != null)
            {
                invAmount = invFood.Amount;
                personInventoryFoodImage.sprite = invFood.Icon;
                personInventoryFoodImage.color = Color.white;
                if(tp) tp.text = invFood.Description;
            }
            personInventoryFoodText.text = invAmount + "/" + ps.GetFoodInventorySize();
            if(invAmount == 0) 
            {
                personInventoryFoodImage.color = new Color(1,1,1,0.1f);
            }
            if(tp) tp.enabled = invAmount > 0;

            //personInventoryImage.gameObject.SetActive(inv != null);
            //personInventoryText.text = invAmount + "/" + ps.GetInventorySize() +" kg";
        }
    }
    private void UpdateObjectInfoPanel()
    {
        if (selectedObject == null || !selectedObject.gameObject.activeSelf)
        {
            if (objectInfoShown)
            {
                //panelObjectInfoSmall.gameObject.SetActive(false);
                panelObjectInfoSmall.GetComponent<Animator>().SetBool("show",false);
                OnHideObjectInfo();
                objectInfoShown = false;
            }
            return;
        }

        NatureObjectScript natureObjectScript = selectedObject.GetComponent<NatureObjectScript>();
        if (natureObjectScript != null)
        {
            objectInfoTitle.text = natureObjectScript.Name;
            objectInfoSmallTitle.text = natureObjectScript.Name;
            GameResources plantRes = new GameResources(natureObjectScript.ResourceCurrent);
            string desc = natureObjectScript.Description;
                    objectInfoImage.sprite = null;
            switch (natureObjectScript.Type)
            {
                case NatureObjectType.Tree:
                    objectInfoImage.sprite = treeIcons[0];
                    desc += "\nGrösse: " + natureObjectScript.GetSizeInMeter() + "m\n" + natureObjectScript.ResourceCurrent.Amount + "kg";
                    break;
                case NatureObjectType.Rock:
                    objectInfoImage.sprite = rockIcons[0];
                    desc += "\n"+ natureObjectScript.ResourceCurrent.Amount + "kg";
                    break;
                case NatureObjectType.Mushroom:
                    desc += "\nSättigung: "+plantRes.Nutrition;
                    break;
                case NatureObjectType.MushroomStump:
                    desc += "\n"+ natureObjectScript.ResourceCurrent.Amount + " Pilze";
                    break;
                /*case NatureObjectType.Reed:
                    desc += "\n"+ natureObjectScript.ResourceCurrent.Amount + " Fische";
                    break;*/
                case NatureObjectType.Crop:
                    break;
                case NatureObjectType.EnergySpot:
                    desc += natureObjectScript.IsBroken() ? " (eingenommen)" : "";
                    break;
                default:
                    Debug.Log("Unhandled object: " + natureObjectScript.Type.ToString());
                    break;
            }
            objectInfoText.text = desc;
        }
        BuildingScript bs = selectedObject.GetComponent<BuildingScript>();
        if(bs != null)
        {
            // if (bs.bluePrint) name += " (prov.)";
            objectInfoSmallTitle.text = bs.Name;
            objectInfoTitle.text = bs.Name;
            objectInfoImage.sprite = bs.Icon;
            objectInfoText.text = bs.Description;
        }
        ItemScript item = selectedObject.GetComponent<ItemScript>();
        if (selectedObject.tag == ItemScript.Tag && item != null)
        {
            objectInfoSmallTitle.text = item.ResName;
            objectInfoTitle.text = item.ResName;
            objectInfoImage.sprite = item.Resource.icon;
            objectInfoText.text = "Kann eingesammelt werden";
        }
        AnimalScript animal = selectedObject.GetComponent<AnimalScript>();
        if (selectedObject.tag == Animal.Tag && animal != null)
        {
            objectInfoSmallTitle.text = animal.Name;
            objectInfoTitle.text = animal.Name;
            objectInfoText.text = "Kann von einem Jäger gejagt werden\nLeben: "+(int)(100f*animal.HealthFactor())+"%";
        }
        PersonScript person = selectedObject.GetComponent<PersonScript>();
        if (selectedObject.tag == "Person" && person != null)
        {
            objectInfoSmallTitle.text = person.firstName;
            objectInfoTitle.text = person.firstName;
            objectInfoText.text = "Kann in dein Dorf augenommen werden.";

            if (!person.wild) OnHideObjectInfo();
        }
    }
    private int bpbw = 1;
    private void UpdateBuildingInfoPanel()
    {
        if (selectedObject != null)
        {
            BuildingScript bs = selectedObject.GetComponent<BuildingScript>();
            if (bs != null)
            {
                buildingInfoTitle.text = bs.Name + (bs.Blueprint ? " (Baustelle)" : "");
                string desc = bs.TotalDescription();

                buildingInfoText.text = desc;

                // Set visibilty of lifebar
                buildingInfoLifebar.gameObject.SetActive(bs.HasLifebar && !bs.Blueprint);
                //buildingInfoLifebarImage.rectTransform.offsetMin = new Vector2(2,2);
                if(bs.HasLifebar)
                { 
                    buildingInfoLifebarImage.rectTransform.offsetMax = new Vector2(-(bpbw + 
                        (buildingInfoLifebar.GetComponent<RectTransform>().rect.width- bpbw*2) * (1f-bs.LifebarFactor)),-bpbw);
                }

                // Only show if building is a hut
                buildingUpgradeShelter.gameObject.SetActive(bs.IsHut() && !bs.Blueprint);
                buildingUpgradeShelter.GetComponentInChildren<Text>().text = bs.MaxStage() ? "Maximale Ausbaustufe erreicht" : "Zu " +bs.Name+" Stufe "+(bs.Stage+2) + " upgraden";
                buildingUpgradeShelter.interactable = !bs.MaxStage() && (myVillage.EnoughResources(bs.GetCostResource(bs.Stage + 1)) || GameManager.IsDebugging());

                buildingUpgradeCostText.gameObject.SetActive(bs.IsHut() && !bs.Blueprint && !bs.MaxStage());
                buildingInfoUpgradeCost.gameObject.SetActive(bs.IsHut() && !bs.Blueprint && !bs.MaxStage());
                DisplayResourceCosts(bs.GetCostResource(bs.Stage+1), buildingInfoUpgradeCost);

                buildingButtonJobs.gameObject.SetActive(bs.IsHut() && !bs.Blueprint && bs.Stage > 0 && bs.FamilyJobId == 0);
                buildingButtonFields.gameObject.SetActive(bs.IsHut() && !bs.Blueprint && (Job.Name(bs.FamilyJobId) == "Bauer" || Job.Name(bs.FamilyJobId) == "Fischer"));
                buildingButtonFields.Find("Farmer").gameObject.SetActive(Job.Name(bs.FamilyJobId) == "Bauer");
                buildingButtonFields.Find("Fisher").gameObject.SetActive(Job.Name(bs.FamilyJobId) == "Fischer");

                // Only show buttons if movable/destroyable
                buildingMoveBut.transform.gameObject.SetActive(bs.Movable && !bs.Blueprint);
                buildingRemoveBut.transform.gameObject.SetActive(bs.Destroyable || bs.Blueprint);
                buildingRemoveBut.GetComponentInChildren<Text>().text = bs.Blueprint ? "Abbrechen" : "Abreissen";
                buildingMoveBut.transform.parent.gameObject.SetActive(bs.Movable || bs.Destroyable);

                List<GameResources> storedRes = GetStoredRes(bs);

                List<GameResources> resDisplay = bs.Blueprint ? bs.BlueprintBuildCost : storedRes;
                // Set storage-res UI visibility
                buildingInfoStorage.gameObject.SetActive(resDisplay.Count > 0);
                buildingStorageText.text = (bs.Blueprint ? "Kosten:" : "Gelagerte Ressourcen:");
                buildingStorageText.gameObject.SetActive(resDisplay.Count > 0);

                DisplayResourcesBuilding(resDisplay, bs, buildingInfoStorage);

                /*buildingInfoName.text = b.GetName();
                buildingInfoDesc.text = b.GetDescription();
                buildingInfoStage.text = "Stufe 1";*/
            }

            /* TODO: individual building info */
            
        }
    }
    private void UpdateRecruitingPanel()
    {
        // TODO
        /*if (selectedObject != null && selectedObject.tag == "Building")
        {
            Building b = selectedObject.GetComponent<Building>();
            
            if (recruitingUnitParent.childCount != Troop.COUNT)
            {
                for (int i = 0; i < recruitingUnitParent.childCount; i++)
                {
                    Destroy(recruitingUnitParent.GetChild(i).gameObject);
                }
                for (int i = 0; i < Troop.COUNT; i++)
                {
                    int j = i;
                    Instantiate(recruitingUnitPrefab, recruitingUnitParent.transform).transform.Find("Button").GetComponent<Button>().onClick.AddListener(() =>
                    {
                        b.recruitingTroop.Add(Troop.FromID(j));
                    });
                }
            }
            else
            {
                for (int i = 0; i < Troop.COUNT; i++)
                {
                    Troop tr = Troop.FromID(i);
                    //recruitingUnitParent.GetChild(i).Find("Button").GetComponent<Image>().sprite = null;
                    recruitingUnitParent.GetChild(i).Find("Text").GetComponent<Text>().text = tr.Desc();
                    
                }
            }

            Troop recrTroop = null;
            if (b.recruitingTroop.Count > 0) recrTroop = b.recruitingTroop[0];

            if(recrTroop == null)
            {
                recruitingTimeImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 0);
                recruitingTimeText.text = "-";
            }
            else
            {
                float perc = 1f - recrTroop.recruitingTime / Troop.FromID(recrTroop.id).recruitingTime;
                recruitingTimeImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 250f * perc);
                recruitingTimeText.text = (recrTroop.recruitingTime).ToString("00") + ":" + ((recrTroop.recruitingTime % 1) *100f).ToString("00");
            }
        }*/
    }
    private void UpdateTechTree()
    {
        Color researchedCol, unlockedCol;
        ColorUtility.TryParseHtmlString("#E26E5F", out researchedCol);
        ColorUtility.TryParseHtmlString("#5FE2CB", out unlockedCol);

        for (int i = 0; i < techTreeAge1.childCount; i++)
        {
            bool unl = myVillage.techTree.IsUnlocked(i);
            bool res = myVillage.techTree.IsResearched(i);
            Button b = techTreeAge1.GetChild(i).GetComponent<Button>();
            if (b == null) continue;
            b.enabled = unl && !res;
            b.GetComponent<Image>().color = res ? researchedCol : (unlockedCol * (unl ? 1f : 0.5f));
            if (i >= myVillage.techTree.tree.Count) continue;
            TechBranch br = myVillage.techTree.tree[i];
            Tooltip tt = b.GetComponent<Tooltip>();
            if(tt)
            {
                tt.enabled = unl;
                if (res)
                {
                    tt.text = br.name + "\nErforscht";
                }
                else
                {
                    string costStr = "";
                    foreach (GameResources cost in br.costResource)
                    {
                        if (cost.Amount > 0)
                            costStr += cost.Name + ": " + cost.Amount + ", ";
                    }
                    if (costStr.Length > 0) costStr = costStr.Substring(0, costStr.Length - 2);

                    tt.text = br.name + "\nKosten: " + costStr + "\nGlaubenspunkte: " + br.costFaith + "\nZeit: " + br.researchTime;
                }
            }
        }
    }
    private void UpdateAchievements()
    {
        if (achievementContent.childCount != 8)
        {
            Debug.Log("wrong ach ui list size");
        }
        else
        {
            int childId = 0;
            foreach(GameAchievement gach in GameManager.gameData.achievements)
            {
                Achievement ach = gach.achievement;
                int lvl = gach.GetLvl();
                string text = "<b>"+ ach.name+"</b>\n";
                if (lvl > 0)
                {
                    text += "Stufe " + lvl + " - ";
                    text += ach.TaskDescription(ach.amountLvl[lvl - 1].ToString());
                }
                text += "<i>Nächste Stufe (" + (lvl + 1) + ") - ";
                string amtxt = gach.currentAmount + "/" + ach.amountLvl[lvl];

                text += ach.TaskDescription(amtxt);
                text += "</i>";
                achievementContent.GetChild(childId).GetComponent<Text>().text = text;

                childId++;
            }
        }
    }
    private void UpdateQuests()
    {
        for (int i = 0; i < GameManager.gameData.openQuests.Count; i++)
            GameManager.gameData.openQuests[i].UpdateFinished();

        if (questsContent.childCount != GameManager.gameData.openQuests.Count)
        {
            for (int i = 0; i < questsContent.childCount; i++)
                Destroy(questsContent.GetChild(i).gameObject);

            foreach (GameQuest gq in GameManager.gameData.openQuests)
            {
                Instantiate(questTextPrefab, questsContent);
            }
        }
        int index = 0;
        int childIndex = questsContent.childCount - GameManager.gameData.openQuests.Count;
        foreach (GameQuest gq in GameManager.gameData.openQuests)
        {
            Text t = questsContent.GetChild(childIndex).GetComponent<Text>();
            t.text = "";
            if (gq.Finished()) t.text += "<color=#70AD36>";
            t.text += "<b>" + gq.quest.name + " (" + (int)(100f * gq.Percentage()) + "%)</b>";
            if (gq.Finished()) t.text += "</color>";
            t.text += "\n" + gq.quest.description;
            index++;
            childIndex++;
        }
    }
    public void UpdateSettingsPanel()
    {
        /* maybe needed later */
    }
    public void UpdateFeedbackPanel()
    {
        
    }
    public void UpdateDebugPanel()
    {
        List<PersonScript> list = new List<PersonScript>(PersonScript.selectedPeople);
        if(list.Count == 0) return;
        PersonScript ps = list[0];
        
        string text = "";
        text += "Name: "+ps.firstName + "\n";
        text += "Routine: "+ps.routine.Count + "\n";
        if(ps.routine.Count > 0)
        {
            Task ct = ps.routine[0];
            text += "Task: "+ct.taskType.ToString() + "\n";
            text += "Time: "+ct.taskTime.ToString("F2") + "\n";
            text += "TargetPos: "+ct.target + "\n";
            text += "TargetTrsf: "+ct.targetTransform + "\n";
            if (ct.targetTransform != null)
            {
                NatureObjectScript natureObjectScript = ct.targetTransform.GetComponent<NatureObjectScript>();
                if(natureObjectScript != null)
                {
                    text += "IsBroken: "+ natureObjectScript.IsBroken() + "\n";
                    text += "MatID: "+ natureObjectScript.ResourceCurrent.Id + "\n";
                    text += "MatAmount: "+ natureObjectScript.ResourceCurrent.Amount + "\n";
                }
            }
            GameResources inv = ps.inventoryMaterial;
            if(inv != null)
            {
                text += "InvMatName: "+inv.Name + "\n";
                text += "InvMatAmount: "+inv.Amount + "\n";
                text += "InvMatType: "+inv.Type + "\n";
                text += "InvMatNutr: "+inv.Nutrition + "\n";
            }
            inv = ps.inventoryFood;
            if(inv != null)
            {
                text += "InvFoodName: "+inv.Name + "\n";
                text += "InvFoodAmount: "+inv.Amount + "\n";
                text += "InvFoodType: "+inv.Type + "\n";
                text += "InvFoodNutr: "+inv.Nutrition + "\n";
            }
        }

        debugText.text = text;
    }

    private void CheckResourceDisplay(List<GameResources> list, Transform parent)
    {
        if (parent.childCount != list.Count)
        {
            for (int i = 0; i < parent.childCount; i++)
                Destroy(parent.GetChild(i).gameObject);

            for (int i = 0; i < list.Count; i++)
            {
                int j = i;
                Transform t = Instantiate(taskResPrefab, parent).transform;
                t.Find("Image").GetComponent<Button>().onClick.AddListener(() => OnTaskResStorSelect(j));
            }
        }
    }
    private void DisplayResourceCosts(List<GameResources> list, Transform parent)
    {
        CheckResourceDisplay(list, parent);

        List<GameResources> total = myVillage.GetTotalResourceCount();

        GameResources inv = null;
        int invAmount = 0;
        int tot = 0;
        for (int i = parent.childCount - list.Count; i < parent.childCount; i++)
        {
            Image resImg = parent.GetChild(i).Find("Image").GetComponent<Image>();
            Text resTxt = parent.GetChild(i).Find("Text").GetComponent<Text>();
            inv = list[i - (parent.childCount - list.Count)];
            tot = myVillage.GetTotalResource(inv);
            if (inv != null)
            {
                invAmount = inv.Amount;
                resImg.sprite = inv.Icon;
                resTxt.color = tot < invAmount ? Color.red : Color.black;
            }
            resTxt.text = tot + "/" + invAmount;

            resImg.GetComponent<Button>().enabled = false;// invAmount > 0;
        }
    }
    private void DisplayResourcesBuilding(List<GameResources> list, BuildingScript storage, Transform parent)
    {
        CheckResourceDisplay(list, parent);

        GameResources inv = null;
        int invAmount = 0;
        for (int i = parent.childCount - list.Count; i < parent.childCount; i++)
        {
            Image resImg = parent.GetChild(i).Find("Image").GetComponent<Image>();
            Text resTxt = parent.GetChild(i).Find("Text").GetComponent<Text>();
            inv = list[i - (parent.childCount - list.Count)];
            int cost = storage.GetCostResource(inv);
            if (inv != null)
            {
                invAmount = inv.Amount;
                resImg.sprite = inv.Icon;
                resImg.color = Color.white;
                if (storage.Blueprint) invAmount = cost - invAmount;
            }
            resTxt.text = invAmount + (storage == null ? "" : ("/" + (storage.Blueprint ? cost : storage.GetStorageTotal(inv))));
            if (!storage.Blueprint && invAmount == 0 || storage.Blueprint && invAmount == cost) resImg.color = new Color(1, 1, 1, 0.7f);

            resImg.GetComponent<Button>().enabled = invAmount > 0;
        }
    }

    public void OnPopulationTab(int i)
    {
        populationTabs.Find("ListOverviewTab").Find("PanelTab").gameObject.SetActive(i == 0);
        populationTabs.Find("JobOverviewTab").Find("PanelTab").gameObject.SetActive(i == 1);
    }
    private void OnResourceToggle(bool b, int i)
    {
        if (!b)
        {
            if (GameManager.FeaturedResources.Contains(i))
                GameManager.FeaturedResources.Remove(i);
        }
        else
        {
            GameManager.FeaturedResources.Add(i);
        }
    }

    public void OnExitGame()
    {
        // Save game before exciting
        SaveLoadManager.SaveGame();
        StartCoroutine(LoadYourAsyncScene());
    }

    IEnumerator LoadYourAsyncScene()
    {
        GameManager.FadeOut();

        // The Application loads the Scene in the background as the current Scene runs.
        // This is particularly good for creating loading screens.
        // You could also load the Scene by using sceneBuildIndex. In this case Scene2 has
        // a sceneBuildIndex of 1 as shown in Build Settings.

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("MainMenu");
        asyncLoad.allowSceneActivation = false;

        // Wait until the asynchronous scene fully loads
        while (asyncLoad.progress < 0.9f) {
            yield return null;
        }

        // wait for fadeout to finish before changing scene
        while(!GameManager.HasFaded() && !SaveLoadManager.errorWhileLoading)
        {
            yield return null;
        }

        SaveLoadManager.errorWhileLoading = false;

        BuildingScript.DestroyAllBuildings();
        AnimalScript.DestroyAllAnimals();
        NatureObjectScript.DestroyAllNatureObjects();

        asyncLoad.allowSceneActivation = true;
    }

    public void Blink(string uiName, bool on)
    {
        Animator[] animators = canvas.GetComponentsInChildren<Animator>();
        for(int i = 0; i < animators.Length; i++)
        {
            if(animators[i].transform.name == uiName)
                animators[i].SetBool("highlighted",on);
        }
    }
    public void ShowMenu(int i)
    {
        if (!InputManager.InputUI()) return;

        string uiName = "";
        switch(i)
        {
            case 1: uiName = "PanelTopPopulation"; break;
            case 3: uiName = "PanelTopResources"; break;
            case 4: uiName = "PanelTopFactors"; break;
            case 5: uiName = "PanelTopYear"; break;
            case 6: uiName = "PanelSettings"; break;
            case 7: uiName = "ButtonBuild"; break;
            case 12: uiName = "PanelInfo"; break;
            case 14: uiName = "PanelTopFaith"; break;
            case 16: uiName = "PanelTopTechTree"; break;
            case 18: uiName = "PanelTopQuests"; break;
        }
        if(uiName != "")
        {
            Blink(uiName, false);
        }

        ExitMenu();
        inMenu = i;

        if(i == 12) OnFeedbackCategory(0);
    }

    public void OnSelectBuilding(int bid)
    {
        BuildManager.placingBuilding = Building.Get(bid);
        
        foreach (Transform child in buildResourceParent)
        {
            Destroy(child.gameObject);
        }

        foreach(GameResources cost in BuildManager.placingBuilding.costResource[0].list)
        {
            if (cost.Amount == 0) continue;

            GameObject obj = Instantiate(buildResourceImagePrefab, buildResourceParent);
            obj.GetComponent<Image>().sprite = cost.Icon;
            obj = Instantiate(buildResourceTextPrefab, buildResourceParent);
            obj.GetComponent<Text>().text = cost.Amount.ToString();
        }
    }
    public void OnPlaceBuildingButtonClick()
    {
        bool canBuy = true;

        if (myVillage.GetCoins() < BuildManager.placingBuilding.cost) canBuy = false;
        if(!CanBuildBuilding()) canBuy = false;
        //if (myVillage.Get < b.populationUse) canBuy = false;
        //for (int j = 0; j < myVillage.GetResourcesCountBuildingMaterial(); j++)
        /*for (int j = 0; j < 3; j++)
            if (myVillage.GetResources(j).GetAmount() < buildingMenuSelected.GetAllMaterialCost()[myVillage.GetResources(j).GetID()]) canBuy = false;*/

        //for (int j = 0; j < myVillage.GetResourcesCount(); j++)
           // myVillage.GetResources(j).Take(buildingMenuSelected.GetAllMaterialCost()[myVillage.GetResources(j).GetID()]);



        if (!canBuy) return;

        ExitMenu();
        BuildManager.StartPlacing();
    }
    public void OnShowObjectInfo(Transform trf)
    {
        if (!InputManager.InputUI()) return;

        objectInfoShown = true;
        selectedObject = trf;

        //panelObjectInfoSmall.gameObject.SetActive(false);
        panelObjectInfoSmall.GetComponent<Animator>().SetBool("show",false);
        if(selectedObject.tag == "Building")
        {
            //panelBuildingInfo.gameObject.SetActive(true);
            panelBuildingInfo.GetComponent<Animator>().SetBool("show",true);
            panelObjectInfo.GetComponent<Animator>().SetBool("show",false);

            BuildingScript bs = selectedObject.GetComponent<BuildingScript>();
            if (bs.Name == "Kriegsplatz")
            {
                ShowMenu(15);
            }
            //panelObjectInfo.gameObject.SetActive(false);
        }
        else
        {
            //panelBuildingInfo.gameObject.SetActive(false);
            panelBuildingInfo.GetComponent<Animator>().SetBool("show",false);
            panelObjectInfo.GetComponent<Animator>().SetBool("show",true);
            //panelObjectInfo.gameObject.SetActive(true);
        }
    }
    public void OnShowSmallObjectInfo(Transform trf)
    {
        if (!InputManager.InputUI()) return;
        if (objectInfoShown) return;

        objectInfoShownSmall = true;
        selectedObject = trf;
        panelObjectInfoSmall.GetComponent<Animator>().SetBool("show",true);
        //panelObjectInfoSmall.gameObject.SetActive(true);
    }
    public void OnHideObjectInfo()
    {
        //panelBuildingInfo.gameObject.SetActive(false);
        //panelObjectInfo.gameObject.SetActive(false);
        panelObjectInfo.GetComponent<Animator>().SetBool("show",false);
        panelBuildingInfo.GetComponent<Animator>().SetBool("show",false);
        objectInfoShown = false;
        selectedObject = null;
    }
    public void OnHideSmallObjectInfo()
    {
        panelObjectInfoSmall.GetComponent<Animator>().SetBool("show",false);
        //panelObjectInfoSmall.gameObject.SetActive(false);
        objectInfoShownSmall = false;
    }
    public void ShowPersonInfo(bool show)
    {
        if (!InputManager.InputUI() && show) return;

        personInfoShown = show;
        panelPeopleInfo.gameObject.SetActive(show);
    }
    public void OnCameraRotate()
    {
        Vector3 rot = Camera.main.transform.rotation.eulerAngles;
        Camera.main.transform.rotation = Quaternion.Euler(rot.x, rot.y + 90, rot.z);
        //Camera.main.transform.Rotate(Vector3.up, 90, Space.Self);
    }
    public void OnShowBuildingInfo(Transform trf)
    {
        ShowMenu(8);
        selectedObject = trf;
    }
    public void OnToggleInvertedMousewheel(bool inverted)
    {
        PlayerPrefs.SetInt("InvertedMousewheel",inverted ? 1 : 0);
        Camera.main.GetComponent<CameraController>().SetInvertedMousewheel(inverted);
    }
    public void OnPersonSelect(int i)
    {
        ExitMenu();
        PersonScript ps = new List<PersonScript>(PersonScript.allPeople)[i];
        PersonScript.DeselectAll();
        ps.OnClick();
        CameraController.ZoomSelectedPeople();
    }
    public void ToggleMiniMap()
    {
        if (inMenu != 13)
            ShowMenu(13);
        else ExitMenu();
    }
    public void EnableFaithBar()
    {
        topFaith.gameObject.SetActive(true);
    }
    public bool IsFaithBarEnabled()
    {
        return topFaith.gameObject.activeSelf;
    }
    public void EnableTechTree()
    {
        topTechTree.gameObject.SetActive(true);
    }
    public bool IsTechTreeEnabled()
    {
        return topTechTree.gameObject.activeSelf;
    }
    public bool IsTransformSelected(Transform transform)
    {
        return objectInfoShown && selectedObject == transform;
    }

    public void OnBuildingMove()
    {
        // make sure a building is selected
        if(!selectedObject) return;
        BuildingScript bs = selectedObject.GetComponent<BuildingScript>();
        if(!bs) return;

        // destroy building
        BuildManager.StartMoving(bs);
    }

    public void OnBuildingRemove()
    {
        // make sure a building is selected
        if(!selectedObject) return;
        BuildingScript bs = selectedObject.GetComponent<BuildingScript>();
        if(!bs) return;

        // destroy building
        bs.DestroyBuilding();
    }
    public void OnDropItem(int invId)
    {
        PersonScript selected = PersonScript.FirstSelectedPerson();
        if(!selected) return;
        GameResources res = invId == 0 ? selected.inventoryMaterial : selected.inventoryFood;
        if(res == null || res.Amount == 0) return;

        while(res.Amount > 0)
        {
            int am = Mathf.Min(res.Amount, UnityEngine.Random.Range(1,3));
            ItemManager.SpawnItem(res.Id, am, selected.transform.position, 0.3f, 0.3f);
            res.Take(am);
        }
    }

    public void OnTaskResInvSlider()
    {
        taskResInvInput.text = taskResInvSlider.value.ToString("F0");
    }
    public void OnTaskResInvInput()
    {
        int am = int.Parse(taskResInvInput.text);
        if(am >= 1 && am <= taskResInvMax)
            taskResInvSlider.value = am;
        else taskResInvInput.text = taskResStorSlider.value.ToString("F0");
    }
    public void OnTaskResStorSlider()
    {
        taskResStorInput.text = taskResStorSlider.value.ToString("F0");
    }
    public void OnTaskResStorInput()
    {
        int am = int.Parse(taskResStorInput.text);
        if(am >= 1 && am <= taskResStorMax)
            taskResStorSlider.value = am;
        else taskResStorInput.text = taskResStorSlider.value.ToString("F0");
    }
    public void OnTaskResInvMax()
    {
        taskResInvInput.text = taskResInvMax.ToString();
        taskResInvSlider.normalizedValue = 1;
    }
    public void OnTaskResStorMax()
    {
        taskResStorInput.text = taskResStorMax.ToString();
        taskResStorSlider.normalizedValue = 1;
    }
    public void OnTaskResInv()
    {
        DoTaskRes(0);
    }
    public void OnTaskResStor()
    {
        DoTaskRes(1);
    }
    public void DoTaskRes(int i)
    {
        BuildingScript bs = selectedObject.GetComponent<BuildingScript>();
        PersonScript ps = taskResRequest.Peek();
        GameResources res = null;
        TaskType tt = TaskType.None;
        if(i == 0) // Inv -> Warehouse
        {
            res = taskResInvSelected == 0 ? ps.inventoryMaterial : ps.inventoryFood;
            res = new GameResources(res.Id, (int)taskResInvSlider.value);
            tt = TaskType.BringToWarehouse;
        }
        else // Warehouse -> Inv
        {
            List<GameResources> storedRes = GetStoredRes(bs);
            if(taskResStorSelected >= storedRes.Count || storedRes[taskResStorSelected] == null) return;
            // copy res to a new instance
            res = new GameResources(storedRes[taskResStorSelected].Id, (int)(taskResStorSlider.value));
            tt = TaskType.TakeFromWarehouse;
        }
        if(ps.AddResourceTask(tt, bs, new GameResources(res))) {
            taskResRequest.Dequeue();
            if(taskResRequest.Count == 0) ExitMenu();
        }
    }
    public void OnTaskResInvSelect(int id)
    {
        taskResInvSelected = id;
    }
    public void OnTaskResStorSelect(int id)
    {
        taskResStorSelected = id;
    }
    public void OnUpgradeShelter()
    {
        BuildingScript bs = GetSelectedBuilding();

        // Take Resource cost to build next stage of building
        if (myVillage.TakeResources(bs.GetCostResource(bs.Stage + 1)))
        {
            bs.NextStage();
        }
    }
    public void OnSelectBuildingJob(Job job)
    {
        GetSelectedBuilding().SetFamilyJob(job);
    }
    public void OnPlaceField(Building b)
    {
        OnSelectBuilding(b.id);
        BuildManager.StartPlacing();
    }

    string[] categoriesStr = {"Bugs", "Vorschläge", "Fragen"};
    Color[] categoriesCol = {new Color(215f/255f, 58f/255f, 74f/255f),new Color(0f/255f, 82f/255f, 204f/255f),new Color(216f/255f, 118f/255f, 227f/255f)};
    int currentCat = 0;
    public void OnFeedbackCategory(int cat)
    {
        currentCat = cat;
        if (feedBackNew.gameObject.activeSelf)
        {
            myFeedback.category = cat;
            Text txtCat = feedBackNew.Find("Category/TextCategory").GetComponent<Text>();
            txtCat.text = categoriesStr[cat];
            txtCat.color = categoriesCol[cat];
        }
        else
        {
            for(int i = 0; i < feedBackContent.childCount; i++)
            {
                Destroy(feedBackContent.GetChild(i).gameObject);
            }
            foreach(Feedback fb in OnlineDatabaseManager.GetFeedbackList(cat))
            {
                Transform entry = Instantiate(feedBackEntryPrefab, feedBackContent).transform;
                Transform desc = entry.Find("PanelDesc");
                desc.Find("Category").GetComponent<Text>().text = categoriesStr[cat];
                desc.Find("Category").GetComponent<Text>().color = categoriesCol[cat];
                desc.Find("Title").GetComponent<Text>().text = fb.title;
                desc.Find("Creator").GetComponent<Text>().text = fb.creator;
                desc.Find("Date").GetComponent<Text>().text = fb.date;
                entry.Find("PanelContent").Find("TextContent").GetComponent<Text>().text = fb.text;
            }
        }
    }

    public void OnNewFeedback()
    {
        feedBackList.gameObject.SetActive(false);
        feedBackNew.gameObject.SetActive(true);
        OnFeedbackCategory(currentCat);  
    }
    public void OnNewFeedbackBack()
    {
        feedBackList.gameObject.SetActive(true);
        feedBackNew.gameObject.SetActive(false);
    }

    public void OnSubmitFeedback()
    {
        myFeedback.title = feedBackInputTitle.text;
        myFeedback.creator =GameManager.Username;
        myFeedback.text = feedBackInputText.text;
        DateTime now = DateTime.Now;
        myFeedback.date = now.Year+"-"+now.Month+"-"+now.Day;
        StartCoroutine(OnlineDatabaseManager.AddNewFeedback(myFeedback));
        StartCoroutine(SendFeedbackEmail(myFeedback));
        OnNewFeedbackBack();
    }
    private IEnumerator SendFeedbackEmail(Feedback fb)
    {
        MailMessage mail = new MailMessage();

        string from = "Dorfbauprojekt";
        string to = "Dorfbauprojekt@googlemail.com";
        mail.From = new MailAddress(from);
        mail.To.Add(to);
        mail.Subject = "Dorfbauprojekt Rückmeldung: "+fb.title;
        mail.Body = "Kategorie: "+ categoriesStr[fb.category] + "\nErsteller: " + fb.creator + "\nDatum: " + fb.date + "\n\n" + fb.text;

        SmtpClient smtpServer = new SmtpClient("smtp.gmail.com");
        smtpServer.Port = 587;
        smtpServer.Credentials = new NetworkCredential("flokicker123@gmail.com", "trrf2012") as ICredentialsByHost;
        smtpServer.EnableSsl = true;
        ServicePointManager.ServerCertificateValidationCallback =
            delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
            { return true; };
        string userState = "feedback message";
        smtpServer.SendCompleted += new SendCompletedEventHandler(SendCompletedCallback);
        smtpServer.SendAsync(mail, userState);

        yield return null;
    }

    private static void SendCompletedCallback(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
    {
        // Get the unique identifier for this asynchronous operation.
        String token = (string)e.UserState;

        if (e.Cancelled)
        {
            ChatManager.Msg("error sending mail: [" + token+"] Send canceled.");
        }
        if (e.Error != null)
        {
            ChatManager.Msg("error sending mail: ["+ token+"] {"+ e.Error.ToString()+"}");
        }
        else
        {
            ChatManager.Msg("Rückmeldung erfolgreich abgeschickt");
        }
    }

    public void TaskResRequest(PersonScript ps)
    {
        taskResRequest.Enqueue(ps);
        ShowMenu(11);
    }

    private void AddJob(int id)
    {
        Job job = Job.Get(id);
        Transform jobItem = Instantiate(jobItemPrefab, jobOverviewContent).transform;
        jobItem.Find("TextName").GetComponent<Text>().text = job.name;
        jobItem.Find("TextCount").GetComponent<Text>().text = "0/"+myVillage.MaxPeopleJob()[id];
        jobItem.Find("Image").GetComponent<Image>().sprite = job.icon;
        jobItem.Find("ButtonAdd").GetComponent<Button>().onClick.AddListener(() => OnAddPersonToJob(id));
        jobItem.Find("ButtonSub").GetComponent<Button>().onClick.AddListener(() => OnTakeJobFromPerson(id));
    }
    private void OnAddPersonToJob(int jobId)
    {
        int max = myVillage.MaxPeopleJob()[jobId];
        if(max != -1 && myVillage.JobEmployedCount()[jobId] >= max) return;
        BuildingScript employerBuilding = null;
        foreach(BuildingScript bs in BuildingScript.allBuildingScripts)
        {
            if(bs.WorkingPeople.Count < bs.Workspace)
            {
                employerBuilding = bs;
                break;
            }
        }

        foreach (PersonScript ps in PersonScript.allPeople)
        {
            if(!ps.IsEmployed())
            {
                ps.job = Job.Get(jobId);
                if(employerBuilding != null)
                {
                    if(employerBuilding.Employ(ps))
                        ps.workingBuilding = employerBuilding;
                }
                return;
            }
        }
    }
    private void OnTakeJobFromPerson(int jobId)
    {
        foreach(PersonScript ps in PersonScript.allPeople)
        {
            if(ps.job.id == jobId)
            {
                ps.UnEmploy();
                return;
            }
        }
    }
    private void OnPersonJob()
    {
        if(PersonScript.selectedPeople.Count == 0) return;

        PersonScript ps = new List<PersonScript>(PersonScript.selectedPeople)[0];
        if(ps.job.type == JobType.Unemployed)
            ShowMenu(1);
        else ps.UnEmploy();
    }

    public List<GameResources> GetStoredRes(BuildingScript bs)
    {
        List<GameResources> storedRes = new List<GameResources>();
        foreach (GameResources tot in bs.Storage)
        {
            if (!ResourceData.IsUnlocked(tot.Id)) continue;
            int amount = 0;
            foreach (GameResources res in bs.StorageCurrent)
                if (res.Id == tot.Id)
                    amount += res.Amount;
            storedRes.Add(new GameResources(tot.Id, amount));
        }
        return storedRes;
    }

    public BuildingScript GetSelectedBuilding()
    {
        if (selectedObject != null && selectedObject.GetComponent<BuildingScript>() != null && (objectInfoShownSmall || objectInfoShown))
            return selectedObject.GetComponent<BuildingScript>();
        return null;
    }

    private bool CanBuildBuilding()
    {
        if(!BuildManager.placingBuilding.multipleBuildings)
        {
            foreach(BuildingScript b in BuildingScript.allBuildingScripts)
            {
                if(b.Id == BuildManager.placingBuilding.id)
                    return false;
            }
        }
        return true;
    }

    public bool InMenu()
    {
        return inMenu != 0;
    }
    /*public int GetPlacingBuilding()
    {
        return buildingMenuSelectedID;
    }*/
    /*public bool InMenu()
    {
        return inMenu != 0 && inMenu != 6 && inMenu != 7;
    }*/
}
