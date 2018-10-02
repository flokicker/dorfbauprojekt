using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Village : MonoBehaviour {

    public Nature nature;
    public TechTree techTree;

    private int coins = 2500;

    // Nahrungsvielfalt, Wohnraum, Gesundheit, Fruchtbarkeit, Luxus
    private float foodFactor = 50, roomspaceFactor = 0, healthFactor = 50, fertilityFactor = 0, luxuryFactor = 0;
    private float totalFactor = 50;

    private float growthTime = 0, deathTime = 0;

    // Religious faith [-100,100]
    private float faithPoints;

    private int wildPeopleGroupSize = 5;
    private Transform wildPeopleSpawnpointsParent;

    void Start()
    {
        nature = GetComponent<Nature>();
        techTree = new TechTree();
    }
    void Update()
    {
        if(!GameManager.IsSetup()) return;

        //resources[5].Add(1);
        RecalculateFactors();
        UpdatePopulation();
        UpdateFaith();

        // Check if any PersonData is dead
        foreach (PersonScript p in PersonScript.allPeople)
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
        tmp = (int)(50000f / (GetTotalFactor() + 1) / GameManager.speedFactor);
        if (growthTime >= tmp)
        {
            int male = 0, female = 0;
            int randFemale = -1;
            foreach (PersonScript ps in PersonScript.allPeople)
            {
                if (GameManager.InRange(BuildManager.Instance.cave.transform.position, ps.transform.position, BuildManager.Instance.cave.BuildRange))
                {
                    if (ps.gender == Gender.Female)
                    {
                        female++;
                        if (ps.CanGetPregnant() && (randFemale == -1 || Random.Range(0, 2) == 0)) randFemale = ps.nr;
                    }
                    else male++;
                }
            }

            if (male >= 1 && female >= 1 && randFemale >= 0)
            {
                growthTime -= tmp;
                PersonScript ps = PersonScript.Identify(randFemale);
                if (!ps) ChatManager.Msg("error: pregnancy set");
                else
                {
                    ps.GetPregnant();
                    ChatManager.Msg("Herzlichen Glückwunsch, " + ps.firstName + " ist schwanger!");
                }
            }
            else
            {
                growthTime *= 0.95f;
            }
        }
    }
    public void UpdateFaith()
    {
        AddFaithPoints(Time.deltaTime / 60f / 5f);
    }
    public void NewPersonFaith()
    {
        int altarCount = AltarCount();

        if (PersonScript.allPeople.Count > altarCount * 30)
        {
            TakeFaithPoints(1);
        }
        else
        {
            AddFaithPoints(1/3f);
        }
    }
    /*public float InitialFaith()
    {
        int fp = 0;
        int altarCount = AltarCount();

        if(PersonScript.allPeople.Count > altarCount * 30)
        {
            fp = 10;
            fp -= (PersonScript.allPeople.Count - altarCount * 30)*2;
            fp = Mathf.Max(0, fp);
        }
        else
        {
            fp = Mathf.Min(10, PersonScript.allPeople.Count / 3);
        }
        return fp;
    }*/
    private int AltarCount()
    {
        return BuildingIdCount(Building.Id("Opferstätte"));
    }
    public int BuildingIdCount(int bid)
    {
        int count = 0;
        foreach (BuildingScript bs in BuildingScript.allBuildingScripts)
        {
            if (bs.Id == bid) count++;
        }
        return count;
    }
    public void TakeFaithPoints(float am)
    {
        faithPoints -= am;
        if (faithPoints < -100) faithPoints = -100;
    }
    public void AddFaithPoints(float am)
    {
        faithPoints += am;
        if (faithPoints > 100) faithPoints = 100;
    }
    public int CountEnergySpots()
    {
        int count = 0;
        foreach(NatureObjectScript p in Nature.nature)
        {
            if (p.Type == NatureObjectType.EnergySpot && p.IsBroken()) count++; 
        }
        return count;
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
        int[] jobEmployedPeople = new int[Job.Count];
        foreach (PersonScript p in PersonScript.allPeople)
            jobEmployedPeople[p.job.id]++;
        return jobEmployedPeople;
    }
    public int[] MaxPeopleJob()
    {
        int[] maxPeople = new int[Job.Count];
        foreach(BuildingScript bs in BuildingScript.allBuildingScripts)
        {
            if(!bs.Blueprint)
            maxPeople[bs.JobId] += bs.Workspace;
        }
        
        for(int i = 0; i < maxPeople.Length; i++)
            if(!Job.Get(i).limited)
                maxPeople[i] = -1;

        return maxPeople;
    }

    public void RecalculateFactors()
    {
        // factor influenced by buildings
        int space = 0;
        luxuryFactor = 0;
        foreach (BuildingScript bs in BuildingScript.allBuildingScripts)
        {
            if (bs.Blueprint) continue;
            space += bs.PopulationRoom[bs.Stage];
            luxuryFactor += bs.LuxuryFactor;
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
        
        foreach (BuildingScript bs in BuildingScript.allBuildingScripts)
        {
            if (bs.Blueprint) continue;
            totHealthFactor += bs.HealthFactor;
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
        totFoodFactor /= PersonScript.allPeople.Count;

        int totNutrition = 0;
        /*foreach (BuildingScript bs in BuildingScript.allBuildingScripts)
        {
            for(int rid = 0; rid < GameResources.COUNT; rid++)
            {
                GameResources r = new GameResources(rid, b.resourceCurrent[rid]);
                if (r.GetResourceType() == ResourceType.Food && r.GetAmount() > 0)
                {
                    totNutrition += (int)(r.GetNutrition() * r.amount);
                }
            }
        }*/
        /* TODO: food factor calculation */
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
        foreach(BuildingScript bs in BuildingScript.allBuildingScripts)
        {
            // outdated: find a food warehouse building
            
            // check if PersonData is in range of food
            if(!GameManager.InRange(ps.transform.position, bs.transform.position, bs.FoodRange)) continue;

            // get all food resources in warehouse
            List<GameResources> foods = new List<GameResources>();
            foods.AddRange(bs.StorageCurrent);

            /* TODO: check if actually is edible */

            while(foods.Count > 0)
            {
                // take a random food
                int j = Random.Range(0,foods.Count);
                if (foods[j].Edible && foods[j].Amount > 0)
                {
                    bs.Take(new GameResources(foods[j].Id, 1));
                    ret = new GameResources(foods[j]);
                    break;
                }
                foods.RemoveAt(j);
            }

            // since there's only one warehouse, we can end looking for buildings
            break;
            
        }
        return ret;
    }

    public List<BuildingQuestInfo> GetTotalBuildingsCount()
    {
        List<BuildingQuestInfo> ret = new List<BuildingQuestInfo>();
        foreach (BuildingScript bs in BuildingScript.allBuildingScripts)
        {
            if (bs.Blueprint) continue;

            bool exists = false;
            foreach (BuildingQuestInfo r in ret)
                if (r.buildingId == bs.Id)
                {
                    r.count++;
                    exists = true;
                }
            if (!exists) ret.Add(new BuildingQuestInfo(bs.Id,1));
        }
        return ret;
    }
    public List<GameResources> GetTotalResourceCount()
    {
        List<GameResources> ret = new List<GameResources>();
        foreach (BuildingScript bs in BuildingScript.allBuildingScripts)
        {
            //if (bs.Type != BuildingType.Storage && bs.Type != BuildingType.Food) continue;
            foreach (GameResources stor in bs.StorageCurrent)
            {
                bool exists = false;
                foreach (GameResources r in ret)
                    if (r.Id == stor.Id)
                    {
                        r.Add(stor.Amount);
                        exists = true;
                    }
                if (!exists) ret.Add(new GameResources(stor));
            }

        }
        return ret;
    }
    public List<GameResources> GetTotalResources(List<int> ids)
    {
        List<GameResources> ret = new List<GameResources>();
        foreach (int resId in ids)
            ret.Add(new GameResources(resId));

        foreach (BuildingScript bs in BuildingScript.allBuildingScripts)
        {
            //if (bs.Type != BuildingType.Storage && bs.Type != BuildingType.Food) continue;
            foreach (GameResources stor in bs.StorageCurrent)
            {
                foreach (GameResources r in ret)
                    if (r.Id == stor.Id)
                        r.Add(stor.Amount);
            }

        }
        return ret;
    }
    public int GetTotalResource(GameResources res)
    {
        if (res == null) return 0;

        int tot = 0;
        foreach (BuildingScript bs in BuildingScript.allBuildingScripts)
        {
            //if (bs.Type != BuildingType.Storage && bs.Type != BuildingType.Food && !bs.IsHut()) continue;
            foreach (GameResources stor in bs.StorageCurrent)
            {
                if (stor.Id == res.Id) tot += stor.Amount;
            }
        }
        return tot;
    }
    public bool TakeResources(List<GameResources> takeRes)
    {
        if (!GameManager.IsDebugging() && !EnoughResources(takeRes)) return false;

        List<GameResources> clonedRes = new List<GameResources>();
        foreach (GameResources res in takeRes)
            clonedRes.Add(new GameResources(res));

        foreach (BuildingScript bs in BuildingScript.allBuildingScripts)
        {
            //if (bs.Type != BuildingType.Storage && bs.Type != BuildingType.Food) continue;
            foreach (GameResources stor in bs.StorageCurrent)
            {
                foreach (GameResources r in clonedRes)
                    if (r.Id == stor.Id && stor.Amount > 0 && r.Amount > 0)
                    {
                        int taking = Mathf.Min(stor.Amount, r.Amount);
                        stor.Take(taking);
                        r.Take(taking);
                    }
                     
            }

        }
        return true;
    }
    public bool EnoughResources(List<GameResources> cost)
    {
        List<GameResources> total = GetTotalResourceCount();

        foreach(GameResources c in cost)
        {
            foreach (GameResources t in total)
            {
                if (t.Id == c.Id)
                {
                    if (t.Amount < c.Amount) return false;
                    else continue;
                }
            }
        }
        return true;
    }

    // setup a new village
    public void SetupNewVillage()
    {
        wildPeopleSpawnpointsParent = transform.Find("WildPeopleSpawnpoints");

        nature = GetComponent<Nature>();
        techTree = new TechTree();

        GameManager.UnlockResource("Holz");
        GameManager.UnlockResource("Stein");

        GameBuilding toSpawn = new GameBuilding(Building.Get("Höhle"), Grid.SpawnX - 1, Grid.SpawnY - 1, 3);
        toSpawn.SetPosition(Vector3.zero);
        toSpawn.SetRotation(Quaternion.Euler(0, -90, 0));
        BuildingScript bs = BuildManager.SpawnBuilding(toSpawn);
        BuildManager.Instance.cave = bs;

        // Add starter resources 
        bs.Restock(new GameResources("Holz", 15));
        bs.Restock(new GameResources("Stein", 15));
        FinishBuildEvent(bs.Building);

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

        techTree = gd.techTree;

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
        foreach(Animal a in Animal.allAnimals)
        {
            for (int i = 0; i < a.spawningLimit/2; i++)
                AddRandomAnimal(a);
        }
    }
    private void AddRandomAnimal(Animal baseAn)
    {
        Vector3 worldPos = Grid.ToWorld(Random.Range(0, Grid.WIDTH), Random.Range(0, Grid.HEIGHT)) + new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)) * Grid.SCALE;
        
        if (baseAn.maxWaterDistance > 0)
        {
            if (Nature.shore.Count != 0)
            {
                Node water = new List<Node>(Nature.shore)[Random.Range(0, Nature.shore.Count)];
                worldPos = Grid.ToWorld(water.gridX + Random.Range(-baseAn.maxWaterDistance, baseAn.maxWaterDistance), water.gridY + Random.Range(-baseAn.maxWaterDistance, baseAn.maxWaterDistance));
            }
        }
        float smph = Terrain.activeTerrain.SampleHeight(worldPos);
        worldPos.y = Terrain.activeTerrain.transform.position.y + smph;

        GameAnimal toSpawn = new GameAnimal(baseAn);
        toSpawn.SetPosition(worldPos);
        toSpawn.SetRotation(Quaternion.Euler(0, Random.Range(0, 360), 0));
        UnitManager.SpawnAnimal(toSpawn);
    }

    private void AddStarterPeople()
    {
        /*PersonData p = RandomPerson(Gender.Male, Random.Range(20, 30));
        UnitManager.SpawnPerson(p);
        p = RandomPerson(Gender.Female, Random.Range(20, 30));
        UnitManager.SpawnPerson(p);*/

        PersonData myPerson = RandomPerson(Gender.Male, 20, -1);
        myPerson.firstName = GameManager.Username;
        myPerson.SetPosition(Grid.SpawnpointNode.transform.position + new Vector3(2,0,1)*Grid.SCALE);
        myPerson.SetRotation(Quaternion.Euler(0,90,0));
        UnitManager.SpawnPerson(myPerson);

        myPerson = RandomPerson(Gender.Female, 22, -1);
        myPerson.firstName = GameManager.Username;
        myPerson.SetPosition(Grid.SpawnpointNode.transform.position + new Vector3(2, 0, -1) * Grid.SCALE);
        myPerson.SetRotation(Quaternion.Euler(0, 90, 0));
        UnitManager.SpawnPerson(myPerson);

        for(int i = 0; i < wildPeopleSpawnpointsParent.childCount; i++)
        {
            Transform trf = wildPeopleSpawnpointsParent.GetChild(i);
            int groupSize = Random.Range(-2, 2) + wildPeopleGroupSize;
            for(int j = 0; j < groupSize; j++)
            {
                // Add a wild person
                myPerson = RandomPerson(Gender.Male, Random.Range(20,40), -1);
                myPerson.firstName = PersonScript.RandomMaleName();
                Vector3 pos;
                int count = 0;
                do
                {
                    pos = trf.position + new Vector3(Random.Range(-3f, 3f), 0, Random.Range(-3f, 3f)) * Grid.SCALE;
                    count++;
                } while (Grid.GetNodeFromWorld(pos).IsOccupied() && count < 100);
                if (count == 100) Debug.Log("no spawning");
                myPerson.SetPosition(pos);
                myPerson.SetRotation(Quaternion.Euler(0, Random.Range(0,360), 0));
                myPerson.wild = true;
                UnitManager.SpawnPerson(myPerson);
            }
        }
    }
    public PersonData PersonBirth(int motherNr, Gender gend, int age)
    {
        PersonData p = RandomPerson(gend, age, motherNr);
        UnitManager.SpawnPerson(p);
        ChatManager.Msg(p.firstName + " ist gerade geboren!");
        NewPersonFaith();
        return p;
    }
    public PersonData PersonBirth(int motherNr)
    {
        return PersonBirth(motherNr, (Gender)Random.Range(0, 2), 0);
    }
    private PersonData RandomPerson()
    {
        return RandomPerson((Gender)Random.Range(0, 2), Random.Range(0,20), -1);
    }
    private PersonData RandomPerson(Gender gend, int age, int motherNr)
    {
        PersonData p = new PersonData();
        p.gender = gend;
        p.motherNr = motherNr;
        p.firstName = p.gender == Gender.Male ? PersonScript.RandomMaleName() : PersonScript.RandomFemaleName();
        p.lastName = PersonScript.RandomLastName();
        p.age = age;
        p.jobID = 0;
        p.health = 100;
        p.hunger = 100;
        p.disease = Disease.None;

        // lifetime expectancy between 45 and 55 years
        p.lifeTimeYears = Random.Range(45,55);
        p.lifeTimeDays = Random.Range(0,365);
        
        PersonScript mother = PersonScript.Identify(motherNr);
        if (mother)
        {
            p.SetPosition(mother.transform.position + new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)) * Grid.SCALE);
        }
        else
        {
            Node spawnNode;
            int counter = 0;
            Node center = Grid.SpawnpointNode;
            do
            {
                spawnNode = Grid.GetNode(center.gridX + Random.Range(-4, 4), center.gridY + Random.Range(-4, 4));
                if ((counter++) > 1000) break;

            } while (spawnNode.IsOccupied() || spawnNode.IsPeopleOccupied());
            p.SetPosition(Grid.ToWorld(spawnNode.gridX, spawnNode.gridY));
        }
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
        for (int i = 0; i < 500; i++)
        {
            int range = Mathf.Min(Grid.SpawnX,i/4+4);
            x = UnityEngine.Random.Range(Grid.SpawnX - range, Grid.SpawnY + range);
            y = UnityEngine.Random.Range(Grid.SpawnX - range, Grid.SpawnY + range);
            if (Grid.GetNode(x, y).IsOccupied()) continue;
            if (itemInNode[x, y]) continue;

            itemInNode[x, y] = true;

            int id = Random.Range(0,2);
            ItemManager.SpawnItem(id, Random.Range(1,3), Grid.ToWorld(x,y), 0.8f, 0.8f);
        }
    }

    public Transform GetNearestPlant(Vector3 position, NatureObjectType type, float range, bool notEmpty)
    {
        if (Nature.nature.Count == 0) return null;
        Transform nearestTree = null;
        float dist = float.MaxValue;
        foreach (NatureObjectScript NatureObjectScript in Nature.nature)
        {
            if (NatureObjectScript && NatureObjectScript.Type == type && NatureObjectScript.gameObject.activeSelf && (!notEmpty || NatureObjectScript.ResourceCurrent.Amount > 0))
            {
                float temp = Vector3.Distance(NatureObjectScript.transform.position, position);
                if (temp < dist)
                {
                    dist = temp;
                    nearestTree=NatureObjectScript.transform;
                }
            }
        }
        if (dist > range) return null;
        //if (nearestTree.GetComponent<NatureObjectScript>().GetPlantType() != NatureObjectType.Tree) return null;
        return nearestTree;
    }
    public Transform GetNearestItemInRange(Vector3 position, int resId, float range)
    {
        return GetNearestItemInRange(position, new GameResources(resId), range);
    }
    public Transform GetNearestItemInRange(Vector3 position, GameResources res, float range)
    {
        Transform nearestItem = null;
        float dist = float.MaxValue;
        foreach (ItemScript it in ItemScript.allItemScripts)
        {
            if (it.ResId == res.Id && it.gameObject.activeSelf)
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
    public Transform GetNearestItemInRange(Vector3 position, float range)
    {
        Transform nearestItem = null;
        float dist = float.MaxValue;
        foreach (ItemScript it in ItemScript.allItemScripts)
        {
            if (it.gameObject.activeSelf)
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
    public BuildingScript GetNearestBuildingNoTask(Vector3 position)
    {
        BuildingScript nearestBuilding = null;
        float dist = float.MaxValue;
        foreach (BuildingScript bs in BuildingScript.allBuildingScripts)
        {
            if (bs.Blueprint) continue;
            // check if building has capacity for no task people
            if (bs.NoTaskCurrent < bs.NoTaskCapacity)
            {
                float temp = Vector3.Distance(bs.transform.position, position);
                if (temp < dist)
                {
                    dist = temp;
                    nearestBuilding = bs;
                }
            }
        }
        return nearestBuilding;
    }
    public BuildingScript GetNearestBuildingType(Vector3 position, BuildingType type)
    {
        BuildingScript nearestStorage = null;
        float dist = float.MaxValue;
        foreach (BuildingScript bs in BuildingScript.allBuildingScripts)
        {
            if (bs.Blueprint) continue;
            if (bs.Type == type)
            {
                float temp = Vector3.Distance(bs.transform.position, position);
                if (temp < dist)
                {
                    dist = temp;
                    nearestStorage = bs;
                }
            }
        }
        return nearestStorage;
    }
    public BuildingScript GetNearestBuildingID(Vector3 position, int id)
    {
        BuildingScript nearestStorage = null;
        float dist = float.MaxValue;
        foreach (BuildingScript bs in BuildingScript.allBuildingScripts)
        {
            if (bs.Blueprint) continue;
            if (bs.Id == id)
            {
                float temp = Vector3.Distance(bs.transform.position, position);
                if (temp < dist)
                {
                    dist = temp;
                    nearestStorage = bs;
                }
            }
        }
        return nearestStorage;
    }
    public BuildingScript GetNearestBuildingBlueprint(Vector3 position)
    {
        BuildingScript nearestStorage = null;
        float dist = float.MaxValue;
        foreach (BuildingScript bs in BuildingScript.allBuildingScripts)
        {
            if (!bs.Blueprint) continue;
            float temp = Vector3.Distance(bs.transform.position, position);
            if (temp < dist)
            {
                dist = temp;
                nearestStorage = bs;
            }
        }
        return nearestStorage;
    }
    public BuildingScript GetNearestStorageBuilding(Vector3 position, int resId, bool checkFull, bool checkEmpty)
    {
        BuildingScript nearestStorage = null;
        float dist = float.MaxValue;
        foreach (BuildingScript bs in BuildingScript.allBuildingScripts)
        {
            // you can store items in storage buildings and crafting buildings
            if (bs.Blueprint || (bs.Type != BuildingType.Storage && bs.Type != BuildingType.Crafting)) continue;
            if(bs.Name == "Jagdhütte") continue;
            if ((bs.GetStorageFree(resId) > 0 || !checkFull) && (bs.GetStorageCurrent(ResourceData.Name(resId)) > 0 || !checkEmpty))
            {
                float temp = Vector3.Distance(bs.transform.position, position);
                if (temp < dist)
                {
                    dist = temp;
                    nearestStorage = bs;
                }
            }
        }
        return nearestStorage;
    }
    public BuildingScript GetNearestHutJob(Vector3 position, Job familyJob)
    {
        BuildingScript nearestStorage = null;
        float dist = float.MaxValue;
        foreach (BuildingScript bs in BuildingScript.allBuildingScripts)
        {
            // you can store items in storage buildings and crafting buildings
            if (bs.Blueprint || !bs.IsHut()) continue;
            if (bs.FamilyJobId == familyJob.id)
            {
                float temp = Vector3.Distance(bs.transform.position, position);
                if (temp < dist)
                {
                    dist = temp;
                    nearestStorage = bs;
                }
            }
        }
        return nearestStorage;
    }
    public BuildingScript GetNearestFieldOfHut(BuildingScript hut)
    {
        BuildingScript nearestField = null;
        float dist = float.MaxValue;
        foreach (BuildingScript bs in BuildingScript.allBuildingScripts)
        {
            if (bs.Blueprint || bs.Type != BuildingType.Field || bs.ParentBuildingNr != hut.Nr) continue;

            float temp = Vector3.Distance(bs.transform.position, hut.transform.position);
            if (temp < dist)
            {
                dist = temp;
                nearestField = bs;
            }
        }
        return nearestField;
    }
    public AnimalScript GetNearestAnimal(Vector3 position, int animalId)
    {
        AnimalScript nearestAnimal = null;
        float dist = float.MaxValue;
        foreach (AnimalScript anis in AnimalScript.allAnimals)
        {
            // you can store items in storage buildings and crafting buildings
            if (anis.IsDead() || !anis.gameObject.activeInHierarchy) continue;
            if (anis.Id == animalId)
            {
                float temp = Vector3.Distance(anis.transform.position, position);
                if (temp < dist)
                {
                    dist = temp;
                    nearestAnimal = anis;
                }
            }
        }
        return nearestAnimal;
    }

    // returns if pos is in build range of cave
    public bool InBuildRange(Vector3 pos)
    {
        return GameManager.InRange(BuildManager.Instance.cave.transform.position, pos, BuildManager.Instance.cave.BuildRange);
    }

    public static void UnlockBuilding(Building b)
    {
        if (b.id != -1 && !Building.IsUnlocked(b.id))
        {
            Building.Unlock(b.id);
            ChatManager.Msg("Neues Gebäude freigeschalten: "+b.name);
            UIManager.Instance.Blink("ButtonBuild", true);
        }
    }

    // When a building is finished, trigger event
    public void FinishBuildEvent(Building b)
    {
        int unlockedBuilding = -1;
        int unlockedJob = -1;
        unlockedBuilding = b.unlockBuildingID;
        unlockedJob = b.jobId;
        if (unlockedBuilding >= Building.Count) unlockedBuilding = -1;
        if(unlockedJob >= Job.Count || unlockedJob == 0) unlockedJob = -1;

        if(unlockedJob != -1 && !Job.IsUnlocked(unlockedJob))
        {
            Job.Unlock(unlockedJob);
            Job nj = Job.Get(unlockedJob);
            ChatManager.Msg("Neuen Beruf freigeschalten: "+nj.name);
        }
        UnlockBuilding(Building.Get(unlockedBuilding));

        if (b.name == "Opferstätte")
        {
            if (AltarCount() == 1 && !UIManager.Instance.IsFaithBarEnabled()) // initial altar
            {
                faithPoints = 10;
            }
            UIManager.Instance.EnableFaithBar();
            UIManager.Instance.Blink("PanelTopFaith", true);
        }
        if (b.name == "Tüftler")
        {
            UIManager.Instance.EnableTechTree();
            UIManager.Instance.Blink("PanelTopTechTree", true);
        }

        foreach (NatureObjectScript p in Nature.nature)
        {
            if(p && p.gameObject.activeSelf)
                p.UpdateBuildingViewRange();
        }
        foreach(ItemScript its in ItemScript.allItemScripts)
        {
            if(its && its.gameObject.activeSelf)
                its.UpdateBuildingViewRange();
        }

        GameManager.UpdateAchievementBuilding(b);
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
