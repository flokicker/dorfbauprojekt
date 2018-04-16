using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Village : MonoBehaviour {

    public Nature nature;

    private int coins;

    // Nahrungsvielfalt, Wohnraum, Gesundheit, Fruchtbarkeit, Luxus
    private float foodFactor, roomspaceFactor, healthFactor, fertilityFactor, luxuryFactor;
    private float totalFactor;

    private float growthTime = 0, deathTime = 0;

    private bool isSetup;

    void Start()
    {
        isSetup = false;
        coins = 2500;

        totalFactor = 0;

        foodFactor = 0;
        roomspaceFactor = 20;
        healthFactor = 20;
        fertilityFactor = 0;
        luxuryFactor = 0;
    }
    void Update()
    {
        if(!isSetup)
        {
            nature = GetComponent<Nature>();
            SetupNewVillage();
            return;
        }

        if (PersonScript.allPeople.Count == 0 && !GameManager.gameOver) 
        {
            GameManager.Msg("Game Over!");
            GameManager.gameOver = true;
        }

        //resources[5].Add(1);
        RecalculateFactors();
        UpdatePopulation();

        // Check if any person is dead
        foreach(PersonScript p in PersonScript.allPeople)
        {
            if(p.GetPerson().IsDead())
            {
                PersonDeath(p);
                break;
            }
        }
    }

    public void UpdatePopulation()
    {
        Person p;
        // Time between deaths
        deathTime += Time.deltaTime;
        if (GetTotalFactor() > 50) deathTime = 0;
        int tmp = (1000 + 50 * (GetTotalFactor() + 1));
        if (deathTime >= tmp)
        {
            deathTime -= tmp;
            p = RandomPersonDeath();
        }

        /* TODO: better calculation for growth */
        // Time between births
        if (GetTotalFactor() > 20)
            growthTime += Time.deltaTime;
        tmp = 50000 / (GetTotalFactor() + 1);
        if (growthTime >= tmp)
        {
            growthTime -= tmp;
            p = PersonBirth();
            GameManager.Msg(p.GetFirstName() + " ist gerade geboren!");
        }
    }
    public int EmployedPeopleCount()
    {
        int employedPeople = 0;
        foreach (PersonScript p in PersonScript.allPeople)
            if (p.GetPerson().IsEmployed())
                employedPeople++;
        return employedPeople;
    }
    public int[] JobEmployedCount()
    {
        int[] jobEmployedPeople = new int[Job.COUNT];
        foreach (PersonScript p in PersonScript.allPeople)
            jobEmployedPeople[p.GetPerson().GetJob().id]++;
        return jobEmployedPeople;
    }
    public int[] MaxPeopleJob()
    {
        int[] maxPeople = new int[Job.COUNT];
        foreach(BuildingScript bs in BuildingScript.allBuildings)
        {
            if(!bs.blueprint)
            maxPeople[bs.GetBuilding().jobId] += bs.GetBuilding().workspace;
        }
        
        for(int i = 0; i < maxPeople.Length; i++)
            if(!new Job(i).limited)
                maxPeople[i] = -1;

        return maxPeople;
    }

    public void RecalculateFactors()
    {
        // factor influenced by buildings
        int space = 0;
        luxuryFactor = 0;
        healthFactor = 0;
        foreach (BuildingScript b in BuildingScript.allBuildings)
        {
            if (b.blueprint) continue;
            space += b.GetBuilding().GetPopulationRoom();
            luxuryFactor += b.LuxuryFactor();
            healthFactor += b.HealthFactor();
        }
        if (PersonScript.allPeople.Count == 0) roomspaceFactor = 0;
        else
            roomspaceFactor = (int)(space * 80 / PersonScript.allPeople.Count);

        // Fertility factor
        int fertileMen = 0;
        int fertileWomen = 0;
        foreach (PersonScript p in PersonScript.allPeople)
        {
            if (p.GetPerson().IsFertile())
            {
                if (p.GetPerson().gender == Gender.Male) fertileMen++;
                else fertileWomen++;
            }
        }
        if(fertileMen > 0)
        {
            float tmpfactor = (float)fertileWomen / fertileMen;
            if (tmpfactor > 1) tmpfactor = 1f / tmpfactor;
            fertilityFactor = (int)(100 * tmpfactor);
        }
        else fertilityFactor = 0;

        // Adjust factor to calculated one
        float totFactDiff = totalFactor - GetCalcTotalFactor();
        if (totFactDiff > 0) totalFactor -= Time.deltaTime / 4f;
        if (totFactDiff < 0) totalFactor += Time.deltaTime / 4f;

        totFactDiff = foodFactor - GetCalcFoodFactor();
        if (totFactDiff > 0)
        {
            // bool message = foodFactor > 0;
            foodFactor -= Time.deltaTime / 1f;
            // if (foodFactor <= 5 && message) NewMessage("Deine Bewohner sind hungrig!");
        }
        if (totFactDiff < 0) foodFactor += Time.deltaTime / 1f;

        // Factors reach from 0 to 100
        foodFactor = Mathf.Clamp(foodFactor, 0, 100);
        roomspaceFactor = Mathf.Clamp(roomspaceFactor, 0, 100);
        healthFactor = Mathf.Clamp(healthFactor, 0, 100);
        fertilityFactor = Mathf.Clamp(fertilityFactor, 0, 100);
        luxuryFactor = Mathf.Clamp(luxuryFactor, 0, 100);
        totalFactor = Mathf.Clamp(totalFactor, 0, 100);
    }

    public int GetFoodFactor()
    {
        return Mathf.RoundToInt(foodFactor);
    }
    public int GetRoomspaceFactor()
    {
        return Mathf.RoundToInt(roomspaceFactor);
    }
    public int GetHealthFactor()
    {
        return Mathf.RoundToInt(healthFactor);
    }
    public int GetFertilityFactor()
    {
        return Mathf.RoundToInt(fertilityFactor);
    }
    public int GetLuxuryFactor()
    {
        return Mathf.RoundToInt(luxuryFactor);
    }
    public int GetTotalFactor()
    {
        return Mathf.RoundToInt(totalFactor);
    }
    public int GetCalcTotalFactor()
    {
        /* TODO: include all factors */
        //(foodFactor + roomspaceFactor + healthFactor + fertilityFactor + luxuryFactor) / 5;
        if (foodFactor <= 5 && (foodFactor + roomspaceFactor + fertilityFactor + healthFactor + luxuryFactor) / 5 > foodFactor) return (int)foodFactor;
        return (int)(foodFactor + roomspaceFactor + fertilityFactor + healthFactor + luxuryFactor) / 5;
    }
    public int GetCalcFoodFactor()
    {
        /*float foodUse = 0;
        foreach (Person p in people) foodUse += p.FoodUse();
        //PeopleCount() * 0.01f; // population * foodPerDayPerson
        float totFoodFactor = 0;
        foreach (GameResources r in resources)
        {
            if (r.GetResourceType() == ResourceType.Food && r.GetAmount() > 0)
            {
                float foodUsePerDay = foodUse / r.GetNutrition();
                float am = (float)r.GetAmount() / foodUsePerDay * 0.05f;
                am = Mathf.Clamp(am,0,25);
                totFoodFactor += am;
            }
        }*/
        if(PersonScript.allPeople.Count == 0) return 0;
        float totFoodFactor = 0;
        foreach (PersonScript p in PersonScript.allPeople) totFoodFactor += p.GetPerson().hunger;
        totFoodFactor /= PersonScript.allPeople.Count;
        return (int)totFoodFactor;
    }
    public int GetCoins()
    {
        return coins;
    }
    public string CoinString()
    {
        string s = "";
        return s;
    }
    public int GetBronzeCoins()
    {
        return coins % 100;
    }
    public int GetSilverCoins()
    {
        return (coins / 100) % 100;
    }
    public int GetGoldCoins()
    {
        return coins / 100 / 100;
    }
    public int GetCoinUnit()
    {
        if (coins < 100) return 0;
        if (coins < 100*100) return 1;
        return 2;
    }
    public string GetCoinString()
    {
        int un = GetCoinUnit();
        float value = 0;
        if (un == 0) return GetBronzeCoins().ToString();
        if (un == 1) value = coins / 100f;
        if (un == 2) value = coins / 100f / 100f;
        return value.ToString("F2");
    }

    /*public void Restock(GameResources res)
    {
        for (int i = 0; i < resources.Count; i++)
        {
            if (resources[i].GetID() == res.GetID())
                resources[i].Add(res.GetAmount());
        }
    }
    public int TakeFromBuilding(GameResources res, BuildingScript bs)
    {
        int[] bsr = bs.GetBuilding().resourceCurrent;
        for (int i = 0; i < bsr.Count; i++)
        {
            if (resources[i].GetID() == res.GetID())
            {
                if (resources[i].GetAmount() >= res.GetAmount())
                {
                    resources[i].Take(res.GetAmount());
                    return res.GetAmount();
                }
                else if (resources[i].GetAmount() < res.GetAmount())
                {
                    int take = resources[i].GetAmount();
                    resources[i].Take(take);
                    return take;
                }
            }
        }
        return 0;
    }
    public void Purchase(Building b)
    {
        coins -= b.GetCost();
        for (int i = 0; i < GameResources.GetBuildingResourcesCount(); i++)
        {
            resources[i].Take(b.GetMaterialCost(i));
        }
    }*/

    // take food from warehouse
    public GameResources TakeFoodForPerson(PersonScript ps)
    {
        GameResources ret = null;
        foreach(BuildingScript bs in BuildingScript.allBuildings)
        {
            // find a food warehouse building
            if(bs.GetBuilding().id == Building.WAREHOUSEFOOD)
            {
                // check if person is in range of food
                if(!GameManager.InRange(ps.transform.position, bs.transform.position, bs.GetBuilding().foodRange)) continue;

                // get all food resources in warehouse
                List<GameResources> foods = new List<GameResources>();
                int[] bsr = bs.GetBuilding().resourceCurrent;
                for(int i = 0; i < bs.GetBuilding().resourceCurrent.Length; i++)
                {
                    if(bsr[i] > 0) foods.Add(new GameResources(i,bsr[i]));
                }
                if(foods.Count > 0)
                {
                    // take a random food
                    int j = Random.Range(0,foods.Count);
                    bs.GetBuilding().Take(new GameResources(foods[j].id, 1));
                    ret = foods[j].Clone();
                }

                // since there's only one warehouse, we can end looking for buildings
                break;
            }
        }
        return ret;
    }

    public int[] GetTotalResourceCount()
    {
        int[] ret = new int[GameResources.COUNT];
        foreach(BuildingScript bs in BuildingScript.allBuildings)
        {
            for(int i = 0; i < GameResources.COUNT; i++)
                if(bs.GetBuilding().GetBuildingType() == BuildingType.Storage)
                    ret[i] += bs.GetBuilding().resourceCurrent[i];
        }
        return ret;
    }

    // setup a new village
    public void SetupNewVillage()
    {
        isSetup = true;

        GameManager.UnlockResource(GameResources.WOOD);
        BuildingScript bs = BuildManager.SpawnBuilding(0, Vector3.zero, Quaternion.identity, 0, Grid.WIDTH/2-1, Grid.HEIGHT/2-1, false);
        bs.GetBuilding().resourceCurrent[GameResources.WOOD] = 25;
        FinishBuildEvent(bs);

        AddStarterPeople();
        SpawnRandomItems();
        nature.SetupNature();
    }

    /*public Node GetGrid(int x, int y)
    {
        return Grid.GetNode(x,y);
    }*/
   /* public bool GetGridOccupied(int x, int y)
    {
        return nodes[x,y].IsOccupied();
    }
    public void SetGridOccupied(int x, int y, Transform t)
    {
        grid[x, y].nodeObject = t;
    }*/
    /*public bool GetGridTemp(int x, int y)
    {
        bool ret = gridTemp[x, y];
        gridTemp[x, y] = false;
        return ret;
    }
    public void SetGridTemp(int x, int y)
    {
        gridTemp[x, y] = true;
    }*/

    private void AddStarterPeople()
    {
        AddRandomNamePerson(Gender.Male, Random.Range(20, 30), new Job(0));
        AddRandomNamePerson(Gender.Female, Random.Range(20, 30), new Job(0));
        /*for (int i = 0; i < 2; i++ )
            AddRandomNamePerson(i < 1 ? Gender.Male : Gender.Female, Random.Range(20, 30), Job.GetJob(2));
        for (int i = 0; i < 2; i++)
            AddRandomNamePerson(i < 1 ? Gender.Male : Gender.Female, Random.Range(20, 30), Job.GetJob(5));*/
    }
    private Person PersonBirth()
    {
        int gend = UnityEngine.Random.Range(0, 2);
        Gender gender = (Gender)gend;

        Job job = new Job(0);
        Person p = new Person(PersonScript.allPeople.Count, gender == Gender.Male ? Person.getRandomMaleName() : Person.getRandomFemaleName(), Person.getRandomLastName(), gender, 0, job);
        UnitManager.SpawnPerson(p);
        return p;
    }
    private Person RandomPersonDeath()
    {
        Person p = null;
        if (PersonScript.allPeople.Count == 0) return p;
        int id = Random.Range(0, PersonScript.allPeople.Count);
        PersonDeath(new List<PersonScript>(PersonScript.allPeople)[id]);
        return p;
    }
    private void PersonDeath(PersonScript p)
    {
        if(p == null) return;

        GameManager.Msg(p.GetPerson().GetFirstName() + " ist gestorben!");

        if (PersonScript.selectedPeople.Contains(p))
        {
            PersonScript.selectedPeople.Remove(p);
        }
        if(p) Destroy(p.gameObject);
    }
    private void AddRandomPeople(int count)
    {
        for (int i = 0; i < count; i++)
        {
            int gend = UnityEngine.Random.Range(0, 2);
            Gender gender = (Gender)gend;

            Job job = new Job(Job.UNEMPLOYED);
            Person p = new Person(i + 1, gender == Gender.Male ? Person.getRandomMaleName() : Person.getRandomFemaleName(), Person.getRandomLastName(), gender, UnityEngine.Random.Range(12, 80), job);
            UnitManager.SpawnPerson(p);
        }
    }
    private void AddRandomNamePerson(Gender gender, int age, Job job)
    {
        Person p = new Person(PersonScript.allPeople.Count+1, gender == Gender.Male ? Person.getRandomMaleName() : Person.getRandomFemaleName(), Person.getRandomLastName(), gender, age, job);
        UnitManager.SpawnPerson(p);
    }
    private void SpawnRandomItems()
    {
        int x, y;
        bool[,] itemInNode = new bool[Grid.WIDTH, Grid.HEIGHT];
        List<int> itemRes = new List<int>();
        itemRes.Add(GameResources.WOOD);
        itemRes.Add(GameResources.STONE);
        for (int i = 0; i < 100; i++)
        {
            x = UnityEngine.Random.Range(0, Grid.WIDTH);
            y = UnityEngine.Random.Range(0, Grid.HEIGHT);
            if (Grid.GetNode(x, y).IsOccupied()) continue;
            if (itemInNode[x, y]) continue;

            itemInNode[x, y] = true;

            int id = Random.Range(0,2);
            ItemManager.SpawnItem(id, Random.Range(1,3), Grid.ToWorld(x,y));
        }
    }

    public Transform GetNearestPlant(Vector3 position, PlantType type, float range)
    {
        if (nature.flora.Count == 0) return null;
        Transform nearestTree = null;
        float dist = float.MaxValue;
        foreach (Plant plant in nature.flora)
        {
            if (plant && plant.type == type && plant.gameObject.activeSelf)
            {
                float temp = Vector3.Distance(plant.transform.position, position);
                if (temp < dist)
                {
                    dist = temp;
                    nearestTree=plant.transform;
                }
            }
        }
        if (dist > range) return null;
        //if (nearestTree.GetComponent<Plant>().GetPlantType() != PlantType.Tree) return null;
        return nearestTree;
    }
    public Transform GetNearestItemInRange(Vector3 position, Item itemType, float range)
    {
        Transform nearestItem = null;
        float dist = float.MaxValue;
        foreach (Item it in Item.allItems)
        {
            if (it.ResID() == itemType.ResID() && it.gameObject.activeSelf)
            {
                float temp = Vector3.Distance(it.transform.position, position);
                if (temp < dist)
                {
                    dist = temp;
                    nearestItem = it.transform;
                }
            }
        }
        if (dist > range) return null;
        return nearestItem;
    }
    public Transform GetNearestBuildingType(Vector3 position, BuildingType type)
    {
        Transform nearestStorage = null;
        float dist = float.MaxValue;
        foreach (BuildingScript b in BuildingScript.allBuildings)
        {
            Building bb = b.GetBuilding();
            if (b.blueprint) continue;
            if (bb.GetBuildingType()  == type)
            {
                float temp = Vector3.Distance(b.transform.position, position);
                if (temp < dist)
                {
                    dist = temp;
                    nearestStorage = b.transform;
                }
            }
        }
        return nearestStorage;
    }
    public Transform GetNearestBuildingID(Vector3 position, int id)
    {
        Transform nearestStorage = null;
        float dist = float.MaxValue;
        foreach (BuildingScript b in BuildingScript.allBuildings)
        {
            Building bb = b.GetBuilding();
            if (b.blueprint) continue;
            if (bb.GetID() == id)
            {
                float temp = Vector3.Distance(b.transform.position, position);
                if (temp < dist)
                {
                    dist = temp;
                    nearestStorage = b.transform;
                }
            }
        }
        return nearestStorage;
    }
    public Transform GetNearestBuildingBlueprint(Vector3 position)
    {
        Transform nearestStorage = null;
        float dist = float.MaxValue;
        foreach (BuildingScript b in BuildingScript.allBuildings)
        {
            Building bb = b.GetBuilding();
            if (!b.blueprint) continue;
            float temp = Vector3.Distance(b.transform.position, position);
            if (temp < dist)
            {
                dist = temp;
                nearestStorage = b.transform;
            }
        }
        return nearestStorage;
    }
    public Transform GetNearestStorageBuilding(Vector3 position, int resId)
    {
        Transform nearestStorage = null;
        float dist = float.MaxValue;
        foreach (BuildingScript b in BuildingScript.allBuildings)
        {
            Building bb = b.GetBuilding();
            // you can store items in storage buildings and crafting buildings
            if (b.blueprint || (bb.GetBuildingType() != BuildingType.Storage &&bb.GetBuildingType() != BuildingType.Crafting)) continue;
            if(bb.resourceCurrent.Length <= resId || bb.resourceStorage.Length <= resId) continue;
            if (bb.resourceCurrent[resId] < bb.resourceStorage[resId])
            {
                float temp = Vector3.Distance(b.transform.position, position);
                if (temp < dist)
                {
                    dist = temp;
                    nearestStorage = b.transform;
                }
            }
        }
        return nearestStorage;
    }

    // returns if pos is in build range of cave
    public bool InBuildRange(Vector3 pos)
    {
        return GameManager.InRange(BuildManager.Instance.cave.transform.position, pos, BuildManager.Instance.cave.GetBuilding().buildRange);
    }

    // When a building is finished, trigger event
    public void FinishBuildEvent(BuildingScript bs)
    {
        Building b = bs.GetBuilding();
        int unlockedBuilding = -1;
        int unlockedJob = -1;
        unlockedBuilding = b.GetID() + 1;
        unlockedJob = b.jobId;
        if (unlockedBuilding >= Building.BuildingCount()) unlockedBuilding = -1;
        if(unlockedJob >= Job.COUNT || unlockedJob == Job.UNEMPLOYED) unlockedJob = -1;

        if(unlockedJob != -1 && !Job.IsUnlocked(unlockedJob))
        {
            Job.Unlock(unlockedJob);
            Job nj = new Job(unlockedJob);
            GameManager.Msg("Neuen Beruf freigeschalten: "+nj.jobName);
        }
        if(unlockedBuilding != -1 && !Building.GetBuilding(unlockedBuilding).IsUnlocked())
        {
            Building nb = Building.GetBuilding(unlockedBuilding);
            nb.Unlock();
            GameManager.Msg("Neues Gebäude freigeschalten: "+nb.GetName());
        }
        
        foreach(Plant p in GameManager.village.nature.flora)
        {
            if(p && p.gameObject.activeSelf)
                p.UpdateBuildingViewRange();
        }
        foreach(Item p in Item.allItems)
        {
            if(p && p.gameObject.activeSelf)
                p.UpdateBuildingViewRange();
        }
    }
}

/* private float gameSpeed = 100f;

    public float currentCurrency;
    public float[] currentMaterials;
    private float maxPopulation;
    public int currentFreePopulation, currentBusyPopulation;
    private List<Person> population = new List<Person>();
    private float[] currentNeed;

    public float taxIncomeTot = 0;

    public float reproduceTimer = 0f;

    public static float taxPerPerson = 10;
    private static float reproduceRate = 1;

    private List<BuildingScript> buildings = new List<BuildingScript>();
    private bool[,] buildingGrid;

    [SerializeField]
    private Transform populationPanel, currencyPanel, materialsPanel;

    void Start () {

        currentCurrency = 0;
        currentMaterials = new float[3];
        maxPopulation = 0;
        buildingGrid = new bool[GameManager.GRID_WIDTH, GameManager.GRID_HEIGHT];
        for (int x = 0; x < GameManager.GRID_WIDTH; x++)
        {
            for (int y = 0; y < GameManager.GRID_HEIGHT; y++)
            {
                buildingGrid[x, y] = false;
            }
        }
    }
    void Update () {

        if (Input.GetMouseButtonDown(0))
        {
            if (populationPanel.Find("PanelPopulationDropdown").gameObject.activeSelf)
            {
                populationPanel.Find("PanelPopulationDropdown").gameObject.SetActive(false);
                populationPanel.Find("PanelPopulation").gameObject.SetActive(true);
            }
            if (currencyPanel.Find("PanelCurrencyDropdown").gameObject.activeSelf)
            {
                currencyPanel.Find("PanelCurrencyDropdown").gameObject.SetActive(false);
                currencyPanel.Find("PanelCurrency").gameObject.SetActive(true);
            }
        }

        reproduceTimer += Time.deltaTime;

        int need = 5; // 1-10
        int toReproduce = 0;

        if (need > 0 && reproduceTimer >= 60f * 60f / reproduceRate / need / gameSpeed)
        {
            reproduceTimer = 0;
            toReproduce = 1;
        }

        int nid = UserManager.nationID;

        taxIncomeTot = 0;

        float hourFac = Time.deltaTime / 60f / 60f * gameSpeed;
        maxPopulation = 0;
        currentFreePopulation = 0; currentBusyPopulation = 0;
        for (int i = 0; i < buildings.Count; i++)
        {
            Building b = buildings[i].b;
            if (toReproduce > 0 && buildings[i].populationCurrent < b.populationRoom)
            {
                buildings[i].populationCurrent++;
                toReproduce--;
            }
            currentFreePopulation += buildings[i].populationCurrent;
            currentBusyPopulation += b.populationUse;
            maxPopulation += b.populationRoom;
            for (int j = 0; j < currentMaterials.Length; j++)
                currentMaterials[j] += b.materialProduce[j] * hourFac;
        }
        currentCurrency += currentFreePopulation * taxPerPerson * hourFac;
        currentCurrency += currentBusyPopulation * taxPerPerson * hourFac;
        taxIncomeTot = currentFreePopulation + currentBusyPopulation;
        taxIncomeTot *= taxPerPerson;
        populationPanel.Find("PanelPopulation").Find("TextPopulation").GetComponent<Text>().text = currentFreePopulation + " Bewohner";
        populationPanel.Find("PanelPopulationDropdown").Find("TextFreePopulation").GetComponent<Text>().text = "Freie Bewohner: "+currentFreePopulation;
        populationPanel.Find("PanelPopulationDropdown").Find("TextWorkPopulation").GetComponent<Text>().text = "Beschäftigte Bewohner: "+currentBusyPopulation;
        populationPanel.Find("PanelPopulationDropdown").Find("TextMaxPopulation").GetComponent<Text>().text = "Max. Bevölkerung: "+maxPopulation;
        currencyPanel.Find("PanelCurrency").Find("TextCurrency").GetComponent<Text>().text = (int)currentCurrency + " " + currencyName[nid];
        currencyPanel.Find("PanelCurrencyDropdown").Find("TextTotalCurrency").GetComponent<Text>().text = "Münzbestand: " + (int)currentCurrency;
        currencyPanel.Find("PanelCurrencyDropdown").Find("TextEarning").GetComponent<Text>().text = "Einnahmen/h: " + (int)taxIncomeTot;
        currencyPanel.Find("PanelCurrencyDropdown").Find("TextInvestment").GetComponent<Text>().text = "Ausgaben: -";
        materialsPanel.Find("Wood").Find("TextWood").GetComponent<Text>().text = (int)currentMaterials[0]+""; //+ " " + materialName[nid, 0];
        materialsPanel.Find("TextClay").GetComponent<Text>().text = (int)currentMaterials[1] + " " + materialName[nid, 1];
        materialsPanel.Find("TextIron").GetComponent<Text>().text = (int)currentMaterials[2] + " " + materialName[nid, 2];
    }

    public bool AddBuilding(BuildingScript b)
    {
        for (int x = b.gridX; x < b.gridX + b.b.gridSizeX; x++)
        {
            for (int y = b.gridY; y < b.gridY + b.b.gridSizeY; y++)
            {
                if (x >= GameManager.GRID_WIDTH || y >= GameManager.GRID_HEIGHT || x < 0 || y < 0) return false;
                if (buildingGrid[x, y]) return false;
            }
        }
        for (int x = b.gridX; x < b.gridX + b.b.gridSizeX; x++)
        {
            for (int y = b.gridY; y < b.gridY + b.b.gridSizeY; y++)
            {
                buildingGrid[x, y] = true;
            }
        }
        buildings.Add(b);
        return true;
    }

    public void EmployPeople(int n)
    {
        for (int i = 0; i < buildings.Count; i++)
        {
            if (n == 0) break;
            while(buildings[i].populationCurrent > 0 && n > 0)
            {
                buildings[i].populationCurrent--;
                n--;
            } 
        }
    }

    public void OnPopulationClick()
    {
        populationPanel.Find("PanelPopulationDropdown").gameObject.SetActive(true);
        populationPanel.Find("PanelPopulation").gameObject.SetActive(false);
    }
    public void OnCurrencyClick()
    {
        currencyPanel.Find("PanelCurrencyDropdown").gameObject.SetActive(true);
        currencyPanel.Find("PanelCurrency").gameObject.SetActive(false);
    }
    public void OnMaterialsClick()
    {
        currencyPanel.Find("PanelCurrencyDropdown").gameObject.SetActive(true);
        currencyPanel.Find("PanelCurrency").gameObject.SetActive(false);
    }

    public static string[,] periodNames = { 
        { "Zeitalter1", "Zeitalter2", "Zeitalter3" },
        { "Zeitalter1", "Zeitalter2", "Zeitalter3" },
        { "Zeitalter1", "Zeitalter2", "Zeitalter3" }
    };
    public static string[] nationName = { "Wikinger", "Ägypter", ""};
    public static string[] currencyName = { "Münzen", "", "" };
    public static string[,] materialName = { 
        { "Holz", "Lehm", "Eisen" },
        { "Holz", "Lehm", "Eisen" }, 
        { "Holz", "Lehm", "Eisen" }
    };*/
