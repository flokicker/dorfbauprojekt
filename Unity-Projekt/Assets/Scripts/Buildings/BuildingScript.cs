using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildingScript : MonoBehaviour
{
    // Collection of all buildings
    public static HashSet<BuildingScript> allBuildingScripts = new HashSet<BuildingScript>();

    // Reference to the clickableObject script
    private ClickableObject co;

    // Blueprint
    private Material[] buildingMaterial, blueprintMaterial;
    private Transform blueprintCanvas, rangeCanvas;
    private Image rangeImage;
    private List<Transform> panelMaterial;
    private List<Text> textMaterial;

    private MeshRenderer meshRenderer;
    private Collider myColliderPhysics, myColliderMouse;

    private Transform currentModel, centerTransform;

    public int Id
    {
        get { return Building.id; }
    }
    public string Name
    {
        get { return Building.name; }
    }
    public BuildingType Type
    {
        get { return Building.type; }
    }
    public string Description
    {
        get { return Building.description; }
    }
    public int Cost
    {
        get { return Building.cost; }
    }
    public List<GameResources> CostResource
    {
        get { return Building.costResource[Stage].list; }
    }
    public List<GameResources> Storage
    {
        get
        {
            int stg = Stage;
            while(stg >= Building.storage.Count)
            {
                stg--;
            }
            if (stg < 0) return new List<GameResources>();
            return Building.storage[stg].list;
        }
    }
    public List<GameResources> StorageCurrent
    {
        get { return gameBuilding.resourceCurrent; }
    }
    public int JobId
    {
        get { return Building.jobId; }
    }
    public int Workspace
    {
        get { return Building.workspace; }
    }
    public int[] PopulationRoom
    {
        get { return Building.populationRoom; }
    }
    public int NoTaskCapacity
    {
        get { return Building.noTaskCapacity; }
    }
    public int GridWidth
    {
        get { return Building.gridWidth; }
    }
    public int GridHeight
    {
        get { return Building.gridHeight; }
    }
    public bool HasEntry
    {
        get { return Building.hasEntry; }
    }
    public int EntryDx
    {
        get { return Building.entryDx; }
    }
    public int EntryDy
    {
        get { return Building.entryDy; }
    }
    public int ViewRange
    {
        get { return Building.viewRange; }
    }
    public int FoodRange
    {
        get { return Building.foodRange; }
    }
    public int BuildRange
    {
        get { return Building.buildRange; }
    }
    public bool Walkable
    {
        get { return Building.walkable; }
    }
    public bool Movable
    {
        get { return Building.movable; }
    }
    public bool Destroyable
    {
        get { return Building.destroyable; }
    }
    public int MaxStages
    {
        get { return Building.maxStages; }
    }
    public float CollisionRadius
    {
        get { return Building.collisionRadius; }
    }
    public float SelectionCircleRadius
    {
        get { return Building.selectionCircleRadius; }
    }
    public bool MultipleBuildings
    {
        get { return Building.multipleBuildings; }
    }
    public bool HasFire
    {
        get { return Building.hasFire; }
    }
    public int UnlockBuildingID
    {
        get { return Building.unlockBuildingID; }
    }
    public int FieldBuildingID
    {
        get { return Building.fieldBuildingId; }
    }
    public int StorageBuildingID
    {
        get { return Building.storageBuildingId; }
    }
    public Sprite Icon
    {
        get { return Building.icon; }
    }
    public Building Building
    {
        get { return gameBuilding.building; }
    }

    // how many fields can you build with the current stage
    public int FieldBuildCount
    {
        get { return (Stage + 1) / 2; }
    }
    public int StorageBuildCount
    {
        get { return (Stage) / 2; }
    }
    public int FieldPlantCount
    {
        get { return 5; }
    }
    public int FieldResPerPlant
    {
        get { return 30; }
    }
    public int SeedTime
    {
        get { return (Application.isEditor && GameManager.IsDebugging()) ? 2 : 1 * 60; }
    }
    public int SeedWaitTime
    {
        get { return (Application.isEditor && GameManager.IsDebugging()) ? 1 : (int)(0.5f * 60); }
    }
    public int GrowTime
    {
        get { return (Application.isEditor && GameManager.IsDebugging()) ? 20 : 5 * 60; }
    }
    public int FieldPlantLifeSpan
    {
        get { return (Application.isEditor && GameManager.IsDebugging()) ? 5 : 5 * 60; }
    }
    public int FieldPlantRotTime
    {
        get { return (Application.isEditor && GameManager.IsDebugging()) ? 20 : 2 * 60; }
    }

    public int LuxuryFactor
    {
        get { return Type == BuildingType.Luxury ? 10 : 0; }
    }
    public int HealthFactor
    {
        get { return Type == BuildingType.Luxury ? 10 : 0; }
    }
    public bool HasLifebar
    {
        get { return HasFire; }
    }
    public float LifebarFactor
    {
        get { Campfire cf = GetComponent<Campfire>();
            if (cf) return cf.GetHealthFactor();
            else return 0;
        }
    }

    public int Nr
    {
        get { return gameBuilding.nr; }
    }
    public bool Blueprint
    {
        get { return gameBuilding.blueprint; }
    }
    public List<GameResources> BlueprintBuildCost
    {
        get { return gameBuilding.blueprintBuildCost; }
    }
    public int GridX
    {
        get { return gameBuilding.gridX; }
    }
    public int GridY
    {
        get { return gameBuilding.gridY; }
    }
    public int Orientation
    {
        get { return gameBuilding.orientation; }
    }
    public int Stage
    {
        get { return gameBuilding.stage; }
    }
    public int NoTaskCurrent
    {
        get { return gameBuilding.noTaskCurrent; }
    }
    public int PopulationCurrent
    {
        get { return gameBuilding.populationCurrent; }
    }
    /*public int FamilyJobId
    {
        get { return gameBuilding.familyJobId; }
    }*/
    public int ParentBuildingNr
    {
        get { return gameBuilding.parentBuildingNr; }
    }
    public List<int> ChildBuildingField
    {
        get { return gameBuilding.childBuildingField; }
    }
    public List<int> ChildBuildingStorage
    {
        get { return gameBuilding.childBuildingStorage; }
    }
    public int FieldResource
    {
        get { return gameBuilding.fieldResource; }
    }
    public List<int> WorkingPeople
    {
        get { return gameBuilding.workingPeople; }
    }

    public float processProgress;

    private GameBuilding gameBuilding;

    private NatureObject fieldPlant;
    public List<Transform> fieldPlantObjects = new List<Transform>();
    private float fieldPlantGrowTimer = 0;

    public Lake nearestLake;

    public BuildingScript replacing;

    // PRE: Building and game building have to be set before strat is called
    private void Start()
    {
        // set nr of building if not already given by game building
        if (gameBuilding.nr == -1)
        {
            gameBuilding.nr = allBuildingScripts.Count;
            BuildingScript par = Identify(ParentBuildingNr);
            if (par) par.AddChildBuilding(this);
        }
        // Update allBuildings collection
        allBuildingScripts.Add(this);
        tag = Building.Tag;

        // make building a clickable object
        //co = gameObject.AddComponent<ClickableObject>();
        // co.clickable = false;
        
        if (currentModel == null)
        {
            for (int i = 0; i < MaxStages; i++)
                transform.GetChild(i).gameObject.SetActive(false);
            SetCurrentModel();
        }

        // Add and disable Campfire script
        if (HasFire)
        {
            Campfire cf = gameObject.AddComponent<Campfire>();
            cf.enabled = !Blueprint;
            cf.maxIntensity = Mathf.Clamp(GridWidth, 1, 1.8f);
        }

        fieldPlant = NatureObject.Get("Korn");

        // init blueprint UI
        blueprintCanvas = transform.Find("CanvasBlueprint");
        panelMaterial = new List<Transform>();
        textMaterial = new List<Text>();
        for (int i = 0; i < blueprintCanvas.Find("Cost").childCount; i++)
        {
            Transform pm = blueprintCanvas.Find("Cost").GetChild(i);
            panelMaterial.Add(pm);
            textMaterial.Add(pm.Find("TextMat").GetComponent<Text>());
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(blueprintCanvas.GetComponent<RectTransform>());
        buildingMaterial = meshRenderer.materials;
        blueprintMaterial = new Material[buildingMaterial.Length];
        for (int i = 0; i < buildingMaterial.Length; i++)
            blueprintMaterial[i] = BuildManager.Instance.blueprintMaterial;
        
        // Finish building if no costs
        if (BlueprintBuildCost.Count == 0) FinishBuilding();
        
        //blueprintCanvas.gameObject.SetActive(false);
        ChangeTerrainGround();

        blueprintCanvas.gameObject.SetActive(false);

        // init range canvas
        rangeCanvas = transform.Find("CanvasRange").transform;
        rangeImage = rangeCanvas.Find("Image").GetComponent<Image>();

        // Make selected person go build this building
        PersonScript ps = PersonScript.FirstSelectedPerson();
        if (ps) ps.AddTargetTransform(transform, true);

        // if building has a custom collision radius, disable default collider and add a capsule collider
        /*if (CollisionRadius > float.Epsilon)
        {
            gameObject.AddComponent<CapsuleCollider>();
            myCollider.enabled = false;
        }*/


        if(ViewRange > 0)
        {
            foreach (NatureObjectScript p in Nature.nature)
            {
                p.UpdateBuildingViewRange();
            }
            foreach (ItemScript p in ItemScript.allItemScripts)
            {
                p.UpdateBuildingViewRange();
            }
            foreach (PersonScript p in PersonScript.wildPeople)
            {
                p.UpdateBuildingViewRange();
            }
            foreach (AnimalScript a in AnimalScript.allAnimals)
            {
                a.UpdateBuildingViewRange();
            }

            gameObject.AddComponent<SimpleFogOfWar.FogOfWarInfluence>().ViewDistance = ViewRange;
        }
        //recruitingTroop = new List<Troop>();
    }
    private void Update()
    {
        if (replacing != null)
        {
            Destroy(replacing.gameObject);
            UIManager.Instance.OnShowObjectInfo(transform);
            replacing = null;
        }

        if (FieldSeeded() || FieldGrown()) UpdateFieldTime();
        if (Building.inWater) GetComponent<Renderer>().enabled = false;

        // update nearest lake
        if (nearestLake == null)
            ResetNearestLake();

        co.highlightable = !Blueprint && Type != BuildingType.Path;

        co.SetSelectionCircleRadius(SelectionCircleRadius > float.Epsilon ? SelectionCircleRadius : Mathf.Max(GridWidth, GridHeight)*0.6f);

        // update transform position rotation on save object
        gameBuilding.SetTransform(transform);

        // only clickable, if not in blueprint mode
        co.clickable = true;// !Blueprint;
        myColliderPhysics.isTrigger = HasEntry ? true : Walkable || Blueprint;
        if(myColliderMouse != myColliderPhysics)
            myColliderMouse.isTrigger = true;

        if (Type == BuildingType.Field)
        {
            if (FieldSeedWaited() && !FieldGrown())
            {
                if (fieldPlantObjects.Count < CurrentPlantCounts())
                {
                    float fx = Random.Range(-0.2f, 0.2f) * GridWidth;
                    float fy = Random.Range(-0.2f, 0.2f) * GridHeight;
                    GameObject plant = Instantiate(fieldPlant.models[0], transform.position + new Vector3(fx, 0, fy) * Grid.SCALE, Quaternion.identity, transform);
                    plant.transform.localScale = Vector3.one * 0.05f;
                    fieldPlantObjects.Add(plant.transform);
                }

                fieldPlantGrowTimer += Time.deltaTime;
                if (fieldPlantGrowTimer >= 1f)
                {
                    foreach (Transform trf in fieldPlantObjects)
                    {
                        if (trf.localScale.x < 1)
                        {
                            trf.localScale += Vector3.one * fieldPlantGrowTimer * (CurrentPlantCounts() / (float)GrowTime) * Random.Range(0.9f, 1.1f);
                        }
                    }
                    fieldPlantGrowTimer = 0;
                }
            }
            else if(FieldGrown() && gameBuilding.fieldResource == 0 &&  !FieldRotting())
            {
                gameBuilding.fieldResource = FieldPlantCount * FieldResPerPlant;
            }
            else if(FieldGrown())
            {
                if(FieldFullyRotted())
                {
                }
                else if(FieldRotting())
                {
                    fieldPlantGrowTimer += Time.deltaTime;
                    if (fieldPlantGrowTimer >= FieldPlantRotTime / (float)(FieldPlantCount * FieldResPerPlant))
                    {
                        fieldPlantGrowTimer -= FieldPlantRotTime / (float)(FieldPlantCount * FieldResPerPlant);
                        if (gameBuilding.fieldResource > 0)
                        gameBuilding.fieldResource--;
                    }

                    if (fieldPlantObjects.Count > CurrentPlantCounts())
                    {
                        if (fieldPlantObjects.Count > 0)
                        {
                            Destroy(fieldPlantObjects[0].gameObject);
                            fieldPlantObjects.RemoveAt(0);
                        }
                    }
                }
            }
        }
        
        if (HasFire)
        {
            gameObject.GetComponent<Campfire>().maxWood = GetStorageTotal(new GameResources("Holz"));
        }

        UpdateRangeView();
        UpdateBlueprint();
        UpdateRecruitingTroop();
        UpdateSacrifice();
    }
    private void LateUpdate()
    {

        // outline for moving building
        if (BuildManager.Instance.movingBuilding == this)
        {
            co.SetOutline(true);
        }
    }

    // Update methods
    private void UpdateRangeView()
    {
        if (UIManager.Instance.GetSelectedBuilding() == this)// || BuildManager.placing)
        {
            int range = 0;
            if (Name == "Höhle") range = ViewRange;
            if (IsHut() && JobId != 0) range = FoodRange;

            rangeCanvas.gameObject.SetActive(range != 0);
            /* TODO: replace with circle on ground */
            rangeImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, range * 20 + 1 + (GridWidth % 2 == 0 ? 0 : 10));
            rangeImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, range * 20 + 1 + (GridHeight % 2 == 0 ? 0 : 10));
        }
        else rangeCanvas.gameObject.SetActive(false);
    }
    private void UpdateBlueprint()
    {
        if (!Blueprint)
        {
            // set materials for blueprint
            if (meshRenderer.materials != buildingMaterial)
                meshRenderer.materials = buildingMaterial;
            return;
        }
        
        // check if already fully built
        int requiredCost = 0;
        foreach (GameResources r in gameBuilding.blueprintBuildCost)
            requiredCost += r.Amount;
        if (requiredCost == 0)
        {
            FinishBuilding();
            blueprintCanvas.gameObject.SetActive(false);
        }
        else
        {
            Camera camera = Camera.main;
            //blueprintCanvas.gameObject.SetActive(co.outlined);
            blueprintCanvas.LookAt(blueprintCanvas.position + camera.transform.rotation * Vector3.forward * 0.0001f, camera.transform.rotation * Vector3.up);
            if (BlueprintBuildCost.Count > 0)
            {
                int panelChildIndex = 0;
                foreach(GameResources res in BlueprintBuildCost)
                {
                    int totCost = GetCostResource(res);
                    panelMaterial[panelChildIndex].gameObject.SetActive(res.Amount > 0);
                    textMaterial[panelChildIndex].text = (totCost - res.Amount) + "/" + totCost;
                    panelChildIndex++;
                }
            }
        }

        // set materials for blueprint
        if (meshRenderer.materials[0] != BuildManager.Instance.blueprintMaterial)
            meshRenderer.materials = blueprintMaterial;
    }
    private void UpdateRecruitingTroop()
    {
        /*if (recruitingTroop.Count > 0)
        {
            recruitingTroop[0].recruitingTime -= Time.deltaTime;
            if (recruitingTroop[0].recruitingTime <= 0)
            {
                recruitingTroop[0].recruitingTime = 0;
                recruitingTroop.RemoveAt(0);
                // TODO: spawn a troop  
            }
        }*/
    }
    private void UpdateSacrifice()
    {
        if (Type != BuildingType.Religion) return;

        // sacrifice all stored items
        foreach(GameResources res in StorageCurrent)
        {
            if(res.Amount > 0)
            {
                GameManager.village.AddFaithPoints(res.Nutrition);
                ChatManager.Msg("Du hast " + res.Amount + " " + res.Name + " geopfert", MessageType.Info);
                res.Take(res.Amount);
            }
        }
    }

    public void RemoveTerrainGround()
    {
        if (Blueprint) return;

        if (Type == BuildingType.Path)
        {
            TerrainModifier.ChangeTexture(GridX, GridY, 1, 1, TerrainTextureType.Grass);
        }
        else
        {
            TerrainModifier.ChangeTexture(GridX, GridY, RotWidth(), RotHeight(), Type == BuildingType.Field ? TerrainTextureType.Field : TerrainTextureType.Building, -0.8f, -0.2f);
        }
    }
    public void ChangeTerrainGround()
    {
        if (!Blueprint)
        {
            if (Type == BuildingType.Path)
            {
                meshRenderer.enabled = false;
                TerrainModifier.ChangeTexture(GridX, GridY, 1, 1, TerrainTextureType.Path);
            }
            else
            {
                TerrainModifier.ChangeTexture(GridX, GridY, RotWidth(), RotHeight(), Type == BuildingType.Field ? TerrainTextureType.Field : TerrainTextureType.Building);
            }
        }

        TerrainModifier.ChangeGrass(GridX, GridY, RotWidth(), RotHeight(), false);
    }

    private void FinishBuilding()
    {
        // Disable blueprint
        meshRenderer.materials = buildingMaterial;
        gameBuilding.blueprint = false;
        // Dsiable blueprint UI
        blueprintCanvas.gameObject.SetActive(false);

        // Enable Campfire script
        if (HasFire)
        {
            gameObject.GetComponent<Campfire>().enabled = true;
        }
        ChangeTerrainGround();

        // Trigger unlock/achievement event
        GameManager.village.FinishBuildEvent(Building);
    }

    // Resource getters
    public List<GameResources> GetCostResource(int stage)
    {
        if (stage >= Building.costResource.Count) return new List<GameResources>();
        return Building.costResource[stage].list;
    }
    public int GetCostResource(GameResources res)
    {
        foreach (GameResources cost in CostResource)
            if (cost.Id == res.Id) return cost.Amount;
        return 0;
    }
    public int GetStorageTotal(GameResources res)
    {
        foreach (GameResources stor in Storage)
            if (stor.Id == res.Id) return stor.Amount;
        return 0;
    }
    public int GetStorageCurrent(GameResources res)
    {
        foreach (GameResources stor in StorageCurrent)
            if (stor.Id == res.Id) return stor.Amount;
        return 0;
    }
    public int GetStorageCurrent(string resName)
    {
        foreach (GameResources stor in StorageCurrent)
            if (stor.Name == resName) return stor.Amount;
        return 0;
    }
    public int GetStorageFree(GameResources res)
    {
        return GetStorageTotal(res) - GetStorageCurrent(res);
    }
    public int GetStorageFree(int resId)
    {
        return GetStorageFree(new GameResources(resId));
    }
    public void Restock(GameResources res)
    {
        foreach (GameResources r in StorageCurrent)
        {
            if (r.Id == res.Id)
            {
                r.Add(res.Amount);
                return;
            }
        }
        gameBuilding.resourceCurrent.Add(new GameResources(res));
    }
    public void Take(GameResources res)
    {
        foreach (GameResources r in StorageCurrent)
        {
            if (r.Id == res.Id)
            {
                r.Take(res.Amount);
                return;
            }
        }
    }

    public bool DoesProduceResource(GameResources res)
    {
        if (IsHut())
        {
            if (JobId == Job.Id("Jäger") && res.Name == "Fleisch") return true;
            if (JobId == Job.Id("Bauer") && res.Name == "Brot") return true;
            if (JobId == Job.Id("Fischer") && res.Name == "Fisch") return true;
        }
        return false;
    }
    public bool DoesProcessResource(GameResources res)
    {
        if(IsHut())
        {
            if (JobId == Job.Id("Jäger") && res.Type == ResourceType.DeadAnimal) return true;
            if (JobId == Job.Id("Bauer") && res.Name == "Korn") return true;
            if (JobId == Job.Id("Fischer") && res.Name == "Roher Fisch") return true;
        }
        return false;
    }

    public int GetPopulationRoom()
    {
        return PopulationRoom[Stage];
    }
    public int GetPopulationFree()
    {
        return PopulationRoom[Stage] - PopulationCurrent;
    }
    public void SetPopulation(int pop)
    {
        gameBuilding.populationCurrent = pop;
    }

    /*public void SetFamilyJob(Job job)
    {
        gameBuilding.familyJobId = job.id;
    }*/
    public void SetBuilding(GameBuilding gameBuilding)
    {
        this.gameBuilding = gameBuilding;
    }
    public void SetPosRot(int gridX, int gridY, int orientation)
    {
        gameBuilding.gridX = gridX;
        gameBuilding.gridY = gridY;
        gameBuilding.orientation = orientation;
    }

    public int CurrentPlantCounts()
    {
        if(FieldRotting())
        {
            return Mathf.Min(FieldPlantCount, (int)(((1f - FieldRotPerc()) * (FieldPlantCount + 1.5f))));
        }
        return Mathf.Min(FieldPlantCount, (int)((FieldGrowPerc() * (FieldPlantCount+1)))+1);
    }
    public bool FieldSeeded()
    {
        return gameBuilding.fieldTime >= SeedTime;
    }
    public bool FieldSeedWaited()
    {
        return gameBuilding.fieldTime >= SeedTime + SeedWaitTime;
    }
    public bool FieldGrown()
    {
        return gameBuilding.fieldTime >= SeedTime + SeedWaitTime + GrowTime;
    }
    public bool FieldRotting()
    {
        return gameBuilding.fieldTime >= SeedTime + SeedWaitTime + GrowTime + FieldPlantLifeSpan;
    }
    public bool FieldFullyRotted()
    {
        return gameBuilding.fieldTime >= SeedTime + SeedWaitTime + GrowTime + FieldPlantLifeSpan + FieldPlantRotTime;
    }
    public float FieldRotPerc()
    {
        return (gameBuilding.fieldTime - SeedWaitTime - SeedTime - GrowTime - FieldPlantLifeSpan) / FieldPlantRotTime;
    }
    public float FieldGrowPerc()
    {
        return (gameBuilding.fieldTime - SeedWaitTime - SeedTime) / GrowTime;
    }
    public float FieldSeedPerc()
    {
        return gameBuilding.fieldTime / SeedTime;
    }
    public int HarvestField()
    {
        int amount = Mathf.Min(1, FieldResource);
        if (FieldResource <= 1)
        {
            foreach (Transform trf in fieldPlantObjects)
            {
                Destroy(trf.gameObject);
            }
            fieldPlantObjects.Clear();
            StartSeeding();

            if (FieldResource == 0)
                return 0;
        }
        else if(FieldResource % FieldResPerPlant == FieldResPerPlant / 2)
        {
            if (fieldPlantObjects.Count > 0)
            {
                Destroy(fieldPlantObjects[0].gameObject);
                fieldPlantObjects.RemoveAt(0);
            }
        }
        gameBuilding.fieldResource -= amount;
        return amount;
    }
    public void UpdateFieldTime()
    {
        if(FieldGrown() || GameManager.GetTwoSeason() == 1)
            gameBuilding.fieldTime += Time.deltaTime;
    }
    public void StartSeeding()
    {
        gameBuilding.fieldTime = 0;
    }

    private void ResetNearestLake()
    {
        float dist = float.MaxValue;
        nearestLake = null;
        foreach(Transform lake in Nature.Instance.lakeParent)
        {
            float temp = Vector3.Distance(lake.transform.position, transform.position);
            if (temp < dist)
            {
                nearestLake = lake.GetComponent<Lake>();
                dist = temp;
            }
        }
    }

    // text displayed on UI Building Information
    public string TotalDescription()
    {
        string desc = Description;
        if (Blueprint) return desc;

        if (Type == BuildingType.Field)
        {
            if (Name == "Kornfeld")
            {
                if (FieldFullyRotted())
                {
                    desc = "Dein Kornfeld ist verrottet!";
                }
                else if (FieldRotting())
                {
                    desc = "Bereit zur Ernte\n" + FieldResource + " Korn\n(verrottet)";
                }
                else if (FieldGrown())
                {
                    desc = "Bereit zur Ernte\n" + FieldResource + " Korn";
                }
                else if (FieldSeedWaited())
                {
                    desc = "Korn wächst\n" + (int)(100f * FieldGrowPerc()) + "%";
                }
                else if (FieldSeeded())
                {
                    desc = "Korn wächst bald...";
                }
                else if(FieldSeedPerc() <= 0.001f)
                {
                    desc = "Feld bereit zur Aussaht";
                }
                else
                {
                    desc = "Korn aussähen\n" + (int)(100f * FieldSeedPerc()) + "%";
                }

                if (GameManager.GetTwoSeason() == 0)
                {
                    desc += "\nIm Winter wächst hier kein Korn!";
                }
            }
            else if(Name == "Fischerbereich")
            {
                desc += "\n"+nearestLake.currentFish + " Fische";
            }
        }
        else if (IsHut())
        {
            if (JobId != 0)
            {
                desc = "Eine " + Job.Name(JobId) + "familie wohnt hier";
                if(processProgress > float.Epsilon)
                {
                    desc += "\nVerarbeitung " + (int)(100f * Mathf.Clamp(processProgress,0f,1f)) + "%";
                }
            }
        }

        if (PopulationRoom[Stage] > 0)
        {
            desc += "\nHier wohnen: " + PopulationCurrent + "/" + PopulationRoom[Stage];
        }

        if (HasFire)
        {
            Campfire cf = gameObject.GetComponent<Campfire>();
            desc += "\nHolzkapazität: "+(int)cf.woodAmount + "/"+ (int)cf.maxWood;
        }

        if(Name == "Opferstätte")
        {
            desc = "Bietet platz für 30 Gläubige."
                    + "\nBeachte das du nicht mehr als 30 Bewohner mit Glauben versorgen kannst."
                    + "\nBietest du zu wenig Opferstätten an, fällst du bei den Göttern in Ungnade und verlierst Punkte."
                    + "\nDank deiner tüchtigen Priester, kannst du die Glaubenspunkte bei ausreichender Anzahl für einen Ressourcen - Bonus oder Technologien einsetzen.";
        }

        return desc;
    }

    public bool Employ(PersonScript ps)
    {
        if (ps == null) return false;
        gameBuilding.workingPeople.Add(ps.Nr);
        return true;
    }
    public bool Unemploy(PersonScript ps)
    {
        if (ps == null) return false;
        return gameBuilding.workingPeople.Remove(ps.Nr);
    }
    public void AddNoTaskPerson()
    {
        gameBuilding.noTaskCurrent++;
    }
    public void RemoveNoTaskPerson()
    {
        gameBuilding.noTaskCurrent--;
    }

    public bool IsHut()
    {
        return Name.ToLower().Contains("hütte");
    }

    public void DestroyAllBuildingsInList(List<int> buildingIds)
    {
        BuildingScript bs;
        foreach (int id in buildingIds)
        {
            bs = Identify(id);
            if (!bs) continue;
            bs.DestroyBuilding();
            ChatManager.Msg(bs.Name + " wurde mit abgerissen!");
            DestroyAllBuildingsInList(buildingIds);
            return;
        }
    }

    // Destroy building and set build resources free
    public void DestroyBuilding()
    {
        // only destroy building if not the starting building (cave)
        if (!Destroyable) return;

        DestroyAllBuildingsInList(ChildBuildingField);
        DestroyAllBuildingsInList(ChildBuildingStorage);

        BuildingScript bs;
        bs = Identify(ParentBuildingNr);
        if (bs)
        {
            bs.RemoveChildBuilding(this);
        }

        // make sure path on terrain is deleted
        RemoveTerrainGround();

        // resources that were needed to build, that will be set free
        List<GameResources> freeResources = new List<GameResources>();
        foreach(GameResources res in CostResource)
        {
            freeResources.Add(new GameResources(res));
        }
        if (Blueprint)
        {
            foreach (GameResources free in freeResources)
            {
                // subtract not yet built resources
                foreach (GameResources bpc in BlueprintBuildCost)
                    if (bpc.Id == free.Id)
                        free.Take(bpc.Amount);

                // spawn free resources as items
                while (free.Amount > 0)
                {
                    int am = Mathf.Min(free.Amount, Random.Range(1, 3));

                    ItemManager.SpawnItem(free.Id, am, transform.position, GridWidth, GridHeight);

                    free.Take(am);
                }
            }
        }

        Destroy(gameObject);
    }
    public void MoveBuilding()
    {
        ChangeTerrainGround();
        ResetNearestLake();
    }

    void OnDestroy()
    {
        int gx = RotWidth();
        int gy = RotHeight();
        for (int dx = 0; dx < gx; dx++)
        {
            for (int dy = 0; dy < gy; dy++)
            {
                if (!Grid.ValidNode(GridX + dx, GridY + dy)) continue;
                Node n = Grid.GetNode(GridX + dx, GridY + dy);
                if (n.nodeObject == this)
                {
                    n.SetNodeObject(null);
                    n.gameObject.SetActive(false);
                }
            }
        }

        allBuildingScripts.Remove(this);
        foreach (NatureObjectScript p in Nature.nature)
        {
            if (p) p.UpdateBuildingViewRange();
        }
        foreach (ItemScript i in ItemScript.allItemScripts)
        {
            if (i) i.UpdateBuildingViewRange();
        }
    }
    
    public bool MaxStage()
    {
        return Stage + 1 >= MaxStages;
    }
    public void NextStage()
    {
        if (MaxStage()) return;
        gameBuilding.stage++;
        SetCurrentModel();
    }
    public void SetCurrentModel()
    {
        if (currentModel) currentModel.gameObject.SetActive(false);
        currentModel = GetCurrentModel(); ;
        currentModel.gameObject.SetActive(true);
        currentModel.gameObject.layer = 12;

        centerTransform = currentModel.Find("Center");
        if (!centerTransform) centerTransform = currentModel;

        meshRenderer = currentModel.GetComponent<MeshRenderer>();

        if (!currentModel.GetComponent<ClickableObject>())
        {
            //currentModel.gameObject.AddComponent<cakeslice.Outline>();
            co = currentModel.gameObject.AddComponent<ClickableObject>();
            co.SetScriptedParent(transform);
        }

        // get reference to collider
        myColliderMouse = currentModel.GetComponent<MeshCollider>();
        if (myColliderMouse && myColliderMouse.enabled)
        {
            ((MeshCollider)myColliderMouse).convex = true;
        }
        else myColliderMouse = currentModel.GetComponent<BoxCollider>();

        myColliderPhysics = currentModel.GetComponent<CapsuleCollider>();
        if (!myColliderPhysics) myColliderPhysics = myColliderMouse;
    }
    public Transform GetCurrentModel()
    {
        return MaxStages == 1 ? transform : transform.GetChild(Stage);
    }

    // Rotated sizes
    public int RotWidth()
    {
        return Orientation % 2 == 0 ? GridWidth : GridHeight;
    }
    public int RotHeight()
    {
        return Orientation % 2 == 1 ? GridWidth : GridHeight;
    }
    // Grid stuff
    public Node EntryNode()
    {
        switch(Orientation)
        {
            case 0:
                return Grid.GetNode(GridX + EntryDx, GridY + EntryDy);
            case 1:
                return Grid.GetNode(GridX + EntryDy, GridY + EntryDx);
            case 2:
                return Grid.GetNode(GridX + RotWidth() - 1 - EntryDx , GridY + RotHeight() - 1 - EntryDy);
            case 3:
                return Grid.GetNode(GridX + RotWidth() - 1 - EntryDy, GridY + RotHeight() - 1 - EntryDx);
        }

        return Grid.GetNode(GridX + EntryDx, GridY + EntryDy);
    }
    public Node CenterNode()
    {
        return Grid.GetNode(GridX + RotWidth() / 2, GridY + RotHeight() / 2);
    }
    public Vector3 Center()
    {
        return centerTransform.position;
    }
    public Vector2 GridPosition()
    {
        Vector3 gridPos = Grid.ToGrid(transform.position);
        return new Vector2(gridPos.x, gridPos.z);
    }

    public static List<GameBuilding> AllGameBuildings()
    { 
        List<GameBuilding> ret = new List<GameBuilding>();
        foreach (BuildingScript bs in allBuildingScripts)
            ret.Add(bs.gameBuilding);
        return ret;         
    }
    public static void DestroyAllBuildings()
    { 
        foreach (BuildingScript b in allBuildingScripts)
            Destroy(b.gameObject);
        allBuildingScripts.Clear();
    }

    public void RemoveChildBuilding(BuildingScript bs)
    {
        if ((bs.Type == BuildingType.Field ? gameBuilding.childBuildingField : gameBuilding.childBuildingStorage).Contains(bs.Nr))
        {
            (bs.Type == BuildingType.Field ? gameBuilding.childBuildingField : gameBuilding.childBuildingStorage).Remove(bs.Nr);
        }

    }
    public void AddChildBuilding(BuildingScript bs)
    {
        if(!(bs.Type == BuildingType.Field ? gameBuilding.childBuildingField : gameBuilding.childBuildingStorage).Contains(bs.Nr))
            (bs.Type == BuildingType.Field ? gameBuilding.childBuildingField : gameBuilding.childBuildingStorage).Add(bs.Nr);
    }
    public bool HasField()
    {
        return FieldBuildingID > 0;
    }
    public bool HasStorage()
    {
        return StorageBuildingID > 0;
    }
    public bool CanBuildField()
    {
        return ChildBuildingField.Count < FieldBuildCount;
    }
    public bool CanBuildStorage()
    {
        return ChildBuildingStorage.Count < StorageBuildCount;
    }

    // identify buildingscript by nr
    public static BuildingScript Identify(int nr)
    {
        foreach (BuildingScript bs in allBuildingScripts)
        {
            if (bs.Nr == nr) return bs;
        }
        return null;
    }
}

    /*
    void Update()
    {
        // only clickable, if not in blueprint mode
        co.clickable = !blueprint;

        //GetComponent<MeshCollider>().isTrigger = thisBuilding.walkable;
        if(UIManager.Instance.GetSelectedBuilding() == this || BuildManager.placing)
        {
            int range = 0;
            if(id == CAVE) range = viewRange;
            if(id == WAREHOUSEFOOD) range = foodRange;

            rangeCanvas.gameObject.SetActive(range != 0);
            rangeImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, range*20+1+(gridWidth % 2 == 0 ? 0:10));
            rangeImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, range*20+1+(gridHeight % 2 == 0 ? 0:10));
        } else rangeCanvas.gameObject.SetActive(false);
        
        if (blueprint)
        {
            int requiredCost = 0;
            foreach (GameResources r in bluePrintBuildCost)
                requiredCost += r.Amount;
            if (requiredCost == 0)
            {
                FinishBuilding();
            }
        }

        if (blueprint && meshRenderer.materials[0] != BuildManager.Instance.blueprintMaterial)
            meshRenderer.materials = blueprintMaterial;

        if(recruitingTroop.Count > 0)
        {
            recruitingTroop[0].recruitingTime -= Time.deltaTime;
            if(recruitingTroop[0].recruitingTime <= 0)
            {
                recruitingTroop[0].recruitingTime = 0;
                recruitingTroop.RemoveAt(0);
                /* TODO: spawn a troop 
            }
        }
    }
    
    
    public void FromID(int id)
    {
        this.id = id;
        switch (id)
        {
            case 0: Set(BuildingType.Storage, "Höhle", "Kleiner Unterschlupf mit Vorratslager", 0, new int[10], new int[] {25,25,0,0,0,0,0,0,0,0,0,0,0,0}, 0, 0, 5, 3, 3, true, false, 20, 0, 20, 0); break;
            case 1: Set(BuildingType.Population, "Unterschlupf", "Erhöht den Wohnraum", 0, new int[] { 40, 10, 0, 0, 0 }, new int[20], 0, 0, 2, 4, 4, true, true, 0, 0, 0, 0); break;

            case 2: Set(BuildingType.Storage, "Lagerplatz", "Lagert Holz, Steine, Fell und Keulen", 0, new int[] { 25, 15, 0, 0, 0 }, new int[] { 200, 200, 0, 0, 0, 0, 0, 0, 0, 0, 0, 50, 0, 20, 0 }, 0, 0, 0, 4, 4, true, true, 0, 0, 0, 0); break;
            case 3: Set(BuildingType.Storage, "Kornspeicher", "Lagert Getreide, Pilze, Fleisch und Fische", 0, new int[] { 20, 0, 0, 0, 0 }, new int[] { 0, 0, 0, 0, 0, 150, 150, 150, 150, 0, 0, 0, 0, 0 }, 0, 0, 0, 2, 2, true, false, 0, 35, 0, 0); break;

            case 4: Set(BuildingType.Food, "Fischerplatz", "Gefangene Fische (Wild) werden hier zu Fisch und Knochen verarbeitet", 0, new int[] { 25, 0, 0, 0, 0 }, new int[] { 0, 0, 0, 0, 0, 0, 50, 0, 0, 50, 50, 0, 0, 0 }, Job.FISHER, 2, 0, 4, 4, true, true, 0, 0, 0, 0); break;
            case 5: Set(BuildingType.Other, "Holzlager", "Erlaubt die Holzverarbeitung", 0, new int[] { 35, 10, 0, 0, 0 }, new int[20], Job.LUMBERJACK, 0, 0, 4, 4, true, false, 0, 0, 0, 0); break;
            case 6: Set(BuildingType.Crafting, "Jagdhütte", "Erlaubt das Jagen. Hier können Tiere zu Fleisch, Knochen, Zähnen und Fell verarbeitet werden", 0, new int[] { 45, 20, 0, 0, 0 }, new int[] { 0, 0, 0, 0, 0, 0, 0, 30, 0, 0, 30, 15, 0, 0, 10, 80 }, Job.HUNTER, 1, 0, 4, 4, true, true, 0, 0, 0, 13); break;
            case 7: Set(BuildingType.Crafting, "Steinzeit Schmied", "Lagerung von Knochen und Herstellung von Knochen-Werkzeug", 0, new int[] { 50, 35, 0, 0, 0 }, new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 40, 0, 10, 0 }, Job.BLACKSMITH, 1, 0, 8, 4, true, true, 0, 0, 0, 0); break;

            case 8: Set(BuildingType.Luxury, "Lagerfeuer", "Bringe Holz, um das Feuer anzuzünden. Erhöht den Gesundheitsfaktor (+2)", 0, new int[] { 15, 5, 0, 0, 0 }, new int[20], Job.GATHERER, 0, 0, 2, 1, false, true, 0, 0, 10, 0); break;
            case 9: Set(BuildingType.Religion, "Opferstätte", "Durch Opfergaben kannst du die Götter gnädig stimmen", 0, new int[] { 0, 100, 0, 0, 0 }, new int[20], Job.PRIEST, 1, 0, 1, 1, false, true, 5, 0, 0, 0); break;

            case 10: Set(BuildingType.Crafting, "Keulenwerkstatt", "In diesem Gebäude werden Keulen hergestellt", 0, new int[] { 40, 25, 0, 0, 0 }, new int[]{ 10, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 5, 0 }, 0, 1, 0, 1, 1, false, true, 5, 0, 0, 0); break;
            case 11: Set(BuildingType.Recruiting, "Kriegsplatz ", "Sammelt alle Arten von rekrutierten Krieger/Soldaten", 0, new int[] { 80, 30, 0, 0, 0 }, new int[20], 0, 30, 0, 2, 2, false, true, 5, 0, 0, 0); break;

            case 12: Set(BuildingType.Research, "Tüftler", "Schaltet den Technologiebaum frei", 0, new int[] { 50, 20, 0, 0 }, new int[20], 0, 0, 0, 1, 1, false, false, 5, 0, 0, 0); break;

            case 13: Set(BuildingType.Luxury, "Schmuckmanufaktur", "Stellt Halsschmuck aus Zähnen her", 0, new int[] { 20, 20, 0, 0 }, new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 80, 10 }, 0, 0, 0, 1, 1, false, true, 5, 0, 0, 0); break;
        }
    }
    private void Set(BuildingType type, string name, string description, int cost, int[] materialCost, int[] resourceStorage, int jobId, int workspace, int populationRoom,
        int gridWidth, int gridHeight, bool walkable, bool multipleBuildings, int viewRange, int foodRange, int buildRange, int unlockBuildingID)
    {
        showGrid = id == CAVE || id == CAMPFIRE || id >= 9;
        this.type = type;
        this.buildingName = name;
        this.description = description;
        this.cost = cost;
        this.materialCost = materialCost;
        this.resourceStorage = resourceStorage;
        this.resourceCurrent = new int[20];

        this.jobId = jobId;
        this.workspace = workspace;
        this.populationRoom = populationRoom;

        this.gridWidth = gridWidth;
        this.gridHeight = gridHeight;

        this.stage = 0;
        this.populationCurrent = 0;
        this.gridX = 0;
        this.gridY = 0;

        this.walkable = walkable;
        this.multipleBuildings = multipleBuildings;

        this.viewRange = viewRange;
        this.foodRange = foodRange;
        this.buildRange = buildRange;

        this.unlockBuildingID = unlockBuildingID;
    }

    public string GetDescription()
    {
        string ret = description;
        if(!multipleBuildings && id > 0) ret += "\nKann nur einmal gebaut werden";
        return ret;
    }
    // get factors influenced by this building
    public int LuxuryFactor()
    {
        if(id == CAMPFIRE)
        {
        }
        return 0;
    }
    public int HealthFactor()
    {
        if(id == CAMPFIRE)
        {
            if(GetComponent<Campfire>().fireBurning) return 2;
        }
        return 0;
    }

    // return wether to display lifebar
    public bool HasLifebar()
    {
        return id == CAMPFIRE;
    }
    public float LifebarFactor()
    {
        if(id == CAMPFIRE && GetComponent<Campfire>()) return GetComponent<Campfire>().GetHealthFactor();
        return 0;
    }

    // identify buildingscript by nr
    public static Building Identify(int nr)
    {
        foreach (Building bs in allBuildings)
        {
            if(bs.nr == nr) return bs;
        }
        return null;
    }
    
    public static int COUNT = 15;
    public static int CAVE = 0;
    public static int SHELTER = 1;
    public static int WAREHOUSE = 2;
    public static int WAREHOUSEFOOD = 3;
    public static int FISHERMANPLACE = 4;
    public static int LUMBERJACK = 5;
    public static int HUNTINGLODGE = 6;
    public static int BLACKSMITH = 7;
    public static int CAMPFIRE = 8;
    public static int SACRIFICIALALTAR = 9;
    public static int CLUB_FACTORY = 10;
    public static int WAR_PLACE = 11;
    public static int RESEARCH = 12;
    public static int JEWLERY_FACTORY = 13;
    public static int DOCK = 14;
    private static bool[] unlocked = new bool[20];
}*/