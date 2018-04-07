using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class VillageUIManager : Singleton<VillageUIManager>
{
    private Village myVillage;

    [SerializeField]
    private Transform canvas;

    private Text topPopulationTot, topCoinsText;
    private Image topCoinsImage;
    [SerializeField]
    private Sprite[] coinSprites = new Sprite[3];
    [SerializeField]
    public List<Sprite> resourceSprites = new List<Sprite>();
    private Transform topResourcesParent;
    [SerializeField]
    private GameObject topResourcePrefab;
    [SerializeField]
    private GameObject buildingBuildImagePrefab, buildResourceImagePrefab, buildResourceTextPrefab;
    [SerializeField]
    private List<Sprite> buildingIcons = new List<Sprite>();

    private Transform populationTabs, panelCoins, panelResources, panelGrowth, panelBuild, panelBuildingInfo, panelTaskResource,
        panelObjectInfo, panelPeopleInfo, panelSinglePersonInfo, panelPeopleInfo6, panelPeopleInfo7, panelObjectInfoSmall, panelTutorial, panelSettings, panelDebug;

    private Text jobOverviewTotalText, jobOverviewBusyText, jobOverviewFreeText;
    private Transform jobOverviewContent, populationListContent;
    [SerializeField]
    private GameObject jobItemPrefab, populationListItemPrefab, resourcePrefab;
    [SerializeField]
    private List<Sprite> jobIcons;

    private Text buildingInfoName, buildingInfoDesc, buildingInfoStage;
    private Transform buildingInfoContent;

    private RectTransform gfactorSlider;
    private Text gfactorTot, gfactorRoomText, gfactorFoodText, gfactorHealthText, gfactorFertilityText, gfactorLuxuryText;

    private Text yearText;

    private Text buildName, buildDescription, buildSize, buildCost, buildWorkspace, buildPopulationRoom;
    private Button buildButton;
    private Transform buildImageListParent, buildResourceParent;

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

    [SerializeField]
    private List<Sprite> treeIcons, rockIcons, itemIcons;
    [SerializeField]
    private Sprite campfireIcon;
    private Text objectInfoTitle, objectInfoText, objectInfoSmallTitle;
    private Image objectInfoImage;

    private Text personInfoName, personInfo, personInventoryMatText, personInventoryFoodText, peopleInfo7;
    private Image personImage, personInfoHealthbar, personInfoFoodbar, personInventoryMatImage, personInventoryFoodImage;

    [SerializeField]
    private GameObject personInfoPrefab;

    private Toggle settingsInvertMousewheel;

    private Text infoMessage, debugText;

    private int inMenu = 0;
    public bool objectInfoShown, objectInfoShownSmall, personInfoShown;
    private Transform selectedObject;
    //private int buildingMenuSelectedID = 1;
    private Building buildingMenuSelected = null;

	void Start () 
    {
        myVillage = GameManager.village;

        SetupReferences();

        //inMenu = 8;
        //panelTutorial.gameObject.SetActive(true);

        OnPopulationTab(1);
	}

    private void SetupReferences()
    {
        Transform topBar = canvas.Find("TopBar");
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

        for (int i = 1; i < Job.COUNT; i++)
        {
            if(Job.IsUnlocked(i))
                AddJob(i);
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

        for (int i = 1; i < Building.BuildingCount(); i++)
        {
            if (!Building.GetBuilding(i).IsUnlocked()) continue;
            int c = i;
            GameObject obj = (GameObject)Instantiate(buildingBuildImagePrefab, buildImageListParent);
            obj.GetComponent<Image>().sprite = buildingIcons[i];
            obj.GetComponent<Button>().onClick.AddListener(() => OnSelectBuilding(c));
        }

        panelTaskResource = canvas.Find("PanelTaskResource");
        Transform resourcesParent = panelTaskResource.Find("Content").Find("Resources");
        taskResInventory = resourcesParent.Find("Inventory").Find("InventoryRes");
        taskResStorage = resourcesParent.Find("Storage").Find("StorageRes");
        
        for(int i = 0; i < taskResInventory.childCount; i++)
        {
            int j = i;
            Image resImg = taskResInventory.GetChild(i).Find("Image").GetComponent<Image>();
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

        infoMessage = canvas.Find("PanelInfoMessage").Find("Text").GetComponent<Text>();

        panelObjectInfo = canvas.Find("PanelObjectInfo");
        panelObjectInfoSmall = canvas.Find("PanelObjectInfoSmall");
        objectInfoSmallTitle = panelObjectInfoSmall.Find("Title").GetComponent<Text>();
        objectInfoSmallTitle.text = "Objekt";
        objectInfoTitle = panelObjectInfo.Find("Title").GetComponent<Text>();
        objectInfoText = panelObjectInfo.Find("Text").GetComponent<Text>();
        objectInfoImage = panelObjectInfo.Find("Image").GetComponent<Image>();

        panelBuildingInfo = canvas.Find("PanelBuilding");
        buildingInfoName = panelBuildingInfo.Find("Title").GetComponent<Text>();
        buildingInfoDesc = panelBuildingInfo.Find("Current").Find("TextDesc").GetComponent<Text>();
        buildingInfoStage = panelBuildingInfo.Find("Current").Find("TextStage").GetComponent<Text>();
        buildingInfoContent = panelBuildingInfo.Find("Content");

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

        panelPeopleInfo6 = panelPeopleInfo.Find("PanelPeople6");
        panelPeopleInfo7 = panelPeopleInfo.Find("PanelPeople7");
        peopleInfo7 = panelPeopleInfo7.Find("TextName").GetComponent<Text>();

        panelTutorial = canvas.Find("PanelHelp");
        
        panelDebug = canvas.Find("PanelDebug");
        debugText = panelDebug.Find("Content").Find("Text").GetComponent<Text>();

        panelSettings = canvas.Find("PanelSettings");
        settingsInvertMousewheel = panelSettings.Find("Content").Find("ToggleMousewheel").GetComponent<Toggle>();
        settingsInvertMousewheel.isOn = PlayerPrefs.GetInt("InvertedMousewheel") == 1;
        settingsInvertMousewheel.onValueChanged.AddListener(OnToggleInvertedMousewheel);
    }

	void Update ()
    {
        if (PersonScript.selectedPeople.Count > 0 && !personInfoShown)
        {
            ShowPersonInfo(true);
        }
        else if(personInfoShown && PersonScript.selectedPeople.Count == 0)
        {
            ShowPersonInfo(false);
        }
        if (!InputManager.InputUI())
        {
            objectInfoShown = false;
            panelObjectInfo.gameObject.SetActive(false);
            panelObjectInfoSmall.gameObject.SetActive(false);
            personInfoShown = false;
            panelPeopleInfo.gameObject.SetActive(false);
        }
        else if (Input.GetKeyDown(KeyCode.H))
        {
            inMenu = 10;
            panelTutorial.gameObject.SetActive(true);
        }
        else if (Input.GetKeyDown(KeyCode.P))
        {
            inMenu = 11;
            panelDebug.gameObject.SetActive(true);
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            /*if (objectInfoShown)
            {
                panelObjectInfo.gameObject.SetActive(false); 
                objectInfoShown = false;
            }*/
            if(objectInfoShown)
            {
                panelObjectInfo.gameObject.SetActive(false);
                objectInfoShown = false;
            }
            ExitMenu();
        }

        infoMessage.text = myVillage.GetMostRecentMessage();
        
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
                        panelObjectInfo.gameObject.SetActive(false);
                        objectInfoShown = false;
                    }
                    if(objectInfoShownSmall)
                    {
                        panelObjectInfoSmall.gameObject.SetActive(false);
                        objectInfoShownSmall = false;
                    }
                }
            }
        }

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
        UpdateDebugPanel();
    }

    public void ExitMenu()
    {
        /*if (personInfoShown)
        {
            panelPersonInfo.gameObject.SetActive(false);
            personInfoShown = false;
        }*/
        if (!InputManager.InputUI())
        {
            // maxmenu=11
            inMenu = 0;
            populationTabs.gameObject.SetActive(false);
            panelCoins.gameObject.SetActive(false);
            panelResources.gameObject.SetActive(false);
            panelGrowth.gameObject.SetActive(false);
            panelBuild.gameObject.SetActive(false);
            panelBuildingInfo.gameObject.SetActive(false);
            panelTutorial.gameObject.SetActive(false);
            panelSettings.gameObject.SetActive(false);
            panelDebug.gameObject.SetActive(false);
            panelTaskResource.gameObject.SetActive(false);
            taskResRequest.Clear();
        }
    }

    private void UpdateTopPanels()
    {
        topPopulationTot.text = "Bewohner: " + myVillage.PeopleCount().ToString();
        topCoinsText.text = myVillage.GetCoinString();
        topCoinsImage.sprite = coinSprites[myVillage.GetCoinUnit()];
        List<GameResources> list = GameManager.GetGameSettings().GetFeaturedResources();
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
        int[] totResourceCount = myVillage.GetTotalResourceCount();
        for (int i = 0; i < list.Count; i++)
        {
            topResourcesParent.GetChild(i).Find("Image").GetComponent<Image>().sprite = resourceSprites[list[i].GetID()];
            topResourcesParent.GetChild(i).Find("Text").GetComponent<Text>().text = totResourceCount[list[i].GetID()].ToString();
        }
        topResourcesParent.gameObject.SetActive(Building.GetBuilding(3).IsUnlocked());

        yearText.text = myVillage.GetTwoSeasonStr() +"\nJahr "+myVillage.GetYear();
    }
    private void UpdateJobOverview()
    {
        int totalPeople = myVillage.PeopleCount();
        int employedPeople = myVillage.EmployedPeopleCount();
        int[] jobemployedPeople = myVillage.JobEmployedCount();

        jobOverviewTotalText.text = "Bewohner insgesamt: " + totalPeople + " (100%)";
        int percBusy = 0;
        if (totalPeople > 0) percBusy = (100 * employedPeople / totalPeople);
        jobOverviewBusyText.text = "Berufstätige Bewohner: " + employedPeople + " (" + percBusy + "%)";
        jobOverviewFreeText.text = "Freie Bewohner: " + (totalPeople - employedPeople) + " (" + (100 - percBusy) + "%)";

        List<Job> unlockedJobs = new List<Job>();
        int[] maxPeople = myVillage.MaxPeopleJob();
        for (int i = 1; i < Job.COUNT; i++)
        {
            if (Job.IsUnlocked(i)) unlockedJobs.Add(new Job(i));
        }
        if (unlockedJobs.Count != jobOverviewContent.childCount)
        {
            for (int i = 0; i < jobOverviewContent.childCount; i++)
                Destroy(jobOverviewContent.GetChild(i).gameObject);

            for (int i = 1; i < Job.COUNT; i++)
            {
                if (!Job.IsUnlocked(i)) continue;
                AddJob(i);
            }
        }
        else
        {
            for (int i = 0; i < jobOverviewContent.childCount; i++)
            {
                int ji = unlockedJobs[i].id;
                int mp = maxPeople[ji];
                string txt = jobemployedPeople[ji].ToString();
                if(mp != -1) txt +=  "/"+ mp;

                jobOverviewContent.GetChild(i).Find("TextCount").GetComponent<Text>().text = txt;
            }
        }
    }
    private void UpdatePopulationList()
    {
        int i = 0;
        if (populationListContent.childCount - 1 != myVillage.PeopleCount())
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
        foreach (Person p in myVillage.GetPeople())
        {
            Transform listItem = populationListContent.GetChild(i+1);
            listItem.GetChild(0).GetComponent<Text>().text = p.nr.ToString();
            listItem.GetChild(1).GetComponent<Text>().text = p.firstName;
            listItem.GetChild(2).GetComponent<Text>().text = p.lastName;
            listItem.GetChild(3).GetComponent<Text>().text = (p.gender == Gender.Male ? "M" : "W");
            listItem.GetChild(4).GetComponent<Text>().text = p.age.ToString();
            listItem.GetChild(5).GetComponent<Text>().text = p.job.jobName;
            i++;
        }
    }
    private void UpdateResourcesPanel()
    {
        Transform content = panelResources.Find("Content");
        if (content.childCount != GameResources.AvailableResourceCount())
        {
            for (int i = 0; i < content.childCount; i++)
                Destroy(content.GetChild(i).gameObject);

            for (int j = 0; j < GameResources.AvailableResourceCount(); j++)
            {
                int i = j;
                GameObject obj = Instantiate(resourcePrefab, content);
                obj.transform.Find("Toggle").GetComponent<Toggle>().onValueChanged.AddListener((b) => OnResourceToggle(b,i));
            }
        }
        List<GameResources> allResources = GameResources.GetAvailableResources();
        int[] totResourceCount = myVillage.GetTotalResourceCount();
        for (int i = 0; i < allResources.Count; i++)
        {
            content.GetChild(i).Find("Image").GetComponent<Image>().sprite = resourceSprites[allResources[i].GetID()];
            content.GetChild(i).Find("Text").GetComponent<Text>().text = totResourceCount[allResources[i].GetID()].ToString();
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
        int[] matC;
        if (buildingMenuSelected == null || buildingMenuSelected.GetID() != BuildManager.placingBuildingID)
        {
            buildingMenuSelected = new Building(BuildManager.placingBuildingID);
            matC = buildingMenuSelected.GetAllMaterialCost();
            foreach (Transform child in buildResourceParent)
            {
                GameObject.Destroy(child.gameObject);
            }

            for (int i = 0; i < matC.Length; i++)
            {
                if (matC[i] == 0) continue;

                GameObject obj = (GameObject)Instantiate(buildResourceImagePrefab, buildResourceParent);
                obj.GetComponent<Image>().sprite = resourceSprites[i]; 
                obj = (GameObject)Instantiate(buildResourceTextPrefab, buildResourceParent);
                obj.GetComponent<Text>().text = matC[i].ToString();
            }
        }
        int unlockedBC = 0;
        for (int i = 1; i < Building.BuildingCount(); i++)
        {
            if (Building.GetBuilding(i).IsUnlocked()) unlockedBC++;
        }
        if (unlockedBC != buildImageListParent.childCount)
        {
            for (int i = 0; i < buildImageListParent.childCount; i++)
                Destroy(buildImageListParent.GetChild(i).gameObject);

            for (int i = 1; i < Building.BuildingCount(); i++)
            {
                if (!Building.GetBuilding(i).IsUnlocked()) continue;
                int c = i;
                GameObject obj = (GameObject)Instantiate(buildingBuildImagePrefab, buildImageListParent);
                obj.GetComponent<Image>().sprite = buildingIcons[i];
                obj.GetComponent<Button>().onClick.AddListener(() => OnSelectBuilding(c));
            }
        }

        matC = buildingMenuSelected.GetAllMaterialCost();
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

        buildName.text = buildingMenuSelected.GetName();
        buildDescription.text = buildingMenuSelected.GetDescription();
        buildSize.text = buildingMenuSelected.GetGridWidth() + ":" + buildingMenuSelected.GetGridHeight();

        b = myVillage.GetCoins() >= buildingMenuSelected.GetCost();
        tcol = b ? Color.black : Color.red;
        if (!b) canPurchase = false;

        buildCost.text = buildingMenuSelected.GetCost().ToString();
        buildCost.color = tcol;
        buildWorkspace.text = buildingMenuSelected.GetWorkspace().ToString();
        buildPopulationRoom.text = buildingMenuSelected.GetPopulationRoom().ToString();

        buildButton.enabled = canPurchase;
    } 
    private void UpdateTaskResPanel()
    {
        // only update if if a building is selected and person-queue is not empty
        if(taskResRequest.Count == 0) return;
        if(!selectedObject || !selectedObject.GetComponent<BuildingScript>()) return;

        // get all references to building/people scripts
        PersonScript ps = taskResRequest.Peek();
        Person p = ps.GetPerson();
        BuildingScript bs = selectedObject.GetComponent<BuildingScript>();
        Building b = bs.GetBuilding();

        // variables used multiple times
        int invAmount = 0;
        GameResources inv = null;
        bool showAmBut = true;

        // update inventory (0=material,1=food)
        for(int i = 0; i < taskResInventory.childCount; i++)
        {
            invAmount = 0;

            if(i == 0) inv = p.inventoryMaterial;
            else inv = p.inventoryFood;

            Image resImg = taskResInventory.GetChild(i).Find("Image").GetComponent<Image>();
            Text resTxt = taskResInventory.GetChild(i).Find("Text").GetComponent<Text>();
            if (inv != null)
            {
                if(i == taskResInvSelected)
                {
                    taskResInvMax = Mathf.Min(inv.amount, b.resourceStorage[inv.id]-b.resourceCurrent[inv.id]);
                    taskResInvSlider.maxValue = taskResInvMax;
                    int input = int.Parse(taskResInvInput.text);
                    input = Mathf.Clamp(input, 1, taskResInvMax);
                    
                    taskResInvImage.sprite = resourceSprites[inv.id];
                    if(inv.amount == 0) showAmBut = false;         
                }

                invAmount = inv.amount;
                resImg.sprite = resourceSprites[inv.id];
                resImg.color = Color.white;
            }
            else if(i == taskResInvSelected)
            {
                showAmBut = false;
            }
            resTxt.text = invAmount + "/" + (i == 0 ? p.GetMaterialInventorySize() : p.GetFoodInventorySize());
            if(invAmount == 0) resImg.color = new Color(1,1,1,0.1f);

            resImg.GetComponent<Button>().enabled = invAmount > 0;
        }

        taskResInvAm.gameObject.SetActive(showAmBut);
        taskResInvBut.gameObject.SetActive(showAmBut);

        List<GameResources> storedRes = GetStoredRes(b);

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
            }
        }

        if(taskResStorSelected >= taskResStorage.childCount) taskResStorSelected = 0;

        for(int i = taskResStorage.childCount-storedRes.Count; i < taskResStorage.childCount; i++)
        {
            Image resImg = taskResStorage.GetChild(i).Find("Image").GetComponent<Image>();
            Text resTxt = taskResStorage.GetChild(i).Find("Text").GetComponent<Text>();
            inv = storedRes[i - (taskResStorage.childCount-storedRes.Count)];
            if (inv != null)
            {
                if(i == taskResStorSelected)
                {
                    taskResStorMax = Mathf.Min(inv.amount, inv.GetResourceType() == ResourceType.BuildingMaterial ? p.GetFreeMaterialInventorySpace() : p.GetFreeFoodInventorySpace());
                    taskResStorSlider.maxValue = taskResStorMax;
                    int input = int.Parse(taskResStorInput.text);
                    input = Mathf.Clamp(input, 1, taskResStorMax);

                    taskResStorImage.sprite = resourceSprites[inv.id];
                    if(inv.amount == 0) showAmBut = false;         
                }

                invAmount = inv.amount;
                resImg.sprite = resourceSprites[inv.id];
                resImg.color = Color.white;
            }
            else if(i == taskResInvSelected)
            {
                showAmBut = false;
            }
            resTxt.text = invAmount + "/" + b.resourceStorage[inv.id];
            if(invAmount == 0) resImg.color = new Color(1,1,1,0.1f);

            resImg.GetComponent<Button>().enabled = invAmount > 0;
        }

        if(storedRes.Count == 0) showAmBut = false;

        taskResStorAm.gameObject.SetActive(showAmBut);
        taskResStorBut.gameObject.SetActive(showAmBut);
    }
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
                    int k = selectedPerson.ID;
                    GameObject obj = (GameObject)Instantiate(personInfoPrefab, Vector3.zero, Quaternion.identity, panelPeopleInfo6);
                    obj.GetComponent<Button>().onClick.AddListener(() => OnPersonSelect(k));
                }
            }
            float maxWidth;
            int index = 0;
            foreach(PersonScript personScript in PersonScript.selectedPeople)
            {
                Transform panel = panelPeopleInfo6.GetChild(index);
                panel.Find("TextName").GetComponent<Text>().text = personScript.GetPerson().GetFirstName();
                maxWidth = panel.Find("Health").Find("ImageHPBack").GetComponent<RectTransform>().rect.width - 4;
                panel.Find("Health").Find("ImageHP").GetComponent<RectTransform>().offsetMax = new Vector2(-(2f+maxWidth*(1f-personScript.GetHealthFactor())),-2);
                panel.Find("Health").Find("ImageHP").GetComponent<Image>().color = personScript.GetConditionCol();
                index++;
            }

            panelSinglePersonInfo.gameObject.SetActive(spc == 0);
            panelPeopleInfo6.gameObject.SetActive(spc == 1);
            panelPeopleInfo7.gameObject.SetActive(spc == 2);

            PersonScript ps = new List<PersonScript>(PersonScript.selectedPeople)[0];
            Person p = ps.GetPerson();

            personInfoName.text = p.GetFirstName() + "\n"+p.GetLastName();
            //personInfoGender.text = "Geschlecht: " + (p.GetGender() == Gender.Male ? "M" : "W");
            //personInfoAge.text = "Alter: " + p.GetAge().ToString();
            string infoText = "";
            if(p.IsEmployed())
                infoText += "Beruf: " + p.GetJob().jobName + "\n";
            string task = "-";
            if(ps.routine.Count > 0)
            {
                Task ct = ps.routine[0];
                if(ct.taskType == TaskType.Walk && ps.routine.Count > 1) ct = ps.routine[1];
                if(ct.taskType == TaskType.CutTree) task = "Holzen";
                if(ct.taskType == TaskType.Fishing) task = "Fischen";
                if(ct.taskType == TaskType.Build) task = "Bauen";
            }
            infoText += "Aufgabe: " + task + "\n";
            infoText += "Zustand: " + ps.GetConditionStr() + "\n";
            personInfo.text = infoText;
            maxWidth = personInfoHealthbar.transform.parent.Find("Back").GetComponent<RectTransform>().rect.width - 4;
            personInfoHealthbar.rectTransform.offsetMax = new Vector2(-(2+ maxWidth * (1f-ps.GetHealthFactor())),-2);
            personInfoHealthbar.color = ps.GetConditionCol();
            maxWidth = personInfoFoodbar.transform.parent.Find("Back").GetComponent<RectTransform>().rect.width - 4;
            personInfoFoodbar.rectTransform.offsetMax = new Vector2(-(2+ maxWidth * (1f-ps.GetFoodFactor())),-2);
            personInfoFoodbar.color = ps.GetFoodCol();

            peopleInfo7.text = PersonScript.selectedPeople.Count+" Bewohner ausgewählt";

            int invAmount = 0;
            GameResources invMat = p.inventoryMaterial;
            GameResources invFood = p.inventoryFood;

            // update material inventory slots
            if (invMat != null)
            {
                invAmount = invMat.GetAmount();
                personInventoryMatImage.sprite = resourceSprites[invMat.GetID()];
                personInventoryMatImage.color = Color.white;
            }
            personInventoryMatText.text = invAmount + "/" + p.GetMaterialInventorySize();
            if(invAmount == 0) personInventoryMatImage.color = new Color(1,1,1,0.1f);
            
            // same for food
            invAmount = 0;
            if (invFood != null)
            {
                invAmount = invFood.GetAmount();
                personInventoryFoodImage.sprite = resourceSprites[invFood.GetID()];
                personInventoryFoodImage.color = Color.white;
            }
            personInventoryFoodText.text = invAmount + "/" + p.GetFoodInventorySize();
            if(invAmount == 0) personInventoryFoodImage.color = new Color(1,1,1,0.1f);

            //personInventoryImage.gameObject.SetActive(inv != null);
            //personInventoryText.text = invAmount + "/" + p.GetInventorySize() +" kg";
        }
    }
    private void UpdateObjectInfoPanel()
    {
        if (selectedObject == null || !selectedObject.gameObject.activeSelf)
        {
            if (objectInfoShown)
            {
                panelObjectInfoSmall.gameObject.SetActive(false);
                panelObjectInfo.gameObject.SetActive(false);
                objectInfoShown = false;
            }
            return;
        }

        Plant plant = selectedObject.GetComponent<Plant>();
        if (plant != null)
        {
            objectInfoTitle.text = plant.GetName();
            objectInfoSmallTitle.text = plant.GetName();
            GameResources plantRes = new GameResources(plant.materialID);
            switch (plant.type)
            {
                case PlantType.Tree:
                    objectInfoImage.sprite = treeIcons[0];
                    objectInfoText.text = "Grösse: " + plant.GetSizeInMeter() + "m\n" + plant.material + "kg Holz";
                    break;
                case PlantType.Rock:
                    objectInfoImage.sprite = rockIcons[0];
                    objectInfoText.text = plant.material + "kg Stein";
                    break;
                case PlantType.Mushroom:
                    objectInfoImage.sprite = null;
                    objectInfoText.text = "Nährwert: "+plantRes.GetNutrition();
                    break;
                case PlantType.MushroomStump:
                    objectInfoImage.sprite = null;
                    objectInfoText.text = "Strunk mit Pilzen\n"+plant.material + " Pilze";
                    break;
                case PlantType.Corn:
                    objectInfoImage.sprite = null;
                    objectInfoText.text = "Kann geerntet werden";
                    break;
                case PlantType.Reed:
                    objectInfoImage.sprite = null;
                    objectInfoText.text = "Hier findest du Fische\n"+plant.material + " Fische";
                    break;
                default:
                    Debug.Log("Unhandled object: " + plant.type.ToString());
                    break;
            }
        }
        BuildingScript bs = selectedObject.GetComponent<BuildingScript>();
        if(bs != null)
        {
            Building b = bs.GetBuilding();
            string name = b.GetName();
            // if (bs.bluePrint) name += " (prov.)";
            objectInfoSmallTitle.text = name;
            objectInfoTitle.text = name;
            objectInfoImage.sprite = buildingIcons[b.GetID()];
            objectInfoText.text = b.GetDescription();
        }
        Campfire cf = selectedObject.GetComponent<Campfire>();
        if(cf != null)
        {
            objectInfoSmallTitle.text = cf.DisplayName;
            objectInfoTitle.text = cf.DisplayName;
            objectInfoImage.sprite = campfireIcon;
            objectInfoText.text = cf.GetHealthPercentage()+"%";
        }
        Item item = selectedObject.GetComponent<Item>();
        if (selectedObject.tag == "Item" && item != null)
        {
            objectInfoSmallTitle.text = item.GetName();
            objectInfoTitle.text = item.GetName();
            objectInfoImage.sprite = itemIcons[0];
            objectInfoText.text = "Kann aufgesammelt werden";
        }
    }
    private void UpdateBuildingInfoPanel()
    {
        if (selectedObject != null)
        {
            BuildingScript bs = selectedObject.GetComponent<BuildingScript>();
            if (bs != null)
            {
                Building b = bs.GetBuilding();
                buildingInfoName.text = b.GetName();
                buildingInfoDesc.text = b.GetDescription();
                buildingInfoStage.text = "Stufe 1";
            }

            /* TODO: individual building info */
            
        }
    }
    public void UpdateSettingsPanel()
    {
        /* maybe needed later */
    }
    public void UpdateDebugPanel()
    {
        List<PersonScript> list = new List<PersonScript>(PersonScript.selectedPeople);
        if(list.Count == 0) return;
        PersonScript ps = list[0];
        
        string text = "";
        text += "Name: "+ps.GetPerson().GetFirstName() + "\n";
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
                Plant plant = ct.targetTransform.GetComponent<Plant>();
                if(plant != null)
                {
                    text += "IsBroken: "+plant.IsBroken() + "\n";
                    text += "MatID: "+plant.materialID + "\n";
                    text += "MatAmount: "+plant.material + "\n";
                }
            }
            GameResources inv = ps.GetPerson().inventoryMaterial;
            if(inv != null)
            {
                text += "InvMatName: "+inv.GetName() + "\n";
                text += "InvMatAmount: "+inv.GetAmount() + "\n";
                text += "InvMatType: "+inv.GetResourceType() + "\n";
                text += "InvMatNutr: "+inv.GetNutrition() + "\n";
            }
            inv = ps.GetPerson().inventoryFood;
            if(inv != null)
            {
                text += "InvFoodName: "+inv.GetName() + "\n";
                text += "InvFoodAmount: "+inv.GetAmount() + "\n";
                text += "InvFoodType: "+inv.GetResourceType() + "\n";
                text += "InvFoodNutr: "+inv.GetNutrition() + "\n";
            }
        }

        debugText.text = text;
    }

    public void OnCloseTutorial()
    {
        inMenu = 0;
        panelTutorial.gameObject.SetActive(false);
    }
    public void OnPopulationTab(int i)
    {
        populationTabs.Find("ListOverviewTab").Find("PanelTab").gameObject.SetActive(i == 0);
        populationTabs.Find("JobOverviewTab").Find("PanelTab").gameObject.SetActive(i == 1);
    }
    private void OnResourceToggle(bool b, int i)
    {
        List<GameResources> allResources = GameResources.GetAvailableResources();
        int id = allResources[i].GetID();
        if (b)
        {
            GameManager.AddFeaturedResourceID(id);
        }
        else
        {
            GameManager.RemoveFeaturedResourceID(id);
        }
    }
    public void OnPopulationButtonClick()
    {
        if (!InputManager.InputUI()) return;

        populationTabs.gameObject.SetActive(true);
        inMenu = 1;
    }
    public void OnCoinButtonClick()
    {
        if (!InputManager.InputUI()) return;

        panelCoins.gameObject.SetActive(true);
        inMenu = 2;
    }
    public void OnResourceButtonClick()
    {
        if (!InputManager.InputUI()) return;

        panelResources.gameObject.SetActive(true);
        inMenu = 3;
    }
    public void OnGrowthButtonClick()
    {
        if (!InputManager.InputUI()) return;

        panelGrowth.gameObject.SetActive(true);
        inMenu = 4;
    }
    public void OnBuildButtonClick()
    {
        /* TODO: do we really want this? */
        PersonScript.DeselectAll();

        if (!InputManager.InputUI()) return;

        panelBuild.gameObject.SetActive(true);
        inMenu = 5;
    }
    public void OnSelectBuilding(int i)
    {
        BuildManager.placingBuildingID = i;
    }
    public void OnPlaceBuildingButtonClick()
    {
        bool canBuy = true;

        if (myVillage.GetCoins() < buildingMenuSelected.GetCost()) canBuy = false;
        if(!CanBuildBuilding()) canBuy = false;
        //if (myVillage.Get < b.populationUse) canBuy = false;
        //for (int j = 0; j < myVillage.GetResourcesCountBuildingMaterial(); j++)
        /*for (int j = 0; j < 3; j++)
            if (myVillage.GetResources(j).GetAmount() < buildingMenuSelected.GetAllMaterialCost()[myVillage.GetResources(j).GetID()]) canBuy = false;*/

        //for (int j = 0; j < myVillage.GetResourcesCount(); j++)
           // myVillage.GetResources(j).Take(buildingMenuSelected.GetAllMaterialCost()[myVillage.GetResources(j).GetID()]);



        if (!canBuy) return;

        panelBuild.gameObject.SetActive(false);
        inMenu = 0;
        BuildManager.StartPlacing();
    }
    public void OnShowObjectInfo(Transform trf)
    {
        if (!InputManager.InputUI()) return;

        objectInfoShown = true;
        selectedObject = trf;

        panelObjectInfoSmall.gameObject.SetActive(false);
        panelObjectInfo.gameObject.SetActive(true);
    }
    public void OnShowSmallObjectInfo(Transform trf)
    {
        if (!InputManager.InputUI()) return;
        if (objectInfoShown) return;

        objectInfoShownSmall = true;
        selectedObject = trf;
        panelObjectInfoSmall.gameObject.SetActive(true);
    }
    public void OnHideObjectInfo()
    {
        panelObjectInfo.gameObject.SetActive(false);
        objectInfoShown = false;
        selectedObject = null;
    }
    public void OnHideSmallObjectInfo()
    {
        panelObjectInfoSmall.gameObject.SetActive(false);
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
        if (!InputManager.InputUI()) return;

        selectedObject = trf;

        panelBuildingInfo.gameObject.SetActive(true);
        inMenu = 8;
    }
    public void OnShowSettings()
    {
        if (!InputManager.InputUI()) return;

        panelSettings.gameObject.SetActive(true);
        inMenu = 9;
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
            res = taskResInvSelected == 0 ? ps.GetPerson().inventoryMaterial : ps.GetPerson().inventoryFood;
            res = res.Clone();
            res.amount = (int)taskResInvSlider.value;
            tt = TaskType.BringToWarehouse;
        }
        else // Warehouse -> Inv
        {
            res = GetStoredRes(bs.GetBuilding())[taskResStorSelected].Clone();
            res.amount = (int)(taskResStorSlider.value);
            tt = TaskType.TakeFromWarehouse;
        }
        if(ps.AddResourceTask(tt, bs,res.Clone())) {
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
    public void OnShowTaskRes()
    {
        if (!InputManager.InputUI()) return;

        panelTaskResource.gameObject.SetActive(true);
        inMenu = 12;
    }

    public void TaskResRequest(PersonScript ps)
    {
        taskResRequest.Enqueue(ps);
        OnShowTaskRes();
    }

    private void AddJob(int id)
    {
        Job job = new Job(id);
        Transform jobItem = Instantiate(jobItemPrefab, jobOverviewContent).transform;
        jobItem.Find("TextName").GetComponent<Text>().text = job.jobName;
        jobItem.Find("TextCount").GetComponent<Text>().text = "0/"+myVillage.MaxPeopleJob()[id];
        jobItem.Find("Image").GetComponent<Image>().sprite = jobIcons[id];
        jobItem.Find("ButtonAdd").GetComponent<Button>().onClick.AddListener(() => OnAddPersonToJob(id));
        jobItem.Find("ButtonSub").GetComponent<Button>().onClick.AddListener(() => OnTakeJobFromPerson(id));
    }
    private void OnAddPersonToJob(int jobId)
    {
        int max = myVillage.MaxPeopleJob()[jobId];
        if(max != -1 && myVillage.JobEmployedCount()[jobId] >= max) return;

        foreach(PersonScript ps in PersonScript.allPeople)
        {
            if(!ps.GetPerson().IsEmployed())
            {
                ps.GetPerson().job = new Job(jobId);
                return;
            }
        }
    }
    private void OnTakeJobFromPerson(int jobId)
    {
        foreach(PersonScript ps in PersonScript.allPeople)
        {
            if(ps.GetPerson().job.id == jobId)
            {
                ps.GetPerson().job = new Job(Job.UNEMPLOYED);
                return;
            }
        }
    }

    public List<GameResources> GetStoredRes(Building b)
    {
        List<GameResources> storedRes = new List<GameResources>();
        for(int i = 0; i < b.resourceStorage.Length; i++)
            if(b.resourceStorage[i] > 0)
            {
                if(GameResources.IsUnlocked(i)) 
                    storedRes.Add(new GameResources(i,b.resourceCurrent[i]));
            }
        return storedRes;
    }

    public BuildingScript GetSelectedBuilding()
    {
        if (selectedObject != null && selectedObject.GetComponent<BuildingScript>() != null)
            return selectedObject.GetComponent<BuildingScript>();
        return null;
    }

    private bool CanBuildBuilding()
    {
        if(!buildingMenuSelected.multipleBuildings)
        {
            foreach(BuildingScript b in BuildingScript.allBuildings)
            {
                if(b.GetBuilding().id == buildingMenuSelected.id)
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
