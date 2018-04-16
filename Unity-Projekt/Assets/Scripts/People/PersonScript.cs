using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PersonScript : MonoBehaviour {

    // Collection of all people and selected people
    public static HashSet<PersonScript> allPeople = new HashSet<PersonScript>();
    public static HashSet<PersonScript> selectedPeople = new HashSet<PersonScript>();

    public int ID;

    private Person thisPerson;
    public bool selected, highlighted;

    private float saturationTimer, saturation;

    private float moveSpeed = 1.2f;
    private float currentMoveSpeed = 0f;

    private List<Node> currentPath;
    public List<Task> routine = new List<Task>();
    private Node lastNode;

    // Activity speeds/times
    private float choppingSpeed = 0.8f, putMaterialSpeed = 2f, buildSpeed = 2f;
    private float fishingTime = 1, processFishTime = 2f, collectingSpeed = 2f;

    private Transform canvas;
    private Image imageHP;

    private bool automatedTasks = false;

    private cakeslice.Outline outline;

    private bool inFoodRange = false;

	// Use this for initialization
    void Start()
    {
        saturationTimer = 0;

        outline = GetComponent<cakeslice.Outline>();
        outline.enabled = false;
        canvas = transform.Find("Canvas").transform;
        imageHP = canvas.Find("Health").Find("ImageHP").GetComponent<Image>();

        Vector3 gp = Grid.ToGrid(transform.position);
        lastNode = Grid.GetNode((int)gp.x, (int)gp.z);

        ID = allPeople.Count;
        allPeople.Add(this);
	}

	// Update is called once per frame
    void Update()
    {
        lastNode.SetPeopleOccupied(false);

        if (routine.Count > 0)
        {
            Task ct = routine[0];
            ExecuteTask(ct);
        }
        
        saturationTimer += Time.deltaTime;

        inFoodRange = CheckIfInFoodRange();

        // Eat after not being saturated anymore
        if(saturationTimer >= saturation) {
            saturation = 0;
            saturationTimer = 0;

            // automatically take food from inventory first
            GameResources food = thisPerson.inventoryFood;

            // only take food from inventory if its food
            if(food != null && (food.amount == 0 || food.GetResourceType() != ResourceType.Food)) food = null;

            // check for food from warehouse
            if(food == null) food = GameManager.village.TakeFoodForPerson(this);
            
            if(food != null)
            {
                thisPerson.health += food.health;
                thisPerson.hunger += food.nutrition;
                saturation = food.nutrition;
                food.Take(1);
            }
        }

        // hp update
        float satFact = 0f;
        if(thisPerson.hunger <= 0) satFact = 1f;
        else if(thisPerson.hunger <= 10) satFact = 0.5f;
        else if(thisPerson.hunger <= 20) satFact = 0.2f;

        thisPerson.health -= Time.deltaTime * satFact;

        // hunger update
        satFact = 0.18f;
        if(saturation == 0) satFact = 1f;

        if(GameManager.GetTwoSeason() == 0) satFact *= 1.5f;

        thisPerson.hunger -= Time.deltaTime * satFact;

        if(thisPerson.hunger < 0) thisPerson.hunger = 0;
        if(thisPerson.hunger > 100) thisPerson.hunger = 100;

        if(thisPerson.health < 0) thisPerson.health = 0;
        if(thisPerson.health > 100) thisPerson.health = 100;

        // position player at correct ground height on terrain
        Vector3 terrPos = transform.position;
        terrPos.y = Terrain.activeTerrain.SampleHeight(terrPos) + Terrain.activeTerrain.transform.position.y;
        transform.position = terrPos;
        lastNode.SetPeopleOccupied(true);
	}

    void OnDestroy()
    {
        allPeople.Remove(this);
        selectedPeople.Remove(this);
    }

    void LateUpdate()
    {
        //GetComponent<Rigidbody>().velocity = GetComponent<Rigidbody>().velocity*0.95f;

        // Update UI canvas for health bar
        Camera camera = Camera.main;
        canvas.LookAt(canvas.position + camera.transform.rotation * Vector3.forward * 0.0001f, camera.transform.rotation * Vector3.up);
        canvas.gameObject.SetActive(highlighted || selected);
        float maxWidth = canvas.Find("Health").Find("ImageHPBack").GetComponent<RectTransform>().rect.width - 2;
        //personInfoHealthbar.rectTransform.offsetMax = new Vector2(-(2+ maxWidth * (1f-ps.GetHealthFactor())),-2);
        imageHP.rectTransform.offsetMax = new Vector2(-(1+ maxWidth * (1f-GetHealthFactor())),-1);
        imageHP.color = GetConditionCol();
        //imageHP.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,);
        
        // Update outline component
        outline.enabled = highlighted || selected;
        outline.color = selected ? 1 : 0;
    }

    // Do a given task 'ct'
    void ExecuteTask(Task ct)
    {
        ct.taskTime += Time.deltaTime;
        Plant plant = null;
        BuildingScript bs = null;
        GameResources inv = null;
        GameResources invFood = thisPerson.inventoryFood;
        GameResources invMat = thisPerson.inventoryMaterial;
        Village myVillage = GameManager.village;
        Transform nearestTrsf = null;
        if (ct.targetTransform != null)
        {
            plant = ct.targetTransform.GetComponent<Plant>();
            bs = ct.targetTransform.GetComponent<BuildingScript>();
        }
        int am = 0;
        switch (ct.taskType)
        {
            case TaskType.CutTree: // Chopping a tree
            case TaskType.CullectMushroomStump: // Collect the mushroom from stump
            case TaskType.MineRock: // Mine the rock to get stones
                if(!plant && ct.taskType == TaskType.CutTree && thisPerson.job.id == Job.LUMBERJACK)
                {
                    if(routine.Count <= 1)
                    {
                        // only automatically find new tree to cut if person is a lumberjack
                        if(ct.taskType == TaskType.CutTree && thisPerson.job.id == Job.LUMBERJACK)
                        {
                            if(thisPerson.GetFreeMaterialInventorySpace() > 0)
                                nearestTrsf = myVillage.GetNearestPlant(transform.position, PlantType.Tree, thisPerson.GetTreeCutRange());
                            else
                            {
                                StoreMaterialInventory();
                                break;
                            }
                        }
                        else if(ct.taskType == TaskType.CullectMushroomStump)
                        {
                        }
                        else if(ct.taskType == TaskType.MineRock)
                        {
                        }

                        if (nearestTrsf) 
                        {
                            SetTargetTransform(nearestTrsf, true);
                            break;
                        }
                    }
                    NextTask();
                    break;
                }
                if(!plant || ct.taskType == TaskType.CutTree && thisPerson.job.id != Job.LUMBERJACK) {
                    //(!plant || ct.taskType == TaskType.CutTree && thisPerson.job.id != Job.LUMBERJACK) && !(ct.taskType == TaskType.CutTree && thisPerson.job.id == Job.LUMBERJACK)) {
                    NextTask();
                    break;
                }
                if (plant.IsBroken())
                {
                    // Collect wood of fallen tree, by chopping it into pieces
                    if (ct.taskTime >= 1f / choppingSpeed)
                    {
                        ct.taskTime = 0;
                        //Transform nearestTree = GameManager.village.GetNearestPlant(PlantType.Tree, transform.position, thisPerson.GetTreeCutRange());

                        int freeSpace = 0;
                        ResourceType pmt = new GameResources(plant.materialID, 0).GetResourceType();
                        if(pmt == ResourceType.BuildingMaterial) freeSpace = thisPerson.GetFreeMaterialInventorySpace();
                        if(pmt == ResourceType.Food) freeSpace = thisPerson.GetFreeFoodInventorySpace();

                        if (plant.material > 0)
                        {
                            GameManager.UnlockResource(plant.materialID);

                            // Amount of wood per one chop gained
                            int mat = plant.materialPerChop;
                            if (plant.material < mat) mat = plant.material;
                            mat = thisPerson.AddToInventory(new GameResources(plant.materialID, mat));
                            plant.TakeMaterial(mat);

                            if(GameManager.debugging) GameManager.Msg(mat + " added to inv");

                            // If still can mine plant, continue
                            if(mat != 0 && plant.material > 0 && freeSpace > 0) break;
                        }
                        
                        if(routine.Count <= 1)
                        {
                            // only automatically find new tree to cut if person is a lumberjack
                            if(ct.taskType == TaskType.CutTree && thisPerson.job.id == Job.LUMBERJACK)
                            {
                                if(freeSpace > 0)
                                    nearestTrsf = myVillage.GetNearestPlant(transform.position, PlantType.Tree, thisPerson.GetTreeCutRange());
                                else
                                {
                                    StoreMaterialInventory();
                                    break;
                                }
                            }
                            else if(ct.taskType == TaskType.CullectMushroomStump)
                            {
                            }
                            else if(ct.taskType == TaskType.MineRock)
                            {
                            }

                            if (nearestTrsf) 
                            {
                                SetTargetTransform(nearestTrsf, true);
                                break;
                            }
                        }

                        NextTask();
                    }
                }
                else if (ct.taskTime >= 1f / choppingSpeed)
                {
                    ct.taskTime = 0;
                    plant.Mine();
                }
                break;
            case TaskType.Fisherplace: // Making food out of fish
                // If person is not a fisher, he can't do anything here
                if (thisPerson.job.id != Job.FISHER)//
                {
                    NextTask();
                }
                else
                {
                    /* TODO: fisherplace logic */

                    // Check what to do (leave rawfish, process fish to edible/bones, take from fisherplace to storage)
                    if(invFood != null && invFood.GetAmount() > 0 && invFood.id == GameResources.RAWFISH && bs.GetBuilding().FreeStorage(GameResources.RAWFISH) > 0)
                    {
                        if(ct.taskTime >= 1f/putMaterialSpeed)
                        {
                            ct.taskTime = 0;

                            // store raw fish in building
                            int stockMat = bs.GetBuilding().Restock(invFood, 1);
                            
                            // check if building storage is full
                            if(stockMat == 0)
                            {
                                GameManager.Msg("Fischer-Platz ist überfüllt mit rohem Fisch!");
                                NextTask();
                                break;
                            }

                            // Take one resource from inventory
                            invFood.Take(1);
                        }
                    }
                    // convert rawfish into fish
                    else if(bs.GetBuilding().FreeStorage(GameResources.FISH) > 0 && bs.GetBuilding().FreeStorage(GameResources.BONES) > 0 && 
                        bs.GetBuilding().resourceCurrent[GameResources.RAWFISH] > 0)
                    {
                        if(ct.taskTime >= processFishTime)
                        {
                            ct.taskTime = 0;

                            // Process raw-fish into bones and edible fish
                            bs.GetBuilding().resourceCurrent[GameResources.RAWFISH]--;
                            bs.GetBuilding().resourceCurrent[GameResources.FISH]++;
                            bs.GetBuilding().resourceCurrent[GameResources.BONES]++;
                            GameManager.UnlockResource(GameResources.FISH);
                            GameManager.UnlockResource(GameResources.BONES);
                        }
                    }
                    // take fish into inventory of person
                    else if(bs.GetBuilding().resourceCurrent[GameResources.FISH] > 0 && 
                        (invFood == null || invFood.amount == 0 || invFood.id == GameResources.FISH && thisPerson.GetFreeFoodInventorySpace() > 0))
                    {
                        if(ct.taskTime >= 1f/putMaterialSpeed)
                        {
                            ct.taskTime = 0;

                            int mat = thisPerson.AddToInventory(new GameResources(GameResources.FISH, 1));
                            if(mat > 0) bs.GetBuilding().resourceCurrent[GameResources.FISH]--;
                            else GameManager.Error("Take fish into inventory of person");
                        }
                    }
                    // take bones into inventory of person
                    else if(bs.GetBuilding().resourceCurrent[GameResources.BONES] > 0 && 
                        (invMat == null || invMat.amount == 0 || invMat.id == GameResources.BONES && thisPerson.GetFreeMaterialInventorySpace() > 0))
                    {
                        if(ct.taskTime >= 1f/putMaterialSpeed)
                        {
                            ct.taskTime = 0;

                            int mat = thisPerson.AddToInventory(new GameResources(GameResources.BONES, 1));
                            if(mat > 0) bs.GetBuilding().resourceCurrent[GameResources.BONES]--;
                            else GameManager.Error("Take bones into inventory of person");
                        }
                    }
                    // walk automatically to warehouse
                    else
                    {
                        bool addedTask = false;
                        if(invFood != null && invFood.id == GameResources.FISH && invFood.amount > 0)
                        {
                            if(StoreFoodInventory()) addedTask = true;
                        }
                        if(invMat != null && invMat.id == GameResources.BONES && invMat.amount > 0)
                        {
                            if(StoreMaterialInventory()) addedTask = true;
                        }
                        if(addedTask)
                        {
                            // automatically start fishing again
                            nearestTrsf = myVillage.GetNearestPlant(transform.position, PlantType.Reed, thisPerson.GetReedRange());
                            if(nearestTrsf) AddTargetTransform(nearestTrsf, true);
                            NextTask();
                        }
                        else
                        {
                            NextTask();
                        }
                    }
                }
                break;
            case TaskType.BringToWarehouse: // Bringing material to warehouse
                while(ct.taskRes.Count > 0 && ct.taskRes[0].amount == 0)
                    ct.taskRes.RemoveAt(0);
                if(ct.taskRes.Count == 0)
                {
                    if(routine.Count <= 1 && ct.automated)
                    {
                        // only automatically find new tree to cut if person is a lumberjack
                        if(thisPerson.job.id == Job.LUMBERJACK)
                        {
                            nearestTrsf = myVillage.GetNearestPlant(transform.position, PlantType.Tree, thisPerson.GetTreeCutRange());
                            if(nearestTrsf != null) SetTargetTransform(nearestTrsf, true);
                            break;
                        }
                    }
                    NextTask();
                    break;
                }

                // Get the right material to bring to warehouse
                inv = null;
                if(invMat != null && invMat.amount > 0 && invMat.id == ct.taskRes[0].id) inv = invMat;
                if(invFood != null && invFood.amount > 0 && invFood.id == ct.taskRes[0].id) inv = invFood;

                if(inv == null || inv.id != ct.taskRes[0].id)
                {
                    // Mostly because person eats food on their way to warehouse

                    //GameManager.Error("inventory wrong for warehouse");
                    ct.taskRes.RemoveAt(0);
                    break;
                }

                // Put one item at a time into warehouse
                if(ct.taskTime >= 1f/putMaterialSpeed)
                {
                    ct.taskTime = 0;

                    int stockMat = bs.GetBuilding().Restock(ct.taskRes[0], 1);
                    
                    // check if building storage is full
                    if(stockMat == 0)
                    {
                        GameManager.Msg("Lager ist voll!");
                        ct.taskRes.RemoveAt(0);
                        break;
                    }

                    // Take one resource from inventory and taskRes
                    ct.taskRes[0].Take(1);
                    inv.Take(1);
                }
                break;
            case TaskType.TakeFromWarehouse: // Taking material from warehouse
                while(ct.taskRes.Count > 0 && ct.taskRes[0].amount == 0)
                    ct.taskRes.RemoveAt(0);
                if(ct.taskRes.Count == 0)
                {
                    NextTask();
                    break;
                }

                inv = null;
                int maxInvSize = 0;

                if(ct.taskRes[0].GetResourceType() == ResourceType.BuildingMaterial) 
                {
                    maxInvSize = thisPerson.GetMaterialInventorySize() ;
                    inv = invMat;
                }
                if(ct.taskRes[0].GetResourceType() == ResourceType.Food)
                {
                    maxInvSize = thisPerson.GetFoodInventorySize() ;
                    inv = invFood;
                }

                if(inv != null && ((inv.id != ct.taskRes[0].id && inv.amount != 0) || inv.amount == maxInvSize))
                {
                    GameManager.Msg("Inventar voll!");
                    //GameManager.Error("inventory wrong for taking from warehouse (first deposit inventory before taking)");
                    ct.taskRes.RemoveAt(0);
                    break;
                }

                // Take one item at a time from warehouse
                if(ct.taskTime >= 1f/putMaterialSpeed)
                {
                    ct.taskTime = 0;

                    int takeRes = bs.GetBuilding().Take(ct.taskRes[0], 1);

                    // check if building storage is full
                    if(takeRes == 0)
                    {
                        GameManager.Msg("Lager ist leer!");
                        ct.taskRes.RemoveAt(0);
                        break;
                    }

                    // Add one resource from inventory and take one from taskRes
                    ct.taskRes[0].Take(1);
                    thisPerson.AddToInventory(new GameResources(ct.taskRes[0].id, 1));
                }
                break;
            case TaskType.Campfire: // Restock campfire fire wood
                Campfire cf = ct.targetTransform.GetComponent<Campfire>();
                if(invMat != null && invMat.id == GameResources.WOOD && invMat.amount > 0)
                {
                    if(ct.taskTime >= 1f/putMaterialSpeed)
                    {
                        ct.taskTime = 0;
                        invMat.Take(cf.Restock(1));
                    }
                }
                else
                {
                    NextTask();
                }
                break;
            case TaskType.Fishing: // Do Fishing
                if(!plant){
                    NextTask();
                    break;
                }
                
                if (thisPerson.job.id == Job.FISHER && (invFood == null || invFood.GetAmount() == 0 || invFood.id == GameResources.RAWFISH))
                {
                    if (ct.taskTime >= fishingTime)
                    {
                        ct.taskTime = 0;
                        int season = GameManager.GetTwoSeason();
                        // only fish in summer
                        if(season == 0)
                        {
                            GameManager.Msg("Keine Fische im Winter");
                        }
                        else if (season == 1 && Random.Range(0, 3) == 1 && plant.material >= 1)
                        {
                            int amount = thisPerson.AddToInventory(new GameResources(GameResources.RAWFISH, 1));
                            
                            GameManager.UnlockResource(plant.materialID);
                            
                            plant.material -= amount;
                            if(plant.material == 0)
                                plant.Break();

                        }
                    }
                    if(routine.Count <= 1)
                    {
                        if(thisPerson.GetFreeFoodInventorySpace() == 0)
                        {
                            // get nearest fishermanPlace
                            nearestTrsf = myVillage.GetNearestBuildingID(transform.position, 4);
                        }
                        else if(plant.material == 0)
                        {
                            nearestTrsf = myVillage.GetNearestPlant(transform.position, PlantType.Reed, thisPerson.GetReedRange());
                            if(!nearestTrsf)
                                nearestTrsf = myVillage.GetNearestBuildingID(transform.position, 4);
                        }

                        if(nearestTrsf)
                        {
                            SetTargetTransform(nearestTrsf, true);
                            break;
                        }
                    }
                    if(thisPerson.GetFreeFoodInventorySpace() == 0 || plant.material == 0)
                        NextTask();
                }
                else
                {
                    NextTask();
                }
                break;
            case TaskType.Build: // Put resources into blueprint building
                if (invMat == null) NextTask();
                else if(ct.taskTime >= 1f/buildSpeed)
                {
                    ct.taskTime = 0;
                    bool built = false;
                    foreach (GameResources r in bs.resourceCost)
                    {
                        if (invMat.id == r.id && r.GetAmount() > 0 && invMat.GetAmount() > 0 && !built)
                        {
                            built = true;
                            invMat.Take(1);
                            r.Take(1);
                            if (r.GetAmount() == 0) NextTask();
                        }
                    }

                    if (!built) NextTask();
                }
                break;
            case TaskType.CollectMushroom: // Collect the mushroom
                if(!plant){
                    NextTask();
                    break;
                }
                // add resources to persons inventory
                if(plant.gameObject.activeSelf && plant.material > 0)
                {
                    if(ct.taskTime >= 1f/collectingSpeed)
                    {
                        ct.taskTime = 0;
                        am = thisPerson.AddToInventory(new GameResources(plant.materialID, 1));
                        if(am > 0) 
                        {
                            plant.material--;
                            GameManager.UnlockResource(plant.materialID);
                            if(plant.material == 0)
                            {
                                // Destroy collected mushroom
                                plant.Break();
                                plant.gameObject.SetActive(false);
                            }
                            else break;
                        }
                    }
                    else break;
                }

                NextTask();

                // Find another mushroom to collect
                if(routine.Count <= 1 && thisPerson.job.id == Job.GATHERER)
                {
                    Transform nearestMushroom = myVillage.GetNearestPlant(transform.position, PlantType.Mushroom, thisPerson.GetCollectingRange());
                    if (thisPerson.GetFreeFoodInventorySpace() == 0)
                    {
                        if (!StoreFoodInventory()) WalkToCenter();
                    }
                    else
                    {
                        if (nearestMushroom != null) SetTargetTransform(nearestMushroom, true);
                        else if (!StoreFoodInventory()) WalkToCenter();
                    }
                }
                break;
            case TaskType.Harvest:
                // add resources to persons inventory
                if(plant.gameObject.activeSelf)
                {
                    if(plant.material == 0)
                    {
                        plant.Break();
                        plant.gameObject.SetActive(false);
                    }
                    else
                    {
                        am = thisPerson.AddToInventory(new GameResources(plant.materialID, plant.material));
                        if(am > 0) 
                        {
                            GameManager.UnlockResource(plant.materialID);
                            // Destroy collected plant
                            plant.Break();
                            plant.gameObject.SetActive(false);
                        }
                    }
                }
                NextTask();
                break;
            case TaskType.PickupItem: // Pickup the item
                Item itemToPickup = routine[0].targetTransform.GetComponent<Item>();
                if (itemToPickup != null && itemToPickup.gameObject.activeSelf && itemToPickup.resource.amount > 0)
                {
                    if(ct.taskTime >= 1f/collectingSpeed)
                    {
                        ct.taskTime = 0;
                        am = thisPerson.AddToInventory(new GameResources(itemToPickup.resource.id, 1));
                        if (am > 0)
                        {
                            GameManager.UnlockResource(itemToPickup.resource.id);

                            itemToPickup.resource.amount--;
                            if(itemToPickup.resource.amount > 0) break;

                            itemToPickup.gameObject.SetActive(false);

                            int freeSpace = 0;
                            ResourceType pmt = itemToPickup.GetResource().GetResourceType();
                            if(pmt == ResourceType.BuildingMaterial) freeSpace = thisPerson.GetFreeMaterialInventorySpace();
                            if(pmt == ResourceType.Food) freeSpace = thisPerson.GetFreeFoodInventorySpace();
                            
                            // Automatically pickup other items in reach or go to warehouse if inventory is full
                            Transform nearestItem = GameManager.village.GetNearestItemInRange(transform.position, itemToPickup, thisPerson.GetCollectingRange());
                            if (routine.Count == 1 && nearestItem != null && freeSpace > 0 && nearestItem.gameObject.activeSelf)
                            { 
                                SetTargetTransform(nearestItem, true);
                                break;
                            }
                            else
                            {
                                //Transform nearestItemStorage = GameManager.village.GetNearestBuildingType(transform.position, BuildingType.StorageFood);
                                //if (nearestFoodStorage != null) SetTargetTransform(nearestFoodStorage);
                                //if (nearestItem != null && nearestFoodStorage != null) AddTargetTransform(nearestItem);
                            }
                        }
                        else
                        {
                            GameManager.Msg(thisPerson.GetFirstName() + " kann " + itemToPickup.GetName() + " nicht aufsammeln");
                        }
                    } else break;
                }
                NextTask();
                break;
            case TaskType.Craft:
                if(thisPerson.job.id == Job.BLACKSMITH)
                {
                    GameResources toCraft;
                    // bone-tool requires 3 bones
                    if(bs.GetBuilding().resourceCurrent[GameResources.BONES] >= 3)
                    {
                        toCraft = new GameResources(GameResources.TOOL_BONE, 1);
                        if(ct.taskTime >= toCraft.craftTime)
                        {
                            ct.taskTime = 0;

                            bs.GetBuilding().resourceCurrent[GameResources.BONES]-=3;
                            bs.GetBuilding().resourceCurrent[toCraft.id]++;
                            GameManager.UnlockResource(toCraft.id);
                            if(bs.GetBuilding().resourceCurrent[GameResources.BONES] < 3) NextTask();
                        }
                    }
                    else
                    {
                        GameManager.Msg("Für ein Knochenwerkzeug brauchst du 3 Knochen!");
                    }
                }
                else
                {
                    NextTask();
                }
                break;
            case TaskType.Walk: // Walk towards the given target
                
                // Firstly check if already in stopradius of targetObject
                float objectStopRadius = 0f;
                Vector3 diff;
                if(ct.targetTransform != null)
                {
                    // standard stop radius for objects
                    objectStopRadius = 0.8f;
                    // Set custom stop radius for trees
                    if (ct.targetTransform.tag == "Plant" && plant != null)
                    {
                        objectStopRadius = plant.GetRadiusInMeters();
                    }
                    else if (ct.targetTransform.tag == "Item")
                    {
                        objectStopRadius = 0.1f;
                    }
                    else if (ct.targetTransform.tag == "Building")
                    {
                        BuildingScript tarBs = ct.targetTransform.GetComponent<BuildingScript>();
                        if(tarBs.GetBuilding().walkable)
                            objectStopRadius = 1f;
                        else
                            objectStopRadius = 0.3f;
                    }
                    else
                    {
                        Debug.Log("Unhandled stop radius");
                    }
                    diff = ct.targetTransform.position - transform.position;
                    float tmpDist = Vector3.SqrMagnitude(diff);
                    if(tmpDist < objectStopRadius * Grid.SCALE)
                    {
                        EndCurrentPath();
                        break;
                    }
                }
                // Get next position to walk towards
                Vector3 nextTarget = ct.target;
                // Distance from taret at which we call it reached
                float stopRadius = 0.1f;
                // If still nodes to walk in path, get them as nextTarget
                if (currentPath != null && currentPath.Count > 0)
                {
                    Node nextNode = currentPath[0];
                    // Debug.Log(Vector3.Distance(Grid.ToWorld(nextNode.gridX,nextNode.gridY),Grid.ToWorld(lastNode.gridX,lastNode.gridY)));
                    nextTarget = nextNode.transform.position;//Grid.ToWorld(nextNode.GetX(), nextNode.GetY());
                    //finalTargetNode = currentPath[currentPath.Count-1];
                }
                else if (currentPath == null)
                {
                    FindPath(ct.target, ct.targetTransform);
                    break;
                }
                // Get forward vector towards target
                diff = nextTarget - transform.position;
                diff.y = 0;
                if (ct.targetTransform != null)
                {
                    if(currentPath.Count == 0) stopRadius = objectStopRadius;
                }

                float distance = Vector3.SqrMagnitude(diff);

                /* TODO: better factor */
                stopRadius *= Grid.SCALE;

                //Debug.Log(distance.ToString("F2") + " / "+stopRadius.ToString("F2"));
                if (currentPath.Count > 1 || distance > stopRadius)
                {
                    currentMoveSpeed += 0.05f * moveSpeed;
                    if (currentMoveSpeed > moveSpeed) currentMoveSpeed = moveSpeed;
                    
                    // Update position/rotation towards target
                    transform.rotation = Quaternion.LookRotation(diff);

                    transform.position += diff.normalized * moveSpeed * Time.deltaTime;

                    foreach(Plant p in GameManager.village.nature.flora)
                    {
                        CheckHideableObject(p,p.currentModel);
                        
                    }
                    foreach(Item p in Item.allItems)
                    {
                        CheckHideableObject(p,p.transform);
                    }
                }

                if (distance <= stopRadius)
                {
                    // If path has ended, continue to next task
                    if (currentPath.Count == 0)
                    {
                        EndCurrentPath();
                        break;
                    }

                    if(currentPath[0].objectWalkable)
                        lastNode = currentPath[0];

                    // remove path node
                    currentPath.RemoveAt(0);
                }
                        
                        
                break;
        }
    }
    public void NextTask()
    {
        // Remove current Task from routine
        routine.RemoveAt(0);

        // If a new path has to be walked, find it with astar
        if (routine.Count > 0 && routine[0].taskType == TaskType.Walk)
        {
            FindPath(routine[0].target, routine[0].targetTransform);
        }
    }

    private void EndCurrentPath()
    {
        Vector3 prevRot = transform.rotation.eulerAngles;
        if (routine[0].targetTransform != null)
            transform.LookAt(routine[0].targetTransform);
        prevRot.y = transform.rotation.eulerAngles.y;
        transform.rotation = Quaternion.Euler(prevRot);
        NextTask();
        currentMoveSpeed = 0f;
    }

    // Walk back to center
    public void WalkToCenter()
    {
        AddRoutineTaskPosition(Vector3.zero, true, true);
    }

    public void CheckHideableObject(HideableObject p, Transform model)
    {
        if(p.inBuildingViewRange) return;
        if(!p) return;
        bool inRadius = Mathf.Abs(transform.position.x - p.transform.position.x) <= thisPerson.viewDistance && Mathf.Abs(transform.position.z - p.transform.position.z) <= thisPerson.viewDistance;
        if(p.personIDs.Contains(ID)) 
        {
            if(!inRadius)
            {
                p.personIDs.Remove(ID);
                if(p.personIDs.Count == 0) {
                    p.ChangeHidden(true);
                    model.GetComponent<cakeslice.Outline>().enabled = false;
                }
            }
        }
        else
        {
            if(inRadius)
            { 
                p.personIDs.Add(ID);
                p.ChangeHidden(false);
            }
        }
    }

    public Person GetPerson()
    {
        return thisPerson;
    }
    public void SetPerson(Person person)
    {
        thisPerson = person;
    }

    public void SetTargetTransform(Transform target, bool automatic)
    {
        AddRoutineTaskTransform(target, target.position, automatic, true);
    }
    public void AddTargetTransform(Transform target, bool automatic)
    {
        AddRoutineTaskTransform(target, target.position, automatic, false);
    }
    public void AddRoutineTaskTransform(Transform target, Vector3 targetPosition, bool automatic, bool clearRoutine)
    {
        Task walkTask = new Task(TaskType.Walk, targetPosition, target);
        Task targetTask = TargetTaskFromTransform(target, automatic);
        if(target != null && targetTask == null) return;
        if(clearRoutine) routine.Clear();

        foreach(Task t in routine)
        {
            if(target != null && t.targetTransform == target ) return;
            if(t.target == targetPosition ) return;
        }
        /*int foundSameWalk = 0;
        // Make sure not to redo same task twice
        foreach(Task t in routine)
            if(t.taskType == TaskType.Walk)
            {
                if(Grid.ToGrid(t.target) - Grid.ToGrid(targetPosition)) 
                {
                    if(foundSameWalk > 0) return;
                    else foundSameWalk = 1;
                }
                else foundSameWalk = 0;
            }*/

        int rc = routine.Count;
        routine.Add(walkTask);
        if (targetTask != null) routine.Add(targetTask);

        if (rc == 0)
        {
            FindPath(target.position, target);
        }
    }
   /* public void SetTargetTransform(Transform target, Vector3 targetPosition, bool automatic)
    {
        AddTargetTransform(target, targetPosition, automatic, true);
    }
    public void SetTargetPosition(Vector3 newTarget, bool automatic)
    {
        routine.Clear();
        AddTargetPosition(newTarget, automatic);
    }*/
    public void AddRoutineTaskPosition(Vector3 newTarget, bool automatic, bool clearRoutine)
    {
        Vector3 gridPos = Grid.ToGrid(newTarget);
        if(!Grid.ValidNode((int)gridPos.x, (int)gridPos.z)) return;
        Node n = Grid.GetNode((int)gridPos.x, (int)gridPos.z);
        if (n.nodeObject != null)
        {
            AddRoutineTaskTransform(n.nodeObject, n.nodeObject.position, automatic, clearRoutine);
            return;
        }
        if(clearRoutine) routine.Clear();
        Task walkTask = new Task(TaskType.Walk, newTarget);
        routine.Add(walkTask);
        if(routine.Count == 1) FindPath(newTarget, null);
    }

    public bool AddResourceTask(TaskType type, BuildingScript bs, GameResources res)
    {
        if(!bs) return false;

        Task walkTask = new Task(TaskType.Walk, bs.transform.position, bs.transform);
        Building b = bs.GetBuilding();
        int maxInvSize = res.GetResourceType() == ResourceType.Food ? thisPerson.GetFoodInventorySize() : thisPerson.GetMaterialInventorySize();

        GameResources inv = null;
        // if building is material storage, get material from inventory
        if(res.GetResourceType() == ResourceType.Food) inv = thisPerson.inventoryFood;
        else inv = thisPerson.inventoryMaterial;

        if(type == TaskType.TakeFromWarehouse)
        {
            if(res.amount > b.resourceCurrent[res.id]) return false;
            if(inv != null && res.amount > maxInvSize-inv.amount) return false;
        }
        if(type == TaskType.BringToWarehouse)
        {
            if(inv == null || res.amount > inv.amount) return false;
            if(res.amount > b.resourceStorage[res.id] - b.resourceCurrent[res.id]) return false;
        }

        List<GameResources> list = new List<GameResources>();
        list.Add(res);

        // if no inventory resource, take from warehouse
        if (res != null && res.GetAmount() > 0) 
        {
            routine.Add(walkTask);
            routine.Add(new Task(type, bs.transform.position, bs.transform, list));
            return true;
        }
        return false;
    }
    public Task TargetTaskFromTransform(Transform target, bool automatic)
    {
        Task targetTask = null;
        if (target != null)
        {
            switch (target.tag)
            {
                case "Building":
                    BuildingScript bs = target.GetComponent<BuildingScript>();
                    if (bs.blueprint)
                    {
                        /* TODO: building master needs to finish building */
                        targetTask = new Task(TaskType.Build, target);
                    }
                    else
                    {
                        Building b = bs.GetBuilding();
                        switch (b.GetBuildingType())
                        {
                            // Warehouse activity 
                            case BuildingType.Storage:
                                // If personscript decides automatically to walk there, just unload everything
                                if(automatic)
                                {
                                   /* GameResources inv = null;
                                    // if building is material storage, get material from inventory
                                    if(b.GetBuildingType() == BuildingType.StorageFood) inv = thisPerson.inventoryFood;
                                    else inv = thisPerson.inventoryMaterial;

                                    List<GameResources> list = new List<GameResources>();
                                    if(inv != null)
                                        list.Add(new GameResources(inv.id, inv.amount));
                                    targetTask = new Task(TaskType.BringToWarehouse, target.position, target, list);*/
                                }
                                else
                                {
                                    UIManager.Instance.OnShowObjectInfo(target);
                                    UIManager.Instance.TaskResRequest(this);
                                }
                                break;
                            case BuildingType.Food:
                                if (b.GetID() == Building.FISHERMANPLACE) // Fischerplatz
                                {
                                    targetTask = new Task(TaskType.Fisherplace, target);
                                }
                                break;
                            case BuildingType.Luxury:
                                if (b.GetID() == Building.CAMPFIRE) // Campfire
                                {
                                    Campfire cf = target.GetComponent<Campfire>();
                                    if (cf != null)
                                    {
                                        targetTask = new Task(TaskType.Campfire, target);
                                    }
                                }
                                break;
                            case BuildingType.Crafting:
                                if (b.GetID() == Building.BLACKSMITH)
                                {
                                    // check if person is a blacksmith, then he can craft
                                    if(thisPerson.job.id == Job.BLACKSMITH)
                                    {
                                        targetTask = new Task(TaskType.Craft, target);
                                    }
                                    // store bones ino that building
                                    else if(thisPerson.inventoryMaterial != null && thisPerson.inventoryMaterial.id == GameResources.BONES)
                                    {
                                        UIManager.Instance.OnShowObjectInfo(target);
                                        UIManager.Instance.TaskResRequest(this);
                                    }
                                    else GameManager.Msg("");
                                }
                                break;
                        }
                    }
                    break;
                case "Plant":
                    Plant plant = target.GetComponent<Plant>();
                    if (plant.type == PlantType.Tree)
                    {
                        // Every person can cut trees
                        if (thisPerson.job.id == Job.LUMBERJACK) //Holzfäller
                        {
                            targetTask = new Task(TaskType.CutTree, target);
                        }
                        else
                        {
                            GameManager.Msg(thisPerson.GetFirstName() + " kann keine Bäume fällen!");
                        }
                    }
                    else if (plant.type == PlantType.Mushroom)
                    {
                        targetTask = new Task(TaskType.CollectMushroom, target);
                    }
                    else if (plant.type == PlantType.MushroomStump)
                    {
                        targetTask = new Task(TaskType.CullectMushroomStump, target);
                    }
                    else if(plant.type == PlantType.Crop)
                    {
                        // can only harvest, if in build range or is a gatherer
                        if(GameManager.village.InBuildRange(target.position) || thisPerson.job.id == Job.GATHERER)
                            targetTask = new Task(TaskType.Harvest, target);
                        else GameManager.Msg("Nur Sammler können ausserhalb des Bau-Bereiches Korn ernten.");
                    }
                    else if (plant.type == PlantType.Reed)
                    {
                        if(thisPerson.job.id == Job.FISHER)
                            targetTask = new Task(TaskType.Fishing, target);
                        else
                            GameManager.Msg(thisPerson.firstName + " kann nicht fischen");
                    }
                    else if (plant.type == PlantType.Rock)
                    {
                        targetTask = new Task(TaskType.MineRock, target);
                    }
                    break;
                case "Item":
                    Item it = target.GetComponent<Item>();
                    if (it != null)
                    {
                        targetTask = new Task(TaskType.PickupItem, target);
                    }
                    break;
            }
        }
        if(targetTask != null) targetTask.automated = automatic;
        return targetTask;
    }
    public bool StoreMaterialInventory()
    {
        if(thisPerson.inventoryMaterial == null) return false;
        return StoreResources(thisPerson.inventoryMaterial);
    }
    public bool StoreFoodInventory()
    {
        if(thisPerson.inventoryFood == null) return false;
        return StoreResources(thisPerson.inventoryFood);
    }
    public bool StoreResources(GameResources res)
    {
        Transform nearestStorage = GameManager.village.GetNearestStorageBuilding(transform.position, res.id);
        if(nearestStorage == null) return false;
        routine.Add(new Task(TaskType.Walk, nearestStorage.position));
        routine.Add(new Task(TaskType.BringToWarehouse, nearestStorage.position, nearestStorage, res.Clone(), true));
        return true;
    }

    public void FindPath(Vector3 targetPosition, Transform targetTransform)
    {
        currentMoveSpeed = 0.05f;
        Vector3 start = Grid.ToGrid(transform.position);
        Vector3 end = Grid.ToGrid(targetPosition);
        int sx = (int)start.x; int sy = (int)start.z;
        int ex = (int)end.x; int ey = (int)end.z;
        if (!Grid.ValidNode(sx, sy) || !Grid.ValidNode(ex, ey)) return;
        //if(sx == ex && sy == ey) routine.RemoveAt(0);
        // if clicked on node with object, but not object
        if (Grid.GetNode(ex, ey).nodeObject != null)
        {
            targetTransform = Grid.GetNode(ex, ey).nodeObject;
        }
        if(!Grid.GetNode(sx, sy).objectWalkable)
        {
            sx = lastNode.gridX;
            sy = lastNode.gridY;
        }
        currentPath = AStar.FindPath(sx, sy, ex, ey);
        // If path is empty and start node is not equal to end node, don't do anything
        int dx = ex - sx; int dy = ey - sy;
        if (currentPath.Count == 0 && ((dx * dx + dy * dy) > 1 || (sx == ex && sy == ey)))
        {
            NextTask();
        }


        // Debug Path
        /*for(int i = 0; i < currentPath.Count; i++)
            Debug.Log("path"+i+"; "+currentPath[i].GetX() +";"+ currentPath[i].GetY());*/
    }

    void OnMouseOver()
    {
        if (CameraController.inputState != 2) return;
        highlighted = true;

        if (Input.GetMouseButtonDown(0)) {
            OnClick();
        }
    }
    void OnMouseExit()
    {
        highlighted = false;
    }

    private bool CheckIfInFoodRange()
    {
        foreach(BuildingScript bs in BuildingScript.allBuildings)
        {
            if(bs.GetBuilding().id == Building.WAREHOUSEFOOD)
            {
                return GameManager.InRange(bs.transform.position, transform.position, bs.GetBuilding().foodRange);
            }
        }
        return false;
    }

    public void OnClick()
    {
        if(!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))
            DeselectAll();
        OnSelect();
    }
    public void OnSelect()
    {
        selectedPeople.Add(this);
        selected = true;
    }

    public void OnDeselect()
    {
        selected = false;
    }

    public static void DeselectAll()
    {
        foreach(PersonScript ps in selectedPeople)
            ps.OnDeselect();
        
        selectedPeople.Clear();
    }

    public float GetFoodFactor()
    {
        return thisPerson.hunger / 100;
    }
    public float GetHealthFactor()
    {
        return thisPerson.health / 100;
    }

    // Get Condition of person (0=dead, 4=well)
    public int GetCondition()
    {
        float hf = GetHealthFactor();
        if(hf > 0.75f) return 4;
        if(hf > 0.5f) return 3;
        if(hf > 0.15f) return 2;
        if(hf > 0) return 1;

        return 0;
    }
    // Convert condition to string
    public string GetConditionStr()
    {
        int cond = GetCondition();
        switch(cond)
        {
            case 0: return "Tot";
            case 1: return "Hungersnot";
            case 2: return "Hungrig";
            case 3: return "Gesund";
            case 4: return "Gesund";
            default: return "unknown";
        }
    }
    // Convert condition to color
    public Color GetConditionCol()
    {
        int cond = GetCondition();
        switch(cond)
        {
            case 0: return new Color(0,0,0,0.6f);
            case 1: return new Color(1,0,0,0.6f);
            case 2: return new Color(1,0.6f,0.15f,0.6f);
            case 3: case 4:
             return new Color(1,0.6f,0.6f,0.6f);
            default: return new Color(0,0,0,0);
        }
    }
    // Convert hunger to color
    public Color GetFoodCol()
    {
        if(!inFoodRange) return new Color(1f,0.8f,0.15f,0.6f);
        return new Color(0,1,0.15f,0.6f);
        /*int cond = GetCondition();
        switch(cond)
        {
            case 0: return new Color(0,0,0,0.6f);
            case 1: return new Color(1,0,0,0.6f);
            case 2: return new Color(1,0.6f,0.15f,0.6f);
            case 3: case 4:
             return new Color(0,1,0.15f,0.6f);
            default: return new Color(0,0,0,0);
        }*/
    }

    public static PersonScript Identify(int id)
    {
        foreach (PersonScript ps in allPeople)
        {
            if(ps.ID == id) return ps;
        }
        return null;
    }
}
