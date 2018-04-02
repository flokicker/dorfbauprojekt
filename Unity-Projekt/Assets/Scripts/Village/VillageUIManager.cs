using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class VillageUIManager : Singleton<VillageUIManager>
{
    [SerializeField]
    private Village myVillage;

    [SerializeField]
    private Transform canvas;

    private Text topPopulationTot, topCoinsText;
    private Image topCoinsImage;
    [SerializeField]
    private Sprite[] coinSprites = new Sprite[3];
    [SerializeField]
    private List<Sprite> resourceSprites = new List<Sprite>();
    private Transform topResourcesParent;
    [SerializeField]
    private GameObject topResourcePrefab;
    [SerializeField]
    private GameObject buildingBuildImagePrefab, buildResourceImagePrefab, buildResourceTextPrefab;
    [SerializeField]
    private List<Sprite> buildingIcons = new List<Sprite>();

    private Transform populationTabs, panelCoins, panelResources, panelGrowth, panelBuild, panelBuildingInfo, 
        panelObjectInfo, panelPeopleInfo, panelSinglePersonInfo, panelPeopleInfo6, panelPeopleInfo7, panelObjectInfoSmall, panelTutorial, panelSettings, panelDebug;

    private Text jobOverviewTotalText, jobOverviewBusyText, jobOverviewFreeText;
    private Transform jobOverviewContent, populationListContent;
    [SerializeField]
    private GameObject jobItemPrefab, populationListItemPrefab, resourcePrefab;
    [SerializeField]
    private List<Sprite> jobIcons;
    private List<Transform> currentJobList = new List<Transform>();

    private Text buildingInfoName, buildingInfoDesc, buildingInfoStage;
    private Transform buildingInfoContent;

    private RectTransform gfactorSlider;
    private Text gfactorTot, gfactorRoomText, gfactorFoodText, gfactorHealthText, gfactorFertilityText, gfactorLuxuryText;

    private Text yearText;

    private Text buildName, buildDescription, buildSize, buildCost, buildWorkspace, buildPopulationRoom;
    private Button buildButton;
    private Transform buildImageListParent, buildResourceParent;

    [SerializeField]
    private List<Sprite> treeIcons, rockIcons, itemIcons;
    [SerializeField]
    private Sprite campfireIcon;
    private Text objectInfoTitle, objectInfoText, objectInfoSmallTitle;
    private Image objectInfoImage;

    private Text personInfoName, personInfo, personInventoryText, peopleInfo7;
    private Image personInfoHealthbar, personInventoryImage;

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

        for (int i = 0; i < Job.JobCount() - 1; i++)
        {
            if(Job.GetJob(i).IsUnlocked())
            AddJob(i + 1);
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
        buildImageListParent = panelBuild.Find("Content").Find("BuildingPreview");
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
        personInfoName = panelSinglePersonInfo.Find("TextName").GetComponent<Text>();
        //personInfoGender = panelPersonInfo.Find("TextGender").GetComponent<Text>();
        //personInfoAge = panelPersonInfo.Find("TextAge").GetComponent<Text>();
        personInfo = panelSinglePersonInfo.Find("TextInfo").GetComponent<Text>();
        personInventoryText = panelSinglePersonInfo.Find("Inventory").Find("Text").GetComponent<Text>();
        personInventoryImage = panelSinglePersonInfo.Find("Inventory").Find("Image").GetComponent<Image>();
        personInfoHealthbar = panelSinglePersonInfo.Find("Health").Find("ImageHP").GetComponent<Image>();

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
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject() && GetBuildingMode() == -1)
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
        for (int i = 0; i < list.Count; i++)
        {
            topResourcesParent.GetChild(i).Find("Image").GetComponent<Image>().sprite = resourceSprites[list[i].GetID()];
            topResourcesParent.GetChild(i).Find("Text").GetComponent<Text>().text = myVillage.GetResources(list[i].GetID()).GetAmount().ToString();
        }
        topResourcesParent.gameObject.SetActive(Building.GetBuilding(3).IsUnlocked());

        yearText.text = myVillage.GetYear() +" n.Chr.";
    }
    private void UpdateJobOverview()
    {
        int totalPeople = myVillage.PeopleCount();
        int employedPeople = myVillage.EmployedPeopleCount();
        int[] jobemplyoedPeople = myVillage.JobEmployedCount();

        jobOverviewTotalText.text = "Bewohner insgesamt: " + totalPeople + " (100%)";
        int percBusy = 0;
        if (totalPeople > 0) percBusy = (100 * employedPeople / totalPeople);
        jobOverviewBusyText.text = "Berufstätige Bewohner: " + employedPeople + " (" + percBusy + "%)";
        jobOverviewFreeText.text = "Freie Bewohner: " + (totalPeople - employedPeople) + " (" + (100 - percBusy) + "%)";

        for (int i = 0; i < currentJobList.Count; i++)
        {
            currentJobList[i].Find("TextCount").GetComponent<Text>().text = jobemplyoedPeople[i].ToString();
        }
    }
    private void UpdatePopulationList()
    {
        if (populationListContent.Find("Nr").childCount - 1 != myVillage.PeopleCount())
        {
            for (int i = 0; i < populationListContent.Find("Nr").childCount - 1; i++)
            {
                Destroy(populationListContent.Find("Nr").GetChild(1+i).gameObject);
                Destroy(populationListContent.Find("FirstName").GetChild(1+i).gameObject);
                Destroy(populationListContent.Find("LastName").GetChild(1+i).gameObject);
                Destroy(populationListContent.Find("Gender").GetChild(1+i).gameObject);
                Destroy(populationListContent.Find("Age").GetChild(1+i).gameObject);
                Destroy(populationListContent.Find("Job").GetChild(1+i).gameObject);
            }
            foreach (Person p in myVillage.GetPeople())
            {
                AddPersonToUI(p);
            }
        }
        else
        {
            int i = 0;
            foreach (Person p in myVillage.GetPeople())
            {
                populationListContent.Find("Nr").GetChild(1 + i).GetComponent<Text>().text = p.GetNr().ToString();
                populationListContent.Find("FirstName").GetChild(1 + i).GetComponent<Text>().text = p.GetFirstName();
                populationListContent.Find("LastName").GetChild(1 + i).GetComponent<Text>().text = p.GetLastName();
                populationListContent.Find("Gender").GetChild(1 + i).GetComponent<Text>().text = (p.GetGender() == Gender.Male ? "M" : "W");
                populationListContent.Find("Age").GetChild(1 + i).GetComponent<Text>().text = p.GetAge().ToString();
                populationListContent.Find("Job").GetChild(1 + i).GetComponent<Text>().text = p.GetJob().GetName();
                i++;
            }
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
        for (int i = 0; i < allResources.Count; i++)
        {
            content.GetChild(i).Find("Image").GetComponent<Image>().sprite = resourceSprites[allResources[i].GetID()];
            content.GetChild(i).Find("Text").GetComponent<Text>().text = myVillage.GetResources(allResources[i].GetID()).GetAmount().ToString();
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
        bool canPurchase = true;
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

                for(int i = 0; i < PersonScript.selectedPeople.Count; i++)
                {
                    int k = i;
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
            infoText += "Beruf: " + p.GetJob().GetName() + "\n";
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
            maxWidth = panelSinglePersonInfo.Find("Health").Find("ImageHPBack").GetComponent<RectTransform>().rect.width - 4;
            personInfoHealthbar.rectTransform.offsetMax = new Vector2(-(2+ maxWidth * (1f-ps.GetHealthFactor())),-2);
            personInfoHealthbar.color = ps.GetConditionCol();

            peopleInfo7.text = PersonScript.selectedPeople.Count+" Bewohner ausgewählt";

            int invAmount = 0;
            GameResources inv = p.GetInventory();
            if (inv != null)
            {
                personInventoryImage.sprite = resourceSprites[inv.GetID()];
                invAmount = inv.GetAmount();
            }
            personInventoryImage.gameObject.SetActive(inv != null);
            personInventoryText.text = invAmount + "/" + p.GetInventorySize() +" kg";
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
                case PlantType.Reed:
                    objectInfoImage.sprite = null;
                    objectInfoText.text = "Hier findest du Fische";
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
            GameResources inv = ps.GetPerson().GetInventory();
            if(inv != null)
            {
                text += "InvName: "+inv.GetName() + "\n";
                text += "InvAmount: "+inv.GetAmount() + "\n";
                text += "InvType: "+inv.GetResourceType() + "\n";
                text += "InvNutr: "+inv.GetNutrition() + "\n";
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
        PersonScript ps = new List<PersonScript>(PersonScript.selectedPeople)[i];
        PersonScript.DeselectAll();
        ps.OnClick();
        CameraController.ZoomSelectedPeople();
    }

    public static void AddPerson(Person p)
    {
        VillageUIManager.Instance.AddPersonToUI(p);
    }
    private void AddPersonToUI(Person p)
    {
        AddPersonToUI(p.GetNr(), p.GetFirstName(), p.GetLastName(), p.GetGender(), p.GetAge(), p.GetJob());
    }
    private void AddPersonToUI(int nr, string firstName, string lastName, Gender gender, int age, Job job)
    {
        Instantiate(populationListItemPrefab, populationListContent.Find("Nr")).GetComponent<Text>().text = nr.ToString();
        Instantiate(populationListItemPrefab, populationListContent.Find("FirstName")).GetComponent<Text>().text = firstName;
        Instantiate(populationListItemPrefab, populationListContent.Find("LastName")).GetComponent<Text>().text = lastName;
        Instantiate(populationListItemPrefab, populationListContent.Find("Gender")).GetComponent<Text>().text = (gender == Gender.Male ? "M" : "W");
        Instantiate(populationListItemPrefab, populationListContent.Find("Age")).GetComponent<Text>().text = age.ToString();
        Instantiate(populationListItemPrefab, populationListContent.Find("Job")).GetComponent<Text>().text = job.GetName();
    }
    private void AddJob(int id)
    {
        Job job = Job.GetJob(id);
        Transform jobItem = Instantiate(jobItemPrefab, jobOverviewContent).transform;
        jobItem.Find("TextName").GetComponent<Text>().text = job.GetName();
        jobItem.Find("TextCount").GetComponent<Text>().text = "0";
        jobItem.Find("Image").GetComponent<Image>().sprite = jobIcons[id-1];
        currentJobList.Add(jobItem);
    }

    public BuildingScript GetSelectedBuilding()
    {
        if (selectedObject != null && selectedObject.GetComponent<BuildingScript>() != null)
            return selectedObject.GetComponent<BuildingScript>();
        return null;
    }

    public void ExitBuildingMode()
    {
        inMenu = 0;
        populationTabs.gameObject.SetActive(false);
        panelCoins.gameObject.SetActive(false);
        panelResources.gameObject.SetActive(false);
        panelGrowth.gameObject.SetActive(false);
        panelBuild.gameObject.SetActive(false);
    }
    public int GetBuildingMode()
    {
        return (inMenu == 6 ? 0 : -1);
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
