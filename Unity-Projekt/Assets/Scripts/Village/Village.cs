using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Village : MonoBehaviour {

    public Nature nature;

    private int currentDay;
    private float dayChangeTimeElapsed;
    private float secondsPerDay = 2f;

    private List<Person> people = new List<Person>();
    private List<PersonScript> peopleScript = new List<PersonScript>();

    private List<BuildingScript> buildings = new List<BuildingScript>();
    public List<Item> items = new List<Item>();

    public List<GameResources> resources = new List<GameResources>();
    private int coins;

    [SerializeField]
    private Transform peopleParentTransform;
    [SerializeField]
    private GameObject personPrefab;

    // Nahrungsvielfalt, Wohnraum, Gesundheit, Fruchtbarkeit, Luxus
    private float foodFactor, roomspaceFactor, healthFactor, fertilityFactor, luxuryFactor;
    private float totalFactor;

    private List<string> recentMessages = new List<string>();

    [SerializeField]
    private List<GameObject> itemPrefabs;

    private Transform plantsParent, specialParent, itemParent, buildingsParent;

    private float growthTime = 0, deathTime = 0;

    private bool isSetup = false;

    void Start()
    {
        coins = 2500;
        currentDay = 365 * 150;
        dayChangeTimeElapsed = 0;
        resources = GameResources.GetAllResources();

        for (int i = 0; i < resources.Count; i++)
            resources[i].SetAmount(0);
        resources[0].SetAmount(0);

        totalFactor = 0;

        foodFactor = 0;
        roomspaceFactor = 20;
        healthFactor = 20;
        fertilityFactor = 0;
        luxuryFactor = 0;

        specialParent = transform.Find("Special");
        itemParent = transform.Find("Dynamic").Find("Items");
        buildingsParent = transform.Find("Buildings");

        for (int i = 0; i < 10; i++)
            recentMessages.Add("Guten Start!");

        //AddCampfire();
        AddStarterPeople();
        SpawnRandomItems();
        //AddRandomPeople(10);
    }
    void Update()
    {
        if(!isSetup)
        {
            SetupNewVillage();
        }

        //resources[5].Add(1);
        RecalculateFactors();
        UpdatePopulation();

        dayChangeTimeElapsed += Time.deltaTime;
        if (dayChangeTimeElapsed >= secondsPerDay)
        {
            NextDay();
            dayChangeTimeElapsed -= secondsPerDay;
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
            p = PersonDeath();
            if (p != null)
            {
                NewMessage(p.GetFirstName() + " ist gestorben!");
                if (people.Count == 0) NewMessage("GAME OVER");
            }
        }

        /* TODO: better calculation for growth */
        // Time between births
        if (GetTotalFactor() > 20)
            growthTime += Time.deltaTime;
        tmp = 100000 / (GetTotalFactor() + 1);
        if (growthTime >= tmp)
        {
            growthTime -= tmp;
            p = PersonBirth();
            NewMessage(p.GetFirstName() + " ist gerade geboren!");
        }
    }
    public List<Person> GetPeople()
    {
        return people;
    }
    public List<PersonScript> GetPeopleScript()
    {
        return peopleScript;
    }
    public void AddPerson(Person p)
    {
        people.Add(p);
    }
    public void AddBuilding(BuildingScript bs)
    {
        buildings.Add(bs);
    }
    public int PeopleCount()
    {
        return people.Count;
    }
    public int EmployedPeopleCount()
    {
        int employedPeople = 0;
        foreach (Person p in people)
            if (p.IsEmployed())
                employedPeople++;
        return employedPeople;
    }
    public int[] JobEmployedCount()
    {
        int[] jobEmployedPeople = new int[Job.COUNT];
        foreach (Person p in people)
            jobEmployedPeople[p.GetJob().id]++;
        return jobEmployedPeople;
    }

    public int GetResourcesCount()
    {
        return resources.Count;
    }
    public GameResources GetResources(int id)
    {
        return resources[id];
    }

    public void RecalculateFactors()
    {
        // Roomspace factor
        int space = 0;
        foreach (BuildingScript b in buildings)
        {
            if (b.blueprint) continue;
            space += b.GetBuilding().GetPopulationRoom();
        }
        if (people.Count == 0) roomspaceFactor = 0;
        else
            roomspaceFactor = (int)(space * 80 / people.Count);

        // Fertility factor
        int fertileMen = 0;
        int fertileWomen = 0;
        foreach (Person p in people)
        {
            if (p.IsFertile())
            {
                if (p.GetGender() == Gender.Male) fertileMen++;
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
        if (foodFactor <= 5 && (foodFactor + roomspaceFactor + fertilityFactor) / 3 > foodFactor) return (int)foodFactor;
        return (int)(foodFactor + roomspaceFactor + fertilityFactor) / 3;
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
        foreach (PersonScript p in PersonScript.allPeople) totFoodFactor += p.health;
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

    public void Restock(GameResources res)
    {
        for (int i = 0; i < resources.Count; i++)
        {
            if (resources[i].GetID() == res.GetID())
                resources[i].Add(res.GetAmount());
        }
    }
    public int Take(GameResources res)
    {
        for (int i = 0; i < resources.Count; i++)
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
    }

    public void SetupNewVillage()
    {
        isSetup = true;
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

    private void NextDay()
    {
        currentDay++;
        if (currentDay % 365 == 0)
        {
            NextYear();
        }

        if (foodFactor < 0) foodFactor = 0;


        /*float foodUse = 0;
        foreach (Person p in people) foodUse += p.FoodUse();
        if (foodFactor < 100)
        {
            foreach (GameResources r in resources)
            {
                if (foodUse > 0 && r.GetResourceType() == ResourceType.Food && r.GetAmount() > 0)
                {
                    foodUse -= r.GetNutrition();
                    /*if (currentDay % (int)r.GetNutrition() == 0)
                        r.Take(1);
                }
            }
        }*/
    }
    private void NextYear()
    {
        NewMessage("Happy new year! " + (currentDay / 365) + "n.Chr.");
        foreach (Person p in people)
        {
            p.AgeOneYear();
        }
    }
    public int GetYear()
    {
        return currentDay / 365;
    }

    public void NewMessage(string s)
    {
        recentMessages.RemoveAt(0);
        recentMessages.Add(s);
    }
    public string GetMostRecentMessage()
    {
        if (recentMessages.Count == 0) return "ERROR: NO MESSAGES";
        return recentMessages[recentMessages.Count - 1];
    }

    /*public void AddCampfire()
    {
        int x = Grid.WIDTH / 2;
        int y = Grid.HEIGHT / 2;
        GameObject cf = (GameObject)Instantiate(campfire, Grid.ToWorld(x, y), Quaternion.identity, specialParent);
        cf.tag = "Special";
        Grid.GetNode(x,y).nodeObject = cf.transform;
        Grid.GetNode(x,y).objectWalkable = false;
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
        Person p = new Person(people.Count, gender == Gender.Male ? Person.getRandomMaleName() : Person.getRandomFemaleName(), Person.getRandomLastName(), gender, 0, job);
        InitializePerson(p);
        return p;
    }
    private Person PersonDeath()
    {
        Person p = null;
        if (people == null || people.Count == 0) return p;
        int id = Random.Range(0, people.Count);
        p = people[id];
        people.RemoveAt(id);
        if (PersonScript.selectedPeople.Contains(peopleScript[id]))
        {
            PersonScript.selectedPeople.Remove(peopleScript[id]);
        }
        Destroy(peopleScript[id].gameObject);
        peopleScript.RemoveAt(id);
        return p;
    }
    private void AddRandomPeople(int count)
    {
        for (int i = 0; i < count; i++)
        {
            int gend = UnityEngine.Random.Range(0, 2);
            Gender gender = (Gender)gend;

            Job job = new Job(Job.UNEMPLOYED);
            Person p = new Person(i + 1, gender == Gender.Male ? Person.getRandomMaleName() : Person.getRandomFemaleName(), Person.getRandomLastName(), gender, UnityEngine.Random.Range(12, 80), job);
            InitializePerson(p);
        }
    }
    private void AddRandomNamePerson(Gender gender, int age, Job job)
    {
        Person p = new Person(people.Count+1, gender == Gender.Male ? Person.getRandomMaleName() : Person.getRandomFemaleName(), Person.getRandomLastName(), gender, age, job);
        InitializePerson(p);
    }
    private void InitializePerson(Person p)
    {
        GameObject obj = (GameObject)Instantiate(personPrefab, new Vector3(UnityEngine.Random.Range(-5, 5) * Grid.SCALE, 0, UnityEngine.Random.Range(-5, 5) * Grid.SCALE), Quaternion.identity, peopleParentTransform);
        obj.GetComponent<PersonScript>().SetPerson(p);
        //VillageUIManager.AddPerson(p);
        AddPerson(p);
        peopleScript.Add(obj.GetComponent<PersonScript>());
    }
    private void SpawnRandomItems()
    {
        int x, y;
        bool[,] itemInNode = new bool[Grid.WIDTH, Grid.HEIGHT];
        List<int> itemRes = new List<int>();
        itemRes.Add(GameResources.WOOD);
        itemRes.Add(GameResources.STONE);
        for (int i = 0; i < 80; i++)
        {
            x = UnityEngine.Random.Range(0, Grid.WIDTH);
            y = UnityEngine.Random.Range(0, Grid.HEIGHT);
            if (Grid.GetNode(x, y).IsOccupied()) continue;
            if (itemInNode[x, y]) continue;

            itemInNode[x, y] = true;

            int id = Random.Range(0,itemPrefabs.Count);

            GameObject go = (GameObject)Instantiate(itemPrefabs[id], Grid.ToWorld(x,y), Quaternion.Euler(0,Random.Range(0,360),0), itemParent);
            Item it = go.AddComponent<Item>();
            it.SetResource(itemRes[id], 1);
            items.Add(it);
        }
    }

    public Transform GetNearestPlant(PlantType type, Vector3 position, float range)
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
    public Transform GetNearestItemInRange(Item itemType, Vector3 position, float range)
    {
        if (items.Count == 0) return null;
        Transform nearestItem = null;
        float dist = float.MaxValue;
        foreach (Item it in items)
        {
            if (it.GetResID() == itemType.GetResID() && it.gameObject.activeSelf)
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
        if (buildings.Count == 0) return null;
        Transform nearestStorage = null;
        float dist = float.MaxValue;
        foreach (BuildingScript b in buildings)
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
        if (buildings.Count == 0) return null;
        Transform nearestStorage = null;
        float dist = float.MaxValue;
        foreach (BuildingScript b in buildings)
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
        if (buildings.Count == 0) return null;
        Transform nearestStorage = null;
        float dist = float.MaxValue;
        foreach (BuildingScript b in buildings)
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

    public void FinishBuildEvent(Building b)
    {
        int unlockedBuilding = -1;
        int unlockedResource = -1;
        int unlockedJob = -1;
        unlockedBuilding = b.GetID() + 1;
        switch (b.GetID())
        {
            case 2: // unlock wood resource and gatherer job
                unlockedResource = GameResources.WOOD;
                break;
            case 3: // unlock mushroom resource and gatherer job
                unlockedResource = GameResources.MUSHROOM;
                break;
            case 4: // unlock fish resource and job
                unlockedResource = GameResources.FISH;
                break;
            default:
                break;
        }
        unlockedJob = b.jobId;
        if (unlockedBuilding >= Building.BuildingCount()) unlockedBuilding = -1;
        if (unlockedResource >= GameResources.ResourceCount()) unlockedResource = -1;
        if(unlockedJob >= Job.COUNT || unlockedJob == Job.UNEMPLOYED) unlockedJob = -1;

        if(unlockedJob != -1 && !Job.IsUnlocked(unlockedJob))
        {
            Job.Unlock(unlockedJob);
            Job nj = new Job(unlockedJob);
            NewMessage("Neuen Beruf freigeschalten: "+nj.jobName);
        }
        if (unlockedResource != -1 && !GameResources.GetAllResources()[unlockedResource].IsUnlocked())
        {
            GameResources.Unlock(unlockedResource);
            GameManager.GetGameSettings().GetFeaturedResources().Add(GameResources.GetAllResources()[unlockedResource]);
        }
        if(unlockedBuilding != -1 && !Building.GetBuilding(unlockedBuilding).IsUnlocked())
        {
            Building nb = Building.GetBuilding(unlockedBuilding);
            nb.Unlock();
            NewMessage("Neues Gebäude freigeschalten: "+nb.GetName());
        }
    }

    /*private void TestForest(int count)
    {
        for (int i = 0; i < count; i++)
        {
            int t = UnityEngine.Random.Range(0, trees.Count);
            int x = UnityEngine.Random.Range(0, Grid.WIDTH);
            int z = UnityEngine.Random.Range(0, Grid.HEIGHT);
            GameObject tree = (GameObject)Instantiate(trees[t], Grid.ToWorld(x,z), Quaternion.Euler(0, Random.Range(0,360), 0), treeParent);
            tree.AddComponent(typeof(cakeslice.Outline));
            Tree treeScript = (Tree)(tree.AddComponent(typeof(Tree)));
            treeScript.SetProperties(t / 10, (t % 10) + 1);
            tree.tag = "Tree";
            Grid.GetNode(x, z).nodeObject = tree.transform;
        }
    }
    private void TestRocks(int count)
    {
        for (int i = 0; i < count; i++)
        {
            int t = UnityEngine.Random.Range(0, rocks.Count);
            int x = UnityEngine.Random.Range(0, Grid.WIDTH);
            int z = UnityEngine.Random.Range(0, Grid.HEIGHT);
            GameObject rock = (GameObject)Instantiate(rocks[t], Grid.ToWorld(x, z), Quaternion.Euler(0, Random.Range(0, 360), 0), rockParent);
            rock.AddComponent(typeof(cakeslice.Outline));
            Rock rockScript = (Rock)(rock.AddComponent(typeof(Rock)));
            rockScript.SetProperties(t/3);
            rock.tag = "Rock";
            Grid.GetNode(x, z).nodeObject = rock.transform;
        }
    }
    private void TestMushrooms(int count)
    {
        for (int i = 0; i < count; i++)
        {
            int t = UnityEngine.Random.Range(0, mushrooms.Count);
            int x = UnityEngine.Random.Range(0, Grid.WIDTH);
            int z = UnityEngine.Random.Range(0, Grid.HEIGHT);
            GameObject mushroom = (GameObject)Instantiate(mushrooms[t], Grid.ToWorld(x, z), Quaternion.Euler(0, Random.Range(0, 360), 0), mushroomsParent);
            mushroom.AddComponent(typeof(cakeslice.Outline));
            Mushroom mushroomScript = (Mushroom)(mushroom.AddComponent(typeof(Mushroom)));
            mushroomScript.SetProperties(t / 5);
            mushroom.tag = "Mushroom";
            Grid.GetNode(x, z).nodeObject = mushroom.transform;
        }
    }*/
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
