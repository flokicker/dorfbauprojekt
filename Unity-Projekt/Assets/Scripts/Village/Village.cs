using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Village : MonoBehaviour {

    public Nature nature;

    private int coins = 2500;

    // Nahrungsvielfalt, Wohnraum, Gesundheit, Fruchtbarkeit, Luxus
    private float foodFactor = 50, roomspaceFactor = 0, healthFactor = 50, fertilityFactor = 0, luxuryFactor = 0;
    private float totalFactor = 50;

    private float growthTime = 0, deathTime = 0;

    // Religious faith [-100,100]
    private float faithPoints;

    void Start()
    {
        nature = GetComponent<Nature>();
    }
    void Update()
    {
        if(!GameManager.IsSetup()) return;

        //resources[5].Add(1);
        RecalculateFactors();
        UpdatePopulation();

        // Check if any PersonData is dead
        foreach(PersonScript p in PersonScript.allPeople)
        {
            if(p.IsDead())
            {
                PersonDeath(p);
                break;
            }
        }
    }

    public void UpdatePopulation()
    {
        PersonData p;
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
            ChatManager.Msg(p.firstName + " ist gerade geboren!");
        }
    }
    public int EmployedPeopleCount()
    {
        int employedPeople = 0;
        foreach (PersonScript p in PersonScript.allPeople)
            if (p.IsEmployed())
                employedPeople++;
        return employedPeople;
    }
    public int[] JobEmployedCount()
    {
        int[] jobEmployedPeople = new int[Job.COUNT];
        foreach (PersonScript p in PersonScript.allPeople)
            jobEmployedPeople[p.job.id]++;
        return jobEmployedPeople;
    }
    public int[] MaxPeopleJob()
    {
        int[] maxPeople = new int[Job.COUNT];
        foreach(Building bs in Building.allBuildings)
        {
            if(!bs.blueprint)
            maxPeople[bs.jobId] += bs.workspace;
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
        foreach (Building b in Building.allBuildings)
        {
            if (b.blueprint) continue;
            space += b.populationRoom;
            luxuryFactor += b.LuxuryFactor();
        }
        if (PersonScript.allPeople.Count == 0) roomspaceFactor = 0;
        else
            roomspaceFactor = (int)(space * 80 / PersonScript.allPeople.Count);

        // Fertility factor
        int fertileMen = 0;
        int fertileWomen = 0;
        foreach (PersonScript p in PersonScript.allPeople)
        {
            if (p.IsFertile())
            {
                if (p.gender == Gender.Male) fertileMen++;
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

        totFactDiff = healthFactor - GetCalcHealthFactor();
        if (totFactDiff > 0)
        {
            healthFactor -= Time.deltaTime / 1f;
        }
        if (totFactDiff < 0) healthFactor += Time.deltaTime / 1f;

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
    public int GetCalcHealthFactor()
    {
        if(PersonScript.allPeople.Count == 0) return 0;
        float totHealthFactor = 0;
        foreach (PersonScript p in PersonScript.allPeople) totHealthFactor += p.health;
        totHealthFactor /= PersonScript.allPeople.Count*2;
        
        foreach (Building b in Building.allBuildings)
        {
            if (b.blueprint) continue;
            totHealthFactor += b.HealthFactor();
        }

        return (int)totHealthFactor;
    }
    public int GetCalcFoodFactor()
    {
        /*float foodUse = 0;
        foreach (PersonData p in people) foodUse += p.FoodUse();
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
        float totFoodFactor = 0, totFoodUse = 0;
        foreach (PersonScript p in PersonScript.allPeople) {
             totFoodFactor += p.hunger;
             totFoodUse += p.FoodUse();
        }
        totFoodFactor /= PersonScript.allPeople.Count*2;

        int totNutrition = 0;
        foreach (Building b in Building.allBuildings)
        {
            for(int rid = 0; rid < GameResources.COUNT; rid++)
            {
                GameResources r = new GameResources(rid, b.resourceCurrent[rid]);
                if (r.GetResourceType() == ResourceType.Food && r.GetAmount() > 0)
                {
                    totNutrition += (int)(r.GetNutrition() * r.amount);
                }
            }
        }
        if(totFoodUse == 0) return (int)totFoodFactor;
        totNutrition /= (int)totFoodUse;

        totFoodFactor += totNutrition;

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
    public float GetFaithPoints()
    {
        return faithPoints;
    }

    /*public void Restock(GameResources res)
    {
        for (int i = 0; i < resources.Count; i++)
        {
            if (resources[i].GetID() == res.GetID())
                resources[i].Add(res.GetAmount());
        }
    }
    public int TakeFromBuilding(GameResources res, Building bs)
    {
        int[] bsr = bs.resourceCurrent;
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
        foreach(Building bs in Building.allBuildings)
        {
            // find a food warehouse building
            if(bs.id == Building.WAREHOUSEFOOD)
            {
                // check if PersonData is in range of food
                if(!GameManager.InRange(ps.transform.position, bs.transform.position, bs.foodRange)) continue;

                // get all food resources in warehouse
                List<GameResources> foods = new List<GameResources>();
                int[] bsr = bs.resourceCurrent;
                for(int i = 0; i < bs.resourceCurrent.Length; i++)
                {
                    if(bsr[i] > 0) foods.Add(new GameResources(i,bsr[i]));
                }
                if(foods.Count > 0)
                {
                    // take a random food
                    int j = Random.Range(0,foods.Count);
                    bs.Take(new GameResources(foods[j].id, 1));
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
        foreach(Building bs in Building.allBuildings)
        {
            for(int i = 0; i < GameResources.COUNT; i++)
                if(bs.type == BuildingType.Storage)
                    ret[i] += bs.resourceCurrent[i];
        }
        return ret;
    }

    // setup a new village
    public void SetupNewVillage()
    {
        nature = GetComponent<Nature>();

        GameManager.UnlockResource(GameResources.WOOD);
        GameManager.UnlockResource(GameResources.STONE);
        Building bs = BuildManager.SpawnBuilding(0, Vector3.zero, Quaternion.Euler(0,-90,0), 3, Grid.WIDTH/2-1, Grid.HEIGHT/2-1, false);
        // Add starter resources 
        bs.resourceCurrent[GameResources.WOOD] = 15;
        bs.resourceCurrent[GameResources.STONE] = 15;
        FinishBuildEvent(bs);

        AddStarterPeople();
        SpawnRandomItems();
        nature.SetupNature();
        AddAnimals();
    }

    public void SetVillageData(GameData gd)
    {
        coins = gd.coins;
        foodFactor = gd.foodFactor;
        roomspaceFactor = gd.roomspaceFactor;
        healthFactor = gd.healthFactor;
        fertilityFactor = gd.fertilityFactor;
        luxuryFactor = gd.luxuryFactor;
        totalFactor = gd.totalFactor;

        faithPoints = gd.faithPoints;
        if (gd.faithEnabled) UIManager.Instance.EnableFaithBar();
        if (gd.techTreeEnabled) UIManager.Instance.EnableTechTree();
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

    private void AddAnimals()
    {
        for(int i = 0; i < 10; i++)
            AddRandomAnimal();
    }
    private void AddRandomAnimal()
    {
        Node water = new List<Node>(nature.shore)[0];
        Vector3 worldPos = Grid.ToWorld(water.gridX+Random.Range(-10,10),water.gridY+Random.Range(-10,10));
        float smph = Terrain.activeTerrain.SampleHeight(worldPos);
        worldPos.y = Terrain.activeTerrain.transform.position.y + smph;
        UnitManager.SpawnAnimal(Random.Range(0,1), worldPos);
    }

    private void AddStarterPeople()
    {
        /*PersonData p = RandomPerson(Gender.Male, Random.Range(20, 30));
        UnitManager.SpawnPerson(p);
        p = RandomPerson(Gender.Female, Random.Range(20, 30));
        UnitManager.SpawnPerson(p);*/

        PersonData myPerson = RandomPerson(Gender.Male, 20);
        myPerson.firstName = GameManager.username;
        myPerson.SetPosition(new Vector3(2,0,0)*Grid.SCALE);
        myPerson.SetRotation(Quaternion.Euler(0,90,0));
        UnitManager.SpawnPerson(myPerson);
    }
    private PersonData PersonBirth()
    {
        PersonData p = RandomPerson();
        UnitManager.SpawnPerson(p);
        return p;
    }
    private PersonData RandomPerson()
    {
        return RandomPerson((Gender)UnityEngine.Random.Range(0, 2), Random.Range(0,20));
    }
    private PersonData RandomPerson(Gender gend, int age)
    {
        PersonData p = new PersonData();
        p.gender = gend;
        p.firstName = p.gender == Gender.Male ? PersonScript.RandomMaleName() : PersonScript.RandomFemaleName();
        p.lastName = PersonScript.RandomLastName();
        p.age = age;
        p.jobID = 0;
        p.health = 100;
        p.hunger = 60;
        p.disease = Disease.None;

        // lifetime expectancy between 30 and 40 years
        p.lifeTimeYears = Random.Range(30,40);
        p.lifeTimeDays = Random.Range(0,365);
        
        Node spawnNode;
        int counter = 0;
        do
        {
            spawnNode = Grid.GetNode(Grid.WIDTH/2+Random.Range(-4,4),Grid.HEIGHT/2+Random.Range(-4,4));
            if((counter++)>1000) break;

        } while(spawnNode.IsOccupied() || spawnNode.IsPeopleOccupied());
        p.SetPosition(Grid.ToWorld(spawnNode.gridX, spawnNode.gridY));
        return p;
    }
    private PersonData RandomPersonDeath()
    {
        PersonData p = null;
        if (PersonScript.allPeople.Count == 0) return p;
        int id = Random.Range(0, PersonScript.allPeople.Count);
        PersonDeath(new List<PersonScript>(PersonScript.allPeople)[id]);
        return p;
    }
    private void PersonDeath(PersonScript p)
    {
        if(p == null) return;

        ChatManager.Msg(p.firstName + " ist gestorben!");

        if (PersonScript.selectedPeople.Contains(p))
        {
            PersonScript.selectedPeople.Remove(p);
        }
        if(p) Destroy(p.gameObject);
    }

    private void SpawnRandomItems()
    {
        int x, y;
        bool[,] itemInNode = new bool[Grid.WIDTH, Grid.HEIGHT];
        List<int> itemRes = new List<int>();
        itemRes.Add(GameResources.WOOD);
        itemRes.Add(GameResources.STONE);
        for (int i = 0; i < 500; i++)
        {
            int range = Mathf.Min(Grid.WIDTH/2,i/4+4);
            x = UnityEngine.Random.Range(Grid.WIDTH/2-range, Grid.WIDTH/2+range);
            y = UnityEngine.Random.Range(Grid.HEIGHT/2-range, Grid.HEIGHT/2+range);
            if (Grid.GetNode(x, y).IsOccupied()) continue;
            if (itemInNode[x, y]) continue;

            itemInNode[x, y] = true;

            int id = Random.Range(0,2);
            ItemManager.SpawnItem(id, Random.Range(1,3), Grid.ToWorld(x,y) + new Vector3(Random.Range(-1f,1f),0,Random.Range(-1f,1f))*Grid.SCALE*0.8f);
        }
    }

    public Transform GetNearestPlant(Vector3 position, PlantType type, float range)
    {
        if (Nature.flora.Count == 0) return null;
        Transform nearestTree = null;
        float dist = float.MaxValue;
        foreach (Plant plant in Nature.flora)
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
        foreach (Building b in Building.allBuildings)
        {
            if (b.blueprint) continue;
            if (b.type == type)
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
        foreach (Building b in Building.allBuildings)
        {
            if (b.blueprint) continue;
            if (b.id == id)
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
        foreach (Building b in Building.allBuildings)
        {
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
    public Transform GetNearestStorageBuilding(Vector3 position, int resId, bool checkFull)
    {
        Transform nearestStorage = null;
        float dist = float.MaxValue;
        foreach (Building b in Building.allBuildings)
        {
            // you can store items in storage buildings and crafting buildings
            if (b.blueprint || (b.type != BuildingType.Storage && b.type != BuildingType.Crafting)) continue;
            if(b.id == Building.HUNTINGLODGE) continue;
            if(b.resourceCurrent.Length <= resId || b.resourceStorage.Length <= resId) continue;
            if (b.resourceCurrent[resId] < b.resourceStorage[resId] || !checkFull)
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
        return GameManager.InRange(BuildManager.Instance.cave.transform.position, pos, BuildManager.Instance.cave.buildRange);
    }

    // When a building is finished, trigger event
    public void FinishBuildEvent(Building b)
    {
        int unlockedBuilding = -1;
        int unlockedJob = -1;
        unlockedBuilding = b.unlockBuildingID;
        unlockedJob = b.jobId;
        if (unlockedBuilding >= Building.COUNT) unlockedBuilding = -1;
        if(unlockedJob >= Job.COUNT || unlockedJob == Job.UNEMPLOYED) unlockedJob = -1;

        if(unlockedJob != -1 && !Job.IsUnlocked(unlockedJob))
        {
            Job.Unlock(unlockedJob);
            Job nj = new Job(unlockedJob);
            ChatManager.Msg("Neuen Beruf freigeschalten: "+nj.jobName);
        }
        if(unlockedBuilding != -1 && !Building.IsUnlocked(unlockedBuilding))
        {
            Building.Unlock(unlockedBuilding);
            ChatManager.Msg("Neues Gebäude freigeschalten");
            UIManager.Instance.Blink("ButtonBuild", true);
        }
        
        if(b.id == Building.SACRIFICIALALTAR)
        {
            UIManager.Instance.EnableFaithBar();
            UIManager.Instance.Blink("PanelTopFaith", true);
        }
        if (b.id == Building.RESEARCH)
        {
            UIManager.Instance.EnableTechTree();
            UIManager.Instance.Blink("PanelTopTechTree", true);
        }

        foreach (Plant p in Nature.flora)
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
    private List<PersonData> population = new List<PersonData>();
    private float[] currentNeed;

    public float taxIncomeTot = 0;

    public float reproduceTimer = 0f;

    public static float taxPerPerson = 10;
    private static float reproduceRate = 1;

    private List<Building> buildings = new List<Building>();
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

    public bool AddBuilding(Building b)
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
